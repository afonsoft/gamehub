# WORKFLOWS.md — GameHub

## Verification Loop

Toda execução deve passar pelo seguinte loop:

```
Agent Output
  → Lint / dotnet format
  → Build: dotnet build Api/GameHub.sln
  → Testes: dotnet test Api/GameHub.sln
  → Docker Compose config (se alterado)
  → CI (GitHub Actions)
  → Revisão humana (PR)
```

## Fluxo de Feature

1. Receber requisito.
2. Carregar `CLAUDE.md` + `.claude/rules/global-rules.md` + skills relevantes.
3. Criar plano (sub-agent `plan` se complexo).
4. Implementar em branch `feature/*`.
5. Adicionar/executar testes.
6. Atualizar `docs/agent-execution-log.md`.
7. Commit com Conventional Commits.
8. Não abrir PR automaticamente; informar branch ao usuário.

## Rollback

- Reverter último commit com `git revert` em branch de feature.
- Nunca fazer `git reset --hard` em código não versionado.

## CI/CD

- `ci-build-test.yml`: build e testes .NET em PR/push.
- `angular-ci.yml`: builds Angular Hub e Admin.
- `code-quality.yml`: Qodana, SonarQube, Snyk e métricas.
