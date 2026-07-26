# Prompt 29 — Match browser e multiplayer ranqueado

## Objetivo

Evoluir o multiplayer do GameHub com descoberta pública de salas, matchmaking
ranqueado e controles de competição sem acoplar o jogo à infraestrutura.

## Escopo futuro

- Navegador público de partidas com filtros por jogo, modo, região e latência.
- Filas ranqueadas com MMR, temporadas, colocação e proteção contra abandono.
- Histórico de partidas, replay metadata e leaderboard por jogo/modo.
- Anti-cheat defensivo baseado em validação de eventos e anomalias.
- Escala distribuída do signaling com Redis backplane e presença multi-instância.
- Métricas de matchmaking: tempo de fila, taxa de conclusão, abandono e latência.
- Ferramentas administrativas para encerrar salas, moderar participantes e auditar sinais.

## Restrições

- Manter `IMatchmakingService` como backend das filas e salas.
- Preservar isolamento multi-tenant e autorização por token escopado ao jogo.
- Não confiar no cliente para MMR, resultado ou pontuação final.
- Manter compatibilidade com o bridge atual e WebRTC quando disponível.
