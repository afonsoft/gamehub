# 38 — Roadmap da próxima sessão

> **Status:** Planejamento
> **Base:** Specs 34–37

## Prioridade recomendada

### P0 — Fluxo de publicação

Executar a Spec 35 primeiro. Sem versões, preview e Inspector conectados, o
desenvolvedor não consegue validar com segurança o artefato que será submetido.

### P1 — Shell e estados do portal

Executar a Spec 34. Consolidar navegação, loading, erro, retry e ações antes de
adicionar mais telas.

### P1 — Documentação

Executar a Spec 37 em paralelo com a Spec 35, atualizando o guia conforme cada
etapa do fluxo fica disponível.

### P2 — Analytics e Earnings

Executar a Spec 36 depois que os contratos de período, autorização e agregação
forem confirmados no backend.

## Regras de execução

- Cada spec deve gerar uma branch/PR independente quando houver código.
- Não alterar contratos existentes sem testes de compatibilidade.
- Não implementar billing real ou payout nesta fase.
- Usar dados estimados explicitamente identificados.
- Atualizar `docs/agent-execution-log.md` após cada entrega.

## Definition of Done

- Código e testes implementados para a spec escolhida.
- Build e testes correspondentes executados.
- User Guide atualizado.
- Documentação de limitações atualizada.
- PR revisável com escopo único.
