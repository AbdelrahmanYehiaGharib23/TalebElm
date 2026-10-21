# Feature Specification: Health Check & Error Handling

## 1. MVP Scope & Objective

Expose a lightweight health endpoint so callers can confirm the API process responds. A global exception middleware is planned, but exception-to-HTTP mapping is not active yet.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

- No entities belong to this feature.

### Relationships

- None.

### Domain Exception Hierarchy

This feature _consumes_ the domain exception hierarchy rather than defining entities:

```
DomainException (base)
├── NotFoundException
├── ValidationException
└── ConflictException
└── NotImplementedException
```

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

- No DTOs. The health endpoint returns a plain string `"healthy"`.

### Service Interface

- No service interface. The health check is handled directly by the controller.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route         | Request DTO | Response DTO         | Status Codes |
| ----------- | ------------- | ----------- | -------------------- | ------------ |
| `GET`       | `/api/health` | —           | `"healthy"` (string) | `200 OK`     |

### Health check scope

- The current `/api/health` endpoint checks only that the API process can receive a request and return a response.
- It does not currently verify database connectivity, cache availability, message brokers, SMTP providers, object storage, or any third-party identity/auth service.
- That narrow scope matches the current codebase, which has no dependency-specific health probes registered.
- If TalebElm adds infrastructure readiness checks later, start with the primary database and any configured auth/token provider, then expose richer dependency status separately from this lightweight liveness endpoint.

### Exception Handling Middleware

The intended `ExceptionHandlingMiddleware` mapping is:

| Exception Type                     | HTTP Status | Response Body                                                                    |
| ---------------------------------- | ----------- | -------------------------------------------------------------------------------- |
| `NotFoundException`                | `404`       | `{ "error": "<message>" }`                                                       |
| `ValidationException`              | `400`       | `{ "error": "Validation failed.", "errors": { "<field>": ["<message>", ...] } }` |
| `ConflictException`                | `409`       | `{ "error": "A conflicting state or duplicate resource was detected."}`          |
| `DomainException`                  | `400`       | `{ "error": "<message>" }`                                                       |
| `NotImplementedException` (Domain) | `501`       | `{ "error": "<message>" }`                                                       |
| Unhandled `Exception`              | `500`       | `{ "error": "An unexpected error occurred." }`                                   |

### Validation error payload (target contract)

Validation failures must return field-specific errors so clients can attach each message to its input field:

```json
{
  "error": "Validation failed.",
  "errors": {
    "name": ["'Name' must not be empty."],
    "capacity": ["'Capacity' must be greater than '0'."]
  }
}
```

- Keys in `errors` are the failing property names in camelCase; each value is an array of messages, because one field can fail more than one rule.
- The top-level `error` string stays present for logging and generic display; failures that are not tied to a specific field are reported there only.
- Services currently throw FluentValidation's `ValidationException` (for example `TrackService.CreateAsync`), whose `ValidationFailure` items already carry `PropertyName` and `ErrorMessage`. The middleware must map those into the `errors` object above. `TalebElm.Domain.Exceptions.ValidationException` carries a single message and maps to `error` only, until it is extended to carry per-field errors.

### Registration (planned)

```csharp
// In Program.cs, before app.MapControllers():
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Input Validation

- No validation required for the health endpoint.

## 5. Domain Exceptions

| Exception                 | When Thrown                                                                                             |
| ------------------------- | ------------------------------------------------------------------------------------------------------- |
| `NotFoundException`       | User lookup by ID returns null                                                                          |
| `ConflictException`       | Duplicate entity, unique constraint violation, or conflicting state (e.g. duplicate enrollment or exam) |
| `ValidationException`     | Duplicate email or missing required fields (planned)                                                    |
| `NotImplementedException` | All current service methods and controller actions (placeholder)                                        |

This feature's middleware _handles_ all domain exceptions thrown by other features. See the mapping table above.
