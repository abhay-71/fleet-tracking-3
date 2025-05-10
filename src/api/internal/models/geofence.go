package models

import (
	"encoding/json"
	"time"
)

// GeofenceType represents the type of geofence
type GeofenceType string

const (
	// GeofenceTypeCircle is a circular geofence
	GeofenceTypeCircle GeofenceType = "circle"
	// GeofenceTypePolygon is a polygon geofence
	GeofenceTypePolygon GeofenceType = "polygon"
	// GeofenceTypeRectangle is a rectangular geofence
	GeofenceTypeRectangle GeofenceType = "rectangle"
)

// GeofenceCategory represents the category of geofence
type GeofenceCategory string

const (
	// GeofenceCategoryRestricted is a restricted area
	GeofenceCategoryRestricted GeofenceCategory = "restricted"
	// GeofenceCategoryWarehouse is a warehouse area
	GeofenceCategoryWarehouse GeofenceCategory = "warehouse"
	// GeofenceCategoryCustomer is a customer site
	GeofenceCategoryCustomer GeofenceCategory = "customer"
	// GeofenceCategoryPOI is a point of interest
	GeofenceCategoryPOI GeofenceCategory = "poi"
	// GeofenceCategoryCustom is a custom category
	GeofenceCategoryCustom GeofenceCategory = "custom"
)

// Geofence represents a geographic area for geofencing
type Geofence struct {
	ID           int              `json:"id" db:"id"`
	CompanyID    int              `json:"company_id" db:"company_id"`
	Name         string           `json:"name" db:"name"`
	Description  string           `json:"description" db:"description"`
	Type         GeofenceType     `json:"type" db:"type"`
	Category     GeofenceCategory `json:"category" db:"category"`
	Color        string           `json:"color" db:"color"`
	Coordinates  string           `json:"coordinates" db:"coordinates"` // JSON string of coordinates
	Radius       *float64         `json:"radius,omitempty" db:"radius"` // For circle type
	Active       bool             `json:"active" db:"active"`
	CreatedAt    time.Time        `json:"created_at" db:"created_at"`
	UpdatedAt    time.Time        `json:"updated_at" db:"updated_at"`
	CreatedBy    int              `json:"created_by" db:"created_by"`
	LastModified int              `json:"last_modified" db:"last_modified"`
	Metadata     json.RawMessage  `json:"metadata,omitempty" db:"metadata"` // Additional metadata in JSON format
}

// Circle represents a circular geofence definition
type Circle struct {
	CenterLat float64 `json:"center_lat"`
	CenterLng float64 `json:"center_lng"`
	Radius    float64 `json:"radius"` // In meters
}

// Polygon represents a polygon geofence definition
type Polygon struct {
	// Array of [lat, lng] pairs representing the vertices of the polygon
	Vertices [][]float64 `json:"vertices"`
}

// Rectangle represents a rectangular geofence definition
type Rectangle struct {
	NorthEastLat float64 `json:"northeast_lat"`
	NorthEastLng float64 `json:"northeast_lng"`
	SouthWestLat float64 `json:"southwest_lat"`
	SouthWestLng float64 `json:"southwest_lng"`
}

// GeofenceEvent represents an event when a vehicle enters or exits a geofence
type GeofenceEvent struct {
	ID         int       `json:"id" db:"id"`
	GeofenceID int       `json:"geofence_id" db:"geofence_id"`
	VehicleID  int       `json:"vehicle_id" db:"vehicle_id"`
	TripID     *int      `json:"trip_id,omitempty" db:"trip_id"`
	EventType  string    `json:"event_type" db:"event_type"` // "entry" or "exit"
	Latitude   float64   `json:"latitude" db:"latitude"`
	Longitude  float64   `json:"longitude" db:"longitude"`
	Timestamp  time.Time `json:"timestamp" db:"timestamp"`
	CreatedAt  time.Time `json:"created_at" db:"created_at"`
}

// GeofenceAssignment represents a geofence assignment to specific vehicles or drivers
type GeofenceAssignment struct {
	ID         int       `json:"id" db:"id"`
	GeofenceID int       `json:"geofence_id" db:"geofence_id"`
	EntityType string    `json:"entity_type" db:"entity_type"` // "vehicle", "driver", or "company"
	EntityID   int       `json:"entity_id" db:"entity_id"`
	CreatedAt  time.Time `json:"created_at" db:"created_at"`
	CreatedBy  int       `json:"created_by" db:"created_by"`
}

// GeofenceAlert represents an alert configuration for a geofence
type GeofenceAlert struct {
	ID          int       `json:"id" db:"id"`
	GeofenceID  int       `json:"geofence_id" db:"geofence_id"`
	Name        string    `json:"name" db:"name"`
	Description string    `json:"description" db:"description"`
	EventType   string    `json:"event_type" db:"event_type"` // "entry", "exit", or "both"
	AlertType   string    `json:"alert_type" db:"alert_type"` // "email", "sms", "in-app", etc.
	Recipients  string    `json:"recipients" db:"recipients"` // JSON array of recipient IDs or addresses
	Enabled     bool      `json:"enabled" db:"enabled"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
	UpdatedAt   time.Time `json:"updated_at" db:"updated_at"`
	CreatedBy   int       `json:"created_by" db:"created_by"`
}
