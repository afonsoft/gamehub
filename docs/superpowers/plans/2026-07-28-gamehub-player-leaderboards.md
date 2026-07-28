# Plano de Execução — Páginas do Jogador e Leaderboards Adicionais

## Objetivo e contexto

Complementar as páginas já existentes (`/player` e `/leaderboard/:gameId`) com navegação acessível, uma lista geral de leaderboards (`/leaderboards`) e pequenos aprimoramentos no perfil do jogador, sem alterar o backend.

## Arquivos e módulos afetados

- `angular/src/app/app.html` e `angular/src/app/app.ts`: links `/player` (quando logado) e `/leaderboards` no header/footer.
- `angular/src/app/app.routes.ts`: rota `/leaderboards`.
- `angular/src/app/public/leaderboards/leaderboards.component.ts/.html/.css` (novo): lista de jogos com CTA para leaderboard individual.
- `angular/src/app/public/player/player.component.ts/.html/.css`: stats rápidos, mensagem de sync e melhorias visuais.
- `angular/public/i18n/en-US.json` e `pt-BR.json`: chaves `nav.player`, `nav.leaderboards`, `leaderboards.*`, `player.*`.
- `docs/agent-execution-log.md`: registro da execução.

## Estratégia

1. Criar branch `feature/devin-20260728-gamehub-player-leaderboards` a partir de `main`.
2. Adicionar rotas e navegação.
3. Implementar `LeaderboardsComponent` consumindo `GameCatalogService.getAll()` e linkando para `/leaderboard/<slug>`.
4. Aprimorar `PlayerComponent` com header, stats e estado vazio i18n.
5. Atualizar traduções.
6. Build e testes.

## Riscos e mitigações

- `GameCatalogService.getAll()` pode retornar muitos jogos; manter limitação de página/catalog e usar lazy loading futuramente.
- `leaderboard/:gameId` usa slug como parâmetro; manter compatibilidade.

## Validação

- `npm run build` em `angular/` e `angular-admin/GameHub.UI/`.
- `dotnet test Api/GameHub.sln -c Release` (para garantir que nada foi alterado no backend).
- `npm test` se ChromeHeadless disponível.
