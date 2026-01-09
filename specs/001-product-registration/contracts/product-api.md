# Product API Contract

**Version**: 1.0.0  
**Base URL**: `/api/products`  
**Date**: 2026-01-08

## Endpoints

### Create Product

**Endpoint**: `POST /api/products`

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "string (required, max 200 characters)",
  "price": "decimal (required, > 0)",
  "quantity": "integer (required, >= 0)"
}
```

**Example Request**:
```json
{
  "name": "Laptop",
  "price": 999.99,
  "quantity": 10
}
```

**Success Response**: `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Laptop",
  "price": 999.99,
  "quantity": 10,
  "createdAt": "2026-01-08T10:30:00Z",
  "updatedAt": "2026-01-08T10:30:00Z",
  "version": 1
}
```

**Response Headers**:
```
Location: /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json
```

**Error Responses**:

`400 Bad Request` - Validation failure
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "name": ["Product name is required and cannot be empty"],
    "price": ["Price must be a positive value greater than zero"],
    "quantity": ["Quantity must be a non-negative integer"]
  }
}
```

---

### Update Product

**Endpoint**: `PUT /api/products/{id}`

**Path Parameters**:
- `id` (Guid, required): The unique identifier of the product to update

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "string (required, max 200 characters)",
  "price": "decimal (required, > 0)",
  "quantity": "integer (required, >= 0)",
  "version": "integer (required, for optimistic concurrency)"
}
```

**Example Request**:
```json
{
  "name": "Gaming Laptop",
  "price": 1299.99,
  "quantity": 5,
  "version": 1
}
```

**Success Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Gaming Laptop",
  "price": 1299.99,
  "quantity": 5,
  "createdAt": "2026-01-08T10:30:00Z",
  "updatedAt": "2026-01-08T10:35:00Z",
  "version": 2
}
```

**Error Responses**:

`400 Bad Request` - Validation failure
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "name": ["Product name cannot exceed 200 characters"]
  }
}
```

`404 Not Found` - Product doesn't exist
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Product with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 was not found"
}
```

`409 Conflict` - Version mismatch
```json
{
  "status": 409,
  "title": "Concurrency Conflict",
  "detail": "Product was modified by another user. Expected version 1, but current version is 2"
}
```

---

### Get Product by ID

**Endpoint**: `GET /api/products/{id}`

**Path Parameters**:
- `id` (Guid, required): The unique identifier of the product to retrieve

**Success Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Laptop",
  "price": 999.99,
  "quantity": 10,
  "createdAt": "2026-01-08T10:30:00Z",
  "updatedAt": "2026-01-08T10:30:00Z",
  "version": 1
}
```

**Error Responses**:

`404 Not Found` - Product doesn't exist
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Product with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 was not found"
}
```

---

### Delete Product

**Endpoint**: `DELETE /api/products/{id}`

**Path Parameters**:
- `id` (Guid, required): The unique identifier of the product to delete

**Success Response**: `204 No Content`
- Empty response body
- Product successfully deleted or didn't exist (idempotent)

**Note**: This endpoint is idempotent. Deleting a non-existent product returns 204, not 404.

---

## Common Error Response Format

All error responses follow RFC 7807 Problem Details format:

```json
{
  "status": 400,
  "title": "Brief error title",
  "detail": "Detailed error message (optional)",
  "errors": {
    "fieldName": ["Error message for this field"]
  }
}
```

## HTTP Status Code Summary

| Status Code | Usage |
|------------|-------|
| 200 OK | Successful GET or PUT operation |
| 201 Created | Successful POST operation |
| 204 No Content | Successful DELETE operation |
| 400 Bad Request | Validation failure (ProductValidationException) |
| 404 Not Found | Product not found (ProductNotFoundException) |
| 409 Conflict | Version mismatch (ProductConcurrencyException) |
| 500 Internal Server Error | Unhandled exception (details hidden from client) |

## Concurrency Control

The API uses optimistic concurrency control via the `version` field:

1. Client retrieves product (GET) → receives version (e.g., version: 1)
2. Client modifies data locally
3. Client sends update (PUT) with current version in request body
4. Server validates version matches current stored version
5. If match: Update succeeds, version incremented (version: 2)
6. If mismatch: 409 Conflict returned, client must retry with latest data

**Best Practice**: 
- Always include current version when updating
- Handle 409 Conflict by re-fetching latest data and re-applying user changes
- Display clear message to user: "Another user modified this product. Please review and try again."

## Validation Rules

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| name | string | Yes | Non-empty, non-whitespace, max 200 characters |
| price | decimal | Yes | Greater than zero |
| quantity | integer | Yes | Greater than or equal to zero |
| version | integer | Yes (PUT only) | Positive integer matching stored version |

## Content Negotiation

- Request: `Content-Type: application/json`
- Response: `Content-Type: application/json`
- Only JSON format supported

## Security Considerations

- No authentication/authorization in this phase (per spec assumptions)
- Error responses hide internal implementation details
- 500 errors return generic message, log full stack trace server-side
- Input validation prevents injection attacks via value objects

## Rate Limiting

Not implemented in this phase. Future consideration for production deployment.

## Versioning

API version embedded in URL path: `/api/products`

Future versions could use: `/api/v2/products` or header-based versioning.
