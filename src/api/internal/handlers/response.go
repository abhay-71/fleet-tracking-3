package handlers

import (
	"encoding/json"
	"net/http"
	"strings"

	"github.com/go-playground/validator/v10"
)

// ErrorResponse represents an error response
type ErrorResponse struct {
	StatusCode int    `json:"-"`
	Message    string `json:"message"`
}

// respondWithError sends an error response
func respondWithError(w http.ResponseWriter, statusCode int, message string) {
	response := ErrorResponse{
		StatusCode: statusCode,
		Message:    message,
	}
	respondWithJSON(w, statusCode, response)
}

// respondWithJSON sends a JSON response
func respondWithJSON(w http.ResponseWriter, statusCode int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	if err := json.NewEncoder(w).Encode(data); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}

// validationErrorMessage formats validation errors into a user-friendly message
func validationErrorMessage(err error) string {
	if _, ok := err.(*validator.InvalidValidationError); ok {
		return "Invalid validation request"
	}

	var messages []string
	for _, err := range err.(validator.ValidationErrors) {
		field := err.Field()
		switch err.Tag() {
		case "required":
			messages = append(messages, field+" is required")
		case "email":
			messages = append(messages, field+" must be a valid email address")
		case "min":
			messages = append(messages, field+" must be at least "+err.Param()+" characters long")
		case "max":
			messages = append(messages, field+" must be at most "+err.Param()+" characters long")
		default:
			messages = append(messages, field+" failed validation: "+err.Tag())
		}
	}

	return strings.Join(messages, ". ")
}
