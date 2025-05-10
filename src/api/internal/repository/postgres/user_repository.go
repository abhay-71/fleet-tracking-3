package postgres

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"strings"

	"fleetvigil.com/api/internal/models"
	"github.com/jmoiron/sqlx"
)

// UserRepo implements the UserRepository interface for PostgreSQL
type UserRepo struct {
	db *sqlx.DB
}

// NewUserRepository creates a new UserRepo
func NewUserRepository(db *sqlx.DB) *UserRepo {
	return &UserRepo{
		db: db,
	}
}

// Create creates a new user
func (r *UserRepo) Create(ctx context.Context, user *models.User) error {
	query := `
		INSERT INTO users (username, email, password_hash, first_name, last_name, phone, status, created_at, updated_at)
		VALUES ($1, $2, $3, $4, $5, $6, $7, NOW(), NOW())
		RETURNING id, created_at, updated_at`

	err := r.db.QueryRowxContext(
		ctx,
		query,
		user.Username,
		user.Email,
		user.PasswordHash,
		user.FirstName,
		user.LastName,
		user.Phone,
		user.Status,
	).StructScan(user)

	if err != nil {
		if strings.Contains(err.Error(), "duplicate key") {
			if strings.Contains(err.Error(), "users_email_key") {
				return fmt.Errorf("email already exists: %v", err)
			}
			if strings.Contains(err.Error(), "users_username_key") {
				return fmt.Errorf("username already exists: %v", err)
			}
		}
		return fmt.Errorf("failed to create user: %v", err)
	}

	return nil
}

// GetByID gets a user by ID
func (r *UserRepo) GetByID(ctx context.Context, id int) (*models.User, error) {
	user := &models.User{}
	query := `
		SELECT id, username, email, password_hash, first_name, last_name, 
		       phone, status, created_at, updated_at
		FROM users
		WHERE id = $1 AND status != 'deleted'`

	err := r.db.QueryRowxContext(ctx, query, id).StructScan(user)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, fmt.Errorf("user not found: %v", err)
		}
		return nil, fmt.Errorf("failed to get user: %v", err)
	}

	return user, nil
}

// GetByUsername gets a user by username
func (r *UserRepo) GetByUsername(ctx context.Context, username string) (*models.User, error) {
	user := &models.User{}
	query := `
		SELECT id, username, email, password_hash, first_name, last_name, 
		       phone, status, created_at, updated_at
		FROM users
		WHERE username = $1 AND status != 'deleted'`

	err := r.db.QueryRowxContext(ctx, query, username).StructScan(user)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, fmt.Errorf("user not found: %v", err)
		}
		return nil, fmt.Errorf("failed to get user by username: %v", err)
	}

	return user, nil
}

// GetByEmail gets a user by email
func (r *UserRepo) GetByEmail(ctx context.Context, email string) (*models.User, error) {
	user := &models.User{}
	query := `
		SELECT id, username, email, password_hash, first_name, last_name, 
		       phone, status, created_at, updated_at
		FROM users
		WHERE email = $1 AND status != 'deleted'`

	err := r.db.QueryRowxContext(ctx, query, email).StructScan(user)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, fmt.Errorf("user not found: %v", err)
		}
		return nil, fmt.Errorf("failed to get user by email: %v", err)
	}

	return user, nil
}

// Update updates a user
func (r *UserRepo) Update(ctx context.Context, user *models.User) error {
	query := `
		UPDATE users
		SET username = $1, email = $2, first_name = $3, last_name = $4,
			phone = $5, status = $6, updated_at = NOW()
		WHERE id = $7
		RETURNING updated_at`

	err := r.db.QueryRowxContext(
		ctx,
		query,
		user.Username,
		user.Email,
		user.FirstName,
		user.LastName,
		user.Phone,
		user.Status,
		user.ID,
	).Scan(&user.UpdatedAt)

	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return fmt.Errorf("user not found: %v", err)
		}
		if strings.Contains(err.Error(), "duplicate key") {
			if strings.Contains(err.Error(), "users_email_key") {
				return fmt.Errorf("email already exists: %v", err)
			}
			if strings.Contains(err.Error(), "users_username_key") {
				return fmt.Errorf("username already exists: %v", err)
			}
		}
		return fmt.Errorf("failed to update user: %v", err)
	}

	return nil
}

// Delete soft deletes a user
func (r *UserRepo) Delete(ctx context.Context, id int) error {
	query := `
		UPDATE users
		SET status = 'deleted', updated_at = NOW()
		WHERE id = $1`

	result, err := r.db.ExecContext(ctx, query, id)
	if err != nil {
		return fmt.Errorf("failed to delete user: %v", err)
	}

	rowsAffected, err := result.RowsAffected()
	if err != nil {
		return fmt.Errorf("failed to get rows affected: %v", err)
	}

	if rowsAffected == 0 {
		return fmt.Errorf("user not found")
	}

	return nil
}

// List lists users with pagination
func (r *UserRepo) List(ctx context.Context, page, limit int) ([]*models.User, int, error) {
	offset := (page - 1) * limit

	// Get total count
	var total int
	countQuery := `
		SELECT COUNT(*)
		FROM users
		WHERE status != 'deleted'`

	err := r.db.QueryRowContext(ctx, countQuery).Scan(&total)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to count users: %v", err)
	}

	// Get users
	query := `
		SELECT id, username, email, password_hash, first_name, last_name, 
		       phone, status, created_at, updated_at
		FROM users
		WHERE status != 'deleted'
		ORDER BY id
		LIMIT $1 OFFSET $2`

	rows, err := r.db.QueryxContext(ctx, query, limit, offset)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to list users: %v", err)
	}
	defer rows.Close()

	users := make([]*models.User, 0)
	for rows.Next() {
		var user models.User
		if err := rows.StructScan(&user); err != nil {
			return nil, 0, fmt.Errorf("failed to scan user: %v", err)
		}
		users = append(users, &user)
	}

	if err := rows.Err(); err != nil {
		return nil, 0, fmt.Errorf("error iterating user rows: %v", err)
	}

	return users, total, nil
}

// GetUserRoles gets the roles for a user
func (r *UserRepo) GetUserRoles(ctx context.Context, userID int) ([]string, error) {
	query := `
		SELECT r.name
		FROM roles r
		JOIN user_roles ur ON r.id = ur.role_id
		WHERE ur.user_id = $1`

	rows, err := r.db.QueryxContext(ctx, query, userID)
	if err != nil {
		return nil, fmt.Errorf("failed to get user roles: %v", err)
	}
	defer rows.Close()

	roles := make([]string, 0)
	for rows.Next() {
		var role string
		if err := rows.Scan(&role); err != nil {
			return nil, fmt.Errorf("failed to scan role: %v", err)
		}
		roles = append(roles, role)
	}

	if err := rows.Err(); err != nil {
		return nil, fmt.Errorf("error iterating role rows: %v", err)
	}

	return roles, nil
}

// AssignRole assigns a role to a user
func (r *UserRepo) AssignRole(ctx context.Context, userID, roleID int) error {
	query := `
		INSERT INTO user_roles (user_id, role_id)
		VALUES ($1, $2)
		ON CONFLICT (user_id, role_id) DO NOTHING`

	_, err := r.db.ExecContext(ctx, query, userID, roleID)
	if err != nil {
		return fmt.Errorf("failed to assign role: %v", err)
	}

	return nil
}

// RemoveRole removes a role from a user
func (r *UserRepo) RemoveRole(ctx context.Context, userID, roleID int) error {
	query := `
		DELETE FROM user_roles
		WHERE user_id = $1 AND role_id = $2`

	result, err := r.db.ExecContext(ctx, query, userID, roleID)
	if err != nil {
		return fmt.Errorf("failed to remove role: %v", err)
	}

	rowsAffected, err := result.RowsAffected()
	if err != nil {
		return fmt.Errorf("failed to get rows affected: %v", err)
	}

	if rowsAffected == 0 {
		return fmt.Errorf("user-role association not found")
	}

	return nil
}
