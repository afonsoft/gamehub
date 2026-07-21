# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Local `docker-compose.infra.yml` (PostgreSQL, Redis, MinIO) and `docker-compose.yml` (API + two frontends) split for flexible local development.
- PostgreSQL initial EF Core migration covering ABP Zero and GameHub entities.
- `ProjectNameDbContextFactory` design-time guard to avoid running `MigrateDatabase` during `dotnet ef` commands.

### Changed

- `Api/Dockerfile` updated to build `GameHub.Web.Host.csproj` and run `GameHub.Web.Host.dll`.
- `scripts/run-local.sh` updated to start infrastructure and application compose files together.

### Fixed

- API Dockerfile no longer references the old `Eaf.ProjectName.Web.Host` template paths.

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

- Renamed the EAF template from `Eaf.ProjectName` to `GameHub` across the solution.
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
