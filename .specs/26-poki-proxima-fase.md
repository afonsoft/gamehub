# 26 — Próxima Fase Poki: Quality Guidelines, Error Scanner, Player Fit e Infra de Multiplayer

> **Status:** parcial — 26.1 a 26.8, 26.11 e 26.12 implementados; 26.9 (Netlib) e 26.10 (AUDS) pendentes para sessão dedicada.
> **Base:** análise de `https://sdk.poki.com/new-requirements`, `https://sdk.poki.com/sdk-documentation` e `https://sdk.poki.com/playtesting`
> **Objetivo:** fechar os gaps remanescentes da documentação Poki e evoluir a plataforma para suportar analytics de erros, player fit, quality gates e multiplayer.

---

## Contexto

O spec 25 implementou:

- Image optimization warnings.
- General Team Settings UI.
- Playtest Recording UI com player e notas.
- Rewarded ad UX refinada (botão verde padrão, não-verde rewarded, single reward, ad-block no reward).
- Onboarding / Easy Access Guide (drop-off rate e sugestões).
- Engagement Guide (duração média/mediana e benchmark por categoria).
- Revenue share / deal types (`WebExclusive`/`NonExclusive`, split rules, flat fee, traffic source).
- Performance & FPS (agregação e alertas).
- Suggested categories & SEO validation.
- Mystery Tile / Playtest Discovery.

Itens implementados e gaps remanescentes na documentação Poki:

1. **Error Scanner / Painel de erros do jogo** — documentação menciona "Details for all of errors of the past 24 hours, updated hourly". Faltam: `GameError` entity, agregação por jogo/build, dashboard no admin e alertas.
2. **Daily Playing Users e Conversion Funnel** — Poki rastreia "page visitors and the percentage of users who are able to play". Faltam: DPU, plays/visitas, loading conversion, funnels por device.
3. **Player Feedback Analytics** — ratings e comentários consolidados por jogo, sentimento, distribuição de notas e alertas de score baixo.
4. **Poki Quality Guidelines gates** — validação automatizada de conteúdo: profanidade em nomes/títulos, detecção de conteúdo sensível, thumbnail sem texto/overlay, ausência de links externos, ausência de IAP UI.
5. **External Resources Policy & Analytics Exemptions** — permitir que devs declarem analytics de terceiros (GameAnalytics etc.) e forneçam privacy statement; workflow de aprovação por moderador.
6. **Game Thumbnail Guide enforcement** — dimensões mínimas, proporção, tamanho máximo, formato recomendado (WebP), ausência de bordas/texto.
7. **Playtesting avançado / Difficulty Balancing** — métricas de heatmap por nível, taxa de morte/repetição, detecção de níveis difíceis ou entediantes a partir dos playtest recordings.
8. **Player Fit Test / Web Fit (Level 3)** — métricas de retenção em 1/7/30 dias, stickiness, benchmark por categoria e sinal de "bom fit" para publicação.
9. **Poki Networking Library (Netlib) e Multiplayer** — suporte a partidas online com WebSockets/SignalR, salas, matchmaking leve e persistência de estado.
10. **Arbitrary User Data Store (AUDS)** — backend genérico de chave/valor JSON para jogos que precisam de dados na nuvem sem schema fixo, com cotas e TTL.
11. **Submission / Approval Workflow** — formulário Poki for Developers, fila de submissão, triagem manual e status `Submitted` -> `InReview` -> `ApprovedForPublishing`.
12. **Earnings & Ad Reports** — relatório de impressões, CPM, split por tipo de ad e payout estimado.

---

## 26.1 — Error Scanner ✅

### Requisito
Capturar e agregar erros do jogo, exibir painel de erros e enviar alertas.

### Tarefas
1. Criar `GameErrorLog` entity (`SessionId`, `GameId`, `BuildId`, `Message`, `StackTrace`, `Source`, `Severity`, `Timestamp`).
2. Expôr endpoint `GameplayBridgeService.reportError` e `GameplayAppService.CaptureErrorAsync`.
3. Criar `AdminDashboardAppService.GetErrorScannerAsync(gameId?, buildId?, hours=24)` com agregação por mensagem e contagem.
4. Health alert quando erros > threshold (ex: 10/h).
5. UI admin com tabela e gráfico de erros.

### Testes
- `GameplayAppService_Tests`: captura de erro e agregação.
- `AdminDashboardAppService_Tests`: top errors e alertas.

---

## 26.2 — Daily Playing Users e Conversion Funnel ✅

### Requisito
Métricas de DPU e conversão de visitas em plays.

### Tarefas
1. Adicionar `GameMetricSnapshot.DailyPlayingUsers`, `PageViews`, `PlayStarts`, `LoadingFinishedCount`, `GameplayStartedCount`.
2. Job diário `GameMetricsAggregationJob` calcular funil.
3. Endpoint `AdminDashboardAppService.GetConversionFunnelAsync`.
4. UI admin com gráfico de funil.

### Testes
- `GameMetricsAggregationJob_Tests`: agregação de funil.

---

## 26.3 — Player Feedback Analytics ✅

### Requisito
Consolidar avaliações e comentários por jogo.

### Tarefas
1. `PlayerFeedbackAnalyticsAppService.GetFeedbackSummaryAsync(gameId)` com avg rating, distribution, total reviews.
2. Agregar `UserContent` reviews por jogo em `GameMetricSnapshot`.
3. Alerta quando avg rating < 3.0 em N reviews.
4. UI admin e developer earnings/dashboard.

### Testes
- `PlayerFeedbackAnalyticsAppService_Tests`.

---

## 26.4 — Quality Guidelines Gates ✅

### Requisito
Validações automáticas de conteúdo antes da publicação.

### Tarefas
1. Estender `BuildPackageValidator` para detectar textos em thumbnails (placeholder: verificar nome de arquivo ou metadata).
2. Verificar ausência de URLs/links no build (outgoing links, social links).
3. Verificar ausência de termos de IAP/currency purchase no texto do jogo (scan `index.html` strings).
4. Integrar `ProfanityFilter` no título, descrição e nomes de arquivos.
5. Quality score com falhas bloqueantes.

### Testes
- `BuildPackageValidator_Tests`.

---

## 26.5 — External Resources & Analytics Exemptions ✅

### Requisito
Permitir analytics de terceiros mediante aprovação e privacy statement.

### Tarefas
1. Adicionar `Game.AllowedExternalDomains` e `Game.PrivacyStatementUrl`.
2. Criar `IExternalResourceAppService` para dev declarar domínios de analytics.
3. Moderador aprova/rejeita com `Pages.Moderation.Review`.
4. `BuildPackageValidator` permite domínios aprovados e bloqueia não aprovados.

### Testes
- `ExternalResourceAppService_Tests`.

---

## 26.6 — Thumbnail Guide Enforcement ✅

### Requisito
Aplicar regras de thumbnail da Poki.

### Tarefas
1. `GameBuildPackageValidator` validar dimensões mínimas 640x360, proporção 16:9, tamanho máximo 2 MB, formato WebP/PNG/JPEG.
2. `AdminGameAppService.ApproveThumbnailAsync` rejeitar thumbnails que não atendam ao guia.
3. UI developer mostrar preview com overlay de recomendações.

### Testes
- `BuildPackageValidator_Tests`.

---

## 26.7 — Playtesting Difficulty Balancing ✅

### Requisito
A partir dos playtest recordings, inferir dificuldade e retenção por nível.

### Tarefas
1. Adicionar `PlaytestRecording.LevelEvents` (JSON com eventos por nível/tempo).
2. `PlaytestAnalyticsAppService.GetDifficultyInsightsAsync(playtestId)` retornar taxa de morte/replay por nível.
3. UI admin mostrar heatmap simplificado (tabela de nível x métricas).

### Testes
- `PlaytestAnalyticsAppService_Tests`.

---

## 26.8 — Player Fit / Web Fit Test ✅

### Requisito
Métricas de retenção e stickiness para decisão de publicação.

### Tarefas
1. `PlayerAccountAppService` rastrear `FirstPlayDate`, `LastPlayDate`.
2. Job calcular retenção 1d/7d/30d por jogo.
3. `AdminDashboardAppService.GetPlayerFitAsync(gameId)` retornar retenção, stickiness, benchmark.
4. Recomendação `GoodFit`/`NeedsImprovement`.

### Testes
- `PlayerFitAppService_Tests`.

---

## 26.9 — Netlib / Multiplayer (base) ⏳

### Requisito
Suporte a partidas online leves.

### Tarefas
1. Adicionar `Game.SupportsMultiplayer`, `MaxPlayersPerMatch`.
2. `IMatchmakingService` com fila por `GameId` e `Mode`.
3. SignalR hub `GameHubMatchHub` para mensagens de jogo.
4. `MatchState` entity para persistir estado.

### Testes
- `MatchmakingService_Tests`.

---

## 26.10 — Arbitrary User Data Store (AUDS) ⏳

### Requisito
Backend genérico de chave/valor JSON para jogos.

### Tarefas
1. `ArbitraryUserData` entity (`GameId`, `UserId`, `Key`, `ValueJson`, `ExpiresAt`).
2. `IArbitraryUserDataAppService` com Get/Set/Delete e quota por game/user.
3. Bridge `GameplayBridgeService.saveArbitrary`/`loadArbitrary`.

### Testes
- `ArbitraryUserDataAppService_Tests`.

---

## 26.11 — Submission / Approval Workflow ✅

### Requisito
Fechamento do ciclo Poki for Developers: submissão e triagem.

### Tarefas
1. `GameStatus` add `Submitted`, `InReview`, `ApprovedForPublishing`.
2. `DeveloperGameAppService.SubmitForReviewAsync(gameId)` valida quality gates.
3. `AdminGameAppService.StartReviewAsync`, `ApproveForPublishingAsync`, `RequestChangesAsync`.
4. UI developer mostrar status e feedback.

### Testes
- `GameSubmissionAppService_Tests`.

---

## 26.12 — Earnings & Ad Reports ✅

### Requisito
Relatórios de ad impressions e revenue detalhados.

### Tarefas
1. `AdImpression` entity (`GameId`, `BuildId`, `Type`, `Provider`, `Country`, `Device`, `Cpm`, `Earnings`).
2. `DeveloperEarningsAppService.GetAdReportAsync` com filtros e totais.
3. UI developer com gráfico de earnings por dia/tipo.

### Testes
- `DeveloperEarningsAppService_Tests`.

---

## Notas

- Itens 26.9 e 26.10 são mais complexos e devem ser delegados a uma sessão dedicada; criar `27-poki-multiplayer-auds.md` quando for iniciar.
- Prioridade sugerida: 26.1 → 26.2 → 26.3 → 26.11 → 26.4 → 26.6 → 26.12 → 26.7 → 26.8 → 26.5 → 26.10 → 26.9.
