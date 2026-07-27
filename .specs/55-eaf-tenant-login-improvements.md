# EAF — Melhorias em Multi-Tenancy, Login Angular/API e SDK

> Spec para uma sessão futura no repositório `afonsoft/EAF` (Enterprise Application Foundation).  
> O objetivo é tornar o EAF nativamente capaz de lidar com o mesmo cenário de multi-tenancy implementado no GameHub: **um usuário host pertence a vários tenants**, faz login no host, escolhe o tenant e recebe um JWT scopado, enquanto o chat/SDK sempre operam no tenant `Player`.

---

## 1. Contexto

No GameHub o cenário exigiu:

- Criar um tenant `Player` padrão para jogadores.
- Criar um tenant para cada empresa desenvolvedora.
- Permitir que um mesmo usuário host seja membro de vários tenants (shadow users + `UserTenantMembership`).
- Fazer login em duas etapas: autenticar host → listar tenants → selecionar tenant → obter token escopado.
- Manter o chat sempre no tenant `Player`, mesmo que o jogo pertença a outro tenant.
- Replicar permissões/roles de host para novos tenants automaticamente.

Essas alterações hoje estão toda em código do GameHub. Esta spec propõe trazer a infraestrutura genérica para o EAF, de forma que qualquer projeto gerado pelo template EAF já tenha o recurso.

---

## 2. Escopo

- **Backend (EAF Core / Application / Web.Core)**
  - Entidade `UserTenantMembership`.
  - `ITenantUserManager` e `TenantUserManager` no domínio.
  - Controle correto do filtro `MayHaveTenant` (`SetTenantId` não reabilita o filtro automaticamente).
  - Replicação de roles/permissões entre host e tenant.
  - Endpoints `available-tenants` e `select-tenant` no `TokenAuthController` (ou novo `MultiTenantAuthController`).
  - Suporte a shadow users no `LogInManager`/`UserManager`.

- **Frontend (Template Angular EAF)**
  - Tela de login em duas etapas.
  - Componente `select-tenant`.
  - `login.service.ts` e `eaf-auth.service.ts` ajustados para trocar o token após seleção.
  - Guarda de rotas e interceptors propagando `Abp.TenantId`/`Authorization`.

- **SDK / Bridge**
  - `GameplayBridgeService` enviando `tenantId` nas mensagens.
  - `HubAuthService` chamando os novos endpoints EAF.

---

## 3. Backend

### 3.1 Entidades e contratos (Eaf.Middleware.Core)

```csharp
// Eaf.Middleware.MultiTenancy
public class UserTenantMembership : CreationAuditedEntity<long>
{
    [Required]
    public virtual long UserId { get; set; }          // host user id

    [Required]
    public virtual int TenantId { get; set; }          // tenant escolhido

    [Required]
    public virtual long TenantUserId { get; set; }      // shadow user id dentro do tenant

    public virtual bool IsDefault { get; set; }         // usado no login automático
}

public interface ITenantUserManager : IDomainService
{
    Task<UserTenantMembership> EnsureMembershipAsync(long hostUserId, int tenantId, bool isDefault = false);
    Task RemoveMembershipAsync(long hostUserId, int tenantId);
    Task SetDefaultAsync(long hostUserId, int tenantId);
    Task<long?> GetTenantUserIdAsync(long hostUserId, int tenantId);
}
```

### 3.2 `TenantUserManager` (domínio)

Regras de implementação:

- **Host users only**: apenas usuários com `TenantId == null` podem ter memberships.
- **Filtro `MayHaveTenant`**: a busca pelo host user deve desabilitar o filtro apenas no escopo mínimo:
  ```csharp
  User hostUser;
  using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
  {
      hostUser = await _userRepository.GetAsync(hostUserId);
  }
  ```
- **Criação do shadow user**: ao entrar no tenant, o `MayHaveTenant` deve estar **habilitado** e com `tenantId` setado:
  ```csharp
  using (CurrentUnitOfWork.SetTenantId(tenantId, switchMustHaveTenantEnableDisable: false))
  using (CurrentUnitOfWork.EnableFilter(AbpDataFilters.MayHaveTenant))
  {
      // cria ou atualiza shadow user e replica roles
  }
  ```
- **Replicação de roles**: quando o host user possuir roles (`Player`, `Developer`, `Admin`), replicar as permissões para o tenant de destino.  
  Se a role não existir no tenant, criar e copiar as permissões do host:
  ```csharp
  var hostRoles = await _userManager.GetRolesAsync(hostUser); // precisa ser obtido com filtro desabilitado
  foreach (var roleName in hostRoles)
  {
      await EnsureRoleInTenantAsync(tenantId, roleName);
      await _userManager.AddToRoleAsync(shadowUser, roleName);
  }
  ```

> **Nota**: no EAF atual `SetTenantId` apenas seta o parâmetro do filtro; se o filtro foi desabilitado anteriormente, ele **não é reabilitado**. O `TenantUserManager` deve explicitamente chamar `EnableFilter(AbpDataFilters.MayHaveTenant)`.

### 3.3 `TenantRolePermissionReplicationService` (domínio / aplicação)

```csharp
public interface ITenantRolePermissionReplicationService : IDomainService
{
    Task EnsureRoleInTenantAsync(int tenantId, string roleName, IEnumerable<string> permissionNames);
    Task CopyRolePermissionsFromHostAsync(int tenantId, string roleName);
}
```

- Usar `RoleManager` + `PermissionManager` dentro de `SetTenantId(tenantId)`.
- Nunca executar fora de `SetTenantId` / `EnableFilter` combinado.
- Invalidar o cache de permissões após alteração (`PermissionManager` / `RoleManager` expõe cache? Verificar; se não, usar `IRepository<RolePermissionSetting>` e invalidar `ICacheManager` `RolePermissionCache`).

### 3.4 API — `TokenAuthController` (Eaf.Middleware.Web.Core)

Adicionar endpoints (ou criar `MultiTenantAuthController`):

```csharp
[AbpAllowAnonymous]
[HttpPost]
public virtual async Task<IActionResult> GetAvailableTenants([FromBody] AvailableTenantsModel model)
{
    // 1. autentica host user via _logInManager com tenancyName = null
    // 2. lista memberships de UserTenantMembership
    // 3. retorna [{tenantId, tenantName, tenancyName, isDefault}]
}

[AbpAllowAnonymous]
[HttpPost]
public virtual async Task<IActionResult> SelectTenant([FromBody] SelectTenantModel model)
{
    // 1. autentica host user
    // 2. verifica membership para model.TenantId
    // 3. cria claims com tenantId e shadow user id
    // 4. retorna accessToken/refreshToken EAF padrão
}
```

Modelos:

```csharp
public class AvailableTenantsModel
{
    public string UserNameOrEmailAddress { get; set; }
    public string Password { get; set; }
}

public class SelectTenantModel : AvailableTenantsModel
{
    public int TenantId { get; set; }
}

public class AvailableTenantResult
{
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string TenancyName { get; set; }
    public bool IsDefault { get; set; }
}
```

### 3.5 `LogInManager` e `UserManager`

- `LogInManager.LoginAsync(userName, password, tenancyName: null)` deve autenticar o usuário no host, mesmo quando `MultiTenancy.IsEnabled`.
- `UserManager.GetUserByIdAsync` e `FindByNameAsync` devem respeitar o filtro `MayHaveTenant` corrente (já fazem, mas documentar que `SetTenantId` não basta se o filtro estiver desabilitado).

---

## 4. Frontend (Template Angular EAF)

### 4.1 Fluxo de login

1. Usuário informa usuário/senha.
2. Chamar `TokenAuth/GetAvailableTenants` (host).
3. Se retornar 1 tenant e `autoSelectSingleTenant` → chamar `SelectTenant` direto.
4. Se retornar > 1 → mostrar tela de seleção.
5. Armazenar `tenantId` no cookie `Abp.TenantId` e o `accessToken` do tenant.

### 4.2 Componentes / serviços

#### `login.service.ts`

```typescript
export class LoginService {
  authenticate(callback?: (success: boolean) => void): void;
  availableTenants(model: AvailableTenantsModel): Observable<AvailableTenantResult[]>;
  selectTenant(model: SelectTenantModel): Observable<AuthenticateResultModel>;
  loginTenant(result: AuthenticateResultModel, tenantId: number, callback?: () => void): void;
}
```

#### `select-tenant.component.ts` (novo)

```typescript
@Component({ ... })
export class SelectTenantComponent {
  tenants: AvailableTenantResult[] = [];
  selectedTenant: AvailableTenantResult;

  select(tenant: AvailableTenantResult): void {
    this.loginService.selectTenant({ ...this.credentials, tenantId: tenant.tenantId })
      .subscribe(result => this.loginService.loginTenant(result, tenant.tenantId));
  }
}
```

#### `login.component.ts`

```typescript
login(): void {
  this.submitting = true;
  this.loginService.availableTenants(this.authenticateModel)
    .subscribe(tenants => {
      this.submitting = false;
      if (tenants.length === 1 && AppConsts.autoSelectSingleTenant) {
        this.selectTenant(tenants[0]);
      } else {
        this.tenants = tenants;
        this.showTenantSelection = true;
      }
    }, () => this.submitting = false);
}
```

### 4.3 `eaf-auth.service.ts`

- `setToken` deve aceitar `tenantId` e limpar cache de permissões/tenant ao trocar.
- `logout` deve limpar token e tenant.

### 4.4 Interceptor HTTP

- Adicionar `Abp.TenantId` header quando houver tenant selecionado.
- Continuar enviando `Authorization: Bearer <token>`.

---

## 5. SDK / Bridge

### 5.1 `GameplayBridgeService`

- Incluir `tenantId` no payload de inicialização (`gamehub:init`).
- Encaminhar `tenantId` em eventos que precisam de contexto backend (chat, leaderboard, cloud save).

### 5.2 `HubAuthService`

```typescript
export class HubAuthService {
  availableTenants(credentials: LoginModel): Observable<AvailableTenantResult[]>;
  selectTenant(credentials: LoginModel, tenantId: number): Observable<AuthResult>;
}
```

---

## 6. Testes

- `TenantUserManager_Tests`: criação de membership, prevenção de usuário de tenant, replicação de roles.
- `MultiTenantAuthController_Tests`: `available-tenants` e `select-tenant` com credenciais corretas/incorretas.
- `RolePermissionReplicationService_Tests`: role criada em tenant com permissões copiadas do host.
- Angular specs para `SelectTenantComponent` e `LoginService`.

---

## 7. Migrações e seed

- Criar migration `AddUserTenantMembership` em `Eaf.Middleware.EntityFrameworkCore`.
- Adicionar seed no `HostRoleAndUserCreator` / `TenantRoleAndUserBuilder` para criar tenant `Player` quando `GameHubConsts.PlayerTenantName` (ou configuração genérica `EafSettings.MultiTenancy.PlayerTenantName`) estiver configurado.

---

## 8. Documentação

- `docs/eaf-multi-tenant-login.md` explicando o fluxo de login em duas etapas.
- `docs/eaf-tenant-user-manager.md` detalhando o uso do `MayHaveTenant` filter e shadow users.
- Atualizar template Angular `README.md` com as novas telas.

---

## 9. Símbolos a modificar

| Camada | Arquivo | Símbolo | Alteração |
|---|---|---|---|
| Core | `Authorization/Users/UserTenantMembership.cs` | `UserTenantMembership` | nova entidade |
| Core | `MultiTenancy/ITenantUserManager.cs` | `ITenantUserManager` | novo contrato |
| Core | `MultiTenancy/TenantUserManager.cs` | `TenantUserManager` | implementação |
| Core | `MultiTenancy/ITenantRolePermissionReplicationService.cs` | `ITenantRolePermissionReplicationService` | novo contrato |
| Core | `MultiTenancy/TenantRolePermissionReplicationService.cs` | `TenantRolePermissionReplicationService` | implementação |
| Web.Core | `Controllers/TokenAuthController.cs` | `GetAvailableTenants`, `SelectTenant` | novos endpoints |
| Web.Core | `Models/TokenAuth/AvailableTenantsModel.cs` | `AvailableTenantsModel` | novo DTO |
| Web.Core | `Models/TokenAuth/SelectTenantModel.cs` | `SelectTenantModel` | novo DTO |
| Web.Core | `Models/TokenAuth/AvailableTenantResult.cs` | `AvailableTenantResult` | novo DTO |
| Angular | `account/login/login.component.ts` | `LoginComponent` | fluxo de duas etapas |
| Angular | `account/login/login.service.ts` | `LoginService` | `availableTenants`, `selectTenant` |
| Angular | `account/select-tenant/select-tenant.component.ts` | `SelectTenantComponent` | novo componente |
| Angular | `shared/auth/eaf-auth.service.ts` | `EafAuthService` | trocar token/tenant |
| SDK | `shared/services/hub-auth.service.ts` | `HubAuthService` | usar novos endpoints |
| SDK | `shared/services/gameplay-bridge.service.ts` | `GameplayBridgeService` | enviar `tenantId` |

---

## 10. Definition of Done

- `dotnet test Eaf.sln` passando.
- `ng build --configuration=production` e `ng test --no-watch` do template Angular passando.
- Docker compose do template EAF subindo com Postgres/Redis.
- Simulação: registrar player no tenant `Player`, criar empresa, convidar funcionário, logar no tenant empresa, criar draft de jogo e enviar mensagem de chat no tenant `Player`.

---

## Referências

- Documentação ABP Multi-Tenancy: https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy
- Implementação de referência no GameHub: `Api/src/GameHub.Core/Domain/MultiTenancy/TenantUserManager.cs`, `Api/src/GameHub.Web.Host/Controllers/HubAuthController.cs`, `angular/src/account/login/login.component.ts`.
