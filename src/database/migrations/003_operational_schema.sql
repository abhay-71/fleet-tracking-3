-- 003_operational_schema.sql

-- Migration Up
-- ============

-- Trip Management Tables
CREATE TABLE IF NOT EXISTS trips (
    id SERIAL PRIMARY KEY,
    vehicle_id INTEGER REFERENCES vehicles(id),
    driver_id INTEGER REFERENCES drivers(id),
    start_location VARCHAR(255),
    end_location VARCHAR(255),
    start_time TIMESTAMP WITH TIME ZONE,
    end_time TIMESTAMP WITH TIME ZONE,
    start_location_geom GEOMETRY(Point, 4326),
    end_location_geom GEOMETRY(Point, 4326),
    distance_km DECIMAL(10, 2),
    status VARCHAR(20) DEFAULT 'scheduled',
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS trip_waypoints (
    id SERIAL PRIMARY KEY,
    trip_id INTEGER REFERENCES trips(id) ON DELETE CASCADE,
    sequence_number INTEGER NOT NULL,
    latitude DECIMAL(10, 8) NOT NULL,
    longitude DECIMAL(11, 8) NOT NULL,
    location GEOMETRY(Point, 4326),
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    speed DECIMAL(5, 2),
    heading INTEGER,
    elevation DECIMAL(8, 2),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Telemetry and Status Tables
CREATE TABLE IF NOT EXISTS vehicle_status (
    id SERIAL PRIMARY KEY,
    vehicle_id INTEGER REFERENCES vehicles(id),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    location GEOMETRY(Point, 4326),
    speed DECIMAL(5, 2),
    heading INTEGER,
    fuel_level DECIMAL(5, 2),
    engine_status VARCHAR(20),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    trip_id INTEGER REFERENCES trips(id),
    is_idle BOOLEAN,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Maintenance Records
CREATE TABLE IF NOT EXISTS maintenance_records (
    id SERIAL PRIMARY KEY,
    vehicle_id INTEGER REFERENCES vehicles(id),
    maintenance_type VARCHAR(50),
    description TEXT,
    performed_by VARCHAR(100),
    performed_at TIMESTAMP WITH TIME ZONE,
    cost DECIMAL(10, 2),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Alerts and Notifications
CREATE TABLE IF NOT EXISTS alerts (
    id SERIAL PRIMARY KEY,
    alert_type VARCHAR(50) NOT NULL,
    vehicle_id INTEGER REFERENCES vehicles(id),
    driver_id INTEGER REFERENCES drivers(id),
    message TEXT NOT NULL,
    is_read BOOLEAN DEFAULT FALSE,
    severity VARCHAR(20),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    coordinates GEOMETRY(Point, 4326),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Geofencing
CREATE TABLE IF NOT EXISTS geo_zones (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    company_id INTEGER REFERENCES companies(id),
    description TEXT,
    boundary GEOMETRY(Polygon, 4326),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance optimization
CREATE INDEX IF NOT EXISTS idx_vehicle_status_vehicle_id ON vehicle_status(vehicle_id);
CREATE INDEX IF NOT EXISTS idx_vehicle_status_timestamp ON vehicle_status(timestamp);
CREATE INDEX IF NOT EXISTS idx_trip_waypoints_trip_id ON trip_waypoints(trip_id);
CREATE INDEX IF NOT EXISTS idx_trip_waypoints_timestamp ON trip_waypoints(timestamp);
CREATE INDEX IF NOT EXISTS idx_trips_vehicle_id ON trips(vehicle_id);
CREATE INDEX IF NOT EXISTS idx_trips_driver_id ON trips(driver_id);
CREATE INDEX IF NOT EXISTS idx_trips_status ON trips(status);
CREATE INDEX IF NOT EXISTS idx_maintenance_records_vehicle_id ON maintenance_records(vehicle_id);

-- Spatial indexes for geospatial queries
CREATE INDEX IF NOT EXISTS idx_trips_start_location_geom ON trips USING GIST(start_location_geom);
CREATE INDEX IF NOT EXISTS idx_trips_end_location_geom ON trips USING GIST(end_location_geom);
CREATE INDEX IF NOT EXISTS idx_trip_waypoints_location ON trip_waypoints USING GIST(location);
CREATE INDEX IF NOT EXISTS idx_vehicle_status_location ON vehicle_status USING GIST(location);
CREATE INDEX IF NOT EXISTS idx_geo_zones_boundary ON geo_zones USING GIST(boundary);

-- Migration Down
-- ==============

-- Drop indexes (not strictly necessary as they'll be dropped with the tables)
DROP INDEX IF EXISTS idx_vehicle_status_vehicle_id;
DROP INDEX IF EXISTS idx_vehicle_status_timestamp;
DROP INDEX IF EXISTS idx_trip_waypoints_trip_id;
DROP INDEX IF EXISTS idx_trip_waypoints_timestamp;
DROP INDEX IF EXISTS idx_trips_vehicle_id;
DROP INDEX IF EXISTS idx_trips_driver_id;
DROP INDEX IF EXISTS idx_trips_status;
DROP INDEX IF EXISTS idx_maintenance_records_vehicle_id;
DROP INDEX IF EXISTS idx_trips_start_location_geom;
DROP INDEX IF EXISTS idx_trips_end_location_geom;
DROP INDEX IF EXISTS idx_trip_waypoints_location;
DROP INDEX IF EXISTS idx_vehicle_status_location;
DROP INDEX IF EXISTS idx_geo_zones_boundary;

-- Drop tables in reverse order due to foreign key constraints
DROP TABLE IF EXISTS geo_zones;
DROP TABLE IF EXISTS alerts;
DROP TABLE IF EXISTS maintenance_records;
DROP TABLE IF EXISTS vehicle_status;
DROP TABLE IF EXISTS trip_waypoints;
DROP TABLE IF EXISTS trips; 