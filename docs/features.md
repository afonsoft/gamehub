# Funcionalidades — GameHub

## Game Hub (Público)

- Catálogo de jogos com destaques, novidades, mais jogados, tendências, recomendações e categorias.
- Página de jogo com execução em iframe sandbox.
- Busca e filtros por categoria, tag, dispositivo e orientação.
- Leaderboards com Redis Sorted Sets.
- Votação e denúncia de jogos.

## Portal do Desenvolvedor

- Wizard de 5 passos para submissão de jogos.
- Upload de builds HTML5/WebGL (zip/tar).
- Validação: `index.html` obrigatório, tamanho máximo 100 MB, SHA-256, sem executáveis.
- Versionamento imutável (semver).
- Publicação no CDN após aprovação.

## Admin / Moderação

- Fila de revisão de builds (aprovar/rejeitar).
- Publicação/suspensão de jogos.
- Fila de denúncias e histórico auditável.

## Gameplay SDK / Bridge

O jogo no iframe emite 10 eventos principais:

`GameLoadingStarted`, `GameLoadingFinished`, `GameplayStarted`, `GameplayStopped`, `CommercialBreakRequested`, `CommercialBreakCompleted`, `RewardedBreakRequested`, `RewardedBreakCompleted`, `GameErrorCaptured`, `GameMeasuredEvent`.

## Segurança e Observabilidade

- JWT/OIDC, RBAC via ABP.
- CSP, security headers, rate limiting.
- Serilog estruturado, OpenTelemetry, CorrelationId.
