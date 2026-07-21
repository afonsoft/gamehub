# 08 - Segurança, LGPD e Compliance

## 1. JWT Configuration

### Algoritmo

| Ambiente | Algoritmo | Descrição |
|----------|-----------|-----------|
| Development | HS256 | Simétrico, chave em `appsettings.Development.json` |
| Production | RS256 | Assimétrico, chave privada em Key Vault / variável de ambiente |

### Configuração

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });
```

### Tokens

| Propriedade | Access Token | Refresh Token |
|------------|-------------|---------------|
| Expiração | 30 minutos | 7 dias |
| Renovação | Não | Sim (single use) |
| Armazenamento | Memory (não localStorage) | HttpOnly cookie |
| Revogação | Blacklist no Redis | Blacklist no Redis |

### Claims

| Claim | Fonte | Descrição |
|-------|-------|-----------|
| `sub` | UserId (Guid) | Identificador único do usuário |
| `name` | UserName | Nome de exibição |
| `email` | EmailAddress | Email do usuário |
| `role` | UserRole[] | Array de roles (Player, Developer, Moderator, Admin) |
| `tenantId` | TenantId | ID do tenant (multi-tenant ABP) |
| `iat` | DateTime | Issued At |
| `exp` | DateTime | Expiration |

### Endpoints

```http
POST /api/TokenAuth/Authenticate          ← Login, retorna access + refresh token
POST /api/TokenAuth/RefreshToken          ← Renova access token using refresh token
POST /api/TokenAuth/Logout                ← Invalida tokens
```

### Segurança do Token

- **Nunca** armazenar tokens em `localStorage` (XSS vulnerability)
- Usar `sessionStorage` ou memory para access token
- Refresh token em `HttpOnly`, `Secure`, `SameSite=Strict` cookie
- Implementar refresh silencioso no frontend (interceptor renova antes de expirar)
- Blacklist de tokens revogados no Redis com TTL = expiração do token

---

## 2. RBAC Permission Matrix

### Roles

| Role | Descrição | Atribuição |
|------|-----------|-----------|
| Player | Usuário autenticado que joga | Todo usuário registrado |
| Developer | Desenvolvedor que publica jogos | Aprovação manual ou self-service |
| Moderator | Revisor de conteúdo | Atribuído por Admin |
| Admin | Administrador completo | Atribuído por Super Admin |

### Matriz de Permissões

| Permissão | Player | Developer | Moderator | Admin |
|-----------|--------|-----------|-----------|-------|
| **Games** | | | | |
| Pages.Games.View | ✓ (published) | ✓ (own + published) | ✓ (all) | ✓ (all) |
| Pages.Games.Create | ✗ | ✓ | ✗ | ✓ |
| Pages.Games.Edit | ✗ | ✓ (own, draft only) | ✗ | ✓ |
| Pages.Games.Delete | ✗ | ✓ (own, draft only) | ✗ | ✓ |
| Pages.Games.Publish | ✗ | ✗ | ✗ | ✓ |
| Pages.Games.Suspend | ✗ | ✗ | ✗ | ✓ |
| **Builds** | | | | |
| Pages.Builds.Upload | ✗ | ✓ (own games) | ✗ | ✓ |
| Pages.Builds.View | ✗ | ✓ (own games) | ✓ (all) | ✓ (all) |
| Pages.Builds.Approve | ✗ | ✗ | ✓ | ✓ |
| Pages.Builds.Reject | ✗ | ✗ | ✓ | ✓ |
| **Moderation** | | | | |
| Pages.Moderation.ViewQueue | ✗ | ✗ | ✓ | ✓ |
| Pages.Moderation.ViewDetail | ✗ | ✗ | ✓ | ✓ |
| Pages.Moderation.CompleteReview | ✗ | ✗ | ✓ | ✓ |
| **Categories & Tags** | | | | |
| Pages.Categories.View | ✓ | ✓ | ✓ | ✓ |
| Pages.Categories.Manage | ✗ | ✗ | ✗ | ✓ |
| Pages.Tags.View | ✓ | ✓ | ✓ | ✓ |
| Pages.Tags.Manage | ✗ | ✗ | ✗ | ✓ |
| **Dashboard & Admin** | | | | |
| Pages.Dashboard.View | ✗ | ✗ | ✗ | ✓ |
| Pages.FeatureFlags.Manage | ✗ | ✗ | ✗ | ✓ |
| Pages.AuditLog.View | ✗ | ✗ | ✗ | ✓ |
| Pages.Users.Manage | ✗ | ✗ | ✗ | ✓ |
| **Gameplay** | | | | |
| Pages.Gameplay.StartSession | ✓ | ✓ | ✓ | ✓ |
| Pages.Gameplay.SendEvent | ✓ | ✓ | ✓ | ✓ |
| Pages.Leaderboard.SubmitScore | ✓ | ✓ | ✓ | ✓ |
| Pages.Leaderboard.GetTop | ✓ | ✓ | ✓ | ✓ |
| **Reports** | | | | |
| Pages.Reports.Submit | ✓ | ✓ | ✓ | ✓ |
| Pages.Reports.Manage | ✗ | ✗ | ✓ | ✓ |

### Implementação ABP

```csharp
// Na definição de permissões (AppPermissions.cs)
public const string Pages_Games = "Pages.Games";
public const string Pages_Games_Create = "Pages.Games.Create";
public const string Pages_Games_Edit = "Pages.Games.Edit";
public const string Pages_Games_Delete = "Pages.Games.Delete";
public const string Pages_Games_Publish = "Pages.Games.Publish";
public const string Pages_Games_Suspend = "Pages.Games.Suspend";
public const string Pages_Builds_Upload = "Pages.Builds.Upload";
public const string Pages_Builds_View = "Pages.Builds.View";
public const string Pages_Builds_Approve = "Pages.Builds.Approve";
public const string Pages_Builds_Reject = "Pages.Builds.Reject";
public const string Pages_Moderation_ViewQueue = "Pages.Moderation.ViewQueue";
public const string Pages_Moderation_ViewDetail = "Pages.Moderation.ViewDetail";
public const string Pages_Moderation_CompleteReview = "Pages.Moderation.CompleteReview";
public const string Pages_Categories = "Pages.Categories";
public const string Pages_Categories_Manage = "Pages.Categories.Manage";
public const string Pages_Tags = "Pages.Tags";
public const string Pages_Tags_Manage = "Pages.Tags.Manage";
public const string Pages_Dashboard = "Pages.Dashboard";
public const string Pages_FeatureFlags = "Pages.FeatureFlags";
public const string Pages_AuditLog = "Pages.AuditLog";
public const string Pages_Users = "Pages.Users";
public const string Pages_Gameplay = "Pages.Gameplay";
public const string Pages_Leaderboard = "Pages.Leaderboard";
public const string Pages_Reports = "Pages.Reports";
public const string Pages_Reports_Manage = "Pages.Reports.Manage";
```

---

## 3. CSP Header Directives

### Content Security Policy

```
default-src 'self';
script-src 'self';
style-src 'self' 'unsafe-inline';
img-src 'self' data: https:;
font-src 'self';
connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev;
frame-src https://games.afonsoft.dev;
frame-ancestors 'self' https://gamehub.afonsoft.dev https://gamehub-admin.afonsoft.dev;
object-src 'none';
base-uri 'self';
form-action 'self';
```

### Notas

- `frame-src` aponta para `games.afonsoft.dev` (isolamento de jogos)
- `frame-ancestors` permite embedding apenas pelos dois frontends legítimos
- `connect-src` inclui WebSocket para possíveis updates em tempo real
- `unsafe-inline` em styles é necessário para componentes Angular (avaliar `nonce` em produção)
- Jogos carregados em iframe NÃO herdam CSP do host (CSP se aplica ao frame, não ao iframe)

### Implementação

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self'; " +
        "connect-src 'self' https://gamehub-api.afonsoft.dev; " +
        "frame-src https://games.afonsoft.dev; " +
        "frame-ancestors 'self' https://gamehub.afonsoft.dev https://gamehub-admin.afonsoft.dev; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self';");
    await next();
});
```

---

## 4. Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "0");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.Append("Strict-Transport-Security",
        "max-age=31536000; includeSubDomains");
    await next();
});
```

| Header | Valor | Justificativa |
|--------|-------|---------------|
| X-Content-Type-Options | nosniff | Previne MIME sniffing |
| X-Frame-Options | DENY | Previne clickjacking (exceto game iframe via CSP) |
| X-XSS-Protection | 0 | Desabilita XSS filter obsoleto do IE (CSP é melhor) |
| Referrer-Policy | strict-origin-when-cross-origin | Envia referrer apenas no mesmo origin |
| Permissions-Policy | camera=(), microphone=(), geolocation=() | Desabilita APIs sensíveis |
| Strict-Transport-Security | max-age=31536000; includeSubDomains | Força HTTPS por 1 ano |

---

## 5. CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameHub", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",               // Game Hub dev
                "http://localhost:4201",               // Admin dev
                "https://gamehub.afonsoft.dev",        // Game Hub prod
                "https://gamehub-admin.afonsoft.dev",  // Admin prod
                "https://games.afonsoft.dev")          // Game isolation
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();                     // Para cookies HttpOnly
    });
});
```

---

## 6. Iframe Isolation Strategy

### Domínios

| Domínio | Propósito | Conteúdo |
|---------|-----------|----------|
| `gamehub.afonsoft.dev` | Frontend principal | Angular SPA |
| `gamehub-admin.afonsoft.dev` | Admin | Angular SPA admin |
| `gamehub-api.afonsoft.dev` | API | ASP.NET Core API |
| `games.afonsoft.dev` | Isolamento de jogos | Static files dos builds |

### Isolamento

- Jogos são servidos a partir de `games.afonsoft.dev` (domínio separado)
- `gamehub.afonsoft.dev` carrega jogos em `<iframe src="https://games.afonsoft.dev/...">`
- CSP `frame-src` limita embeds a `games.afonsoft.dev` apenas
- `X-Frame-Options: DENY` no host bloqueia embedding de terceiros
- `frame-ancestors` permite que apenas `gamehub.afonsoft.dev` e `gamehub-admin.afonsoft.dev` embedem
- Cookies NÃO são compartilhados entre domínios (SameSite=Strict)

### Sandboxing do iframe

```html
<iframe
  src="https://games.afonsoft.dev/{game-slug}/{version}/index.html"
  sandbox="allow-scripts allow-pointer-lock allow-same-origin allow-forms"
  referrerpolicy="no-referrer"
  allow="fullscreen; gamepad"
  class="game-frame">
</iframe>
```

> `allow-same-origin` é necessário para jogos que usam APIs do browser (localStorage, etc.). O isolamento real vem do domínio separado (`games.afonsoft.dev` vs `gamehub.afonsoft.dev`).

---

## 7. LGPD Compliance

### 7.1 Registro de Tratamento de Dados

| Atividade | Dados Coletados | Base Legal | Retenção | Finalidade |
|-----------|----------------|------------|----------|------------|
| Autenticação | Nome, email, senha (hash) | Execução de contrato | Até exclusão | Identificação do usuário |
| Perfil de Desenvolvedor | Nome legal, website, email suporte | Execução de contrato | Até exclusão | Publicação de jogos |
| Sessão de Gameplay | GameId, UserId/AnonymousIdHash, device, browser, country | Legítimo interesse | 12 meses | Analytics de uso |
| Eventos de Gameplay | EventType, EventName, PayloadJson | Legítimo interesse | 6 meses (raw), 24 meses (aggregated) | Melhoria de produto |
| Score/Leaderboard | UserId, Score | Execução de contrato | Até exclusão | Funcionalidade de ranking |
| Report de Conteúdo | UserId (opcional), Reason, Description | Legítimo interesse | 5 anos | Moderação |
| Audit Log | UserId, Action, Entity, Timestamp | Obrigação legal | 5 anos | Compliance e auditoria |
| Feature Flags | Flag name, isEnabled, userId | Legítimo interesse | 12 meses | Controle de features |
| Login/Session | IP, user agent, timestamp | Obrigação legal | 12 meses | Segurança |

### 7.2 Dados Pessoais por Role

| Role | Dados Coletados | Necessário |
|------|----------------|-----------|
| Player (anônimo) | AnonymousIdHash (SHA256), device, browser | Não coleta PII |
| Player (autenticado) | Nome, email, scores, play history | Sim |
| Developer | Nome legal, email, website, jogos publicados | Sim |
| Moderator/Admin | Nome, email, roles | Sim |

### 7.3 Direitos do Titular (LGPD Art. 18)

| Direito | Implementação | Endpoint |
|---------|--------------|----------|
| **Acesso** (Art. 18, I) | Exportar todos os dados do usuário em JSON | `GET /api/services/app/Account/ExportData` |
| **Retificação** (Art. 18, III) | Editar nome, email, perfil | `PUT /api/services/app/Account/UpdateProfile` |
| **Exclusão** (Art. 18, VI) | Anonimizar dados, manter apenas dados com obrigação legal | `DELETE /api/services/app/Account/DeleteMyAccount` |
| **Portabilidade** (Art. 18, V) | Exportar dados em formato estruturado (JSON/CSV) | `GET /api/services/app/Account/ExportData?format=csv` |
| **Oposição** (Art. 18, IV) | Opt-out de analytics e marketing | `PUT /api/services/app/Account/OptOut` |
| **Informação sobre compartilhamento** | Listar com quem os dados são compartilhados | `GET /api/services/app/Account/DataSharingInfo` |

### 7.4 Exclusão de Conta — Fluxo

1. Usuário solicita exclusão via `DELETE /api/services/app/Account/DeleteMyAccount`
2. Sistema verifica:
   - Jogos publicados ativos → bloqueia exclusão até arquivar
   - Sessões de gameplay ativas → aguarda conclusão
3. Após validação:
   - Anonimiza dados pessoais (nome → "Deleted User", email → hash)
   - Mantém dados agregados (play counts, etc.) sem identificação
   - Mantém audit logs por obrigação legal
   - Remove refresh tokens do Redis
   - Marca conta como `IsDeleted = true`
4. Confirma exclusão por email

### 7.5 Retenção de Dados

| Tipo de Dado | Retenção Raw | Retenção Agregado | Ação |
|-------------|-------------|-------------------|------|
| PlaySession | 12 meses | 24 meses (contagens) | Hard delete raw, manter snapshot |
| GameplayEvent | 6 meses | 24 métricas agregadas | Hard delete raw, manter GameMetricSnapshot |
| GameMetricSnapshot | — | 24 meses | Manter para gráficos |
| UserAccount | Até exclusão | — | Anonimizar dados PII |
| AuditLog | 5 anos | — | Manter por obrigação legal |
| UserReport | 5 anos | — | Manter para compliance de moderação |
| RefreshToken | 7 dias | — | Auto-expira |
| BlacklistedToken | Até expiração do token | — | Auto-expira via TTL Redis |

### 7.6 Notificação de Brecha (ANPD)

- Prazo: 72 horas após conhecimento da brecha
- Dados a reportar: natureza dos dados, número de titulares afetados, medidas de contenção, riscos estimados
- Processo:
  1. Detectar brecha (monitoramento de acessos anômalos)
  2. Conter (bloquear acesso, revogar tokens)
  3. Avaliar impacto (quais dados, quantos usuários)
  4. Notificar ANPD (via formulário ou email)
  5. Notificar usuários afetados se risco alto
  6. Documentar em audit log

### 7.7 DPO (Data Protection Officer)

- Designar DPO em produção
- Contato: `dpo@afonsoft.dev`
- Responsabilidades:
  - Responder solicitações de titulares
  - Monitorar compliance LGPD
  - Relatar incidentes à ANPD
  - Treinar equipe

### 7.8 Cookies

| Cookie | Tipo | Duração | Finalidade | Consentimento |
|--------|------|---------|-----------|--------------|
| `.AspNetCore.Antiforgery` | Essencial | Session | CSRF protection | Não requer |
| `refresh_token` | Essencial | 7 dias | Renovação de sessão | Não requer |
| `_gid` | Analytics | 24 horas | Google Analytics (se habilitado) | Sim |

---

## 8. Supply Chain Security

- **Dependabot/Renovate**: scan automático de dependências NuGet/npm
- **Container Scan**: Trivy ou Grype em pipeline CI/CD
- **SBOM**: Gerado via `dotnet tool install -g dotnet-sbom` (opcional)
- **Imagens Docker**: Assinatura com cosign em produção
- **Lock files**: Commitar `package-lock.json` e `packages.lock.json`
- **Privilegios mínimos**: containers rodando como non-root

---

## 9. Checklist de Publicação de Jogo

### Validação Técnica

- [ ] `index.html` existe no pacote
- [ ] Tamanho máximo: 100MB
- [ ] SHA256 calculado e armazenado
- [ ] Sem executáveis (.exe, .dll, .bat, .cmd, .ps1)
- [ ] Sem scripts de terceiros não autorizados
- [ ] Tipos de arquivo permitidos: .html, .js, .css, .json, .png, .jpg, .svg, .woff, .woff2, .mp3, .mp4

### Validação de Conteúdo

- [ ] Conteúdo adequado para público geral
- [ ] Sem conteúdo ofensivo, discriminatório ou sexual
- [ ] Sem violência gráfica
- [ ] Sem jogos de azar
- [ ] Sem uso indevido de marca/IP de terceiros
- [ ] Desenvolvedor declara propriedade/licença dos assets

### Validação Técnica do Jogo

- [ ] Funciona em desktop, mobile e tablet
- [ ] Suporta incognito/private mode
- [ ] Escala corretamente em 16:9 ou orientação declarada
- [ ] Textos localizáveis quando aplicável
- [ ] Não faz chamadas externas não autorizadas
- [ ] Não tenta acessar APIs do browser proibidas
- [ ] Respeita CSP (não tenta carregar scripts externos)
