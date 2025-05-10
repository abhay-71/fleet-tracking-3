# Fleet Tracking Application - Database Schema

This document outlines the detailed PostgreSQL database schema for the Fleet Tracking Application.

## Database Overview

The database design follows a normalized structure with strategic denormalization for performance optimization. It includes support for spatial data through PostGIS extension and uses inheritance and partitioning for efficient data storage and retrieval, especially for time-series telemetry data.

### Extensions

- PostGIS for spatial data handling
- TimescaleDB (optional) for time-series data management
- pgcrypto for encryption functions

## Schema Diagrams

### Core Tables

![Core Tables Schema Diagram]

### Telemetry Tables

![Telemetry Tables Schema Diagram]

### Financial Tables

![Financial Tables Schema Diagram]

## Table Definitions

### Authentication & Authorization

#### `users`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| user_id | SERIAL | PRIMARY KEY | Unique user identifier |
| username | VARCHAR(50) | NOT NULL, UNIQUE | User login name |
| email | VARCHAR(100) | NOT NULL, UNIQUE | User email address |
| password_hash | VARCHAR(255) | NOT NULL | Hashed user password |
| salt | VARCHAR(50) | NOT NULL | Password salt |
| first_name | VARCHAR(50) | NOT NULL | User's first name |
| last_name | VARCHAR(50) | NOT NULL | User's last name |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Whether user account is active |
| last_login | TIMESTAMP | | Last login timestamp |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Account creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |
| company_id | INT | REFERENCES companies(company_id) | Associated company |

#### `roles`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| role_id | SERIAL | PRIMARY KEY | Unique role identifier |
| name | VARCHAR(50) | NOT NULL, UNIQUE | Role name |
| description | VARCHAR(255) | | Role description |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `permissions`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| permission_id | SERIAL | PRIMARY KEY | Unique permission identifier |
| name | VARCHAR(50) | NOT NULL, UNIQUE | Permission name |
| description | VARCHAR(255) | | Permission description |
| resource | VARCHAR(50) | NOT NULL | Resource this permission applies to |
| action | VARCHAR(50) | NOT NULL | Action type (read, write, delete, etc.) |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### `role_permissions`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| role_id | INT | NOT NULL, REFERENCES roles(role_id) | Associated role |
| permission_id | INT | NOT NULL, REFERENCES permissions(permission_id) | Associated permission |
| PRIMARY KEY | | (role_id, permission_id) | Composite primary key |

#### `user_roles`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| user_id | INT | NOT NULL, REFERENCES users(user_id) | Associated user |
| role_id | INT | NOT NULL, REFERENCES roles(role_id) | Associated role |
| assigned_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Assignment timestamp |
| assigned_by | INT | REFERENCES users(user_id) | User who assigned this role |
| PRIMARY KEY | | (user_id, role_id) | Composite primary key |

### Organization

#### `companies`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| company_id | SERIAL | PRIMARY KEY | Unique company identifier |
| name | VARCHAR(100) | NOT NULL | Company name |
| address | VARCHAR(255) | | Physical address |
| city | VARCHAR(50) | | City |
| state | VARCHAR(50) | | State/province |
| country | VARCHAR(50) | | Country |
| postal_code | VARCHAR(20) | | Postal/ZIP code |
| phone | VARCHAR(20) | | Contact phone number |
| email | VARCHAR(100) | | Contact email |
| website | VARCHAR(100) | | Company website |
| logo_url | VARCHAR(255) | | URL to company logo |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Whether company is active |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `departments`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| department_id | SERIAL | PRIMARY KEY | Unique department identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| name | VARCHAR(100) | NOT NULL | Department name |
| description | VARCHAR(255) | | Department description |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `vendors`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| vendor_id | SERIAL | PRIMARY KEY | Unique vendor identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| name | VARCHAR(100) | NOT NULL | Vendor name |
| contact_person | VARCHAR(100) | | Primary contact person |
| phone | VARCHAR(20) | | Contact phone |
| email | VARCHAR(100) | | Contact email |
| address | VARCHAR(255) | | Vendor address |
| service_type | VARCHAR(50) | | Type of service provided |
| contract_details | TEXT | | Contract information |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Whether vendor is active |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

### Vehicle Management

#### `vehicle_types`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| type_id | SERIAL | PRIMARY KEY | Unique type identifier |
| name | VARCHAR(50) | NOT NULL | Type name (sedan, truck, van, etc.) |
| description | VARCHAR(255) | | Type description |
| capacity | DECIMAL(10,2) | | Cargo capacity in tons |
| fuel_efficiency | DECIMAL(5,2) | | Average fuel efficiency |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### `vehicles`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| vehicle_id | SERIAL | PRIMARY KEY | Unique vehicle identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| department_id | INT | REFERENCES departments(department_id) | Associated department |
| type_id | INT | NOT NULL, REFERENCES vehicle_types(type_id) | Vehicle type |
| registration_number | VARCHAR(20) | NOT NULL, UNIQUE | Vehicle registration number |
| vin | VARCHAR(50) | NOT NULL, UNIQUE | Vehicle identification number |
| make | VARCHAR(50) | NOT NULL | Vehicle manufacturer |
| model | VARCHAR(50) | NOT NULL | Vehicle model |
| year | INT | NOT NULL | Manufacturing year |
| color | VARCHAR(20) | | Vehicle color |
| fuel_type | VARCHAR(20) | | Type of fuel used |
| tank_capacity | DECIMAL(8,2) | | Fuel tank capacity in liters |
| current_mileage | DECIMAL(12,2) | | Current vehicle mileage |
| purchase_date | DATE | | Date vehicle was purchased |
| purchase_price | DECIMAL(12,2) | | Purchase price |
| vendor_id | INT | REFERENCES vendors(vendor_id) | Vendor vehicle was purchased from |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'available' | Current status (available, maintenance, retired) |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `vehicle_assignments`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| assignment_id | SERIAL | PRIMARY KEY | Unique assignment identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Assigned vehicle |
| driver_id | INT | NOT NULL, REFERENCES drivers(driver_id) | Assigned driver |
| start_date | TIMESTAMP | NOT NULL | Assignment start date |
| end_date | TIMESTAMP | | Assignment end date (null if ongoing) |
| assigned_by | INT | REFERENCES users(user_id) | User who made the assignment |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'active' | Assignment status |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `vehicle_devices`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| device_id | SERIAL | PRIMARY KEY | Unique device identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| device_type | VARCHAR(50) | NOT NULL | Type of device (GPS, fuel sensor, etc.) |
| serial_number | VARCHAR(50) | NOT NULL, UNIQUE | Device serial number |
| model | VARCHAR(50) | | Device model |
| manufacturer | VARCHAR(50) | | Device manufacturer |
| installation_date | DATE | NOT NULL | Date device was installed |
| last_maintenance | DATE | | Last maintenance date |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'active' | Device status |
| firmware_version | VARCHAR(20) | | Current firmware version |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `maintenance_schedules`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| schedule_id | SERIAL | PRIMARY KEY | Unique schedule identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| maintenance_type | VARCHAR(50) | NOT NULL | Type of maintenance |
| interval_type | VARCHAR(20) | NOT NULL | Interval type (mileage, time) |
| interval_value | INT | NOT NULL | Interval value (miles or days) |
| last_performed | TIMESTAMP | | When maintenance was last performed |
| last_value | DECIMAL(12,2) | | Mileage when last performed |
| next_due | TIMESTAMP | | When next maintenance is due |
| next_value | DECIMAL(12,2) | | Mileage when next due |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `maintenance_records`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| record_id | SERIAL | PRIMARY KEY | Unique record identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| schedule_id | INT | REFERENCES maintenance_schedules(schedule_id) | Associated schedule |
| maintenance_type | VARCHAR(50) | NOT NULL | Type of maintenance |
| performed_date | TIMESTAMP | NOT NULL | When maintenance was performed |
| mileage | DECIMAL(12,2) | NOT NULL | Vehicle mileage at maintenance |
| cost | DECIMAL(10,2) | | Maintenance cost |
| vendor_id | INT | REFERENCES vendors(vendor_id) | Vendor who performed maintenance |
| technician | VARCHAR(100) | | Technician who performed maintenance |
| description | TEXT | | Detailed description of work |
| parts_replaced | TEXT | | Parts that were replaced |
| notes | TEXT | | Additional notes |
| created_by | INT | REFERENCES users(user_id) | User who created record |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

### Driver Management

#### `drivers`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| driver_id | SERIAL | PRIMARY KEY | Unique driver identifier |
| user_id | INT | REFERENCES users(user_id) | Associated user account |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| department_id | INT | REFERENCES departments(department_id) | Associated department |
| license_number | VARCHAR(50) | NOT NULL, UNIQUE | Driver's license number |
| license_type | VARCHAR(20) | NOT NULL | Type of license |
| license_expiry | DATE | NOT NULL | License expiration date |
| date_of_birth | DATE | NOT NULL | Driver's date of birth |
| hire_date | DATE | NOT NULL | Date driver was hired |
| phone | VARCHAR(20) | NOT NULL | Contact phone number |
| emergency_contact | VARCHAR(100) | | Emergency contact name |
| emergency_phone | VARCHAR(20) | | Emergency contact phone |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'active' | Driver status |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `driver_certifications`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| certification_id | SERIAL | PRIMARY KEY | Unique certification identifier |
| driver_id | INT | NOT NULL, REFERENCES drivers(driver_id) | Associated driver |
| certification_type | VARCHAR(50) | NOT NULL | Type of certification |
| certification_number | VARCHAR(50) | | Certification number |
| issuing_authority | VARCHAR(100) | NOT NULL | Authority that issued certification |
| issue_date | DATE | NOT NULL | Date certification was issued |
| expiry_date | DATE | NOT NULL | Date certification expires |
| document_url | VARCHAR(255) | | URL to certification document |
| verified | BOOLEAN | NOT NULL, DEFAULT false | Whether certification has been verified |
| verified_by | INT | REFERENCES users(user_id) | User who verified certification |
| verified_date | TIMESTAMP | | Date certification was verified |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `driver_violations`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| violation_id | SERIAL | PRIMARY KEY | Unique violation identifier |
| driver_id | INT | NOT NULL, REFERENCES drivers(driver_id) | Associated driver |
| vehicle_id | INT | REFERENCES vehicles(vehicle_id) | Associated vehicle |
| violation_type | VARCHAR(50) | NOT NULL | Type of violation |
| violation_date | TIMESTAMP | NOT NULL | Date and time of violation |
| location | VARCHAR(255) | | Location of violation |
| geo_location | GEOGRAPHY(POINT) | | Precise geo-location of violation |
| description | TEXT | NOT NULL | Detailed description |
| severity | VARCHAR(20) | NOT NULL | Violation severity (minor, major, critical) |
| points | INT | | Points assessed for violation |
| fine_amount | DECIMAL(10,2) | | Fine amount if applicable |
| reported_by | INT | REFERENCES users(user_id) | User who reported violation |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'pending' | Violation status |
| resolution | TEXT | | Resolution details |
| resolved_date | TIMESTAMP | | Date violation was resolved |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

### Trip Management

#### `trips`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| trip_id | SERIAL | PRIMARY KEY | Unique trip identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Vehicle used for trip |
| driver_id | INT | NOT NULL, REFERENCES drivers(driver_id) | Driver assigned to trip |
| trip_type | VARCHAR(50) | NOT NULL | Type of trip |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'scheduled' | Trip status |
| scheduled_start | TIMESTAMP | NOT NULL | Scheduled start time |
| scheduled_end | TIMESTAMP | | Scheduled end time |
| actual_start | TIMESTAMP | | Actual start time |
| actual_end | TIMESTAMP | | Actual end time |
| origin_address | VARCHAR(255) | NOT NULL | Trip origin address |
| origin_geo | GEOGRAPHY(POINT) | NOT NULL | Origin geo-location |
| destination_address | VARCHAR(255) | NOT NULL | Trip destination address |
| destination_geo | GEOGRAPHY(POINT) | NOT NULL | Destination geo-location |
| estimated_distance | DECIMAL(10,2) | | Estimated trip distance in km |
| actual_distance | DECIMAL(10,2) | | Actual trip distance in km |
| estimated_duration | INT | | Estimated trip duration in minutes |
| actual_duration | INT | | Actual trip duration in minutes |
| purpose | VARCHAR(255) | | Trip purpose |
| notes | TEXT | | Additional notes |
| created_by | INT | REFERENCES users(user_id) | User who created the trip |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `trip_waypoints`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| waypoint_id | SERIAL | PRIMARY KEY | Unique waypoint identifier |
| trip_id | INT | NOT NULL, REFERENCES trips(trip_id) | Associated trip |
| sequence | INT | NOT NULL | Order in the sequence of waypoints |
| address | VARCHAR(255) | | Waypoint address |
| geo_location | GEOGRAPHY(POINT) | NOT NULL | Waypoint geo-location |
| scheduled_arrival | TIMESTAMP | | Scheduled arrival time |
| actual_arrival | TIMESTAMP | | Actual arrival time |
| scheduled_departure | TIMESTAMP | | Scheduled departure time |
| actual_departure | TIMESTAMP | | Actual departure time |
| stop_purpose | VARCHAR(100) | | Purpose of the stop |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `trip_events`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| event_id | SERIAL | PRIMARY KEY | Unique event identifier |
| trip_id | INT | NOT NULL, REFERENCES trips(trip_id) | Associated trip |
| event_type | VARCHAR(50) | NOT NULL | Type of event |
| event_time | TIMESTAMP | NOT NULL | Time of the event |
| geo_location | GEOGRAPHY(POINT) | | Event geo-location |
| description | TEXT | | Event description |
| severity | VARCHAR(20) | | Event severity (info, warning, critical) |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

### Telemetry Data

#### `gps_logs`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| log_id | BIGSERIAL | PRIMARY KEY | Unique log identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| device_id | INT | REFERENCES vehicle_devices(device_id) | Associated GPS device |
| trip_id | INT | REFERENCES trips(trip_id) | Associated trip (if any) |
| timestamp | TIMESTAMP | NOT NULL | GPS reading timestamp |
| geo_location | GEOGRAPHY(POINT) | NOT NULL | GPS coordinates |
| altitude | DECIMAL(10,2) | | Altitude in meters |
| speed | DECIMAL(10,2) | | Speed in km/h |
| heading | DECIMAL(5,2) | | Heading in degrees |
| accuracy | DECIMAL(5,2) | | GPS accuracy in meters |
| odometer | DECIMAL(12,2) | | Vehicle odometer reading |
| engine_status | BOOLEAN | | Engine running status |
| satellite_count | INT | | Number of GPS satellites |
| hdop | DECIMAL(5,2) | | Horizontal dilution of precision |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### `fuel_logs`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| log_id | BIGSERIAL | PRIMARY KEY | Unique log identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| device_id | INT | REFERENCES vehicle_devices(device_id) | Associated fuel sensor |
| trip_id | INT | REFERENCES trips(trip_id) | Associated trip (if any) |
| timestamp | TIMESTAMP | NOT NULL | Fuel reading timestamp |
| fuel_level | DECIMAL(5,2) | NOT NULL | Fuel level percentage |
| fuel_volume | DECIMAL(10,2) | | Fuel volume in liters |
| fuel_consumption_rate | DECIMAL(5,2) | | Current consumption rate (l/100km) |
| geo_location | GEOGRAPHY(POINT) | | GPS coordinates at reading |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### `engine_logs`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| log_id | BIGSERIAL | PRIMARY KEY | Unique log identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| device_id | INT | REFERENCES vehicle_devices(device_id) | Associated device |
| trip_id | INT | REFERENCES trips(trip_id) | Associated trip (if any) |
| timestamp | TIMESTAMP | NOT NULL | Reading timestamp |
| engine_rpm | INT | | Engine RPM |
| engine_load | DECIMAL(5,2) | | Engine load percentage |
| coolant_temp | DECIMAL(5,2) | | Coolant temperature in Celsius |
| oil_pressure | DECIMAL(5,2) | | Oil pressure |
| intake_temp | DECIMAL(5,2) | | Intake temperature in Celsius |
| geo_location | GEOGRAPHY(POINT) | | GPS coordinates at reading |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

### Analytics Data

#### `vehicle_daily_summary`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| summary_id | SERIAL | PRIMARY KEY | Unique summary identifier |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| summary_date | DATE | NOT NULL | Summary date |
| distance_traveled | DECIMAL(10,2) | | Total distance traveled in km |
| engine_hours | DECIMAL(5,2) | | Total engine running hours |
| idle_time | DECIMAL(5,2) | | Total idle time in hours |
| max_speed | DECIMAL(5,2) | | Maximum speed recorded |
| avg_speed | DECIMAL(5,2) | | Average speed |
| fuel_used | DECIMAL(10,2) | | Total fuel used in liters |
| fuel_cost | DECIMAL(10,2) | | Total fuel cost |
| trip_count | INT | | Number of trips completed |
| harsh_braking_count | INT | | Number of harsh braking events |
| harsh_acceleration_count | INT | | Number of harsh acceleration events |
| speeding_events | INT | | Number of speeding events |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `driver_daily_summary`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| summary_id | SERIAL | PRIMARY KEY | Unique summary identifier |
| driver_id | INT | NOT NULL, REFERENCES drivers(driver_id) | Associated driver |
| summary_date | DATE | NOT NULL | Summary date |
| vehicle_id | INT | REFERENCES vehicles(vehicle_id) | Vehicle driven |
| distance_traveled | DECIMAL(10,2) | | Total distance traveled in km |
| driving_time | DECIMAL(5,2) | | Total driving time in hours |
| rest_time | DECIMAL(5,2) | | Total rest time in hours |
| trip_count | INT | | Number of trips completed |
| harsh_braking_count | INT | | Number of harsh braking events |
| harsh_acceleration_count | INT | | Number of harsh acceleration events |
| speeding_events | INT | | Number of speeding events |
| fuel_efficiency | DECIMAL(5,2) | | Average fuel efficiency (km/l) |
| safety_score | DECIMAL(5,2) | | Overall safety score |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

### Geofencing

#### `geo_zones`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| zone_id | SERIAL | PRIMARY KEY | Unique zone identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| name | VARCHAR(100) | NOT NULL | Zone name |
| description | TEXT | | Zone description |
| zone_type | VARCHAR(20) | NOT NULL | Zone type (customer, depot, restricted, etc.) |
| geometry | GEOGRAPHY(POLYGON) | NOT NULL | Zone boundary geometry |
| address | VARCHAR(255) | | Zone address |
| speed_limit | INT | | Speed limit in zone (km/h) |
| active | BOOLEAN | NOT NULL, DEFAULT true | Whether zone is active |
| created_by | INT | REFERENCES users(user_id) | User who created zone |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `zone_alerts`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| alert_id | SERIAL | PRIMARY KEY | Unique alert identifier |
| zone_id | INT | NOT NULL, REFERENCES geo_zones(zone_id) | Associated geo-zone |
| vehicle_id | INT | NOT NULL, REFERENCES vehicles(vehicle_id) | Associated vehicle |
| driver_id | INT | REFERENCES drivers(driver_id) | Associated driver |
| trip_id | INT | REFERENCES trips(trip_id) | Associated trip |
| alert_type | VARCHAR(20) | NOT NULL | Alert type (entry, exit, speeding, etc.) |
| timestamp | TIMESTAMP | NOT NULL | Alert timestamp |
| geo_location | GEOGRAPHY(POINT) | NOT NULL | Location at alert time |
| speed | DECIMAL(5,2) | | Vehicle speed at alert time |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

### Inventory Management

#### `inventory_categories`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| category_id | SERIAL | PRIMARY KEY | Unique category identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| name | VARCHAR(100) | NOT NULL | Category name |
| description | TEXT | | Category description |
| parent_category_id | INT | REFERENCES inventory_categories(category_id) | Parent category |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `inventory_items`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| item_id | SERIAL | PRIMARY KEY | Unique item identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| category_id | INT | REFERENCES inventory_categories(category_id) | Item category |
| name | VARCHAR(100) | NOT NULL | Item name |
| description | TEXT | | Item description |
| sku | VARCHAR(50) | UNIQUE | Stock keeping unit |
| barcode | VARCHAR(50) | | Item barcode |
| unit_of_measure | VARCHAR(20) | NOT NULL | Unit of measure (each, kg, liter, etc.) |
| current_quantity | DECIMAL(10,2) | NOT NULL, DEFAULT 0 | Current quantity in stock |
| minimum_quantity | DECIMAL(10,2) | DEFAULT 0 | Reorder threshold |
| reorder_quantity | DECIMAL(10,2) | DEFAULT 0 | Quantity to reorder |
| unit_cost | DECIMAL(10,2) | | Item unit cost |
| unit_price | DECIMAL(10,2) | | Item unit price |
| location | VARCHAR(50) | | Storage location |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Whether item is active |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `inventory_transactions`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| transaction_id | SERIAL | PRIMARY KEY | Unique transaction identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| item_id | INT | NOT NULL, REFERENCES inventory_items(item_id) | Associated inventory item |
| transaction_type | VARCHAR(20) | NOT NULL | Transaction type (purchase, use, adjustment, transfer) |
| quantity | DECIMAL(10,2) | NOT NULL | Transaction quantity |
| unit_cost | DECIMAL(10,2) | | Unit cost for transaction |
| total_cost | DECIMAL(10,2) | | Total cost for transaction |
| reference_id | VARCHAR(50) | | Reference identifier (PO number, etc.) |
| notes | TEXT | | Transaction notes |
| performed_by | INT | REFERENCES users(user_id) | User who performed transaction |
| vehicle_id | INT | REFERENCES vehicles(vehicle_id) | Associated vehicle |
| maintenance_id | INT | REFERENCES maintenance_records(record_id) | Associated maintenance record |
| transaction_date | TIMESTAMP | NOT NULL, DEFAULT NOW() | Transaction timestamp |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

### Financial Management

#### `vendors`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| vendor_id | SERIAL | PRIMARY KEY | Unique vendor identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| name | VARCHAR(100) | NOT NULL | Vendor name |
| contact_person | VARCHAR(100) | | Primary contact person |
| phone | VARCHAR(20) | | Contact phone |
| email | VARCHAR(100) | | Contact email |
| address | VARCHAR(255) | | Vendor address |
| payment_terms | VARCHAR(50) | | Payment terms |
| vendor_type | VARCHAR(50) | | Type of vendor |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Whether vendor is active |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `purchase_orders`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| po_id | SERIAL | PRIMARY KEY | Unique purchase order identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| vendor_id | INT | NOT NULL, REFERENCES vendors(vendor_id) | Associated vendor |
| po_number | VARCHAR(50) | NOT NULL, UNIQUE | Purchase order number |
| order_date | DATE | NOT NULL | Order date |
| expected_delivery | DATE | | Expected delivery date |
| actual_delivery | DATE | | Actual delivery date |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'draft' | PO status |
| shipping_address | VARCHAR(255) | | Shipping address |
| shipping_method | VARCHAR(50) | | Shipping method |
| subtotal | DECIMAL(12,2) | NOT NULL | Order subtotal |
| tax_amount | DECIMAL(12,2) | NOT NULL | Tax amount |
| shipping_cost | DECIMAL(12,2) | NOT NULL | Shipping cost |
| total_amount | DECIMAL(12,2) | NOT NULL | Total order amount |
| notes | TEXT | | Order notes |
| created_by | INT | REFERENCES users(user_id) | User who created PO |
| approved_by | INT | REFERENCES users(user_id) | User who approved PO |
| approved_date | TIMESTAMP | | Approval timestamp |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `po_items`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| item_id | SERIAL | PRIMARY KEY | Unique PO item identifier |
| po_id | INT | NOT NULL, REFERENCES purchase_orders(po_id) | Associated purchase order |
| inventory_item_id | INT | REFERENCES inventory_items(item_id) | Associated inventory item |
| description | VARCHAR(255) | NOT NULL | Item description |
| quantity | DECIMAL(10,2) | NOT NULL | Ordered quantity |
| unit_price | DECIMAL(10,2) | NOT NULL | Unit price |
| tax_rate | DECIMAL(5,2) | NOT NULL, DEFAULT 0 | Tax rate percentage |
| tax_amount | DECIMAL(10,2) | NOT NULL, DEFAULT 0 | Tax amount |
| total_price | DECIMAL(10,2) | NOT NULL | Total price including tax |
| received_quantity | DECIMAL(10,2) | DEFAULT 0 | Quantity received |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `invoices`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| invoice_id | SERIAL | PRIMARY KEY | Unique invoice identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| vendor_id | INT | NOT NULL, REFERENCES vendors(vendor_id) | Associated vendor |
| po_id | INT | REFERENCES purchase_orders(po_id) | Associated purchase order |
| invoice_number | VARCHAR(50) | NOT NULL | Invoice number |
| invoice_date | DATE | NOT NULL | Invoice date |
| due_date | DATE | NOT NULL | Payment due date |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'unpaid' | Invoice status |
| subtotal | DECIMAL(12,2) | NOT NULL | Invoice subtotal |
| tax_amount | DECIMAL(12,2) | NOT NULL, DEFAULT 0 | Tax amount |
| total_amount | DECIMAL(12,2) | NOT NULL | Total invoice amount |
| paid_amount | DECIMAL(12,2) | NOT NULL, DEFAULT 0 | Amount paid so far |
| balance_due | DECIMAL(12,2) | NOT NULL | Balance due |
| notes | TEXT | | Invoice notes |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

#### `payments`
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| payment_id | SERIAL | PRIMARY KEY | Unique payment identifier |
| company_id | INT | NOT NULL, REFERENCES companies(company_id) | Associated company |
| vendor_id | INT | NOT NULL, REFERENCES vendors(vendor_id) | Associated vendor |
| invoice_id | INT | REFERENCES invoices(invoice_id) | Associated invoice |
| payment_number | VARCHAR(50) | NOT NULL, UNIQUE | Payment reference number |
| payment_date | DATE | NOT NULL | Payment date |
| payment_method | VARCHAR(50) | NOT NULL | Payment method |
| amount | DECIMAL(12,2) | NOT NULL | Payment amount |
| reference | VARCHAR(100) | | Payment reference details |
| notes | TEXT | | Payment notes |
| created_by | INT | REFERENCES users(user_id) | User who recorded payment |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last update timestamp |

## Indexes and Constraints

### Primary Indexes
- Primary key on all tables as specified

### Foreign Key Constraints
- Foreign key constraints as specified in table definitions

### Performance Indexes
- Index on `gps_logs(vehicle_id, timestamp)`
- Index on `gps_logs(trip_id, timestamp)`
- Index on `gps_logs using GIST(geo_location)`
- Index on `vehicle_daily_summary(vehicle_id, summary_date)`
- Index on `driver_daily_summary(driver_id, summary_date)`
- Index on `trips(vehicle_id, scheduled_start)`
- Index on `trips(driver_id, scheduled_start)`
- Index on `trips(status, scheduled_start)`
- Index on `geo_zones using GIST(geometry)`
- Index on `inventory_transactions(item_id, transaction_date)`
- Index on `payments(invoice_id)`
- Index on `invoices(vendor_id, invoice_date)`

## Partitioning Strategy

For high-volume tables (particularly telemetry data), partitioning will be used for improved performance:

### Time-Based Partitioning
- `gps_logs` partitioned by month
- `fuel_logs` partitioned by month
- `engine_logs` partitioned by month
- `trip_events` partitioned by month

## Spatial Data Optimization

- PostGIS spatial indexes on all GEOGRAPHY columns
- Geospatial functions for distance calculations, containment testing, etc.
- Coordinate reference system: EPSG:4326 (WGS 84)

## Data Retention Policy

- Detailed telemetry data: 12 months
- Aggregated daily/weekly/monthly data: 5 years
- Trip data: 5 years
- Financial data: 7 years or as required by regulations

## Database Backup Strategy

- Daily full backups
- Hourly incremental backups
- Point-in-time recovery capability
- Offsite backup storage

## Performance Considerations

- Connection pooling with pgBouncer
- Statement timeouts for long-running queries
- Asynchronous commit for high-volume telemetry inserts
- Read replicas for reporting queries
- Caching layer for frequently accessed data
- Data archiving for historical data

## Maintenance Operations

- Regular VACUUM and ANALYZE operations
- Index rebuilding schedule
- Statistics collection
- Disk space monitoring
- Extension updates 