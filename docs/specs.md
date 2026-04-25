# HR Management Web Application — Specs

## Project overview
This project is a web-based human resource management application for internal company users to manage candidate profiles. Employees can create and maintain candidate records, while administrators can configure which input fields appear on candidate profiles so the organization can adapt profile structure without code changes.

## Core features
- Employee authentication and access to candidate management functions.
- Create candidate profiles with core attributes (`Name`, `BirthDate`, `Sex`) and admin-defined dynamic fields stored in `JSONB`.
- View and update candidate profile data.
- Admin management of profile field definitions (add, edit, remove fields).
- Validation of candidate profile input based on active field definitions.
- Audit-friendly metadata on configuration and profile changes (actor/time fields).

## User flow
1. User signs in to the system.
2. If user is an employee, they navigate to candidate profile creation.
3. System loads current active profile field definitions.
4. Employee enters candidate information and submits.
5. System validates required/typed fields and stores the profile.
6. Employee can revisit and update an existing candidate profile.
7. If user is an admin, they open field-definition management.
8. Admin adds/edits/removes profile input fields.
9. Updated field definitions apply to subsequent candidate profile create/update operations.

## Tech stack
- Language: C#
- Framework: ASP.NET Core Web API
- Architecture: Controller -> Service -> Repository
- Database: PostgreSQL
- API style: REST (JSON)
- Testing: .NET test framework with integration tests using Testcontainers + PostgreSQL
- Hosting: TBD (see open questions)

## Out of scope
- Resume parsing or OCR ingestion.
- Candidate ranking/scoring or AI matching.
- Job requisition/posting management.
- Offer management and onboarding workflow.
- Payroll, attendance, or broader HR modules.
- External ATS integrations.

## Data models
- `User`
  - `Id`, `Email`, `PasswordHash`, `Role` (Employee/Admin), `CreatedAt`, `UpdatedAt`
- `ProfileFieldDefinition`
  - `Id`, `Name`, `Key`, `DataType` (String/Number/Date/Boolean/Enum), `IsRequired`, `IsActive`, `OptionsJson` (for enum), `CreatedBy`, `CreatedAt`, `UpdatedAt`
  - Purpose: defines validation rules for dynamic fields saved in `CandidateProfile.CustomFields`.
- `CandidateProfile`
  - `Id`, `Name`, `BirthDate`, `Sex`, `CustomFields` (`JSONB`), `CreatedByUserId`, `UpdatedByUserId`, `CreatedAt`, `UpdatedAt`
  - `Age` is not persisted; it is derived from `BirthDate`.

### Relationships
- One `User` creates many `CandidateProfile` records.
- One `ProfileFieldDefinition` validates keys stored inside `CandidateProfile.CustomFields`.

## API contracts

### Auth
- `POST /api/auth/login`
  - Request: `{ email, password }`
  - Response: `{ accessToken, user: { id, email, role } }`

### Candidate profiles
- `POST /api/candidates`
  - Request: `{ name, birthDate, sex, customFields: { [fieldKey]: value } }`
  - Response: `{ id, name, birthDate, age, sex, customFields, createdAt }`
- `GET /api/candidates/{id}`
  - Response: `{ id, name, birthDate, age, sex, customFields }`
- `PUT /api/candidates/{id}`
  - Request: `{ name, birthDate, sex, customFields: { [fieldKey]: value } }`
  - Response: `{ id, name, birthDate, age, sex, customFields, updatedAt }`
- `DELETE /api/candidates/{id}`
  - Response: success/failure
- `GET /api/candidates`
  - Query (optional): pagination/filter fields
  - Response: paginated candidate list

### Profile field definitions (admin)
- `GET /api/admin/profile-fields`
  - Response: list of all dynamic field definitions
- `POST /api/admin/profile-fields`
  - Request: `{ name, key, dataType, isRequired, isActive, options }`
  - Response: created field definition
- `PUT /api/admin/profile-fields/{id}`
  - Request: updatable field attributes
  - Response: updated field definition
- `DELETE /api/admin/profile-fields/{id}`
  - Response: success/failure

## Constraints
- Platform target: web application with REST backend.
- Security: role-based access control; only admins can manage field definitions.
- Validation: profile writes must enforce field definition constraints for `customFields`.
- Data model strategy: keep `Name`, `BirthDate`, and `Sex` as physical columns; store non-core fields in `JSONB`.
- Derived data rule: compute `Age` from `BirthDate` at read/query time; do not persist `Age`.
- Indexing: add B-tree indexes for `Name`, `BirthDate`, `Sex` and a GIN index for `CustomFields` (`JSONB`).
- Performance target: CRUD endpoints should remain responsive under normal internal HR usage (exact SLO TBD).
- Budget/time constraints: TBD.

## Open questions
- What authentication mechanism is required (JWT, cookie-based session, SSO)?
- Should employee users be allowed to delete candidate profiles, or only create/update?
- How should field definition edits affect existing candidate data (migrate, keep legacy, or invalidate)?
- What are pagination, filtering, and sorting requirements for candidate list endpoints?
- What deployment target should be used (Azure/AWS/on-prem) and what is the timeline/budget?
