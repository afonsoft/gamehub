# Eaf.Middleware.AzureActiveDirectory

## Visão Geral

O módulo `Eaf.Middleware.AzureActiveDirectory` fornece integração com Azure Active Directory para autenticação em aplicações EAF.

## Propósito

Permitir que usuários se autentiquem no EAF utilizando suas credenciais do Azure AD, facilitando o Single Sign-On (SSO) em ambientes corporativos.

## Instalação

```bash
Install-Package Microsoft.Identity.Web
Install-Package Microsoft.Identity.Web.UI
```

## Configuração

### 1. Registro da Aplicação no Azure AD

1. Navegue até "Azure Active Directory" > "App registrations" > "New registration"
2. Dê um nome à sua aplicação EAF
3. Em "Redirect URI", selecione "Web" e insira o URI de redirecionamento
4. Clique em "Register"
5. Anote o `Application (client) ID` e o `Directory (tenant) ID`

### 2. Configuração em appsettings.json

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "seudominio.onmicrosoft.com",
    "TenantId": "seu-tenant-id-azure-ad",
    "ClientId": "seu-client-id-da-app-eaf",
    "ClientSecret": "seu-client-secret-se-necessario",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

### 3. Configuração em Startup.cs

```csharp
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
```

## Uso

Após a autenticação, os claims do Azure AD estão disponíveis no `HttpContext.User.Claims`:

```csharp
public class UserInfoService : ApplicationService
{
    public async Task<MyEafUserDto> GetCurrentAzureAdUserInfo()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var displayName = User.FindFirstValue("name");
        return new MyEafUserDto { Email = email, DisplayName = displayName };
    }
}
```

## Troubleshooting

- **Redirect URI Mismatch**: Verifique se o Redirect URI corresponde exatamente ao configurado no Azure AD
- **Consentimento do Administrador**: Algumas permissões exigem consentimento do administrador
- **Nonce Cookie Issue**: Garanta que a proteção de dados do ASP.NET Core esteja configurada corretamente
