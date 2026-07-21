# GameHub — Known Issues

## EF Core Migrations

- `dotnet-ef` tooling is not installed in the build environment, so an EF Core migration for the GameHub entities has not been generated yet.
- The test suite uses the in-memory database, which does not require migrations.
- Before deploying, run `dotnet ef migrations add GameHub_Initial -p src/GameHub.EntityFrameworkCore -s src/GameHub.Web.Host` after installing the tooling.

## Runtime Caches

- `IGameCatalogCache` and `ILeaderboardCache` are currently implemented as in-memory caches.
- Redis-backed implementations should be added when moving to production.

## Angular Admin

- The `angular-admin` EAF application builds and already contains the standard administration module.
- GameHub-specific administration screens (games, categories, tags, moderation) can be added incrementally as lazy-loaded modules under `src/app/admin`.

## Upload Storage

- `GameBuildAppService` validates and registers build metadata but stores packages in a configurable `OriginalPackageUrl`.
- A concrete `IBlobStorage` provider (MinIO/S3) integration is reserved for a follow-up task.
