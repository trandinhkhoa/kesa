# Frontend Local Run Guide

This folder contains the static UI app implemented with plain `HTML + CSS + JavaScript`.

## Prerequisites
- Docker Desktop running
- Local repository cloned and opened at project root

## 1) Start full stack (repo root)

```bash
docker compose up -d
docker compose logs -f backend
```

Optional frontend logs:

```bash
docker compose logs -f frontend
```

Backend URL expected by frontend:
- `http://localhost:8080`

If needed, edit `frontend/js/config.js` to change API base URL.

## 2) Open frontend app

Open in browser:
- `http://localhost:5173`

## 3) UI smoke checklist (Phase 6)

### A. Field definitions
1. Open `Profile Field Definitions` tab.
2. Create field:
   - Name: `Years Experience`
   - Key: `yearsExperience`
   - Data Type: `Number`
   - Required: unchecked
   - Active: checked
3. Verify it appears in the list.
4. Edit and save once.
5. Delete only if you want to test delete behavior.

### B. Candidates
1. Open `Candidates` tab.
2. Create candidate with custom fields, including:
   - `yearsExperience` as a number (for example `5`)
3. Verify candidate appears in list.
4. Use `View` to read candidate details in form.
5. Use `Edit` to update `yearsExperience` and save.
6. Verify updated value in list/detail flow.
7. Delete candidate and verify it no longer appears.

### C. Error handling checks
1. Submit candidate with empty required core fields to verify `400` validation display.
2. Create duplicate field key (same `key`) to verify conflict handling (`409`).
3. Stop backend container and attempt refresh to verify network error + retry behavior.

## Current limitations
- No authentication/authorization in UI.
- No advanced filtering/sorting beyond backend pagination.
- No file upload/resume workflows.

## Useful backend commands

```bash
docker compose ps
docker compose stop
docker compose down
```
