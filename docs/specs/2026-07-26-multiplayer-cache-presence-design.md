# Design — Cache distribuído e presença multiplayer

## Contexto

O GameHub possui Redis configurável, `IConnectionMultiplexer`, caches de catálogo/leaderboard e um `NetworkPeerRegistry` em memória usado pelo hub `/signalr-network`. O objetivo desta evolução é preparar a presença distribuída usando a abstração de cache do ASP.NET Boilerplate (`ICacheManager`), sem introduzir ainda Pub/Sub ou o backplane oficial do SignalR.

O provider base não pertence ao GameHub: o `MiddlewareWebCoreModule` do EAF
executa `CacheConfigurer` para o `ICacheManager` e `RedisConfigurer` para o
`IDistributedCache`. O GameHub deve consumir essas abstrações e implementar
somente as extensões específicas do multiplayer.

## Decisão aprovada

Implementar em três etapas:

1. Padronizar o uso de `ICacheManager` através de uma abstração própria do GameHub.
2. Migrar o registro de presença do signaling para entradas com TTL no cache.
3. Adicionar operação, health checks, métricas e testes multi-instância; manter backplane/Pub/Sub como evolução futura.

## Limite importante

`ICacheManager` resolve armazenamento distribuído de estado efêmero, mas não replica mensagens de hub. Sem backplane:

- uma instância pode consultar a presença lógica de outras instâncias;
- `Signal` e `Broadcast` continuam entregando mensagens somente para conexões mantidas pela instância local;
- a migração não pode prometer signaling cross-instance completo.

## Abordagens rejeitadas nesta fase

- Acesso direto a `IConnectionMultiplexer` como abstração de aplicação: acopla o contrato ao Redis.
- Implementação de Pub/Sub próprio: aumenta o protocolo operacional e não é necessário para a primeira etapa.
- Ativação imediata do backplane oficial: fica documentada no Prompt 33, após validar provider, carga e topologia.

## Regras de arquitetura

- Core não referencia ABP cache, Redis ou SignalR.
- Application referencia apenas `IMultiplayerPresenceStore`.
- Web/Infrastructure implementa o store com `ICacheManager`.
- Todo key prefix inclui tenant, jogo e partida quando aplicável.
- Nenhum payload de sinal sensível deve ser persistido no cache de presença.
- Toda entrada possui TTL e deve sobreviver à ausência de evento explícito de desconexão.
- `WebHostModule` não deve chamar `Configuration.Caching.UseRedis(...)`;
  essa configuração é responsabilidade do EAF.
- `IConnectionMultiplexer` direto só deve ser registrado para caches ou
  componentes do GameHub que realmente precisem de comandos Redis.

## Sequência alvo

```text
JoinLobby
  -> valida token e partida
  -> PresenceStore.Register(connection, match, instance)
  -> registra TTL
  -> adiciona conexão ao grupo local SignalR

Heartbeat
  -> PresenceStore.Refresh(connection)

Signal/Broadcast
  -> valida presença
  -> entrega apenas por Clients local

Disconnect/TTL
  -> PresenceStore.Remove(connection)
  -> matchmaking mantém a regra de grace period existente
```

## Critérios de aceite do design

- Os contratos de cache não vazam `ICacheManager` para Core/Application.
- A implementação pode usar o provider Redis do EAF/ABP sem alterar o modelo relacional.
- A configuração de `ICacheManager` e `IDistributedCache` não é duplicada no
  GameHub.
- O comportamento de instância única permanece compatível.
- A limitação de entrega cross-instance está explícita na documentação e nos testes.
- A futura adição de Pub/Sub não exige alterar o contrato de presença.
