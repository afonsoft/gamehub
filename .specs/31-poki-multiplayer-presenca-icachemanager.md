# Prompt 31 — Presença multiplayer distribuída com `ICacheManager`

## Objetivo

Substituir o `NetworkPeerRegistry` puramente local por um registro de presença baseado em cache, preservando as APIs atuais de `/signalr-network`, a reconexão de partidas e a compatibilidade com WebRTC.

## Modelo de presença

Cada conexão deve ser uma entrada independente, pois `ICacheManager` não deve depender de scan/enumeration:

```text
gamehub:multiplayer:presence:{tenant}:{match}:{connection}
```

Valor mínimo:

```json
{
  "connectionId": "...",
  "matchId": "...",
  "gameId": "...",
  "userId": 123,
  "instanceId": "...",
  "joinedAt": "...",
  "lastSeenAt": "...",
  "expiresAt": "..."
}
```

Não armazenar payloads de `Signal` ou `Broadcast`.

## Operações

Implementar `IMultiplayerPresenceStore`:

```text
RegisterAsync(entry)
RefreshAsync(connectionId, matchId)
RemoveAsync(connectionId, matchId)
GetAsync(connectionId, matchId)
IsPresentAsync(connectionId, matchId)
```

Opcionalmente, oferecer índices conhecidos por partida:

```text
gamehub:multiplayer:presence:match:{tenant}:{match}
```

O índice não deve ser a fonte única de verdade; entradas individuais e TTL são a autoridade. Se o provider não suportar atualização atômica, a implementação deve preferir consistência eventual e remover índices expirados oportunisticamente, nunca bloquear a entrega local.

## Alterações no hub

`NetworkSignalRHub` deve:

1. Registrar presença depois de validar token e partida em `JoinLobby`.
2. Atualizar presença por heartbeat explícito ou atividade controlada.
3. Remover a entrada em `OnDisconnectedAsync`, sem depender apenas disso.
4. Validar `Signal`/`Broadcast` contra presença atual.
5. Continuar usando `Groups.AddToGroupAsync` para entrega local.
6. Manter `PeerId`, `MatchId`, `RoomCode`, nomes de eventos e rotas atuais.

Adicionar heartbeat somente se o bridge/SDK puder suportá-lo sem quebrar clientes antigos. Caso contrário, usar refresh em operações existentes e um TTL conservador.

## Reconexão

Preservar a regra atual de grace period de 30 segundos no matchmaking. A presença de conexão deve ter TTL independente:

- TTL de presença: aproximadamente 90 segundos;
- grace period de participante: 30 segundos;
- desconexão explícita remove presença imediatamente;
- reconexão registra nova conexão e reativa o participante conforme regra atual.

## Limitação deliberada

Com somente `ICacheManager`:

- instância A consegue saber que uma conexão existe na instância B;
- instância A não consegue executar `Clients.Client(connectionId)` na instância B;
- grupos SignalR permanecem locais;
- entrega cross-instance continua fora do escopo.

Essa limitação deve aparecer em documentação, testes e logs operacionais.

## Segurança e multi-tenancy

- Prefixar toda chave com tenant.
- Validar `gameId` contra token escopado.
- Não aceitar `matchId` de outro tenant.
- Não permitir consultar presença de sala não autorizada.
- Não expor `instanceId` em endpoints públicos.
- Aplicar limite de conexões por usuário/partida.

## Testes obrigatórios

- `Dado_JoinLobby_Quando_RegistrarPresenca_Entao_EntradaPossuiTTL`
- `Dado_Heartbeat_Quando_AtualizarPresenca_Entao_TTLFoiRenovado`
- `Dado_Desconexao_Quando_RemoverPresenca_Entao_ConexaoNaoEstaPresente`
- `Dado_TTLExpirado_Quando_ConsultarPresenca_Entao_RetornaAusente`
- `Dado_OutroTenant_Quando_ConsultarPresenca_Entao_AcessoNegado`
- `Dado_DuasInstancias_Quando_ConsultarPresenca_Entao_RegistroEVisivel`
- `Dado_DuasInstanciasSemBackplane_Quando_EnviarSignal_Entao_LimitacaoEObservavel`

## Critérios de aceite

- `NetworkPeerRegistry` deixa de ser a fonte primária.
- Clientes existentes continuam funcionando sem alteração obrigatória.
- TTL recupera falhas abruptas de processo.
- Presença pode ser observada entre instâncias pelo cache.
- Nenhum teste promete entrega de sinal cross-instance sem backplane.
