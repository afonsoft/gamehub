# 51 — Roadmap de execução das próximas sessions

> **Status:** Índice de execução (atualizado em 2026-07-27)
> **Objetivo:** ordenar as entregas por dependência, risco e valor

---

## Entregas concluídas

As specs 46–50 e a integração EAF 9.3.1 foram entregues nos PRs #67–72:

- Spec 46: moderação, segurança e operação.
- Spec 47: analytics, deduplicação e exportação.
- Spec 48: portal, publicação e acessibilidade (base).
- Spec 49: SDK, privacidade e resiliência.
- Spec 50: evoluções no EAF (chat contextual, contratos de notificação, rate limit, auditoria) — publicadas em EAF 9.3.1.

---

## Próxima entrega

### Fase — Poki Parity v3

1. **Spec 52**: hardening operacional, UX do portal, fluxo de publicação, analytics/earnings e documentação.
   - Ver `.specs/52-poki-parity-v3-operacional-ux.md`.
   - Ver `docs/superpowers/plans/2026-07-27-gamehub-poki-parity-v3.md`.

---

## Fora de escopo

- payout e billing real;
- recomendações personalizadas/ML;
- nova persistência de chat no GameHub (usar EAF);
- segundo sistema de amizades, bloqueio ou notificações;
- alteração de workflows sem aprovação;
- backplane obrigatório para ambientes locais;
- exposição de dados privados ao iframe.

---

## Definition of Done

- código e contratos documentados;
- testes de aplicação e frontend;
- build backend e Angular;
- `git diff --check`;
- métricas e runbook;
- validação multi-tenant;
- revisão de segurança.
