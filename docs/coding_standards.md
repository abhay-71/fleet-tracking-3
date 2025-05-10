# Fleet Tracking Application - Coding Standards

This document outlines the coding standards for the Fleet Tracking Application project to ensure consistency and maintainability across the codebase.

## General Guidelines

1. **Code Readability**: Write code that is clear and easy to understand. Prioritize readability over cleverness.
2. **Comments**: Add comments to explain complex logic, but avoid obvious comments that don't add value.
3. **Documentation**: Include documentation for public APIs, functions, and types.
4. **Error Handling**: Always handle errors appropriately; never ignore them.
5. **Testing**: Write tests for all new functionality.

## Go (API) Coding Standards

### Naming Conventions

1. **Package Names**: Use short, lowercase, single-word names without underscores.
2. **Variable Names**: Use camelCase for local variables and PascalCase for exported variables.
3. **Function Names**: Use camelCase for unexported functions and PascalCase for exported functions.
4. **Interface Names**: Use PascalCase, often ending with the suffix "er" (e.g., `Reader`, `Writer`).
5. **Constant Names**: Use PascalCase for exported constants and camelCase for unexported constants.

### Code Organization

1. **Package Structure**: Organize code into logical packages by functionality, not by type.
2. **File Organization**: Group related functionality in a single file, but keep files of manageable size (< 500 lines).
3. **Import Ordering**: Group imports into standard library, third-party packages, and local packages, with a blank line between each group.

### Formatting

1. **Gofmt**: All Go code must be formatted using `gofmt` or `go fmt`.
2. **Import Aliasing**: Avoid aliasing imports unless necessary to prevent name conflicts.
3. **Line Length**: Keep lines to a reasonable length (< 100 characters).

### Best Practices

1. **Error Handling**: Return errors instead of using panics. Use the `errors` package or `fmt.Errorf` to create descriptive errors.
2. **Concurrency**: Use goroutines and channels appropriately. Avoid sharing memory; prefer communication.
3. **Context**: Use context for managing cancellation, deadlines, and request-scoped values.
4. **API Design**: Follow RESTful API design principles. Use appropriate HTTP methods and status codes.
5. **Logging**: Use structured logging to make logs more searchable and analyzable.

## SQL (PostgreSQL) Coding Standards

### Naming Conventions

1. **Table Names**: Use snake_case, plural names (e.g., `users`, `vehicles`).
2. **Column Names**: Use snake_case (e.g., `first_name`, `created_at`).
3. **Primary Keys**: Name primary key columns `id` unless there's a good reason not to.
4. **Foreign Keys**: Name foreign key columns with the singular form of the referenced table followed by `_id` (e.g., `user_id`, `vehicle_id`).
5. **Indexes**: Prefix index names with `idx_` followed by the table name and columns (e.g., `idx_users_email`).

### Formatting

1. **Keywords**: Use UPPERCASE for SQL keywords (e.g., `SELECT`, `INSERT`, `UPDATE`).
2. **Indentation**: Use consistent indentation for complex queries.
3. **Aliasing**: Use meaningful table aliases in joins.

### Best Practices

1. **Transactions**: Use transactions for operations that need to be atomic.
2. **Indexes**: Create appropriate indexes for frequently queried columns.
3. **Constraints**: Use constraints to enforce data integrity (e.g., NOT NULL, UNIQUE, FOREIGN KEY).
4. **Prepared Statements**: Use prepared statements to prevent SQL injection.
5. **Migrations**: Keep database schema changes in versioned migration files.

## C# (ASP.NET MVC) Coding Standards

### Naming Conventions

1. **Class Names**: Use PascalCase for class names.
2. **Method Names**: Use PascalCase for methods.
3. **Variable Names**: Use camelCase for local variables and parameters.
4. **Interface Names**: Prefix with "I" (e.g., `IVehicleService`).
5. **Constants**: Use PascalCase for constants.
6. **Private Fields**: Prefix with underscore and use camelCase (e.g., `_connectionString`).

### Code Organization

1. **Namespace Structure**: Organize namespaces by feature, not by type.
2. **File Organization**: One class per file, with the filename matching the class name.
3. **MVC Structure**: Follow the MVC pattern, keeping controllers, models, and views in their respective directories.

### Formatting

1. **Braces**: Always use braces for control structures, even for single-line bodies.
2. **Indentation**: Use 4 spaces for indentation.
3. **Line Length**: Keep lines to a reasonable length (< 100 characters).

### Best Practices

1. **Exception Handling**: Use try-catch blocks appropriately. Avoid catching general exceptions.
2. **Dependency Injection**: Use dependency injection instead of static methods and singletons.
3. **Async/Await**: Use async/await for asynchronous operations.
4. **Validation**: Validate model data using Data Annotations or a validation library.
5. **Security**: Follow security best practices to prevent common vulnerabilities (XSS, CSRF, etc.).

## Version Control (Git) Standards

1. **Commit Messages**: Write clear, concise commit messages in the imperative mood.
2. **Branching Strategy**: Follow GitFlow or a similar branching strategy.
3. **Pull Requests**: Keep pull requests focused on a single task or feature.
4. **Rebasing**: Rebase feature branches on the main branch before merging.
5. **Code Reviews**: All code changes must be reviewed before merging.

## Documentation Standards

1. **README**: Maintain an up-to-date README file with project overview, setup instructions, and other essential information.
2. **API Documentation**: Document all API endpoints with request/response formats.
3. **Code Comments**: Add inline comments for complex logic and document public APIs.
4. **Architecture Documentation**: Keep architecture documentation up-to-date with any significant changes.

By following these coding standards, we ensure a consistent, maintainable, and high-quality codebase for the Fleet Tracking Application. 