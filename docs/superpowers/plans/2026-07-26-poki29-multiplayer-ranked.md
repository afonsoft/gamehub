# Poki 29 Multiplayer Browser and Ranked Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add public match discovery, server-authoritative ranked queues and match history, defensive multiplayer validation, distributed SignalR presence, and administrative controls while preserving the existing `IMatchmakingService` and game-scoped authorization.

**Architecture:** Extend the multiplayer domain with persisted ranked seasons, player ratings, queue entries, match history, replay metadata, and security events. Keep matchmaking decisions in `IMatchmakingService`, expose query/queue/history operations through application services, use Redis for distributed queue/presence/backplane state when enabled with an in-memory fallback for tests, and keep client score/MMR claims advisory only.

**Tech Stack:** .NET 10, ASP.NET Boilerplate/EAF, EF Core, PostgreSQL/SQL Server, SignalR, StackExchange.Redis, Hangfire, Angular 20, xUnit, Shouldly.

---

### Task 1: Establish the new branch and baseline

**Files:**
- Branch only: `devin/*-poki29-multiplayer-ranked`
- Read: `.specs/29-poki-multiplayer-browser-ranked.md`
- Read: `Api/src/GameHub.Core/Domain/Multiplayer/IMatchmakingService.cs`
- Read: `Api/src/GameHub.Application/Gameplay/LeaderboardAppService.cs`

- [ ] **Step 1: Verify the checkout is clean and based on merged `main`**

Run:

```bash
git status --short
git fetch origin main
git merge-base --is-ancestor origin/main HEAD
```

Expected: empty status and exit code `0` for the merge-base check.

- [ ] **Step 2: Restore backend and frontend dependencies**

Run:

```bash
dotnet restore Api/GameHub.sln
(cd angular && npm install)
```

Expected: both commands exit `0`.

---

### Task 2: Add persisted ranked and match-history domain models

**Files:**
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/RankedSeason.cs`
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/PlayerRating.cs`
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/RankedQueueEntry.cs`
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/MatchHistory.cs`
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/ReplayMetadata.cs`
- Create: `Api/src/GameHub.Core/Domain/Multiplayer/MultiplayerSecurityEvent.cs`
- Modify: `Api/src/GameHub.Core/Domain/Multiplayer/MatchState.cs`

- [ ] **Step 1: Write failing domain/application tests**

Add BDD tests asserting:

```csharp
[Fact]
public void Dado_RatingInicial_Quando_AplicarResultado_Entao_MMRNaoFicaNegativo()
{
    var rating = new PlayerRating { Rating = 1000 };
    rating.ApplyResult(0, 1, 32);
    rating.Rating.ShouldBe(968);
}

[Fact]
public void Dado_MatchFinalizado_Quando_CriarHistorico_Entao_NaoAceitaScoreDoClienteComoResultado()
{
    // The server result enum is the only input used to update ratings.
}
```

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Ranked"
```

Expected: FAIL because the new types do not exist.

- [ ] **Step 2: Implement the entities and invariants**

Use `FullAuditedEntity`/`FullAuditedAggregateRoot` with `IMayHaveTenant`, nullable tenant IDs, explicit indexes, and these invariants:

- `RankedSeason`: game, mode, name, start/end dates, `IsActive`.
- `PlayerRating`: game, season, user, mode, `Rating` default `1000`, wins/losses/draws, `GamesPlayed`, `LastPlayedAt`.
- `RankedQueueEntry`: game, season, mode, user, anonymous hash, rating snapshot, region, enqueue time, status, matched match ID.
- `MatchHistory`: match/game/season/mode, started/ended times, completion/abandonment status, server winner/result, participant result rows in JSON.
- `ReplayMetadata`: match ID, storage key, event count, duration, hash, retention expiry.
- `MultiplayerSecurityEvent`: match/game/user/connection, event type, reason, payload hash, created time.
- `MatchState`: add `Region`, `IsRanked`, `RankedSeasonId`, `AverageLatencyMs`, `CompletedAt`.

- [ ] **Step 3: Run the focused tests and commit**

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Ranked"
```

Expected: PASS.

Commit:

```bash
git add Api/src/GameHub.Core/Domain/Multiplayer Api/test/GameHub.Tests
git commit -m "feat(multiplayer): add ranked domain models"
```

---

### Task 3: Add EF mappings and migration

**Files:**
- Modify: `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`
- Modify: `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs`
- Create: `Api/src/GameHub.EntityFrameworkCore/Migrations/*_Poki29.cs`
- Create: `Api/src/GameHub.EntityFrameworkCore/Migrations/*_Poki29.Designer.cs`
- Modify: `Api/src/GameHub.EntityFrameworkCore/Migrations/GameHubDbContextModelSnapshot.cs`

- [ ] **Step 1: Add DbSets and relational mappings**

Configure required lengths, enum conversions, tenant indexes, unique `(GameId, SeasonId, UserId, Mode)` rating constraint, queue indexes by `(GameId, SeasonId, Mode, Status, EnqueuedAt)`, history indexes by game/date, and cascade/restrict behavior matching the existing multiplayer mappings.

- [ ] **Step 2: Generate and inspect migration**

Run:

```bash
dotnet ef migrations add Poki29 \
  --project Api/src/GameHub.EntityFrameworkCore/GameHub.EntityFrameworkCore.csproj \
  --startup-project Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj \
  --configuration Release
```

Expected: migration creates all new tables/columns and no unrelated schema changes.

- [ ] **Step 3: Build the solution**

Run:

```bash
dotnet build Api/GameHub.sln -c Release --no-restore
```

Expected: `Build succeeded`, zero errors.

---

### Task 4: Implement public match browser and ranked queue contracts

**Files:**
- Create: `Api/src/GameHub.Application/Multiplayer/Dto/MatchBrowserInputs.cs`
- Create: `Api/src/GameHub.Application/Multiplayer/Dto/MatchBrowserDtos.cs`
- Create: `Api/src/GameHub.Application/Multiplayer/Dto/RankedQueueDtos.cs`
- Modify: `Api/src/GameHub.Core/Domain/Multiplayer/IMatchmakingService.cs`
- Modify: `Api/src/GameHub.Application/Multiplayer/IMultiplayerAppService.cs`
- Modify: `Api/src/GameHub.Application/Multiplayer/MultiplayerAppService.cs`
- Modify: `Api/src/GameHub.Application/Multiplayer/MatchmakingService.cs`

- [ ] **Step 1: Add failing application tests**

Cover:

```csharp
Dado_SalasPublicas_Quando_FiltrarPorJogoModoRegiaoELatencia_Entao_RetornaSomenteCorrespondentes
Dado_FilaRanqueada_Quando_EnfileirarDuasPontuacoesCompativeis_Entao_CriaMatchNoMesmoModoERegiao
Dado_FilaRanqueada_Quando_Desistir_Entao_MarcaAbandonoSemAlterarMMR
```

Run the tests and verify they fail before implementation.

- [ ] **Step 2: Implement browser query**

Add `BrowseMatchesAsync` with validated page size (`1..100`), optional game/mode/region/status filters, maximum latency filter, and only public non-expired waiting/in-progress rooms. Return match ID, room code, game, mode, player/spectator counts, max players, region, average latency, ranked flag, and timestamps.

- [ ] **Step 3: Implement ranked queue**

Add enqueue/cancel/status operations. Match only entries from the same game, season, mode, region, and expanding MMR window. The server creates the match and queue transition atomically; client-provided score/rank is never accepted as authoritative.

- [ ] **Step 4: Implement match completion**

Add a server-authoritative completion operation requiring a participant and server result. Persist history and replay metadata, update ratings with a bounded Elo-style delta, and prevent duplicate completion by match ID.

- [ ] **Step 5: Run focused tests and commit**

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Multiplayer"
```

Expected: PASS.

Commit:

```bash
git add Api/src/GameHub.Core/Domain/Multiplayer Api/src/GameHub.Application/Multiplayer Api/test/GameHub.Tests
git commit -m "feat(multiplayer): add match browser and ranked queue"
```

---

### Task 5: Add Redis-backed distributed queue, presence, and SignalR backplane

**Files:**
- Create: `Api/src/GameHub.Application/Multiplayer/IRankedMatchmakingStore.cs`
- Create: `Api/src/GameHub.Application/Multiplayer/InMemoryRankedMatchmakingStore.cs`
- Create: `Api/src/GameHub.Web.Host/Caching/RedisRankedMatchmakingStore.cs`
- Modify: `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs`
- Modify: `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- Modify: `Api/src/GameHub.Web.Host/Hubs/NetworkSignalRHub.cs`
- Modify: `Api/src/GameHub.Web.Host/Hubs/GameHubMatchHub.cs`
- Modify: `Api/src/GameHub.Web.Host/appsettings.Development.json`

- [ ] **Step 1: Write store contract tests**

Verify enqueue/dequeue/cancel, connection-to-user presence lookup, TTL expiry, and peer broadcast membership using the in-memory store.

- [ ] **Step 2: Implement in-memory and Redis stores**

Use tenant/game/season/mode/region-prefixed keys, sorted queue timestamps, TTLs, atomic Redis transactions where needed, and no key scanning in request paths.

- [ ] **Step 3: Enable SignalR Redis backplane only when configured**

When `RedisCache:IsEnabled` and a connection string are present, call `AddStackExchangeRedis` for SignalR and replace the ranked store with Redis; otherwise retain the existing in-memory behavior for tests and local development.

- [ ] **Step 4: Add presence and reconnect telemetry**

Register logical user/anonymous identity with connection IDs, preserve the existing 30-second reconnect semantics, and publish reconnect/disconnect events through the hub group.

- [ ] **Step 5: Run store and hub tests and commit**

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Redis|FullyQualifiedName~SignalR|FullyQualifiedName~Multiplayer"
```

Expected: PASS.

Commit:

```bash
git add Api/src/GameHub.Application/Multiplayer Api/src/GameHub.Web.Host Api/test/GameHub.Tests
git commit -m "feat(multiplayer): add distributed queue and presence"
```

---

### Task 6: Add defensive validation, replay metadata, and matchmaking metrics

**Files:**
- Create: `Api/src/GameHub.Core/Application/Jobs/CleanupMultiplayerHistoryJob.cs`
- Create: `Api/src/GameHub.Application/Multiplayer/Dto/MatchHistoryDtos.cs`
- Modify: `Api/src/GameHub.Application/Multiplayer/MatchmakingService.cs`
- Modify: `Api/src/GameHub.Core/Application/Monitoring/GameHubMetrics.cs`
- Modify: `Api/src/GameHub.Core/Application/Extensions/HangfireExtensions.cs`
- Modify: `Api/src/GameHub.Web.Host/Hubs/GameHubMatchHub.cs`
- Modify: `Api/src/GameHub.Web.Host/Hubs/NetworkSignalRHub.cs`

- [ ] **Step 1: Add failing security and metric tests**

Cover malformed event payloads, impossible score/result transitions, duplicate completion, oversized replay metadata, and counters for queue wait, completed matches, abandoned matches, and latency.

- [ ] **Step 2: Implement server-side event validation**

Validate schema, event sequence, payload size, participant ownership, monotonic timestamps, and permitted state transitions. Store only hashes and bounded diagnostic metadata in `MultiplayerSecurityEvent`; never trust client MMR, winner, or final score.

- [ ] **Step 3: Implement metrics and cleanup**

Add meters named `multiplayer.queue.wait_seconds`, `multiplayer.matches.completed`, `multiplayer.matches.abandoned`, and `multiplayer.latency_ms`. Schedule history/replay/security retention cleanup daily.

- [ ] **Step 4: Run tests and commit**

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Multiplayer"
```

Expected: PASS.

Commit:

```bash
git add Api/src/GameHub.Application/Multiplayer Api/src/GameHub.Core/Application Api/src/GameHub.Web.Host Api/test/GameHub.Tests
git commit -m "feat(multiplayer): add validation and matchmaking telemetry"
```

---

### Task 7: Add administrative controls and frontend match browser

**Files:**
- Create: `Api/src/GameHub.Application/Admin/Dto/MultiplayerAdminDtos.cs`
- Modify: `Api/src/GameHub.Application/Admin/IAdminDashboardAppService.cs`
- Modify: `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs`
- Modify: `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs`
- Modify: `Api/src/GameHub.Core/Application/Authorization/GameHubAuthorizationProvider.cs`
- Modify: `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubPermissionSeeder.cs`
- Create: `angular/src/app/core/services/multiplayer.service.ts`
- Create: `angular/src/app/public/match-browser/match-browser.component.ts`
- Create: `angular/src/app/public/match-browser/match-browser.component.html`
- Create: `angular/src/app/public/match-browser/match-browser.component.css`
- Modify: `angular/src/app/app.routes.ts`
- Modify: `angular/public/gamehub-sdk.js`
- Modify: `angular/src/app/core/services/gameplay-bridge.service.ts`

- [ ] **Step 1: Add failing admin/application tests**

Cover aggregation by game/day, permission denial for non-admin users, ending a match, moderating a participant, and returning security audit records without sensitive payloads.

- [ ] **Step 2: Implement admin operations**

Add permission-protected list/close/moderate/audit methods. Closing a room must set `Cancelled`, preserve history, and notify connected clients. Moderation must deactivate a participant and record a security event.

- [ ] **Step 3: Implement public match browser**

Add a paginated, filterable page with game/mode/region/latency/ranked filters, empty/loading/error states, join and spectate actions, and no direct trust of client-provided rating data.

- [ ] **Step 4: Extend bridge and SDK**

Expose `browseMatches`, `enqueueRanked`, `cancelRanked`, `getRankedStatus`, `getMatchHistory`, and `completeMatch`; retain existing `joinLobby`, `signal`, `broadcast`, reconnect, and spectator APIs.

- [ ] **Step 5: Run backend and Angular tests/build and commit**

Run:

```bash
dotnet test Api/GameHub.sln -c Release --no-restore
(cd angular && npm run build)
```

Expected: backend tests pass and Angular build completes with only pre-existing budget warnings.

Commit:

```bash
git add Api/src/GameHub.Application/Admin Api/src/GameHub.Core/Application/Authorization Api/src/GameHub.EntityFrameworkCore/Migrations/Seed angular
git commit -m "feat(multiplayer): add admin controls and match browser"
```

---

### Task 8: Documentation, final verification, and PR

**Files:**
- Modify: `README.md`
- Modify: `README.pt-BR.md`
- Modify: `CHANGELOG.md`
- Modify: `docs/agent-execution-log.md`
- Modify: `.specs/29-poki-multiplayer-browser-ranked.md`

- [ ] **Step 1: Document contracts and operational behavior**

Document public match filters, ranked queue states, server-authoritative result rules, Redis configuration, retention, metrics, and admin permissions in both README variants where applicable.

- [ ] **Step 2: Run the complete verification loop**

Run:

```bash
dotnet build Api/GameHub.sln -c Release --no-restore
dotnet test Api/GameHub.sln -c Release --no-restore
(cd angular && npm run build)
git diff --check
git status --short
```

Expected: build/test exit `0`, frontend build exits `0`, diff check is clean, and only intended files are modified.

- [ ] **Step 3: Review migration and diff**

Run:

```bash
git diff --merge-base origin/main --stat
git diff --merge-base origin/main -- Api/src/GameHub.EntityFrameworkCore/Migrations
```

Confirm no secrets, unrelated generated files, or protected-branch changes.

- [ ] **Step 4: Fetch the PR template, push, and create the PR**

Use the built-in Git tools after committing and pushing the feature branch. The PR body must include the implementation summary and exact verification results.
