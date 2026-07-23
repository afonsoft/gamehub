# GameHub — Known Issues

## Migrations

- A migration inicial do PostgreSQL (`Migrations/20260721121054_Initial`) foi gerada e substituiu a migration anterior SQL Server-only.
- A API aplica a migration automaticamente em runtime via `GameHubDbContext.MigrateDatabase`.
- Para deploys SQL Server é necessário gerar uma migration específica com `Database__Provider=SqlServer`.

## Caches

- `IGameCatalogCache` e `ILeaderboardCache` possuem implementações Redis (`RedisGameCatalogCache` e `RedisLeaderboardCache`) registradas quando `RedisCache:IsEnabled=true`.
- Em desenvolvimento sem Redis, caem para as implementações in-memory padrão.

## Storage

- `MinioGameAssetStorage` é a implementação concreta de `IGameAssetStorage`. Extraí o ZIP para `builds/{gameId}/{buildId}/` e armazena também o pacote original.
- O endpoint público dos jogos ainda usa o próprio endpoint do MinIO; um domínio dedicado `games.afonsoft.dev` (CDN/proxy) ainda não foi configurado.

## Segurança

- `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware` e `RateLimitingMiddleware` foram removidos do pipeline para resolver erros 504/CORS no admin Angular/EAF.
- JWT ainda usa `localStorage` no frontend. Refresh token `HttpOnly` e movimentação do access token para `sessionStorage` ainda estão pendentes.

## Admin

- As telas GameHub (games, moderação, categorias, tags, uploads, usuários, dashboard, feature flags, audit logs) foram criadas, mas a navegação ainda convive com o menu padrão do template EAF.

## Frontends

- O hub e o admin não possuem design system próprio nem i18n pt-BR/en-US implementados.
- `GameplayBridgeService` dispara `gameplayStart` automaticamente no carregamento; a correta é o jogo disparar no primeiro input do jogador.
