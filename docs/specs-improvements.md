# Análise de `.specs` vs Implementação Atual — Oportunidades de Melhoria

> Baseado no estado do repositório após os ajustes iniciais de build (commit `19be040` em `main`).

## 1. Resumo executivo

A pasta `.specs` descreve uma plataforma completa de catálogo, distribuição e moderação de jogos web. A implementação atual ainda é essencialmente o template EAF/ABP renomeado, com o domínio de exemplo `Airplane` e muitos placeholders `ProjectName`. As maiores oportunidades estão em: modelar o domínio GameHub, substituir/renamear artefatos do template, implementar os contratos da API, reescrever os frontends conforme as rotas e, por fim, reforçar segurança, observabilidade e DevOps.

---

## 2. Backend — API / Domínio

### 2.1 Substituição do domínio de exemplo

**Especificação:** `04-modelagem-dados.md` define entidades `Game`, `GameBuild`, `DeveloperProfile`, `Category`, `Tag`, `PlaySession`, `GameMetricSnapshot`, `GamePlacement`, `ModerationReview`.

**Implementação atual:**
- `Api/src/GameHub.Core/Airplanes/*` — domínio de exemplo.
- `Api/src/GameHub.Application/Airplanes/*` — AppService de exemplo.
- Migrations iniciais contêm tabelas do template.

**Oportunidade:**
- Remover `Airplanes` e criar as entidades/aggregates do GameHub em `GameHub.Core`.
- Criar DbSets em `GameHub.EntityFrameworkCore/EntityFrameworkCore/ProjectNameDbContext.cs`.
- Gerar nova migration inicial (`dotnet ef migrations add Initial_GameHub`).
- Aplicar soft-delete, `TenantId` e índices conforme a modelagem (ex: `IX_Game_Slug`, `IX_GameBuild_GameId_Version`).

### 2.2 Renomear artefatos `ProjectName`

**Implementação atual:** ainda existem `ProjectNameConsts`, `ProjectNameAuthorizationProvider`, `ProjectNamePermissions`, `ProjectNameApplicationModule`, `ProjectNameDbContext`, etc.

**Oportunidade:**
- Renomear tudo para `GameHub*` (ex: `GameHubConsts`, `GameHubAuthorizationProvider`, `GameHubDbContext`) para manter consistência e evitar confusão no namespace.

### 2.3 Permissões RBAC

**Especificação:** `12-rbac-permissions.md` define hierarquias `Pages.Games.*`, `Pages.Builds.*`, `Pages.Moderation.*` etc.

**Implementação atual:** `ProjectNamePermissions` contém permissões genéricas do template.

**Oportunidade:**
- Criar `GameHubPermissions` com as constantes e descrições do spec.
- Registrar no `AuthorizationProvider`.
- Seed de roles (`Player`, `Developer`, `Moderator`, `Admin`) e atribuições no `HostRoleAndUserCreator`/`TenantRoleAndUserBuilder`.

### 2.4 DTOs e Application Services

**Especificação:** `05-api-contratos.md` + `14-dto-complete-reference.md` definem DTOs de catalog, builds, auth, moderation, leaderboard.

**Implementação atual:** nenhum DTO ou AppService do GameHub.

**Oportunidade:**
- Criar `GameHub.Application/*` por contexto: `Catalog`, `Games`, `Builds`, `Moderation`, `Developer`, `Leaderboard`.
- Usar `PagedAndSortedResultRequestDto` para listagens.
- Adicionar DataAnnotations; evitar FluentValidation (conforme spec).
- Expor via Dynamic API (`/api/services/app`) e controllers explícitos para upload/download (`/api/build/upload`, `/api/build/download`).

### 2.5 Upload e validação de builds

**Especificação:** `10-checklist-validacao.md` exige validador de build: `index.html`, tamanho máximo, SHA-256, tipos permitidos, sem executáveis.

**Implementação atual:** nenhum endpoint de upload.

**Oportunidade:**
- Implementar `GameBuildAppService`/`BuildValidator` que:
  - Recebe pacote `.zip`.
  - Verifica `.exe`, `.dll`, `.bat`, `.cmd`, `.ps1`.
  - Exige `index.html`.
  - Calcula SHA-256.
  - Armazena em MinIO/S3 ou filesystem e persiste URL no `GameBuild`.

### 2.6 Leaderboards e Redis

**Especificação:** leaderboards em Redis Sorted Sets com snapshots no banco.

**Implementação atual:** Redis apenas configurável; sem implementação.

**Oportunidade:**
- Criar `LeaderboardService` usando `StackExchange.Redis`/`IDistributedCache`.
- Implementar `ZADD`, `ZREVRANGE` e snapshots periódicos.

### 2.7 Health checks e observabilidade

**Especificação:** `07-runtime-devops-observabilidade.md` e `10-checklist-validacao.md` pedem endpoint `/health`, Serilog estruturado e OpenTelemetry.

**Implementação atual:** pacotes `Eaf.OpenTelemetry` e `Eaf.Castle.Serilog` referenciados, mas sem configuração explícita no `Program.cs` além do padrão EAF.

**Oportunidade:**
- Adicionar `/health` com checks de PostgreSQL, Redis e MinIO.
- Enriquecer logs com `CorrelationId`, `TenantId`, `UserId`, `GameId`, `BuildId`, `RequestPath`, `ElapsedMs`.
- Configurar exportação de traces/metrics OpenTelemetry.

---

## 3. Segurança e Compliance

### 3.1 Content Security Policy

**Especificação:** `15-csp-security-headers.md` define CSP restritiva, headers `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`, `Permissions-Policy`, `Referrer-Policy` e `Cross-Origin-Resource-Policy`.

**Implementação atual:** `ContentSecurityPolicyMiddleware.cs` tem CSP muito permissiva (`default-src * ... 'unsafe-inline' 'unsafe-eval'`) e utiliza header legado `X-Content-Security-Policy`. Faltam os demais headers.

**Oportunidade:**
- Reescrever o middleware para aplicar a CSP do spec, separando dev (`Content-Security-Policy-Report-Only`) e produção (`Content-Security-Policy`).
- Adicionar todos os security headers listados.
- Ajustar `frame-src` para o domínio de jogos (`https://games.afonsoft.dev` ou CDN).

### 3.2 JWT e refresh tokens

**Especificação:** `08-seguranca-lgpd-compliance.md` — HS256 em dev, RS256 em prod, access token 30 min, refresh token 7 dias em cookie `HttpOnly`, blacklist no Redis.

**Implementação atual:** autenticação EAF/ABP padrão, sem refresh token customizado e com `UseEafKeyVault(opt => opt.Provider = EnumKeyVault.None)`.

**Oportunidade:**
- Configurar `JwtBearer` conforme o spec (claim `sub` = UserId, `role` array).
- Implementar `TokenAuthController` com refresh/revogação.
- Substituir `localStorage` por `sessionStorage` (access) + cookie `HttpOnly` (refresh).
- Criar interceptor de refresh silencioso no Angular.

### 3.3 Rate limiting

**Especificação:** `05-api-contratos.md` define limites por recurso e headers `X-RateLimit-*`.

**Implementação atual:** ausente.

**Oportunidade:**
- Usar `AspNetCoreRateLimit` ou `Microsoft.AspNetCore.RateLimiting`.
- Configurar políticas distintas para catalog, gameplay events, leaderboard submit, upload, login e reports.
- Garantir retorno `429` com envelope padrão.

### 3.4 CORS

**Especificação:** `08-seguranca-lgpd-compliance.md` — CORS configurado para os dois frontends.

**Implementação atual:** precisa ser revisado em `Startup.cs`/`Program.cs`.

**Oportunidade:**
- Configurar `Cors` para origens `https://gamehub.afonsoft.dev` e `https://gamehub-admin.afonsoft.dev` em produção, e `http://localhost:4200`/`http://localhost:4201` em dev.

### 3.5 LGPD

**Especificação:** `08-seguranca-lgpd-compliance.md` menciona consentimento, anonimização e retenção.

**Implementação atual:** não implementado.

**Oportunidade:**
- Adicionar consent tracking (`UserConsent`).
- Implementar endpoint `DELETE /api/profile/me` para anonimização.
- Endpoint `GET /api/profile/export` para dados pessoais.
- Política de retenção para logs e eventos.

---

## 4. Frontend Game Hub (`angular/`)

### 4.1 Estrutura e rotas

**Especificação:** `06-frontend-angular.md` e `13-frontend-routing.md` definem módulos `core/`, `shared/`, `public/`, `player/`, `developer/` com lazy loading.

**Implementação atual:** app Angular 20 standalone recém-gerado, apenas um `App` component com hello world.

**Oportunidade:**
- Criar estrutura modular (mesmo que standalone) com pastas `core`, `shared`, `public`, `player`, `developer`.
- Configurar lazy routes para `/games`, `/play/:slug`, `/leaderboard/:gameId`, `/developer/*`, `/login`, `/register`.
- Implementar `AuthGuard`, `DeveloperGuard`, `GuestGuard`.
- Implementar `JwtInterceptor`, `ErrorInterceptor`, `CorrelationIdInterceptor`.
- Criar `GameplayBridgeService` com os 10 eventos (`gameLoadingFinished`, `gameplayStart`, `gameplayStop`, `commercialBreak`, `rewardedBreak`, `captureError`, `measure`, etc.).
- Criar `GameShellComponent` com `<iframe sandbox>` para o jogo.

### 4.2 Design system

**Especificação:** design system próprio, **não** Angular Material.

**Implementação atual:** nenhum design system.

**Oportunidade:**
- Criar componentes base em `shared/components`: `button`, `card`, `input`, `modal`, `table`, `badge`, `skeleton`, `toast`, `pagination`, `dropdown`, `tabs`.
- Definir tokens CSS de cores/tipografia em `styles.css`.

### 4.3 Serviços e modelos

**Oportunidade:**
- `GameService`, `BuildService`, `DeveloperService`, `LeaderboardService`.
- Modelos `GameCardDto`, `GameDetailDto`, `BuildDto`, `PagedResult<T>` etc.
- Pipes `date`, `truncate`, `safe-html`.

---

## 5. Frontend Admin (`angular-admin/GameHub.UI`)

### 5.1 Módulos

**Especificação:** `06b-frontend-admin.md` — módulos `games/`, `moderation/`, `categories/`, `tags/`, `dashboard/`.

**Implementação atual:** template EAF com User/Roles/Tenants, mas sem módulos GameHub.

**Oportunidade:**
- Criar modules/routes para `games`, `moderation`, `categories`, `tags`, `dashboard`.
- Criar `AdminGuard` e `ModeratorGuard`.
- Tabelas com paginação e filtros.
- Fila de moderação (`review-queue`, `review-detail`).
- Dashboard de métricas.

### 5.2 Ajustes do template EAF

**Implementação atual:** o `eaf-ng2-module` estava com `LogService` ausente (corrigido provisoriamente com stub). O nome do package ainda é `eaf-projectname-ui`.

**Oportunidade:**
- Renomear package e referências de `ProjectName` para `GameHub` no admin.
- Revisar e substituir o `LogService` stub por implementação alinhada à estratégia de logging do EAF/Serilog.

---

## 6. DevOps e Infraestrutura

### 6.1 Docker Compose local

**Especificação:** `07-runtime-devops-observabilidade.md` — compose com Postgres 16, Redis 7, MinIO, backend, angular-hub e angular-admin.

**Implementação atual:** existe `Api/docker-compose.yml` apenas para a API; não há compose na raiz com todos os serviços.

**Oportunidade:**
- Criar `docker-compose.yml` na raiz do repo.
- Adicionar `.env.example` com `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `MINIO_*`, `JWT_SECRET`.
- Validar Dockerfiles do EAF para API e admin; criar Dockerfile para `angular/` (hub).

### 6.2 CI/CD GitHub Actions

**Implementação atual:** criados `ci-build-test.yml`, `angular-ci.yml`, `code-quality.yml`, `delete-branch-on-merge.yml`.

**Oportunidade:**
- Adicionar workflow de `docker build` e `docker compose config` para validar a stack local.
- Adicionar workflow de publicação de imagens (`publish.yml`) e deploy.
- Configurar secrets `SONARQUBE_TOKEN`, `SNYK_TOKEN`, `QODANA_TOKEN` para ativar as ferramentas de qualidade.
- Adicionar testes no Angular (`ng test --no-watch --browsers=ChromeHeadlessNoSandbox`) quando existirem.

---

## 7. Testes

### 7.1 Cobertura e domínio

**Implementação atual:** testes do template passam (211 passed), mas são do domínio `Airplane`.

**Oportunidade:**
- Substituir por testes de domínio (`GameTests`, `GameBuildTests`, `BuildValidatorTests`).
- Testes de integração nos Application Services e controllers.
- Testes de build/upload.
- Alinhar com xUnit + FluentAssertions/Moq (conforme AGENTS) ou manter Shouldly/NSubstitute se for a intenção do projeto.

---

## 8. Priorização sugerida

1. **Crítico — build e estrutura base:**
   - Renomear `ProjectName*` para `GameHub*`.
   - Criar entidades do spec e gerar nova migration.
   - Criar DTOs e Application Services mínimos para `Game`/`GameBuild`.
   - Substituição do `Airplane` de exemplo.

2. **Alto — segurança:**
   - Reescrever `ContentSecurityPolicyMiddleware`.
   - Configurar CORS, rate limiting e refresh tokens/JWT.

3. **Alto — frontend:**
   - Estruturar `angular/` com rotas, guards e gameplay bridge.
   - Criar módulos admin para games/moderation/categories/tags.

4. **Médio — DevOps:**
   - Criar root `docker-compose.yml` e `.env.example`.
   - Adicionar secrets e ativar Sonar/Snyk/Qodana.

5. **Médio/Baixo — qualidade:**
   - Cobertura de testes > 80%.
   - XML docs em APIs públicas.
   - i18n pt-BR/en-US nos frontends.
   - Documentação de LGPD.

---

## 9. Notas técnicas

- O backend já compila com .NET 10, EAF 9.2.0 e ABP 10.4.0; a base está saudável para evolução.
- O admin Angular compila após a inclusão do `LogService`, mas ainda usa metadados/rotas do template EAF; uma refatoração para os módulos do GameHub será necessária.
- O hub Angular foi criado em Angular 20 com componentes standalone. As specs usam módulos lazy (`loadChildren`). Recomenda-se migrar para `loadComponent`/`loadChildren` com standalone ou criar feature modules dependendo do padrão que a equipe preferir.
- Os workflows estão ativos; recomenda-se validar os secrets de qualidade antes de habilitar os jobs opcionais.
