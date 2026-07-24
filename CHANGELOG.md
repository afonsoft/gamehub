# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Cloud saves (Poki 22.1)**: `Game.SupportsCloudSaves`, `CloudSaveAppService.DeleteAsync`, `GameplayBridgeService.save()`/`load()` aliases, `gamehub_ignore_` local-only key filtering, and a hint when cloud saves are disabled.
- **User accounts in SDK (Poki 22.2)**: `PlayerAccountAppService.GetPlayerProfileAsync()` returning `{ username, avatarUrl }`; `PlayerAccountAppService.GetTokenAsync()` returning a short JWT with `sub`, `gameId`, `tenantId`, `exp` claims via `IGameTokenProvider`/`GameTokenProvider`; `GameplayBridgeService.login()` redirect, `getUser()`, and `getToken()`.
- **Scroll lock and adaptive controls (Poki 22.4 + 22.5)**: `Game.ControlScheme` enum (`Keyboard`/`Touch`/`Both`) and metadata; `GameFrameComponent` iframe CSS `overscroll-behavior: contain`/`touch-action: none`; focus/blur parent scroll lock; ESC/Space `pauseRequested`/`resumeRequested`; control hints overlay; `Game.CutscenesSkippable` and skip button after 2s.
- **In-game privacy consent (Poki 22.9)**: `PlayerPrivacyConsent` entity, `PrivacyAppService.GetForGameAsync()` and `SaveConsentAsync()` endpoints, `GameplayBridgeService.getPrivacyPolicy()`, and inline consent UI in `GameFrameComponent` when a privacy policy URL is set.
- **Game localization fields**: `Game.DefaultLanguage` and `Game.SupportedLanguages` mapped to DTOs and Angular `GameDetail`/`GameCard` interfaces.
- **Player language preference**: `PlayerPreference` entity, `PlayerAccountAppService.GetLanguageAsync`/`SetLanguageAsync`, `GameplayBridgeService.getLanguage`/`setLanguage`, and in-game language selector on `GameFrameComponent`.
- Tests for `CloudSaveAppService.DeleteAsync`, `PlayerAccountAppService.GetPlayerProfileAsync`/`GetTokenAsync`/`GetLanguageAsync`/`SetLanguageAsync`, and `PrivacyAppService.GetForGameAsync`/`SaveConsentAsync`.
- EF Core migrations `AddPokiCloudSaveAndControls` (new `Game` columns + `PlayerPrivacyConsents`) and `AddPlayerPreference`.

- **Inspector de QA v2**: `InspectorSession`, `InspectorSdkEvent` e `InspectorWarning` entities; `IInspectorAppService` with SDK event validation (duplicates, order, `gameplayStart` before `gameLoadingFinished`, events during ad breaks); scaling tests and warnings; admin page `/app/main/gamehub/inspector/session/:id` with sandboxed iframe, event timeline, and re-run validation.
- **Privacy, UGC and Performance**: `Game.PrivacyPolicyUrl` with publish gate for builds with external requests; `ProfanityFilter` domain service with pt-BR/en-US word lists and leet support; `UserContent` entity with moderation fields and `IUserContentAppService` for comments/reviews; `PlaySession.FpsAverage`/`FpsMin` and `GameMetricSnapshot.AvgFps`/`MinFps`; FPS aggregation and admin health alerts for low FPS.
- **Web Exclusives and discovery**: `Category.Description`/`Keywords` for SEO, `IsWebExclusive` flag on game cards, `GetCategoryBySlugAsync`, and catalog filters for `Exclusivity` and `MinRating`. Angular public home, catalog, detail, and admin category forms updated.
- **Player accounts**: `PlayerFavorite` and `PlayerRecentGame` entities, `IPlayerAccountAppService`, `/player` page with favorites/recent tabs, localStorage fallback for anonymous users, and merge on login.
- **Ad provider integration**: `AdBreakResult` value object, `IAdProvider` returns structured results, `FakeAdProvider` with `SimulateAdBlocked`, `StaticVastAdProvider` example, `ConfigurableAdProvider` routed by `AdBreakOptions.Provider`, and `AdBreakAppService` metrics increment for `PlaySession` and `GameMetricSnapshot`.
- **Registration UX**: password requirements hint and display of API error messages (e.g. password policy).
- Tests for `InspectorAppService`, `ProfanityFilter`, `UserContentAppService`, `AdminGameAppService` publish gate, `BuildPackageValidator` external request detection, `GameplayAppService` FPS aggregation, `PlayerAccountAppService`, `AdBreakAppService`, `GameCatalogAppService` web exclusives/filters, and `FakeAdProvider` ad-block scenarios.
- **Thumbnails and aspect ratio (Poki 23.1 + 23.4)**: `GameThumbnailStatus` enum (`Pending`/`Approved`/`Rejected`), `Game.AnimatedThumbnailUrl` and `Game.ThumbnailStatus`; developer portal drag-and-drop upload for static/animated thumbnails with preview; `AdminGameAppService.ApproveThumbnailAsync`/`RejectThumbnailAsync`; home/catalog cards display animated thumbnail on hover with static fallback when not approved.
- **Inspector v3 (Poki 23.3)**: `InspectorChecklistAnswer` entity with unique `{SessionId, QuestionId}` index; `SaveChecklistAnswerAsync`/`GetChecklistCompletionAsync`; scaling preset selector (640x360, 836x470, 1031x580, portrait/landscape, mobile/tablet/desktop); re-run validation without restarting session; SDK event timeline and checklist completion UI in the admin inspector page.
- **Preview tokens (Poki 23.2)**: `PreviewToken` entity, `IGameTokenProvider.CreatePreviewTokenAsync`, `GamePreviewAppService` with `CreatePreviewTokenAsync` and `[AbpAllowAnonymous] ValidatePreviewAsync`; Angular public route `/preview/:slug/:version` and `GameFrameComponent` preview token validation.
- **Poki quality requirements (Poki 23.4)**: `Game.AspectRatio` enum (`Aspect16x9`/`Aspect4x3`/`Any`) mapped to DTOs and forms; `GameBuildPackageValidator` warnings for `.map` files, `console.log`, `debugger;`, test files and `node_modules` in uploaded zips; warnings for unpacked size > 8 MB and outgoing links/splash screens.
- Tests for `ThumbnailModerationAppService`, `GamePreviewAppService`, and `InspectorAppService` checklist completion.

### Changed

- `GameplayBridgeService` now sends `adBreakMute`/`adBreakUnmute` events around ad breaks, exposes `measureFps` for performance telemetry, and supports inspector mode routing.
- `IAdProvider` method signatures changed from `Task`/`Task<bool>` to `Task<AdBreakResult>`.
- `PlaySession` now tracks `CommercialBreakCount` and `RewardedBreakCount` for reconciliation.
- `UpdateFpsAsync` overwrites session FPS values and aggregates daily `AvgFps`/`MinFps` into `GameMetricSnapshot`.
- `RevenueContract` relationship now points to `Game.RevenueContracts` inverse collection, removing the shadow `GameId1` foreign key.

### Fixed

- Registration screen now surfaces ABP `success: false` API error messages instead of attempting to log in.
- `GameCatalogAppService` web-exclusives logic now loads `Game.RevenueContracts` correctly after EF Core relationship mapping fix.

### Security

- Builds with external requests are blocked from publishing unless a privacy policy URL is provided.
- No custom ad-block messages are displayed; `AdBlocked` flag is propagated to the game without exposing provider details.
- Profanity filtering applied to display names, reports, reviews, and UGC before persistence.

## [0.9.0] - 2026-07-21

### Added

- Admin application services: `AdminDashboardAppService`, `FeatureFlagAppService`, `AuditLogAppService`, `AdminReportAppService`.
- Developer and moderation services: `DeveloperProfileAppService`, `UserReportAppService`.
- `GameBuildsController` for multipart game build uploads (`POST /api/game-builds/{gameId}/upload`).
- GameHub permission hierarchy (`GameHubPermissions`) registered under the existing `Pages` permission.
- `GameHubAdminModule` in Angular Admin with lazy-loaded routes for dashboard, games, moderation, categories, tags, feature flags, and audit log.
- `GameHubAdminService` HTTP proxy service for the new admin endpoints.
- Public Game Hub (`angular/`): Poki-like catalog with hero, search, category chips, large game card grids, game detail page, and fullscreen play frame. No login required.

### Changed

- `Api/Dockerfile` updated to build `GameHub.Web.Host.csproj` and run `GameHub.Web.Host.dll`.
- `scripts/run-local.sh` updated to start infrastructure and application compose files together.
- Dashboard permission renamed from `Pages.Dashboard` to `Pages.GameHubDashboard` to avoid conflict with EAF's built-in dashboard permission.
- `AdminGameAppService.GetAllAsync` and `AuditLogAppService.GetAllAsync` queries typed as `IQueryable<T>` to prevent EF Core include-type mismatches.

### Fixed

- API Dockerfile no longer references the old `Eaf.GameHub.Web.Host` template paths.
- `AuditLogAppService` now resolves `UserName` from `IRepository<User, long>` because `Abp.Auditing.AuditLog` only stores `UserId`.

## [0.9.0] - 2026-07-21

### Added

- **Backend domain model**: `Game`, `GameBuild`, `Category`, `Tag`, `GamePlacement`, `DeveloperProfile`, `PlaySession`, `GameplayEvent`, `GameMetricSnapshot`, `LeaderboardEntry`, `ModerationReview`, and `UserReport`.
- **Domain value objects and enums**: `Slug`, `AgeRating`, `BuildVersion`, `GameStatus`, `GameBuildStatus`, `GameOrientation`, `GameplayEventType`, `DeveloperProfileStatus`, and `ModerationReviewStatus`.
- **Application services**: `GameCatalogAppService`, `DeveloperGameAppService`, `GameBuildAppService`, `GameplayAppService`, `LeaderboardAppService`, `AdminGameAppService`, `ModerationAppService`, `CategoryAppService`, and `TagAppService`.
- **EF Core integration**: `DbSet` declarations for all GameHub entities and `GameHubModelCreatingExtensions` with Fluent API configurations, indexes, and relationships.
- **Cache abstractions**: `IGameCatalogCache` / `ILeaderboardCache` with in-memory implementations (Redis-backed implementations reserved for production).
- **Build upload and validation**: `IGameBuildPackageValidator` and `GameBuildPackageValidator` enforcing ZIP size, SHA-256, `index.html` presence, and blocking of executable extensions.
- **Security middleware**: `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware`, and `RateLimitingMiddleware`.
- **Angular Game Hub** (`angular/`): routes, `GameCatalogService`, `GameplayBridgeService`, `GameFrameComponent`, and `HomeComponent`.
- **Angular Admin** (`angular-admin/GameHub.UI/`): existing EAF admin module plus placeholders for GameHub-specific screens.
- **Docker support**: `Api/Dockerfile`, `angular/Dockerfile`, `angular-admin/GameHub.UI/dockerfile`, `.env.example`, and `docker-compose.yml`.
- **Scripts**: `scripts/build.sh`, `scripts/test.sh`, and `scripts/run-local.sh`.
- **Tests**: xUnit/Shouldly tests for `Game` lifecycle, build validation, leaderboard cache, categories, and moderation.
- **CI/CD**: GitHub Actions workflows for API build/test (`ci-build-test.yml`), Angular builds (`angular-ci.yml`), code quality (`code-quality.yml`), and branch cleanup (`delete-branch-on-merge.yml`).
- **Documentation**: `docs/agent-execution-log.md`, `docs/known-issues.md`, and `docs/specs-improvements.md`.

### Changed

- Renamed the EAF template from `Eaf.GameHub` to `GameHub` across the solution.
- Switched EAF references from local project paths to NuGet packages (`Eaf.Middleware.*` 9.2.0).
- Updated `GameHub.Web.Host` and test projects to use `Eaf.KeyVault.AspNetCore`.

### Fixed

- API and admin build failures caused by missing `LogService` in the `eaf-ng2-module`.
- Broken package references and local EAF paths in `.csproj` files.

### Security

- Added CSP and security headers middleware.
- Added per-IP token-bucket rate limiting.
- Build validation rejects executable files (`*.exe`, `*.dll`, `*.bat`, `*.cmd`, `*.ps1`) and requires `index.html` at the package root.

### Deprecated

- In-memory catalog and leaderboard caches are temporary implementations; Redis-backed replacements will replace them in a future release.

### Removed

- Legacy Angular UI files and old solution files from the original EAF template.
- SQL Server-only EF Core initial migration; replaced by a PostgreSQL-compatible `Initial` migration.

## [0.1.0] - 2026-07-21

### Added

- Initial repository structure based on the EAF/ABP template.
- .NET backend projects (`GameHub.Core`, `GameHub.Application`, `GameHub.EntityFrameworkCore`, `GameHub.Web.Host`, `GameHub.Migrator`).
- Angular Admin UI project with EAF components.
- `LICENSE` and base `README.md`.
