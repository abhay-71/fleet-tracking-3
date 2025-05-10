package middleware

import (
	"context"
	"net/http"
	"strings"

	"fleetvigil.com/api/internal/config"
	"fleetvigil.com/api/pkg/auth"
)

// Auth middleware handles JWT authentication
func Auth(cfg config.AuthConfig) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			// Skip authentication for specific endpoints
			if isExemptPath(r.URL.Path) {
				next.ServeHTTP(w, r)
				return
			}

			// Get the Authorization header
			authHeader := r.Header.Get("Authorization")
			if authHeader == "" {
				http.Error(w, "Authorization header required", http.StatusUnauthorized)
				return
			}

			// Extract the token from the Authorization header
			tokenParts := strings.Split(authHeader, " ")
			if len(tokenParts) != 2 || strings.ToLower(tokenParts[0]) != "bearer" {
				http.Error(w, "Invalid authorization format. Expected: Bearer <token>", http.StatusUnauthorized)
				return
			}
			tokenString := tokenParts[1]

			// Validate the token
			claims, err := auth.ValidateToken(tokenString, cfg.JWTSecret)
			if err != nil {
				if err == auth.ErrExpiredToken {
					http.Error(w, "Token expired", http.StatusUnauthorized)
				} else {
					http.Error(w, "Invalid token", http.StatusUnauthorized)
				}
				return
			}

			// Add claims to the request context
			ctx := context.WithValue(r.Context(), "user", claims)
			r = r.WithContext(ctx)

			// Continue to the next handler
			next.ServeHTTP(w, r)
		})
	}
}

// isExemptPath returns true if the path is exempt from authentication
func isExemptPath(path string) bool {
	exemptPaths := []string{
		"/health",
		"/api/v1/auth/login",
		"/api/v1/auth/register",
		"/api/v1/auth/refresh",
	}

	for _, exemptPath := range exemptPaths {
		if strings.HasPrefix(path, exemptPath) {
			return true
		}
	}

	return false
}
