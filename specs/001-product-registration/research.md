# Research: Product Registration

**Feature**: Product Registration  
**Date**: 2026-01-08  
**Purpose**: Document technology decisions and best practices research

## Technology Stack Decisions

### Decision: .NET 10 with Minimal API
**Rationale**: 
- Modern, performant framework suitable for REST APIs
- Minimal API reduces boilerplate compared to traditional controllers
- Built-in dependency injection and middleware pipeline
- Cross-platform runtime support

**Alternatives Considered**:
- ASP.NET Core with Controllers: More verbose, unnecessary complexity for CRUD operations
- Node.js/Express: Team expertise in .NET, type safety benefits of C#
- Python/FastAPI: .NET ecosystem better suited for enterprise e-commerce requirements

### Decision: In-Memory Storage (ConcurrentDictionary)
**Rationale**:
- Meets current requirement for in-memory only (FR-009 per spec)
- ConcurrentDictionary provides thread-safe operations for concurrent access
- Built-in .NET type, no external dependencies
- Simple migration path to persistent storage later (repository pattern abstraction)

**Alternatives Considered**:
- Dictionary with manual locking: More error-prone, ConcurrentDictionary is standard
- In-memory database (SQLite :memory:): Over-engineered for current scope
- Static list: Not thread-safe for concurrent operations

### Decision: Optimistic Concurrency Control via Version Field
**Rationale**:
- Prevents conflicting simultaneous updates per FR-007
- Version field in AggregateRoot base class per constitution
- Update operations validate version matches, increment on success
- ProductConcurrencyException thrown on version mismatch
- Lightweight, no locking required

**Alternatives Considered**:
- Pessimistic locking: Reduces concurrency, not needed for in-memory scenario
- Last-write-wins: Violates FR-007 requirement to prevent conflicts
- Timestamp-based: Version counter simpler and more explicit

## Best Practices Research

### Domain-Driven Design (DDD) Patterns

**Value Objects for Invariant Validation**:
- ProductName enforces non-empty, max 200 characters
- Money enforces Amount > 0
- StockQuantity enforces Value >= 0
- Benefits: Compile-time type safety, reusable validation, expressive domain model
- Source: Domain-Driven Design principles (Eric Evans, Vaughn Vernon)

**Aggregate Root Pattern**:
- Product is the aggregate root for Product bounded context
- Encapsulates consistency boundary
- All modifications through Product methods (Create, UpdateDetails)
- Benefits: Enforces invariants, clear transactional boundaries
- Source: DDD patterns for aggregate design

**Repository Pattern**:
- IProductRepository interface in Domain layer
- Implementation in Infrastructure layer
- Benefits: Persistence ignorance, testability, abstraction over storage
- Source: Repository pattern (Martin Fowler, Patterns of Enterprise Application Architecture)

### Hexagonal Architecture (Ports & Adapters)

**Layer Separation**:
- Domain: Pure business logic, no external dependencies
- Application: Use case orchestration, commands, DTOs
- Infrastructure: External concerns (storage, I/O)
- API: Entry points, HTTP concerns

**Dependency Rule**:
- Dependencies point inward: API → Application → Domain
- Domain has zero dependencies on outer layers
- Benefits: Testable domain logic, swappable infrastructure

**Source**: Hexagonal Architecture (Alistair Cockburn), Clean Architecture (Robert C. Martin)

### Testing Strategies

**TDD Approach**:
1. Write failing test
2. Implement minimum code to pass
3. Refactor for quality
- Benefits: Design validation, regression prevention, living documentation

**Test Pyramid**:
- Many unit tests (domain logic, value objects)
- Fewer integration tests (repository, application service)
- Few end-to-end tests (API endpoints)
- Benefits: Fast feedback, isolated failures, maintainability

**FluentAssertions Library**:
- Readable assertions: `product.Name.Should().Be("Expected Name")`
- Better error messages than basic Assert
- Source: FluentAssertions documentation

### API Design Best Practices

**RESTful Conventions**:
- POST /api/products → 201 Created with Location header
- GET /api/products/{id} → 200 OK or 404 Not Found
- PUT /api/products/{id} → 200 OK, 404, 409 Conflict for version mismatch
- DELETE /api/products/{id} → 204 No Content or 404

**HTTP Status Codes**:
- 400 Bad Request: Validation failures (ProductValidationException)
- 404 Not Found: Product doesn't exist (ProductNotFoundException)
- 409 Conflict: Version mismatch (ProductConcurrencyException)
- 500 Internal Server Error: Unexpected errors (hide internal details)

**Global Exception Handler**:
- Centralized exception-to-HTTP mapping
- Consistent error response format
- Security: Don't leak internal implementation details
- Source: ASP.NET Core middleware patterns

## Integration Patterns

### Dependency Injection
- Register IProductRepository → InMemoryProductRepository
- Register ProductApplicationService as scoped
- ASP.NET Core built-in DI container sufficient for current needs

### Middleware Pipeline
- Global exception handler runs early in pipeline
- Maps domain exceptions to appropriate HTTP responses
- Logs exceptions for diagnostics

## Performance Considerations

**ConcurrentDictionary Operations**:
- GetOrAdd, TryUpdate for atomic operations
- Avoid ToList() or enumerations during concurrent modifications
- Expected performance: O(1) lookups, ~microseconds for in-memory access

**Target**: <100ms retrieval per success criteria SC-004 - easily achievable with in-memory storage

## Migration Path (Future Considerations)

**To Persistent Storage**:
1. Implement new repository: EfCoreProductRepository or DapperProductRepository
2. Replace registration in DI container
3. Domain and Application layers unchanged (persistence ignorance)
4. Infrastructure layer absorbs all migration impact

**To Distributed System**:
1. Replace optimistic concurrency with distributed lock (e.g., Redis)
2. Consider event sourcing for audit trail
3. Repository pattern enables these changes with minimal ripple effect

## Unresolved Questions

None - All requirements from spec.md are clear and addressable with chosen technology stack.
