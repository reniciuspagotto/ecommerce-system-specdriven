# Implementation Plan: Product Registration

**Branch**: `001-product-registration` | **Date**: 2026-01-08 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-product-registration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement Product Registration feature enabling catalog managers to perform CRUD operations (create, update, delete, retrieve) on products in the inventory system. The implementation follows hexagonal architecture with .NET 10 minimal API, domain-driven design with aggregate roots, value objects for invariant validation, and optimistic concurrency control to prevent conflicting simultaneous updates.

## Technical Context

**Language/Version**: .NET 10 (C#)  
**Primary Dependencies**: ASP.NET Core Minimal API, FluentAssertions (testing), xUnit (unit testing framework)  
**Storage**: In-memory (ConcurrentDictionary for thread-safe access)  
**Testing**: xUnit for unit/integration tests, FluentAssertions for test readability  
**Target Platform**: Cross-platform (.NET 10 runtime)
**Project Type**: Web API (single project with hexagonal architecture layers)  
**Performance Goals**: <100ms for product retrieval operations (in-memory), handle concurrent updates safely  
**Constraints**: Thread-safe concurrent access required, optimistic concurrency control mandatory  
**Scale/Scope**: In-memory storage suitable for development/prototype phase, production persistence deferred

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Principle I: Specification-Driven Development
- ✅ Complete specification exists at `/specs/001-product-registration/spec.md`
- ✅ User stories prioritized (P1-P4) with acceptance scenarios
- ✅ Functional requirements defined (FR-001 to FR-010)
- ✅ Success criteria measurable and technology-agnostic

### ✅ Principle II: Independent User Story Testing
- ✅ User Story 1 (P1): Create Product - MVP functionality, independently testable
- ✅ User Story 2 (P2): Update Product - builds on create, independently testable
- ✅ User Story 3 (P3): Retrieve Product - independently testable
- ✅ User Story 4 (P4): Delete Product - independently testable
- ✅ Each story deliverable independently without breaking others

### ✅ Principle III: Test-First Development (NON-NEGOTIABLE)
- ✅ TDD approach planned: tests written before implementation
- ✅ xUnit framework selected per constitution
- ✅ All public methods in Product aggregate will have unit tests
- ✅ Contract tests planned for repository interfaces
- ✅ Integration tests planned for API endpoints and user journeys

### ✅ Principle IV: Code Coverage Standards
- ✅ Target: 80% code coverage for Product domain logic
- ✅ FluentAssertions for readable test assertions
- ✅ Coverage tracking planned in CI/CD pipeline

### ✅ Testing Standards
- ✅ Unit Testing: xUnit framework, isolated and deterministic tests
- ✅ Code Coverage: 80% minimum for domain logic
- ✅ TDD: Tests fail first, then implementation makes them pass
- ✅ Integration & Contract Testing: API boundaries and end-to-end user journeys

### ✅ Domain Model Standards
- ✅ Product aggregate inherits from AggregateRoot base class
- ✅ AggregateRoot provides: Id (Guid), CreatedAt (DateTime), UpdatedAt (DateTime), Version (int)
- ✅ Value objects enforce invariants: ProductName, Money, StockQuantity
- ✅ Business logic encapsulated in domain models
- ✅ Invariant validation in constructors and mutating methods
- ✅ Domain exceptions: ProductValidationException, ProductNotFoundException, ProductConcurrencyException
- ✅ Factory method: Product.Create() for creation
- ✅ Mutating method: UpdateDetails() increments Version for optimistic concurrency

### ✅ Project Structure Standards
- ✅ Hexagonal architecture with clear layer separation:
  - Domain Layer: Product aggregate, value objects, domain exceptions, IProductRepository interface
  - Application Layer: ProductApplicationService, commands (CreateProductCommand, UpdateProductCommand), ProductDto
  - Infrastructure Layer: InMemoryProductRepository implementation
  - API Layer: Minimal API endpoints, global exception handler middleware
- ✅ Naming conventions followed:
  - Aggregate: Product.cs
  - Repository interface: IProductRepository
  - Application service: ProductApplicationService
  - Value objects in dedicated folder
  - Domain exceptions suffixed with Exception

**Constitution Check Result**: ✅ PASSED - No violations detected. All constitutional requirements satisfied.

## Project Structure

### Documentation (this feature)

```text
specs/001-product-registration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Aggregates/
│   │   ├── AggregateRoot.cs               # Base class with Id, CreatedAt, UpdatedAt, Version
│   │   └── Product.cs                      # Product aggregate root
│   ├── ValueObjects/
│   │   ├── ProductName.cs                  # Validates non-empty, max 200 chars
│   │   ├── Money.cs                        # Validates Amount > 0
│   │   └── StockQuantity.cs                # Validates Value >= 0
│   ├── Exceptions/
│   │   ├── ProductValidationException.cs
│   │   ├── ProductNotFoundException.cs
│   │   └── ProductConcurrencyException.cs
│   └── Repositories/
│       └── IProductRepository.cs           # Repository interface (domain boundary)
├── Application/
│   ├── Services/
│   │   └── ProductApplicationService.cs    # Orchestrates product operations
│   ├── Commands/
│   │   ├── CreateProductCommand.cs         # (string Name, decimal Price, int Quantity)
│   │   └── UpdateProductCommand.cs         # (Guid Id, string Name, decimal Price, int Quantity, int Version)
│   ├── DTOs/
│   │   └── ProductDto.cs                   # (Guid Id, string Name, decimal Price, int Quantity, DateTime CreatedAt, DateTime UpdatedAt, int Version)
│   ├── Mappings/
│   │   └── ProductMapper.cs                # Maps Product entity to ProductDto
│   └── Repositories/
│       └── IProductRepository.cs           # Repository interface (moved from Domain in original design)
├── Infrastructure/
│   └── Repositories/
│       └── InMemoryProductRepository.cs    # ConcurrentDictionary<Guid, Product> implementation
└── API/
    ├── Endpoints/
    │   └── ProductEndpoints.cs             # Minimal API endpoint definitions
    ├── Requests/
    │   ├── CreateProductRequest.cs
    │   └── UpdateProductRequest.cs
    └── Middleware/
        └── GlobalExceptionHandler.cs       # Maps domain exceptions to HTTP status codes

tests/
├── Domain.Tests/
│   ├── ProductTests.cs                     # Product aggregate tests
│   └── ValueObjects/
│       ├── ProductNameTests.cs
│       ├── MoneyTests.cs
│       └── StockQuantityTests.cs
├── Application.Tests/
│   └── ProductApplicationServiceTests.cs   # Application service tests
├── Infrastructure.Tests/
│   └── InMemoryProductRepositoryTests.cs   # Repository implementation tests
└── API.Tests/
    ├── Endpoints/
    │   └── ProductEndpointsTests.cs        # End-to-end API tests
    ├── Integration/
    │   └── [Integration test files]
    └── Performance/
        └── ConcurrentOperationsTests.cs    # Performance tests
```

**Structure Decision**: Selected single project with hexagonal architecture layers following constitutional requirements. The .NET solution organizes code by architectural layer (Domain, Application, Infrastructure, API) rather than by feature, ensuring clear separation of concerns. The Domain layer contains no dependencies on outer layers, maintaining persistence ignorance and enabling independent testing of business logic.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitutional violations detected. All complexity is justified by business requirements:
- Value objects enforce domain invariants at compile time
- ConcurrentDictionary provides thread-safe access for concurrent operations
- Optimistic concurrency (Version field) prevents data corruption per FR-007
- Hexagonal architecture maintains testability and adherence to domain-driven design principles
