-- 002_core_schema.sql

-- Migration Up
-- ============

-- Company Management Tables
CREATE TABLE IF NOT EXISTS companies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    address TEXT,
    city VARCHAR(50),
    state VARCHAR(50),
    country VARCHAR(50),
    postal_code VARCHAR(20),
    phone VARCHAR(20),
    email VARCHAR(100),
    website VARCHAR(100),
    logo_url VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Vehicle Management Tables
CREATE TABLE IF NOT EXISTS vehicle_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS vehicles (
    id SERIAL PRIMARY KEY,
    company_id INTEGER REFERENCES companies(id),
    vehicle_type_id INTEGER REFERENCES vehicle_types(id),
    registration_number VARCHAR(50) NOT NULL UNIQUE,
    make VARCHAR(50),
    model VARCHAR(50),
    year INTEGER,
    color VARCHAR(30),
    vin VARCHAR(50) UNIQUE,
    license_plate VARCHAR(20),
    fuel_type VARCHAR(20),
    fuel_capacity DECIMAL(10, 2),
    status VARCHAR(20) DEFAULT 'inactive',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Driver Management Tables
CREATE TABLE IF NOT EXISTS drivers (
    id SERIAL PRIMARY KEY,
    company_id INTEGER REFERENCES companies(id),
    user_id INTEGER REFERENCES users(id),
    license_number VARCHAR(50) UNIQUE,
    license_expiry DATE,
    status VARCHAR(20) DEFAULT 'inactive',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create default vehicle types
INSERT INTO vehicle_types (name, description) VALUES
    ('Sedan', 'Standard passenger car'),
    ('SUV', 'Sport utility vehicle'),
    ('Truck', 'Goods transportation truck'),
    ('Van', 'Passenger or cargo van'),
    ('Bus', 'Large passenger vehicle');

-- Migration Down
-- ==============

-- Drop tables in reverse order due to foreign key constraints
DROP TABLE IF EXISTS drivers;
DROP TABLE IF EXISTS vehicles;
DROP TABLE IF EXISTS vehicle_types;
DROP TABLE IF EXISTS companies; 