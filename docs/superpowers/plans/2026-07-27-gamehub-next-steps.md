# Plano de Próximos Passos — GameHub + EAF 9.3.0

> **For agentic workers:** REQUIRED SUB-SKILL: `superpowers:executing-plans` ou `superpowers:subagent-driven-development`. Cada fase pode virar PR independente.

**Goal:** Implementar as specs 46–51 do GameHub, replicar as correções de segurança/observabilidade dos templates EAF 9.3.0 e evoluir os contratos compartilhados sem duplicar persistência.

**Architecture:** O GameHub continua como consumidor dos módulos EAF (identidade, cache, SignalR, chat, notificações). As melhorias do EAF 9.3.0 são replicadas nas configurações de host (`Startup`, `appsettings`, middleware) e nos contratos de erro/correlation ID; as capacidades sociais/contextuais novas são implementadas como fachada delegando para EAF quando o contrato já existir, sem criar segunda persistência.

**Tech Stack:** .NET 10, EAF 9.3.0, ASP.NET Boilerplate, Angular 20, PostgreSQL 16, Redis 7, Serilog, OpenTelemetry, xUnit/Shouldly/NSubstitute.

---

## Análise rápida — specs e estado atual

### Specs pendentes ou com status desatualizado

| Spec | Título | Status no repo | Observação |
|------|--------|----------------|------------|
| 15 | CSP & Security Headers | Draft | Middlewares já existem, mas **não estão registrados** em `Startup.Configure`. |
| 16 | Plano de Implementação dos Gaps | Draft | Ainda guia trabalho futuro; vários itens já parcialmente entregues. |
| 19.8 | Contas de Jogador e Favoritos | Planejado | `PlayerAccountAppService` e entidades existem — validar integração e testes. |
| 19.9 | Ads Provider | Planejado | `IAdProvider`/`AdBreakAppService` existem — falta regras de UX completas. |
| 19.10 | Inspector QA v2 | Planejado | `InspectorAppService`/`InspectorSdkEvent` existem — falta hardening. |
| 19.11 | Web Exclusives | Planejado | Não encontrado feature flag `WebExclusive` em `Catalog`. |
| 19.12 | Privacidade/UGC/Performance | Planejado | `PrivacyAppService`/`UserContent`/`ProfanityFilter` existem — falta integração SDK. |
| 22–26, 28–33 | Próximas fases Poki | Parciais/pendentes | Várias entidades já criadas; dependem de front/integração. |
| 34–40 | Especificação para execução | Em execução (PRs 65–69) | Portal, publicação, chat social, analytics já têm base de código. |
| **46** | **Moderação, segurança e operação** | **Novo (branch `devin/1785124000-next-platform-specs`)** | **Próximo passo prioritário (P0).** |
| **47** | **Analytics, exportação e operação** | **Novo** | **P0.** |
| **48** | **Portal/publicação/acessibilidade** | **Novo** | **P1.** |
| **49** | **SDK/privacidade/resiliência** | **Novo** | **P1.** |
| **50** | **Evoluções EAF** | **Novo / repositório EAF** | **P0/P1.** |
| **51** | **Roadmap de execução** | **Novo** | **Índice.** |

### Correções dos templates EAF 9.3.0 a replicar no GameHub

O commit `1956bb9` do EAF introduziu nos templates `Templates/Api` e `Templates/Angular`:

- Contratos públicos: `ContextualChatMessageContract`, `RateLimitContract`, `ModerationAuditContract`, `PublicErrorContract`.
- Interceptor Angular: `EafCorrelationIdInterceptor` com retry GET transitório e `normalizeEafError`.
- Template API: `AddDataProtection` com `PersistKeysToFileSystem` + `SetApplicationName`, `UseRateLimiter`, `UseCookiePolicy`, `UseMiddleware<SecurityHeadersMiddleware>`, `UseMiddleware<ContentSecurityPolicyMiddleware>`.
- CORS explícito com rejeição de `*` em produção.

No GameHub:

- `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware` e `RateLimitingMiddleware` existem em `Api/src/GameHub.Web.Host/Middleware/` mas **não são registrados** em `Startup.Configure`.
- `DataProtection` não está configurado.
- Não há contrato `PublicErrorContract`/`SdkError` normalizado.
- O `error.interceptor.ts` não normaliza erros nem faz retry transitório.

---

## Fase 0 — Sincronizar base de trabalho

**Files:**
- Inspect: `origin/devin/1785124000-next-platform-specs` (especs 46–51)
- Modify: `.specs/46-poki-moderacao-seguranca-operacao.md` → marcar como em execução
- Modify: `docs/agent-execution-log.md`

- [ ] **Step 1:** Criar branch `feature/gamehub-specs-46-51` a partir de `main`.
- [ ] **Step 2:** Trazer os arquivos `.specs/46*.md` a `51*.md` da branch `origin/devin/1785124000-next-platform-specs` para a branch de trabalho (são specs de planejamento, não código).
- [ ] **Step 3:** Atualizar `docs/agent-execution-log.md` com a data e a decisão de seguir specs 46–51.

---

## Fase 1 — Replicar hardening dos templates EAF 9.3.0 no host

**Files:**
- Modify: `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- Modify: `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs`
- Modify: `Api/src/GameHub.Web.Host/appsettings.json`
- Modify: `Api/src/GameHub.Web.Host/appsettings.Production.json`
- Modify: `Api/src/GameHub.Web.Host/appsettings.Development.json`
- Modify: `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs`
- Create: `Api/src/GameHub.Application/Dto/SdkError.cs`

### Task 1.1 — Registrar middlewares de segurança e rate limit

Em `Startup.Configure`, inserir na ordem correta de pipeline:

```csharp
app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();          // adiciona headers de segurança
app.UseMiddleware<ContentSecurityPolicyMiddleware>();      // adiciona CSP
app.UseMiddleware<RateLimitingMiddleware>();               // ou substituir por UseRateLimiter + policy GameHub
app.UseCookiePolicy();
app.UseEafHealthChecks();
...
app.UseCors(GameHubConsts.DefaultCorsPolicyName);
app.UseJwtTokenMiddleware();
app.UseAbpRequestLocalization();
app.UseRouting();
```

- `SecurityHeadersMiddleware` já implementa `X-Content-Type-Options`, `X-Frame-Options` dinâmico para `/play`, `Referrer-Policy`, `Permissions-Policy`, `Cross-Origin-Resource-Policy`, `X-Permitted-Cross-Domain-Policies`, HSTS em produção e remove `Server`/`X-Powered-By`.
- `ContentSecurityPolicyMiddleware` já constrói CSP por ambiente (production = `Content-Security-Policy`; development = `Content-Security-Policy-Report-Only`).
- `RateLimitingMiddleware` já conta por `tenant:usuário:jogo:partida` e devolve `429` com `Retry-After`; faltam headers `X-RateLimit-*` em todas as respostas e uso de `ClientRequestId` para idempotência (ver Fase 2).

### Task 1.2 — Adicionar Data Protection

Em `Startup.ConfigureServices`, após `AddControllersWithViews`:

```csharp
services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(_hostingEnvironment.ContentRootPath, "data-protection-keys")))
    .SetApplicationName("GameHub");
```

- Em produção, o caminho deve ser substituído por volume compartilhado ou Key Vault/Blob (documentar em `docs/eaf/gamehub-eaf-improvements.md`).

### Task 1.3 — Criar contrato público de erro

Criar `Api/src/GameHub.Application/Dto/SdkError.cs`:

```csharp
public class SdkError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public bool Retryable { get; set; }
    public string CorrelationId { get; set; }
}
```

Códigos estáveis: `not_authenticated`, `not_authorized`, `feature_disabled`, `rate_limited`, `invalid_context`, `temporarily_unavailable`, `validation_failed`.

### Task 1.4 — Ajustar CORS e appsettings

Em `CorsConfiguration.ConfigurePolicy`, garantir que `AllowAnyOrigin` só seja permitido em dev e que headers expostos incluam `Retry-After`.

Adicionar a `appsettings.json`:

```json
"RedisCache": {
  "IsEnabled": "true",
  "ConnectionString": "localhost:6379,abortConnect=false"
},
"OpenTelemetry": {
  "OtlpEndpoint": "https://otlp.nr-data.net:4318",
  "OtlpProtocol": "http/protobuf"
},
"DataProtection": {
  "KeysPath": "data-protection-keys"
}
```

Em `appsettings.Production.json`: `Cors:AllowAnyOrigin: false`, `Cors:HubOrigins` e `Cors:AdminOrigins` explícitos.

### Task 1.5 — Validar

```bash
dotnet build Api/GameHub.sln -c Release --no-restore
dotnet test Api/GameHub.sln -c Release --no-build
```

---

## Fase 2 — Spec 46: Moderação, segurança e operação

**Files:**
- Modify: `Api/src/GameHub.Application/Moderation/Dto/SubmitUserContentInput.cs`
- Modify: `Api/src/GameHub.Application/Moderation/Dto/UserReportInput.cs`
- Modify: `Api/src/GameHub.Application/Moderation/Dto/ModerateUserContentInput.cs`
- Modify: `Api/src/GameHub.Application/Moderation/UserContentAppService.cs`
- Modify: `Api/src/GameHub.Application/Moderation/UserReportAppService.cs`
- Modify: `Api/src/GameHub.Application/Moderation/ModerationAppService.cs`
- Modify: `Api/src/GameHub.Application/Social/GameSocialAppService.cs`
- Modify: `Api/src/GameHub.Application/Social/IGameSocialAppService.cs`
- Modify: `Api/src/GameHub.Application/Dto/AbpResponse.cs` ou criar `Api/src/GameHub.Web.Host/Middleware/PublicErrorMiddleware.cs`
- Create/Modify: `Api/src/GameHub.Application/Moderation/IModerationAuditService.cs` (fachada para EAF)
- Test: `test/GameHub.Tests/Moderation/UserContentAppService_Tests.cs`
- Test: `test/GameHub.Tests/Moderation/UserReportAppService_Tests.cs`

### Task 2.1 — Idempotência e limites nos inputs

Adicionar `ClientRequestId` aos DTOs:

```csharp
public class SubmitUserContentInput
{
    public Guid GameId { get; set; }
    public string Text { get; set; }
    public string ContentType { get; set; }
    public int? Rating { get; set; }
    public string ClientRequestId { get; set; }  // novo
}
```

Semelhante para `UserReportInput` e `ModerateUserContentInput`.

### Task 2.2 — Hardening de `UserContentAppService.SubmitAsync`

Modificar `UserContentAppService.SubmitAsync` para:

- Validar `GameId` existe e usuário não está bloqueado/mutado.
- Limitar tamanho de `Text` (constante `MaxTextLength = 2000`).
- Normalizar/trim `Text`.
- Verificar `ClientRequestId` no cache por 5 minutos para rejeitar duplicatas.
- Usar `GuidGenerator.Create()` em vez de `Guid.NewGuid()`.
- Retornar `SdkError` via `IUserFriendlyException` ou middleware (não `InvalidOperationException`).

```csharp
public async Task<UserContentDto> SubmitAsync(SubmitUserContentInput input)
{
    await EnsureNotBlockedAsync(input.GameId);
    await EnsureRateLimitAsync(input.GameId);
    await EnsureIdempotencyAsync(input.ClientRequestId);
    // ... lógica existente de profanity + inserção
}
```

### Task 2.3 — Hardening de `UserReportAppService.SubmitAsync`

- Rejeitar auto-report (reporter == reported).
- Validar `GameId` existe.
- `ClientRequestId` para idempotência.
- Rate limit por tenant + usuário + jogo.

### Task 2.4 — `ModerationAppService.CompleteReviewAsync`

- Validar `AbpAuthorize(GameHubPermissions.Pages_Moderation_Complete)`.
- Registrar auditoria via `IModerationAuditService.WriteAsync` (fachada que delega para EAF quando `IModerationAuditWriter` existir).
- Garantir que o review pertence ao tenant do usuário.

### Task 2.5 — `GameSocialAppService` para block/mute/report

Adicionar métodos (ou reforçar existentes):

```csharp
Task BlockPlayerAsync(BlockPlayerInput input);
Task UnblockPlayerAsync(BlockPlayerInput input);
Task ReportPlayerAsync(ReportPlayerInput input);
```

- `BlockPlayer` delega para EAF `FriendshipAppService.BlockUser` se disponível; senão, usa cache/fachada local.
- `ReportPlayer` valida auto-report, chama `IUserReportAppService.SubmitAsync`.

### Task 2.6 — Middleware de erros públicos

Criar `PublicErrorMiddleware` (opcional) ou alterar `AbpResponse` para envelopar exceções não tratadas:

```csharp
public class PublicErrorMiddleware
{
    public async Task Invoke(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            var sdkError = MapToSdkError(ex, correlationId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(sdkError);
        }
    }
}
```

Registrar em `Startup.Configure` antes de `UseDeveloperExceptionPage`/`UseExceptionHandler`.

### Task 2.7 — Testes

- `UserContentAppService_Tests`: duplicata com mesmo `ClientRequestId`, rate limit, texto acima do limite, jogo inexistente, usuário bloqueado.
- `UserReportAppService_Tests`: auto-report, jogo inexistente, idempotência, tenant isolation.

---

## Fase 3 — Spec 47: Analytics completo, exportação e operação

**Files:**
- Modify: `Api/src/GameHub.Application/Gameplay/Dto/GameMetricsFilter.cs`
- Modify: `Api/src/GameHub.Application/Gameplay/Dto/GameMetricsExportDto.cs`
- Modify: `Api/src/GameHub.Application/Gameplay/Dto/GameplayEventInput.cs`
- Modify: `Api/src/GameHub.Application/Gameplay/GameMetricsAppService.cs`
- Modify: `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`
- Create/Modify: `Api/src/GameHub.Application/Gameplay/GameMetricsAggregationJob.cs`
- Modify: `Api/src/GameHub.Web.Host/Startup/Startup.cs` (agendar job)
- Test: `test/GameHub.Tests/Gameplay/GameMetricsAppService_Tests.cs`

### Task 3.1 — Enriquecer `GameMetricsFilter`

```csharp
public class GameMetricsFilter : PagedAndSortedResultRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? BuildId { get; set; }
    public string DeviceType { get; set; }
    public string CountryCode { get; set; }
    public string TrafficSource { get; set; }
    public string UtmSource { get; set; }
    public string UtmMedium { get; set; }
    public string UtmCampaign { get; set; }
    public bool? IsPlaytest { get; set; }
}
```

### Task 3.2 — Deduplicação de eventos

Em `GameplayAppService.EventAsync`:

```csharp
public async Task EventAsync(GameplayEventInput input)
{
    if (!string.IsNullOrEmpty(input.ClientEventId))
    {
        var dedupKey = $"event:{AbpSession.TenantId}:{input.SessionId}:{input.ClientEventId}";
        if (await _dedupCache.GetOrDefaultAsync(dedupKey) != null)
            return;
        await _dedupCache.SetAsync(dedupKey, "1", absoluteExpireTime: DateTimeOffset.UtcNow.AddHours(24));
    }
    // ... persistir evento
}
```

### Task 3.3 — Agregação idempotente

`GameMetricsAggregationJob` deve receber `GameMetricsAggregationArgs` com `Date`.

```csharp
public class GameMetricsAggregationJob : BackgroundJob<GameMetricsAggregationArgs>, ITransientDependency
{
    public override void Execute(GameMetricsAggregationArgs args)
    {
        var window = args.Date.Date;
        // deletar snapshot existente do window e regravar
    }
}
```

### Task 3.4 — Exportação CSV

`GameMetricsAppService.ExportCsvAsync` deve gerar UTF-8 com escaping RFC 4180, sem PII, datas ISO 8601 UTC, e respeitar os mesmos filtros de `GetMetricsAsync`.

```csharp
public async Task<GameMetricsExportDto> ExportCsvAsync(Guid gameId, GameMetricsFilter input)
{
    var metrics = await GetMetricsAsync(gameId, input);
    var csv = BuildCsv(metrics);
    return new GameMetricsExportDto { Content = csv, FileName = $"metrics-{gameId:N}-{DateTime.UtcNow:yyyyMMdd}.csv" };
}
```

### Task 3.5 — Testes

- Reprocessar mesma janela não altera totais.
- `ClientEventId` duplicado não conta duas vezes.
- CSV com vírgula, aspas e Unicode.
- Playtests excluídos das métricas públicas.

---

## Fase 4 — Spec 48: Portal do desenvolvedor, publicação e acessibilidade

**Files:**
- Modify: `angular/src/app/developer/pages/developer-games/developer-games.component.ts` (+ html/css)
- Modify: `angular/src/app/developer/pages/developer-earnings/developer-earnings.component.ts`
- Modify: `angular/src/app/developer/components/builds/builds.component.ts`
- Modify: `angular/src/app/core/services/developer.service.ts`
- Modify: `angular/src/app/core/services/gameplay-bridge.service.ts`
- Test: `angular/src/app/core/services/developer.service.spec.ts` (se existir)

### Task 4.1 — Estados de UI

Cada componente deve expor:

```typescript
isLoading$ = signal(false);
isEmpty$ = signal(false);
error$ = signal<SdkError | null>(null);
retry$ = signal(() => this.load());
```

### Task 4.2 — Acessibilidade

- Landmarks (`<main>`, `<nav>`).
- Tabelas com `<caption>`, `<thead>`, `scope="col"`.
- Botões com `aria-label` e foco visível.
- Regiões `aria-live="polite"` para mensagens de sucesso/erro.

### Task 4.3 — Retry e cancelamento

- `developer.service.ts`: cancelar requisições concorrentes (switchMap/signals).
- Retry somente para erros transitórios (`retryable === true`).
- Tratamento explícito de `401` (login), `403` (sem permissão), `409` (conflito), `429` (rate limit + `Retry-After`).

---

## Fase 5 — Spec 49: SDK, privacidade, telemetria e resiliência

**Files:**
- Modify: `angular/src/app/core/services/gameplay-bridge.service.ts`
- Modify: `angular/src/app/core/services/player.service.ts`
- Modify: `angular/src/app/core/services/ad-break.service.ts`
- Modify: `angular/src/app/core/auth/auth.service.ts`
- Modify: `angular/src/app/core/auth/token.service.ts`
- Create: `angular/src/app/shared/models/gamehub-sdk.model.ts`
- Test: `angular/src/app/core/services/gameplay-bridge.service.spec.ts`

### Task 5.1 — Contratos do SDK

Criar `angular/src/app/shared/models/gamehub-sdk.model.ts`:

```typescript
export interface GameHubSdk {
  getCapabilities(): Promise<GameHubCapabilities>;
  getPrivacyPolicy(gameId: string): Promise<PrivacyPolicy>;
  setTelemetryConsent(input: TelemetryConsent): Promise<void>;
  measure(input: MeasureInput): Promise<void>;
  reportPlayer(input: ReportPlayerInput): Promise<void>;
  blockPlayer(userId: number): Promise<void>;
  unblockPlayer(userId: number): Promise<void>;
}
```

### Task 5.2 — Consentimento e telemetria

`GameplayBridgeService` deve:

- Aceitar somente `postMessage` de `environment.gameOrigin`.
- Buffer de eventos com `clientEventId`.
- Flush periódico ou no `beforeunload`.
- Não enviar chat, token, e-mail, IP ou claims.
- `setTelemetryConsent` grava no `PlayerPrivacyConsent` via `PrivacyAppService`.

### Task 5.3 — Refresh token e retry

`AuthService`:

- Interceptar `401` no `error.interceptor.ts` e tentar refresh uma única vez.
- Se refresh falhar, chamar `tokenService.clearToken()` e navegar para `/login`.
- `TokenService` continua com localStorage nesta fase; migração para cookie HttpOnly fica para spec 16 (hardening futuro).

---

## Fase 6 — Spec 50: Evoluções necessárias no EAF

**Files (repositório `afonsoft/EAF`):**
- Modify: `src/Eaf.Middleware.Core/Chat/ChatMessage.cs` (adicionar `ConversationId`, `GameId`, `MatchId`, `ContextType`, `ClientMessageId` opcionais)
- Create/Modify: `src/Eaf.Middleware.Application/Chat/Dto/GetChatHistoryInput.cs`
- Create/Modify: `src/Eaf.Middleware.Application/Chat/Dto/MarkChatReadInput.cs`
- Modify: `src/Eaf.Middleware.Application/Chat/ChatAppService.cs` (ou equivalente) com `GetHistoryAsync` e `MarkReadAsync`
- Create/Modify: `src/Eaf.Middleware.Application/Notifications/INotificationPublisher.cs` e DTOs com metadata
- Create/Modify: `src/Eaf.Middleware.Application/Authorization/BlockUserInput.cs`, `MuteUserInput.cs`
- Create/Modify: `src/Eaf.Middleware.Application/Realtime/IRateLimitManager.cs`
- Create/Modify: `src/Eaf.Middleware.Application/Moderation/IModerationAuditWriter.cs`
- Modify: `Templates/Api/src/Eaf.ProjectName.Web.Host/Startup/Startup.cs` (DataProtection, RateLimiter, middlewares)
- Modify: `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/eaf-contracts/*`
- Test: `test/Eaf.Middleware.Application.Tests/Chat/*_Tests.cs`

### Task 6.1 — Contratos retrocompatíveis

Adicionar campos opcionais a `ChatMessage` e `ChatMessageDto`;

```csharp
public class ChatMessage
{
    public Guid? ConversationId { get; set; }
    public Guid? GameId { get; set; }
    public Guid? MatchId { get; set; }
    public string ContextType { get; set; }
    public string ClientMessageId { get; set; }
}
```

### Task 6.2 — Histórico contextual

```csharp
Task<ListResultDto<ChatMessageDto>> GetHistoryAsync(GetChatHistoryInput input);
Task MarkReadAsync(MarkChatReadInput input);
```

### Task 6.3 — Rate limit compartilhado

```csharp
public interface IRateLimitManager
{
    Task<RateLimitDecision> CheckAsync(
        string policy,
        string subject,
        TimeSpan window,
        int limit,
        CancellationToken cancellationToken = default);
}
```

- Implementar com `ICacheManager` (fallback) e Redis `IConnectionMultiplexer` (atômico).

### Task 6.4 — Templates

- Replicar `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware`, `RateLimitingMiddleware` no template API se ainda não estiverem.
- Atualizar `appsettings*.json` com DataProtection, CORS explícito, Redis, OpenTelemetry.
- Atualizar `README.md` e `docs/integration/gamehub-consumer-contracts.md`.

### Task 6.5 — Integrar no GameHub

- Após publicação dos pacotes EAF 9.4.0 (ou patch), bump no `GameHub.Web.Host.csproj`.
- Substituir implementações próprias de rate limit/block/mute pelas do EAF quando disponíveis, preservando fachadas do GameHub.

---

## Fase 7 — Validação, documentação e PR

**Files:**
- Modify: `docs/agent-execution-log.md`
- Modify: `docs/eaf/gamehub-eaf-improvements.md` (atualizar status)
- Modify: `CHANGELOG.md`
- Modify: `.specs/46-poki-moderacao-seguranca-operacao.md` e demais specs (atualizar status)

### Task 7.1 — Verificações por fase

Cada fase deve rodar:

```bash
dotnet build Api/GameHub.sln -c Release --no-restore
dotnet test Api/GameHub.sln -c Release --no-build
cd angular && npm run build
cd angular-admin/GameHub.UI && npm run build
docker compose -f docker-compose.infra.yml -f docker-compose.yml config
```

### Task 7.2 — Pull Requests

- **PR 1:** Fase 1 (hardening EAF 9.3.0 no host) — `feature/gamehub-eaf-930-template-replication`.
- **PR 2:** Fase 2 (Spec 46) — `feature/gamehub-spec-46-moderation-security`.
- **PR 3:** Fase 3 (Spec 47) — `feature/gamehub-spec-47-analytics-export`.
- **PR 4:** Fase 4/5 (Specs 48/49) — `feature/gamehub-spec-48-49-portal-sdk`.
- **PR 5 (EAF):** Fase 6 (Spec 50) — branch no `afonsoft/EAF`.

---

## Critérios de aceite gerais

1. Middlewares de segurança e rate limit estão registrados e testados.
2. Todos os erros públicos usam `SdkError` (code, message, retryable, correlationId).
3. Nenhum endpoint aceita identidade/tenant vindos do iframe.
4. `ClientRequestId`/`ClientEventId` evitam duplicação em UGC, reports e eventos de gameplay.
5. Dashboard e CSV apresentam os mesmos totais para mesmos filtros.
6. Exportações e respostas não expõem PII (e-mail, IP, connection ID, claims).
7. Templates EAF 9.3.0/9.4.0 contêm as mesmas correções documentadas.
8. Build, testes e lint passam em ambos os repositórios.
9. Documentação (`agent-execution-log`, `CHANGELOG`, specs) atualizada.
