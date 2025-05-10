package config

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
)

// Config represents the application configuration
type Config struct {
	Server   ServerConfig   `json:"server"`
	Database DatabaseConfig `json:"database"`
	Auth     AuthConfig     `json:"auth"`
	Logging  LoggingConfig  `json:"logging"`
}

// ServerConfig contains server-related configuration
type ServerConfig struct {
	Port int    `json:"port"`
	Host string `json:"host"`
}

// DatabaseConfig contains database connection configuration
type DatabaseConfig struct {
	Host     string `json:"host"`
	Port     int    `json:"port"`
	User     string `json:"user"`
	Password string `json:"password"`
	DBName   string `json:"dbname"`
	SSLMode  string `json:"sslmode"`
}

// AuthConfig contains authentication configuration
type AuthConfig struct {
	JWTSecret           string `json:"jwt_secret"`
	TokenExpirationHour int    `json:"token_expiration_hour"`
}

// LoggingConfig contains logging configuration
type LoggingConfig struct {
	Level string `json:"level"`
	File  string `json:"file"`
}

// Load loads configuration from file and environment variables
func Load() (*Config, error) {
	// Default configuration
	config := &Config{
		Server: ServerConfig{
			Port: 8080,
			Host: "0.0.0.0",
		},
		Database: DatabaseConfig{
			Host:     "localhost",
			Port:     5432,
			User:     "postgres",
			Password: "postgres",
			DBName:   "fleetvigil",
			SSLMode:  "disable",
		},
		Auth: AuthConfig{
			JWTSecret:           "fleetvigil-secret-key",
			TokenExpirationHour: 24,
		},
		Logging: LoggingConfig{
			Level: "info",
			File:  "logs/api.log",
		},
	}

	// Try to load configuration from config file
	configPath := getConfigPath()
	if configPath != "" {
		if err := loadFromFile(configPath, config); err != nil {
			return nil, err
		}
	}

	// Override with environment variables if present
	applyEnvironment(config)

	return config, nil
}

// getConfigPath determines the config file path
func getConfigPath() string {
	// Check if config file path is specified in an environment variable
	if path := os.Getenv("FLEETVIGIL_CONFIG"); path != "" {
		return path
	}

	// Default config paths to check
	paths := []string{
		"config.json",
		"configs/config.json",
		filepath.Join("..", "configs", "config.json"),
	}

	for _, path := range paths {
		if _, err := os.Stat(path); err == nil {
			return path
		}
	}

	return ""
}

// loadFromFile loads configuration from a JSON file
func loadFromFile(path string, config *Config) error {
	file, err := os.Open(path)
	if err != nil {
		return fmt.Errorf("failed to open config file: %w", err)
	}
	defer file.Close()

	decoder := json.NewDecoder(file)
	if err := decoder.Decode(config); err != nil {
		return fmt.Errorf("failed to decode config file: %w", err)
	}

	return nil
}

// applyEnvironment applies environment variables to override config
func applyEnvironment(config *Config) {
	// Server configuration
	if port := os.Getenv("FLEETVIGIL_PORT"); port != "" {
		fmt.Sscanf(port, "%d", &config.Server.Port)
	}
	if host := os.Getenv("FLEETVIGIL_HOST"); host != "" {
		config.Server.Host = host
	}

	// Database configuration
	if host := os.Getenv("FLEETVIGIL_DB_HOST"); host != "" {
		config.Database.Host = host
	}
	if port := os.Getenv("FLEETVIGIL_DB_PORT"); port != "" {
		fmt.Sscanf(port, "%d", &config.Database.Port)
	}
	if user := os.Getenv("FLEETVIGIL_DB_USER"); user != "" {
		config.Database.User = user
	}
	if password := os.Getenv("FLEETVIGIL_DB_PASSWORD"); password != "" {
		config.Database.Password = password
	}
	if dbName := os.Getenv("FLEETVIGIL_DB_NAME"); dbName != "" {
		config.Database.DBName = dbName
	}
	if sslMode := os.Getenv("FLEETVIGIL_DB_SSLMODE"); sslMode != "" {
		config.Database.SSLMode = sslMode
	}

	// Auth configuration
	if jwtSecret := os.Getenv("FLEETVIGIL_JWT_SECRET"); jwtSecret != "" {
		config.Auth.JWTSecret = jwtSecret
	}
	if tokenExpiration := os.Getenv("FLEETVIGIL_TOKEN_EXPIRATION"); tokenExpiration != "" {
		fmt.Sscanf(tokenExpiration, "%d", &config.Auth.TokenExpirationHour)
	}

	// Logging configuration
	if logLevel := os.Getenv("FLEETVIGIL_LOG_LEVEL"); logLevel != "" {
		config.Logging.Level = logLevel
	}
	if logFile := os.Getenv("FLEETVIGIL_LOG_FILE"); logFile != "" {
		config.Logging.File = logFile
	}
}
