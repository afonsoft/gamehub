# 46 — Moderação, segurança e operação sem alteração do EAF

> **Status:** Especificação para execução
> **Base:** Specs 39–45 e PRs 67–68
> **Prioridade:** P0
> **Dependências:** nenhuma alteração no EAF

## Objetivo

Consolidar a proteção das APIs e do SDK do GameHub usando os contratos atuais do
EAF para identidade, autorização, cache, auditoria e block/unblock.

## Escopo

### Backend

Alterar ou criar:

```csharp
Task<UserContentDto> SubmitAsync(SubmitUserContentInput input);
Task<UserReportDto> SubmitAsync(UserReportInput input);
Task<ModerationReviewDto> ModerateAsync(ModerateUserContentInput input);
Task ReportPlayerAsync(ReportPlayerInput input);
```

- aplicar limites de tamanho e normalização antes do filtro;
- separar `validation_failed`, `rate_limited`, `not_authorized` e
  `temporarily_unavailable`;
- rate limit por tenant, usuário, jogo e operação;
- tornar reports idempotentes com `ClientRequestId` opcional;
- impedir auto-report e reports para jogos inexistentes;
- validar ownership/tenant em todas as ações de desenvolvedor;
- não aceitar identidade, tenant ou connection ID vindos do iframe;
- manter block/unblock como fachada dos endpoints EAF.

### Segurança de telemetria

- validar `SessionId`/`GameId`/`BuildId` antes de persistir;
- rejeitar payload com secrets, tokens, cookies, e-mails ou texto de chat;
- limitar cardinalidade de `EventName`, `Source` e dimensões;
- aplicar retenção configurável para logs de erro e eventos brutos;
- registrar `CorrelationId`, sem registrar payload sensível.

### Operação

- métricas de rejeições, rate limits, reports abertos e tempo de resolução;
- health check para banco, cache e storage já fornecidos pelo EAF;
- runbook para fallback de Redis e limpeza de chaves expiradas;
- alertas para aumento de reports, erros de validação e falha de cache.

## Testes obrigatórios

- isolamento entre tenants;
- usuário anônimo e autenticado;
- ownership de jogo/build;
- auto-report e jogo inexistente;
- repetição com a mesma chave idempotente;
- payload sensível e payload acima do limite;
- rate limit no limite e após expiração;
- fallback de cache sem exposição de dados.

## Critérios de aceite

1. Nenhum endpoint aceita identidade do cliente como fonte de autorização.
2. Todos os erros públicos usam o contrato `SdkError`.
3. Nenhuma entidade de chat, amizade, notificação ou auditoria é duplicada.
4. Métricas operacionais não contêm PII.
5. Testes de aplicação e build passam sem warnings novos.
