# 50 — Evoluções necessárias no EAF

> **Status:** Especificação para execução em repositório separado
> **Base:** `docs/eaf/gamehub-eaf-improvements.md` e Specs 39–40
> **Prioridade:** P0/P1
> **Dependência:** aprovação e implementação no `afonsoft/EAF`

## Objetivo

Definir as alterações que desbloqueiam capacidades ainda não implementáveis
corretamente somente no GameHub, mantendo o EAF como fonte de verdade.

## P0 — Chat contextual

Evoluir os contratos EAF sem quebrar consumidores existentes:

```csharp
Task<ListResultDto<ChatMessageDto>> GetHistoryAsync(
    GetChatHistoryInput input);

Task MarkReadAsync(MarkChatReadInput input);
```

Adicionar campos opcionais e versionados:

- `ConversationId`;
- `GameId`;
- `MatchId`;
- `ContextType`;
- `ClientMessageId`;
- `SentAtUtc`.

Regras:

- histórico paginado e ordenado por cursor;
- `MarkRead` idempotente;
- deduplicação server-side;
- block/mute aplicado antes de persistir e publicar;
- autorização por tenant e participante;
- contratos antigos continuam aceitos.

## P0 — Distribuição e Data Protection

- documentar e testar backplane SignalR separado de `ICacheManager`;
- provider compartilhado de Data Protection Keys;
- `ApplicationName` estável entre instâncias;
- health checks de banco, cache, Pub/Sub e backplane;
- readiness/liveness com criticidade configurável;
- teste automatizado com duas instâncias.

## P1 — Notificações e capacidades

- metadata de notificação sem PII;
- cursor/reconexão e deduplicação de eventos;
- capabilities por versão;
- preferências e mark-read idempotente;
- contrato público de erros;
- correlação entre logs, traces e métricas.

## P1 — Templates

### `Templates/Api`

- separar cache, Hangfire storage e backplane;
- configuração explícita de CORS/WebSocket;
- Data Protection compartilhado;
- exemplos de rate limit e health checks;
- OpenTelemetry por ambiente;
- XML docs e geração de proxies;
- testes de startup com Redis habilitado/desabilitado.

### `Templates/Angular`

- matriz de compatibilidade Angular/TypeScript/RxJS;
- interceptor de correlation ID e refresh;
- cliente de notificações e chat contextual;
- retry/backoff e reconexão;
- acessibilidade dos componentes existentes;
- testes headless no CI;
- nenhuma edição manual de `service-proxies.ts`.

## Migração e compatibilidade

1. adicionar colunas opcionais;
2. publicar código compatível com schema antigo;
3. backfill assíncrono;
4. habilitar capabilities por ambiente;
5. validar duas instâncias;
6. remover somente após janela de compatibilidade documentada.

## Critérios de aceite

- zero duplicação de persistência no GameHub;
- consumidores antigos compilam;
- migration reversível ou com procedimento de rollback;
- testes de autorização, tenant, deduplicação e reconexão;
- documentação dos templates atualizada no mesmo PR do EAF.
