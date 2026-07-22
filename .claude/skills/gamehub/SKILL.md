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
