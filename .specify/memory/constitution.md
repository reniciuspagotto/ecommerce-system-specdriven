<!--
SYNC IMPACT REPORT

Version Change: 1.1.0 → 1.2.0
Rationale: MINOR version bump - Added Project Structure Standards section with hexagonal architecture and naming conventions

Modified Principles:
- No principle changes (existing principles remain unchanged)

Added Sections:
- Project Structure Standards (new section with hexagonal architecture layers and naming conventions)

Removed Sections:
- None

Templates Status:
✅ plan-template.md - Project Structure section will guide layer organization decisions
✅ spec-template.md - No changes required (entity naming aligns with standards)
✅ tasks-template.md - File path conventions align with layer organization
✅ agent-file-template.md - No changes required
✅ checklist-template.md - No changes required

Follow-up TODOs:
- None
-->

# E-Commerce System Constitution

## Core Principles

### I. Specification-Driven Development

Every feature MUST begin with a complete specification before implementation. The specification process follows this mandatory sequence:

- Feature description → Specification document → Implementation plan → Task breakdown → Implementation
- All specifications MUST be stored in `/specs/[###-feature-name]/` with versioned artifacts
- Specifications MUST include: user stories (prioritized), functional requirements, success criteria, and acceptance scenarios
- All design decisions MUST be documented in research.md, data-model.md, or contracts/ before implementation begins

**Rationale**: Prevents scope creep, ensures shared understanding, and provides traceable decision history.

### II. Independent User Story Testing

User stories MUST be designed, implemented, and tested as independently deliverable value increments:

- Each user story MUST have assigned priority (P1, P2, P3, etc.) indicating delivery order
- Each user story MUST be testable in isolation with clear acceptance scenarios
- User Story 1 (P1) MUST represent the Minimum Viable Product (MVP)
- Implementation MUST allow each story to be deployed independently without breaking previously delivered stories
- Task organization MUST group by user story (Phase per story) enabling parallel development

**Rationale**: Enables incremental delivery, parallel team development, and early user validation.

### III. Test-First Development (NON-NEGOTIABLE)

Testing MUST precede implementation following strict Test-Driven Development (TDD) discipline:

- Tests MUST be written first, reviewed, and confirmed to FAIL before implementation begins
- Red-Green-Refactor cycle MUST be strictly enforced for all production code
- All public methods in domain models MUST have corresponding unit tests
- Unit tests MUST use xUnit testing framework
- Contract tests MUST be written for all API endpoints and service boundaries
- Integration tests MUST verify user journeys and cross-component interactions

**Rationale**: Ensures specification compliance, prevents regression, and validates design decisions before implementation investment.

### IV. Code Coverage Standards

Test coverage MUST meet minimum thresholds to ensure code quality and reliability:

- Domain logic MUST achieve minimum 80% code coverage
- Coverage reports MUST be generated and reviewed before merging code
- Untested code paths MUST be explicitly justified in PR descriptions
- Test-driven development approach MUST be applied where appropriate
- Coverage metrics MUST be tracked and visible in CI/CD pipeline

**Rationale**: Ensures critical business logic is thoroughly tested while maintaining development velocity.

## Testing Standards

All testing practices MUST adhere to the following requirements:

**Unit Testing**:
- Framework: xUnit for all unit testing
- All public methods in domain models MUST have corresponding unit tests
- Unit tests MUST be isolated, fast, and deterministic

**Code Coverage**:
- Minimum 80% code coverage required for domain logic
- Coverage reports MUST be generated and tracked in CI/CD
- Gaps in coverage MUST be documented and justified

**Test-Driven Development**:
- TDD approach MUST be applied where appropriate
- Tests MUST be written before implementation
- All tests MUST fail initially, then pass after implementation

**Integration & Contract Testing**:
- Contract tests MUST verify API boundaries and service interfaces
- Integration tests MUST validate end-to-end user journeys
- Tests MUST be organized by user story for independent validation

## Domain Model Standards

All domain modeling MUST adhere to Domain-Driven Design (DDD) principles with the following architectural requirements:

**Aggregate Root Architecture**:
- All aggregate roots MUST inherit from an abstract `AggregateRoot` base class
- The `AggregateRoot` base class MUST provide:
  - `Id` property of type `Guid` for unique identification
  - `CreatedAt` property of type `DateTime` for creation timestamp
  - `UpdatedAt` property of type `DateTime` for last modification timestamp
  - `Version` property of type `int` for optimistic concurrency control
- Aggregate roots MUST encapsulate consistency boundaries
- Only aggregate roots MAY be directly referenced by entities outside the aggregate

**Business Logic Encapsulation**:
- Domain entities MUST contain business logic and validation within the model
- Business rules MUST NOT leak into application or infrastructure layers
- Domain models MUST be persistence-ignorant (no database/ORM attributes in domain layer)
- All state changes MUST occur through well-named, intention-revealing methods

**Invariant Validation**:
- All domain operations MUST validate invariants before state changes
- Invariants MUST be enforced in domain entity constructors and mutating methods
- Invalid state transitions MUST throw domain-specific exceptions
- Validation failures MUST provide clear, actionable error messages
- Domain entities MUST NOT allow creation or modification into invalid states

**Entity Design Guidelines**:
- Entities MUST be identified by their unique identity (`Id`), not by attributes
- Value objects MUST be immutable and identified by their attributes
- Domain events SHOULD be raised for significant state changes
- Rich domain models MUST be preferred over anemic domain models

**Rationale**: Ensures domain integrity, prevents invalid states, and maintains a clean separation between business logic and technical concerns.

## Project Structure Standards

All features MUST follow hexagonal architecture with clear layer separation:

**Layer Organization**:
- Domain Layer: Aggregates, value objects, domain exceptions, repository interfaces
- Application Layer: Application services, commands, queries, DTOs
- Infrastructure Layer: Repository implementations, external service adapters
- API Layer: Endpoints, middleware, request/response models

**Naming Conventions**:
- Aggregate files named after entity (Product.cs, Order.cs, Customer.cs)
- Repository interfaces prefixed with I (IProductRepository)
- Application services suffixed with ApplicationService
- Value objects in dedicated namespace/folder
- Domain exceptions suffixed with Exception

**Rationale**: Ensures consistent structure across Product, Order, and Customer domains for team navigation and maintenance.

## Development Workflow

All development activities MUST follow this workflow sequence:

**Phase 0 - Planning**:
- Run `/speckit.specify` to create feature specification
- Specification MUST include prioritized user stories with acceptance criteria
- All unknowns MUST be marked as "NEEDS CLARIFICATION" for research phase

**Phase 1 - Research & Design**:
- Run `/speckit.plan` to generate implementation plan and research artifacts
- All "NEEDS CLARIFICATION" items MUST be resolved in research.md
- Data models, contracts, and quickstart guides MUST be generated
- Constitution Check gate MUST pass before proceeding

**Phase 2 - Task Breakdown**:
- Run `/speckit.tasks` to generate actionable task list
- Tasks MUST be organized by user story (one phase per story)
- Dependencies MUST be explicitly documented
- Parallel execution opportunities MUST be marked with [P] flag

**Phase 3+ - Implementation**:
- Run `/speckit.implement` to execute tasks
- Tests MUST be written and approved BEFORE implementation
- Each user story MUST be validated independently before moving to next
- Code reviews MUST verify constitution compliance

**Quality Gates**:
- Specification completeness (all sections filled, no placeholders)
- Constitution Check passed (no unjustified violations)
- Tests written first and initially failing
- Code coverage meets 80% threshold for domain logic
- Independent user story validation successful

## Governance

**Constitutional Authority**:
- This constitution supersedes all other development practices and guidelines
- All pull requests and code reviews MUST verify compliance with constitutional principles
- Violations MUST be documented and justified in the Complexity Tracking section of plan.md
- Unjustified complexity or constitutional violations MUST block implementation

**Amendment Process**:
- Amendments MUST be proposed with clear rationale and impact analysis
- Version increments MUST follow semantic versioning:
  - MAJOR: Backward-incompatible governance changes, principle removals/redefinitions
  - MINOR: New principles added, material expansions to guidance
  - PATCH: Clarifications, wording improvements, non-semantic refinements
- All amendments MUST include Sync Impact Report documenting affected templates and artifacts
- Consistency propagation MUST update all dependent templates and documentation

**Compliance Review**:
- Constitution compliance MUST be verified at specification, planning, and implementation phases
- Template updates MUST be validated against constitution after any amendments
- Agent-specific references (e.g., "CLAUDE") MUST be replaced with generic guidance when applicable
- Runtime development guidance files MUST reflect current constitutional principles

**Versioning & Change Management**:
- Constitution MUST maintain version number, ratification date, and last amended date
- Sync Impact Report MUST be prepended as HTML comment after each update
- Follow-up action items MUST be tracked until completion
- Suggested commit messages MUST follow format: `docs: amend constitution to vX.Y.Z (description)`

**Agent Context Updates**:
- After Phase 1 design artifacts are generated, agent context MUST be updated
- Run `.specify/scripts/bash/update-agent-context.sh [agent-name]` to update agent-specific files
- Only new technologies from current plan MUST be added
- Manual additions between markers MUST be preserved

**Version**: 1.2.0 | **Ratified**: 2026-01-07 | **Last Amended**: 2026-01-08
