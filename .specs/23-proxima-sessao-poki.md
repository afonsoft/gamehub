# 23 — Próxima Sessão: Poki Thumbnails, P4D v2 e Inspector v3

> **Status:** pendente
> **Base:** análise de `https://sdk.poki.com/new-requirements.html`, `https://sdk.poki.com/what-is-p4d.html`, `https://sdk.poki.com/poki-inspector.html` e `https://github.com/poki/poki-cli`
> **Objetivo:** fechar os itens remanescentes do spec 22 e incorporar requisitos de qualidade/publicação da Poki.

---

## Contexto

A sessão anterior (`feature/poki-cloud-accounts`) entregou:

- ✅ 22.1 — Cloud saves (`SupportsCloudSaves`, `save`/`load`, `DeleteAsync`, `gamehub_ignore_` fallback).
- ✅ 22.2 — User accounts no SDK (`getUser`, `getToken`, `login`, `GameTokenProvider`).
- ✅ 22.4 + 22.5 — Scroll lock, controles adaptativos, ESC/Space pause/resume, botão "Pular".
- ✅ 22.6 — Localization (`DefaultLanguage`, `SupportedLanguages`, `PlayerPreference`, `getLanguage`/`setLanguage`).
- ✅ 22.9 — In-game privacy consent (`PlayerPrivacyConsent`, `GetForGame`, `SaveConsent`).

Restam:

- 🔄 22.3 — Thumbnails estáticos e animados.
- 🔄 22.7 — Portal do Desenvolvedor v2 (P4D): equipes, billing, versions, playtests, preview mode.
- 🔄 22.8 — Inspector v3: preview, QR code, checklist persistente, scaling tests.

Novos requisitos identificados na documentação Poki para incluir nessa ou em sessões futuras:

1. **Splash screens / outgoing links** — validar que builds publicados não exibam splash screens e não tenham links externos não autorizados.
2. **Aspect ratio / scaling** — validar 16:9 e redimensionamento responsivo; adicionar "scaling tests" no Inspector com dimensões pré-definidas.
3. **File size target** — avisar quando o download inicial exceder 8 MB.
4. **Clean build** — detectar e bloquear artifacts de desenvolvimento (source maps desnecessários, debuggers, logs de console).
5. **Incognito / first-party cookies** — garantir que cloud saves e preferências funcionem sem third-party cookies.
6. **Poki CLI parity** — endpoint/API key para upload de builds via CLI/CI, gerando `gamehub.json` similar ao `poki.json`.
7. **Audit log de eventos do SDK** — Event Log persistente por sessão de Inspector para análise posterior.

---

## 23.1 — Thumbnails Estáticos e Animados (22.3)

### Requisito
Jogos publicados devem ter thumbnail estático e animado (GIF/WebP/MP4 curto) para release global.

### Tarefas
1. Adicionar `Game.AnimatedThumbnailUrl` e `Game.ThumbnailStatus` (`Pending`, `Approved`, `Rejected`) no domínio e DTOs.
2. No portal do desenvolvedor, wizard de upload de thumbnail com drag-and-drop e pré-visualização.
3. Após publicação, atualização de thumbnail entra em fila de moderação (`ThumbnailModerationAppService`).
4. Na home/catálogo, usar animado no hover em cards "destaque" (fallback para estático se ainda não aprovado).
5. Armazenar arquivos em MinIO/S3 com path `thumbnails/{gameId}/{static|animated}.{ext}`.

### Testes
- `ThumbnailModerationAppService_Tests`: upload, aprovação, rejeição e status após publicação.
- Teste de storage: upload e leitura de URL pública assinada.

---

## 23.2 — Portal do Desenvolvedor v2 (22.7)

### Requisito
Evoluir o portal para "Poki For Developers" com equipes, permissões, versions, playtests e preview.

### Tarefas
1. **Team Settings**:
   - `DeveloperTeam` entity (`Name`, `PrimaryContactEmail`, `Country`).
   - `DeveloperTeamMember` entity (`UserId`, `TeamId`, `Role`: `Developer` | `Support` | `Billing`).
   - `IDeveloperTeamAppService` convidar/remover membros.
2. **Billing Info**:
   - `DeveloperBillingProfile` (tax info, payment method placeholder, sem dados reais/sensíveis).
   - Endpoint para dev preencher/aprovar (apenas campos, sem integração real de pagamento).
3. **Versions**:
   - Listar todas as builds publicadas e em rascunho por jogo.
   - "Upload New Version" reutiliza `GameBuildAppService`.
   - "Open in Inspector" e "Preview on Game Hub".
4. **Playtests**:
   - `PlaytestSession` entity (`GameId`, `Status`, `RecordingUrl`, `CreatedAt`).
   - `IPlaytestAppService` solicitar/agendar playtests e listar gravações.
5. **Preview Mode**:
   - Rota pública `/preview/:gameSlug/:version` acessível apenas com token de preview (claims `gameId`, `preview: true`).
   - No admin/dev, botão "Preview" gera token JWT curto com permissão `Preview`.

### Testes
- `DeveloperTeamAppService_Tests`: roles, convite, remoção.
- `PlaytestAppService_Tests`: criação, listagem.
- `PreviewTokenProvider` gera/valida token.

---

## 23.3 — Inspector v3 (22.8)

### Requisito
Inspirado na aba Inspector da Poki: preview no Game Hub, QR code para mobile, checklist salvo e scaling tests.

### Tarefas
1. Na tela do Inspector, adicionar botão "Preview" que abre o jogo no Game Hub com flag `?inspector=1`.
2. Gerar QR code (serviço simples ou biblioteca front) para modo mobile; ao escanear, abre `/play/:slug?inspector=1`.
3. `InspectorSession` persistir respostas do checklist do QA module (`InspectorChecklistAnswer`).
4. `IInspectorAppService.ValidateSessionAsync` retornar percentual de conclusão do checklist.
5. Scaling tests: botões de dimensões 640x360, 836x470, 1031x580, portrait/landscape e popular devices.
6. Event Log persistente: salvar `InspectorSdkEvent` já criado e expor timeline na sessão.
7. Permitir re-run de validação sem reiniciar sessão.

### Testes
- `InspectorAppService_Tests`: checklist persistence, completion percentage, scaling dimensions.

---

## 23.4 — Novos Requisitos de Qualidade da Poki

### Tarefas
1. **Splash screens e outgoing links**:
   - Adicionar validação em `GameBuildPackageValidator` detectando `window.open`, `location.href` atribuições, `<a href="...">` e splash screens.
   - Bloquear publicação se encontrado, a menos que explicitamente permitido na review.
2. **Aspect ratio / scaling**:
   - Adicionar `Game.AspectRatio` enum (`Aspect16x9`, `Aspect4x3`, `Both`) e validar no build/inspector.
   - No Inspector, permitir trocar dimensões e verificar se o canvas preenche o viewport.
3. **File size target**:
   - Aviso quando build descompactado exceder 8 MB; rejeitar quando exceder 100 MB (já existe).
4. **Clean build**:
   - Detectar `.map`, `console.log`, `debugger;`, arquivos de teste, `node_modules` no zip e reportar como warning.
5. **Incognito / first-party**:
   - Garantir que cloud saves e `PlayerPreference` funcionem apenas com `localStorage` quando cookies de terceiros bloqueados.
6. **Poki CLI parity** (futuro):
   - Documentar contrato de `gamehub.json` e endpoint `POST /api/services/app/GameBuild/UploadFromCli` com API key.

### Testes
- `BuildPackageValidator_Tests` para splash, links, aspect ratio e file size.
- `InspectorAppService_Tests` para scaling e completion.

---

## Critérios de Aceite

- `dotnet build Api/GameHub.sln` passando.
- `dotnet test Api/GameHub.sln --no-build` passando.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` passando.
- Novas entidades cobertas por migrações EF Core.
- Documentação em `docs/agent-execution-log.md`, `CHANGELOG.md` e `.specs/23-proxima-sessao-poki.md` atualizada.
- Commits por funcionalidade; push e PR no final.

---

## Notas

- Thumbnails e Inspector v3 podem ser entregues primeiro por serem mais independentes.
- P4D v2 é o maior escopo; considerar dividir em specs 23.2a, 23.2b, 23.2c se necessário.
- Não commitar secrets, credenciais de API key ou dados reais de billing/pagamento.
