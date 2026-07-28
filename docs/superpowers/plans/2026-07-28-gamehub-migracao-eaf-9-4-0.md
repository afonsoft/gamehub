# Plano de Migração — GameHub EAF 9.3.1 → 9.4.0 (ABP 10.5.0)

> **For agentic workers:** usar `superpowers:executing-plans` ou `superpowers:subagent-driven-development` para executar este plano task-a-task.

**Goal:** Atualizar o backend .NET e o frontend Angular do `afonsoft/gamehub` para EAF 9.4.0 / ABP 10.5.0, adotando CORS refletivo, `PublicErrorContract`, login multi-tenant em duas etapas, SignalR moderno (`@microsoft/signalr`) e os componentes reutilizáveis do template 9.4.0.

**Architecture / data flow:**
1. **Backend pipeline:** `UseExceptionHandler` → `UseEafPublicErrorMiddleware` → `UseJwtTokenMiddleware` → `UseCors(GameHubConsts.DefaultCorsPolicyName)` → endpoints (`/signalr*`, `/api/*`).
2. **CORS:** `AddEafCors(..., isDevelopment, GameHubConsts.DefaultCorsPolicyName)` lê `App:CorsOrigins`, reflete origem, permite header/cookie `Abp-TenantId`.
3. **Erros:** `EafExceptionFilter` (ordem 1000) mapeia exceções ABP para `PublicErrorContract`; `EafPublicErrorMiddleware` captura exceções não tratadas no pipeline.
4. **Tenant:** header/cookie nomeado `Abp-TenantId`; `UserTenantMembership` (`gh_UserTenantMemberships`) já migrado em `20260727151245_AddUserTenantMembership`.
5. **Login:** `TokenAuthController.GetAvailableTenants` → `SelectTenant` (EAF); Angular decide 0/1/N tenants e navega para `account/select-tenant`.
6. **SignalR:** `SignalRHelper.buildConnection` com `accessTokenFactory`; `chat-signalr.service` usa `HubConnectionBuilder` diretamente.

**Tech Stack:** .NET 10, ASP.NET Core 10, ABP 10.5.0, EF Core 10.0.10, EAF 9.4.0, PostgreSQL, Angular 20, PrimeNG, NSwag.

---

## Baseline / estado atual

- `Api/common.props`: `<Version>9.3.1</Version>`
- `GameHub.Web.Host.csproj`: `Eaf.*` `9.3.1`
- `GameHub.Core.csproj`: `Abp`/`Abp.ZeroCore`/`Abp.AutoMapper`/`Abp.AspNetCore` `10.4.0`; `Microsoft.EntityFrameworkCore` `10.0.8`
- `GameHub.EntityFrameworkCore.csproj`: `Microsoft.EntityFrameworkCore.SqlServer`/`Tools`/`Design` `10.0.8`; `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.2`; `Abp.ZeroCore.EntityFrameworkCore` `10.4.0`
- Já existe migration `20260727151245_AddUserTenantMembership` que cria `gh_UserTenantMemberships` e adiciona campos contextuais em `EafChatMessages`.
- Backend possui CORS custom (`CorsConfiguration.cs`), `PublicErrorMiddleware` e `GameHubExceptionFilter`.
- Frontend `angular-admin` usa `Abp.TenantId` (ponto), login sem two-step e `eaf.signalr-client.js`.

---

## Task 1 — Atualizar versões dos pacotes NuGet

**Files:**
- `Modify:` `Api/common.props:4`
- `Modify:` `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj:52-55`
- `Modify:` `Api/src/GameHub.Core/GameHub.Core.csproj:17-22`
- `Modify:` `Api/src/GameHub.Application/GameHub.Application.csproj:17`
- `Modify:` `Api/src/GameHub.EntityFrameworkCore/GameHub.EntityFrameworkCore.csproj:11-22`
- `Modify:` `Api/src/GameHub.Migrator/GameHub.Migrator.csproj:18`
- `Modify:` `Api/test/GameHub.Web.Tests/GameHub.Web.Tests.csproj:25`
- `Modify:` `Api/test/GameHub.Tests/GameHub.Tests.csproj:30-32`

**Code:**

```xml
<!-- Api/common.props -->
<Version>9.4.0</Version>
```

| Pacote | De | Para |
|--------|----|----|
| `Eaf.Castle.Serilog` | 9.3.1 | 9.4.0 |
| `Eaf.Middleware.Web.Core` | 9.3.1 | 9.4.0 |
| `Eaf.Middleware.Core` | 9.3.1 | 9.4.0 |
| `Eaf.Middleware.Application` | 9.3.1 | 9.4.0 |
| `Eaf.OpenTelemetry` | 9.3.1 | 9.4.0 |
| `Eaf.KeyVault.AspNetCore` | 9.3.1 | 9.4.0 |
| `Abp` / `Abp.ZeroCore` / `Abp.AutoMapper` / `Abp.AspNetCore` | 10.4.0 | 10.5.0 |
| `Abp.ZeroCore.EntityFrameworkCore` | 10.4.0 | 10.5.0 |
| `Abp.Castle.Log4Net` | 10.4.0 | 10.5.0 |
| `Abp.TestBase` | 10.4.0 | 10.5.0 |
| `Abp.AspNetCore.TestBase` | 10.4.0 | 10.5.0 |
| `Microsoft.EntityFrameworkCore` | 10.0.8 | 10.0.10 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.8 | 10.0.10 |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.8 | 10.0.10 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.8/10.0.9 | 10.0.10 |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | 10.0.10 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.8 | 10.0.10 |

> `Npgsql.EntityFrameworkCore.PostgreSQL` permanece `10.0.2` se for a versão estável compatível com EF Core 10; verificar e elevar para `10.0.4` se publicada.

**Step / verify:**
```bash
cd /home/ubuntu/repos/gamehub/Api
dotnet restore GameHub.sln
```
Expected: restore sem erros de compatibilidade major.

---

## Task 2 — Backend: CORS seguro (`AddEafCors`)

**Files:**
- `Delete:` `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs` (não será mais referenciado)
- `Modify:` `Api/src/GameHub.Web.Host/Startup/Startup.cs:14-16, 71-78, 154, 260, 264`
- `Modify:` `Api/src/GameHub.Web.Host/appsettings*.json`
- `Modify:` `Api/test/GameHub.Tests/Middleware/CorsConfiguration_Tests.cs`

**Code:**

```csharp
// Startup.cs — adicionar usings
using Eaf.Middleware.Web.Filters;
using Eaf.Middleware.Web.Startup;

// Startup.cs — ConfigureServices
services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<SerilogMvcLoggingAttribute>();
    options.Filters.Add<GameHubExceptionFilter>();
    options.Filters.Add(typeof(EafExceptionFilter), 1000);   // << ADD
    options.Filters.Add(new ResponseCacheAttribute() { NoStore = true, Location = ResponseCacheLocation.None });
}).AddNewtonsoftJson();

// Startup.cs — ConfigureServices (substituir AddGameHubCors)
services.AddEafCors(
    _appConfiguration,
    _hostingEnvironment.IsDevelopment(),
    GameHubConsts.DefaultCorsPolicyName);

// Startup.cs — Configure (após UseExceptionHandler, antes de UseJwtTokenMiddleware)
if (env.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseEafPublicErrorMiddleware();          // << ADD (substitui PublicErrorMiddleware)
app.UseJwtTokenMiddleware();
app.UseAbpRequestLocalization();
app.UseRouting();
app.UseCors(GameHubConsts.DefaultCorsPolicyName);
```

```json
// appsettings.Development.json / Local
{
  "App": {
    "CorsOrigins": "*"
  }
}

// appsettings.Staging.json
{
  "App": {
    "CorsOrigins": "https://hom-gamehub.afonsoft.dev;https://hom-gamehub-admin.afonsoft.dev"
  }
}

// appsettings.Production.json
{
  "App": {
    "CorsOrigins": "https://gamehub.afonsoft.dev;https://gamehub-admin.afonsoft.dev"
  }
}
```

> Pode-se manter `Cors:HubOrigins`/`Cors:AdminOrigins` como fallback legado, mas não serão lidos por `AddEafCors`.

**Test update:** `CorsConfiguration_Tests.cs` deve testar `AddEafCors` e a política `GameHubConsts.DefaultCorsPolicyName`; remover asserts de `HubPolicy` e `AdminPolicy`.

**Step / verify:**
```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~Cors"
```

---

## Task 3 — Backend: erros públicos (`PublicErrorContract`)

**Files:**
- `Delete:` `Api/src/GameHub.Web.Host/Middleware/PublicErrorMiddleware.cs`
- `Modify:` `Api/src/GameHub.Web.Host/Startup/Startup.cs:30, 260`
- `Modify:` `Api/test/GameHub.Tests/Middleware/PublicErrorMiddleware_Tests.cs`

**Code:**

```csharp
// Startup.cs — Configure
app.UseEafPublicErrorMiddleware();   // substitui app.UseMiddleware<PublicErrorMiddleware>();
```

```csharp
// PublicErrorMiddleware_Tests.cs — reescrever contra EafPublicErrorMiddleware + PublicErrorContract
using Eaf.Middleware.Contracts;
using Eaf.Middleware.Web.Middleware;

[Fact]
public async Task Dado_UserFriendlyException_Quando_Invocar_Entao_DeveRetornar400ComRetryableFalse()
{
    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();
    var middleware = new EafPublicErrorMiddleware(
        _ => throw new Abp.UI.UserFriendlyException("Invalid"),
        NullLogger<EafPublicErrorMiddleware>.Instance);

    await middleware.Invoke(context);

    context.Response.StatusCode.ShouldBe(400);
    context.Response.ContentType.ShouldContain("application/json");
    var body = await ReadBodyAsync(context.Response.Body);
    body.ShouldContain("Invalid");
}
```

> `GameHubExceptionFilter` continua ativo para mapear `GameHubException` → `SdkError` (códigos específicos do domínio GameHub). `EafExceptionFilter` (ordem 1000) mapeia `UserFriendlyException`, `AbpValidationException`, `AbpAuthorizationException`, etc. para `PublicErrorContract`.

**Step / verify:**
```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~PublicError"
```

---

## Task 4 — Backend: header/cookie de tenant `Abp-TenantId` e SignalR token

**Files:**
- `Modify:` `Api/src/GameHub.Web.Host/Startup/Startup.cs` (CORS já configura `Abp-TenantId` via `AddEafCors`)
- `No action on `MiddlewareControllerBase` / `AuthConfigurer` — são fornecidos pelo pacote `Eaf.Middleware.Web.Core` 9.4.0.

**Notes:**
- `EafCorsConfiguration` já expõe `Abp-TenantId` no `WithHeaders`.
- `AuthConfigurer.SetToken` no EAF 9.4.0 lê `access_token` da query string para requisições `/signalr*`.

**Step / verify:**
```bash
# após build, inspecionar CORS preflight
curl -i -X OPTIONS \
  -H "Origin: http://localhost:4200" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Abp-TenantId,Authorization" \
  http://localhost:8001/api/TokenAuth/GetAvailableTenants
```

---

## Task 5 — Backend: `UserTenantMembership` e desambiguação de namespace

**Files:**
- `Modify:` `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs:7, 22, 129`
- `Modify:` `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs:942`
- `Modify:` `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/SeedHelper.cs:58, 104`

**Code:**

```csharp
// GameHubDbContext.cs — manter using Eaf.Middleware.MultiTenancy; (Tenant/Role/User),
// mas remover using GameHub.MultiTenancy; para evitar conflito com UserTenantMembership do EAF 9.4.0.
using Eaf.Middleware.MultiTenancy;   // Tenant, Role, User
// using GameHub.MultiTenancy;       // << REMOVE

public virtual DbSet<GameHub.MultiTenancy.UserTenantMembership> UserTenantMemberships { get; set; }
```

```csharp
// GameHubModelCreatingExtensions.cs
modelBuilder.Entity<GameHub.MultiTenancy.UserTenantMembership>(b =>
{
    b.ToTable(GameHubConsts.DbTablePrefix + "UserTenantMemberships", GameHubConsts.DbSchema);
    b.Property(x => x.UserId).IsRequired();
    b.Property(x => x.TenantId).IsRequired();
    b.Property(x => x.TenantUserId).IsRequired();
    b.Property(x => x.IsDefault).IsRequired();
    b.HasIndex(x => new { x.UserId, x.TenantId }).IsUnique();
    b.HasIndex(x => new { x.UserId, x.IsDefault });
    b.HasIndex(x => x.TenantUserId);
});
```

```csharp
// SeedHelper.cs
var existingMembership = context.UserTenantMemberships.IgnoreQueryFilters()...;
context.UserTenantMemberships.Add(new GameHub.MultiTenancy.UserTenantMembership { ... });
```

> A migration `20260727151245_AddUserTenantMembership` já criou a tabela e os índices. Após o bump de pacotes, rodar `dotnet ef migrations add Eaf940Alignment` para capturar eventuais diferenças no snapshot; se vazia, removê-la.

**Step / verify:**
```bash
dotnet ef migrations add Eaf940Alignment \
  --project Api/src/GameHub.EntityFrameworkCore/GameHub.EntityFrameworkCore.csproj \
  --startup-project Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj
```

---

## Task 6 — Frontend: `AppConsts` e configurações de multi-tenancy

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/shared/AppConsts.ts`

**Code:**

```typescript
export class AppConsts {
  // ... campos existentes ...

  static readonly multiTenancy = {
    twoStepLogin: false,
  };

  static autoSelectSingleTenant = true;
}
```

**Step / verify:**
```bash
cd angular-admin/GameHub.UI
grep -n "twoStepLogin\|autoSelectSingleTenant" src/shared/AppConsts.ts
```

---

## Task 7 — Frontend: cookie/header de tenant `Abp-TenantId`

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/assets/lib/eaf-web-resources/eaf.js:34`
- `Modify:` `angular-admin/GameHub.UI/src/assets/lib/eaf-ng2-module/src/eafHttpInterceptor.ts:388-397`
- `Modify:` `angular-admin/GameHub.UI/src/app/shared/common/auth/app-auth.service.ts:10-18`
- `Modify:` `angular-admin/GameHub.UI/src/AppPreBootstrap.ts:53-61, 99-107, 138-148`

**Code:**

```javascript
// eaf.js
eaf.multiTenancy.tenantIdCookieName = 'Abp-TenantId';
```

```typescript
// eafHttpInterceptor.ts
protected addTenantIdHeader(headers: HttpHeaders): HttpHeaders {
  const tenantIdCookieName = (window as any).eaf?.multiTenancy?.tenantIdCookieName || 'Abp-TenantId';
  const cookieTenantIdValue = this._storageService.getCookieValue(tenantIdCookieName);
  if (cookieTenantIdValue && headers && !headers.has(tenantIdCookieName)) {
    headers = headers.set(tenantIdCookieName, cookieTenantIdValue);
  }
  return headers;
}
```

```typescript
// app-auth.service.ts
logout(reload?: boolean, returnUrl?: string): void {
  const currentTenantId = eaf.multiTenancy.getTenantIdCookie();
  const customHeaders: any = {
    Authorization: 'Bearer ' + eaf.auth.getToken(),
  };
  if (currentTenantId) {
    customHeaders[eaf.multiTenancy.tenantIdCookieName] = currentTenantId.toString();
  }
  // ... restante do logout
}
```

```typescript
// AppPreBootstrap.ts — getApplicationConfig
const tenantId = eaf.multiTenancy.getTenantIdCookie();
const customHeaders = tenantId
  ? [{ name: eaf.multiTenancy.tenantIdCookieName, value: tenantId.toString() }]
  : [];
```

> Em `getUserConfiguration` e `impersonatedAuthenticate`, adicionar `if (currentTenantId)` antes de incluir o header.

**Step / verify:**
```bash
grep -R "Abp\\.TenantId" angular-admin/GameHub.UI/src || true
# deve retornar vazio após a migração
```

---

## Task 8 — Frontend: `TokenService` com parsing de JWT

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/assets/lib/eaf-ng2-module/src/auth/token.service.ts`

**Code:** substituir o arquivo pelo conteúdo do template EAF 9.4.0 (`Templates/Angular/Eaf.ProjectName.UI/src/assets/lib/eaf-ng2-module/src/auth/token.service.ts`), que expõe:

```typescript
export interface TokenPayload { ... }

export class TokenService {
  getToken(): string;
  getTokenCookieName(): string;
  clearToken(): void;
  setToken(authToken: string, expireDate?: Date): void;
  getPayload(token?: string): TokenPayload | null;
  isValid(): boolean;
  getUserId(): number | null;
  getTenantId(): number | null;
  getUserName(): string | null;
  getRoles(): string[];
  isInRole(role: string): boolean;
}
```

**Step / verify:**
```bash
cd angular-admin/GameHub.UI
npx tsc --noEmit src/assets/lib/eaf-ng2-module/src/auth/token.service.ts
```

---

## Task 9 — Frontend: login em duas etapas

**Files:**
- `Create:` `angular-admin/GameHub.UI/src/account/login/select-tenant/select-tenant.component.ts`
- `Create:` `angular-admin/GameHub.UI/src/account/login/select-tenant/select-tenant.component.html`
- `Modify:` `angular-admin/GameHub.UI/src/account/login/login.service.ts`
- `Modify:` `angular-admin/GameHub.UI/src/account/login/login.component.ts`
- `Modify:` `angular-admin/GameHub.UI/src/account/account.module.ts`
- `Modify:` `angular-admin/GameHub.UI/src/account/account-routing.module.ts`

**Code (interfaces e métodos a adicionar em login.service.ts):**

```typescript
export interface AvailableTenantResult {
  tenantId: number;
  tenantName: string;
  tenancyName: string;
  isDefault: boolean;
}

export interface AvailableTenantsModel {
  userNameOrEmailAddress: string;
  password: string;
}

export interface SelectTenantModel extends AvailableTenantsModel {
  tenantId: number;
}

export class LoginService {
  availableTenantsResult: AvailableTenantResult[] = [];

  // ... métodos existentes ...

  availableTenants(model: AvailableTenantsModel): Observable<AvailableTenantResult[]> {
    const url = `${AppConsts.remoteServiceBaseUrl}/api/TokenAuth/GetAvailableTenants`;
    return this._httpClient.post<AvailableTenantResult[]>(url, model);
  }

  selectTenant(model: SelectTenantModel): Observable<AuthenticateResultModel> {
    const url = `${AppConsts.remoteServiceBaseUrl}/api/TokenAuth/SelectTenant`;
    return this._httpClient.post<AuthenticateResultModel>(url, model);
  }

  loginTenant(result: AuthenticateResultModel, tenantId: number, redirectUrl?: string): void {
    eaf.multiTenancy.setTenantIdCookie(tenantId);
    this.login(result.accessToken, result.encryptedAccessToken, result.expireInSeconds, this.rememberMe, redirectUrl);
  }

  navigateToSelectTenant(): void {
    this._router.navigate(['account/select-tenant']);
  }

  private clear(): void {
    this.authenticateModel = new AuthenticateModel();
    this.authenticateModel.rememberClient = false;
    this.authenticateResult = null;
    this.rememberMe = false;
    this.availableTenantsResult = [];
  }
}
```

> `LoginService` já possui `HttpClient`? Atualmente não. Adicionar `private readonly _httpClient: HttpClient` no construtor e importar `HttpClientModule`/`HttpClient`.

**Code (login.component.ts):**

```typescript
export class LoginComponent extends AppComponentBase implements OnInit {
  get isTwoStepLogin(): boolean {
    return AppConsts.multiTenancy?.twoStepLogin ?? false;
  }

  login(): void {
    if (this.isTwoStepLogin) {
      this.twoStepLogin();
      return;
    }
    this.normalLogin();
  }

  private normalLogin(): void { /* ... */ }

  private twoStepLogin(): void {
    const recaptchaCallback = (token: string) => {
      this.submitting = true;
      this.dataTableHelper.showLoadingIndicator();
      const model = {
        userNameOrEmailAddress: this.loginService.authenticateModel.userNameOrEmailAddress,
        password: this.loginService.authenticateModel.password,
      };
      this.loginService.availableTenants(model).subscribe({
        next: tenants => {
          this.loginService.availableTenantsResult = tenants;
          if (tenants.length === 0) {
            this.loginService.authenticate(() => { this.submitting = false; this.dataTableHelper.hideLoadingIndicator(); }, undefined, token);
          } else if (tenants.length === 1 && AppConsts.autoSelectSingleTenant) {
            this.loginService.selectTenant({ ...model, tenantId: tenants[0].tenantId })
              .subscribe({
                next: result => this.loginService.loginTenant(result, tenants[0].tenantId),
                error: () => { this.submitting = false; this.dataTableHelper.hideLoadingIndicator(); }
              });
          } else {
            this.submitting = false;
            this.dataTableHelper.hideLoadingIndicator();
            this.loginService.navigateToSelectTenant();
          }
        },
        error: () => { this.submitting = false; this.dataTableHelper.hideLoadingIndicator(); }
      });
    };
    // captcha ou direto
  }
}
```

**Route/module:**

```typescript
// account-routing.module.ts
import { SelectTenantComponent } from './login/select-tenant/select-tenant.component';
// ...
{ path: 'select-tenant', component: SelectTenantComponent, canActivate: [AccountRouteGuard] },
```

```typescript
// account.module.ts
import { SelectTenantComponent } from './login/select-tenant/select-tenant.component';
 declarations: [..., SelectTenantComponent]
```

**Step / verify:**
```bash
cd angular-admin/GameHub.UI
npx tsc --noEmit src/account/login/login.component.ts src/account/login/select-tenant/select-tenant.component.ts
```

---

## Task 10 — Frontend: SignalR moderno

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/shared/helpers/SignalRHelper.ts`
- `Modify:` `angular-admin/GameHub.UI/src/app/shared/layout/chat/chat-signalr.service.ts`
- `Modify:` `angular-admin/GameHub.UI/src/app/app.component.ts`
- `Delete / remove usage:` `angular-admin/GameHub.UI/src/assets/lib/eaf-web-resources/Eaf/Framework/scripts/libs/eaf.signalr-client.js` (deixar arquivo legado, mas não carregar)

**Code:** substituir `SignalRHelper.ts` e `chat-signalr.service.ts` pelos templates EAF 9.4.0. Destaques:

```typescript
// SignalRHelper.ts
static init(tokenService: TokenService): void { this._tokenService = tokenService; }

static buildConnection(hubUrl: string = '/signalr'): signalR.HubConnection {
  const base = (AppConsts.remoteServiceBaseUrl || '').replace(/\/$/, '');
  const fullUrl = base + hubUrl;
  return new signalR.HubConnectionBuilder()
    .withUrl(fullUrl, {
      accessTokenFactory: () => this._tokenService?.getToken() ?? '',
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}
```

```typescript
// chat-signalr.service.ts
init(): void {
  this._zone.runOutsideAngular(async () => {
    this.chatHub = SignalRHelper.buildConnection('/signalr-chat');
    this.chatHub.onreconnecting(...);
    this.chatHub.onreconnected(...);
    this.chatHub.onclose(...);
    this.registerChatEvents(this.chatHub);
    try {
      await this.chatHub.start();
      this.isChatConnected = true;
      eaf.event.trigger('app.chat.connected');
    } catch (error) {
      eaf.log.error('Chat connection failed: ' + error);
    }
  });
}
```

```typescript
// app.component.ts
import { TokenService } from '@eaf/auth/token.service';
// ...
if (this.appSession.application) {
  SignalRHelper.init(this._tokenService);
  this._chatSignalrService.init();
}
// ngAfterViewInit: remover eaf.signalr.autoConnect = false;
```

**Step / verify:**
```bash
cd angular-admin/GameHub.UI
npx tsc --noEmit src/shared/helpers/SignalRHelper.ts src/app/shared/layout/chat/chat-signalr.service.ts
```

---

## Task 11 — Frontend: tratamento de erros públicos

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/assets/lib/eaf-ng2-module/src/eafHttpInterceptor.ts:91-108`

**Code:**

```typescript
handleNonEafErrorResponse(response: any) {
  const body = response.error ?? response.body;
  if (body?.message) {
    const error = <IErrorInfo>{
      message: body.message,
      details: body.message,
      code: body.code,
      validationErrors: [],
    };
    this.logError(error);
    this.showError(error);
    return;
  }

  switch (response.status) {
    case 401: this.handleUnAuthorizedRequest(this.showError(this.defaultError401), '/'); break;
    case 403: this.showError(this.defaultError403); break;
    case 404: this.showError(this.defaultError404); break;
    default: this.showError(this.defaultError); break;
  }
}
```

**Step / verify:**
```bash
npx tsc --noEmit src/assets/lib/eaf-ng2-module/src/eafHttpInterceptor.ts
```

---

## Task 12 — Frontend: componentes reutilizáveis e responsividade

**Files:**
- `Create:` `angular-admin/GameHub.UI/src/app/shared/components/empty-state/empty-state.component.ts`
- `Create:` `angular-admin/GameHub.UI/src/app/shared/components/status-badge/status-badge.component.ts`
- `Modify:` `angular-admin/GameHub.UI/src/app/shared/common/app-common.module.ts:22-23, 45-47, 56-57`
- `Modify:` `angular-admin/GameHub.UI/src/app/admin/tenants/tenants.component.html`
- `Modify:` `angular-admin/GameHub.UI/src/app/admin/users/users.component.html`
- `Modify:` `angular-admin/GameHub.UI/src/assets/common/styles/styles.css`
- `Modify:` `angular-admin/GameHub.UI/src/app/shared/layout/topbar.component.ts:108-114`

**Code:**

```typescript
// empty-state.component.ts
@Component({
  selector: 'app-empty-state',
  standalone: false,
  template: `<div class="text-center p-4 text-muted"><i class="fa fa-inbox fa-2x mb-3"></i><p class="mb-0">{{ message }}</p></div>`,
})
export class EmptyStateComponent { @Input() message = ''; }

// status-badge.component.ts
@Component({
  selector: 'app-status-badge',
  standalone: false,
  template: `<span class="m-badge" [ngClass]="value ? trueClass : falseClass">{{ value ? trueLabel : falseLabel }}</span>`,
})
export class StatusBadgeComponent {
  @Input() value: boolean;
  @Input() trueLabel = 'Yes';
  @Input() falseLabel = 'No';
  @Input() trueClass = 'm-badge--success m-badge--wide';
  @Input() falseClass = 'm-badge--metal m-badge--wide';
}
```

```typescript
// app-common.module.ts
import { EmptyStateComponent } from '../components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../components/status-badge/status-badge.component';

@NgModule({
  declarations: [..., EmptyStateComponent, StatusBadgeComponent],
  exports: [..., EmptyStateComponent, StatusBadgeComponent],
})
```

```html
<!-- tenants.component.html / users.component.html -->
<td>
  <app-status-badge
    [value]="record.isActive"
    [trueLabel]="'Yes' | localize"
    [falseLabel]="'No' | localize">
  </app-status-badge>
</td>

<app-empty-state *ngIf="dataTableHelper.totalRecordsCount == 0 && !dataTableHelper.isLoading" [message]="'NoData' | localize"></app-empty-state>
```

```typescript
// topbar.component.ts
setCurrentLoginInformations(): void {
  const user = this.appSession.user;
  this.shownLoginName = user ? this.appSession.getShownLoginName() : '';
  this.shownFullName = user ? `${user.name} ${user.surname || ''}`.trim() : '';
  this.tenancyName = this.appSession.tenancyName || '';
  this.userName = user ? user.userName : '';
  this.isSystemUser = user ? user.authenticationSource == undefined : true;
}
```

```css
/* styles.css — adicionar ao final */
.m-page,
.m-grid.m-grid--root,
.m-login,
.m-login__wrapper {
    min-height: 100dvh;
}

.m-login__aside {
    display: flex;
    align-items: center;
}

.m-login__wrapper {
    width: 100%;
    max-width: 420px;
}

.m-login__form .form-control,
.m-login__form .m-input-icon,
.m-login__form .btn,
.m-login__form button {
    min-height: 44px;
    font-size: 16px;
}

@media (max-width: 576px) {
    .m-login__wrapper { max-width: 100%; padding: 0; }
    .m-login__aside { padding: 0.75rem; }
}
```

**Step / verify:**
```bash
npx ng build --configuration=production
```

---

## Task 13 — Regenerar service proxies (NSwag)

**Files:**
- `Modify:` `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxies.ts` (gerado, não editar manualmente)

**Step:**
```bash
cd angular-admin/GameHub.UI
npm run service-update
```

> Após a atualização, verificar se `TokenAuthServiceProxy` possui `getAvailableTenants` e `selectTenant`. Se `login.service.ts` passar a usar estes métodos em vez de `HttpClient`, os DTOs `AvailableTenantsModel`/`SelectTenantModel`/`AvailableTenantResult` devem existir no proxy.

---

## Task 14 — Validação final

**Backend:**
```bash
cd /home/ubuntu/repos/gamehub/Api
dotnet build GameHub.sln -c Release
dotnet test GameHub.sln -c Release
```

**Frontend:**
```bash
cd /home/ubuntu/repos/gamehub/angular-admin/GameHub.UI
npm ci --legacy-peer-deps
npm run build
```

**Database (quando houver ambiente PostgreSQL):**
```bash
dotnet ef database update \
  --project Api/src/GameHub.EntityFrameworkCore/GameHub.EntityFrameworkCore.csproj \
  --startup-project Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj
```

**Smoke checks:**
- Swagger em `/swagger` responde.
- CORS preflight retorna `200` com origem refletida e header `Abp-TenantId` permitido.
- Login host: `POST /api/TokenAuth/Authenticate` com tenant vazio funciona.
- Login two-step: `POST /api/TokenAuth/GetAvailableTenants` retorna tenants; `POST /api/TokenAuth/SelectTenant` retorna token.
- Erro de validação retorna `{ message, code, retryable, correlationId }` com status 400.
- SignalR `/signalr-chat` conecta com token via query string.
- Admin UI: tabelas `Tenants` e `Users` exibem `app-status-badge` e `app-empty-state`.

---

## Rollback

1. Restaurar branch anterior.
2. Restaurar backup do banco.
3. Se migrations foram aplicadas: `dotnet ef database update <MigrationAnterior>`.

---

## Notas / riscos

- `GameHub.MultiTenancy.UserTenantMembership` deve ser totalmente qualificado após o bump, pois EAF 9.4.0 introduz `Eaf.Middleware.MultiTenancy.UserTenantMembership`.
- `CorsConfiguration.cs`, `PublicErrorMiddleware.cs` e seus testes devem ser removidos ou adaptados; `GameHubExceptionFilter` permanece para `GameHubException`.
- A migration `20260727151245_AddUserTenantMembership` já cobre os campos de chat contextual e a tabela de membership; pode ser necessária uma migration de alinhamento vazia, que deve ser removida se não produzir diferenças.
- O frontend público `angular/` não usa EAF diretamente; este plano foca no admin `angular-admin/GameHub.UI` (baseado no template EAF). Se o frontend público precisar de ajustes de CORS/SignalR, tratá-los como tarefa separada.