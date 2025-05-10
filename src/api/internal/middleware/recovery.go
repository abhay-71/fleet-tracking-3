package middleware

import (
	"log"
	"net/http"
	"runtime/debug"
)

// Recovery middleware recovers from panics and returns a 500 response
func Recovery(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if err := recover(); err != nil {
				// Log the error and stack trace
				log.Printf("PANIC: %v\n%s", err, debug.Stack())
				
				// Return a 500 Internal Server Error response
				http.Error(w, http.StatusText(http.StatusInternalServerError), http.StatusInternalServerError)
			}
		}()
		
		next.ServeHTTP(w, r)
	})
} 