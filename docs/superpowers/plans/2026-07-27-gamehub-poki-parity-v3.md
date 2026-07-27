# Plano de Execução — GameHub Poki Parity v3

> **Data:** 2026-07-27
> **Base:** Análise do repositório GameHub, specs `.specs/34-51`, documentação Poki (`https://sdk.poki.com/*`) e estado atual do EAF 9.3.1
> **Objetivo:** Definir as próximas entregas de hardening operacional, UX do portal do desenvolvedor, fluxo de publicação, analytics/earnings e documentação.

---

## Resumo Executivo

As funcionalidades principais inspiradas na Poki já estão implementadas no GameHub: catálogo, player, SDK bridge, portal do desenvolvedor, moderação, analytics, multiplayer/AUDS, chat/presença/notificações, Inspector, playtests, monetização e fluxo de publicação. A EAF foi atualizada para 9.3.1 com os contratos contextuais de chat, rate limit e auditoria.

Os gaps remanescentes identificados nas docs do Poki e no código são de **polimento operacional**, **consistência de UX do portal**, **documentação do usuário** e **cobertura de testes**. Este plano detalha as alterações necessárias para fechar a Poki Parity v3 sem alterar o EAF.

---

## Fase 1 — Hardening Operacional

### 1.1 Objetivo

Padronizar headers de rate limit, idempotência, envelope de erro público e health checks, reduzindo riscos operacionais e melhorando a experiência do SDK.

### 1.2 Símbolos e alterações

#### `Api/src/GameHub.Web.Host/Middleware/RateLimitingMiddleware.cs`

```csharp
public class RateLimitingMiddleware
{
    public async Task Invoke(HttpContext context);
    private static void AddRateLimitHeaders(HttpContext context, int limit, int remaining, DateTimeOffset reset);
    private static string GetClientRequestId(HttpContext context);
    private Task<bool> IsDuplicateRequestAsync(string clientRequestId, TimeSpan window);
    private static RateLimitRule ResolveRule(HttpContext context);
    // ...existing helpers
}
```

**Alterações:**
1. Adicionar `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` em todas as respostas.
2. Ler header `X-Client-Request-Id`; quando presente, usá-lo como chave de idempotência por 5 min para métodos mutáveis.
3. Preservar `Retry-After` em `429`.

#### `Api/src/GameHub.Web.Host/Middleware/PublicErrorMiddleware.cs` (novo)

```csharp
public class PublicErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PublicErrorMiddleware> _logger;

    public PublicErrorMiddleware(RequestDelegate next, ILogger<PublicErrorMiddleware> logger);
    public async Task Invoke(HttpContext context);
    private static SdkError MapToSdkError(Exception ex, string correlationId);
}
```

**Alterações:**
1. Capturar exceções não tratadas após `UseExceptionHandler`.
2. Retornar `SdkError` com `code`, `message`, `retryable`, `correlationId`.
3. Nunca expor stack trace, connection strings ou PII.
4. Registrar em Serilog com `CorrelationId`, `TenantId`, `UserId`, `RequestPath`.

#### `Api/src/GameHub.Web.Host/Startup/Startup.cs`

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    app.UseResponseCompression();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ContentSecurityPolicyMiddleware>();
    app.UseCookiePolicy();
    app.UseEafHealthChecks();
    app.UseMiddleware<PublicErrorMiddleware>(); // novo
    if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    app.UseJwtTokenMiddleware();
    app.UseAbpRequestLocalization();
    app.UseRouting();
    app.UseCors(GameHubConsts.DefaultCorsPolicyName);
    app.UseMiddleware<RateLimitingMiddleware>();
    app.UseEndpoints(...);
}
```

#### Health checks

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<GameHubDbContext>("database")
    .AddCheck<MultiplayerPresenceHealthCheck>("multiplayer_presence_cache")
    .AddCheck<RedisCacheHealthCheck>("redis_cache");
```

### 1.3 Fluxo de dados

```text
Request → SecurityHeaders → CSP → CookiePolicy → PublicErrorMiddleware
  → JwtToken → Localization → Routing → CORS → RateLimitingMiddleware
    → (API/Hub)
```

### 1.4 Testes

- `RateLimitingMiddleware_Tests`: headers, 429, idempotência.
- `PublicErrorMiddleware_Tests`: exceção genérica, `GameHubException`, PII.
- `HealthChecks_Tests`: database, redis, multiplayer presence.

---

## Fase 2 — Portal do Desenvolvedor v3 UX

### 2.1 Objetivo

Tornar o portal consistente, acessível e resiliente a erros de rede, com shell compartilhado e estados padronizados.

### 2.2 Símbolos e alterações

#### `angular/src/app/developer/components/developer-shell/developer-shell.component.ts` (novo)

```typescript
@Component({
  selector: 'app-developer-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './developer-shell.component.html',
  styleUrl: './developer-shell.component.css',
})
export class DeveloperShellComponent {
  isMobileNavOpen = signal(false);
  toggleMobileNav(): void;
}
```

#### `angular/src/app/developer/shared/page-state.model.ts` (novo)

```typescript
export interface PageState<T> {
  loading: boolean;
  empty: boolean;
  error: SdkError | null;
  retry: () => void;
  data: T | null;
}
```

#### `angular/src/app/developer/games/games.component.ts`

```typescript
export class DeveloperGamesComponent implements OnInit, OnDestroy {
  private readonly state = signal<PageState<GameSummary[]>>({ loading: false, empty: false, error: null, retry: () => {}, data: null });
  readonly vm = computed(() => this.state());

  loadGames(): void {
    this.state.set({ ...this.state(), loading: true, error: null });
    this.developerService.getMyGames(0, 100)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => this.state.set({ loading: false, empty: result.items.length === 0, error: null, retry: () => this.loadGames(), data: result.items }),
        error: err => this.state.set({ loading: false, empty: false, error: mapToSdkError(err), retry: () => this.loadGames(), data: null })
      });
  }
}
```

#### `angular/src/app/core/services/error-mapper.service.ts` (novo)

```typescript
@Injectable({ providedIn: 'root' })
export class ErrorMapperService {
  map(err: HttpErrorResponse): SdkError;
  private fromApiError(error: any): SdkError;
}

export interface SdkError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
  retryAfter?: number;
}
```

### 2.3 Fluxo de controle

```text
DeveloperShell
  ├── Sidebar (desktop) / MobileNav
  └── <router-outlet>
        ├── DashboardComponent (PageState)
        ├── GamesComponent (PageState)
        ├── BuildsComponent (PageState)
        ├── EarningsComponent (PageState)
        └── ProfileComponent (PageState)
```

### 2.4 Testes

- `DeveloperShellComponent_Tests`: navegação, mobile, landmarks.
- `GamesComponent_Tests`: filtro por status, confirmação de submissão, retry.
- `EarningsComponent_Tests`: filtro de período, expansão diária, retry.

---

## Fase 3 — Fluxo de Publicação

### 3.1 Objetivo

Conectar o ciclo `draft → build → validation → preview → review → publish` de forma única e auditável no portal do desenvolvedor.

### 3.2 Símbolos e alterações

#### `angular/src/app/developer/versions/versions.component.ts` (novo)

```typescript
export interface GameVersion {
  id: string;
  gameId: string;
  gameSlug: string;
  version: string;
  buildNumber: number;
  status: string;
  sizeBytes: number;
  hashSha256: string;
  createdAt: string;
  publishedAt?: string;
  validationSummary?: ValidationSummary;
}

@Component({...})
export class DeveloperVersionsComponent implements OnInit {
  versions: GameVersion[] = [];
  loading = false;
  error: SdkError | null = null;

  openInspector(version: GameVersion): void;
  openPreview(version: GameVersion): void;
  submitForReview(version: GameVersion): void;
  canSubmitForReview(version: GameVersion): boolean;
}
```

#### `angular/src/app/core/services/developer.service.ts`

```typescript
getVersions(gameId: string): Observable<GameVersion[]>;
startInspectorForBuild(gameId: string, buildId: string, devicePreset: string, resolution: string): Observable<InspectorSessionResult>;
createPreviewTokenForBuild(gameId: string, version: string): Observable<PreviewTokenResult>;
```

#### `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`

```csharp
public class DeveloperGameAppService : ...
{
    public async Task<ListResultDto<GameVersionDto>> GetVersionsAsync(Guid gameId);
    public async Task<PreviewTokenDto> CreatePreviewTokenForBuildAsync(CreatePreviewTokenForBuildInput input);
    public async Task<InspectorSessionDto> StartInspectorSessionForBuildAsync(StartInspectorSessionForBuildInput input);
    public async Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input);
}
```

### 3.3 Regras de negócio

- `canSubmitForReview` retorna `true` somente quando `latestBuildStatus == 'Approved'`.
- Build inválido não pode ser submetido; frontend desabilita ação e backend retorna `validation_failed`.
- Preview token é curto (<= 15 min) e vinculado a `gameId`/`buildId`.
- Histórico de revisão é exibido como timeline no `versions` e `builds`.

### 3.4 Testes

- `DeveloperGameAppService_Tests`: submissão bloqueada com build inválido, preview token expirado, tenant isolation.
- `VersionsComponent_Tests`: ações habilitadas/desabilitadas por status.

---

## Fase 4 — Analytics & Earnings UI v3

### 4.1 Objetivo

Tornar métricas e receita úteis para decisões de publicação, com filtros avançados e exportação CSV, sem apresentar estimativas como valores pagos.

### 4.2 Símbolos e alterações

#### `Api/src/GameHub.Application/Gameplay/GameMetricsAppService.cs`

```csharp
public class GameMetricsAppService : ...
{
    public async Task<GameMetricsResult> GetMetricsAsync(Guid gameId, GameMetricsFilter input);
    public async Task<GameMetricsExportDto> ExportCsvAsync(Guid gameId, GameMetricsFilter input);
}
```

#### `Api/src/GameHub.Application/Developer/DeveloperEarningsAppService.cs`

```csharp
public class DeveloperEarningsAppService : ...
{
    public async Task<DeveloperEarningsDto> GetEarningsAsync(DeveloperEarningsFilter input);
    public async Task<GameMetricsExportDto> ExportCsvAsync(DeveloperEarningsFilter input);
}
```

#### `angular/src/app/developer/earnings/earnings.component.ts`

```typescript
export class DeveloperEarningsComponent implements OnInit {
  filter = signal<DeveloperEarningsFilter>({});
  earnings = signal<DeveloperEarnings | null>(null);
  loading = signal(false);
  error = signal<SdkError | null>(null);

  applyFilter(): void;
  exportCsv(): void;
  trackByGame(index: number, item: GameEarnings): string;
}
```

### 4.3 Regras de negócio

- Filtro de período não aceita `from > to`.
- Receita bruta, dev e plataforma sempre separadas.
- Aviso "Valores estimados; não representam payout confirmado" visível.
- CSV sem PII, datas ISO 8601 UTC, escaping RFC 4180.
- Isolamento por tenant e autorização do desenvolvedor.

### 4.4 Testes

- `GameMetricsAppService_Tests`: CSV com vírgula/aspas, deduplicação, playtests excluídos.
- `DeveloperEarningsAppService_Tests`: período vazio, múltiplos jogos, filtros, isolamento.

---

## Fase 5 — User Guide Completo

### 5.1 Objetivo

Fornecer documentação operacional suficiente para um desenvolvedor publicar, validar, revisar e acompanhar um jogo sem consultar código.

### 5.2 Arquivos

- `angular/src/app/public/docs/user-guide/user-guide.component.html`
- `angular/public/i18n/pt-BR.json`
- `angular/public/i18n/en-US.json`
- `docs/user-guide.md` (novo)

### 5.3 Símbolos (i18n)

```json
{
  "docs.ug.browsing": "Navegando jogos",
  "docs.ug.browsingText": "Use a barra de busca ou as categorias para encontrar jogos. Clique em um card para ver detalhes e pressione Jogar.",
  "docs.ug.account": "Conta",
  "docs.ug.accountText": "Crie uma conta para salvar pontuações, favoritos e progresso. Modo anônimo usa localStorage do navegador.",
  "docs.ug.developerPortal": "Portal do desenvolvedor",
  "docs.ug.developerPortalText": "Acesse /developer após entrar. Dashboard, Meus Jogos, Builds, Receitas e Perfil.",
  "docs.ug.builds": "Builds e validação",
  "docs.ug.buildsText": "Envie um ZIP com index.html e assets. Revise warnings (tamanho, imagens, requests externos) antes de aprovar.",
  "docs.ug.publishingWorkflow": "Fluxo de publicação",
  "docs.ug.publishingWorkflowText": "Upload → Validação → Preview → Inspector → Aprovação → Submissão → Revisão → Publicação.",
  "docs.ug.metrics": "Métricas e receita",
  "docs.ug.metricsText": "Filtre por período, país e dispositivo. Valores são estimados e não representam payout confirmado.",
  "docs.ug.security": "Segurança e privacidade",
  "docs.ug.securityText": "Nunca insira tokens, API keys ou connection strings no ZIP. Declare domínios externos quando solicitado."
}
```

### 5.4 Testes

- `UserGuideComponent_Tests`: seções renderizadas, traduções carregadas.
- Script de verificação de chaves i18n ausentes.

---

## Fase 6 — Runbooks e Test Coverage

### 6.1 Runbooks

- `docs/runbooks/rate-limit.md`: como interpretar headers, `429` e `Retry-After`.
- `docs/runbooks/multiplayer-cache.md`: TTL, heartbeat, grace period, diagnóstico.
- `docs/runbooks/signalr-backplane.md`: quando habilitar, limitações, troubleshooting.
- `docs/runbooks/health-checks.md`: endpoints, estados degraded/unhealthy.

### 6.2 Cobertura de testes

- Backend: manter ≥ 80% nos serviços alterados.
- Frontend: adicionar specs para `DeveloperShellComponent`, `GamesComponent`, `EarningsComponent`, `VersionsComponent`, `UserGuideComponent`.
- Integração: teste de duas instâncias com Redis para presença multiplayer (quando Redis disponível).

---

## Critérios de Aceite Gerais

1. `dotnet build Api/GameHub.sln -c Release --no-restore` sem erros/warnings novos.
2. `dotnet test Api/GameHub.sln -c Release --no-build` passando.
3. `npm run build` em `angular/` e `angular-admin/GameHub.UI/` passando.
4. Nenhum secret ou configuração real commitada.
5. Documentação (`docs/agent-execution-log.md`, `CHANGELOG.md`, runbooks) atualizada.
6. Commits por funcionalidade; PR para `main`.

---

## Notas

- Este plano não altera o EAF; usa os contratos já publicados em 9.3.1.
- Payout/billing real continua fora de escopo.
- Recomendações personalizadas/ML continuam fora do MVP.
