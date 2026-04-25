# Kesa API Quick Start

## Start local stack (Docker Compose)
Run from repo root:

```bash
docker compose up -d
```

This starts:
- `db` at `localhost:5432`
- `backend` at `http://localhost:8080`
- `frontend` at `http://localhost:5173`

View backend logs (follow live output):

```bash
docker compose logs -f backend
```

Optional frontend logs:

```bash
docker compose logs -f frontend
```

Optional useful commands:

```bash
docker compose ps
docker compose stop
docker compose down
```

Frontend smoke guide:
- `frontend/README.md`

Set base URL for curl examples:

```bash
BASE_URL="http://localhost:8080"
```

## API test queries used so far

### 1) Check API is up
```bash
curl -i "$BASE_URL/openapi/v1.json"
```

### 2) List admin profile fields
```bash
curl -i "$BASE_URL/api/v1/admin/profile-fields"
```

### 3) Create candidate
```bash
curl -i -X POST "$BASE_URL/api/v1/candidates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Nguyen",
    "birthDate": "1998-05-10",
    "sex": "Female",
    "customFields": {
      "address": "HCMC",
      "religion": "others",
      "marriage": "no"
    }
  }'
```

### 4) List candidates (pagination)
```bash
curl -i "$BASE_URL/api/v1/candidates?pageNumber=1&pageSize=20"
```

### 5) Get candidate by id
```bash
curl -i "$BASE_URL/api/v1/candidates/<CANDIDATE_ID>"
```

### 6) Update candidate
```bash
curl -i -X PUT "$BASE_URL/api/v1/candidates/<CANDIDATE_ID>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Nguyen Updated",
    "birthDate": "1998-05-10",
    "sex": "Female",
    "customFields": {
      "address": "Da Nang",
      "religion": "christian",
      "marriage": "married"
    }
  }'
```

### 7) Delete candidate
```bash
curl -i -X DELETE "$BASE_URL/api/v1/candidates/<CANDIDATE_ID>"
```

### 8) Create admin field definition (`yearsExperience`)
```bash
curl -i -X POST "$BASE_URL/api/v1/admin/profile-fields" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Years Experience",
    "key": "yearsExperience",
    "dataType": "Number",
    "isRequired": false,
    "isActive": true
  }'
```

### 9) Create candidate with `yearsExperience`
```bash
curl -i -X POST "$BASE_URL/api/v1/candidates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "birthDate": "1995-08-15",
    "sex": "Male",
    "customFields": {
      "address": "Hanoi",
      "religion": "others",
      "marriage": "no",
      "yearsExperience": 5
    }
  }'
```

### 10) Update Alice with `yearsExperience`
```bash
curl -i -X PUT "$BASE_URL/api/v1/candidates/cd2b14dc-5e9d-44ac-8e31-acf3a68e6458" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Nguyen Updated",
    "birthDate": "1998-05-10",
    "sex": "Female",
    "customFields": {
      "address": "Da Nang",
      "marriage": "married",
      "religion": "christian",
      "yearsExperience": 4
    }
  }'
```

### 11) Verify Alice update
```bash
curl -i "$BASE_URL/api/v1/candidates/cd2b14dc-5e9d-44ac-8e31-acf3a68e6458"
```
