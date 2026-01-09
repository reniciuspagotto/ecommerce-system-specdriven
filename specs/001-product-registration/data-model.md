# Data Model: Product Registration

**Feature**: Product Registration  
**Date**: 2026-01-08  
**Purpose**: Define domain entities, value objects, and relationships

## Aggregate Root

### Product

**Purpose**: Represents a product in the catalog inventory system. Product is the aggregate root that maintains consistency boundary for all product-related operations.

**Inherits From**: `AggregateRoot` base class

**Base Class Properties** (from AggregateRoot per constitution):
- `Id` (Guid): Unique identifier for the product
- `CreatedAt` (DateTime): Timestamp when the product was created
- `UpdatedAt` (DateTime): Timestamp when the product was last modified
- `Version` (int): Version number for optimistic concurrency control

**Product-Specific Properties**:
- `Name` (ProductName value object): Product name with validation
- `Price` (Money value object): Product price with validation
- `Quantity` (StockQuantity value object): Available inventory quantity with validation

**Factory Method**:
```csharp
public static Product Create(ProductName name, Money price, StockQuantity quantity)
```
- Creates new Product instance
- Validates all invariants
- Assigns new Guid for Id
- Sets CreatedAt and UpdatedAt to DateTime.UtcNow
- Initializes Version to 1
- Throws ProductValidationException if any invariant violated

**Mutating Methods**:
```csharp
public void UpdateDetails(ProductName name, Money price, StockQuantity quantity)
```
- Updates Name, Price, and Quantity
- Validates all invariants
- Increments Version (e.g., 1 → 2, 2 → 3)
- Updates UpdatedAt to DateTime.UtcNow
- Throws ProductValidationException if any invariant violated

**Invariants Enforced**:
1. Name must be valid ProductName (non-empty, ≤200 characters)
2. Price must be valid Money (Amount > 0)
3. Quantity must be valid StockQuantity (Value ≥ 0)
4. Version must be positive integer
5. UpdatedAt must be ≥ CreatedAt

**Domain Events** (optional for future enhancement):
- ProductCreated
- ProductUpdated
- ProductDeleted

## Value Objects

### ProductName

**Purpose**: Encapsulates product name with validation rules

**Properties**:
- `Value` (string): The product name

**Validation Rules**:
- Cannot be null
- Cannot be empty string
- Cannot be whitespace-only
- Maximum length: 200 characters
- Special characters: Unicode allowed, control characters rejected, no sanitization

**Throws**: ProductValidationException if validation fails

**Immutability**: Once created, Value cannot be changed (value object semantics)

**Equality**: Two ProductName instances equal if Value matches

---

### Money

**Purpose**: Encapsulates monetary amount with validation rules

**Properties**:
- `Amount` (decimal): The monetary value

**Validation Rules**:
- Must be greater than zero (Amount > 0)
- Cannot be negative
- Cannot be exactly zero

**Throws**: ProductValidationException if validation fails

**Immutability**: Once created, Amount cannot be changed

**Equality**: Two Money instances equal if Amount matches

**Note**: Currency is not specified; price is currency-agnostic per assumptions in spec.md

---

### StockQuantity

**Purpose**: Encapsulates inventory quantity with validation rules

**Properties**:
- `Value` (int): The stock quantity

**Validation Rules**:
- Must be non-negative (Value ≥ 0)
- Zero is valid (out of stock)
- Cannot be negative

**Throws**: ProductValidationException if validation fails

**Immutability**: Once created, Value cannot be changed

**Equality**: Two StockQuantity instances equal if Value matches

## Domain Exceptions

### ProductValidationException

**Purpose**: Thrown when product data violates business rules

**Inherits From**: Exception

**Properties**:
- `Message` (string): Clear, actionable error message indicating which field failed validation
- `Errors` (optional Dictionary<string, string>): Field-specific error messages

**Example Messages**:
- "Product name is required and cannot be empty"
- "Product name cannot exceed 200 characters"
- "Price must be a positive value greater than zero"
- "Quantity must be a non-negative integer"

---

### ProductNotFoundException

**Purpose**: Thrown when attempting to retrieve or operate on a non-existent product

**Inherits From**: Exception

**Properties**:
- `ProductId` (Guid): The ID of the product that was not found
- `Message` (string): "Product with ID {ProductId} was not found"

---

### ProductConcurrencyException

**Purpose**: Thrown when update operation detects version mismatch (optimistic concurrency conflict)

**Inherits From**: Exception

**Properties**:
- `ProductId` (Guid): The ID of the product with version conflict
- `ExpectedVersion` (int): The version provided in the update request
- `ActualVersion` (int): The current version stored in the system
- `Message` (string): "Product was modified by another user. Expected version {ExpectedVersion}, but current version is {ActualVersion}"

## Repository Interface

### IProductRepository

**Purpose**: Defines contract for product persistence operations

**Methods**:

```csharp
void Add(Product product)
```
- Adds new product to storage
- Assigns Guid if not already set
- Sets CreatedAt and UpdatedAt to DateTime.UtcNow
- Sets Version to 1

```csharp
void Update(Product product)
```
- Updates existing product in storage
- Validates Version matches current stored version
- Increments Version by 1
- Updates UpdatedAt to DateTime.UtcNow
- Throws ProductConcurrencyException if version mismatch
- Throws ProductNotFoundException if product doesn't exist

```csharp
void Delete(Guid id)
```
- Removes product from storage
- Idempotent: No error if product doesn't exist

```csharp
Product? GetById(Guid id)
```
- Retrieves product by unique identifier
- Returns Product if found, null otherwise
- Does not throw exception for not found

**Note**: Repository interface resides in Domain layer, implementation in Infrastructure layer (dependency inversion principle)

## Application DTOs

### ProductDto

**Purpose**: Data transfer object for API responses

**Properties**:
- `Id` (Guid)
- `Name` (string)
- `Price` (decimal)
- `Quantity` (int)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)
- `Version` (int)

**Mapping**: ProductApplicationService maps Product aggregate → ProductDto

## Relationships and Boundaries

**Product Aggregate Boundary**:
- Product is the only aggregate root
- ProductName, Money, StockQuantity are value objects within the aggregate
- No child entities at this stage (simple CRUD)

**Consistency Boundary**:
- All modifications to Product go through Product.UpdateDetails()
- Version field ensures optimistic concurrency across aggregate modifications
- Repository persists entire aggregate atomically

**Future Considerations**:
- If product categories added: Category might be separate aggregate
- If product variants added: Consider Product as parent, Variant as child entity or separate aggregate
- For now: Single aggregate, single bounded context

## State Transitions

```
[New Product Data] 
    ↓
Product.Create(name, price, quantity)
    ↓
[Product Created: Version 1]
    ↓
UpdateDetails(new name, price, quantity)
    ↓
[Product Updated: Version 2]
    ↓
UpdateDetails(...) [concurrent update with stale version]
    ↓
[ProductConcurrencyException thrown]
    
[Product Retrieved]
    ↓
Delete(id)
    ↓
[Product Deleted: No longer retrievable]
```

## Validation Summary

| Entity/Value Object | Validation Rules | Exception |
|-------------------|------------------|-----------|
| ProductName | Non-null, non-empty, non-whitespace, ≤200 chars | ProductValidationException |
| Money | Amount > 0 | ProductValidationException |
| StockQuantity | Value ≥ 0 | ProductValidationException |
| Product.Create() | All value objects valid | ProductValidationException |
| Product.UpdateDetails() | All value objects valid | ProductValidationException |
| IProductRepository.Update() | Version matches | ProductConcurrencyException |
| IProductRepository.Update() | Product exists | ProductNotFoundException |
