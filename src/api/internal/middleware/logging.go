package middleware

import (
	"log"
	"net/http"
	"time"
)

// Logging middleware logs HTTP requests
func Logging(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		// Create a custom response writer to capture the status code
		wrw := &responseWriter{
			ResponseWriter: w,
			statusCode:     http.StatusOK,
		}

		// Call the next handler in the chain
		next.ServeHTTP(wrw, r)

		// Log the request details
		log.Printf(
			"[%s] %s %s %d %s",
			r.Method,
			r.URL.Path,
			r.RemoteAddr,
			wrw.statusCode,
			time.Since(start),
		)
	})
}

// responseWriter is a custom response writer that captures the status code
type responseWriter struct {
	http.ResponseWriter
	statusCode int
}

// WriteHeader captures the status code
func (rw *responseWriter) WriteHeader(code int) {
	rw.statusCode = code
	rw.ResponseWriter.WriteHeader(code)
} 