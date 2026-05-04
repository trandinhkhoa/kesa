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

## Remote hosting (Linode VM)

### One-time VM setup
The VM requires Docker Compose v2. The default Ubuntu package `docker-compose-plugin` is unavailable on this image, so install the binary directly:

```bash
curl -SL https://github.com/docker/compose/releases/download/v2.27.0/docker-compose-linux-x86_64 -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
```

### Deploy / update
From your local machine, sync the project and restart the stack:

```bash
rsync -av --exclude='.git' --exclude='obj' --exclude='bin' . root@<VM_IP>:/root/app/
ssh root@<VM_IP> "cd /root/app && docker-compose up -d --force-recreate"
```

If old containers from a previous docker-compose v1 run are blocking the start:

```bash
ssh root@<VM_IP> "docker rm -f \$(docker ps -aq); cd /root/app && docker-compose up -d"
```

### Access
- **UI:** `http://<VM_IP>:5173`
- **API:** `http://<VM_IP>:8080` (direct, bypass nginx)

### Architecture notes
- nginx (port 5173) proxies `/api/` requests to the `backend` container internally — no CORS headers needed.
- The backend runs `rm -rf obj bin` on startup to avoid NuGet cache incompatibility between macOS-built artifacts and the Linux container.
- Migrations run automatically on every backend startup via `dotnet-ef database update`.

---

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
      "religion": "Khac",
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
      "religion": "Dao Thien Chua",
      "marriage": "Doc Than"
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
      "religion": "Khac",
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
      "marriage": "Doc Than",
      "religion": "Dao Thien Chua",
      "yearsExperience": 4
    }
  }'
```

### 11) Verify Alice update
```bash
curl -i "$BASE_URL/api/v1/candidates/cd2b14dc-5e9d-44ac-8e31-acf3a68e6458"
```
