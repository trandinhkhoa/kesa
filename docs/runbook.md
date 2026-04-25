# Prototype Runbook

## Purpose
This runbook describes how to set up, run, test, and smoke-verify the HR management prototype backend.

## Prerequisites
- .NET SDK 10.0.x
- Docker Desktop (running)
- PostgreSQL 16+ (for local app runtime) or containerized PostgreSQL

## Local setup
1. Restore dependencies:
   - `dotnet restore`
2. Configure database connection in `appsettings.json`:
   - `ConnectionStrings:DefaultConnection`
3. Apply migrations to the configured database:
   - `dotnet ef database update`

## Run the API
- Start the API:
  - `dotnet run`
- Default route base:
  - `api/v1`
- OpenAPI endpoint in development:
  - `/openapi/v1.json`

## Core API endpoints
### Admin profile field definitions
- `GET /api/v1/admin/profile-fields`
- `GET /api/v1/admin/profile-fields/{id}`
- `POST /api/v1/admin/profile-fields`
- `PUT /api/v1/admin/profile-fields/{id}`
- `DELETE /api/v1/admin/profile-fields/{id}`

Example create payload:
```json
{
  "name": "Nationality",
  "key": "nationality",
  "dataType": "String",
  "isRequired": true,
  "isActive": true
}
```

### Candidate profiles
- `GET /api/v1/candidates?pageNumber=1&pageSize=20`
- `GET /api/v1/candidates/{id}`
- `POST /api/v1/candidates`
- `PUT /api/v1/candidates/{id}`
- `DELETE /api/v1/candidates/{id}`

Example create payload:
```json
{
  "name": "Sample Candidate",
  "birthDate": "1996-03-12",
  "sex": "Female",
  "customFields": {
    "nationality": "Vietnamese"
  }
}
```

## Test commands
- Run all tests:
  - `dotnet test Kesa.Tests/Kesa.Tests.csproj --logger "console;verbosity=minimal"`
- Build verification:
  - `dotnet build`

Notes:
- Integration tests require Docker because they use PostgreSQL testcontainers.
- Test database state is reset between tests through shared fixture utilities.

## Smoke test workflow
1. Start API (`dotnet run`).
2. Create a profile field definition (`POST /api/v1/admin/profile-fields`).
3. Create candidate with matching required `customFields` (`POST /api/v1/candidates`).
4. Get candidate (`GET /api/v1/candidates/{id}`).
5. Update candidate (`PUT /api/v1/candidates/{id}`).
6. List candidates (`GET /api/v1/candidates`).
7. Delete candidate (`DELETE /api/v1/candidates/{id}`).
8. Verify not found after delete (`GET /api/v1/candidates/{id}` returns `404`).

## Known limitations and deferred scope
- Authentication is deferred.
- Authorization / role enforcement is deferred.
- No production hardening for secrets, observability, or deployment automation.
- No advanced filtering/sorting beyond baseline pagination.
- No external ATS integrations or broader HR modules (out of scope).
