# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Multiplayer browser and ranked foundation (Poki 29)**: public match browsing filters, ranked seasons/ratings/queue entries, server-side match history and replay metadata, security audit events, matchmaking metrics, SignalR Redis configuration compatibility, and administrator controls for match closure, participant moderation, and security-event auditing.

- **SignalR / AUDS deepening (Poki 28)**: authenticated game-scoped SignalR connections, reconnect grace periods, spectator rooms, payload validation and rate limits, the `/signalr-network` WebRTC signaling relay, AUDS TTL cleanup and quota-returning saves, multiplayer/AUDS metrics, Hangfire cleanup jobs, the `Poki28` migration, and bridge/SDK helpers for reconnect, spectating, signaling, broadcasting, and arbitrary data.

- **Error Scanner (Poki 26.1)**: `AdminDashboardAppService.GetErrorScannerAsync` aggregates `GameErrorLog` by message/severity and raises health alerts when errors exceed 10/hour; `GameplayBridgeService.reportError` persists errors from the SDK.
- **Daily Playing Users / Conversion Funnel (Poki 26.2)**: `GameMetricSnapshot` adds `DailyPlayingUsers`, `PageViews`, `LoadingStartedCount`, `LoadingFinishedCount`, and `GameplayStartedCount`; `GameMetricsAggregationJob` computes funnel counts; `AdminDashboardAppService.GetConversionFunnelAsync` returns PageView → Loading → Gameplay conversion stages.
- **Player Feedback Analytics (Poki 26.3)**: `UserContent` supports `Rating`; `PlayerFeedbackAnalyticsAppService.GetFeedbackSummaryAsync` returns average rating, distribution, sentiment, and recent comments; `GetHealthAlertsAsync` warns for games with `AverageRating < 3.0` and `ReviewCount >= 10`.
- **Quality Guidelines Gates (Poki 26.4)**: `GameBuildPackageValidator` flags IAP/ads keywords in `index.html`, applies `ProfanityFilter` to titles/descriptions/filenames, detects outgoing links, and computes `QualityScore` on `ValidationSummaryDto`.
- **External Resources & Analytics Exemptions (Poki 26.5)**: `ExternalResourceExemption` entity, `IExternalResourceAppService` for request/review/list, and validator filters against approved domains when a `gameId` is supplied.
- **Thumbnail Guide Enforcement (Poki 26.6)**: `ImageHeaderAnalyzer` parses PNG/JPEG/GIF/WebP headers; thumbnail validation enforces min 640x360, 16:9 aspect ratio, max 2 MB, WebP/PNG/JPEG format, and no text overlays (heuristic).
- **Playtest Difficulty Balancing (Poki 26.7)**: `PlaytestRecording.LevelEvents` JSON column; `PlaytestAppService.GetDifficultyInsightsAsync` aggregates start/death/restart/complete events per level.
- **Player Fit / Retention (Poki 26.8)**: `AdminDashboardAppService.GetPlayerFitAsync` computes 1d/7d/30d retention, stickiness, and category benchmarks from `PlaySession` history.
- **Submission / Approval Workflow (Poki 26.11)**: `GameStatus.Submitted`, `InReview`, `ApprovedForPublishing`, and `Rejected` transitions; `DeveloperGameAppService.SubmitForReviewAsync` and `AdminGameAppService.StartReviewAsync`/`ApproveForPublishingAsync`/`RequestChangesAsync`.
- **Earnings & Ad Reports (Poki 26.12)**: `AdImpression` entity with `Type`, `Provider`, `Country`, `Device`, `Cpm`, and `Earnings`; `AdBreakAppService` records impressions; `DeveloperEarningsAppService.GetAdReportAsync` groups by type, provider, country, and device.
- EF Core migration `Poki26` for all new entities and columns.
- Tests for `AdminDashboardAppService` Error Scanner/Conversion Funnel/Player Fit, `PlayerFeedbackAnalyticsAppService`, `ExternalResourceAppService`, `PlaytestAppService` difficulty insights, `DeveloperEarningsAppService` ad report, and `GameBuildPackageValidator` quality rules.
- **Netlib / Multiplayer (Poki 27.1)**: `Game.SupportsMultiplayer` and `MaxPlayersPerMatch`; `MatchState`, `MatchParticipant`, `MatchStatus`; `IMatchmakingService` and `MatchmakingService` with room-code generation, find-or-create matchmaking, join/leave, auto-start when full, match state updates and end; `IMultiplayerAppService`/`MultiplayerAppService` with HTTP endpoints; SignalR `GameHubMatchHub` at `/signalr-match` exposing `CreateMatch`, `JoinMatch`, `JoinMatchByRoomCode`, `LeaveMatch`, `SendMatchState`, `EndMatch`; Angular `GameplayBridgeService` methods `createMatch`, `joinMatch`, `joinMatchByRoomCode`, `leaveMatch`, `sendMatchState`, `onMatchStateChanged`, with automatic SignalR connection, reconnect and room-group management.
- **Arbitrary User Data Store (Poki 27.2)**: `ArbitraryUserDataRecord` entity; `IArbitraryUserDataAppService`/`ArbitraryUserDataAppService` with `GetAsync`, `SetAsync`, `DeleteAsync`, `GetQuotaAsync`; JSON validation, reserved `gamehub_ignore_*` prefix rejection, 100-key quota, 64 KB/value size limit, and optional TTL; bridge methods `loadArbitrary`/`saveArbitrary`/`deleteArbitrary` exposed through `GameplayAppService`.
- **SignalR infrastructure**: `services.AddSignalR()` in `Startup.cs`, `GameHub.Web.Host/Hubs/GameHubMatchHub.cs` registered with `ITransientDependency`; `@microsoft/signalr` added to `angular/package.json`; bridge `matchHubUrl` at `/signalr-match` with `withAutomaticReconnect`.
- Tests for `MultiplayerAppService` (create, join, join by room code, leave, end, update state, create-or-join), `MatchmakingService` (full match, room code uniqueness, match expiration), `ArbitraryUserDataAppService` (set/get, delete, quota, invalid JSON, reserved prefix), and `GameplayBridgeService` SignalR message handling.
- EF Core migration `Poki27` for `MatchStates`, `MatchParticipants`, `ArbitraryUserDataRecords`, and `Game` multiplayer columns.

- **Image optimization warnings (Poki 25.1)**: `GameHubConsts.ImageOptimizationWarningSizeBytes` (100 KB); `GameBuildPackageValidator` inspects ZIP image entries and emits `ImageOptimizationWarningDto` with estimated savings and WebP recommendation.
- **General Team Settings UI (Poki 25.2)**: `IDeveloperTeamAppService.UpdateGeneralSettingsAsync`/`GetGeneralSettingsAsync`; Angular developer portal `/developer/team` page to edit team name, primary contact email and country; Support role blocked from earnings/metrics in `DeveloperEarningsAppService` and `DeveloperDashboardAppService`.
- **Playtest recordings UI (Poki 25.3)**: `PlaytestRecording` entity with URL, duration, device, country, console output and notes; `IPlaytestAppService.GetRecordingAsync`, `ListRecordingsAsync`, `AddNotesAsync` and `GetAllRecordingsAsync` for moderators; admin Angular page `/app/main/gamehub/playtests` with video player, console output and editable notes.
- **Rewarded ad UX refinada (Poki 25.4)**: green default "Watch for reward" button and non-green "No thanks" button in `GameFrameComponent`; single reward per break; ad-block detection results in no reward; `GameplayBridgeService.setOnRewardedBreak` and `requestRewardedAd`.
- **Onboarding / Easy Access Guide (Poki 25.5)**: `GameMetricSnapshot.OnboardingDropOffRate`; `AdminDashboardAppService.GetOnboardingInsightsAsync` with device-level drop-off and actionable suggestions.
- **Engagement Guide (Poki 25.6)**: `AvgSessionDurationSeconds` and `MedianSessionDurationSeconds` on `GameMetricSnapshot`; `AdminDashboardAppService.GetEngagementInsightsAsync` with category benchmark (120s default) excluding playtest sessions.
- **Revenue share / deal types (Poki 25.7)**: `RevenueContractType.WebExclusive`/`NonExclusive`; `RevenueSplitCalculator` returns 100% to dev on direct traffic, 50/50 when Poki brings traffic, and flat fee for non-exclusive; `TrafficSource.Poki`/`Campaign`; `FlatFeeAmount` on `RevenueContract` and earnings calculation.
- **Performance & FPS (Poki 25.8)**: `measureFps` in `GameplayBridgeService`; FPS aggregation per session into `GameMetricSnapshot.FpsAcceptableSessions`/`FpsTotalSessions`; admin health alert when < 85% users hit 30 FPS per device.
- **Suggested categories & SEO (Poki 25.9)**: `AdminGameAppService.SuggestCategoriesAsync` matching game description to category keywords; `ValidateSeoAsync` checking `SeoDescription` length and `SuggestedDescription` quality.
- **Mystery Tile / Playtest Discovery (Poki 25.10)**: `PlaytestSession.IsDiscovery` and `DisplayProbability`; public `GameCatalogAppService.GetMysteryTileAsync` returning a discovery playtest or fallback published game; Angular home Mystery Tile with recording-consent prompt.
- Tests for `GameBuildPackageValidator` image warnings, `AdminDashboardAppService` onboarding/engagement/playtest exclusion, `AdminGameAppService` SEO/category suggestions, `PlaytestAppService` recordings, `GameCatalogAppService` Mystery Tile, and `RevenueContractAppService` split rules.
- EF Core migration `AddPoki25Phase` covering `PlaytestRecording`, updated `PlaytestSession`/`PlaySession`/`GameMetricSnapshot`/`RevenueContract` columns.

- **P4D v2 teams and billing (Poki 24.1)**: `DeveloperTeam`, `DeveloperTeamMember` with `Developer`/`Support`/`Billing` roles, `IDeveloperTeamAppService` (create, update, get my team, invite/remove/accept members), `DeveloperBillingProfile` linked to a team, and endpoint for the developer to fill billing and mark it pending approval.
- **P4D v2 playtests (Poki 24.2)**: `PlaytestSession` entity, `IPlaytestAppService` with `RequestPlaytestAsync`, `GetPlaytestsByGameAsync`, and `UploadRecordingAsync` for admin/moderator; UI in the developer portal.
- **Inspector v3 QR and warnings (Poki 24.3)**: `/play/:slug?inspector=1&inspectorSession={id}` mobile QR code in the admin inspector, and unexpected-behavior warning category persisted for SDK validation issues.
- **Incognito / first-party UX (Poki 24.4)**: `GameplayBridgeService` localStorage operations wrapped in try/catch with `typeof localStorage` guards, `CloudSaveAppService` returns `{ saved: false, message: "Progresso local apenas" }` on persistence failures, `GameFrameComponent` toast for local-only progress, and `PlayerPreference`/`PlayerPrivacyConsent` localStorage fallbacks in the bridge.
- **Poki CLI parity (Poki 24.5)**: `gamehub.json` contract, `GamehubCliManifest` DTO, `POST /api/services/app/GameBuild/UploadFromCli` with API-key auth tied to `DeveloperProfile`/`DeveloperTeam`, reusing `GameBuildAppService` validation and storage; documented in `docs/gamehub-cli.md`.
- **Versions tab actions (Poki 24.6)**: per-build "Open in Inspector" and "Preview on Game Hub" actions in the developer builds list, backed by `DeveloperGameAppService.CreatePreviewTokenForBuildAsync` and `StartInspectorSessionForBuildAsync`.
- **Poki Pill / mobile overlay (Poki 24.7)**: `movePill(topPercent, topPx)` SDK message, CSS overlay positioned in `GameFrameComponent`, and optional position persistence in `localStorage`.
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
