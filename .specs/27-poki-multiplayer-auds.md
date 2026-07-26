# 27 — Poki Netlib / Multiplayer e Arbitrary User Data Store (AUDS)

> **Status:** pendente
> **Base:** análise de `https://sdk.poki.com/new-requirements`, `https://sdk.poki.com/sdk-documentation` e referências da Poki Networking Library
> **Objetivo:** implementar suporte a partidas online leves e um backend genérico de chave/valor JSON para jogos que precisam persistir dados arbitrários na nuvem.

---

## Contexto

O spec 26 entregou analytics de erros, funil, feedback, quality gates, retenção, workflow de submissão e relatórios de anúncios. Dois itens foram reservados por complexidade:

- **26.9 — Netlib / Multiplayer**: partidas online, matchmaking e estado de sala.
- **26.10 — Arbitrary User Data Store (AUDS)**: persistência genérica de dados de jogos.

---

## 27.1 — Netlib / Multiplayer (base)

### Requisito
Suporte a partidas online leves via WebSockets/SignalR.

### Entidades
- `MatchState` (`Id`, `TenantId`, `GameId`, `RoomCode`, `Mode`, `Status`, `PlayerIds`, `PayloadJson`, `ExpiresAt`).
- `MatchParticipant` (`Id`, `TenantId`, `MatchId`, `UserId`, `AnonymousIdHash`, `ConnectionId`, `JoinedAt`, `LeftAt`).

### Domínio
- `Game.SupportsMultiplayer` (`bool`), `Game.MaxPlayersPerMatch` (`int`).
- `IMatchmakingService`: fila por `GameId` + `Mode`; cria `MatchState`; gerencia lifecycle.

### Infraestrutura
- SignalR hub `GameHubMatchHub` em `GameHub.Web.Host`.
- Grupos por `matchId`; mensagens `MatchUpdated`, `PlayerJoined`, `PlayerLeft`, `MatchEnded`.
- `IGameTokenProvider` valida token curto (`getToken`) para conexão anônima/autenticada.

### Endpoints (Application)
- `IMultiplayerAppService`:
  - `CreateMatchAsync(CreateMatchInput)` — cria sala.
  - `JoinMatchAsync(JoinMatchInput)` — entra em sala por código.
  - `LeaveMatchAsync(Guid matchId)` — sai da sala.
  - `UpdateMatchStateAsync(UpdateMatchStateInput)` — atualiza payload JSON.
  - `GetMatchAsync(Guid matchId)` — retorna estado e participantes.

### Bridge SDK
- `GameplayBridgeService` expõe:
  - `createMatch(mode, maxPlayers)`
  - `joinMatch(roomCode)`
  - `leaveMatch()`
  - `sendMatchState(payload)`
  - `onMatchStateChanged(callback)`

### Testes
- `MatchmakingService_Tests`: criação, join, leave, limite de jogadores, expiração.
- `MultiplayerAppService_Tests`: autorização, tenant isolation.

---

## 27.2 — Arbitrary User Data Store (AUDS)

### Requisito
Backend genérico de chave/valor JSON para jogos persistirem dados na nuvem sem schema fixo.

### Entidade
```csharp
public class ArbitraryUserData : FullAuditedEntity<Guid>, IMayHaveTenant
{
    public int? TenantId { get; set; }
    public Guid GameId { get; set; }
    public long? UserId { get; set; }
    [StringLength(64)] public string AnonymousIdHash { get; set; }
    [StringLength(128)] public string Key { get; set; }
    public string ValueJson { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public virtual Game Game { get; set; }
}
```

### Regras
- Quota por `(GameId, UserId/AnonymousId)`: máximo 100 chaves e 64 KB por valor (default).
- `ValueJson` validado como JSON sintaticamente válido.
- Chaves com prefixo `gamehub_ignore_` rejeitadas (reservadas para local-only).
- TTL `ExpiresAt` opcional; job diário remove registros expirados.

### Application
- `IArbitraryUserDataAppService`:
  - `SetAsync(SetArbitraryUserDataInput)` — upsert com validação de quota/TTL.
  - `GetAsync(GetArbitraryUserDataInput)` — retorna `ValueJson` ou null.
  - `DeleteAsync(DeleteArbitraryUserDataInput)`.
  - `GetQuotaAsync(Guid gameId)` — retorna uso atual.

### Bridge SDK
- `GameplayBridgeService` expõe:
  - `saveArbitrary(key, value, ttlSeconds?)`
  - `loadArbitrary(key)`
  - `deleteArbitrary(key)`

### Testes
- `ArbitraryUserDataAppService_Tests`: CRUD, quota, TTL, JSON inválido, prefixo reservado.

---

## 27.3 — Integrações

### Segurança
- Multiplayer e AUDS respeitam `TenantId` e `AbpSession`.
- Conexões SignalR exigem token JWT curto (`getToken`) ou cookie de autenticação.
- AUDS valida que o jogo chamador (`GameId`) corresponde à origem do token.

### Observabilidade
- Métricas: `multiplayer.matches.created`, `multiplayer.matches.active`, `auds.keys.stored`.
- Logs estruturados com `GameId`, `MatchId`, `UserId`.

---

## Notas

- Itens 27.1 e 27.2 são independentes e podem ser entregues separadamente.
- Prioridade sugerida: 27.2 (AUDS) → 27.1 (Multiplayer) — AUDS é mais simples e desbloqueia cloud saves genéricos.
- Criar migração `AddPoki27MultiplayerAndAuds` ao final.
