# 25 — Próxima Fase Poki: Otimizações Finais, Onboarding e Monetização Avançada

> **Status:** implementado em `feature/poki-25-final`
> **Base:** análise de `https://sdk.poki.com/new-requirements.html`, `https://sdk.poki.com/playtesting` e `https://sdk.poki.com/deals`
> **Objetivo:** fechar os gaps de UX e qualidade identificados na documentação Poki, evoluir o P4D e preparar a plataforma para testes de onboarding e monetização.

---

## Contexto

O spec 24 entregou:

- ✅ P4D v2 (equipes, billing e playtests — backend + portal dev parcial).
- ✅ Inspector v3 QR code, scaling tests e checklist persistente.
- ✅ CLI parity com `gamehub.json` e `UploadFromCli`.
- ✅ Incognito/First-Party UX, Save System UX, Poki Pill e Versions tab actions.

Gaps identificados na documentação Poki e no estado atual do GameHub:

1. **Image Optimization warnings** — `GameBuildPackageValidator` ainda não analisa arquivos de imagem dentro do zip para sugerir compressão quando > 100 KB.
2. **General Team Settings UI** — `DeveloperTeam` tem campos, mas falta a tela no portal do desenvolvedor para editar nome, e-mail de contato principal e país.
3. **Playtest Recordings UI** — upload de gravação no backend existe; falta player de vídeo no admin e anotações por gravação.
4. **Rewarded Ad UX refinada** — Poki exige botão padrão igual/maior que o rewarded, verde, ao lado/acima; botões rewarded não podem ser verdes e devem ter ícone 🎬/🎞️; confirmar recompensa com animação/son.
5. **Onboarding / Easy Access Guide** — métricas de drop-off no primeiro minuto e guia de melhorias de onboarding.
6. **Engagement Guide** — sugestões automáticas baseadas em tempo de sessão e playtests.
7. **Revenue Share / Deal Types** — `RevenueContract` precisa suportar `WebExclusive` (5 anos, 70/30 ou 100/0 de acordo com origem do tráfego) e `NonExclusive` (flat fee).
8. **Performance & FPS** — alertas quando `AvgFps < 30` e garantia de 85% dos usuários com FPS aceitável por dispositivo.
9. **Suggested Categories & SEO** — sugerir até 4 categorias e validar `SeoDescription` no admin.
10. **Mystery Tile / Playtest discovery** — tile anônimo na home para convidar jogadores a playtests.

---

## 25.1 — Image Optimization Warnings

### Requisito
Analisar imagens dentro do zip de build e sugerir compressão quando houver assets grandes.

### Tarefas
1. Adicionar validador `ImageOptimizationValidator` em `GameBuildPackageValidator`.
2. Detectar `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif` e estimar bytes economizáveis.
3. Emitir warning quando imagem > 100 KB sem compressão aparente.
4. Exibir warning no relatório de validação do admin e no portal do desenvolvedor.

### Testes
- `BuildPackageValidator_Tests`: zip com imagens grandes e pequenas.

---

## 25.2 — General Team Settings UI

### Requisito
Permitir que o desenvolvedor edite informações gerais do time no P4D.

### Tarefas
1. Criar `DeveloperTeamAppService.UpdateGeneralSettingsAsync(name, primaryContactEmail, country)`.
2. Criar DTOs `DeveloperTeamGeneralSettingsDto` e `UpdateTeamGeneralSettingsInput`.
3. Adicionar tela `developer/team/settings` no Angular.
4. Garantir que `Support` não acesse earnings/métricas (verificar em `DeveloperEarningsAppService`/`DeveloperDashboardAppService`).

### Testes
- `DeveloperTeamAppService_Tests`: update e permissões por role.

---

## 25.3 — Playtest Recordings UI

### Requisito
Visualizar gravações de playtests no admin e adicionar anotações.

### Tarefas
1. Criar `PlaytestRecording` entity com `Url`, `DurationSeconds`, `DeviceType`, `CountryCode`, `ConsoleOutput`, `Notes`.
2. Adicionar `IPlaytestAppService.GetRecordingAsync`, `ListRecordingsAsync`, `AddNotesAsync`.
3. Criar componente admin `/app/main/gamehub/playtests/:id/recordings` com player de vídeo.
4. Exibir inputs do jogador e console output quando disponíveis.

### Testes
- `PlaytestAppService_Tests`: listagem e anotações.

---

## 25.4 — Rewarded Ad UX Refinada

### Requisito
Ajustar UI de recompensas para seguir as diretrizes Poki.

### Tarefas
1. No `GameFrameComponent`, quando `rewardedBreak` é oferecido:
   - Botão padrão "Continuar" (verde, igual ou maior que o rewarded).
   - Botão rewarded com ícone 🎬/🎞️, não verde, ao lado ou acima do padrão.
   - Exibir confirmação visual/sonora após recompensa.
2. Garantir que não se exija múltiplos vídeos consecutivos para uma recompensa.
3. Não conceder recompensa quando ad blocker detectado.

### Testes
- Testes de unidade Angular para lógica de botões; `AdBreakAppService_Tests` para regra de ad block.

---

## 25.5 — Onboarding / Easy Access Guide

### Requisito
Identificar onde jogadores abandonam o jogo no primeiro minuto e sugerir melhorias.

### Tarefas
1. Métrica `OnboardingDropOff` em `GameMetricSnapshot`: % de sessões com duração < 60s.
2. `AdminDashboardAppService.GetOnboardingInsightsAsync(gameId)` com segmentação por dispositivo/país.
3. Sugestões automáticas (ex.: "Adicione skip no tutorial" se drop-off alto).
4. Página no admin `GameHub → Quality → Onboarding`.

### Testes
- `AdminDashboardAppService_Tests`: agregação de drop-off e insights.

---

## 25.6 — Engagement Guide

### Requisito
Sugerir melhorias baseadas em tempo de sessão e playtests.

### Tarefas
1. `GameMetricSnapshot.AvgSessionDuration` e `MedianSessionDuration`.
2. Comparar com benchmark por categoria.
3. Exibir alerta se média < 2 minutos.
4. Integrar com playtests para marcar sessões de teste e não contar em métricas de produção.

---

## 25.7 — Revenue Share / Deal Types

### Requisito
Suportar contratos `WebExclusive` e `NonExclusive` com regras de split por origem.

### Tarefas
1. Adicionar `RevenueContractType` enum (`WebExclusive`, `NonExclusive`).
2. `RevenueSplitCalculator` usar regras:
   - `WebExclusive`: 100% dev quando tráfego direto; 50% dev quando Poki traz.
   - `NonExclusive`: flat fee fixo; sem revenue share adicional.
3. Vincular `TrafficSource` (Direct, Poki, Campaign) em `PlaySession` e `RevenueEvent`.
4. Exibir estimativa de earnings por origem no painel do desenvolvedor.

### Testes
- `RevenueContractAppService_Tests`: split por tipo e origem.

---

## 25.8 — Performance & FPS

### Requisito
Alertas de FPS e garantia de performance por dispositivo.

### Tarefas
1. `GameplayBridgeService.measureFps` enviar `FpsMeasured` com média/mínimo.
2. `GameMetricSnapshot` agregar percentual de sessões com `AvgFps >= 30` e `< 30`.
3. Admin health alert quando < 85% das sessões atingem 30 FPS em um dispositivo.
4. Página de performance por jogo no admin.

### Testes
- `GameplayAppService_Tests` e `AdminDashboardAppService_Tests` para agregação e alertas.

---

## 25.9 — Suggested Categories & SEO

### Requisito
Sugerir categorias e validar SEO no admin.

### Tarefas
1. `AdminGameAppService.SuggestCategoriesAsync(gameId)` baseado em título, descrição e tags.
2. Validar `SeoDescription` (150-160 chars) e `SuggestedDescription` no formulário.
3. Exibir preview de busca no admin.

### Testes
- `AdminGameAppService_Tests`: sugestão e validação de SEO.

---

## 25.10 — Mystery Tile / Playtest Discovery

### Requisito
Tile anônimo na home para convidar jogadores a playtests.

### Tarefas
1. `PlaytestSession` com flag `IsDiscovery` e `DisplayProbability`.
2. Endpoint público `GetMysteryTileAsync` retornando jogo de playtest ativo quando aprovado.
3. Angular `home.component` exibir tile "Mistério" com CTA para playtest.
4. Gravar consentimento de gravação no `PlaySession`.

### Testes
- `GameCatalogAppService_Tests`: mistery tile respeita flags e consentimento.

---

## Critérios de Aceite

- `dotnet build Api/GameHub.sln` sem warnings.
- `dotnet test Api/GameHub.sln --no-build` passando.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/`.
- Novas entidades com migrações EF Core.
- `docs/agent-execution-log.md`, `CHANGELOG.md`, `.specs/25-proxima-sessao-poki.md` e `README.md` atualizados.
- Commits por funcionalidade; push e PR apenas no final.

---

## Notas

- Itens 25.4 e 25.10 envolvem UX visual e podem exigir assets de exemplo (usar placeholders).
- 25.7 (deal types) pode impactar modelagem financeira; validar com stakeholder antes de persistir dados reais.
- Manter 85% de usuários com FPS aceitável requer métricas suficientes; em ambiente de teste usar thresholds adaptativos.
