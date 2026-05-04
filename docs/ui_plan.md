# Frontend UI Implementation Plan (HTML + CSS + JS)

## Goal
Build a local frontend UI for the current backend APIs defined in `docs/specs.md` with these constraints:
- No authentication/authorization UI.
- No user creation/login screens.
- Any user who can access the UI has full API access.
- Stack limited to plain `HTML + CSS + JavaScript`.

## Scope from backend API
- Candidate profile CRUD:
  - `GET /api/v1/candidates`
  - `GET /api/v1/candidates/{id}`
  - `POST /api/v1/candidates`
  - `PUT /api/v1/candidates/{id}`
  - `DELETE /api/v1/candidates/{id}`
- Admin profile field definition CRUD:
  - `GET /api/v1/admin/profile-fields`
  - `GET /api/v1/admin/profile-fields/{id}`
  - `POST /api/v1/admin/profile-fields`
  - `PUT /api/v1/admin/profile-fields/{id}`
  - `DELETE /api/v1/admin/profile-fields/{id}`

## Architectural approach (confirmed)
Use a single-page UI with modular JavaScript files:
- `index.html` (layout + mount points)
- `styles.css` (all styling)
- `js/config.js` (API base URL)
- `js/api.js` (HTTP wrapper + error handling)
- `js/state.js` (in-memory state)
- `js/fields.js` (field definition module)
- `js/candidates.js` (candidate module)
- `js/ui.js` (shared UI helpers)
- `js/main.js` (bootstrap + app orchestration)

This preserves the simple stack while keeping feature logic separated and maintainable.

## Information architecture
- Top header: app title + backend URL indicator.
- Two primary sections/tabs:
  1. `Candidates`
  2. `Profile Field Definitions`
- Candidates section:
  - Candidate table/list.
  - Search/filter placeholder (UI only for now, optional wiring later).
  - Pagination controls (`pageNumber`, `pageSize`).
  - Create/Edit candidate form.
  - Dynamic custom fields generated from active field definitions.
- Field definitions section:
  - Field definition table/list.
  - Create/Edit field definition form.
  - Delete action with confirmation.

## Detailed implementation phases

### Phase 1: Project scaffolding
1. Create frontend directory (for example `frontend/`) with file structure listed above.
2. Add a tiny static serving approach for local run (VS Code Live Server or simple static server).
3. Configure backend base URL in `js/config.js` (default `http://localhost:8080`).

Deliverable:
- App shell renders with empty sections and no API calls yet.

### Phase 2: API client and global behavior
1. Implement `api.js` with reusable helpers:
   - `get(path)`, `post(path, body)`, `put(path, body)`, `del(path)`.
2. Standardize response handling:
   - Parse JSON on success.
   - Parse `ProblemDetails`/validation payload on failure.
3. Add timeout and network error messages.
4. Add loading indicator hooks and error toast/alert hooks.

Deliverable:
- Any module can perform backend calls with consistent error handling.

### Phase 3: Profile field definition module
1. Implement list view from `GET /api/v1/admin/profile-fields`.
2. Implement create form for:
   - `name`, `key`, `dataType`, `isRequired`, `isActive`, `options`.
3. Add dataType-aware option editor:
   - Show options input only when `dataType = Enum`.
4. Implement update flow (`PUT`) and delete flow (`DELETE`).
5. Refresh list after create/update/delete operations.

Deliverable:
- Full CRUD for dynamic field definitions from UI.

### Phase 4: Candidate module
1. Implement paginated candidate list from:
   - `GET /api/v1/candidates?pageNumber=&pageSize=`.
2. Implement candidate create form:
   - Core fields: `name`, `birthDate`, `sex`.
   - Dynamic custom fields rendered from active definitions.
3. Implement candidate detail/edit flow:
   - Load by id (`GET /api/v1/candidates/{id}`) when editing.
   - Update with `PUT /api/v1/candidates/{id}`.
4. Implement delete action with confirmation.
5. Add post-action refresh and optimistic UI state updates where simple.

Deliverable:
- Full candidate CRUD including dynamic field support.

### Phase 5: UX polish and validation
1. Client-side pre-validation (required fields, basic type checks) before submit.
2. Server-side validation feedback mapping:
   - Show field-level errors from `ValidationProblemDetails`.
3. Empty/loading/error states for all lists and forms.
4. Confirm dialogs for destructive actions.
5. Responsive layout for desktop and mobile widths.

Deliverable:
- Usable, clear, and resilient UI for daily local testing.

### Phase 6: Local integration and smoke checklist
1. Run backend via Docker Compose (`db` + `backend`).
2. Run frontend static server.
3. Validate end-to-end flow:
   - Create field definition `yearsExperience`.
   - Create candidate with `yearsExperience`.
   - Update candidate custom fields.
   - Delete candidate and verify not found.
4. Capture known limitations and next improvements.

Deliverable:
- Verified local workflow for backend + frontend integration.

## Data mapping notes
- `Candidate.customFields` is a key-value object; keys must match active field definition keys.
- Render dynamic input component by `dataType`:
  - `String` -> text input
  - `Number` -> numeric input
  - `Date` -> date input
  - `Boolean` -> checkbox/toggle
  - `Enum` -> select dropdown from options
- Preserve JSON types correctly when serializing request payloads.

## Error handling plan
- `400` validation errors: inline field messages + summary panel.
- `404`: show not-found notification and refresh list state.
- `409`: show conflict message with actionable hint.
- `500`/network failures: non-blocking banner/toast with retry option.

## Non-goals (current iteration)
- Auth, roles, login, token refresh.
- Advanced filtering/sorting beyond current API pagination.
- File uploads, attachments, or resume parsing.
- UI framework adoption (React/Vue/etc.).

## Suggested implementation order (execution)
1. Frontend scaffolding + API client.
2. Field definitions CRUD UI.
3. Candidate CRUD UI with dynamic custom fields.
4. Validation/error UX and responsive polish.
5. Integration smoke test and documentation updates.

## Acceptance criteria
- UI can create/update/delete profile field definitions.
- UI can create/update/delete candidates.
- Dynamic fields in candidate form reflect active backend definitions.
- Backend validation errors are visible and understandable in UI.
- End-to-end local testing works with Docker Compose backend setup.

---

## Changelog

### Phase 7: Candidate page-based navigation (hash router)

**Goal:** Split the candidate section from a side-by-side list+form layout into two distinct pages navigated by URL hash.

**New file: `js/router.js`**
A minimal client-side hash router. Supports parameterised patterns (e.g. `/candidates/:id/edit`). `route(pattern, handler)` registers a handler; `navigate(path)` sets `window.location.hash`; `initRouter()` attaches the `hashchange` listener and dispatches the initial route.

**URL scheme:**
| Hash | Page |
|------|------|
| `#/candidates` | Candidate list (default) |
| `#/candidates/new` | Candidate create form |
| `#/candidates/:id/edit` | Candidate edit form |
| `#/candidates/:id/view` | Candidate view (read-only) |
| `#/fields` | Profile field definitions panel |

**`index.html` changes**
- Removed the `.panel-grid` two-column layout from the candidates section.
- Added `#candidate-list-view`: full-width card with table, pagination, and a `+ Ứng Viên Mới` button in the header.
- Added `#candidate-detail-view`: a constrained-width card (`max-width: 680px`) containing the same form used for create, edit, and view — only the mode badge and field editability differ. Includes a `← Quay Lại` back button.
- The Fields panel is unchanged (still uses the side-by-side `.panel-grid` layout).

**`js/candidates.js` changes**
- Imported `navigate` from `router.js`.
- Table row buttons (View, Edit) now call `navigate(...)` instead of directly filling the inline form.
- Exported three route-handler functions: `showCandidateList`, `showCandidateCreate`, `showCandidateDetail(id, mode)`.
- After a successful create or update, `navigate('/candidates')` returns the user to the list.
- After a 404 error on open, redirects to `/candidates` instead of resetting the inline form.
- `initializeCandidateModule` wires the new `candidate-new-btn` and `candidate-back-btn` buttons.

**`js/main.js` changes**
- Imports `initRouter`, `navigate`, `route` from `router.js`.
- `initializeTabs` now calls `navigate('/candidates')` / `navigate('/fields')` instead of the old `switchTab` helper directly.
- `registerRoutes()` maps the four candidate routes and the fields route; each calls `switchTab` then the appropriate show function.
- `initRouter()` is called at the end of `bootstrap()`, after data is loaded, so the initial route dispatches with field definitions already in state.

**`js/i18n.js` changes**
- Added `newCandidate: "+ Ứng Viên Mới"` and `backToList: "← Quay Lại"`.

**`styles.css` changes**
- `.header-actions`: flex row for the list-page header buttons.
- `.page-header-left`: flex row for the back button + title in the detail-page header.
- `.detail-view`: caps width at `680px` so the form does not stretch full-viewport on wide screens.
