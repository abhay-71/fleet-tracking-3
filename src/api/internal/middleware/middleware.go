package middleware

import (
	"context"
	"errors"
	"fmt"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v4"
)

// AuthConfig contains configuration for the Auth middleware
type AuthConfig struct {
	JWTKey    []byte
	JWTIssuer string
}

// UserClaims represents the structure of our JWT claims
type UserClaims struct {
	UserID   int      `json:"user_id"`
	Username string   `json:"username"`
	Roles    []string `json:"roles"`
	jwt.RegisteredClaims
}

// contextKey is a private type for context keys
type contextKey string

// Constants for user context keys
const (
	UserIDKey   contextKey = "user_id"
	UsernameKey contextKey = "username"
	RolesKey    contextKey = "roles"
)

// JWTAuth is a middleware that handles JWT authentication
func JWTAuth(config AuthConfig) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			// Get token from Authorization header
			authHeader := r.Header.Get("Authorization")
			if authHeader == "" {
				http.Error(w, "Authorization header required", http.StatusUnauthorized)
				return
			}

			// Check if the header format is correct (Bearer token)
			parts := strings.Split(authHeader, " ")
			if len(parts) != 2 || parts[0] != "Bearer" {
				http.Error(w, "Authorization header format must be Bearer {token}", http.StatusUnauthorized)
				return
			}

			// Parse and validate token
			tokenString := parts[1]
			claims, err := validateToken(tokenString, config.JWTKey)
			if err != nil {
				http.Error(w, fmt.Sprintf("Invalid token: %v", err), http.StatusUnauthorized)
				return
			}

			// Add user info to request context
			ctx := r.Context()
			ctx = context.WithValue(ctx, UserIDKey, claims.UserID)
			ctx = context.WithValue(ctx, UsernameKey, claims.Username)
			ctx = context.WithValue(ctx, RolesKey, claims.Roles)

			// Call the next handler with the updated context
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

// validateToken validates a JWT token and returns its claims
func validateToken(tokenString string, key []byte) (*UserClaims, error) {
	// Parse token with claims
	token, err := jwt.ParseWithClaims(tokenString, &UserClaims{}, func(token *jwt.Token) (interface{}, error) {
		// Validate signing method
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}
		return key, nil
	})

	if err != nil {
		return nil, err
	}

	// Check token validity and extract claims
	if claims, ok := token.Claims.(*UserClaims); ok && token.Valid {
		return claims, nil
	}

	return nil, errors.New("invalid token")
}

// GetUserID gets the user ID from the request context
func GetUserID(ctx context.Context) (int, error) {
	value := ctx.Value(UserIDKey)
	if value == nil {
		return 0, errors.New("user ID not found in context")
	}

	userID, ok := value.(int)
	if !ok {
		return 0, errors.New("user ID has invalid type")
	}

	return userID, nil
}

// GetUsername gets the username from the request context
func GetUsername(ctx context.Context) (string, error) {
	value := ctx.Value(UsernameKey)
	if value == nil {
		return "", errors.New("username not found in context")
	}

	username, ok := value.(string)
	if !ok {
		return "", errors.New("username has invalid type")
	}

	return username, nil
}

// GetUserRoles gets the user roles from the request context
func GetUserRoles(ctx context.Context) ([]string, error) {
	value := ctx.Value(RolesKey)
	if value == nil {
		return nil, errors.New("user roles not found in context")
	}

	roles, ok := value.([]string)
	if !ok {
		return nil, errors.New("user roles have invalid type")
	}

	return roles, nil
}

// Cors is a middleware that handles CORS
func Cors(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// Set CORS headers
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")

		// Handle preflight requests
		if r.Method == "OPTIONS" {
			w.WriteHeader(http.StatusOK)
			return
		}

		// Call the next handler
		next.ServeHTTP(w, r)
	})
}
