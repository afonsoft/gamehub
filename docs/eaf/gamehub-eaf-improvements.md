# Evoluções necessárias no EAF para o GameHub

## Objetivo

Registrar, separadamente do código do GameHub, os contratos e capacidades que
devem evoluir no EAF para que o GameHub continue reutilizando identidade,
cache, notificações, amizades, chat, auditoria e SignalR sem criar
implementações paralelas.

Esta documentação não altera o repositório `afonsoft/EAF` e não autoriza
mudanças incompatíveis nos contratos compartilhados.

## Prioridade 1 — Chat contextual por partida

### Lacuna atual

O contrato de `ChatMessage` do EAF não transporta `MatchId` nem uma metadata
contextual de conversa. O GameHub consegue autorizar o envio contextual, mas
não consegue consultar histórico ou estados de leitura isolados por partida.

### Evolução proposta

Adicionar metadata opcional e retrocompatível ao contrato de chat:

```csharp
public class ChatMessage
{
    public Guid? ConversationId { get; set; }
    public Guid? GameId { get; set; }
    public Guid? MatchId { get; set; }
    public string ContextType { get; set; }
}
```

Evoluir os contratos existentes sem criar uma segunda entidade de mensagens:

```csharp
Task<ListResultDto<ChatMessageDto>> GetHistoryAsync(
    GetChatHistoryInput input);

Task MarkReadAsync(
    MarkChatReadInput input);
```

Requisitos:

- `MatchId` opcional para manter clientes antigos compatíveis;
- filtros de histórico por `TenantId`, `GameId`, `MatchId` e período;
- índices compostos para consulta contextual;
- `markRead` limitado ao usuário autenticado e à conversa autorizada;
- preservação do endpoint/hub `/signalr-chat`;
- nenhuma persistência de mensagens no GameHub.

## Prioridade 2 — Notificações reutilizáveis

### Lacuna atual

O GameHub possui uma entidade específica para algumas notificações sociais,
enquanto o EAF já fornece `INotificationPublisher`,
`INotificationStore` e `INotificationAppService`.

### Evolução proposta

Permitir que módulos consumidores registrem definições e payloads de
notificação sem duplicar armazenamento:

```csharp
public interface INotificationPublisher
{
    Task PublishAsync(
        string notificationName,
        NotificationData data,
        UserIdentifier[] userIds);
}
```

Adicionar suporte documentado para:

- payload JSON versionado;
- `GameId`, `MatchId` e `InviteId` como metadata;
- severidade e expiração;
- leitura individual e em lote;
- entrega SignalR para usuários online;
- fallback persistido para usuários offline;
- isolamento por tenant.

O GameHub deve migrar gradualmente sua leitura/escrita de notificações sociais
para esses contratos, mantendo DTOs públicos próprios somente como fachada
compatível do SDK.

## Prioridade 3 — Amizades, block e mute

### Capacidades já existentes

O EAF já possui `FriendshipAppService`, `BlockUserInput`,
`UnblockUserInput`, cache de amizades e comunicação SignalR.

### Evolução proposta

Documentar e estabilizar contratos para módulos consumidores:

```csharp
Task BlockUser(BlockUserInput input);
Task UnblockUser(UnblockUserInput input);
Task<ListResultDto<FriendshipDto>> GetFriends(
    GetFriendsInput input);
```

Para o contexto de jogos, considerar uma operação de mute com escopo
independente do block:

```csharp
Task MuteUser(MuteUserInput input);
Task UnmuteUser(UnmuteUserInput input);
```

Requisitos:

- não expor e-mail, claims internas, IP ou connection ID ao iframe;
- bloquear entrega de chat conforme a relação efetiva;
- definir precedência entre block, mute e amizade;
- suportar tenant host/tenant explicitamente;
- emitir eventos de alteração para invalidar caches;
- manter operações idempotentes.

## Prioridade 4 — Rate limiting compartilhado

### Lacuna atual

O GameHub implementa limites específicos sobre `ICacheManager`, mas os limites
de chat, notificações e amizades não possuem uma política comum no EAF.

### Evolução proposta

Criar uma abstração reutilizável:

```csharp
public interface IRateLimitManager
{
    Task<RateLimitDecision> CheckAsync(
        string policy,
        string subject,
        TimeSpan window,
        int limit);
}
```

`RateLimitDecision` deve informar apenas dados operacionais seguros:

- permitido ou bloqueado;
- limite;
- consumo atual;
- segundos até retry;
- identificador de política.

Requisitos:

- provider baseado no cache configurado pelo EAF;
- operação atômica quando Redis estiver disponível;
- fallback explícito quando o provider não for distribuído;
- chaves tenant-aware;
- suporte a idempotency key antes da contagem;
- métricas e logs estruturados sem conteúdo privado.

## Prioridade 5 — Auditoria de moderação

### Lacuna atual

ABP já audita entidades `FullAuditedEntity`, mas decisões de moderação,
bloqueios e alterações de reports precisam de um contrato operacional uniforme.

### Evolução proposta

Adicionar evento/serviço de auditoria de ações:

```csharp
public interface IModerationAuditWriter
{
    Task WriteAsync(ModerationAuditEntry entry);
}
```

O registro deve conter:

- tenant e usuário executor;
- tipo de ação;
- alvo anonimizado quando exibido ao cliente;
- motivo e decisão;
- correlation ID;
- data UTC;
- referência opcional para `GameId`, `MatchId` e report.

A consulta administrativa deve suportar paginação, filtro por ação, usuário,
período e tenant, respeitando permissões EAF/ABP.

## Prioridade 6 — Observabilidade e erros públicos

Padronizar no EAF:

```text
not_authenticated
not_authorized
feature_disabled
rate_limited
invalid_context
temporarily_unavailable
validation_failed
```

Cada erro deve possuir:

- código estável;
- mensagem localizada para UI;
- `retryable`;
- correlation ID;
- detalhes internos somente no log.

Adicionar métricas para:

- mensagens enviadas/bloqueadas;
- reports abertos/resolvidos;
- notificações publicadas/entregues;
- bloqueios e mutes;
- rate limits acionados;
- falhas de backplane SignalR;
- latência de cache e persistência.

## Prioridade 7 — Integração SignalR e backplane

O EAF deve documentar o contrato operacional entre `IChatCommunicator`,
`SignalRChatCommunicator`, `ChatHub` e o backplane oficial:

- configuração condicional por ambiente;
- `ChannelPrefix` obrigatório e isolado;
- comportamento quando Redis está indisponível;
- health check de conexão e Pub/Sub;
- compatibilidade entre versões de instâncias;
- teste de grupos e mensagens entre duas instâncias.

`ICacheManager` continua responsável por cache/TTL; ele não deve ser tratado
como substituto de Pub/Sub ou do backplane SignalR.

## Prioridade 8 — Contratos para SDKs consumidores

Publicar no EAF contratos estáveis para que o GameHub mantenha sua fachada
SDK:

- notificações;
- presença coarse-grained;
- block/mute;
- histórico contextual;
- mark-read;
- erros versionados;
- capabilities/feature flags.

Os contratos devem ser opcionais e versionados para não quebrar aplicações
existentes do EAF.

## Ajustes obrigatórios nos templates do EAF

As evoluções devem ser aplicadas primeiro aos templates para que novos
consumidores recebam os contratos corretos sem copiar configurações do
GameHub.

### `Templates/Api`

Atualizar o template de API para:

- registrar `ICacheManager`/`IDistributedCache` através do módulo EAF, sem
  configurar o provider novamente na aplicação consumidora;
- oferecer configuração opcional de Redis backplane SignalR com
  `ChannelPrefix` por aplicação e ambiente;
- registrar health checks para banco, cache, Pub/Sub e serviços externos;
- habilitar correlation ID, Serilog estruturado e OpenTelemetry por padrão;
- expor middleware de erro com códigos públicos versionados;
- incluir exemplos de `INotificationPublisher`, `IRateLimitManager`,
  `IModerationAuditWriter` e contratos de chat contextual;
- incluir políticas de autorização e filtros de multi-tenancy no exemplo;
- incluir testes de integração para autorização, cache, notificações, rate
  limit, auditoria e fallback quando Redis estiver indisponível;
- incluir migration guide quando uma entidade do template receber metadata de
  chat ou auditoria.

Assinaturas de configuração esperadas:

```csharp
public static IServiceCollection AddEafRealtime(
    this IServiceCollection services,
    IConfiguration configuration);

public static IServiceCollection AddEafObservability(
    this IServiceCollection services,
    IConfiguration configuration);
```

Esses métodos devem ser idempotentes e não podem registrar uma segunda
implementação de cache, chat ou notificações quando o módulo EAF já estiver
ativo.

### `Templates/Angular`

Atualizar o template Angular para:

- consumir os contratos de erro `{ code, message, retryable, correlationId }`;
- fornecer interceptor para correlation ID e refresh de autenticação;
- incluir cliente de notificações e estado de leitura;
- incluir cliente de chat contextual com `gameId`/`matchId` opcionais;
- expor operações de block/mute como fachada dos endpoints EAF;
- implementar retry limitado somente para falhas transitórias;
- incluir estados loading, empty, error e retry acessíveis;
- garantir foco visível, navegação por teclado, labels, tabelas semânticas e
  regiões `aria-live`;
- incluir testes Jasmine para sucesso, erro, retry, expiração e reconexão
  SignalR;
- manter geração de proxies como etapa do build, sem editar
  `service-proxies.ts` manualmente.

Interfaces mínimas sugeridas:

```typescript
export interface EafError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
}

export interface ContextualChatMessage {
  conversationId?: string;
  gameId?: string;
  matchId?: string;
  text: string;
}
```

### Checklist de validação dos templates

Cada alteração nos templates deve validar:

1. novo projeto compila sem dependências específicas do GameHub;
2. cache EAF é registrado uma única vez;
3. Redis desabilitado não impede o startup local;
4. duas instâncias compartilham presença/notificações quando configuradas;
5. erros públicos não expõem stack trace ou PII;
6. testes backend e Angular cobrem os exemplos;
7. build dos templates passa com warnings de budget controlados;
8. documentação explica quais contratos são EAF e quais são específicos do
   consumidor.

## Sequência recomendada de implementação no EAF

1. Metadata contextual de chat e índices.
2. Rate limiting compartilhado.
3. Notificações com metadata e entrega SignalR.
4. Block/mute e integração com chat.
5. Auditoria de moderação.
6. Erros públicos e observabilidade.
7. Validação operacional do backplane em duas instâncias.
8. Atualização do SDK/contratos consumidores do GameHub.

## Critérios de aceite

- Nenhuma segunda persistência de chat ou notificações no GameHub.
- Clientes EAF existentes continuam compilando e funcionando.
- Todos os contratos novos são opcionais ou versionados.
- Dados são isolados por tenant.
- Nenhum payload público expõe PII, claims ou identificadores de conexão.
- Testes cobrem integração, concorrência, expiração, autorização e falhas de
  Redis/backplane.
- A documentação do EAF inclui migration guide e estratégia de rollout.

## Revisão dos módulos EAF — gaps adicionais identificados

Esta revisão foi feita contra os módulos e templates atualmente consumidos pelo
GameHub. As alterações abaixo pertencem ao EAF; nesta branch ficam somente
registradas para evitar implementação duplicada no GameHub.

### 1. Template API e configuração de runtime

O template já centraliza `AddEafConfigurer`, `AddEafHealthChecks`,
`AddEafOpenTelemetry`, `UseEafHealthChecks` e o mapeamento de `ChatHub`.
A documentação do EAF deve complementar esses pontos com:

- matriz explícita de configurações por ambiente para banco, cache, Hangfire,
  CORS, Data Protection, SignalR e OpenTelemetry;
- distinção formal entre cache de aplicação, armazenamento do Hangfire e
  backplane SignalR — Redis pode atender os três, mas cada uso exige
  isolamento, TTL e observabilidade próprios;
- procedimento de duas instâncias para validar grupos SignalR, notificações,
  presença e reconexão;
- health checks separados para banco, cache e Pub/Sub, com status degradado
  quando um recurso opcional estiver desabilitado;
- persistência compartilhada de Data Protection Keys em produção; o diretório
  local do template não é suficiente para múltiplas instâncias;
- política de CORS com origens explícitas, incluindo exemplos seguros para
  desenvolvimento, staging e produção;
- política de retenção e mascaramento para logs de `TenantId`, `UserId`,
  correlation ID e payloads de chat;
- documentação de graceful shutdown para SignalR e Hangfire;
- `CancellationToken` nos exemplos públicos de serviços e jobs;
- exemplos de migrations, rollback e compatibilidade entre versões de schema.

### 2. Cache, rate limit e execução distribuída

Os módulos de cache devem documentar:

- provider efetivo de `ICacheManager` em cada configuração;
- comportamento de fallback e indicação operacional quando Redis estiver
  indisponível;
- prefixo obrigatório por aplicação, ambiente e tenant;
- limites de tamanho, serialização, TTL e política de invalidação;
- atomicidade esperada para contadores de rate limit;
- proibição de usar cache como fila, lock distribuído ou Pub/Sub sem contrato
  específico;
- métricas de hit/miss, latência, erros e chaves expiradas;
- teste de concorrência e de failover.

Para o GameHub, o EAF deve publicar uma abstração compartilhada semelhante a:

```csharp
public interface IRateLimitManager
{
    Task<RateLimitDecision> CheckAsync(
        string policy,
        string subject,
        TimeSpan window,
        int limit,
        CancellationToken cancellationToken = default);
}
```

### 3. Chat, amizades e notificações

Os módulos EAF já possuem `ChatHub`, `IChatCommunicator`, `ChatMessage`,
`FriendshipAppService`, block/unblock e notificações. Faltam contratos
documentados e versionados para:

- `GameId`, `MatchId` e `ConversationId` opcionais no chat;
- histórico paginado por contexto;
- `MarkRead` idempotente;
- mute com expiração e motivo;
- filtro de usuários bloqueados antes da persistência e da entrega;
- idempotência por `clientMessageId`;
- ordenação, timestamp UTC e paginação estável;
- metadata de notificação sem PII;
- capabilities para que o SDK saiba quais recursos estão habilitados;
- reconexão com recuperação de estado e cursor de eventos.

O GameHub deve continuar usando esses contratos como fachada, sem criar
entidades paralelas de chat, amizade, block, mute ou notificação.

### 4. Segurança, identidade e privacidade

Os módulos EAF devem esclarecer no guia de integração:

- identidade sempre derivada do token/sessão, nunca do payload do iframe;
- autorização por tenant, usuário, amizade, jogo e partida;
- proteção contra enumeração de usuários e histórico;
- limites de tamanho e normalização de mensagens;
- retenção, anonimização e exclusão de dados de chat, auditoria e telemetria;
- não exposição de connection IDs, claims internas, IP ou e-mail em contratos
  públicos;
- rotação e armazenamento compartilhado de Data Protection Keys;
- CORS, CSP, antiforgery e cookies em cenários com frontends separados;
- erros públicos estáveis sem stack trace.

### 5. Templates Angular

Além da seção de implementação anterior, a documentação do template Angular
deve corrigir e esclarecer:

- versão real do Angular/TypeScript/RxJS por release; o guia não deve manter
  Angular 18 se o template distribuído estiver em versão posterior;
- estratégia de migração e compatibilidade do pacote `@microsoft/signalr`;
- interceptor de autenticação, correlation ID e tratamento de `401/403`;
- retry limitado com backoff somente para erros transitórios;
- cancelamento de requisições e limpeza de subscriptions;
- reconexão SignalR com backoff, estado offline e reidratação de notificações;
- geração de `service-proxies.ts` e proibição de edição manual;
- checklist WCAG AA e testes automatizados de teclado, foco, live regions e
  tabelas;
- orçamento de bundle e CSS, com warnings tratados como dívida rastreável;
- testes em navegador headless no CI, incluindo diagnóstico quando o Chrome
  não estiver disponível.

### 6. Módulos ainda sem guia operacional suficiente

O EAF deve adicionar documentação específica para:

- `Eaf.KeyVault` e `Eaf.KeyVault.AspNetCore`: resolução de secrets, rotação,
  fallback seguro e falha de startup;
- `Eaf.OpenTelemetry`: nomes de serviço, propagação, sampling, exporters e
  atributos permitidos;
- `Eaf.Castle.Serilog`: correlação, sink, PII e retenção;
- `Eaf.Middleware.Worker`: idempotência, shutdown, retries e health checks;
- `Eaf.SqlServerCache`/`Eaf.SqliteCache`: quando usar, limitações e migração;
- Hangfire: storage, locks, retries, dashboards e execução em múltiplas
  instâncias;
- webhooks: assinatura, replay protection, timeout, retry e auditoria;
- geração de clientes Angular/OpenAPI: versionamento e breaking changes.

### Backlog priorizado do EAF

| Prioridade | Entrega | Impacto no GameHub |
| --- | --- | --- |
| P0 | Chat contextual, idempotência e histórico paginado | Habilita chat por partida sem duplicação |
| P0 | Data Protection compartilhado e runbook multi-instância | Permite escala horizontal segura |
| P0 | Erros públicos, correlation ID e rate limit compartilhado | Padroniza SDK, portal e APIs |
| P1 | Mute, block integrado ao chat e notificações com metadata | Completa moderação e social |
| P1 | Health checks e métricas de cache/SignalR/Hangfire | Melhora operação e alertas |
| P1 | Atualização dos templates API/Angular e testes | Evita regressão em novos projetos |
| P2 | Guias de KeyVault, Worker, webhooks e cache alternativo | Reduz risco operacional e tempo de adoção |
