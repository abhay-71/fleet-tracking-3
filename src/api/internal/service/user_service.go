package service

import (
	"context"
	"errors"
	"fmt"
	"time"

	"fleetvigil.com/api/internal/models"
	"fleetvigil.com/api/internal/repository"
	"github.com/golang-jwt/jwt/v4"
	"golang.org/x/crypto/bcrypt"
)

// UserSvc implements the UserService interface
type UserSvc struct {
	userRepo      repository.UserRepository
	roleRepo      repository.RoleRepository
	jwtKey        []byte
	jwtIssuer     string
	jwtExpiry     time.Duration
	refreshExpiry time.Duration
}

// NewUserService creates a new UserSvc
func NewUserService(
	userRepo repository.UserRepository,
	roleRepo repository.RoleRepository,
	jwtKey []byte,
	jwtIssuer string,
	jwtExpiry, refreshExpiry time.Duration,
) *UserSvc {
	return &UserSvc{
		userRepo:      userRepo,
		roleRepo:      roleRepo,
		jwtKey:        jwtKey,
		jwtIssuer:     jwtIssuer,
		jwtExpiry:     jwtExpiry,
		refreshExpiry: refreshExpiry,
	}
}

// Create creates a new user
func (s *UserSvc) Create(ctx context.Context, user *models.User) error {
	if user.PasswordHash == "" {
		return errors.New("password hash cannot be empty")
	}

	return s.userRepo.Create(ctx, user)
}

// Get gets a user by ID
func (s *UserSvc) Get(ctx context.Context, id int) (*models.User, error) {
	return s.userRepo.GetByID(ctx, id)
}

// GetByUsername gets a user by username
func (s *UserSvc) GetByUsername(ctx context.Context, username string) (*models.User, error) {
	return s.userRepo.GetByUsername(ctx, username)
}

// GetByEmail gets a user by email
func (s *UserSvc) GetByEmail(ctx context.Context, email string) (*models.User, error) {
	return s.userRepo.GetByEmail(ctx, email)
}

// Update updates a user
func (s *UserSvc) Update(ctx context.Context, user *models.User) error {
	return s.userRepo.Update(ctx, user)
}

// Delete deletes a user
func (s *UserSvc) Delete(ctx context.Context, id int) error {
	return s.userRepo.Delete(ctx, id)
}

// List lists users with pagination
func (s *UserSvc) List(ctx context.Context, page, limit int) ([]*models.User, int, error) {
	return s.userRepo.List(ctx, page, limit)
}

// GetRoles gets the roles for a user
func (s *UserSvc) GetRoles(ctx context.Context, userID int) ([]string, error) {
	return s.userRepo.GetUserRoles(ctx, userID)
}

// AssignRole assigns a role to a user
func (s *UserSvc) AssignRole(ctx context.Context, userID, roleID int) error {
	// Check if user exists
	_, err := s.userRepo.GetByID(ctx, userID)
	if err != nil {
		return fmt.Errorf("user not found: %v", err)
	}

	// Check if role exists
	role, err := s.roleRepo.GetByID(ctx, roleID)
	if err != nil {
		return fmt.Errorf("role not found: %v", err)
	}

	// Assign role
	return s.userRepo.AssignRole(ctx, userID, role.ID)
}

// RemoveRole removes a role from a user
func (s *UserSvc) RemoveRole(ctx context.Context, userID, roleID int) error {
	return s.userRepo.RemoveRole(ctx, userID, roleID)
}

// Register registers a new user
func (s *UserSvc) Register(ctx context.Context, registration *models.UserRegistration) (*models.User, error) {
	// Check if username exists
	_, err := s.userRepo.GetByUsername(ctx, registration.Username)
	if err == nil {
		return nil, errors.New("username already exists")
	}

	// Check if email exists
	_, err = s.userRepo.GetByEmail(ctx, registration.Email)
	if err == nil {
		return nil, errors.New("email already exists")
	}

	// Hash password
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(registration.Password), bcrypt.DefaultCost)
	if err != nil {
		return nil, fmt.Errorf("failed to hash password: %v", err)
	}

	// Create user
	user := &models.User{
		Username:     registration.Username,
		Email:        registration.Email,
		PasswordHash: string(hashedPassword),
		FirstName:    registration.FirstName,
		LastName:     registration.LastName,
		Phone:        registration.Phone,
		Status:       "active",
	}

	if err := s.userRepo.Create(ctx, user); err != nil {
		return nil, fmt.Errorf("failed to create user: %v", err)
	}

	// Assign default user role
	defaultRole, err := s.roleRepo.GetByName(ctx, "user")
	if err == nil {
		_ = s.userRepo.AssignRole(ctx, user.ID, defaultRole.ID)
	}

	// Clear password hash for response
	user.PasswordHash = ""

	return user, nil
}

// Login logs in a user
func (s *UserSvc) Login(ctx context.Context, credentials *models.LoginCredentials) (string, string, error) {
	// Get user by username
	user, err := s.userRepo.GetByUsername(ctx, credentials.Username)
	if err != nil {
		return "", "", errors.New("invalid username or password")
	}

	// Check password
	err = bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(credentials.Password))
	if err != nil {
		return "", "", errors.New("invalid username or password")
	}

	// Get user roles
	roles, err := s.userRepo.GetUserRoles(ctx, user.ID)
	if err != nil {
		return "", "", fmt.Errorf("failed to get user roles: %v", err)
	}

	// Generate tokens
	accessToken, err := s.generateAccessToken(user.ID, user.Username, roles)
	if err != nil {
		return "", "", fmt.Errorf("failed to generate access token: %v", err)
	}

	refreshToken, err := s.generateRefreshToken(user.ID)
	if err != nil {
		return "", "", fmt.Errorf("failed to generate refresh token: %v", err)
	}

	return accessToken, refreshToken, nil
}

// RefreshToken refreshes an authentication token
func (s *UserSvc) RefreshToken(ctx context.Context, refreshToken string) (string, string, error) {
	// Validate refresh token
	claims, err := validateRefreshToken(refreshToken, s.jwtKey)
	if err != nil {
		return "", "", fmt.Errorf("invalid refresh token: %v", err)
	}

	// Get user
	user, err := s.userRepo.GetByID(ctx, claims.UserID)
	if err != nil {
		return "", "", fmt.Errorf("user not found: %v", err)
	}

	// Get user roles
	roles, err := s.userRepo.GetUserRoles(ctx, user.ID)
	if err != nil {
		return "", "", fmt.Errorf("failed to get user roles: %v", err)
	}

	// Generate new tokens
	accessToken, err := s.generateAccessToken(user.ID, user.Username, roles)
	if err != nil {
		return "", "", fmt.Errorf("failed to generate access token: %v", err)
	}

	newRefreshToken, err := s.generateRefreshToken(user.ID)
	if err != nil {
		return "", "", fmt.Errorf("failed to generate refresh token: %v", err)
	}

	return accessToken, newRefreshToken, nil
}

// generateAccessToken generates a JWT access token
func (s *UserSvc) generateAccessToken(userID int, username string, roles []string) (string, error) {
	now := time.Now()
	claims := JWTClaims{
		UserID:   userID,
		Username: username,
		Roles:    roles,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(now.Add(s.jwtExpiry)),
			IssuedAt:  jwt.NewNumericDate(now),
			Issuer:    s.jwtIssuer,
			Subject:   fmt.Sprintf("%d", userID),
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(s.jwtKey)
}

// generateRefreshToken generates a JWT refresh token
func (s *UserSvc) generateRefreshToken(userID int) (string, error) {
	now := time.Now()
	claims := RefreshTokenClaims{
		UserID: userID,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(now.Add(s.refreshExpiry)),
			IssuedAt:  jwt.NewNumericDate(now),
			Issuer:    s.jwtIssuer,
			Subject:   fmt.Sprintf("%d", userID),
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(s.jwtKey)
}

// JWTClaims represents JWT claims for access tokens
type JWTClaims struct {
	UserID   int      `json:"user_id"`
	Username string   `json:"username"`
	Roles    []string `json:"roles"`
	jwt.RegisteredClaims
}

// RefreshTokenClaims represents JWT claims for refresh tokens
type RefreshTokenClaims struct {
	UserID int `json:"user_id"`
	jwt.RegisteredClaims
}

// validateRefreshToken validates a refresh token and returns its claims
func validateRefreshToken(tokenString string, key []byte) (*RefreshTokenClaims, error) {
	token, err := jwt.ParseWithClaims(tokenString, &RefreshTokenClaims{}, func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}
		return key, nil
	})

	if err != nil {
		return nil, err
	}

	claims, ok := token.Claims.(*RefreshTokenClaims)
	if !ok || !token.Valid {
		return nil, errors.New("invalid token")
	}

	return claims, nil
}
