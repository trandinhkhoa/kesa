# Implementation Plan (Prototype First)

## Scope and prototype goal
- Build a working backend prototype that supports:
  - Candidate profile CRUD operations.
  - Admin profile-field definition CRUD operations.
- Explicitly ignore authentication and authorization for this prototype.

## Part 1: High-level phases / milestones

- [ ] **P1 — Foundation and data schema**
  **Depends on:** none
  Prepare project structure, database schema, migrations, and local dev/test infrastructure.

- [ ] **P2 — Data access layer**
  **Depends on:** P1
  Implement repositories for candidate profiles (core columns + `JSONB` custom fields) and field definitions.

- [ ] **P3 — Service layer business logic**
  **Depends on:** P2
  Implement validation and CRUD workflows in services, including dynamic field constraints.

- [ ] **P4 — API/controllers and contracts**
  **Depends on:** P3
  Expose REST endpoints for candidate CRUD and admin field-definition CRUD.

- [ ] **P5 — Testing, hardening, and prototype readiness**
  **Depends on:** P4
  Add automated tests (including integration tests with PostgreSQL testcontainers), fix defects, and ensure prototype run instructions are complete.

## Part 2: Detailed task breakdown

### Phase 1 — Foundation and data schema

- [ ] **1.1 Create prototype baseline and conventions**
  **Depends on:** none
  Confirm project folders and naming for Models/Repositories/Services/Controllers; define error response shape and API version prefix for consistency.

- [ ] **1.2 Define domain entities and relationships**
  **Depends on:** 1.1
  Model `CandidateProfile` with core columns (`Name`, `BirthDate`, `Sex`) plus `CustomFields` (`JSONB`), and `ProfileFieldDefinition` for dynamic field rules.

- [ ] **1.3 Add database context and mappings**
  **Depends on:** 1.2
  Configure EF Core mappings (keys, required fields, field-key uniqueness), JSONB mapping for `CustomFields`, B-tree indexes for core fields, and GIN index for `CustomFields`.

- [ ] **1.4 Create initial migrations and seed baseline fields**
  **Depends on:** 1.3
  Generate initial migration and optional seed data for minimum candidate fields needed to create profiles.

- [ ] **1.5 Prepare test infrastructure early**
  **Depends on:** 1.4
  Set up test project structure and PostgreSQL testcontainers wiring so integration tests can be added quickly in later phases.

### Phase 2 — Data access layer

- [ ] **2.1 Implement field-definition repository interfaces**
  **Depends on:** 1.5
  Add repository contracts for creating, listing, updating, and deleting profile field definitions.

- [ ] **2.2 Implement candidate profile repository interfaces**
  **Depends on:** 1.5
  Add repository contracts for candidate create/read/update/delete and list with pagination hooks.

- [ ] **2.3 Implement repository classes with EF Core**
  **Depends on:** 2.1, 2.2
  Implement concrete repositories with transactional write behavior for profile core columns and `CustomFields` JSONB payloads.

- [ ] **2.4 Add repository-level query safeguards**
  **Depends on:** 2.3
  Add defensive checks for missing records and safe handling of soft/hard delete strategy chosen for prototype.

### Phase 3 — Service layer business logic

- [ ] **3.1 Define service DTOs and service contracts**
  **Depends on:** 2.4
  Create request/response DTOs for candidate and field-definition operations, keeping controllers thin.

- [ ] **3.2 Implement field-definition service rules**
  **Depends on:** 3.1
  Implement add/edit/remove logic with constraints (unique field key, supported data types, required/active semantics).

- [ ] **3.3 Implement candidate create/update validation logic**
  **Depends on:** 3.2
  Validate candidate payloads by enforcing core required columns and validating `customFields` against active `ProfileFieldDefinition` rules (required fields, data types, enum options).

- [ ] **3.4 Implement candidate read/list/delete service flows**
  **Depends on:** 3.3
  Implement retrieval and delete behavior, including age derivation from `BirthDate`, not-found handling, and validation error handling.

- [ ] **3.5 Add service-layer logging and error mapping**
  **Depends on:** 3.4
  Add structured logging and map domain errors to consistent service result types.

### Phase 4 — API/controllers and contracts

- [ ] **4.1 Implement admin field-definition controllers**
  **Depends on:** 3.5
  Implement `GET/POST/PUT/DELETE /api/admin/profile-fields` endpoints delegating all business rules to services.

- [ ] **4.2 Implement candidate profile controllers**
  **Depends on:** 3.5
  Implement `POST/GET/PUT/DELETE /api/candidates` and list endpoint with pagination parameters.

- [ ] **4.3 Add request validation and API error responses**
  **Depends on:** 4.1, 4.2
  Add model validation, status code mapping, and uniform error envelope.

- [ ] **4.4 Add OpenAPI documentation for prototype endpoints**
  **Depends on:** 4.3
  Document request/response schemas and sample payloads for candidate and admin flows.

### Phase 5 — Testing, hardening, and prototype readiness

- [ ] **5.1 Add model and repository integration tests**
  **Depends on:** 4.4
  Add integration tests using PostgreSQL testcontainers for persistence correctness and relational behavior.

- [ ] **5.2 Add service tests for dynamic-field validation rules**
  **Depends on:** 5.1
  Cover required core columns, invalid dynamic data types, enum validation, unknown field keys, and age derivation edge cases.

- [ ] **5.3 Add controller integration tests for CRUD endpoints**
  **Depends on:** 5.2
  Verify status codes, response contracts, and end-to-end candidate + admin workflows.

- [ ] **5.4 Perform prototype smoke tests and bug fixes**
  **Depends on:** 5.3
  Run full local workflow from field-definition setup to candidate CRUD and resolve blocking defects.

- [ ] **5.5 Finalize prototype runbook and known limitations**
  **Depends on:** 5.4
  Document setup, migrations, test commands, API usage, and explicitly list deferred items (auth/authz and other out-of-scope work).
