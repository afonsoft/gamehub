# 52 — Poki Parity v3: Hardening Operacional, UX do Portal e Documentação

> **Status:** Especificação para execução
> **Base:** `.specs/34-40`, `.specs/46-51`, `docs/superpowers/plans/2026-07-27-gamehub-next-steps.md` e análise das docs do Poki (`https://sdk.poki.com/*`)
> **Prioridade:** P0/P1
> **Dependências:** nenhuma alteração no EAF (EAF já em 9.3.1 com contratos contextuais)

---

## 1. Contexto

As specs 19–29, 46–51 e a integração EAF 9.3.1 entregaram a base funcional do GameHub: catálogo, player, SDK bridge, portal do desenvolvedor, moderação, analytics, multiplayer/AUDS, chat/presença/notificações, Inspector, playtests, monetização e fluxo de publicação.

A análise das docs do Poki (`html5`, `new-requirements`, `sdk-documentation`, `what-is-p4d`, `playtesting`, `poki-inspector`, `final-review`, `deals`, `faq`) mostra que os grandes pilares estão mapeados. Os gaps remanescentes são de **polimento operacional**, **consistência de UX do portal**, **documentação do desenvolvedor/jogador** e **cobertura de testes** — itens que bloqueiam uma release pública segura.

---

## 2. Objetivo

Fechar a parity v3 com a Poki em cinco frentes:

1. **Hardening operacional** — rate limit headers, idempotência, public error middleware, health checks e runbooks.
2. **Portal do Desenvolvedor v3** — shell compartilhado, estados de loading/erro/retry, acessibilidade e cancelamento de requests.
3. **Fluxo de Publicação** — versions tab integrada, histórico de revisão, preview/inspector por build e bloqueio de submissão com build inválido.
4. **Analytics & Earnings UI** — filtros avançados, exportação CSV, aviso de estimativa e consistência de totais.
5. **User Guide completo** — conteúdo pt-BR/en-US cobrindo jogador, desenvolvedor e admin.

---

## 3. Escopo

### 3.1 Hardening operacional

#### 3.1.1 RateLimitingMiddleware — headers completos e idempotência

**Arquivos:**
- `Api/src/GameHub.Web.Host/Middleware/RateLimitingMiddleware.cs`
- `Api/src/GameHub.Application/Dto/RateLimitHeaders.cs` (novo)
- `Api/src/GameHub.Web.Host/Configuration/RateLimitRules.cs`

**Símbolos:**

```csharp
// Adiciona headers de rate limit em toda resposta 200/201/202 também
void AddRateLimitHeaders(HttpContext context, int limit, int remaining, DateTimeOffset reset);

// Lê ClientRequestId do header e o adiciona como partition key alternativa
string GetClientRequestId(HttpContext context);

// Rejeita requisições duplicadas dentro de uma janela curta (idempotência)
Task<bool> IsDuplicateRequestAsync(string clientRequestId, TimeSpan window);
```

**Comportamento:**
- Responder com `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` em todas as requisições.
- Header `X-Client-Request-Id` opcional; quando presente, usado como chave de idempotência por 5 min para `POST`/`PUT`/`PATCH`.
- Manter `Retry-After` em `429`.

**Testes:**
- `RateLimitingMiddleware_Tests`: headers presentes, 429 com retry-after, idempotência por client request id.

---

#### 3.1.2 PublicErrorMiddleware — envelope SdkError para exceções não tratadas

**Arquivos:**
- `Api/src/GameHub.Web.Host/Middleware/PublicErrorMiddleware.cs` (novo)
- `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- `Api/src/GameHub.Application/Dto/SdkError.cs`

**Símbolos:**

```csharp
public class PublicErrorMiddleware
{
    public async Task Invoke(HttpContext context);
    private static SdkError MapToSdkError(Exception ex, string correlationId);
}
```

**Comportamento:**
- Capturar exceções não tratadas após `UseDeveloperExceptionPage`/`UseExceptionHandler` em produção.
- Retornar `SdkError` com `correlationId`, `code: temporarily_unavailable` ou `validation_failed`, `retryable: true/false`.
- Nunca expor stack trace, connection strings ou PII.
- Registrar em Serilog com `CorrelationId`, `TenantId`, `UserId`, `RequestPath`.

**Testes:**
- `PublicErrorMiddleware_Tests`: exceção genérica retorna 500 com SdkError; `GameHubException` retorna status mapeado.

---

#### 3.1.3 Health checks e runbooks

**Arquivos:**
- `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- `docs/runbooks/` (novos)

**Símbolos:**

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<GameHubDbContext>("database")
    .AddCheck<MultiplayerPresenceHealthCheck>("multiplayer_presence_cache")
    .AddCheck<RedisCacheHealthCheck>("redis_cache");
```

**Runbooks:**
- `docs/runbooks/rate-limit.md`
- `docs/runbooks/multiplayer-cache.md`
- `docs/runbooks/signalr-backplane.md`
- `docs/runbooks/health-checks.md`

---

### 3.2 Portal do Desenvolvedor v3 UX

#### 3.2.1 Shell compartilhado

**Arquivos:**
- `angular/src/app/developer/components/developer-shell/developer-shell.component.ts` (novo)
- `angular/src/app/developer/components/developer-shell/developer-shell.component.html` (novo)
- `angular/src/app/developer/components/developer-shell/developer-shell.component.css` (novo)
- `angular/src/app/developer/games/games.component.ts`
- `angular/src/app/developer/builds/builds.component.ts`
- `angular/src/app/developer/earnings/earnings.component.ts`
- `angular/src/app/developer/dashboard/dashboard.component.ts`

**Símbolos:**

```typescript
// Shell compartilhado com sidebar desktop e navegação horizontal mobile
export class DeveloperShellComponent {
  isMobileNavOpen = signal(false);
  toggleMobileNav(): void;
}

// Estado de página padronizado
export interface PageState {
  loading: boolean;
  empty: boolean;
  error: SdkError | null;
  retry: () => void;
}
```

**Comportamento:**
- Extrair sidebar/nav para componente compartilhado.
- Preservar rotas: `/developer`, `/developer/games`, `/developer/games/:id/builds`, `/developer/earnings`, `/developer/profile`, `/developer/team`.
- Adicionar `aria-label` em botões, foco visível e landmarks (`<main>`, `<nav>`).
- Tabelas com `<caption>`, `<thead>`, `scope="col"` e rolagem horizontal no mobile.

**Testes:**
- `DeveloperShellComponent_Tests`: navegação mobile, landmarks, rotas.

---

#### 3.2.2 Estados de loading, erro e retry

**Símbolos:**

```typescript
// games.component.ts
isLoading$ = signal(false);
isEmpty$ = signal(false);
error$ = signal<SdkError | null>(null);
retry$ = signal(() => this.loadGames());

loadGames(): void {
  this.isLoading$.set(true);
  this.error$.set(null);
  this.developerService.getMyGames().pipe(
    takeUntil(this.destroy$)
  ).subscribe({
    next: result => { /* ... */ },
    error: err => this.error$.set(mapToSdkError(err))
  });
}
```

**Comportamento:**
- Cancelar requisições pendentes com `takeUntil` ou `switchMap`.
- Retry apenas para erros `retryable === true`.
- Tratar `401` (login), `403` (sem permissão), `409` (conflito), `429` (rate limit + `Retry-After`).
- `window.confirm` substituído por modal/confirm component reutilizável (se já existir no shared UI).

---

### 3.3 Fluxo de Publicação

#### 3.3.1 Versions tab integrada

**Arquivos:**
- `angular/src/app/developer/versions/versions.component.ts` (novo)
- `angular/src/app/developer/builds/builds.component.ts`
- `angular/src/app/core/services/developer.service.ts`
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`

**Símbolos:**

```typescript
// versions.component.ts
export class DeveloperVersionsComponent {
  versions: GameVersion[] = [];
  openInspector(version: GameVersion): void;
  openPreview(version: GameVersion): void;
  submitForReview(version: GameVersion): void;
}

// DeveloperService
getVersions(gameId: string): Observable<GameVersion[]>;
startInspectorForBuild(gameId: string, buildId: string, devicePreset: string, resolution: string): Observable<InspectorSessionResult>;
createPreviewTokenForBuild(gameId: string, version: string): Observable<PreviewTokenResult>;
```

**Comportamento:**
- Listar builds com status, tamanho, data de upload/publicação, validação.
- Ações por versão: "Open in Inspector", "Preview on Game Hub".
- Build inválido não pode ser submetido (`canSubmitForReview` verifica `latestBuildStatus === 'Approved'`).
- Preview token expirado retorna `SdkError` e oferece regenerar.

**Testes:**
- `DeveloperGameAppService_Tests`: preview/inspector por build, submissão bloqueada para build inválido.
- `VersionsComponent_Tests`: ações desabilitadas para status inválido.

---

#### 3.3.2 Histórico de revisão

**Símbolos:**

```typescript
export interface DeveloperReviewHistoryItem {
  id: string;
  gameId: string;
  gameBuildId?: string;
  status: string;
  decision?: string;
  notes: string;
  createdAt: string;
  completedAt?: string;
}

// DeveloperGameAppService
Task<IReadOnlyList<DeveloperReviewHistoryDto>> GetReviewHistoryAsync(Guid gameId);
```

**Comportamento:**
- Exibir timeline de decisões no `builds` e `versions`.
- Nova submissão somente quando requisitos de qualidade forem satisfeitos.

---

### 3.4 Analytics & Earnings UI v3

#### 3.4.1 Filtros avançados e exportação

**Arquivos:**
- `angular/src/app/developer/earnings/earnings.component.ts`
- `angular/src/app/core/services/developer.service.ts`
- `Api/src/GameHub.Application/Gameplay/GameMetricsAppService.cs`
- `Api/src/GameHub.Application/Developer/DeveloperEarningsAppService.cs`

**Símbolos:**

```typescript
export interface GameMetricsFilter {
  from?: string;
  to?: string;
  countryCode?: string;
  deviceType?: string;
  trafficSource?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  isPlaytest?: boolean;
}

export interface DeveloperEarningsFilter {
  from?: string;
  to?: string;
  countryCode?: string;
  deviceType?: string;
  gameIds?: string[];
}

// GameMetricsAppService
Task<GameMetricsExportDto> ExportCsvAsync(Guid gameId, GameMetricsFilter input);

// DeveloperEarningsAppService
Task<GameMetricsExportDto> ExportEarningsCsvAsync(DeveloperEarningsFilter input);
```

**Comportamento:**
- Filtros por período, país, dispositivo, tráfego UTM.
- Exportação CSV UTF-8 com escaping RFC 4180, sem PII, datas ISO 8601 UTC.
- Aviso "Valores estimados; não representam payout confirmado" em todas as telas.
- Diferenciar zero, ausente e indisponível.

**Testes:**
- `GameMetricsAppService_Tests`: CSV com vírgula, aspas, Unicode; playtests excluídos.
- `DeveloperEarningsAppService_Tests`: período vazio, múltiplos jogos, filtros, deduplicação.

---

### 3.5 User Guide completo

**Arquivos:**
- `angular/src/app/public/docs/user-guide/user-guide.component.html`
- `angular/public/i18n/pt-BR.json`
- `angular/public/i18n/en-US.json`
- `docs/user-guide.md` (novo)

**Símbolos (i18n):**

```json
{
  "docs.ug.browsing": "Navegando jogos",
  "docs.ug.browsingText": "Use a barra de busca ou categorias...",
  "docs.ug.publishingWorkflow": "Fluxo de publicação",
  "docs.ug.publishingWorkflowText": "O fluxo é upload, validação, Preview, Inspector, aprovação, submissão, revisão e publicação..."
}
```

**Conteúdo obrigatório:**
1. Encontrar e jogar jogos.
2. Criar conta e entender modo anônimo.
3. Entrar no portal do desenvolvedor.
4. Criar e editar um jogo.
5. Enviar um build (ZIP, index.html, limites).
6. Interpretar validação (warnings, erros, image optimization, external requests).
7. Usar Preview e Inspector.
8. Submeter para revisão e responder a pedidos de alteração.
9. Consultar métricas e Earnings (estimados, timezone, filtros).
10. Reportar problemas e solicitar suporte.
11. Segurança e privacidade (não colocar tokens, declarar domínios externos).

**Testes:**
- `UserGuideComponent_Tests`: traduções carregadas, links internos válidos.
- `i18n` sem chaves ausentes (script de verificação).

---

## 4. Critérios de Aceite

1. `dotnet build Api/GameHub.sln -c Release --no-restore` sem erros/warnings novos.
2. `dotnet test Api/GameHub.sln -c Release --no-build` passando.
3. `npm run build` em `angular/` e `angular-admin/GameHub.UI/` passando.
4. `npm run test` (ou `ng test --no-watch`) em `angular/` com cobertura mínima de 60% dos componentes alterados.
5. Nenhum secret ou configuração real commitada.
6. Documentação em `docs/agent-execution-log.md`, `CHANGELOG.md` e runbooks atualizada.
7. Commits por funcionalidade; PR para `main`.

---

## 5. Fora de escopo

- Payout e billing real.
- Recomendações personalizadas/ML.
- Chat/social fora dos contratos já existentes.
- Alterações no EAF (já entregues em 9.3.1).
- Apps nativos iOS/Android.

---

## 6. Ordem Sugerida de Implementação

1. **Hardening operacional** — rate limit headers, idempotência, public error middleware (impacta todas as APIs).
2. **Developer Portal v3 UX** — shell, estados, acessibilidade (impacta todas as telas do portal).
3. **Publishing Workflow** — versions tab, review history, preview/inspector (desbloqueia submissão segura).
4. **Analytics/Earnings UI + CSV** — filtros e exportação (último passo antes de abrir para devs).
5. **User Guide** — documentação final sincronizada com as telas.
6. **Runbooks** — operação e troubleshooting.
7. **Test coverage** — cobrir gaps dos componentes alterados.
