# Phase 1 Retrospective

## Context
Phase 1 focused on foundation and data schema work: baseline project setup, domain entities, EF Core PostgreSQL mappings, initial migration/seed data, and early test infrastructure.

## Difficulties encountered and resolutions

### 1) .NET 10 + PostgreSQL provider compatibility
- **Issue:** Stable `Npgsql.EntityFrameworkCore.PostgreSQL` for EF Core 10 was not available.
- **Impact:** `dotnet restore` failed, blocking DbContext/migration work.
- **Resolution:** Aligned to `.NET 10 + EF Core 9.0.4 + Npgsql 9.0.4` (stable).
- **Why this worked:** Npgsql provider major version matched EF Core major version.

### 2) EF migration tooling was missing
- **Issue:** `dotnet ef` command was unavailable in the environment.
- **Impact:** Could not generate initial migration.
- **Resolution:** Added local tool manifest and installed local `dotnet-ef` `9.0.4`.
- **Why this worked:** Tool version matched EF dependency line and became reproducible in-repo.

### 3) Test project source leaked into app compilation
- **Issue:** `Kesa.Tests` lived inside app root, so app project picked up test files.
- **Impact:** Main build failed on xUnit symbols (e.g., `Fact`) from test files.
- **Resolution:** Excluded `Kesa.Tests/**` compile/resource items from `Kesa.csproj`.
- **Why this worked:** Restored project boundary between app and test code.

### 4) Test package and API mismatches
- **Issue:** Version conflicts and test API usage mismatch (`SkipException` constructor).
- **Impact:** `dotnet test` failed.
- **Resolution:** Pinned `Microsoft.EntityFrameworkCore.Relational` to `9.0.4` in test project and replaced skip path with assertion-based failure messaging; also updated Testcontainers builder usage.
- **Why this worked:** Dependency graph became consistent and tests compiled/executed correctly.

### 5) Migration drift after model refinements
- **Issue:** Model updates (JSONB operator/index and seeded options) diverged from existing migration.
- **Impact:** Snapshot/migration could become out of sync.
- **Resolution:** Removed and regenerated the initial migration.
- **Why this worked:** Migration artifacts now exactly reflect the final Phase 1 model.

## Outcomes
- Phase 1 tasks (`1.1`–`1.5`) completed.
- Initial schema and seed data in place.
- Testcontainers-based PostgreSQL test infrastructure is running.

## Follow-up guardrails for next phases
- Keep EF/Npgsql major versions aligned.
- Regenerate migrations only when model changes are finalized for a task.
- Keep test project references isolated from app compilation scope.
