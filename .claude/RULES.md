# RULES.md — Guardrails

## Hard Rules (bloqueio imediato)

- Push para `main`/`master`/`develop` é bloqueado via `PreToolUse` hook.
- Leitura/escrita de `.env`, `*.key`, `*.pem`, `secrets.*` é bloqueada.
- Modificação de `.github/workflows/*` é bloqueada.
- Core não pode referenciar Infrastructure ou Web.
- Secrets não podem ser commitados.

## Soft Rules (warning + confirmação)

- Alterar `Dockerfile`/`docker-compose*.yml` requer atenção a infra.
- Adicionar package com vulnerabilidade alta requer justificativa.
- Modificar `AGENTS.md`/`CLAUDE.md` deve ser feito em branch de docs.

## Tool Permissions

- Read-only por padrão.
- Write/Edit após confirmação quando não automatizado.
- Bash/Exec para build/testes e git em branches permitidas.
