package database

import (
	"database/sql"
	"fmt"
	"time"

	"fleetvigil.com/api/internal/config"
	_ "github.com/lib/pq" // Import PostgreSQL driver
)

// DB is a wrapper around sql.DB with additional functionality
type DB struct {
	*sql.DB
}

// Connect establishes a connection to the PostgreSQL database
func Connect(cfg config.DatabaseConfig) (*DB, error) {
	connStr := fmt.Sprintf(
		"host=%s port=%d user=%s password=%s dbname=%s sslmode=%s",
		cfg.Host, cfg.Port, cfg.User, cfg.Password, cfg.DBName, cfg.SSLMode,
	)

	db, err := sql.Open("postgres", connStr)
	if err != nil {
		return nil, fmt.Errorf("failed to open database connection: %w", err)
	}

	// Set connection pool parameters
	db.SetMaxOpenConns(25)
	db.SetMaxIdleConns(5)
	db.SetConnMaxLifetime(5 * time.Minute)

	// Test the connection
	if err := db.Ping(); err != nil {
		db.Close()
		return nil, fmt.Errorf("failed to ping database: %w", err)
	}

	return &DB{db}, nil
}

// Close closes the database connection
func (db *DB) Close() error {
	return db.DB.Close()
}

// CheckPostGIS checks if PostGIS extension is installed
func (db *DB) CheckPostGIS() (bool, error) {
	var exists bool
	query := `
		SELECT EXISTS (
			SELECT 1 FROM pg_extension WHERE extname = 'postgis'
		)
	`
	err := db.QueryRow(query).Scan(&exists)
	if err != nil {
		return false, fmt.Errorf("failed to check PostGIS extension: %w", err)
	}
	return exists, nil
}

// InstallPostGIS attempts to install the PostGIS extension
func (db *DB) InstallPostGIS() error {
	_, err := db.Exec("CREATE EXTENSION IF NOT EXISTS postgis")
	if err != nil {
		return fmt.Errorf("failed to install PostGIS extension: %w", err)
	}
	return nil
}

// Transaction begins a database transaction
func (db *DB) Transaction(txFunc func(*sql.Tx) error) error {
	tx, err := db.Begin()
	if err != nil {
		return err
	}

	defer func() {
		if p := recover(); p != nil {
			tx.Rollback()
			panic(p) // re-throw panic after Rollback
		}
	}()

	if err := txFunc(tx); err != nil {
		if rbErr := tx.Rollback(); rbErr != nil {
			return fmt.Errorf("error rolling back transaction: %v, original error: %w", rbErr, err)
		}
		return err
	}

	if err := tx.Commit(); err != nil {
		return fmt.Errorf("error committing transaction: %w", err)
	}

	return nil
}
