# Prompt 32 — Operação e testes do cache de presença multiplayer

## Configuração delegada ao EAF

`RedisCache:IsEnabled` e `RedisCache:IsRedisEnabled` são lidos pelo EAF.
`CacheConfigurer` configura o `ICacheManager` com
`RedisCache:ConnectionString` e `RedisCache:DatabaseId`; `RedisConfigurer`
configura o `IDistributedCache`. O GameHub não deve repetir essas chamadas.

Este prompt cobre apenas diagnóstico e operação do uso de cache pelo
multiplayer. O `WebHostModule` pode registrar `IConnectionMultiplexer` para
caches específicos que usam comandos Redis diretamente, sem substituir a
configuração central do EAF.

## Objetivo

Tornar a presença baseada em `ICacheManager` operável, mensurável e verificável em ambientes local, staging e produção, sem transformar cache efêmero em requisito de disponibilidade do catálogo.

## Health checks

Adicionar health check protegido/configurável para:

- provider de cache resolvido;
- leitura/escrita de uma chave sintética com TTL curto;
- latência da operação;
- identificação segura do modo (`local`, `redis`, outro provider);
- falha de cache sem incluir connection string.

Definir estados:

- `Healthy`: leitura/escrita e TTL funcionando;
- `Degraded`: cache indisponível, fallback local ativo;
- `Unhealthy`: cache obrigatório configurado, mas sem leitura/escrita.

## Métricas

Adicionar métricas com nomes estáveis:

```text
multiplayer.presence.registered
multiplayer.presence.refreshed
multiplayer.presence.removed
multiplayer.presence.expired
multiplayer.presence.cache_errors
multiplayer.presence.operation_duration_ms
multiplayer.presence.active_local_connections
```

Não incluir `connectionId`, `userId` ou payload em labels de alta cardinalidade.

## Logs

Usar logging estruturado com:

- `TenantId`;
- `GameId`;
- `MatchId`;
- operação;
- resultado;
- duração;
- provider lógico.

Nunca registrar token, payload de sinal, connection string ou conteúdo de cache.

## Testes de integração

Criar matriz:

| Cenário | Provider local | Redis |
|---|---:|---:|
| Register/Get | obrigatório | obrigatório quando disponível |
| TTL | obrigatório | obrigatório quando disponível |
| Refresh concorrente | obrigatório | recomendado |
| Isolamento tenant | obrigatório | obrigatório quando disponível |
| Falha de provider | simulado | recomendado |
| Duas instâncias | processo duplo simulado | obrigatório em staging |

O teste de duas instâncias deve validar presença, não entrega SignalR.

## Runbook

Documentar:

1. Como habilitar `RedisCache:IsEnabled`.
2. Como validar o provider efetivo.
3. Como configurar TTL/heartbeat.
4. Como interpretar `Degraded`.
5. Como limpar entradas órfãs por expiração natural.
6. Como diagnosticar Redis sem expor credenciais.
7. Como distinguir falha de presença de ausência de backplane.

## Rollout

1. Implementar store atrás de feature flag.
2. Executar shadow read do registro local e cache, sem alterar entrega.
3. Comparar divergências e latência.
4. Ativar escrita no cache.
5. Ativar leitura para validações.
6. Remover `ConcurrentDictionary` como fonte primária após observação.
7. Manter fallback local apenas para desenvolvimento ou degradação explicitamente aceita.

## Critérios de aceite

- Health check e métricas estão publicados pela infraestrutura de observabilidade atual.
- Falha de cache tem comportamento definido e testado.
- Não existem labels de alta cardinalidade.
- Runbook cobre configuração e troubleshooting.
- O rollout pode ser revertido sem migration de banco.
