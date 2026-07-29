---
name: gamehub
description: >
  What: fornece contexto e padrões específicos da plataforma GameHub.
  When: ao trabalhar com domínio, application services, EF Core, Angular ou Docker do GameHub.
  Do NOT: use para dúvidas genéricas de C# ou Angular fora do contexto GameHub.
tools: Read, Grep, Glob
triggers: gamehub, eaf, abp, game, gamebuild, leaderboard, moderation
---

# GameHub Skill

Use este skill quando for implementar ou alterar partes da plataforma GameHub.

## Domínio

- Aggregate roots: `Game`, `DeveloperProfile`.
- Entities: `GameBuild`, `Category`, `Tag`, `PlaySession`, `GameplayEvent`, `LeaderboardEntry`, `ModerationReview`, `UserReport`.
- Value objects: `Slug`, `AgeRating`, `BuildVersion`.
- Enums: `GameStatus`, `GameBuildStatus`, `GameOrientation`, `GameplayEventType`, `DeveloperProfileStatus`, `ModerationReviewStatus`.

## Regras de Negócio

- Um jogo só pode ser publicado se estiver `InReview` ou `Draft` e tiver um build `Approved`.
- Build só pode ser publicado se estiver `Approved`.
- Uploads de builds devem conter `index.html` na raiz, ter no máximo 100 MB e não conter executáveis.

## Caminhos Importantes

- Domain: `Api/src/GameHub.Core/Domain/`
- Application: `Api/src/GameHub.Application/`
- EF Core config: `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs`
- Web Host: `Api/src/GameHub.Web.Host/Startup/`
- Angular Hub: `angular/src/app/`
- Angular Admin: `angular-admin/GameHub.UI/src/app/`
- Specs: `.specs/`

## Exemplos

- Criar entidade: herdar `FullAuditedEntity<Guid>` ou `FullAuditedAggregateRoot<Guid>`, implementar `IMayHaveTenant`.
- Application service: herdar `ApplicationService` ou `AsyncCrudAppService<...>`, usar `IRepository<T>`.
- EF config: adicionar `DbSet` em `GameHubDbContext` e configurar em `GameHubModelCreatingExtensions`.

## Testes End-to-End (EAF/ABP + Angular)

- Subir infra: `docker compose -f docker-compose.infra.yml up -d` (postgres, redis, minio).
- Rodar migrações e seed: execute `GameHub.Migrator` a partir do diretório `Api/src/GameHub.Migrator` com:
  ```
  EafMigrator=LOCAL
  Database__Provider=PostgreSQL
  ConnectionStrings__LOCAL="Host=localhost;Port=5432;Database=gamehub;Username=gamehub;Password=change-me"
  ```
- O seed cria `admin/123qwe` com `ShouldChangePasswordOnNextLogin=true`; para testar login sem tela de reset, desative via SQL:
  `UPDATE "AbpUsers" SET "ShouldChangePasswordOnNextLogin"=false WHERE "UserName"='admin';`
- Backend: `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://+:8001 dotnet .../GameHub.Web.Host.dll`.
- O `appconfig.Local.json` do admin (`angular-admin/GameHub.UI/src/assets`) vem com `appBaseUrl` em `http://127.0.0.1:4602`. O `ng serve` pode vincular a IPv6 (`::1:8000`), então para testes locais altere temporariamente `appBaseUrl` para `http://localhost:8000` e `remoteServiceBaseUrl` para `http://localhost:4601`.
- O `ng serve` do admin pode falhar com BOM no mapa de origem (`devtools-ignore-plugin.js`). Workaround: remover `\uFEFF` e tratar `JSON.parse` no arquivo de plugin.
- Para `npm run test` no public, o Chrome do Devin pode ser um wrapper; use um script wrapper apontando para o binário real com `--no-sandbox` e defina `CHROME_BIN`.
- Testes unitários do public (`DeveloperGamesComponent`/`DeveloperEarningsComponent`) podem falhar com `NG0201: No provider found for _HttpClient` — normalmente requerem providers de teste (`provideHttpClientTesting()` / `TranslateModule.forRoot()` / `I18nService`) no `TestBed`.
- Login public: com apenas um tenant (`Default`), o fluxo de two-step pode pular a tela `/select-tenant` e fazer login automático.
- Para verificar SignalR por query string, use o token `Eaf.AuthToken` do cookie, chame `POST /{hub}/negotiate?access_token=...` e abra o WebSocket com `?access_token=...&id=<connectionId>&negotiateVersion=0`.
- Selectores úteis para Playwright: public login `#userName`/`#password`, admin login `#userNameOrEmailAddress`/`#Password`, tabelas `p-table`, badges `app-status-badge`, empty `app-empty-state`, filtros `#TenantNameOrTenancyCode` e `#UsersFilterText`.
