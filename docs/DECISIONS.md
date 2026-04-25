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
