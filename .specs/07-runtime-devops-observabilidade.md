# 07 - Runtime, DevOps e Observabilidade

## Ambiente de produção

| Serviço | DNS | Diretório |
|---------|-----|-----------|
| API | `gamehub-api.afonsoft.dev` | `aspnet-core/` |
| Game Hub | `gamehub.afonsoft.dev` | `angular/` |
| Admin | `gamehub-admin.afonsoft.dev` | `angular-admin/` |

**Servidor**: PostgreSQL e Redis já gerenciados no servidor de produção. Não necessita containerizar esses serviços.

**Dockerfiles**: O template EAF já possui Dockerfiles para a API e para o admin. Utilizar os Dockerfiles existentes do template.

## Docker Compose local

Serviços mínimos para desenvolvimento local:

```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7
    ports:
      - "6379:6379"

  minio:
    image: minio/minio
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ACCESS_KEY}
      MINIO_ROOT_PASSWORD: ${MINIO_SECRET_KEY}
    ports:
      - "9000:9000"
      - "9001:9001"

  backend:
    build:
      context: ./aspnet-core
      dockerfile: Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}
      ConnectionStrings__Default: Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      Redis__Configuration: redis:6379
    depends_on:
      - postgres
      - redis
    ports:
      - "5000:80"

  angular-hub:
    build:
      context: ./angular
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      - backend

  angular-admin:
    build:
      context: ./angular-admin
      dockerfile: Dockerfile
    ports:
      - "4201:80"
    depends_on:
      - backend

volumes:
  pgdata:
```

> Em produção, Postgres e Redis estão no servidor e não são containerizados.

## Arquivo .env.example

```env
ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL
POSTGRES_DB=gamehub
POSTGRES_USER=gamehub
POSTGRES_PASSWORD=change-me

# Redis
REDIS_CONNECTION=redis:6379

# JWT
JWT_SECRET=change-me-dev-only

# MinIO / S3
STORAGE_PROVIDER=MinIO
MINIO_ENDPOINT=http://minio:9000
MINIO_ACCESS_KEY=change-me
MINIO_SECRET_KEY=change-me
```

> Nunca commitar `.env` com credenciais reais. Apenas `.env.example`.

## CI/CD mínimo

Pipeline:

1. Restore backend.
2. Build backend.
3. Test backend.
4. Restore frontend hub.
5. Lint frontend hub.
6. Test frontend hub.
7. Build frontend hub.
8. Restore frontend admin.
9. Lint frontend admin.
10. Test frontend admin.
11. Build frontend admin.
12. Docker build (backend, hub, admin).
13. Security scan.
14. Publish artifacts/images.

## Scripts sugeridos

```bash
./scripts/bootstrap.sh
./scripts/run-local.sh
./scripts/test-all.sh
./scripts/lint-all.sh
./scripts/migrate-db.sh
./scripts/seed-dev.sh
```

Todos devem ser idempotentes.

## Health checks

```http
GET /health       ← Verifica API, PostgreSQL e Redis
```

Configurar no Docker Compose:

```yaml
backend:
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:80/health"]
    interval: 30s
    timeout: 10s
    retries: 3
```

## Observabilidade

### Logs

- Serilog com JSON.
- Campos obrigatórios:
  - correlationId
  - tenantId
  - userId
  - gameId
  - buildId
  - requestPath
  - elapsedMs

### Métricas

- Requests por endpoint.
- Latência p95/p99.
- Erros por endpoint.
- Uploads de build por status.
- Validações com falha.
- Gameplay events por jogo.
- Tempo médio de sessão.
- Taxa de loading finished.

### Traces

- API requests.
- EF Core queries.
- Redis operations.
- Jobs Hangfire.
- Storage operations.

## Jobs Hangfire

Jobs iniciais:

- ValidateGameBuildJob
- PublishGameBuildJob
- AggregateGameplayMetricsJob
- RecalculateTrendingGamesJob
- CleanupExpiredUploadsJob
- SyncRedisLeaderboardSnapshotJob

## Cache TTL (Redis)

| Dado | TTL |
|------|-----|
| Home catalog | 5 min |
| Categorias | 30 min |
| Tags | 30 min |
| Game detail | 10 min |
| Leaderboard | 1 min |
| Search results | 2 min |

## Ambientes

- local
- dev
- staging
- prod

Cada ambiente deve possuir appsettings próprio e usar secrets externos para credenciais (Azure KeyVault, AWS Secrets Manager, etc.).

## CI/CD — GitHub Actions

Ferramenta oficial de CI/CD: **GitHub Actions**.

- **Workflow file**: `.github/workflows/ci.yml`
- **Triggers**: push para `main` e PRs contra `main`.
- **Jobs**:
  - `test-backend` — `dotnet restore`, `dotnet build`, `dotnet test`.
  - `test-frontend-hub` — `npm ci`, `npm run lint`, `npm run test` (headless), `npm run build` em `angular/`.
  - `test-frontend-admin` — `npm ci`, `npm run lint`, `npm run test` (headless), `npm run build` em `angular-admin/`.
  - `docker-build` — build das imagens backend, hub e admin usando os Dockerfiles do template EAF.
  - `migrate` — job separado antes do deploy (ver `Migração no CI/CD`).
  - `deploy` — somente após `test-backend`, `test-frontend-*` e `migrate` passarem.
- **Secrets** necessários no repositório:
  - `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` (push de imagens ECR).
  - `DOCKER_REGISTRY` (URI do registry privado).
  - `DEPLOY_SSH_KEY` (chave de deploy no servidor).
  - `PROD_CONNECTION_STRING` (usada apenas pelo job `migrate`).
  - `OTEL_EXPORTER_OTLP_ENDPOINT` (opcional em CI).

## Log Sinks

Configurados por ambiente via `appsettings.{Environment}.json`:

- **Local development**: Serilog → Console (structured JSON).
- **Staging**: Serilog → Seq (https://hub.docker.com/r/datalust/seq). Endpoint configurável via `Serilog__WriteTo__1__Args__serverUrl`.
- **Production**: Serilog → OpenTelemetry Collector → backend (Grafana Tempo/Loki). Exportado via OTLP.

A troca de sink é puramente configuracional — código permanece o mesmo em todos ambientes.

## OpenTelemetry Export

- **OTLP endpoint**: configurável via variável de ambiente `OTEL_EXPORTER_OTLP_ENDPOINT`.
- **Local**: Jaeger (container Docker no `docker-compose.yml`). UI em `http://localhost:16686`.
- **Production**: OpenTelemetry Collector → Grafana Tempo.
- **Traces instrumentados**:
  - API requests (ASP.NET Core).
  - EF Core queries.
  - Redis operations.
  - Hangfire jobs.
  - Storage operations (S3/MinIO).

## Migração no CI/CD

- Migrations rodam como **job separado antes do deploy**.
- Usar projeto dedicado `GameHub.Migrator` (console app que referencia `GameHub.EntityFrameworkCore`).
- **Comando**: `dotnet run --project GameHub.Migrator --configuration Release`.
- **Idempotente**: EF Core garante que cada migration seja aplicada apenas uma vez (tabela `__EFMigrationsHistory`).
- Job `migrate` usa a connection string de produção (secret) e bloqueia o deploy se houver erro.

## Hangfire Dashboard

- Exposto em `/hangfire` via middleware do ASP.NET Core (`app.UseHangfireDashboard(...)`).
- **Proteção**: autorização restrita à role `Admin`.
- **Networking**: acessível apenas na rede interna (não exposto publicamente via DNS). Em produção, exposto apenas atrás de um tunnel/VPN ou via container-side port (interal only), nunca no ingress público.
