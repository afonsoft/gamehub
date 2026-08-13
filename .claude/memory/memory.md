# memory.md — short-term memory

Working state of the current session. Overwritten on every update.
Durable records belong in `.claude/memory/{YYYYMMDD}-memory.md`.

- Last verified commit: 561315d (`devin/ajuste-layout-developer`)
- Active branch: `devin/ajuste-layout-developer`
- Active work item: Ajustar layout das telas `/developer` no frontend Angular e adicionar link para `gamehub-admin`.
- Changes made:
  - `environment.ts` e `environment.prod.ts`: adicionado `adminUrl`.
  - `developer-shell.component.{ts,html,css}`: exposto `adminUrl` e adicionado link "GameHub Admin" na sidebar.
  - `dashboard.component.{ts,html,css}`: reescrito layout, adicionado botão "GameHub Admin" e tabela rolável.
  - `earnings.component.{html,css}`: removido `.dev-page`, reescrito estilos responsivos.
  - `team.component.css` e `profile.component.css`: estilos claros e responsivos.
  - `games/builds/metrics.component.html`: removido wrapper `.dev-page` herdado.
  - `angular.json`: ajustado budget de `anyComponentStyle`.
- Validation:
  - `npm run build` no `angular` concluído sem erros.
  - `npx ng test --watch=false --browsers=ChromeHeadless` passou 8/8.
- Uncommitted files: nenhum.
- Blockers / out-of-scope findings: nenhum.
- Next action: aguardar orientação do usuário para abrir PR ou realizar mais ajustes.
- Last updated: 20260813
