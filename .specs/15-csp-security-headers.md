# 15 — CSP & Security Headers

> **Status:** Draft  
> **Stack:** .NET 10 LTS · ASP.NET Core · Angular 20+ · PostgreSQL 16+ · Redis 7+  
> **DNS:** gamehub.afonsoft.dev · gamehub-admin.afonsoft.dev · gamehub-api.afonsoft.dev  
> **CDN:** gamehub.afonsoft.dev (static assets)

---

## 1. Content Security Policy (CSP)

### 1.1 Complete Directive Set

```
default-src 'self';
script-src 'self';
style-src 'self' 'unsafe-inline';
img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev;
font-src 'self';
connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev;
frame-src https://games.afonsoft.dev;
frame-ancestors 'self' https://gamehub.afonsoft.dev;
object-src 'none';
base-uri 'self';
form-action 'self';
upgrade-insecure-requests;
```

### 1.2 Directive Explanations

| Directive | Value | Purpose |
|---|---|---|
| `default-src` | `'self'` | Fallback for any directive not explicitly set. Only allows same-origin resources. |
| `script-src` | `'self'` | Only allow scripts from same origin. No inline scripts, no eval, no CDN. |
| `style-src` | `'self' 'unsafe-inline'` | Allow same-origin stylesheets plus inline styles (required by Angular component styles). |
| `img-src` | `'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev` | Allow same-origin images, data URIs (for base64 inline images), and images from the hub and API domains. |
| `font-src` | `'self'` | Only same-origin font files. |
| `connect-src` | `'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev` | Allow same-origin AJAX/fetch, plus API HTTPS and WebSocket connections. |
| `frame-src` | `https://games.afonsoft.dev` | Allow game iframes only from the game hosting domain. |
| `frame-ancestors` | `'self' https://gamehub.afonsoft.dev` | Only the hub itself can embed this page in an iframe. Prevents clickjacking. |
| `object-src` | `'none'` | Block Flash, Java plugins, and other object embeds. |
| `base-uri` | `'self'` | Prevent `<base>` tag injection. |
| `form-action` | `'self'` | Form submissions only to same origin. |
| `upgrade-insecure-requests` | — | Automatically upgrade HTTP requests to HTTPS. |

### 1.3 Development vs Production CSP

| Environment | CSP Header | Behavior |
|---|---|---|
| **Development** | `Content-Security-Policy-Report-Only` | Reports violations without blocking. Allows debugging CSP issues. |
| **Production** | `Content-Security-Policy` | Enforces all directives. Violations block resource loading. |

Development override adds:

```
script-src 'self' 'unsafe-eval' 'unsafe-inline';
style-src 'self' 'unsafe-inline' 'unsafe-eval';
connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev http://localhost:* ws://localhost:*;
```

### 1.4 ASP.NET Core Middleware Implementation

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace GameHub.Web.Security;

public static class SecurityHeadersMiddleware
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // ── CSP ──
            if (env.IsDevelopment())
            {
                headers["Content-Security-Policy-Report-Only"] = BuildDevelopmentCsp();
            }
            else
            {
                headers["Content-Security-Policy"] = BuildProductionCsp();
            }

            // ── Security Headers ──
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "0";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            headers["X-Permitted-Cross-Domain-Policies"] = "none";
            headers["Cross-Origin-Resource-Policy"] = "same-site";

            // Remove server identification
            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            await next();
        });

        return app;
    }

    private static string BuildProductionCsp()
    {
        return string.Join("; ", new[]
        {
            "default-src 'self'",
            "script-src 'self'",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
            "font-src 'self'",
            "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev",
            "frame-src https://games.afonsoft.dev",
            "frame-ancestors 'self' https://gamehub.afonsoft.dev",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "upgrade-insecure-requests"
        });
    }

    private static string BuildDevelopmentCsp()
    {
        return string.Join("; ", new[]
        {
            "default-src 'self'",
            "script-src 'self' 'unsafe-eval' 'unsafe-inline'",
            "style-src 'self' 'unsafe-inline' 'unsafe-eval'",
            "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
            "font-src 'self'",
            "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev http://localhost:* ws://localhost:*",
            "frame-src https://games.afonsoft.dev",
            "frame-ancestors 'self' https://gamehub.afonsoft.dev",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'"
        });
    }
}
```

Register in the pipeline:

```csharp
// Program.cs
app.UseSecurityHeaders(env);
```

---

## 2. Security Headers

### 2.1 Complete Header List

| Header | Value | Purpose |
|---|---|---|
| `X-Content-Type-Options` | `nosniff` | Prevents MIME-type sniffing. Browser must respect declared Content-Type. |
| `X-Frame-Options` | `DENY` | Prevents the page from being embedded in any iframe. Overridden to `SAMEORIGIN` for game shell routes. |
| `X-XSS-Protection` | `0` | Disables legacy XSS filter. Modern browsers rely on CSP instead. Legacy filter can introduce vulnerabilities. |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Sends full URL for same-origin requests, only origin for cross-origin. Never sends referrer to less-secure protocols. |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=(), payment=()` | Disables browser features by default. Games can request specific permissions via iframe attributes. |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains; preload` | Forces HTTPS for 1 year, includes all subdomains, eligible for HSTS preload list. |
| `X-Permitted-Cross-Domain-Policies` | `none` | Prevents Flash/PDF from loading cross-origin resources. |
| `Cross-Origin-Resource-Policy` | `same-site` | Prevents cross-origin reads of resources (images, scripts, etc.). |

### 2.2 Game Shell Route Override

For the `/play/:slug` route, `X-Frame-Options` must be relaxed to allow game embedding:

```csharp
// In the game shell controller or middleware
if (context.Request.Path.StartsWithSegments("/play"))
{
    headers["X-Frame-Options"] = "SAMEORIGIN";
}
```

---

## 3. CORS Configuration

### 3.1 Policy Definition

```csharp
using Microsoft.AspNetCore.Cors;

namespace GameHub.Web.Configuration;

public static class CorsConfiguration
{
    public const string HubPolicy = "GameHubCors";
    public const string AdminPolicy = "GameHubAdminCors";

    public static IServiceCollection AddGameHubCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            // Hub frontend
            options.AddPolicy(HubPolicy, policy =>
            {
                policy.WithOrigins(
                        "https://gamehub.afonsoft.dev",
                        "http://localhost:4200")  // Angular dev server
                    .AllowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .AllowedHeaders(
                        "Authorization",
                        "Content-Type",
                        "Accept",
                        "X-Requested-With",
                        "X-Correlation-ID")
                    .ExposedHeaders(
                        "X-RateLimit-Limit",
                        "X-RateLimit-Remaining",
                        "X-RateLimit-Reset")
                    .AllowCredentials()
                    .SetPreflightMaxAge(600);
            });

            // Admin frontend
            options.AddPolicy(AdminPolicy, policy =>
            {
                policy.WithOrigins(
                        "https://gamehub-admin.afonsoft.dev",
                        "http://localhost:4201")  // Angular admin dev server
                    .AllowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .AllowedHeaders(
                        "Authorization",
                        "Content-Type",
                        "Accept",
                        "X-Requested-With",
                        "X-Correlation-ID")
                    .ExposedHeaders(
                        "X-RateLimit-Limit",
                        "X-RateLimit-Remaining",
                        "X-RateLimit-Reset")
                    .AllowCredentials()
                    .SetPreflightMaxAge(600);
            });
        });

        return services;
    }
}
```

### 3.2 Apply CORS Policies

```csharp
// Program.cs
app.UseCors(CorsConfiguration.HubPolicy);
// OR per-controller:
// [EnableCors(CorsConfiguration.AdminPolicy)]
```

### 3.3 Preflight Handling

- `OPTIONS` requests are handled automatically by the CORS middleware.
- `Access-Control-Max-Age: 600` caches preflight results for 10 minutes.
- Only explicitly listed methods and headers are allowed.

### 3.4 Credentials

- `AllowCredentials()` is enabled to support HttpOnly authentication cookies.
- `Access-Control-Allow-Origin` is always a specific origin (never `*`).

---

## 4. Iframe Security

### 4.1 Game Iframe Attributes

```html
<iframe
  src="https://games.afonsoft.dev/game-slug/index.html"
  sandbox="allow-scripts allow-pointer-lock allow-same-origin allow-forms"
  referrerpolicy="no-referrer"
  allow="fullscreen; gamepad"
  title="Game Player"
  loading="lazy">
</iframe>
```

| Attribute | Value | Purpose |
|---|---|---|
| `sandbox` | `allow-scripts` | Allow JavaScript execution (required for games). |
| | `allow-pointer-lock` | Allow pointer lock API for FPS/precision games. |
| | `allow-same-origin` | Allow same-origin access (needed for storage). |
| | `allow-forms` | Allow form submission within the game. |
| | ~~`allow-popups`~~ | NOT allowed. Games cannot open new windows. |
| | ~~`allow-top-navigation`~~ | NOT allowed. Games cannot navigate the parent page. |
| `referrerpolicy` | `no-referrer` | Do not send referrer to the game hosting domain. |
| `allow` | `fullscreen; gamepad` | Allow fullscreen API and gamepad API. |

### 4.2 CSP frame-src Directive

```
frame-src https://games.afonsoft.dev;
```

Only the designated game hosting domain can be loaded in iframes. Any other domain will be blocked by the browser.

### 4.3 Game Loading Errors

```typescript
// angular/src/app/player/components/game-shell/game-shell.component.ts
@Component({
  selector: 'gh-game-shell',
  template: `
    @if (loadingError) {
      <div class="game-error">
        <h2>Failed to load game</h2>
        <p>{{ loadingError }}</p>
        <button (click)="retryLoad()">Retry</button>
        <a routerLink="/">Back to catalog</a>
      </div>
    } @else {
      <iframe
        [src]="gameUrl"
        sandbox="allow-scripts allow-pointer-lock allow-same-origin allow-forms"
        referrerpolicy="no-referrer"
        allow="fullscreen; gamepad"
        (load)="onGameLoaded()"
        (error)="onGameLoadError()">
      </iframe>
    }
  `
})
export class GameShellComponent implements OnInit {
  gameUrl: SafeResourceUrl | null = null;
  loadingError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private sanitizer: DomSanitizer,
    private gameService: GameService
  ) {}

  ngOnInit() {
    const slug = this.route.snapshot.paramMap.get('slug')!;
    this.gameService.getBuildUrl(slug).subscribe({
      next: (url) => {
        this.gameUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      },
      error: (err) => {
        this.loadingError = 'Game is currently unavailable. Please try again later.';
      }
    });
  }

  onGameLoaded() {
    this.loadingError = null;
  }

  onGameLoadError() {
    this.loadingError = 'Failed to load game resources. Please check your connection.';
  }

  retryLoad() {
    this.loadingError = null;
    this.ngOnInit();
  }
}
```

### 4.4 PostMessage Security

#### Parent → Game (game control commands)

```typescript
// angular/src/app/player/services/game-bridge.service.ts
@Injectable({ providedIn: 'root' })
export class GameBridgeService {
  private readonly GAME_ORIGIN = 'https://games.afonsoft.dev';

  constructor() {
    window.addEventListener('message', (event) => {
      this.handleMessage(event);
    });
  }

  private handleMessage(event: MessageEvent) {
    // Always validate origin
    if (event.origin !== this.GAME_ORIGIN) {
      return;
    }

    // Validate message type
    const validTypes = [
      'gameLoadingFinished',
      'gameplayStart',
      'gameplayStop',
      'commercialBreak',
      'rewardedBreak',
      'captureError',
      'measure',
      'scoreSubmit'
    ];

    if (!validTypes.includes(event.data?.type)) {
      return;
    }

    // Process validated message
    this.processGameEvent(event.data);
  }

  private processGameEvent(data: any) {
    switch (data.type) {
      case 'gameplayStart':
        this.sessionService.startSession(data.payload);
        break;
      case 'gameplayStop':
        this.sessionService.stopSession(data.payload);
        break;
      case 'scoreSubmit':
        this.scoreService.submitScore(data.payload);
        break;
      case 'captureError':
        this.errorService.reportGameError(data.payload);
        break;
      // …
    }
  }

  sendCommand(command: string, payload?: any) {
    const iframe = document.querySelector('iframe');
    iframe?.contentWindow?.postMessage(
      { command, payload },
      this.GAME_ORIGIN
    );
  }
}
```

#### Game → Parent (event listeners)

The Game SDK (hosted at `games.afonsoft.dev`) sends events to the parent:

```javascript
// In the game (on games.afonsoft.dev)
window.parent.postMessage({
  type: 'gameplayStart',
  payload: { gameId: '...', timestamp: Date.now() }
}, 'https://gamehub.afonsoft.dev');
```

The parent always validates:
1. `event.origin` matches the expected game hosting domain.
2. `event.data.type` is in the whitelist.
3. `event.data.payload` has the expected schema.

---

## 5. JWT Security

### 5.1 Token Storage

| Method | Storage | Pros | Cons |
|---|---|---|---|
| **HttpOnly Cookie** (recommended) | Server-set cookie | XSS-resistant, sent automatically | CSRF risk (mitigated with SameSite) |
| **In-Memory** | JavaScript variable | No XSS token theft | Lost on page refresh |

**Recommendation:** HttpOnly cookie with `SameSite=Strict; Secure; Path=/`.

```csharp
// Token cookie configuration
context.Response.Cookies.Append("GameHub.Auth", encryptedToken, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Path = "/",
    MaxAge = TimeSpan.FromHours(2)
});
```

### 5.2 Token Refresh Strategy

| Flow | Description |
|---|---|
| **Access Token** | Short-lived (2 hours). Stored in HttpOnly cookie. |
| **Refresh Token** | Long-lived (30 days). Stored in HttpOnly cookie (separate). |
| **Refresh Endpoint** | `POST /api/TokenAuth/Refresh` — validates refresh token, issues new access + refresh tokens. |
| **Sliding Expiration** | Refresh token lifetime resets on each use. |

```
Client                           Server
  │                                │
  │  GET /api/games                │
  │  Cookie: GameHub.Auth=eyJ...   │
  │───────────────────────────────>│
  │                                │
  │  401 Unauthorized              │
  │  (token expired)               │
  │<───────────────────────────────│
  │                                │
  │  POST /api/TokenAuth/Refresh   │
  │  Cookie: GameHub.Refresh=eyJ...│
  │───────────────────────────────>│
  │                                │
  │  200 OK                        │
  │  Set-Cookie: GameHub.Auth=...  │
  │  Set-Cookie: GameHub.Refresh=..│
  │<───────────────────────────────│
  │                                │
  │  GET /api/games                │
  │  (retry with new cookie)       │
  │───────────────────────────────>│
  │                                │
  │  200 OK                        │
  │<───────────────────────────────│
```

### 5.3 Token Revocation

| Mechanism | Trigger | Implementation |
|---|---|---|
| **Immediate Revocation** | Admin suspends user | Redis blacklist: `revoked:token:{jti}` with TTL matching token expiry |
| **Role Change** | Permission/role modified | Redis key `perm:{tenantId}:{userId}` invalidated. Token still valid but permission check uses fresh data. |
| **Logout** | User clicks logout | Client clears cookies. Server adds token `jti` to Redis blacklist. |
| **Password Change** | User changes password | All existing tokens revoked via `revoked:user:{userId}` |

```csharp
// TokenRevocationService.cs
public class TokenRevocationService : ITokenRevocationService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _maxTokenLifetime = TimeSpan.FromHours(2);

    public async Task RevokeTokenAsync(string jti)
    {
        var key = $"revoked:token:{jti}";
        await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _maxTokenLifetime
        });
    }

    public async Task<bool> IsTokenRevokedAsync(string jti)
    {
        var key = $"revoked:token:{jti}";
        var value = await _cache.GetStringAsync(key);
        return value != null;
    }

    public async Task RevokeAllUserTokensAsync(long userId)
    {
        var key = $"revoked:user:{userId}";
        await _cache.SetStringAsync(key, DateTime.UtcNow.ToString("O"));
    }
}
```

### 5.4 Key Rotation

| Key | Rotation Schedule | Method |
|---|---|---|
| **Signing Key** (RSA) | Every 90 days | New key generated, old key retained for validation during overlap period (7 days). JWKS endpoint exposes current keys. |
| **Encryption Key** | Every 90 days | Same rotation strategy as signing key. |
| **Refresh Token Secret** | Every 180 days | Rotated on schedule. Existing refresh tokens invalidated on rotation. |

```csharp
// JwtKeyRotationService.cs
public class JwtKeyRotationService : IJwtKeyRotationService
{
    private readonly IDistributedCache _cache;

    public async Task<RsaSecurityKey> GetCurrentSigningKeyAsync()
    {
        var keyJson = await _cache.GetStringAsync("jwt:signing:current");
        if (keyJson != null)
        {
            return JsonSerializer.Deserialize<RsaSecurityKey>(keyJson)!;
        }

        // Generate new key
        var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa) { KeyId = Guid.NewGuid().ToString("N") };

        await _cache.SetStringAsync("jwt:signing:current",
            JsonSerializer.Serialize(key),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
            });

        return key;
    }
}
```

---

## 6. Rate Limiting

### 6.1 Middleware Configuration

```csharp
// Program.cs
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfterTime)
            ? retryAfterTime.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers["Retry-After"] =
            ((int)retryAfter).ToString();

        var response = new
        {
            success = false,
            error = new
            {
                code = "GameHub.RateLimitExceeded",
                message = "Too many requests. Please try again later.",
                retryAfterSeconds = (int)retryAfter
            }
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response);
    };

    // Default: 100 requests per minute per IP
    options.AddPolicy("default", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });

    // Auth endpoints: 10 per minute per IP
    options.AddPolicy("auth", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: $"auth:{ip}",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });

    // Gameplay events: 60 per minute per session
    options.AddPolicy("gameplay", context =>
    {
        var sessionId = context.Request.Headers["X-Session-Id"].ToString()
                        ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: $"gameplay:{sessionId}",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });

    // Build upload: 5 per hour per developer
    options.AddPolicy("upload", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? "anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: $"upload:{userId}",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromHours(1),
                SegmentsPerWindow = 4
            });
    });
});

// In pipeline
app.UseRateLimiter();
```

### 6.2 Per-Endpoint Limits

| Endpoint | Policy | Limit | Window | Partition Key |
|---|---|---|---|---|
| `POST /api/TokenAuth/Authenticate` | `auth` | 10 | 1 min | IP |
| `POST /api/TokenAuth/Refresh` | `auth` | 20 | 1 min | IP |
| `POST /api/TokenAuth/Register` | `auth` | 5 | 1 min | IP |
| `GET /api/services/app/Game/*` | `default` | 100 | 1 min | IP |
| `POST /api/services/app/Game/UploadBuild` | `upload` | 5 | 1 hour | UserId |
| `POST /api/services/app/Gameplay/*` | `gameplay` | 60 | 1 min | SessionId |
| `GET /api/services/app/Leaderboard/*` | `default` | 100 | 1 min | IP |
| All other | `default` | 100 | 1 min | IP |

### 6.3 Response Headers

Every response includes rate limit headers:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1719859200
```

### 6.4 429 Response Body

```json
{
  "success": false,
  "error": {
    "code": "GameHub.RateLimitExceeded",
    "message": "Too many requests. Please try again later.",
    "retryAfterSeconds": 45
  }
}
```

---

## 7. HTTPS Enforcement

### 7.1 HSTS Configuration

```csharp
// Program.cs
if (!env.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
```

HSTS header:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

| Directive | Value | Purpose |
|---|---|---|
| `max-age` | `31536000` (1 year) | Browser remembers HTTPS-only for 1 year. |
| `includeSubDomains` | — | Applies to all subdomains (hub, admin, API). |
| `preload` | — | Eligible for browser HSTS preload lists. |

### 7.2 HTTP → HTTPS Redirect

```csharp
// Program.cs
app.UseHttpsRedirection();

// Force redirect all HTTP traffic to HTTPS
app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps && !env.IsDevelopment())
    {
        var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(httpsUrl, permanent: true);
        return;
    }
    await next();
});
```

### 7.3 Certificate Management

| Environment | Certificate | Renewal |
|---|---|---|
| **Production** | Let's Encrypt via certbot | Automatic via cron (daily check) |
| **Staging** | Let's Encrypt staging | Same as production |
| **Development** | Dev certificate (`dotnet dev-certs https`) | Manual |

```bash
# Certbot auto-renewal cron
0 0 1 * * certbot renew --quiet --deploy-hook "systemctl reload nginx"
```

---

## 8. Input Validation

### 8.1 Request Body Validation

All input DTOs use ABP DataAnnotations (see §14). Validation runs automatically through ABP's filter pipeline.

```csharp
// Example: automatic validation
[HttpPost]
public async Task<GameSummaryDto> CreateGameDraft(CreateGameDraftInput input)
{
    // ABP automatically validates:
    // - Title: Required, StringLength(200, MinimumLength = 3)
    // - ShortDescription: Required, StringLength(200, MinimumLength = 10)
    // - AgeRating: Required, must be valid enum
    // Returns 400 Bad Request with structured error on failure
}
```

### 8.2 File Upload Validation

```csharp
// GameBuildValidator.cs
public class GameBuildValidator : IGameBuildValidator
{
    private static readonly string[] AllowedExtensions = { ".zip" };
    private static readonly string[] BlockedExtensions =
    {
        ".exe", ".dll", ".bat", ".cmd", ".ps1",
        ".msi", ".com", ".scr", ".vbs", ".js", ".jar"
    };
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
    private const string RequiredContentType = "application/zip";

    public ValidationResult Validate(IFormFile file)
    {
        var errors = new List<string>();

        // Content type
        if (file.ContentType != RequiredContentType)
        {
            errors.Add($"Invalid content type '{file.ContentType}'. Only ZIP files are accepted.");
        }

        // File size
        if (file.Length > MaxFileSizeBytes)
        {
            errors.Add($"File size {file.Length} exceeds maximum of {MaxFileSizeBytes} bytes.");
        }

        // Extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            errors.Add($"File extension '{extension}' is not allowed.");
        }

        // Blocked extensions in archive (checked during extraction)
        // See BuildValidationService for archive scanning

        return errors.Count == 0
            ? ValidationResult.Valid()
            : ValidationResult.Invalid(errors);
    }
}
```

### 8.3 Archive Content Validation

```csharp
// BuildValidationService.cs
public class BuildValidationService : IBuildValidationService
{
    private static readonly string[] BlockedArchiveEntries =
    {
        ".exe", ".dll", ".bat", ".cmd", ".ps1",
        ".msi", ".com", ".scr", ".vbs", ".jar"
    };

    public async Task<ValidationResult> ValidateArchiveContentsAsync(Stream zipStream)
    {
        var errors = new List<string>();

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        // Must contain index.html
        var hasIndexHtml = archive.Entries.Any(e =>
            e.FullName.Equals("index.html", StringComparison.OrdinalIgnoreCase));

        if (!hasIndexHtml)
        {
            errors.Add("Build must contain an 'index.html' file at the root.");
        }

        // Check for blocked executables
        foreach (var entry in archive.Entries)
        {
            var extension = Path.GetExtension(entry.FullName).ToLowerInvariant();
            if (BlockedArchiveEntries.Contains(extension))
            {
                errors.Add($"Blocked file type found: '{entry.FullName}'. Executable files are not allowed.");
            }
        }

        // Validate SHA-256
        zipStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(zipStream);
        var hashString = Convert.ToHexString(hash).ToLowerInvariant();

        return errors.Count == 0
            ? ValidationResult.Valid()
            : ValidationResult.Invalid(errors);
    }
}
```

### 8.4 SQL Injection Prevention

EF Core parameterized queries prevent SQL injection by default:

```csharp
// SAFE — parameterized query
var games = await _dbContext.Games
    .Where(g => g.Title.Contains(searchTerm))
    .ToListAsync();

// SAFE — parameterized query
var game = await _dbContext.Games
    .FirstOrDefaultAsync(g => g.Slug == slug);

// DANGEROUS — NEVER do this
// var games = await _dbContext.Games
//     .FromSqlRaw($"SELECT * FROM Games WHERE Title LIKE '%{searchTerm}%'")
//     .ToListAsync();
```

### 8.5 XSS Prevention

| Layer | Mechanism |
|---|---|
| **Output Encoding** | Angular auto-escapes template bindings (`{{ value }}`). Use `DomSanitizer` only for trusted URLs. |
| **CSP** | `script-src 'self'` blocks inline scripts and eval. |
| **Input Validation** | `StringLength` constraints prevent oversized payloads. |
| **HTML Content** | Game descriptions support HTML. Sanitize with a library like `HtmlSanitizer` before storage. |

---

## 9. Audit Logging

### 9.1 Events to Audit

| Category | Event | Severity |
|---|---|---|
| **Authentication** | Successful login | Info |
| | Failed login (3+ attempts) | Warning |
| | Password change | Info |
| | Account locked | Warning |
| **Authorization** | Permission granted/revoked | Warning |
| | Role assigned/removed | Warning |
| | Unauthorized access attempt | Warning |
| **Games** | Game created | Info |
| | Game published | Info |
| | Game suspended | Warning |
| | Game deleted | Warning |
| **Builds** | Build uploaded | Info |
| | Build approved | Info |
| | Build rejected | Warning |
| | Build published | Info |
| **Moderation** | Review started | Info |
| | Review completed (Approved) | Info |
| | Review completed (Rejected) | Warning |
| **Admin** | Feature flag toggled | Warning |
| | User created/suspended | Info |
| | Category/tag created/modified | Info |
| **System** | Rate limit exceeded | Warning |
| | CSP violation reported | Warning |
| | Exception occurred | Error |

### 9.2 Audit Log Schema

```csharp
namespace GameHub.Auditing;

/// <summary>
/// Audit log entry stored in PostgreSQL.
/// </summary>
public class AuditLogEntry
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>UTC timestamp of the event.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>User who performed the action (null for system actions).</summary>
    public long? UserId { get; set; }

    /// <summary>User's display name at time of action.</summary>
    public string? UserName { get; set; }

    /// <summary>Tenant identifier (for multi-tenant).</summary>
    public int? TenantId { get; set; }

    /// <summary>Action performed (e.g., "Game.Publish", "Build.Approve").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Entity type affected (e.g., "Game", "GameBuild", "User").</summary>
    public string Entity { get; set; } = string.Empty;

    /// <summary>Entity identifier.</summary>
    public string? EntityId { get; set; }

    /// <summary>Previous state (JSON) for updates.</summary>
    public string? OldValue { get; set; }

    /// <summary>New state (JSON) for creates/updates.</summary>
    public string? NewValue { get; set; }

    /// <summary>Client IP address.</summary>
    public string? IpAddress { get; set; }

    /// <summary>User agent string.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Correlation ID for request tracing.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Additional metadata (JSON).</summary>
    public string? ExtraData { get; set; }
}
```

### 9.3 Audit Log Table (PostgreSQL)

```sql
CREATE TABLE audit_log_entries (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id         BIGINT,
    user_name       VARCHAR(256),
    tenant_id       INTEGER,
    action          VARCHAR(200) NOT NULL,
    entity          VARCHAR(100) NOT NULL,
    entity_id       VARCHAR(100),
    old_value       JSONB,
    new_value       JSONB,
    ip_address      INET,
    user_agent      VARCHAR(500),
    correlation_id  VARCHAR(100),
    extra_data      JSONB
);

CREATE INDEX idx_audit_log_timestamp ON audit_log_entries (timestamp);
CREATE INDEX idx_audit_log_user_id ON audit_log_entries (user_id);
CREATE INDEX idx_audit_log_entity ON audit_log_entries (entity, entity_id);
CREATE INDEX idx_audit_log_action ON audit_log_entries (action);
CREATE INDEX idx_audit_log_correlation_id ON audit_log_entries (correlation_id);
```

### 9.4 Audit Interceptor

```csharp
// AuditLogInterceptor.cs
public class AuditLogInterceptor : IAsyncInterceptor
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public async Task InterceptAsync(IInvocation invocation)
    {
        var auditAttribute = invocation.MethodInvocationTarget
            .GetCustomAttribute<AuditAttribute>();

        if (auditAttribute == null)
        {
            await invocation.ProceedAsync();
            return;
        }

        var entry = new AuditLogEntry
        {
            Timestamp = DateTime.UtcNow,
            UserId = AbpSession.UserId,
            UserName = AbpSession.UserName,
            TenantId = AbpSession.TenantId,
            Action = auditAttribute.Action,
            Entity = auditAttribute.Entity,
            IpAddress = _httpContextAccessor.HttpContext?.Connection
                .RemoteIpAddress?.ToString(),
            CorrelationId = _correlationIdAccessor.GetCorrelationId()
        };

        try
        {
            await invocation.ProceedAsync();
            entry.ExtraData = JsonSerializer.Serialize(new { success = true });
        }
        catch (Exception ex)
        {
            entry.ExtraData = JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
            throw;
        }
        finally
        {
            await _auditLogRepository.InsertAsync(entry);
        }
    }
}
```

### 9.5 Usage Example

```csharp
[Audit(Action = "Game.Publish", Entity = "Game")]
public async Task PublishGameAsync(PublishGameInput input)
{
    var game = await _gameRepository.GetAsync(input.GameId);
    game.Publish();
    await _gameRepository.UpdateAsync(game);
}
```

### 9.6 Retention Policy

| Policy | Duration | Action |
|---|---|---|
| **Active audit logs** | 5 years | Stored in `audit_log_entries` table |
| **Archived logs** | 10 years | Exported to cold storage (S3 Glacier or equivalent) |
| **Deletion** | After 10 years | Permanent deletion with confirmation |

```sql
-- Retention cleanup job (runs monthly via Hangfire)
DELETE FROM audit_log_entries
WHERE timestamp < NOW() - INTERVAL '5 years'
  AND id IN (
      SELECT id FROM audit_log_entries
      WHERE timestamp < NOW() - INTERVAL '5 years'
      ORDER BY timestamp
      LIMIT 10000
  );
```

### 9.7 Audit Dashboard Queries

```sql
-- Most audited entities
SELECT entity, action, COUNT(*) as count
FROM audit_log_entries
WHERE timestamp > NOW() - INTERVAL '30 days'
GROUP BY entity, action
ORDER BY count DESC;

-- Failed logins by IP
SELECT ip_address, COUNT(*) as attempts
FROM audit_log_entries
WHERE action = 'Auth.LoginFailed'
  AND timestamp > NOW() - INTERVAL '24 hours'
GROUP BY ip_address
HAVING COUNT(*) > 5;

-- Game publish activity
SELECT DATE(timestamp) as date, COUNT(*) as publishes
FROM audit_log_entries
WHERE action = 'Game.Publish'
  AND timestamp > NOW() - INTERVAL '30 days'
GROUP BY DATE(timestamp)
ORDER BY date;
```
