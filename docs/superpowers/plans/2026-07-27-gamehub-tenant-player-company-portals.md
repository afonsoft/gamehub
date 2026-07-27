# Plano: Tenant Player, Portal da Empresa e Chat/SDK Tenant-Aware

> Status: rascunho pendente de aprovação.  
> Baseado na simulação Docker do cadastro/login multi-tenant (PR #75).

## 1. Objetivo

1. Isolar todos os jogadores em um tenant chamado **Player** (default para jogadores).  
2. O chat dos jogos deve sempre operar no tenant **Player**, independente de qual empresa/tenant o usuário esteja administrando.  
3. Ajustar o SDK (`gameplay-bridge`) para enviar o `tenantId` correto em chat/friendship.  
4. Transferir a administração de empresas e funcionários para o **angular-admin** (host/admin).  
5. No portal público (**angular/hub**) manter apenas a visualização básica da empresa e o cadastro de funcionários/desenvolvedores.  
6. Melhorar layouts do admin e criar telas de controle da empresa (funcionários, convites, status).

---

## 2. Contexto da Simulação

Durante a simulação Docker foram identificados e corrigidos em PR #75:

- Migrations com nome de tabela errado (`GameplayEvents` vs `gh_GameplayEvents`) e colunas duplicadas.  
- `DateTimeKind.Local` no PostgreSQL `timestamp with time zone` resolvido com UTC clock.  
- `DbContextOptions<GameHubDbContext>` e `ITokenAuthenticationService` registrados no `Startup`.  
- `JwtTokenAuthenticationService` reescrito para emitir tokens EAF-compatíveis (`token_validity_key`, `token_validity_value`, `user_identifier`, `SecurityStamp`).  
- `RegistrationAppService` passou a criar usuários não-desenvolvedores no tenant `Default`.

O fluxo end-to-end validado:

```text
POST /api/services/app/Registration/Register -> player no tenant Default
POST /api/hub/auth/available-tenants      -> [{ tenantId: 1, tenantName: "Default" }]
POST /api/hub/auth/select-tenant          -> accessToken JWT
GET  /api/services/app/PlayerAccount/GetPlayerProfile (com token) -> { username: "..." }
```

Próximos gargalos detectados:

- O tenant padrão ainda se chama **Default**. O domínio de chat/SDK não distingue "tenant do jogo" de "tenant administrativo".  
- `angular-admin` não tem uma gestão de empresa (tenant) orientada a negócio (CNPJ, site, status, funcionários).  
- `UserTenantAssociationService` do `angular-admin` aponta para action names que não existem no backend (`GetAllByUser` vs `GetUserMemberships`, `AssociateUserToTenant` vs `Associate`, etc.).  
- `angular` não expõe info da empresa nem convite de funcionários no hub público.  
- `GameChatAppService` usa `AbpSession.TenantId` do token; para jogos isso deve ser fixo no tenant Player.

---

## 3. Arquitetura Proposta

```
┌─────────────────────────────────────────────────────────────────┐
│                         HUB PÚBLICO (angular)                   │
│  login/register -> select-tenant -> Player (default)              │
│  /company/:id -> info básica + convidar funcionário             │
│  /developer/* -> CRUD de jogos/builds do tenant selecionado     │
│  gameplay bridge -> tenantId do token Player para chat          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         ADMIN (angular-admin)                   │
│  /app/main/companies          -> listar empresas (tenants)      │
│  /app/main/companies/:id      -> editar empresa + funcionários  │
│  /app/main/employees          -> listar convites/pendentes      │
│  /app/admin/users             -> associar usuário a tenants      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         BACKEND (.NET)                          │
│  Tenant = Empresa (exceto tenant "Player")                       │
│  UserTenantMembership = funcionário vinculado à empresa          │
│  DeveloperTeam = perfil da empresa dentro do tenant              │
│  GameChatAppService sempre usa tenant "Player"                   │
│  GameTokenProvider emite token com tenant do player             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. Fases de Implementação

### Fase 1 — Domínio, Seed e Tenant Player

**Objetivo**: garantir que exista um tenant `Player` e que todos os cadastros públicos caiam nele.

| Symbol | O que muda |
|--------|------------|
| `GameHubConsts` (Core) | Adicionar `public const string PlayerTenantName = "Player";` |
| `InitialHostDbBuilder` / `SeedHelper` (EF Migrations) | Criar/renomear seed do tenant padrão para `TenancyName = "Player"`, `Name = "Player"`. Se já existir `Default`, renomear via data seed no startup. |
| `RegistrationAppService.RegisterAsync` | Buscar `tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == GameHubConsts.PlayerTenantName)` em vez de `AbpTenantBase.DefaultTenantName`. Se não existir, criar via `TenantManager`. |
| `HubAuthController` | Ajustar `select-tenant` para retornar `tenantId` correto; garantir que tokens de player apontem para tenant `Player`. |

**Seed efetivo esperado**:

```sql
INSERT INTO "AbpTenants" ("TenancyName", "Name", "ConnectionString", "IsDeleted")
VALUES ('Player', 'Player', NULL, FALSE);
```

---

### Fase 2 — APIs de Empresa e Funcionários

**Objetivo**: centralizar no admin a criação de empresas (tenants) e o vínculo de funcionários.

#### 2.1 DTOs (Application.Contracts ou Application)

```csharp
public class CompanyDto
{
    public int Id { get; set; }
    public string TenancyName { get; set; }
    public string Name { get; set; }
    public string PrimaryContactEmail { get; set; }
    public string Country { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateOrUpdateCompanyInput
{
    [Required] [StringLength(128)] public string TenancyName { get; set; }
    [Required] [StringLength(128)] public string Name { get; set; }
    [Required] [StringLength(256)] public string PrimaryContactEmail { get; set; }
    [StringLength(128)] public string Country { get; set; }
}

public class CompanyEmployeeDto
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public string Role { get; set; }   // "Admin", "Developer", "Tester"
    public bool IsDefault { get; set; }
    public DateTime? JoinedAt { get; set; }
}

public class InviteEmployeeInput
{
    [Required] public int TenantId { get; set; }
    [Required] [StringLength(256)] public string EmailOrUserName { get; set; }
    [Required] public string Role { get; set; }
    public bool IsDefault { get; set; }
}

public class RemoveEmployeeInput
{
    public int TenantId { get; set; }
    public long UserId { get; set; }
}
```

#### 2.2 App Services e Domain Services

```csharp
// Api/src/GameHub.Application/Companies/ICompanyAppService.cs
public interface ICompanyAppService : IApplicationService
{
    Task<PagedResultDto<CompanyDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<CompanyDto> GetAsync(int id);
    Task<CompanyDto> CreateAsync(CreateOrUpdateCompanyInput input);
    Task<CompanyDto> UpdateAsync(int id, CreateOrUpdateCompanyInput input);
    Task DeleteAsync(int id);
    Task<CompanyDto> GetByTenancyNameAsync(string tenancyName);
}

// Api/src/GameHub.Application/Companies/CompanyAppService.cs
[AbpAuthorize(GameHubPermissions.Pages_Companies)]
public class CompanyAppService : GameHubAppServiceBase, ICompanyAppService
{
    private readonly TenantManager _tenantManager;
    private readonly IRepository<Tenant, int> _tenantRepository;
    private readonly IRepository<DeveloperTeam, Guid> _teamRepository;
    private readonly ITenantUserManager _tenantUserManager;

    public async Task<CompanyDto> CreateAsync(CreateOrUpdateCompanyInput input)
    {
        // 1. Criar Tenant via TenantManager.CreateAsync.
        // 2. Criar DeveloperTeam { TenantId = tenant.Id, Name = input.Name, PrimaryContactEmail = input.PrimaryContactEmail, Country = input.Country }.
        // 3. Mapear para CompanyDto.
    }
}

// Api/src/GameHub.Application/Companies/ICompanyEmployeeAppService.cs
public interface ICompanyEmployeeAppService : IApplicationService
{
    Task<List<CompanyEmployeeDto>> GetEmployeesAsync(int tenantId);
    Task<CompanyEmployeeDto> InviteAsync(InviteEmployeeInput input);
    Task RemoveAsync(RemoveEmployeeInput input);
    Task SetDefaultAsync(int tenantId, long userId);
}

// Api/src/GameHub.Application/Companies/CompanyEmployeeAppService.cs
[AbpAuthorize(GameHubPermissions.Pages_Company_Employees_Manage)]
public class CompanyEmployeeAppService : GameHubAppServiceBase, ICompanyEmployeeAppService
{
    private readonly ITenantUserManager _tenantUserManager;
    private readonly IRepository<UserTenantMembership, long> _membershipRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly UserManager _userManager;

    public async Task<CompanyEmployeeDto> InviteAsync(InviteEmployeeInput input)
    {
        // 1. Localizar usuário host por e-mail/username.
        // 2. Se não existir, criar usuário host temporário (convite) ou retornar erro solicitando cadastro.
        // 3. _tenantUserManager.EnsureMembershipAsync(hostUser.Id, input.TenantId, input.IsDefault).
        // 4. Atribuir role no shadow user (Admin/Developer/Tester).
        // 5. Criar DeveloperTeamMember { TenantId = input.TenantId, TeamId = team.Id, UserId = hostUser.Id, Role = input.Role }.
        // 6. Retornar CompanyEmployeeDto.
    }
}
```

#### 2.3 Permissionamentos

```csharp
// Api/src/GameHub.Core/Authorization/GameHubPermissions.cs
public const string Pages_Companies = "Pages.Companies";
public const string Pages_Companies_Manage = "Pages.Companies.Manage";
public const string Pages_Company_Employees = "Pages.Company.Employees";
public const string Pages_Company_Employees_Manage = "Pages.Company.Employees.Manage";
```

Seed em `GameHubPermissionSeeder`:
- `AdminPermissions` e `HostAdminPermissions` recebem `Pages_Companies*`.
- Criar role `CompanyAdmin` (per tenant) com `Pages_Developer_Profile`, `Pages_Developer_Games`, `Pages_Company_Employees_Manage`.

---

### Fase 3 — Chat e SDK Tenant-Aware

**Objetivo**: garantir que o chat dos jogos use sempre o tenant `Player`.

#### 3.1 Backend

```csharp
// Api/src/GameHub.Application/Chat/GameChatAppService.cs
public async Task<GameChatMessageResult> SendAsync(SendGameChatMessageInput input)
{
    var playerTenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == GameHubConsts.PlayerTenantName);
    if (playerTenant == null) throw new InvalidOperationException("Player tenant not found.");

    var sender = await ResolvePlayerUserAsync(playerTenant.Id, AbpSession.UserId.Value);
    var senderIdentifier = new UserIdentifier(playerTenant.Id, sender.Id);

    // usar playerTenant.Id em vez de AbpSession.TenantId para envio de mensagens
}
```

Ajustar `Conversation` parsing para aceitar `user:<playerTenantId>:<userId>`.

#### 3.2 Frontend (gameplay-bridge)

```typescript
// angular/src/app/core/services/gameplay-bridge.service.ts
private get playerTenantId(): number {
  return this.tokenService.getTenantId() ?? 0;
}

async blockPlayer(requestId, userId, tenantId?) {
  tenantId = tenantId ?? this.playerTenantId;
  ...
}

private parseConversationId(conversationId: string): { tenantId?: number; userId?: number; groupId?: number } {
  // já existe; verificar se o tenantId extraído é o Player, senão fallback
}
```

#### 3.3 GameTokenProvider

```csharp
// Api/src/GameHub.Web.Host/Security/GameTokenProvider.cs
// Ao emitir token de jogo, garantir que o claim "tenantid" seja o tenant Player do usuário,
// e não o tenant administrativo atual.
```

---

### Fase 4 — angular-admin: Gestão de Empresas e Funcionários

**Objetivo**: novas telas e ajuste de rotas existentes.

#### 4.1 Novas rotas

```typescript
// angular-admin/GameHub.UI/src/app/main/gamehub/companies/companies.routes.ts
export const companyRoutes: Routes = [
  { path: '', component: CompanyListComponent },
  { path: 'create', component: CompanyCreateComponent },
  { path: ':id', component: CompanyDetailComponent },
  { path: ':id/edit', component: CompanyEditComponent },
  { path: ':id/employees', component: CompanyEmployeesComponent },
];

// Incluir em main-routing.module.ts ou gamehub-routing.module.ts
```

#### 4.2 Componentes e serviços a criar

```typescript
// src/app/main/gamehub/companies/company-list.component.ts
// src/app/main/gamehub/companies/company-create.component.ts
// src/app/main/gamehub/companies/company-detail.component.ts
// src/app/main/gamehub/companies/company-edit.component.ts
// src/app/main/gamehub/companies/company-employees.component.ts
// src/app/main/gamehub/companies/company.service.ts
```

```typescript
// company.service.ts
@Injectable()
export class CompanyService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/Company`;

  getAll(...): Observable<PagedResultDto<CompanyDto>> { ... }
  get(id: number): Observable<CompanyDto> { ... }
  create(input: CreateOrUpdateCompanyInput): Observable<CompanyDto> { ... }
  update(id: number, input: CreateOrUpdateCompanyInput): Observable<CompanyDto> { ... }
  delete(id: number): Observable<void> { ... }
  getEmployees(tenantId: number): Observable<CompanyEmployeeDto[]> { ... }
  invite(input: InviteEmployeeInput): Observable<CompanyEmployeeDto> { ... }
  remove(tenantId: number, userId: number): Observable<void> { ... }
}
```

#### 4.3 Menu

```typescript
// src/app/shared/layout/nav/app-navigation.service.ts
new AppMenuItem('Companies', 'Pages.Companies', 'la la-building', '/app/main/companies'),
new AppMenuItem('Employees', 'Pages.Company.Employees', 'la la-user-tie', '/app/main/employees'),
```

#### 4.4 Correção do modal de memberships

```typescript
// src/app/admin/users/user-tenant-association.service.ts
getByUser(userId: number) {
  return this.http.post<UserTenantMembershipDto[]>(`${this.baseUrl}/GetUserMemberships`, { userId });
}

associate(input) {
  return this.http.post<void>(`${this.baseUrl}/Associate`, input);
}

remove(input) {
  return this.http.post<void>(`${this.baseUrl}/RemoveAssociation`, input);
}

setDefault(input) {
  return this.http.post<void>(`${this.baseUrl}/SetDefault`, input);
}
```

#### 4.5 Layouts

- `company-list.component.html`: padrão `card` + `p-table` com paginação, colunas: Nome, TenancyName, E-mail, País, Ativo, Funcionários, Ações.
- `company-employees.component.html`: tabela de funcionários com role, status convite, default, ações (remover, set default).
- Ajustar `users.component.html` e `tenants.component.html` para seguir o mesmo padrão de cards/tabelas (continuação do trabalho anterior).

---

### Fase 5 — angular (Hub): Empresa e Convite de Funcionários

**Objetivo**: informação básica da empresa e cadastro de funcionários/desenvolvedores no portal público.

#### 5.1 Novas rotas

```typescript
// angular/src/app/public/public.routes.ts
{ path: 'company/:tenancyName', component: CompanyPublicComponent }

// angular/src/app/developer/developer.routes.ts
{ path: 'team', loadComponent: () => import('./team/team.component') },
// (já existe; integrar com backend real)
```

#### 5.2 Componentes e serviços

```typescript
// angular/src/app/public/company/company-public.component.ts
// Exibe nome, e-mail de contato, país, lista de jogos públicos, botão "Trabalhe aqui"/"Cadastrar como desenvolvedor".

// angular/src/app/core/services/company.service.ts
@Injectable({ providedIn: 'root' })
export class CompanyService {
  getPublicByTenancyName(tenancyName: string): Observable<PublicCompanyDto> { ... }
  inviteEmployee(tenantId: number, email: string, role: string): Observable<unknown> { ... }
}
```

#### 5.3 Tela de convite

No `developer/team` ou novo `company/invite`:

```html
<h2>Convidar desenvolvedor</h2>
<form>
  <input type="email" [(ngModel)]="model.email" placeholder="E-mail" />
  <select [(ngModel)]="model.role">
    <option value="Developer">Desenvolvedor</option>
    <option value="Tester">Testador</option>
    <option value="Admin">Administrador</option>
  </select>
  <button (click)="invite()">Enviar convite</button>
</form>
```

---

### Fase 6 — Testes e Documentação

#### 6.1 Backend

- `CompanyAppService_Tests`: CRUD, duplicação de tenancyName, associação automática de `DeveloperTeam`.
- `CompanyEmployeeAppService_Tests`: convite, remoção, role shadow, default.
- `GameChatAppService_Tests`: envio sempre no tenant Player; partidas e conversas 1-1.
- `GameTokenProvider_Tests`: token de jogo com tenant Player.
- `HubAuthController_Tests`: player login retorna tenant Player.

#### 6.2 Frontend

- `angular-admin`: specs de `CompanyService`, `CompanyListComponent`, `CompanyEmployeesComponent`.
- `angular`: specs de `CompanyService`, `CompanyPublicComponent`, convite.

#### 6.3 Documentação

- Atualizar `docs/agent-execution-log.md` com resultados da implementação.
- Criar `docs/tenancy-model.md` explicando Player vs Company tenants, shadow users e chat.
- Atualizar `README.pt-BR.md` e `README.md` seção multi-tenancy.

---

## 5. Símbolos Modificados (Resumo)

| Camada | Symbol | Ação |
|--------|--------|------|
| Core | `GameHubConsts` | Adicionar `PlayerTenantName` |
| Core | `GameHubPermissions` | Adicionar `Pages_Companies*`, `Pages_Company_Employees*` |
| Core | `DeveloperTeam` | Manter vínculo com `TenantId` |
| Application | `RegistrationAppService` | Usar `PlayerTenantName` |
| Application | `ICompanyAppService` / `CompanyAppService` | Novo |
| Application | `ICompanyEmployeeAppService` / `CompanyEmployeeAppService` | Novo |
| Application | `IUserTenantAssociationAppService` | (opcional) renomear actions para evitar incompatibilidade |
| Application | `GameChatAppService` | Forçar tenant Player |
| Web.Host | `HubAuthController` | Validar tenant Player no registro |
| Web.Host | `GameTokenProvider` | Token com tenant Player |
| Web.Host | `JwtTokenAuthenticationService` | Continuar compatível EAF (já feito em PR #75) |
| EF | `InitialHostDbBuilder` / `SeedHelper` | Seed tenant Player |
| angular-admin | `app-navigation.service.ts` | Menu Companies / Employees |
| angular-admin | `user-tenant-association.service.ts` | Corrigir action names |
| angular-admin | `Company*Component`, `company.service.ts` | Novos |
| angular | `CompanyService`, `CompanyPublicComponent` | Novos |
| angular | `gameplay-bridge.service.ts` | Fallback tenant Player |
| angular | `token.service.ts` | Expor `getTenantId()` |

---

## 6. Critérios de Aceitação

- [ ] Seed cria tenant `Player` automaticamente; cadastro público gera usuário nesse tenant.  
- [ ] Admin pode criar empresa (tenant) e convidar funcionários; funcionários recebem shadow user + role.  
- [ ] `angular-admin` exibe lista de empresas, detalhe e funcionários; layouts seguem padrão card + `p-table`.  
- [ ] `angular` exibe página pública `/company/:tenancyName` e permite convite de funcionário logado.  
- [ ] Chat de jogos (`GameChatAppService`) processa mensagens no tenant `Player` e tokens de jogo carregam esse tenant.  
- [ ] `dotnet test` passa; `npm run build` e `npm test` passam para `angular` e `angular-admin`.  
- [ ] Docker Compose sobe com infra completa e simulação de empresa + funcionário funciona.

---

## 7. Próximos Passos Imediatos (após aprovação)

1. Criar branch `feature/tenant-player-company`.  
2. Implementar Fase 1 (seed + `RegistrationAppService`).  
3. Rodar `dotnet test` e `docker compose up` para validar registro no tenant Player.  
4. Prosseguir para Fase 2 (APIs empresa/funcionários) e Fase 3 (chat/SDK).  
5. Finalizar frontends e testes.
