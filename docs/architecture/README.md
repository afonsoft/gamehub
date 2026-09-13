# GameHub — Architecture Diagrams

## System Architecture

![System Architecture](gamehub_system_architecture.mmd)

### Component Overview

| Layer | Component | Technology | Port |
|-------|-----------|------------|------|
| **Frontend** | angular-hub (Public Portal) | Angular 20 | 4600 |
| **Frontend** | angular-admin (Admin Panel) | EAF Angular | 4602 |
| **Frontend** | iframe Player | GameplayBridge (postMessage) | — |
| **API** | ASP.NET Core Web.Host | .NET 10 / ABP | 4601→80 |
| **Security** | JWT Auth + CORS + Rate Limiting | ASP.NET Core | — |
| **Security** | CSP Headers + HSTS | SecurityHeadersMiddleware | — |
| **Application** | GameAppService | Clean Architecture | — |
| **Application** | PlayerAccountAppService | Clean Architecture | — |
| **Application** | RevenueAppService | Clean Architecture | — |
| **Application** | AnalyticsAppService | Clean Architecture | — |
| **Application** | ModerationAppService | Clean Architecture | — |
| **Application** | PrivacyAppService | Clean Architecture | — |
| **Data** | PostgreSQL 16 | Primary Database | 5432 (internal) |
| **Data** | Redis 7 | Cache + Sessions | 6379 (internal) |
| **Data** | MinIO | Object Storage | 9000/9001 |

---

## Deployment Architecture (Docker Compose)

![Deployment](gamehub_deployment.mmd)

### Container Topology

```
┌─────────────────────────────────────────────────────────┐
│  VPS Ubuntu — /home/ubuntu/project/gamehub               │
│  Network: gamehub (bridge)                               │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐                     │
│  │ angular-hub  │  │ angular-admin│                     │
│  │ Port: 4600   │  │ Port: 4602   │                     │
│  └──────┬───────┘  └──────┬───────┘                     │
│         │ HTTP            │ HTTP                         │
│         └────────┬────────┘                              │
│                  ▼                                       │
│         ┌──────────────┐                                 │
│         │   backend    │                                 │
│         │ Port: 4601   │                                 │
│         └──────┬───────┘                                 │
│                │                                         │
│    ┌───────────┼───────────┐                             │
│    ▼           ▼           ▼                             │
│ ┌──────┐  ┌──────┐  ┌────────┐                          │
│ │postgres│  │ redis │  │ minio  │                         │
│ │ :5432 │  │ :6379 │  │:9000/1 │                        │
│ └──────┘  └──────┘  └────────┘                          │
└─────────────────────────────────────────────────────────┘
```

---

## API Request Sequence

![Sequence](gamehub_sequence_auth.mmd)

### Authentication Flow

1. User enters credentials → Angular sends POST `/api/account/login`
2. Backend validates credentials against PostgreSQL
3. JWT tokens generated (Access + Refresh)
4. Tokens stored in localStorage, sent via Authorization header

### Authenticated Request Flow

1. Frontend sends request with Bearer token
2. JWT middleware validates signature, checks Claims
3. Redis cache checked (hit → cached response; miss → DB query + cache store)
4. Response returned to frontend

---

## Data Flow

![Data Flow](gamehub_data_flow.mmd)

### Key Data Paths

| Flow | Description |
|------|-------------|
| 🎮 Game Catalog | API → Redis Cache → JSON Response |
| 👤 Player Stats | PostgreSQL → AppService → Analytics |
| 💰 Revenue Splits | Analytics → Revenue Splitter → Developer Earnings |
| 🛡️ Moderation | Upload → Audit → Approve → Build Pipeline |
| 📊 Telemetry | GameplayBridge → API → OpenTelemetry Export |

---

## Generated Artifacts

| File | Type | Description |
|------|------|-------------|
| `gamehub_system_architecture.mmd` | Mermaid | System architecture (C4 container) |
| `gamehub_deployment.mmd` | Mermaid | Docker Compose deployment |
| `gamehub_sequence_auth.mmd` | Mermaid | Auth + API request sequence |
| `gamehub_data_flow.mmd` | Mermaid | Data flow diagram |
| `gamehub_system_architecture.drawio` | draw.io | Editable system architecture |
