# 11 - Backlog Sugerido por Fases

## Cadência

- Sprints de 2 semanas.
- MVP = Fase 0 + Fase 1 + Fase 2 (8-10 semanas estimadas).
- Release candidate = Fase 3 + Fase 4.
- Production ready = Fase 5 + Fase 6.
- Monetização = Fase 7 (pós-lançamento).

## Dependências entre fases

| Fase | Depende de |
|------|------------|
| Fase 0 | nenhuma |
| Fase 1 | Fase 0 |
| Fase 2 | Fase 0, Fase 1 |
| Fase 3 | Fase 0 |
| Fase 4 | Fase 2, Fase 3 |
| Fase 5 | Fase 1, Fase 2 |
| Fase 6 | Fase 1, Fase 2, Fase 4 |
| Fase 7 | Fase 1, Fase 2 |

## Estimativa de esforço

| Fase | Duração estimada |
|------|------------------|
| Fase 0 | 1-2 semanas |
| Fase 1 | 2-3 semanas |
| Fase 2 | 2-3 semanas |
| Fase 3 | 2 semanas |
| Fase 4 | 1-2 semanas |
| Fase 5 | 1-2 semanas |
| Fase 6 | 1 semana |
| Fase 7 | 2-3 semanas |

## Fase 0 - Fundação técnica

- Criar repo/branch.
- Aplicar template legado AspZero/EAF (Dockerfiles já existem para API e admin).
- Configurar DNS de produção: `gamehub.afonsoft.dev` (hub), `gamehub-admin.afonsoft.dev` (admin), `gamehub-api.afonsoft.dev` (API). Servidor já possui PostgreSQL e Redis gerenciados.
- Configurar Docker Compose local com banco e Redis (PostgreSQL 16, Redis 7; em produção, usar os serviços gerenciados do servidor).
- Configurar `.env.example`.
- Criar documentação inicial.
- Configurar logs estruturados e correlation id (Serilog).
- Configurar health checks.
- Verificar DNS (gamehub.afonsoft.dev, gamehub-admin.afonsoft.dev, gamehub-api.afonsoft.dev).

## Fase 1 - Catálogo público MVP

- Entidade Game (ver `04-modelagem-dados.md`).
- Categorias/tags.
- Home pública (game hub).
- Página de detalhe (game hub).
- Busca básica com full-text search (PostgreSQL).
- Seed de jogos fake para desenvolvimento.

## Fase 2 - Player e Game Shell

- Game iframe sandbox.
- PlaySession.
- Eventos de gameplay (10 tipos, ver `04-modelagem-dados.md`).
- Loading/start/stop/error.
- Leaderboard básico (Redis Sorted Sets).

## Fase 3 - Developer Portal

- Developer profile.
- Draft game.
- Upload de build.
- Validação de zip.
- Status de submissão.

## Fase 4 - Moderação e publicação

- Queue de revisão (admin).
- Aprovar/reprovar build (admin).
- Publicar jogo (admin).
- Suspender jogo (admin).
- Reports de usuário.

## Fase 5 - Observabilidade e analytics

- Métricas agregadas (GameMetricSnapshot).
- Jobs Hangfire.
- Dashboard administrativo (admin).
- Trending/recommendations simples.

## Fase 6 - Hardening

- CSP avançada.
- Subdomínio isolado para jogos.
- Scan de build.
- LGPD: export/delete user data.
- Rate limiting completo.
- CORS validado.

## Fase 7 - Monetização

- Interface de ads provider (IAdProvider).
- Commercial break.
- Rewarded break.
- Relatórios de receita.
- Revenue share para desenvolvedor.
