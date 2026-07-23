# Autenticação no EAF (Enterprise Application Foundation)

## 1. Introdução à Autenticação no EAF

A autenticação é o processo de verificar a identidade de um usuário, sistema ou serviço que tenta acessar a aplicação. É um pilar fundamental da segurança, garantindo que apenas entidades legítimas possam interagir com os recursos protegidos.

O EAF (Enterprise Application Foundation) é uma plataforma open source que baseia-se fortemente no **ASP.NET Boilerplate Zero (`AbpZeroCoreModule` ou similar)** para suas funcionalidades centrais de autenticação. Isso inclui uma integração profunda com o **ASP.NET Identity**, fornecendo um sistema robusto e extensível para gerenciamento de usuários, papéis, permissões, e os próprios mecanismos de autenticação.

Este documento descreve os principais mecanismos de autenticação utilizados no EAF, como eles são configurados e as melhores práticas associadas.

## 2. Mecanismos Centrais de Autenticação

O EAF suporta múltiplos mecanismos de autenticação para atender a diferentes tipos de clientes e cenários de aplicação:

### 2.1. Autenticação Baseada em Cookie

*   **Uso Principal**: Aplicações web tradicionais, como interfaces de usuário (UI) administrativas construídas com ASP.NET Core MVC (e.g., módulos em `Eaf.Web.Mvc`).
*   **Funcionamento**: Após um login bem-sucedido, um cookie de autenticação é enviado ao navegador do usuário. Este cookie é então enviado automaticamente pelo navegador em requisições subsequentes, permitindo que o servidor identifique o usuário e mantenha a sessão.
*   **Configuração e Segurança (ASP.NET Core Identity)**:
    *   O EAF utiliza a configuração padrão do ASP.NET Core Identity para cookies, geralmente definida em `Startup.cs` ou em um módulo de inicialização web.
    *   **Opções Chave do Cookie**:
        *   `HttpOnly`: **True** por padrão. Impede que o cookie seja acessado por JavaScript no lado do cliente, mitigando ataques XSS.
        *   `SecurePolicy`: **Always** em produção. Garante que o cookie só seja transmitido sobre HTTPS.
        *   `SameSite`: **Lax** ou **Strict**. Ajuda a proteger contra ataques CSRF. `Lax` é um bom padrão para a maioria das aplicações web.
        *   `ExpireTimeSpan`: Define a duração da validade do cookie.
    *   O nome do esquema de autenticação é tipicamente `CookieAuthenticationDefaults.AuthenticationScheme`.

### 2.2. Autenticação Baseada em Token (JWT - JSON Web Token)

*   **Uso Principal**: Single Page Applications (SPAs) como Angular, aplicações móveis, e clientes de API externos.
*   **Funcionamento**:
    1.  O cliente envia credenciais (e.g., username/password) para um endpoint de autenticação específico (e.g., `/api/TokenAuth/Authenticate`).
    2.  Se as credenciais forem válidas, o servidor gera um JWT e o retorna ao cliente.
    3.  O cliente armazena o JWT (geralmente no Local Storage ou Session Storage para SPAs, ou de forma segura em apps móveis) e o envia em requisições subsequentes para APIs protegidas, tipicamente no header `Authorization` com o schema `Bearer`.
        `Authorization: Bearer <seu_jwt_aqui>`
    4.  O servidor valida o JWT em cada requisição (verifica assinatura, expiração, issuer, audience).
*   **Geração e Validação de JWT**:
    *   O ABP fornece o `TokenAuthController` que lida com a geração de tokens.
    *   A validação é feita por um middleware JWT Bearer (`JwtBearerDefaults.AuthenticationScheme`) configurado no pipeline do ASP.NET Core.
*   **Configuração (Exemplo em `appsettings.json`)**:
    ```json
    {
      "Authentication": {
        "JwtBearer": {
          "IsEnabled": "true",
          "SecurityKey": "EAF_DEFAULT_SECURITY_KEY_ASYMMETRIC_XE6V8_VERY_VERY_SECURE_AND_COMPLEX_!12345", // DEVE SER ALTERADO E PROTEGIDO!
          "Issuer": "Eaf", // Emissor do token
          "Audience": "EafApplication" // Destinatário do token
        }
      }
    }
    ```
    *   **`SecurityKey`**: Uma chave secreta forte usada para assinar e verificar os tokens. **É crucial que esta chave seja complexa, única por ambiente e mantida em segredo** (e.g., usando Azure Key Vault em produção, como descrito em `eaf-extensions.md`).
    *   `Issuer` e `Audience`: Usados para validar que o token foi emitido pela autoridade correta e para a aplicação correta.

### 2.3. Processo de Login

*   **Cookie-based**: Tipicamente gerenciado pelo `AccountController` do ABP (`Abp.AspNetCore.Mvc.Controllers.AccountController`). Ele usa o `AbpLogInManager` para validar o usuário e, se bem-sucedido, o `AbpSignInManager` (que usa o `SignInManager` do ASP.NET Identity) para criar o cookie de autenticação.
*   **Token-based**: O `TokenAuthController` do ABP (`Abp.AspNetCore.Mvc.Controllers.TokenAuthController`) recebe as credenciais, usa o `AbpLogInManager` para validá-las, e então gera um JWT.

    Ambos os processos envolvem o `AbpLogInManager.LoginAsync(...)` que executa a lógica central de validação de credenciais, verifica status do usuário (ativo, email confirmado se necessário, lockout, etc.).

### 2.4. Processo de Logout

*   **Cookie-based**: Uma chamada para um endpoint de logout (e.g., `AccountController`'s `Logout` action) invoca `AbpSignInManager.SignOutAsync()` (ou `SignInManager.SignOutAsync()` do ASP.NET Identity), que limpa o cookie de autenticação do usuário.
*   **Token-based**: O logout é primariamente uma operação do lado do cliente, onde o JWT armazenado é descartado. Como JWTs são stateless, o token em si permanece válido até sua expiração. Para invalidação imediata crítica (e.g., comprometimento de conta), estratégias mais avançadas como uma blocklist de tokens no servidor podem ser implementadas, embora isso adicione complexidade.

## 3. Registro de Usuário

*   **Processo**: O EAF utiliza o `AbpUserManager` (que estende o `UserManager` do ASP.NET Identity) para o registro de novos usuários. O método `AbpUserManager.CreateAsync(User user)` é central.
*   **Auto-Registro**:
    *   O ABP Zero fornece funcionalidades para que usuários se registrem. Isso pode ser habilitado/desabilitado através das configurações do sistema (`ISettingManager`).
    *   `Abp.Zero.Configuration.IUserManagementConfig.IsSelfRegistrationEnabled`.
*   **Confirmação de Email**:
    *   O sistema pode ser configurado para exigir que os usuários confirmem seu endereço de e-mail após o registro.
    *   `AbpUserManager.GenerateEmailConfirmationTokenAsync(User user)` e `AbpUserManager.ConfirmEmailAsync(User user, string token)`.
    *   Configuração via `ISettingManager`: `Abp.Zero.UserManagement.IsEmailConfirmationRequiredForLogin`.
*   **Configurações Específicas do EAF**: O EAF pode adicionar validações customizadas ou passos adicionais ao fluxo de registro através da substituição de serviços ou eventos de domínio.

## 4. Autenticação de Dois Fatores (2FA/MFA)

O ABP Zero fornece suporte robusto para autenticação de dois fatores, que o EAF herda:

*   **Provedores Suportados**:
    *   **Aplicativos Autenticadores (TOTP)**: Como Google Authenticator, Microsoft Authenticator. O usuário escaneia um QR code e insere o código gerado.
    *   **Email**: Um código de segurança é enviado para o email registrado do usuário.
    *   **SMS**: Um código de segurança é enviado via SMS para o número de telefone registrado do usuário (requer integração com um provedor de SMS).
*   **Configuração**:
    *   Pode ser habilitada globalmente (`ISettingManager`: `Abp.Zero.UserManagement.TwoFactorLogin.IsEnabled`).
    *   Usuários podem habilitar e configurar 2FA individualmente em seus perfis, se permitido.
*   **Fluxo**: Após a validação das credenciais primárias (username/password), se 2FA estiver habilitado para o usuário, ele será solicitado a fornecer um código do seu segundo fator. O `AbpSignInManager` gerencia este fluxo.

## 5. Provedores de Autenticação Externa

O EAF, através do ASP.NET Core Identity e ABP, suporta a integração com provedores de identidade externos, permitindo que os usuários façam login com contas de outros sistemas.

*   **Conceito**: Delega o processo de autenticação a um provedor de identidade confiável (IdP) externo. Após a autenticação bem-sucedida no IdP, o usuário é redirecionado de volta ao EAF, que então cria uma sessão local.
*   **Extensões Específicas do EAF**: O EAF possui integrações detalhadas para vários provedores externos. Para configurações detalhadas, exemplos e troubleshooting, consulte `docs/architecture/eaf-extensions.md` nas seguintes seções:
    *   **Integração com Azure Active Directory (`Eaf.Middleware.AzureActiveDirectoryModule`)**: Para login com contas corporativas do Azure AD (OpenID Connect).
    *   **Integração com LDAP (`Eaf.Middleware.LdapModule`)**: Para autenticação contra diretórios LDAP/Active Directory on-premises.
    *   **Provedores de Login Externo baseados em Tenant**: Para cenários multi-tenant onde cada tenant pode configurar seu próprio IdP (OpenID Connect, WS-Federation, SAML2).
*   **Configuração Geral**: A configuração de provedores externos geralmente envolve registrar o manipulador de autenticação apropriado no `Startup.cs` (ou em um módulo EAF), fornecendo Client ID, Client Secret, Authority URL, e outros metadados do IdP. O EAF pode abstrair essa configuração através de seus módulos específicos.

## 6. Serviços e Configurações Chave

Vários serviços e configurações do ABP/ASP.NET Identity são centrais para a autenticação no EAF:

*   **Serviços**:
    *   `AbpUserManager<TRole, TUser>`: Gerencia usuários (criar, buscar, atualizar, gerenciar claims, papéis, tokens 2FA, etc.).
    *   `AbpRoleManager<TRole>`: Gerencia papéis e permissões de papéis.
    *   `AbpSignInManager<TTenant, TRole, TUser>`: Gerencia operações de login e logout (incluindo 2FA e logins externos).
    *   `AbpLogInManager<TTenant, TRole, TUser>`: Contém a lógica principal para tentar um login, verificando todas as condições (lockout, email confirmado, etc.).
    *   `Microsoft.AspNetCore.Identity.IPasswordHasher<TUser>`: Usado para hash e verificação de senhas de forma segura.
*   **Configurações Relevantes (geralmente em `appsettings.json` ou configuradas via `ISettingManager` e acessíveis no `PreInitialize` de um módulo)**:
    *   **Lockout de Usuário**:
        *   `Abp.Zero.UserManagement.UserLockOut.IsEnabled`
        *   `Abp.Zero.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout`
        *   `Abp.Zero.UserManagement.UserLockOut.DefaultAccountLockoutSeconds`
    *   **Configurações de Senha (ASP.NET Identity)**:
        *   `Abp.Identity.Password.RequiredLength`
        *   `Abp.Identity.Password.RequireDigit`
        *   `Abp.Identity.Password.RequireLowercase`
        *   `Abp.Identity.Password.RequireNonAlphanumeric`
        *   `Abp.Identity.Password.RequireUppercase`
    *   **Validação de Usuário**:
        *   `Abp.Zero.UserManagement.UserValidation.AllowOnlyAlphanumericUserNames`
        *   `Abp.Zero.UserManagement.UserValidation.IsEmailConfirmationRequiredForLogin`

## 7. Troubleshooting de Problemas Comuns de Autenticação

*   **Falhas de Login**:
    *   **Credenciais Inválidas**: Verifique se o username/email e senha estão corretos. Para logins externos, verifique a tabela `AbpUserLogins`.
    *   **Usuário Bloqueado (Locked Out)**: Se o usuário excedeu o número máximo de tentativas de login falhadas. Verifique `User.LockoutEndDateUtc`.
    *   **Usuário Não Ativo**: A propriedade `User.IsActive` pode estar `false`.
    *   **Email Não Confirmado**: Se `IsEmailConfirmationRequiredForLogin` estiver habilitado e `User.IsEmailConfirmed` for `false`.
    *   **Tenant Desabilitado ou Não Encontrado**: Em aplicações multi-tenant.
*   **Erros de Validação de Token JWT**:
    *   **Token Expirado**: O cliente precisa obter um novo token (e.g., usando um refresh token, se implementado).
    *   **Issuer/Audience/SecurityKey Incorretos**: A configuração do middleware JWT Bearer no servidor deve corresponder aos valores usados para gerar o token. Verifique `appsettings.json`.
    *   **Clock Skew**: Diferenças significativas de tempo entre o servidor de token e o servidor de API.
*   **Problemas com Cookies**:
    *   **Cookie Não Enviado ou Rejeitado**: Verifique as configurações de `SecurePolicy` (HTTPS), `SameSite`, domínio e caminho do cookie.
    *   **Loop de Redirecionamento**: Pode ocorrer se a página de login requer autenticação ou se há problemas com o esquema de autenticação padrão.
*   **Problemas com Provedores Externos**:
    *   **Configuração Incorreta**: Client ID, Client Secret, Redirect URI, Authority/Metadata URL devem estar corretos tanto no EAF quanto no provedor externo.
    *   **Permissões/Scopes**: O EAF pode não estar solicitando os scopes corretos, ou o usuário não consentiu as permissões necessárias no provedor externo.
    *   Consulte as seções de troubleshooting específicas em `docs/architecture/eaf-extensions.md` para Azure AD, LDAP, etc.
*   **Logs**: Verifique os logs da aplicação EAF (e do ABP) para mensagens de erro detalhadas relacionadas à autenticação. Aumentar o nível de log para Debug pode fornecer mais insights.

A autenticação é uma área complexa mas crítica. O EAF, ao construir sobre as fundações do ABP Zero e ASP.NET Identity, fornece um ponto de partida sólido e seguro, com amplas capacidades de extensão para atender a diversos requisitos corporativos.
