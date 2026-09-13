# SPEC-20260912-system-dockerfile-deployment

## 0. Metadata

| Field | Value |
| --- | --- |
| Feature | `system-dockerfile-deployment` |
| Type | `Infra` |
| Stack | `Docker` |
| Repository | `/home/ubuntu/project/gamehub` |
| Branch | `feature/devin-20260912-system-dockerfile-deployment` |
| Ticket | `GH-INFRA-001` |
| Status | `Completed` |

## 1. User Story

**As a** system administrator and developer
**I want** the Docker environment and backend connection strings (`POSTGRES_HOST`, `REDIS_CONNECTION`) correctly configured and deployed on the VPS via Docker Compose
**So that** the backend container successfully connects to PostgreSQL without DNS name resolution failures and all services run stably.

**Problem context:**
The backend container crashed in a restart loop due to `host.docker.internal` not resolving properly inside the isolated Docker bridge network for PostgreSQL/Redis. Updating `.env` to use `postgres` and `redis` service names and deploying via Docker Compose resolves this issue.

## 2. Scope

**In scope:**
- Update `.env` / environment configurations to use container service names (`postgres`, `redis`) instead of `host.docker.internal`.
- Review and refine `Api/Dockerfile` and `docker-compose.all.yml`.
- Execute Docker build and deployment on the VPS using `docker compose`.
- Verify container health and logs.

**Out of scope:**
- Changes to database schema or business logic code.

## 3. Technical Context

**Where the change happens**
- Root configuration (`.env`, `docker-compose.all.yml`, `Api/Dockerfile`)

**Files to read before implementing:**
- `docker-compose.all.yml`
- `.env`
- `Api/Dockerfile`

**Files to create or modify:**
```text
.env
Api/Dockerfile
docker-compose.all.yml
```

## 4. Requirements

### RF-001: Correct Database and Redis Host Configuration
- **Description:** Update `.env` so `POSTGRES_HOST` points to `postgres` and `REDIS_CONNECTION` points to `redis:6379,abortConnect=false`.
- **Rules:** Must resolve correctly within the `gamehub` Docker network.
- **Input → Output:** Backend container starts → Connects successfully to PostgreSQL and Redis.

### RF-002: Docker Build and Deployment on VPS
- **Description:** Rebuild and start containers using `docker compose -f docker-compose.all.yml up -d --build`.
- **Rules:** All services (`postgres`, `redis`, `minio`, `backend`, `angular-hub`, `angular-admin`) must reach healthy status.
- **Input → Output:** Compose command executed → All containers running and healthy.

## 5. API Contract (if applicable)
*N/A — Infrastructure deployment change.*

## 6. Acceptance Criteria

- [ ] **Given** the updated `.env` configuration **when** running `docker compose -f docker-compose.all.yml up -d --build` **then** `gamehub-backend` starts successfully without `Name or service not known` errors.
- [ ] **Given** running containers **when** checking `docker ps` **then** all services show `Up` and healthy status.

## 7. Task Plan (agent execution)

- [ ] **T1 — Configuration Update:** update `.env` with correct internal Docker container service hosts (`postgres`, `redis`).
- [ ] **T2 — Build & Deploy:** execute docker compose build and up on the VPS.
- [ ] **T3 — Verification:** check container logs and health status (`docker compose ps`, `docker logs gamehub-backend`).

## 8. Organization Guardrails

- **Branches:** use `feature/devin-20260912-system-dockerfile-deployment`.
- **Workflows:** do not modify `.github/workflows/`.
- **Security:** do not commit plaintext production secrets.

## 9. Definition of Done

- [x] Configuration files updated.
- [x] Containers built and deployed successfully.
- [x] `gamehub-backend` running stably without restarts.

## 10. Delivered

- **Commit**: `2dab3dd` (fix(infra): update docker-compose.yml defaults to use postgres/redis service names)
- **Branch**: `main`
- **Date**: 2026-09-13
