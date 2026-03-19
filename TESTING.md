# RansomGuard - Testing Guide

Last Updated: 2026-03-19

## Table of Contents

1. [Testing Philosophy](#testing-philosophy)
2. [Test Infrastructure](#test-infrastructure)
3. [Integration Test Database Setup](#integration-test-database-setup)
4. [Production Database Safety](#production-database-safety)
5. [Running Tests](#running-tests)
6. [Troubleshooting](#troubleshooting)

---

## Testing Philosophy

RansomGuard follows test isolation best practices:

- **Unit Tests**: Test individual components in isolation with mocking
- **Integration Tests**: Test full HTTP pipeline with real dependencies
- **Production Safety**: Tests never touch production database (ransomguard.db)
- **Performance**: Fast test execution with in-memory databases
- **Reliability**: Repeatable tests with no shared state

---

## Test Infrastructure

### Custom Test Infrastructure

Location: /backend/RansomGuard.API.Tests/CustomWebApplicationFactory.cs

This factory creates an isolated test environment by:

1. Replacing production DbContext with in-memory SQLite
2. Applying migrations automatically before tests run
3. Providing fresh database for each test class
4. Cleaning up resources after tests complete

Key Technologies:

- xUnit as test framework
- IClassFixture for shared test context per class
- IAsyncLifetime for async setup and teardown
- In-Memory SQLite using DataSource=:memory:

---

## Integration Test Database Setup

### The Problem (Fixed on 2026-03-19)

Integration tests were failing because EF Core migrations existed but weren't applied before tests ran. Tests used default WebApplicationFactory with no database initialization, causing them to try saving data to a database with no schema.

### The Solution

Created CustomWebApplicationFactory that:

1. **Replaces production database**: Removes production DbContext and adds in-memory test database using DataSource=:memory:

2. **Applies migrations automatically**: The InitializeAsync method creates database schema before tests run

3. **Integrates with test classes**: Test classes use IClassFixture pattern to get isolated test environment

### Why In-Memory Database?

| Feature   | In-Memory SQLite           | File-Based SQLite                 |
| --------- | -------------------------- | --------------------------------- |
| Speed     | Extremely fast (RAM)       | Slower (disk I/O)                 |
| Isolation | Zero risk to production    | Could accidentally use wrong file |
| Cleanup   | Auto-deleted after tests   | Manual cleanup required           |
| CI/CD     | No file permissions issues | Potential permissions problems    |

---

## Production Database Safety

### Tests Never Touch Production Database

Technical Guarantees:

1. **Connection String Replacement**
    - Production: Data Source=ransomguard.db (physical file)
    - Tests: DataSource=:memory: (RAM only)

2. **Service Container Isolation**
   The factory removes production DbContext descriptor and adds test-specific DbContext. No code path can access production connection string during tests.

3. **Environment Separation**
   Tests run in "Testing" environment, not "Development" or "Production".

---

## Running Tests

### All Tests

Command: dotnet test
Expected: 32 tests pass (31 unit + 2 integration)

### Unit Tests Only

Command: dotnet test --filter "FullyQualifiedName~RansomGuard.API.Tests.Unit"

### Integration Tests Only

Command: dotnet test --filter "FullyQualifiedName~RansomGuard.API.Tests.Integration"

### Verbose Output

Command: dotnet test --logger "console;verbosity=detailed"

### With Coverage

Command: dotnet test /p:CollectCoverage=true

---

## Troubleshooting

### Error: "no such table: AnalysisResults"

Cause: Migrations not applied to test database.

Solution: Ensure you're using CustomWebApplicationFactory, not WebApplicationFactory<Program>

### Error: "Failed to initialize test database"

Cause: Migration error during initialization.

Debug Steps:

1. Check if migrations exist in backend/RansomGuard.API/Migrations/
2. Verify migration syntax with: dotnet ef migrations list
3. Enable detailed logging in CustomWebApplicationFactory

### Tests pass locally but fail in CI/CD

Common Causes:

- File permissions on database file (shouldn't happen with in-memory)
- Missing EF Core Design tools

Solution: Add package Microsoft.EntityFrameworkCore.Design

### Error: "Database connection is in use"

Cause: Shared connection not properly disposed.

Solution: Verify CustomWebApplicationFactory.DisposeAsync() properly closes and disposes the connection.

---

Maintained by: RansomGuard Development Team
Questions? Check TODO.md for development status
