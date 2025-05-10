# Fleet Tracking Application - API Design

This document outlines the design of the GoLang API for the Fleet Tracking Application, including endpoints, authentication, data structures, and communication patterns.

## API Overview

The API follows RESTful design principles and serves as the communication layer between the ASP.NET MVC frontend and the PostgreSQL database. It is designed to be stateless, secure, and scalable to handle the demands of real-time fleet tracking.

## Base URL Structure

The API will be accessible at:

```
https://api.fleettracking.example.com/v1/
```

Where:
- `v1` indicates the API version

## Authentication and Authorization

### Authentication Methods

The API uses JWT (JSON Web Tokens) for authentication:

1. **Client obtains token:**
   ```
   POST /auth/login
   ```

2. **Token refresh:**
   ```
   POST /auth/refresh
   ```

3. **Token usage:**
   All API requests should include the token in the Authorization header:
   ```
   Authorization: Bearer {token}
   ```

### Authorization Levels

The API supports role-based access control with the following roles:
- Administrator
- Manager
- Dispatcher
- Driver
- Viewer

## API Endpoints

### Authentication

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/auth/login` | POST | Authenticate user and receive JWT token |
| `/auth/refresh` | POST | Refresh an existing JWT token |
| `/auth/logout` | POST | Invalidate the current JWT token |
| `/auth/password/reset` | POST | Request password reset |
| `/auth/password/change` | POST | Change password with token |

### User Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/users` | GET | List users (with pagination) |
| `/users` | POST | Create a new user |
| `/users/{id}` | GET | Get user details |
| `/users/{id}` | PUT | Update user details |
| `/users/{id}` | DELETE | Delete a user |
| `/users/{id}/roles` | GET | Get user roles |
| `/users/{id}/roles` | PUT | Update user roles |
| `/roles` | GET | List all roles |
| `/roles/{id}/permissions` | GET | Get role permissions |
| `/roles/{id}/permissions` | PUT | Update role permissions |

### Company Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/companies` | GET | List companies |
| `/companies` | POST | Create a new company |
| `/companies/{id}` | GET | Get company details |
| `/companies/{id}` | PUT | Update company details |
| `/companies/{id}` | DELETE | Delete a company |
| `/companies/{id}/departments` | GET | List departments in company |
| `/departments` | POST | Create a new department |
| `/departments/{id}` | GET | Get department details |
| `/departments/{id}` | PUT | Update department details |
| `/departments/{id}` | DELETE | Delete a department |

### Vehicle Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/vehicles` | GET | List vehicles (with filters and pagination) |
| `/vehicles` | POST | Register a new vehicle |
| `/vehicles/{id}` | GET | Get vehicle details |
| `/vehicles/{id}` | PUT | Update vehicle details |
| `/vehicles/{id}` | DELETE | Delete a vehicle |
| `/vehicles/{id}/status` | GET | Get current vehicle status |
| `/vehicles/{id}/location` | GET | Get current vehicle location |
| `/vehicles/{id}/assignments` | GET | List vehicle assignments |
| `/vehicles/{id}/maintenance` | GET | List maintenance records |
| `/vehicles/{id}/trips` | GET | List vehicle trips |
| `/vehicles/{id}/telemetry` | GET | Get recent telemetry data |
| `/vehicles/{id}/analytics` | GET | Get vehicle performance analytics |
| `/vehicle-types` | GET | List vehicle types |
| `/vehicle-types` | POST | Create a vehicle type |
| `/vehicle-types/{id}` | GET | Get vehicle type details |
| `/vehicle-types/{id}` | PUT | Update vehicle type |
| `/vehicle-types/{id}` | DELETE | Delete vehicle type |

### Driver Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/drivers` | GET | List drivers (with filters and pagination) |
| `/drivers` | POST | Register a new driver |
| `/drivers/{id}` | GET | Get driver details |
| `/drivers/{id}` | PUT | Update driver details |
| `/drivers/{id}` | DELETE | Delete a driver |
| `/drivers/{id}/assignments` | GET | List driver assignments |
| `/drivers/{id}/certifications` | GET | List driver certifications |
| `/drivers/{id}/violations` | GET | List driver violations |
| `/drivers/{id}/trips` | GET | List driver trips |
| `/drivers/{id}/analytics` | GET | Get driver performance analytics |
| `/certifications` | POST | Add driver certification |
| `/certifications/{id}` | PUT | Update certification |
| `/certifications/{id}` | DELETE | Delete certification |
| `/violations` | POST | Record driver violation |
| `/violations/{id}` | PUT | Update violation |
| `/violations/{id}` | DELETE | Delete violation |

### Trip Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/trips` | GET | List trips (with filters and pagination) |
| `/trips` | POST | Create a new trip |
| `/trips/{id}` | GET | Get trip details |
| `/trips/{id}` | PUT | Update trip details |
| `/trips/{id}` | DELETE | Delete a trip |
| `/trips/{id}/start` | POST | Start a trip |
| `/trips/{id}/complete` | POST | Complete a trip |
| `/trips/{id}/cancel` | POST | Cancel a trip |
| `/trips/{id}/waypoints` | GET | Get trip waypoints |
| `/trips/{id}/waypoints` | POST | Add waypoint to trip |
| `/trips/{id}/events` | GET | Get trip events |
| `/trips/{id}/route` | GET | Get trip route |
| `/trips/{id}/telemetry` | GET | Get trip telemetry data |
| `/trips/active` | GET | List all active trips |
| `/trips/scheduled` | GET | List all scheduled trips |
| `/trips/completed` | GET | List all completed trips |
| `/waypoints/{id}` | PUT | Update waypoint |
| `/waypoints/{id}` | DELETE | Delete waypoint |

### Telemetry

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/telemetry/gps` | POST | Record GPS data |
| `/telemetry/fuel` | POST | Record fuel data |
| `/telemetry/engine` | POST | Record engine data |
| `/telemetry/bulk` | POST | Record bulk telemetry data |
| `/telemetry/vehicles/{id}/gps` | GET | Get vehicle GPS history |
| `/telemetry/vehicles/{id}/fuel` | GET | Get vehicle fuel history |
| `/telemetry/vehicles/{id}/engine` | GET | Get vehicle engine history |
| `/telemetry/trips/{id}/replay` | GET | Get trip replay data |

### Geofencing

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/geozones` | GET | List geozones |
| `/geozones` | POST | Create a new geozone |
| `/geozones/{id}` | GET | Get geozone details |
| `/geozones/{id}` | PUT | Update geozone |
| `/geozones/{id}` | DELETE | Delete geozone |
| `/geozones/check` | POST | Check if point is in geozones |
| `/geozones/alerts` | GET | List geozone alerts |
| `/geozones/{id}/alerts` | GET | Get geozone alerts |

### Maintenance

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/maintenance/schedules` | GET | List maintenance schedules |
| `/maintenance/schedules` | POST | Create maintenance schedule |
| `/maintenance/schedules/{id}` | GET | Get schedule details |
| `/maintenance/schedules/{id}` | PUT | Update schedule |
| `/maintenance/schedules/{id}` | DELETE | Delete schedule |
| `/maintenance/records` | GET | List maintenance records |
| `/maintenance/records` | POST | Create maintenance record |
| `/maintenance/records/{id}` | GET | Get record details |
| `/maintenance/records/{id}` | PUT | Update record |
| `/maintenance/records/{id}` | DELETE | Delete record |
| `/maintenance/due` | GET | List upcoming maintenance |

### Analytics and Reporting

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/analytics/vehicles/daily` | GET | Get daily vehicle analytics |
| `/analytics/vehicles/weekly` | GET | Get weekly vehicle analytics |
| `/analytics/vehicles/monthly` | GET | Get monthly vehicle analytics |
| `/analytics/drivers/daily` | GET | Get daily driver analytics |
| `/analytics/drivers/weekly` | GET | Get weekly driver analytics |
| `/analytics/drivers/monthly` | GET | Get monthly driver analytics |
| `/analytics/fleet/utilization` | GET | Get fleet utilization analytics |
| `/analytics/fleet/performance` | GET | Get fleet performance analytics |
| `/analytics/fleet/costs` | GET | Get fleet cost analytics |
| `/reports/trips` | GET | Generate trip reports |
| `/reports/vehicles` | GET | Generate vehicle reports |
| `/reports/drivers` | GET | Generate driver reports |
| `/reports/fuel` | GET | Generate fuel reports |
| `/reports/maintenance` | GET | Generate maintenance reports |
| `/reports/violations` | GET | Generate violation reports |
| `/reports/schedules` | GET | List scheduled reports |
| `/reports/schedules` | POST | Create report schedule |
| `/reports/schedules/{id}` | DELETE | Delete report schedule |

### Inventory Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/inventory/categories` | GET | List inventory categories |
| `/inventory/categories` | POST | Create category |
| `/inventory/categories/{id}` | GET | Get category details |
| `/inventory/categories/{id}` | PUT | Update category |
| `/inventory/categories/{id}` | DELETE | Delete category |
| `/inventory/items` | GET | List inventory items |
| `/inventory/items` | POST | Create item |
| `/inventory/items/{id}` | GET | Get item details |
| `/inventory/items/{id}` | PUT | Update item |
| `/inventory/items/{id}` | DELETE | Delete item |
| `/inventory/transactions` | GET | List transactions |
| `/inventory/transactions` | POST | Create transaction |
| `/inventory/transactions/{id}` | GET | Get transaction details |
| `/inventory/low-stock` | GET | List low stock items |

### Financial Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/vendors` | GET | List vendors |
| `/vendors` | POST | Create vendor |
| `/vendors/{id}` | GET | Get vendor details |
| `/vendors/{id}` | PUT | Update vendor |
| `/vendors/{id}` | DELETE | Delete vendor |
| `/purchase-orders` | GET | List purchase orders |
| `/purchase-orders` | POST | Create purchase order |
| `/purchase-orders/{id}` | GET | Get PO details |
| `/purchase-orders/{id}` | PUT | Update PO |
| `/purchase-orders/{id}` | DELETE | Delete PO |
| `/purchase-orders/{id}/items` | GET | List PO items |
| `/purchase-orders/{id}/items` | POST | Add item to PO |
| `/purchase-orders/{id}/approve` | POST | Approve PO |
| `/invoices` | GET | List invoices |
| `/invoices` | POST | Create invoice |
| `/invoices/{id}` | GET | Get invoice details |
| `/invoices/{id}` | PUT | Update invoice |
| `/invoices/{id}` | DELETE | Delete invoice |
| `/payments` | GET | List payments |
| `/payments` | POST | Create payment |
| `/payments/{id}` | GET | Get payment details |
| `/payments/{id}` | PUT | Update payment |
| `/payments/{id}` | DELETE | Delete payment |

### System Management

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/settings` | GET | Get system settings |
| `/settings` | PUT | Update system settings |
| `/settings/{key}` | GET | Get specific setting |
| `/settings/{key}` | PUT | Update specific setting |
| `/logs` | GET | Get system logs |
| `/health` | GET | System health check |
| `/metrics` | GET | System metrics |

## Request and Response Formats

### Request Format

- For GET requests, parameters are passed as query string parameters
- For POST/PUT/DELETE requests, the request body is in JSON format
- All requests should include the following headers:
  - `Content-Type: application/json`
  - `Authorization: Bearer {token}`
  - `Accept: application/json`

### Response Format

All API responses follow a standard format:

```json
{
  "success": true,
  "data": { ... },
  "meta": {
    "pagination": {
      "total": 100,
      "per_page": 25,
      "current_page": 1,
      "last_page": 4,
      "next_page_url": "https://api.fleettracking.example.com/v1/resource?page=2",
      "prev_page_url": null
    }
  },
  "error": null
}
```

For errors:

```json
{
  "success": false,
  "data": null,
  "meta": {},
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable error message",
    "details": { ... }
  }
}
```

## Data Models

### User

```json
{
  "user_id": 1,
  "username": "john.doe",
  "email": "john.doe@example.com",
  "first_name": "John",
  "last_name": "Doe",
  "is_active": true,
  "company_id": 1,
  "last_login": "2023-06-10T14:30:00Z",
  "created_at": "2023-01-15T09:00:00Z",
  "updated_at": "2023-06-10T14:30:00Z"
}
```

### Vehicle

```json
{
  "vehicle_id": 1,
  "company_id": 1,
  "department_id": 2,
  "type_id": 3,
  "registration_number": "ABC123",
  "vin": "1HGCM82633A123456",
  "make": "Toyota",
  "model": "Hilux",
  "year": 2022,
  "color": "White",
  "fuel_type": "Diesel",
  "tank_capacity": 80.0,
  "current_mileage": 12500.5,
  "status": "active",
  "current_location": {
    "latitude": 37.7749,
    "longitude": -122.4194,
    "altitude": 10.5,
    "speed": 45.2,
    "heading": 90.5,
    "timestamp": "2023-06-15T14:30:00Z"
  },
  "created_at": "2023-01-15T09:00:00Z",
  "updated_at": "2023-06-15T14:30:00Z"
}
```

### Driver

```json
{
  "driver_id": 1,
  "user_id": 5,
  "company_id": 1,
  "department_id": 2,
  "license_number": "DL12345678",
  "license_type": "Commercial",
  "license_expiry": "2025-05-15",
  "date_of_birth": "1985-07-20",
  "hire_date": "2020-03-15",
  "phone": "+1234567890",
  "status": "active",
  "created_at": "2020-03-15T09:00:00Z",
  "updated_at": "2023-01-10T11:25:00Z"
}
```

### Trip

```json
{
  "trip_id": 1,
  "company_id": 1,
  "vehicle_id": 3,
  "driver_id": 2,
  "trip_type": "delivery",
  "status": "in_progress",
  "scheduled_start": "2023-06-15T08:00:00Z",
  "scheduled_end": "2023-06-15T16:00:00Z",
  "actual_start": "2023-06-15T08:15:00Z",
  "actual_end": null,
  "origin_address": "123 Main St, City, Country",
  "origin_geo": {
    "latitude": 37.7749,
    "longitude": -122.4194
  },
  "destination_address": "456 Market St, City, Country",
  "destination_geo": {
    "latitude": 37.7831,
    "longitude": -122.4039
  },
  "estimated_distance": 15.5,
  "actual_distance": 16.2,
  "estimated_duration": 45,
  "actual_duration": null,
  "purpose": "Customer delivery",
  "created_at": "2023-06-10T15:30:00Z",
  "updated_at": "2023-06-15T08:15:00Z"
}
```

## Real-time Data

For real-time updates, the API supports both polling and WebSocket connections:

### Polling

Endpoints for polling current data:
- `/vehicles/{id}/location`
- `/vehicles/{id}/status`
- `/trips/active`

### WebSockets

WebSocket connection for real-time updates:
```
wss://api.fleettracking.example.com/ws
```

Supported channels:
- `vehicle.location.{id}`
- `vehicle.status.{id}`
- `trip.status.{id}`
- `trip.event.{id}`
- `alert.{company_id}`

## Rate Limiting

The API implements rate limiting to prevent abuse:

- Standard tier: 100 requests per minute
- Premium tier: 300 requests per minute
- Telemetry ingestion: 1000 requests per minute

Rate limit headers included in each response:
- `X-RateLimit-Limit`
- `X-RateLimit-Remaining`
- `X-RateLimit-Reset`

## Error Codes

| Code | Description |
|------|-------------|
| `AUTH_INVALID_CREDENTIALS` | Invalid username or password |
| `AUTH_TOKEN_EXPIRED` | JWT token has expired |
| `AUTH_TOKEN_INVALID` | JWT token is invalid |
| `AUTH_INSUFFICIENT_PERMISSIONS` | User lacks required permissions |
| `RESOURCE_NOT_FOUND` | Requested resource not found |
| `VALIDATION_ERROR` | Request validation failed |
| `RATE_LIMIT_EXCEEDED` | Rate limit has been exceeded |
| `SERVER_ERROR` | Internal server error |

## API Versioning

The API uses URL versioning to maintain backward compatibility:
- `/v1/` - Current version
- Future versions will be `/v2/`, `/v3/`, etc.

Major changes will result in a new API version, while minor changes and additions will be made to the current version.

## Documentation

The API includes Swagger/OpenAPI documentation accessible at:
```
https://api.fleettracking.example.com/docs
```

## Implementation Considerations

### Performance Optimizations

- Connection pooling for database access
- Caching for frequently accessed data
- Query optimization for large datasets
- Asynchronous processing for non-critical operations

### Security Measures

- HTTPS for all communications
- JWT with appropriate expiration
- Input validation and sanitization
- Rate limiting and throttling
- Auditing and logging

### Scalability Features

- Stateless API design for horizontal scaling
- Microservices architecture
- Load balancing
- Database sharding for high-volume tables 