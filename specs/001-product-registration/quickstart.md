# Product Registration - Quickstart Guide

**Feature**: Product Registration (001)  
**Version**: 1.0.0  
**Date**: 2026-01-08

## Prerequisites

- .NET 10 SDK installed ([download here](https://dot.net))
- Code editor (VS Code, Visual Studio, Rider, or similar)
- Terminal/Command Prompt
- REST client (curl, Postman, or VS Code REST Client extension)

Verify .NET installation:
```bash
dotnet --version
# Should output: 10.x.x
```

## Project Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd ecommerce-system-specdriven
git checkout 001-product-registration
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build Project

```bash
dotnet build
```

### 4. Run Tests

Verify setup by running all tests:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Expected output: All tests pass with ≥80% coverage on domain logic.

### 5. Run API

```bash
cd src/API
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

API available at: `http://localhost:5000`

## API Usage Examples

### Create Product

**Request**:
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Mouse",
    "price": 29.99,
    "quantity": 50
  }'
```

**Response** (201 Created):
```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "name": "Wireless Mouse",
  "price": 29.99,
  "quantity": 50,
  "createdAt": "2026-01-08T14:30:00Z",
  "updatedAt": "2026-01-08T14:30:00Z",
  "version": 1
}
```

**Save the `id` from response for next steps!**

---

### Get Product

Replace `{id}` with actual product ID from create response:

```bash
curl http://localhost:5000/api/products/a1b2c3d4-5678-90ab-cdef-1234567890ab
```

**Response** (200 OK):
```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "name": "Wireless Mouse",
  "price": 29.99,
  "quantity": 50,
  "createdAt": "2026-01-08T14:30:00Z",
  "updatedAt": "2026-01-08T14:30:00Z",
  "version": 1
}
```

---

### Update Product

**Request**:
```bash
curl -X PUT http://localhost:5000/api/products/a1b2c3d4-5678-90ab-cdef-1234567890ab \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ergonomic Wireless Mouse",
    "price": 39.99,
    "quantity": 45,
    "version": 1
  }'
```

**Response** (200 OK):
```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "name": "Ergonomic Wireless Mouse",
  "price": 39.99,
  "quantity": 45,
  "createdAt": "2026-01-08T14:30:00Z",
  "updatedAt": "2026-01-08T14:32:00Z",
  "version": 2
}
```

**Note**: Version incremented from 1 → 2

---

### Delete Product

```bash
curl -X DELETE http://localhost:5000/api/products/a1b2c3d4-5678-90ab-cdef-1234567890ab
```

**Response**: 204 No Content (empty body)

Verify deletion:
```bash
curl http://localhost:5000/api/products/a1b2c3d4-5678-90ab-cdef-1234567890ab
```

**Response** (404 Not Found):
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Product with ID a1b2c3d4-5678-90ab-cdef-1234567890ab was not found"
}
```

## Validation Examples

### Empty Name

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "price": 29.99,
    "quantity": 50
  }'
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "name": ["Product name is required and cannot be empty"]
  }
}
```

---

### Negative Price

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Mouse",
    "price": -10.00,
    "quantity": 50
  }'
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "price": ["Price must be a positive value greater than zero"]
  }
}
```

---

### Negative Quantity

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Mouse",
    "price": 29.99,
    "quantity": -5
  }'
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "quantity": ["Quantity must be a non-negative integer"]
  }
}
```

---

### Name Too Long

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"$(printf 'A%.0s' {1..201})\",
    \"price\": 29.99,
    \"quantity\": 50
  }"
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "name": ["Product name cannot exceed 200 characters"]
  }
}
```

## Concurrency Scenario

Simulate concurrent updates to test optimistic locking:

**Step 1**: Create product
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Product", "price": 10.00, "quantity": 100}'
# Save the ID and note version: 1
```

**Step 2**: User A updates (version 1 → 2)
```bash
curl -X PUT http://localhost:5000/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated by A", "price": 15.00, "quantity": 100, "version": 1}'
# Response version: 2
```

**Step 3**: User B tries to update with stale version
```bash
curl -X PUT http://localhost:5000/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated by B", "price": 20.00, "quantity": 100, "version": 1}'
```

**Response** (409 Conflict):
```json
{
  "status": 409,
  "title": "Concurrency Conflict",
  "detail": "Product was modified by another user. Expected version 1, but current version is 2"
}
```

**Step 4**: User B re-fetches and retries with correct version
```bash
# Get latest version
curl http://localhost:5000/api/products/{id}
# Note version: 2

# Retry update with current version
curl -X PUT http://localhost:5000/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated by B", "price": 20.00, "quantity": 100, "version": 2}'
# Success! Version now: 3
```

## Development Workflow

### TDD Cycle (Red-Green-Refactor)

1. **RED**: Write failing test
   ```bash
   dotnet test
   # Test fails (expected)
   ```

2. **GREEN**: Write minimal code to pass
   ```bash
   # Implement feature
   dotnet test
   # Test passes
   ```

3. **REFACTOR**: Improve code quality
   ```bash
   # Clean up implementation
   dotnet test
   # Tests still pass
   ```

### Running Specific Tests

```bash
# Domain layer tests only
dotnet test tests/Domain.Tests/

# Application layer tests only
dotnet test tests/Application.Tests/

# Infrastructure layer tests only
dotnet test tests/Infrastructure.Tests/

# API layer tests only
dotnet test tests/API.Tests/

# Run specific test class
dotnet test --filter FullyQualifiedName~ProductTests

# Run specific test method
dotnet test --filter FullyQualifiedName~ProductTests.Create_WithValidData_ShouldSucceed
```

### Code Coverage Report

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Install ReportGenerator tool (first time only)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
open coveragereport/index.html  # macOS
# OR
start coveragereport/index.html  # Windows
# OR
xdg-open coveragereport/index.html  # Linux
```

## Troubleshooting

### Port Already in Use

Error: `Address already in use`

**Solution**: Kill process on port 5000 or change port
```bash
# macOS/Linux
lsof -ti:5000 | xargs kill -9

# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# OR change port in Program.cs
builder.WebApplication.CreateBuilder(args)
    .WebHost.UseUrls("http://localhost:5001");
```

### Tests Failing

**Check**:
1. All dependencies restored: `dotnet restore`
2. Clean build: `dotnet clean && dotnet build`
3. Test isolation: Run tests individually to identify conflicts
4. In-memory state: Restart API between test runs

### Validation Not Working

**Verify**:
- Request has `Content-Type: application/json` header
- JSON syntax is valid (use JSON validator)
- Field names match exactly (case-sensitive)
- Data types match contract (string, decimal, integer)

## Next Steps

1. ✅ Verify API works with all CRUD operations
2. ✅ Run complete test suite (≥80% coverage)
3. ✅ Test concurrency control (version conflicts)
4. ✅ Test all validation scenarios
5. 📋 Review [data-model.md](data-model.md) for domain design
6. 📋 Review [contracts/product-api.md](contracts/product-api.md) for full API spec
7. 🚀 Ready to implement additional features!

## Project Structure Quick Reference

```
ecommerce-system-specdriven/
├── src/
│   ├── Domain/              # Core business logic
│   │   ├── Products/
│   │   │   ├── Product.cs   # Aggregate root
│   │   │   ├── ProductName.cs   # Value object
│   │   │   ├── Money.cs     # Value object
│   │   │   └── StockQuantity.cs # Value object
│   │   └── Common/
│   │       └── AggregateRoot.cs # Base class
│   ├── Application/         # Use cases & DTOs
│   │   ├── Products/
│   │   │   ├── IProductRepository.cs
│   │   │   ├── ProductApplicationService.cs
│   │   │   ├── Commands/    # CreateProduct, UpdateProduct
│   │   │   └── DTOs/        # ProductDto
│   ├── Infrastructure/      # Data persistence
│   │   └── Persistence/
│   │       └── InMemoryProductRepository.cs
│   └── API/                 # HTTP endpoints
│       ├── Program.cs       # Minimal API setup
│       └── Endpoints/
│           └── ProductEndpoints.cs
└── tests/
    ├── Domain.Tests/
    ├── Application.Tests/
    ├── Infrastructure.Tests/
    └── API.Tests/
```

## Support

For issues or questions:
1. Check [spec.md](spec.md) for requirements
2. Review [plan.md](plan.md) for technical decisions
3. Consult [.specify/memory/constitution.md](../../.specify/memory/constitution.md) for standards
4. Run tests to verify behavior

---

**Happy Coding! 🚀**
