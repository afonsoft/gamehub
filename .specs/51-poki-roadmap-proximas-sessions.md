# 51 — Roadmap de execução das próximas sessions

> **Status:** Índice de execução
> **Objetivo:** ordenar as Specs 46–50 por dependência, risco e valor

## Ordem recomendada

### Fase 1 — Segurança e dados confiáveis

1. Spec 46: moderação, segurança e operação.
2. Spec 47: analytics, deduplicação e exportação.
3. Validar migrations, ownership e métricas.

### Fase 2 — Produto e experiência

4. Spec 48: portal, publicação e acessibilidade.
5. Spec 49: SDK, privacidade e resiliência.
6. Executar testes Angular e validação manual desktop/mobile.

### Fase 3 — Evolução do EAF

7. Abrir branch/repositório separado para Spec 50.
8. Implementar chat contextual e contratos de notificação no EAF.
9. Atualizar templates API/Angular do EAF.
10. Integrar no GameHub por feature flags.

## Fora de escopo

- payout e billing real;
- nova persistência de chat no GameHub;
- segundo sistema de amizades, bloqueio ou notificações;
- alteração de workflows sem aprovação;
- backplane obrigatório para ambientes locais;
- exposição de dados privados ao iframe.

## Definition of Done

- código e contratos documentados;
- testes de aplicação e frontend;
- build backend e Angular;
- `git diff --check`;
- métricas e runbook;
- validação multi-tenant;
- revisão de segurança;
- PR separado para alterações no EAF.
