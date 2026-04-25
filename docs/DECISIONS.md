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
