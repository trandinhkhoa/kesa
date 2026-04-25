## Baseline API stack and layering — 2026-04-25
**Context:** While creating `docs/specs.md` from `project_description.md`, the specification required a concrete tech stack and architecture section.
**Options considered:** (1) C# with ASP.NET Core and layered architecture, (2) Node.js with Express, (3) Java with Spring Boot.
**Decision:** Use C# with ASP.NET Core Web API and Controller -> Service -> Repository layering.
**Reason:** This aligns with established project rules and ensures business logic remains in services while controllers stay request/response focused.

## Prototype scope prioritization — 2026-04-25
**Context:** While creating `docs/planning.md`, the implementation scope needed to be narrowed for a fast, working prototype.
**Options considered:** (1) Implement full scope including auth/authz now, (2) Build CRUD-only prototype first and defer auth/authz, (3) Build only admin field-definition CRUD first.
**Decision:** Build candidate profile CRUD and admin field-definition CRUD first, and explicitly defer authentication/authorization.
**Reason:** This delivers the core product behavior fastest, reduces early complexity, and keeps iteration speed high for validating dynamic profile-field workflows.

## Candidate storage strategy (core columns + JSONB) — 2026-04-25
**Context:** The candidate profile model needed to support both high-performance querying for always-present fields and flexibility for admin-defined fields.
**Options considered:** (1) Fully normalized EAV model, (2) JSONB-only profile storage, (3) Hybrid model with core columns and JSONB for dynamic fields.
**Decision:** Use PostgreSQL with a hybrid schema: `Name`, `BirthDate`, and `Sex` as dedicated columns, and `CustomFields` as JSONB for all admin-defined fields. Keep `ProfileFieldDefinition` to validate dynamic keys and data types.
**Reason:** This keeps essential queries efficient while preserving runtime flexibility for field changes. It also simplifies schema evolution and keeps validation centralized in service logic.

## Age calculation policy — 2026-04-25
**Context:** The model discussion included whether to persist `Age` as a column when `BirthDate` exists.
**Options considered:** (1) Persist `Age` and synchronize updates, (2) Derive `Age` from `BirthDate` at runtime.
**Decision:** Do not persist `Age`; compute it from `BirthDate` in read/query flows.
**Reason:** Prevents stale data and eliminates synchronization complexity.

## Project reset strategy for API bootstrap — 2026-04-25
**Context:** Before implementing Phase 1 task 1.1, the existing project type and starter code needed to be selected.
**Options considered:** (1) Convert current project in place, (2) split into domain and API projects, (3) reset to a fresh ASP.NET Web API template.
**Decision:** Reset to a fresh ASP.NET Web API template and overwrite app code while preserving documentation files.
**Reason:** The user did not need to preserve current app code, and a clean template gives the fastest stable baseline for prototype delivery.

## Error response contract baseline — 2026-04-25
**Context:** Task 1.1 required defining a standard API error response format.
**Options considered:** (1) ProblemDetails only, (2) custom error envelope only, (3) hybrid ProblemDetails with `errorCode` extension.
**Decision:** Use hybrid error responses: ProblemDetails plus `errorCode` extension.
**Reason:** This preserves standards compatibility and gives explicit business/application error coding.

## API versioning strategy — 2026-04-25
**Context:** Task 1.1 required selecting an API versioning convention for controller routes.
**Options considered:** (1) URL versioning (`/api/v1/...`), (2) header versioning, (3) no versioning.
**Decision:** Use URL versioning with `/api/v1/...`.
**Reason:** It is explicit, easy to test, and simple for a prototype baseline.

## .NET 10 data stack compatibility — 2026-04-25
**Context:** During Phase 1 setup, stable Npgsql EF provider versions were not available for EF Core 10, while the project needed to stay on .NET 10.
**Options considered:** (1) .NET 10 + EF Core 9 + stable Npgsql 9, (2) .NET 10 + EF Core 10 + preview Npgsql provider, (3) switch away from EF Core.
**Decision:** Keep .NET 10 and use EF Core 9 with stable Npgsql 9.
**Reason:** This preserves the .NET target while avoiding preview dependency risk and keeping EF-based workflow.

## Candidate sex field modeling — 2026-04-25
**Context:** Phase 1 data model required selecting how to represent the core `Sex` field.
**Options considered:** (1) text value with service-layer validation, (2) numeric enum storage, (3) separate lookup table.
**Decision:** Store `Sex` as text and validate values in service logic.
**Reason:** This keeps the prototype schema simple and easy to evolve while following service-layer validation rules.

## Baseline dynamic field seed set — 2026-04-25
**Context:** Phase 1 migration seed data needed default admin-defined fields for dynamic profile attributes.
**Options considered:** (1) no defaults, (2) seed requested baseline fields, (3) config-driven seed source.
**Decision:** Seed `address`, `religion` (`buddism`, `christian`, `others`), and `marriage` (`no`, `married`, `divoced`, `widowed`).
**Reason:** This matches explicit user requirements while keeping startup data minimal.

## Phase 1 test infrastructure stack — 2026-04-25
**Context:** Task 1.5 required selecting an early integration-test foundation.
**Options considered:** (1) xUnit + Testcontainers PostgreSQL, (2) NUnit + Testcontainers PostgreSQL, (3) MSTest + Testcontainers PostgreSQL.
**Decision:** Use xUnit with Testcontainers PostgreSQL.
**Reason:** It provides straightforward .NET integration-test setup and aligns with project testing guidance.

## Phase 2 repository and data-access contracts — 2026-04-25
**Context:** Implementing Phase 2 required finalizing repository interface return shapes, candidate list pagination behavior, and delete semantics before coding data-access logic.
**Options considered:** (1) entity-first repository contracts with nullable/bool not-found signaling + offset pagination (`pageNumber`, `pageSize`, `totalCount`) + hard delete, (2) result-wrapper repository contracts + limit/offset without total count + hard delete, (3) split read/write repository models + cursor pagination + soft delete.
**Decision:** Use entity-first repository contracts returning domain entities with nullable/bool for missing rows; use offset pagination returning `(items, totalCount)` for candidate lists; use hard delete in repositories for the prototype.
**Reason:** This provides the closest fit to the current prototype scope, keeps repository complexity low, aligns with familiar JPA-style CRUD patterns, and avoids schema changes that are unnecessary for Phase 2.

## Phase 3 service contracts and validation policies — 2026-04-25
**Context:** Before implementing service DTOs, validation rules, and error mapping for candidate and profile-field workflows, service-layer contract and validation policies had to be locked.
**Options considered:** (1) unified `ServiceResult<T>` + `Dictionary<string, JsonElement>` custom field DTO + reject unknown/inactive custom-field keys + restrict `Sex` to `Male/Female/Other`, (2) exception-driven service errors + raw JSON string custom field DTO + ignore unknown keys + accept any non-empty `Sex`, (3) per-operation service result shapes + `JsonDocument` DTO + permissive unknown keys + dynamic `Sex` validation.
**Decision:** Use unified `ServiceResult<T>` / `ServiceResult` with structured error payloads; use `Dictionary<string, JsonElement>` for candidate custom fields in DTOs; reject unknown and inactive custom-field keys; validate `Sex` as one of `Male`, `Female`, `Other` (case-insensitive).
**Reason:** This keeps service behavior explicit and testable, enforces dynamic-field integrity as required by specs, and provides consistent error mapping for upcoming controller/API layers.

## Phase 4 controller and API contract strategy — 2026-04-25
**Context:** Before implementing API/controller endpoints and OpenAPI documentation, route patterns, API DTO boundaries, status mapping, and pagination defaults needed to be finalized.
**Options considered:** (1) explicit admin route (`/api/v1/admin/profile-fields`) + separate API DTOs from service DTOs + standard status mapping (`400/404/409/500`) + optional pagination defaults with max clamp, (2) base-route-only controller naming + shared DTOs + simplified status mapping + required pagination params, (3) additional admin base class + mixed DTO strategy + non-standard status envelope + unbounded optional pagination.
**Decision:** Use explicit admin route while keeping existing base controller; use separate API DTOs and map to/from service DTOs; map service errors to standard HTTP status codes (`VALIDATION_ERROR->400`, `NOT_FOUND->404`, `CONFLICT->409`, `UNEXPECTED_ERROR->500`); support optional candidate pagination with defaults (`pageNumber=1`, `pageSize=20`) and clamp page size to 100.
**Reason:** This keeps controllers thin and maintainable, preserves clean API boundaries for long-term evolution, and improves client usability while maintaining safe defaults.
