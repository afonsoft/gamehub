# Social, moderation, analytics and operations Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the next social SDK, contextual moderation, analytics, portal-quality and SignalR operational capabilities in one compatible GameHub branch.

**Architecture:** Reuse EAF chat, friendships, cache and SignalR infrastructure. GameHub owns only game/match authorization, contextual invitations, moderation policy, SDK adaptation and developer-facing analytics. Persist new game-scoped invitations/reports through GameHub entities; keep presence ephemeral in the existing cache store and keep the Redis backplane optional.

**Tech Stack:** .NET 10, ASP.NET Boilerplate, EF Core, PostgreSQL/SQL Server, Redis/ICacheManager, SignalR, Angular 20, xUnit, Shouldly.

---

### Task 1: Contextual social contracts and persistence

**Files:**
- Create: `Api/src/GameHub.Core/Domain/Social/GameInvite.cs`
- Create: `Api/src/GameHub.Core/Domain/Social/GameNotification.cs`
- Create: `Api/src/GameHub.Application/Social/GameSocialDtos.cs`
- Create: `Api/src/GameHub.Application/Social/IGameSocialAppService.cs`
- Create: `Api/src/GameHub.Application/Social/GameSocialAppService.cs`
- Modify: `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`
- Test: `Api/test/GameHub.Tests/GameHub/Application/GameSocialAppService_Tests.cs`

- [ ] Write failing tests for invite authorization, expiration, tenant isolation, notification read state and profile-safe responses.
- [ ] Run the focused tests and confirm they fail because the social contracts do not exist.
- [ ] Implement the smallest tenant-aware entities and app-service methods:
  `GetPublicProfileAsync`, `GetPresenceAsync`, `GetNotificationsAsync`,
  `MarkNotificationReadAsync`, `InvitePlayerAsync`, and `AcceptInviteAsync`.
- [ ] Add DbSets and a migration only after tests pass against the test database.
- [ ] Add authorization checks requiring authenticated users and active match/game membership.

### Task 2: Presence and SDK capability integration

**Files:**
- Modify: `Api/src/GameHub.Application/Multiplayer/IMultiplayerPresenceStore.cs`
- Modify: `Api/src/GameHub.Web.Host/Multiplayer/CacheMultiplayerPresenceStore.cs`
- Modify: `angular/src/app/core/services/gameplay-bridge.service.ts`
- Test: `Api/test/GameHub.Tests/GameHub/Application/GameSocialAppService_Tests.cs`
- Test: `angular/src/app/core/services/gameplay-bridge.service.spec.ts`

- [ ] Add a user-scoped cache index with TTL cleanup and tenant-qualified keys.
- [ ] Add presence lookup returning only `online`, `away` or `offline`.
- [ ] Add bridge methods/events for capabilities, public profile, presence and notifications.
- [ ] Return `feature_disabled`/`not_authorized` contracts without exposing tenant, email, token or internal claims.

### Task 3: Chat moderation, rate limiting and contextual limitations

**Files:**
- Modify: `Api/src/GameHub.Application/Chat/GameChatAppService.cs`
- Modify: `Api/src/GameHub.Application/Chat/GameChatDtos.cs`
- Create: `Api/src/GameHub.Application/Chat/GameChatModerationService.cs`
- Test: `Api/test/GameHub.Tests/GameHub/Application/GameChatAppService_Tests.cs`
- Modify: `angular/src/app/core/services/gameplay-bridge.service.ts`

- [ ] Add failing tests for spam rate limit, repeated-message blocking, report authorization and safe SDK errors.
- [ ] Implement cache-based per-user/game/conversation limits and reuse the existing profanity/content policy.
- [ ] Add a report contract without logging full message text or returning moderation claims.
- [ ] Keep match history/mark-read disabled until EAF exposes contextual message metadata; return a stable `feature_disabled` error.

### Task 4: Analytics and developer portal quality

**Files:**
- Modify: `Api/src/GameHub.Application/Gameplay/GameMetricsAppService.cs`
- Modify: `Api/src/GameHub.Application/Developer/DeveloperEarningsAppService.cs`
- Modify: `angular/src/app/developer/earnings/earnings.component.ts`
- Modify: `angular/src/app/developer/games/games.component.ts`
- Test: `Api/test/GameHub.Tests/GameHub/Application/DeveloperEarningsAppService_Tests.cs`
- Test: `Api/test/GameHub.Tests/GameHub/Application/GameMetricsAppService_Tests.cs`

- [ ] Add tests for duplicate event aggregation, country/device filters, empty periods and tenant ownership.
- [ ] Implement deterministic deduplication and explicit UTC period handling.
- [ ] Add CSV export only if the existing API contract supports it; otherwise document it as deferred.
- [ ] Add component-level tests for status filters, retry, date validation and expandable daily detail.

### Task 5: Operational validation and documentation

**Files:**
- Modify: `Api/src/GameHub.Web.Host/Controllers/SignalRHealthController.cs`
- Modify: `Api/src/GameHub.Web.Host/Multiplayer/SignalRBackplaneSettings.cs`
- Modify: `docs/agent-execution-log.md`
- Create: `docs/runbooks/signalr-redis-backplane.md`

- [ ] Add tests for disabled Redis, missing connection string, channel prefix isolation and enabled backplane configuration.
- [ ] Document two-instance validation using the existing optional `AddStackExchangeRedis` configuration.
- [ ] Run `git diff --check`, backend build/tests, Angular build and focused tests.
- [ ] Commit once with a Conventional Commit, push the branch and open one PR.
