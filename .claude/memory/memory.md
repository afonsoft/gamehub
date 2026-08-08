# memory.md — short-term memory

Working state of the current session. Overwritten on every update.
Durable records belong in `.claude/memory/{YYYYMMDD}-memory.md`.

- Last verified base commit: `a35b5c1` (`main` — PR #126 EAF 9.4.4 alignment already merged)
- Active branch: `devin/readme-badges` (README badge updates pushed)
- Active work item: Análise e triagem dos PRs abertos do gamehub contra EAF 9.4.4; README badges
- EAF 9.4.4 alignment: already present in `main` via PR #126 (`common.props`, `.csproj` packages, `Startup.cs` Local CORS, `GameHubDbContext` decimal precision, ngsw-config, topbar). Missing: `angular-admin/GameHub.UI/package-lock.json` mismatch introduced by #126 (`@angular/compiler` 20.3.26 in lock vs 20.3.27 in package.json).
- PR triage:
  - #128 approved/suggested first merge (fixes lock + updates dompurify)
  - #113 approved but partially superseded by #128
  - #127 blocked by stale lock (not hono); needs rebase after lock fix
  - #125/#124/#123/#122/#121 checks passed but need rebase/re-run after lock fix
  - #14 rejected/closed (Angular major version incompatibility)
  - #3 rejected/closed (wrong project path and conflicts)
- Blocker: `gh pr merge` into `main`/`master` is blocked in this environment; merges and rejections done via comments/closures; user must click merge for approved PRs.
- Last updated: 20260808
