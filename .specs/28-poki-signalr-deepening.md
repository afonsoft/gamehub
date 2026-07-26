# 28 — Aprofundamento SignalR / Netlib e limpeza

> **Status:** pendente
> **Base:** análise de `https://github.com/poki/netlib`, `https://sdk.poki.com/sdk-documentation` e estado do spec 27
> **Objetivo:** evoluir a base multiplayer e AUDS com foco em SignalR/WebRTC signaling, resiliência, observabilidade e manutenção.

---

## Contexto

O spec 27 entregou a base de Netlib/Multiplayer (sala, matchmaking, estado da partida) e o Arbitrary User Data Store (AUDS) via HTTP + SignalR hub simples. Para ficar mais próximo da Poki Netlib, o próximo passo é transformar o SignalR em ponto de sinalização WebRTC e adicionar resiliência/presença.

---

## 28.1 — Autorização e autenticação no hub

### Requisito
O `GameHubMatchHub` deve autenticar conexões antes de permitir joins.

### Detalhes
- Receber token JWT curto via query string (`?access_token=...`) ou cookie.
- `IGameTokenProvider` valida `gameId`, `sub`, `tenantId` e `exp`.
- Método `OnConnectedAsync` rejeita conexão sem token válido.
- Registrar `IUserIdProvider` para mapear `Context.UserIdentifier` ao `UserId` do ABP.

### Testes
- `GameHubMatchHub_Tests` (in-memory SignalR test server): conexão sem token retorna 401; conexão com token válido consegue `CreateMatch`.

---

## 28.2 — Reconexão resiliente e presence

### Requisito
Tolerar falhas de rede e manter presence de jogadores.

### Detalhes
- Mapear `ConnectionId` → `UserId`/`AnonymousIdHash` em `IOnlineClientManager` ou cache Redis.
- `OnDisconnectedAsync` não remove participante imediatamente; aguardar 30s de grace period.
- Se reconectar com novo `ConnectionId` dentro do grace period, reativar o mesmo `MatchParticipant`.
- `Clients.Group` recebe `PlayerReconnected` com novo `ConnectionId`.
- Expiração de salas inativas por mais de 4h (já configurada; adicionar background job).

### Testes
- `MatchmakingService_Tests`: `LeaveMatchAsync` sem active participants mantém status `InProgress` por 30s; `ReactivateParticipantAsync` após disconnect.

---

## 28.3 — Spectator support

### Requisito
Permitir espectadores em partidas sem contar no limite de jogadores.

### Detalhes
- Novo campo `MatchParticipant.IsSpectator`.
- `SpectateMatchAsync(matchId)` no `IMultiplayerAppService` e hub.
- Espectadores recebem `MatchStateChanged` mas não podem chamar `SendMatchState`.
- Limite de espectadores por sala (padrão 10).

### Testes
- Spectator join após sala cheia; spectator envia estado → `UnauthorizedAccessException`.

---

## 28.4 — Rate limiting e validação de mensagens

### Requisito
Evitar spam de estado/mensagens no SignalR.

### Detalhes
- Rate limit de `SendMatchState` (ex: 20 msg/s por sala, 5 msg/s por jogador).
- Rejeitar `PayloadJson` > 64 KB no hub (já limitado no AUDS; aplicar igual no match).
- Sanitizar `PayloadJson` para evitar JSON malformado.

### Testes
- `GameHubMatchHub_Tests`: envio rápido retorna erro de rate limit; payload > 64 KB rejeitado.

---

## 28.5 — WebRTC signaling hub (Netlib parity)

### Requisito
Fornecer sinalização para P2P via WebRTC, mantendo o relay SignalR como fallback.

### Detalhes
- `NetworkSignalRHub` com métodos:
  - `JoinLobby(gameId, mode, maxPlayers)` — encontra/cria sala.
  - `Signal(peerId, payload)` — encaminha SDP/ICE candidates entre pares.
  - `Broadcast(channel, payload)` — mensagem para todos os peers (fallback quando P2P ainda não estabelecido).
- Tipos de canal: `reliable` e `unreliable` (mapear para envio SignalR/HTTP).
- Preservar abstração `IMatchmakingService` como backend do lobby.

### Testes
- Dois clients conectam e trocam mensagens `signal` via hub; broadcast chega a todos.

---

## 28.6 — AUDS evolução

### Requisito
Limpar dados expirados e expor evolução.

### Detalhes
- Background job diário `CleanupExpiredArbitraryUserDataJob` (Hangfire) removendo `ExpiresAt < Clock.Now`.
- `GameplayBridgeService.loadArbitrary` retorna `{}` quando chave inexistente.
- `GameplayBridgeService.saveArbitrary` retorna `{ saved, quota }` com total usado.
- Implementar delete e quota na bridge.

### Testes
- `ArbitraryUserDataAppService_Tests`: TTL expirado não retorna valor; background job remove registro.

---

## 28.7 — Observabilidade

### Requisito
Métricas e logs estruturados para multiplayer/AUDS.

### Detalhes
- Contadores: `multiplayer.matches.created`, `multiplayer.matches.active`, `multiplayer.players.connected`, `multiplayer.messages.sent`, `auds.keys.stored`, `auds.bytes.stored`.
- Logs com `GameId`, `MatchId`, `UserId`, `ConnectionId`, `CorrelationId`.
- Health check para SignalR: `/health/signalr` ou endpoint de status do hub.

### Testes
- `AdminDashboardAppService_Tests`: agregação de métricas multiplayer por jogo/dia.

---

## Entregáveis

1. Backend: autorização no hub, presence, spectator, rate limit, WebRTC signaling, AUDS cleanup, métricas.
2. Frontend: bridge `reconnect`, `spectateMatch`, `signal`, `broadcast`, `saveArbitrary` com retorno de quota.
3. Tests: `GameHubMatchHub_Tests` com in-memory server, `MatchmakingService_Tests`, `ArbitraryUserDataAppService_Tests`.
4. Migração EF Core `Poki28` para `MatchParticipant.IsSpectator` e ajustes.
5. README/CHANGELOG/agent log + spec 29 com próximos passos (ex: public match browser, ranked multiplayer).

---

## Notas

- Prioridade: 28.1 (autenticação) e 28.2 (reconnect/presence) são críticos para produção.
- 28.5 (WebRTC signaling) é mais avançado; se muito complexo, reservar sessão dedicada.
- Manter a camada `GameHubMatchHub` fina; lógica de lobby e estado continua no `IMatchmakingService`.
