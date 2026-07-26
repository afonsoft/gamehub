# Prompt 30 — `ICacheManager` como cache multiplayer

## Objetivo

Criar uma abstração de cache multiplayer baseada no `ICacheManager` do ASP.NET Boilerplate/EAF, permitindo que presença, leases e estado efêmero usem o provider configurado pela API sem acoplar o domínio ao Redis.

## Contexto atual

- `WebHostModule` registra `IConnectionMultiplexer` quando `RedisCache:IsEnabled=true`.
- `RedisGameCatalogCache` usa `IDistributedCache`.
- `RedisLeaderboardCache` usa `IConnectionMultiplexer`.
- `NetworkPeerRegistry` usa `ConcurrentDictionary<string, Guid>` em memória.
- O EAF pode fornecer `IDistributedCache`/cache ABP conforme configuração; o provider efetivo precisa ser validado, não presumido.

## Escopo

### Contratos

Criar em Application:

- `IMultiplayerCache`
- `IMultiplayerPresenceStore`
- DTO/modelo interno `MultiplayerCacheEntry`

O contrato deve oferecer, no mínimo:

```text
GetAsync<T>(key)
SetAsync<T>(key, value, ttl)
RemoveAsync(key)
RefreshAsync(key, ttl)
ExistsAsync(key)
```

O contrato deve aceitar `CancellationToken` quando suportado pela implementação e deve definir o comportamento para cache ausente, TTL inválido e falha do provider.

### Implementação ABP

Criar em Web/Infrastructure uma implementação baseada em `ICacheManager`:

- obter cache nomeado e tipado por bounded context;
- serializar valores de maneira consistente;
- aplicar TTL sempre;
- usar prefixo `gamehub:multiplayer`;
- não enumerar chaves em request path;
- não armazenar payload de signaling;
- não registrar valores de tokens, cookies ou credenciais.

O nome do cache deve ser estável, por exemplo:

```text
GameHub.Multiplayer
```

### Configuração

Documentar e validar:

```json
{
  "RedisCache": {
    "IsEnabled": false,
    "ConnectionString": "localhost",
    "DatabaseId": 0
  },
  "Multiplayer": {
    "Presence": {
      "IsEnabled": true,
      "EntryTtlSeconds": 90,
      "HeartbeatIntervalSeconds": 30
    }
  }
}
```

`IsEnabled` da presença não deve assumir que o provider é Redis. Em modo sem Redis, a implementação pode operar localmente para desenvolvimento/testes, mas deve expor o modo efetivo em health/diagnóstico.

### Provider efetivo

Adicionar diagnóstico seguro que informe apenas o nome do tipo de provider, sem connection string:

```text
MemoryDistributedCache
RedisCache
Eaf...
```

O diagnóstico deve estar disponível em health check ou endpoint administrativo protegido.

## Fora do escopo

- Pub/Sub.
- Backplane SignalR.
- Persistência relacional de presença.
- Enumeração de chaves Redis.
- Alteração de contratos públicos do bridge.

## Testes obrigatórios

- `Dado_CacheConfigurado_Quando_SalvarEConsultar_Entao_RetornaMesmoValor`
- `Dado_TTLExpirado_Quando_Consultar_Entao_RetornaAusente`
- `Dado_EntradaExistente_Quando_Refresh_Entao_RenovaTTL`
- `Dado_KeyDeOutroTenant_Quando_Consultar_Entao_NaoRetornaValor`
- `Dado_TTLInvalido_Quando_Salvar_Entao_RejeitaEntrada`
- `Dado_ProviderIndisponivel_Quando_Consultar_Entao_AplicacaoAplicaPoliticaDefinida`

## Critérios de aceite

- Application não referencia `ICacheManager`.
- Todas as chaves são tenant-aware.
- Toda entrada tem limite de tamanho e TTL.
- O provider efetivo pode ser diagnosticado sem expor segredo.
- Os testes passam com provider em memória.
- A implementação Redis é exercitada em teste de integração quando Redis estiver disponível.
