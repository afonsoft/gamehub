# GameHub — Known Issues

## EF Core Migrations

- The PostgreSQL initial migration (`Migrations/20260721121054_Initial`) was generated and replaces the previous SQL Server-only migration.
- At runtime the API applies the migration automatically via `GameHubDbContext.MigrateDatabase`.
- For SQL Server deployments a provider-specific migration must be generated with `Database__Provider=SqlServer`.

## Runtime Caches

- `IGameCatalogCache` and `ILeaderboardCache` are currently implemented as in-memory caches.
- Redis-backed implementations should be added when moving to production.

## Angular Admin

- The `angular-admin` EAF application builds and already contains the standard administration module.
- GameHub-specific administration screens (games, categories, tags, moderation) can be added incrementally as lazy-loaded modules under `src/app/admin`.

## Upload Storage

- `GameBuildAppService` validates and registers build metadata but stores packages in a configurable `OriginalPackageUrl`.
- A concrete `IBlobStorage` provider (MinIO/S3) integration is reserved for a follow-up task.
