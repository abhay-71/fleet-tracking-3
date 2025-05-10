-- 001_initial_schema.sql

-- Migration Up
-- ============

-- Enable PostGIS extension for spatial data
CREATE EXTENSION IF NOT EXISTS postgis;

-- User Management Tables
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS permissions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS role_permissions (
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    permission_id INTEGER REFERENCES permissions(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(100) NOT NULL,
    first_name VARCHAR(50),
    last_name VARCHAR(50),
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    last_login TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS user_roles (
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, role_id)
);

-- Create default roles and permissions
INSERT INTO roles (name, description) VALUES 
    ('admin', 'System administrator with full access'),
    ('manager', 'Company manager with company-wide access'),
    ('dispatcher', 'User responsible for managing trips and vehicles'),
    ('driver', 'Vehicle driver with limited access'),
    ('viewer', 'User with read-only access');

INSERT INTO permissions (name, description) VALUES
    ('user:read', 'Can view user information'),
    ('user:create', 'Can create users'),
    ('user:update', 'Can update user information'),
    ('user:delete', 'Can delete users'),
    ('vehicle:read', 'Can view vehicle information'),
    ('vehicle:create', 'Can add vehicles'),
    ('vehicle:update', 'Can update vehicle information'),
    ('vehicle:delete', 'Can delete vehicles'),
    ('trip:read', 'Can view trip information'),
    ('trip:create', 'Can create trips'),
    ('trip:update', 'Can update trip information'),
    ('trip:delete', 'Can delete trips');

-- Assign permissions to roles
-- Admin role gets all permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    (SELECT id FROM roles WHERE name = 'admin'),
    id
FROM permissions;

-- Manager role permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    (SELECT id FROM roles WHERE name = 'manager'),
    id
FROM permissions
WHERE name IN ('user:read', 'user:create', 'user:update', 'vehicle:read', 'vehicle:create', 'vehicle:update', 'trip:read', 'trip:create', 'trip:update');

-- Dispatcher role permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    (SELECT id FROM roles WHERE name = 'dispatcher'),
    id
FROM permissions
WHERE name IN ('user:read', 'vehicle:read', 'vehicle:update', 'trip:read', 'trip:create', 'trip:update');

-- Driver role permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    (SELECT id FROM roles WHERE name = 'driver'),
    id
FROM permissions
WHERE name IN ('vehicle:read', 'trip:read', 'trip:update');

-- Viewer role permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    (SELECT id FROM roles WHERE name = 'viewer'),
    id
FROM permissions
WHERE name IN ('user:read', 'vehicle:read', 'trip:read');

-- Create default admin user (password is 'admin')
INSERT INTO users (username, email, password_hash, first_name, last_name, is_active)
VALUES ('admin', 'admin@fleetvigil.com', '$2a$10$3euPcmQFCiblsZAp9DvtDetKEUw0tRU0HJ3Y2LgSRaWr0Y5pHzHie', 'System', 'Administrator', true);

-- Assign admin role to admin user
INSERT INTO user_roles (user_id, role_id)
VALUES (
    (SELECT id FROM users WHERE username = 'admin'),
    (SELECT id FROM roles WHERE name = 'admin')
);

-- Migration Down
-- ==============

-- Disable this in production!
DROP TABLE IF EXISTS user_roles;
DROP TABLE IF EXISTS role_permissions;
DROP TABLE IF EXISTS permissions;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS roles; 