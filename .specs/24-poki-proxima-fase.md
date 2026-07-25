# 24 — Próxima Fase Poki: P4D v2 Completo, Inspector Final e Otimizações

> **Status:** implementado
> **Base:** análise de `https://sdk.poki.com/new-requirements`, `https://sdk.poki.com/poki-inspector.html` e `https://sdk.poki.com/what-is-p4d.html`
> **Objetivo:** fechar os itens remanescentes do spec 23 e incorporar otimizações e integrações finais da Poki.

---

## Contexto

O spec 23 entregou:

- ✅ 23.1 — Thumbnails estáticos e animados com moderação.
- ✅ 23.3 — Inspector v3: checklist persistente, scaling tests, re-run de validação e preview.
- ✅ 23.4 — Aspect ratio, clean build, file size target, outgoing links e splash screens.
- ✅ Preview mode do 23.2 — tokens JWT para builds não publicados e rota pública.

Restam do spec 23:

- 🔄 23.2a — `DeveloperTeam` e `DeveloperTeamMember` (roles `Developer` / `Support` / `Billing`).
- 🔄 23.2b — `DeveloperBillingProfile` (tax info, payment method placeholder, sem dados sensíveis reais).
- 🔄 23.2c — `PlaytestSession` e `IPlaytestAppService`.
- 🔄 23.3 — QR code para modo mobile do Inspector.
- 🔄 23.4 — Incognito/first-party cookies (já parcial com `localStorage` fallback; reforçar try/catch e testes).
- 🔄 23.4 — Poki CLI parity: `gamehub.json` e endpoint `POST /api/services/app/GameBuild/UploadFromCli` com API key.

Novos requisitos identificados na documentação Poki:

1. **Image Optimization warnings** — avisar quando imagens do build podem ser comprimidas para reduzir file size/loading time.
2. **Unexpected Behavior Detected** — warning quando o jogo dispara eventos SDK fora de ordem, duplicados ou durante ad breaks (já parcial no Inspector; falta exposição automática e UI).
3. **Poki Pill / mobile UI** — permitir que o jogo reposicione o "Poki Pill" no mobile (`movePill(topPercent, topPx)`) — equivalente a overlay/notification area no GameHub.
4. **General Team Settings** — nome do time, e-mail de contato principal e país no portal do desenvolvedor.
5. **Suggested Categories & Description** — já entregue em 19.4; revisar se faltam até 4 categorias sugeridas e SEO description.
6. **Versions tab actions** — "Open in Inspector" e "Preview on Game Hub" por build na lista de versões do desenvolvedor.
7. **Save System UX** — informar o jogador quando progresso não será salvo (anônimo / cloud saves desabilitados).

---

## 24.1 — P4D v2: Equipes e Billing (23.2a + 23.2b)

### Requisito
Tornar o portal do desenvolvedor multi-usuário com roles e billing placeholder.

### Tarefas
1. Criar `DeveloperTeam` (Id, TenantId, Name, PrimaryContactEmail, Country, CreatedAt).
2. Criar `DeveloperTeamMember` (Id, TenantId, TeamId, UserId, Role: Developer/Support/Billing, InvitedAt, AcceptedAt).
3. Criar `IDeveloperTeamAppService`:
   - `CreateTeamAsync`, `UpdateTeamAsync`, `GetMyTeamAsync`.
   - `InviteMemberAsync(email, role)`, `RemoveMemberAsync(userId)`, `AcceptInvitationAsync(token)`.
4. Criar `DeveloperBillingProfile` vinculado a `DeveloperTeam` (TaxId, Address, PaymentMethodPlaceholder, IsApproved).
5. Endpoint para dev preencher e marcar billing como pendente de aprovação.
6. Garantir que `Support` não acesse earnings/métricas de monetização após release.

### Testes
- `DeveloperTeamAppService_Tests`: convite, aceite, remoção, permissões por role.
- `DeveloperBillingProfileAppService_Tests`: preenchimento e leitura.

---

## 24.2 — P4D v2: Playtests (23.2c)

### Requisito
Solicitar e listar sessões de playtest com gravações.

### Tarefas
1. Criar `PlaytestSession` (Id, TenantId, GameId, RequestedByUserId, Status, RecordingUrl, CreatedAt, CompletedAt).
2. Criar `IPlaytestAppService`:
   - `RequestPlaytestAsync(gameId, notes)`.
   - `GetPlaytestsByGameAsync(gameId)`.
   - `UploadRecordingAsync(playtestId, url)` (admin/moderador).
3. UI no portal do desenvolvedor para solicitar e visualizar gravações.

### Testes
- `PlaytestAppService_Tests`: criação, listagem e upload de gravação.

---

## 24.3 — Inspector v3: QR Code e Warnings Automáticas

### Requisito
Completar o Inspector com QR code para mobile e warnings automáticas de comportamento inesperado.

### Tarefas
1. **QR Code**:
   - No admin Inspector, botão "Mobile Mode" exibe QR code apontando para `/play/:slug?inspector=1&inspectorSession={id}`.
   - Usar biblioteca front (ex. `qrcode` npm) ou serviço simples (`https://api.qrserver.com/v1/create-qr-code/?data=...` como fallback).
2. **Unexpected Behavior warnings**:
   - `InspectorAppService.ValidateSessionAsync` já retorna warnings; adicionar categoria `UnexpectedBehavior` para eventos fora de ordem/durante ad break.
   - Exibir warning badge no admin Inspector com lista de mensagens.
3. **Image Optimization warnings** (futuro/low-priority):
   - Analisar arquivos de imagem no zip e sugerir compressão quando > 100 KB e não otimizado.

### Testes
- `InspectorAppService_Tests`: warnings de `UnexpectedBehavior`.

---

## 24.4 — Incognito / First-Party e Save System UX

### Requisito
Garantir que o jogo funcione em modo anônito/incognito e comunique ao jogador quando progresso não será salvo.

### Tarefas
1. Envolver todas as operações `localStorage` do `GameplayBridgeService` em `try/catch` para evitar falhas no incognito.
2. Quando `CloudSaveAppService` não puder persistir (anônimo e `localStorage` indisponível), retornar `saved: false` e mensagem amigável.
3. Na UI do `GameFrameComponent`, exibir toast "Progresso local apenas" quando o jogador estiver anônimo e `localStorage` for o único storage disponível.
4. Garantir que `PlayerPreference` e `PlayerPrivacyConsent` usem `localStorage` como fallback.

### Testes
- `CloudSaveAppService_Tests`: cenário de `localStorage` indisponível (mock).

---

## 24.5 — Poki CLI Parity (23.4)

### Requisito
Permitir upload de builds via CLI/CI com um `gamehub.json` similar ao `poki.json`.

### Tarefas
1. Definir contrato `gamehub.json`:
   ```json
   {
     "name": "Game Title",
     "slug": "game-slug",
     "version": "1.0.0",
     "buildDir": "./dist",
     "entryPoint": "index.html",
     "apiKey": "${GAMEHUB_API_KEY}"
   }
   ```
2. Criar `POST /api/services/app/GameBuild/UploadFromCli` (ou `/api/services/app/DeveloperGame/UploadFromCli`):
   - Receber zip + `gamehub.json`.
   - Autenticar via API Key vinculada a `DeveloperTeam`/`DeveloperProfile`.
   - Reutilizar `GameBuildAppService`.
3. Documentar contrato em `docs/gamehub-cli.md`.

### Testes
- `GameBuildCliAppService_Tests` (ou controller integration test): upload com API key válida/inválida.

---

## 24.6 — Versions Tab: Open in Inspector / Preview

### Requisito
Na lista de versões do desenvolvedor, permitir abrir cada build no Inspector ou Preview.

### Tarefas
1. `DeveloperGameAppService.GetVersionsAsync` já existe; adicionar ações `OpenInInspector` e `PreviewOnGameHub`.
2. No front (`developer/versions` ou `game-edit`), adicionar botões por versão:
   - "Open in Inspector" → abre `/app/main/gamehub/inspector?gameId={id}&buildId={buildId}`.
   - "Preview" → chama `GamePreviewAppService.CreatePreviewTokenAsync` e abre `/preview/:slug/:version?token=...`.

### Testes
- `DeveloperGameAppService_Tests`: mapeamento de ações (se houver lógica).

---

## 24.7 — Poki Pill / Mobile Overlay

### Requisito
Suportar reposicionamento de overlay/platform pill no mobile.

### Tarefas
1. Adicionar mensagem SDK `movePill(topPercent, topPx)` no `GameplayBridgeService`.
2. No `GameFrameComponent`, ajustar CSS de um overlay/pill (ex. "Jogue mais" / anúncios) com `top` calculado.
3. Persistir posição em `PlayerPreference`? Opcional.

### Testes
- Teste de unidade no front se houver lógica complexa.

---

## Critérios de Aceite

- `dotnet build Api/GameHub.sln` passando sem warnings.
- `dotnet test Api/GameHub.sln --no-build` passando.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` passando.
- Novas entidades cobertas por migrações EF Core.
- `docs/agent-execution-log.md`, `CHANGELOG.md` e `.specs/24-poki-proxima-fase.md` atualizados.
- Commits por funcionalidade; push e PR no final.

---

## Notas

- Prioridade: 24.1 e 24.2 (P4D v2) são os maiores escopos; podem ser divididos em sessões dedicadas se necessário.
- 24.3 QR code pode ser implementado rapidamente com uma biblioteca front; se demorar, usar serviço externo fallback.
- Não commitar secrets, API keys reais ou dados de billing.
