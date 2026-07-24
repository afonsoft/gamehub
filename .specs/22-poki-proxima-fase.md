# 22 — Próxima Fase (inspirada na documentação Poki)

> **Base:** análise de `https://sdk.poki.com/new-requirements.html`, `https://sdk.poki.com/poki-inspector.html` e `https://sdk.poki.com/what-is-p4d.html`
> **Status:** planejado
> **Objetivo:** fechar gaps de qualidade, publicação e experiência do desenvolvedor que ainda não estão implementados.

---

## 22.1 — Save System & Cloud Saves

### Requisito
Jogos devem salvar progresso quando apropriado, ou informar claramente quando o progresso não será salvo ao sair.

### Tarefas
1. Adicionar `Game.SupportsCloudSaves` (bool) no `Game` e DTOs.
2. Criar entidade `PlayerGameSave` (`UserId`, `GameId`, `SaveData` JSONB/texto comprimido, `UpdatedAt`, limite 1 MB).
3. Criar `ICloudSaveAppService` com `GetAsync`, `SaveAsync`, `DeleteAsync`.
4. No `GameplayBridgeService` expor `save(json)` e `load(): Promise<json>` para jogos.
5. Quando `Game.SupportsCloudSaves = false`, exibir hint na saída: "Progresso deste jogo não é salvo".
6. Para usuários anônimos, fallback em `localStorage`/`IndexedDB` com prefixo `gamehub_ignore` e try/catch.

### Testes
- `CloudSaveAppService_Tests`: limite de 1 MB, merge de anônimo ao logar, exclusão.
- Teste unitário `GameplayBridgeService` chamando `save`/`load`.

---

## 22.2 — User Accounts no SDK

### Requisito
O SDK deve permitir `login()`, `getUser()` e `getToken()` conforme documentação Poki.

### Tarefas
1. Expandir `PlayerAccountAppService` com `GetPlayerProfileAsync()` retornando `{ username, avatarUrl }`.
2. Criar endpoint `/api/services/app/PlayerAccount/GetToken` que retorna JWT curto ( claims: `sub`, `gameId`, `tenantId`, `exp` ).
3. No `GameplayBridgeService` expor:
   - `login(): Promise<void>` -> abre modal OAuth e, no sucesso, recarrega a página (full page refresh, conforme Poki).
   - `getUser(): Promise<{ username, avatarUrl }>`.
   - `getToken(): Promise<string>`.
4. Garantir que `login()` só seja chamado em resposta a interação do usuário (não no load).

### Testes
- `PlayerAccountAppService_Tests.GetUser_returns_username_and_avatar`.
- `PlayerAccountAppService_Tests.GetToken_returns_jwt_with_game_id`.

---

## 22.3 — Thumbnails Estáticos e Animados

### Requisito
Jogos publicados devem ter thumbnail estático e animado (GIF/WebP/MP4 curto) para release global.

### Tarefas
1. Adicionar `Game.AnimatedThumbnailUrl` e `Game.ThumbnailStatus` (Pending, Approved, Rejected).
2. No portal do desenvolvedor, wizard de upload de thumbnail com pré-visualização.
3. Após publicação, atualização de thumbnail entra em fila de moderação.
4. Criar `ThumbnailModerationAppService` com aprovação/rejeição e notificação ao dev.
5. Na home/catálogo, usar animado no hover ou em cards "destaque".

### Testes
- `ThumbnailModerationAppService_Tests`: upload, aprovação, status após publicação.

---

## 22.4 — Page Integration: Scroll Lock & Viewport

### Requisito
O scroll do jogo dentro do iframe não deve afetar a página pai (Poki: "Prevent game viewport scrolling from affecting the parent page").

### Tarefas
1. Adicionar CSS/JS no `GameFrameComponent` para `overscroll-behavior: contain` e `touch-action: none` quando focado.
2. Enviar mensagem `focus`/`blur` do iframe para o host; ao receber `focus`, desabilitar scroll da página pai; ao `blur`, reabilitar.
3. Adicionar teste E2E de scroll (quando disponível) ou teste unitário do listener.

---

## 22.5 — Controles Adaptativos

### Requisito
- Mobile/tablet deve forçar controles touch.
- Jogos com teclado devem usar ESC ou espaço para pausar/resumir.
- Cutscenes devem ser skippables.

### Tarefas
1. Criar `Game.ControlScheme` enum (`Keyboard`, `Touch`, `Both`) e metadados.
2. No `game-frame` detectar device e enviar `controlScheme` para o jogo via postMessage.
3. Capturar `keydown` ESC/Space na página e enviar `pauseRequested`/`resumeRequested` quando o jogo está em foco.
4. Na descrição/overlay do jogo, exibir hints de controles (teclas desktop / gestos mobile).
5. Adicionar campo `Game.CutscenesSkippable` (bool); se true, exibir botão "Pular" após 2s em telas de cutscene.

### Testes
- Testes unitários do `GameFrameComponent` para eventos de teclado e `controlScheme`.

---

## 22.6 — Localization do Jogo

### Requisito
Jogos com muito texto devem oferecer múltiplos idiomas e layouts adaptáveis.

### Tarefas
1. Adicionar `Game.SupportedLanguages` (lista de culturas) e `Game.DefaultLanguage`.
2. `GameplayBridgeService` expor `getLanguage(): Promise<string>` e `setLanguage(lang)`.
3. Armazenar preferência de idioma do jogador (autenticado: backend; anônimo: localStorage).
4. Na página do jogo, seletor de idioma quando `SupportedLanguages.Count > 1`.

### Testes
- `PlayerPreferenceAppService` (ou similar) salva/retorna idioma.

---

## 22.7 — Portal do Desenvolvedor v2 (P4D)

### Requisito
Evoluir o portal para "Poki For Developers": equipes, permissões, versões, playtests e preview.

### Tarefas
1. **Team Settings**:
   - `DeveloperTeam` entity (`Name`, `PrimaryContactEmail`, `Country`).
   - `DeveloperTeamMember` entity (`UserId`, `TeamId`, `Role`: `Developer` | `Support` | `Billing`).
   - `IDeveloperTeamAppService` convidar/remover membros.
2. **Billing Info**:
   - `DeveloperBillingProfile` (tax info, payment method placeholder, sem dados reais).
   - Endpoint para dev preencher/aprovar (apenas campos, sem integração real).
3. **Versions**:
   - Listar todas as builds publicadas e em rascunho por jogo.
   - "Upload New Version" reutiliza `GameBuildAppService`.
   - "Open in Inspector" e "Preview on Game Hub".
4. **Playtests**:
   - `PlaytestSession` entity (`GameId`, `Status`, `RecordingUrl`, `CreatedAt`).
   - `IPlaytestAppService` solicitar/agendar playtests e listar gravações.
5. **Preview Mode**:
   - Rota pública `/preview/:gameSlug/:version` acessível apenas com token de preview (claims `gameId`, `preview: true`).
   - No admin/dev, botão "Preview" gera token JWT curto com `Preview` permission.

### Testes
- `DeveloperTeamAppService_Tests`: roles, convite, remoção.
- `PlaytestAppService_Tests`: criação, listagem.
- `PreviewTokenProvider` gera/valida token.

---

## 22.8 — Inspector v3: Preview, QR Code e Checklist Persistente

### Requisito
Inspirado na aba Inspector da Poki: preview no Game Hub, QR code para mobile, checklist salvo.

### Tarefas
1. Na tela do Inspector, adicionar botão "Preview" que abre o jogo no Game Hub com flag `?inspector=1`.
2. Gerar QR code (serviço simples ou biblioteca front) para modo mobile; ao escanear, abre `/play/:slug?inspector=1`.
3. `InspectorSession` persistir respostas do checklist do QA module (`InspectorChecklistAnswer`).
4. `IInspectorAppService.ValidateSessionAsync` retornar percentual de conclusão do checklist.
5. Permitir re-run de validação sem reiniciar sessão.

### Testes
- `InspectorAppService_Tests`: checklist persistence e completion percentage.

---

## 22.9 — Política de Privacidade In-Game

### Requisito
Se o jogo faz requests externos (analytics, multiplayer), deve exibir uma UI de política de privacidade dentro do jogo e fornecer URL hospedada (não Google Docs).

### Tarefas
1. Criar endpoint `/api/services/app/Privacy/GetForGame?gameSlug=` retornando `PrivacyPolicyDto` (texto e URL).
2. No `GameplayBridgeService` expor `getPrivacyPolicy(): Promise<{ url, text }>`.
3. Criar componente `PrivacyConsentComponent` exibido antes do gameplay quando `Game.HasExternalRequests = true` e consentimento não salvo.
4. Salvar consentimento em `PlayerPrivacyConsent` (usuario + jogo + data).

### Testes
- `PrivacyAppService_Tests.GetForGame_returns_policy`.
- `PrivacyConsent` não bloqueia jogos sem external requests.

---

## Ordem Sugerida de Implementação

1. **22.1** (Cloud saves) — maior retenção de jogadores.
2. **22.2** (User accounts no SDK) — habilita login/token para jogos.
3. **22.4** (Scroll lock) + **22.5** (Adaptive controls) — UX rápida.
4. **22.3** (Thumbnails) + **22.6** (Localization) — qualidade de catálogo.
5. **22.9** (Privacy consent) — conformidade necessária antes de release.
6. **22.7** (P4D v2) + **22.8** (Inspector v3) — infra do desenvolvedor.

---

## Critérios de Aceite

- Toda funcionalidade relevante com testes backend e/ou frontend.
- `dotnet build Api/GameHub.sln` passando.
- `dotnet test Api/GameHub.sln --no-build` passando.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` passando.
- Documentação em `docs/agent-execution-log.md` e `CHANGELOG.md` atualizadas.
- Sem secrets commitados.
