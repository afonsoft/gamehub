# Documentação da API — GameHub

## Visão Geral

A API REST do GameHub está disponível em `Api/src/GameHub.Web.Host/`, utiliza ASP.NET Core com EAF/ABP e é documentada via Swagger/OpenAPI.

## Swagger

Quando executada localmente em ambiente de desenvolvimento, a documentação Swagger está em:

```
https://localhost:5000/swagger
```

## Módulos Principais

| Módulo | Controller / AppService | Descrição |
|--------|--------------------------|-----------|
| Catalog | `GameCatalogAppService` | Catálogo público de jogos |
| Developer | `DeveloperGameAppService` | Gestão de jogos do desenvolvedor |
| Builds | `GameBuildAppService` | Upload e versionamento de builds |
| Moderation | `ModerationAppService` | Fila de moderação |
| Admin | `AdminGameAppService` | Publicação/suspensão de jogos |
| Leaderboard | `LeaderboardAppService` | Leaderboards Redis |

## Autenticação

- JWT Bearer via `/api/Account/Authenticate` (padrão ABP).
- Permissões RBAC definidas em `GameHub.Application.Shared`.

## Eventos do Gameplay

O jogo publica eventos via `GameplayAppService`:

- `POST /api/services/app/Gameplay/Event` — eventos do SDK.

Mais detalhes sobre os eventos em `docs/features.md`.
