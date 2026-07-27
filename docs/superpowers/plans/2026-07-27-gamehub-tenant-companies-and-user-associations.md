# GameHub — Empresas (Tenants) e Associação Multi-Usuário

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir que empresas desenvolvedoras de jogos sejam gerenciadas como tenants ABP, com um tenant padrão (`Default`), usuários associados a múltiplos tenants, login no portal público (`angular`) com seleção de tenant e acesso a jogos/dashboards filtrados pelo tenant selecionado.

**Architecture:**
- Reutiliza o mecanismo de multi-tenancy do **ASP.NET Boilerplate (ABP Zero)** já habilitado em `GameHubConsts.MultiTenancyEnabled` e entidades `IMayHaveTenant` do GameHub.
- Introduz `UserTenantMembership` (domínio) para associar um `User` "host" (`TenantId` nulo) a um ou mais `Tenant`s. A associação contém o `TenantUserId` — o `Id` do `User` real dentro daquele tenant.
- Ao associar um usuário a um tenant, o sistema cria/atualiza um `User` *dentro* do tenant, com a mesma senha e dados essenciais. Dessa forma permissões, roles e filtros de tenant funcionam naturalmente no ABP.
- Login do hub em duas etapas: `POST /api/hub/auth/available-tenants` autentica o usuário host e retorna os tenants disponíveis; `POST /api/hub/auth/select-tenant` autentica o `User` do tenant escolhido e emite o JWT com o `TenantId` correto, que o ABP usa para filtrar dados.
- O gerenciamento de empresas (tenants) e associações de usuários é feito no `angular-admin`. A seleção de tenant é feita no `angular` público, logo após o login.

**Tech Stack:** .NET 10, ASP.NET Boilerplate (ABP Zero), EAF 9.3.1, EF Core, PostgreSQL, Angular 20, xUnit, Shouldly, NSubstitute.

**Branches:** Criar a partir de `develop`/`main` atual: `feature/tenant-companies-and-user-associations`.

---

## Contexto e premissas

1. O `GameHub` já usa `AbpZeroDbContext<Tenant, Role, User, GameHubDbContext>` (ver `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs:34`).
2. A entidade `Tenant` (`Eaf.Middleware.MultiTenancy.Tenant`) e `User` (`Eaf.Middleware.Authorization.Users.User`) vêm do EAF; não devem ser editadas diretamente, apenas estendidas.
3. `GameHubConsts.MultiTenancyEnabled` já é `true` (`Api/src/GameHub.Core/GameHubConsts.cs:34`).
4. Entidades de negócio (`Game`, `GameBuild`, `PlayerRating`, etc.) já implementam `IMayHaveTenant` e portanto já separam dados por tenant automaticamente quando o `IAbpSession.TenantId` está definido.
5. O `angular-admin` já possui CRUD de tenants (`/app/admin/tenants`) via `TenantServiceProxy`. O objetivo aqui é estender esse gerenciamento com associação de usuários e garantir seed do tenant `Default`.
6. O `angular` público usa autenticação simples em `AuthService` (`/api/TokenAuth/Authenticate`) sem seleção de tenant. O fluxo será alterado para duas etapas.

---

## Task 1: Domínio — `UserTenantMembership` e repositório

**Files:**
- Create: `Api/src/GameHub.Core/Domain/MultiTenancy/UserTenantMembership.cs`
- Create: `Api/src/GameHub.Core/Domain/MultiTenancy/IUserTenantMembershipRepository.cs`
- Modify: `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`

**Step 1: Create `UserTenantMembership` entity**

```csharp
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameHub.Domain.MultiTenancy
{
    /// <summary>
    /// Association between a host-level user and one of the tenants they can access.
    /// The <see cref="TenantUserId"/> holds the real Abp User Id inside the tenant,
    /// enabling per-tenant permissions and data filtering.
    /// </summary>
    [Table("GameHubUserTenantMemberships")]
    public class UserTenantMembership : CreationAuditedEntity<long>, IMustHaveTenant
    {
        [Required]
        public virtual long UserId { get; set; }

        [Required]
        public virtual int TenantId { get; set; }

        /// <summary>
        /// The Id of the shadow <see cref="Eaf.Middleware.Authorization.Users.User"/>
        /// created inside <see cref="TenantId"/> for this membership.
        /// </summary>
        [Required]
        public virtual long TenantUserId { get; set; }

        public virtual bool IsDefault { get; set; }
    }
}
```

**Step 2: Create repository interface**

```csharp
using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameHub.Domain.MultiTenancy
{
    public interface IUserTenantMembershipRepository : IRepository<UserTenantMembership, long>
    {
        Task<UserTenantMembership> GetByUserAndTenantAsync(long userId, int tenantId);
        Task<List<UserTenantMembership>> GetAllByUserAsync(long userId);
        Task<UserTenantMembership> GetDefaultByUserAsync(long userId);
        Task<bool> ExistsAsync(long userId, int tenantId);
    }
}
```

**Step 3: Add `DbSet` and configure in `GameHubDbContext`**

In `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs` add property:

```csharp
public virtual DbSet<UserTenantMembership> UserTenantMemberships { get; set; }
```

In `OnModelCreating` (or `GameHubModelCreatingExtensions`), add:

```csharp
modelBuilder.Entity<UserTenantMembership>(b =>
{
    b.ToTable("GameHubUserTenantMemberships");
    b.HasIndex(x => new { x.UserId, x.TenantId }).IsUnique();
    b.HasIndex(x => new { x.UserId, x.IsDefault });
    b.HasIndex(x => x.TenantUserId);
});
```

**Test:**
- Create `Api/test/GameHub.Tests/Domain/UserTenantMembership_Tests.cs` asserting uniqueness and default membership.

Run:
```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~UserTenantMembership_Tests" -c Release
```
Expected: PASS.

---

## Task 2: Domínio — `TenantUserManager` (sincronização de usuários por tenant)

**Files:**
- Create: `Api/src/GameHub.Core/Domain/MultiTenancy/TenantUserManager.cs`
- Create: `Api/src/GameHub.Core/Domain/MultiTenancy/ITenantUserManager.cs`

**Goal:** Quando um host user é associado a um tenant, cria/atualiza um `User` real dentro daquele tenant e mantém `TenantUserId` na associação.

**Step 1: Define domain service interface**

```csharp
using Abp.Domain.Services;
using System.Threading.Tasks;

namespace GameHub.Domain.MultiTenancy
{
    public interface ITenantUserManager : IDomainService
    {
        /// <summary>
        /// Ensures a shadow user exists inside the target tenant for the given host user,
        /// creates the membership record and returns the tenant-level User Id.
        /// </summary>
        Task<UserTenantMembership> EnsureMembershipAsync(long hostUserId, int tenantId, bool isDefault = false);

        Task RemoveMembershipAsync(long hostUserId, int tenantId);
    }
}
```

**Step 2: Implement using ABP `CurrentUnitOfWork.SetFilterParameter` / `DisableFilter`**

```csharp
using Abp.Authorization.Users;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Domain.MultiTenancy
{
    public class TenantUserManager : DomainService, ITenantUserManager
    {
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly UserManager _userManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TenantUserManager(
            IRepository<UserTenantMembership, long> membershipRepository,
            IRepository<User, long> userRepository,
            IRepository<Tenant, int> tenantRepository,
            UserManager userManager,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public virtual async Task<UserTenantMembership> EnsureMembershipAsync(long hostUserId, int tenantId, bool isDefault = false)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var hostUser = await _userRepository.GetAsync(hostUserId);
                if (hostUser.TenantId != null)
                    throw new UserFriendlyException("Only host users can be associated with multiple tenants.");

                var tenant = await _tenantRepository.GetAsync(tenantId);
                var existing = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUserId && m.TenantId == tenantId);
                if (existing != null)
                {
                    if (isDefault)
                    {
                        await ClearDefaultFlagAsync(hostUserId);
                        existing.IsDefault = true;
                    }
                    return existing;
                }

                User tenantUser;
                using (CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, tenantId))
                {
                    tenantUser = await _userRepository.FirstOrDefaultAsync(u => u.UserName == hostUser.UserName && u.TenantId == tenantId);
                    if (tenantUser == null)
                    {
                        tenantUser = new User
                        {
                            TenantId = tenantId,
                            UserName = hostUser.UserName,
                            Name = hostUser.Name,
                            Surname = hostUser.Surname,
                            EmailAddress = hostUser.EmailAddress,
                            IsEmailConfirmed = hostUser.IsEmailConfirmed,
                            IsActive = hostUser.IsActive,
                            Password = hostUser.Password, // same hashed password
                        };
                        (await _userManager.CreateAsync(tenantUser)).CheckErrors();
                    }
                    else
                    {
                        tenantUser.Name = hostUser.Name;
                        tenantUser.Surname = hostUser.Surname;
                        tenantUser.EmailAddress = hostUser.EmailAddress;
                        tenantUser.IsActive = hostUser.IsActive;
                        tenantUser.Password = hostUser.Password;
                        (await _userManager.UpdateAsync(tenantUser)).CheckErrors();
                    }
                }

                if (isDefault)
                    await ClearDefaultFlagAsync(hostUserId);

                var membership = new UserTenantMembership
                {
                    UserId = hostUserId,
                    TenantId = tenantId,
                    TenantUserId = tenantUser.Id,
                    IsDefault = isDefault,
                };
                await _membershipRepository.InsertAsync(membership);
                return membership;
            }
        }

        public virtual async Task RemoveMembershipAsync(long hostUserId, int tenantId)
        {
            var membership = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUserId && m.TenantId == tenantId);
            if (membership == null)
                return;

            using (CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, tenantId))
            {
                var tenantUser = await _userRepository.GetAsync(membership.TenantUserId);
                await _userManager.DeleteAsync(tenantUser);
            }

            await _membershipRepository.DeleteAsync(membership);
        }

        private async Task ClearDefaultFlagAsync(long hostUserId)
        {
            var defaults = await _membershipRepository.GetAllListAsync(m => m.UserId == hostUserId && m.IsDefault);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
                await _membershipRepository.UpdateAsync(d);
            }
        }
    }
}
```

**Note:** The domain service uses `UserManager` from `Eaf.Middleware.Authorization.Users.UserManager`. It must be injected as `UserManager` (type alias) or fully qualified in Core. Since GameHub.Core does not reference the EAF user type directly, add `using UserManager = Eaf.Middleware.Authorization.Users.UserManager;` in the file and ensure Core references the correct packages (already does via EAF Middleware Core).

**Test:**
- `Api/test/GameHub.Tests/Domain/TenantUserManager_Tests.cs` — create host user, tenant, ensure membership, assert shadow user created with same password and `TenantId`.

Run:
```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~TenantUserManager_Tests" -c Release
```
Expected: PASS.

---

## Task 3: Application — DTOs e `IUserTenantAssociationAppService`

**Files:**
- Create: `Api/src/GameHub.Application/MultiTenancy/Dto/AssociateUserToTenantInput.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/Dto/RemoveUserTenantAssociationInput.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/Dto/UserTenantMembershipDto.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/Dto/SetDefaultTenantInput.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/Dto/GetUserTenantMembershipsInput.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/IUserTenantAssociationAppService.cs`
- Create: `Api/src/GameHub.Application/MultiTenancy/UserTenantAssociationAppService.cs`

**Step 1: DTOs**

```csharp
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace GameHub.MultiTenancy.Dto
{
    public class AssociateUserToTenantInput
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public int TenantId { get; set; }

        public bool IsDefault { get; set; }
    }

    public class RemoveUserTenantAssociationInput
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public int TenantId { get; set; }
    }

    public class SetDefaultTenantInput
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public int TenantId { get; set; }
    }

    public class GetUserTenantMembershipsInput
    {
        [Required]
        public long UserId { get; set; }
    }

    public class UserTenantMembershipDto : EntityDto<long>
    {
        public long UserId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantTenancyName { get; set; }
        public long TenantUserId { get; set; }
        public bool IsDefault { get; set; }
    }
}
```

**Step 2: Application service interface and implementation**

```csharp
using Abp.Application.Services;
using GameHub.MultiTenancy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameHub.MultiTenancy
{
    public interface IUserTenantAssociationAppService : IApplicationService
    {
        Task<UserTenantMembershipDto> AssociateAsync(AssociateUserToTenantInput input);
        Task RemoveAssociationAsync(RemoveUserTenantAssociationInput input);
        Task<UserTenantMembershipDto> SetDefaultAsync(SetDefaultTenantInput input);
        Task<List<UserTenantMembershipDto>> GetUserMembershipsAsync(GetUserTenantMembershipsInput input);
    }
}
```

```csharp
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using GameHub.Domain.MultiTenancy;
using GameHub.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.MultiTenancy
{
    public class UserTenantAssociationAppService : ApplicationService, IUserTenantAssociationAppService
    {
        private readonly ITenantUserManager _tenantUserManager;
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;

        public UserTenantAssociationAppService(
            ITenantUserManager tenantUserManager,
            IRepository<UserTenantMembership, long> membershipRepository)
        {
            _tenantUserManager = tenantUserManager;
            _membershipRepository = membershipRepository;
        }

        public virtual async Task<UserTenantMembershipDto> AssociateAsync(AssociateUserToTenantInput input)
        {
            var membership = await _tenantUserManager.EnsureMembershipAsync(input.UserId, input.TenantId, input.IsDefault);
            return ObjectMapper.Map<UserTenantMembershipDto>(membership);
        }

        public virtual async Task RemoveAssociationAsync(RemoveUserTenantAssociationInput input)
        {
            await _tenantUserManager.RemoveMembershipAsync(input.UserId, input.TenantId);
        }

        public virtual async Task<UserTenantMembershipDto> SetDefaultAsync(SetDefaultTenantInput input)
        {
            var membership = await _tenantUserManager.EnsureMembershipAsync(input.UserId, input.TenantId, isDefault: true);
            return ObjectMapper.Map<UserTenantMembershipDto>(membership);
        }

        public virtual async Task<List<UserTenantMembershipDto>> GetUserMembershipsAsync(GetUserTenantMembershipsInput input)
        {
            var query = from m in _membershipRepository.GetAll()
                        where m.UserId == input.UserId
                        select m;

            return await ObjectMapper.ProjectTo<UserTenantMembershipDto>(query).ToListAsync();
        }
    }
}
```

**Step 3: AutoMapper profile**

Add to `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` (or create a new `MultiTenancyDtoMapper`):

```csharp
createMap<UserTenantMembership, UserTenantMembershipDto>();
```

**Test:**
- `Api/test/GameHub.Tests/MultiTenancy/UserTenantAssociationAppService_Tests.cs` — associate, remove, set default, list.

Run:
```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~UserTenantAssociationAppService_Tests" -c Release
```
Expected: PASS.

---

## Task 4: Web — `HubAuthController` (login com seleção de tenant)

**Files:**
- Create: `Api/src/GameHub.Web.Host/Controllers/HubAuthController.cs`
- Modify: `Api/src/GameHub.Web.Host/Startup/Startup.cs` (CORS if needed — geralmente já ok)

**Goal:** Duas novas APIs para o `angular` público:
- `POST /api/hub/auth/available-tenants`
- `POST /api/hub/auth/select-tenant`

**Step 1: Create models**

Create `Api/src/GameHub.Web.Host/Models/HubAuth/AvailableTenantsModel.cs`:

```csharp
namespace GameHub.Web.Models.HubAuth
{
    public class AvailableTenantsModel
    {
        public string UserNameOrEmailAddress { get; set; }
        public string Password { get; set; }
    }
}
```

Create `Api/src/GameHub.Web.Host/Models/HubAuth/SelectTenantModel.cs`:

```csharp
namespace GameHub.Web.Models.HubAuth
{
    public class SelectTenantModel
    {
        public string UserNameOrEmailAddress { get; set; }
        public string Password { get; set; }
        public int TenantId { get; set; }
    }
}
```

Create `Api/src/GameHub.Web.Host/Models/HubAuth/AvailableTenantResult.cs`:

```csharp
namespace GameHub.Web.Models.HubAuth
{
    public class AvailableTenantResult
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenancyName { get; set; }
        public bool IsDefault { get; set; }
    }
}
```

**Step 2: Create controller**

```csharp
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using Eaf.Middleware.Web.Authentication;
using GameHub.Domain.MultiTenancy;
using GameHub.Web.Models.HubAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Web.Controllers
{
    [Route("api/hub/auth")]
    public class HubAuthController : GameHubControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITokenAuthenticationService _tokenAuthenticationService;
        private readonly UserManager _userManager;
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public HubAuthController(
            LogInManager logInManager,
            ITokenAuthenticationService tokenAuthenticationService,
            UserManager userManager,
            IRepository<UserTenantMembership, long> membershipRepository,
            IRepository<Tenant, int> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _logInManager = logInManager;
            _tokenAuthenticationService = tokenAuthenticationService;
            _userManager = userManager;
            _membershipRepository = membershipRepository;
            _tenantRepository = tenantRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [HttpPost("available-tenants")]
        public virtual async Task<IActionResult> GetAvailableTenants([FromBody] AvailableTenantsModel model)
        {
            // Force host context to find the global user account.
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var loginResult = await _logInManager.LoginAsync(model.UserNameOrEmailAddress, model.Password);
                if (loginResult.Result != AbpLoginResultType.Success)
                    throw new UserFriendlyException("Invalid username or password.");

                var hostUser = loginResult.User;
                if (hostUser.TenantId != null)
                {
                    // Already a tenant-bound user, return only its own tenant.
                    var tenant = await _tenantRepository.GetAsync(hostUser.TenantId.Value);
                    return Ok(new List<AvailableTenantResult>
                    {
                        new() { TenantId = tenant.Id, TenantName = tenant.Name, TenancyName = tenant.TenancyName, IsDefault = true }
                    });
                }

                var memberships = await _membershipRepository.GetAllListAsync(m => m.UserId == hostUser.Id);
                var tenantIds = memberships.Select(m => m.TenantId).ToList();
                var tenants = await _tenantRepository.GetAllListAsync(t => tenantIds.Contains(t.Id));

                var result = memberships.Select(m =>
                {
                    var t = tenants.FirstOrDefault(t => t.Id == m.TenantId);
                    return new AvailableTenantResult
                    {
                        TenantId = m.TenantId,
                        TenantName = t?.Name,
                        TenancyName = t?.TenancyName,
                        IsDefault = m.IsDefault,
                    };
                }).ToList();

                return Ok(result);
            }
        }

        [HttpPost("select-tenant")]
        public virtual async Task<IActionResult> SelectTenant([FromBody] SelectTenantModel model)
        {
            // Validate membership in host context.
            UserTenantMembership membership;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var hostUser = await _userManager.FindByNameOrEmailAsync(model.UserNameOrEmailAddress);
                if (hostUser == null)
                    throw new UserFriendlyException("User not found.");

                membership = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUser.Id && m.TenantId == model.TenantId);
                if (membership == null)
                    throw new UserFriendlyException("User is not associated with the selected tenant.");
            }

            // Authenticate the tenant-level shadow user.
            using (CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, model.TenantId))
            {
                var tenantUser = await _userManager.GetUserByIdAsync(new Abp.UserIdentifier(model.TenantId, membership.TenantUserId));
                if (tenantUser == null)
                    throw new UserFriendlyException("Tenant user not found.");

                // We do not re-check password here because the host password was already checked above;
                // the shadow user password is kept in sync by TenantUserManager.
                var claimsPrincipal = await _userManager.ClaimsFactory.CreateAsync(tenantUser);
                var accessToken = await _tokenAuthenticationService.CreateAccessTokenAsync(
                    claimsPrincipal.Claims,
                    tokenExpiration: null);

                return Ok(new
                {
                    accessToken,
                    expireInSeconds = accessToken.ExpireInSeconds,
                    userId = tenantUser.Id,
                    tenantId = model.TenantId,
                });
            }
        }
    }
}
```

**Important:** The `LogInManager` namespace is `Eaf.Middleware.Authorization.LogInManager`? Actually the XML showed `Eaf.Middleware.Identity.LogInManager`. Verify exact namespace before implementation. Also `GameHubControllerBase` may not exist — use `ControllerBase` or `MiddlewareControllerBase` as base. Adapt as needed.

**Note:** Using `_userManager.ClaimsFactory` may not be exposed. If not, manually build claims with `AbpClaimTypes` constants (see EAF `UserClaimsPrincipalFactory`). As a fallback, call `SignInManager.CreateUserPrincipalAsync(tenantUser)` or replicate claim generation. The exact call must be validated against EAF source during implementation.

**Step 3: Add `[Authorize]` to `SelectTenant` only if needed? No, it must be anonymous.**

**Step 4: Register HTTP client route in `angular`**

Angular service will call `/api/hub/auth/available-tenants` and `/api/hub/auth/select-tenant`.

**Test:**
- `Api/test/GameHub.Web.Tests/Controllers/HubAuthController_Tests.cs` — login, retrieve available tenants, select tenant and verify token contains `TenantId` claim.

Run:
```bash
dotnet test Api/test/GameHub.Web.Tests/GameHub.Web.Tests.csproj --filter "FullyQualifiedName~HubAuthController_Tests" -c Release
```
Expected: PASS.

---

## Task 5: Infraestrutura — Migration e seed do tenant `Default`

**Files:**
- Create migration: `dotnet ef migrations add GameHubUserTenantMemberships`
- Modify: `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/SeedHelper.cs` (ou similar)
- Modify: `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/HostRoleAndUserCreator.cs` (se existir)

**Goal:** Garantir que o banco contenha tenant `Default` (id=1) e que o admin padrão tenha uma membership default apontando para ele.

**Step 1: Ensure `Default` tenant exists**

ABP já cria tenant `Default` via `CreateWithAdminUserAsync`? Verify seed. If not, add to `SeedHelper`:

```csharp
private async Task CreateDefaultTenantIfNeeded()
{
    var defaultTenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == "Default");
    if (defaultTenant != null) return;

    await _tenantManager.CreateWithAdminUserAsync(
        tenancyName: "Default",
        name: "Default",
        adminPassword: "123qwe", // use configured default password
        adminEmailAddress: "admin@gamehub.local",
        isActive: true,
        shouldChangePasswordOnNextLogin: false,
        sendActivationEmail: false,
        emailActivationLink: null);
}
```

**Step 2: Seed host admin user membership to `Default`**

After creating host admin user, call `ITenantUserManager.EnsureMembershipAsync(hostAdminUserId, defaultTenantId, isDefault: true)`.

**Step 3: Generate EF Core migration**

```bash
cd /home/ubuntu/repos/gamehub/Api/src/GameHub.EntityFrameworkCore
dotnet ef migrations add GameHubUserTenantMemberships --startup-project ../GameHub.Web.Host
cd /home/ubuntu/repos/gamehub
dotnet build Api/GameHub.sln -c Release
```

**Step 4: Update test seed**

Ensure test project (`GameHub.Tests`) seeds `Default` tenant and membership before tests run, or uses in-memory state with `GameHubDbContext` initialization.

Run:
```bash
dotnet test Api/GameHub.sln -c Release
```
Expected: all pass.

---

## Task 6: `angular-admin` — Gerenciamento de associações

**Files:**
- Create: `angular-admin/GameHub.UI/src/app/admin/tenants/tenant-users-modal.component.ts`
- Create: `angular-admin/GameHub.UI/src/app/admin/tenants/tenant-users-modal.component.html`
- Modify: `angular-admin/GameHub.UI/src/app/admin/tenants/tenants.component.ts`
- Modify: `angular-admin/GameHub.UI/src/app/admin/tenants/tenants.component.html`
- Modify: `angular-admin/GameHub.UI/src/app/admin/users/users.component.ts`
- Modify: `angular-admin/GameHub.UI/src/app/admin/users/users.component.html`
- Modify: `angular-admin/GameHub.UI/src/app/admin/users/create-user-modal.component.ts` (ou edit-user-modal)
- Create proxy: `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxies.ts` regenerated via `nswag` (or manually add new methods)

**Step 1: Extend `service-proxies.ts` with new `UserTenantAssociationAppService` methods**

After backend build, run `npm run service-update` in `angular-admin/GameHub.UI` to regenerate proxies, or manually add:

```typescript
export class UserTenantAssociationServiceProxy {
    associate(body: AssociateUserToTenantInput | undefined): Observable<UserTenantMembershipDto> { ... }
    removeAssociation(body: RemoveUserTenantAssociationInput | undefined): Observable<void> { ... }
    setDefault(body: SetDefaultTenantInput | undefined): Observable<UserTenantMembershipDto> { ... }
    getUserMemberships(userId: number | undefined): Observable<UserTenantMembershipDto[]> { ... }
}
```

**Step 2: Add "Users" action to tenant row**

In `tenants.component.html`, add a button in the actions column:

```html
<button *ngIf="permission.isGranted('Pages.Tenants.ManageUsers')" class="btn btn-sm btn-primary" (click)="manageUsers(record)">
  <i class="la la-users"></i>
</button>
```

**Step 3: Create `TenantUsersModalComponent`**

Opens from `tenants.component.ts` and allows associating/removing users for that tenant. Use `CommonLookupModalComponent` to find users (existing component in admin).

```typescript
@Component({
  selector: 'tenantUsersModal',
  templateUrl: './tenant-users-modal.component.html',
})
export class TenantUsersModalComponent extends AppComponentBase {
  @ViewChild('modal', { static: true }) modal: ModalDirective;
  @Output() modalSave = new EventEmitter<any>();

  active = false;
  saving = false;
  tenantId: number;
  tenantName: string;
  members: UserTenantMembershipDto[] = [];

  constructor(
    injector: Injector,
    private readonly _userTenantService: UserTenantAssociationServiceProxy,
    private readonly _userServiceProxy: UserServiceProxy,
    private readonly _commonLookup: CommonLookupModalComponent,
  ) {
    super(injector);
  }

  show(tenantId: number, tenantName: string): void {
    this.tenantId = tenantId;
    this.tenantName = tenantName;
    this.active = true;
    this.loadMembers();
    this.modal.show();
  }

  loadMembers(): void {
    // We need a backend endpoint to list members by tenant. Alternatively, iterate users and filter.
    // Add to UserTenantAssociationAppService if needed.
  }

  addUser(user: UserListDto): void {
    this._userTenantService.associate(new AssociateUserToTenantInput({
      userId: user.id,
      tenantId: this.tenantId,
      isDefault: false,
    })).subscribe(() => {
      this.notify.success(this.l('SavedSuccessfully'));
      this.loadMembers();
    });
  }

  remove(userId: number): void {
    this._userTenantService.removeAssociation(new RemoveUserTenantAssociationInput({
      userId,
      tenantId: this.tenantId,
    })).subscribe(() => this.loadMembers());
  }

  setDefault(membership: UserTenantMembershipDto): void {
    this._userTenantService.setDefault(new SetDefaultTenantInput({
      userId: membership.userId,
      tenantId: membership.tenantId,
    })).subscribe(() => this.loadMembers());
  }
}
```

**Step 4: Add "Tenants" tab to user create/edit modal**

In `create-user-modal.component.ts` / `edit-user-modal.component.ts`, add a multi-select of tenants and a checkbox `IsDefault`. On save, call `UserTenantAssociationServiceProxy.associate` for each selected tenant.

**Step 5: Update localization strings**

Add to `angular-admin/GameHub.UI/src/app/shared/localization/source-files` (XML/JSON depending on EAF setup):
- `ManageTenantUsers`
- `TenantUsers`
- `SetDefaultTenant`
- `UserTenants`

Run:
```bash
cd /home/ubuntu/repos/gamehub/angular-admin/GameHub.UI
npm run build
npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox
```
Expected: build OK, tests PASS.

---

## Task 7: `angular` público — Login com seleção de tenant

**Files:**
- Create: `angular/src/app/public/login/tenant-selection.component.ts`
- Create: `angular/src/app/public/login/tenant-selection.component.html`
- Modify: `angular/src/app/core/auth/auth.service.ts`
- Modify: `angular/src/app/core/auth/token.service.ts`
- Modify: `angular/src/app/public/login/login.component.ts`
- Modify: `angular/src/app/public/login/login.component.html`

**Goal:** Após login, se o usuário tiver mais de um tenant, mostrar modal de seleção; se tiver um só, selecionar automaticamente; se for admin/host sem tenants, permitir acesso a host.

**Step 1: Add models to `auth.service.ts`**

```typescript
export interface AvailableTenant {
  tenantId: number;
  tenantName: string;
  tenancyName: string;
  isDefault: boolean;
}

export interface SelectTenantModel {
  userNameOrEmailAddress: string;
  password: string;
  tenantId: number;
}

export interface TenantSelectionResult {
  accessToken: string;
  expireInSeconds: number;
  userId: number;
  tenantId: number;
}
```

**Step 2: Add methods to `AuthService`**

```typescript
private readonly hubAuthUrl = '/api/hub/auth';

getAvailableTenants(model: AuthenticateModel): Observable<AvailableTenant[]> {
  return this.http.post<AvailableTenant[]>(`${this.hubAuthUrl}/available-tenants`, model);
}

selectTenant(model: SelectTenantModel): Observable<TenantSelectionResult> {
  return this.http.post<TenantSelectionResult>(`${this.hubAuthUrl}/select-tenant`, model);
}
```

**Step 3: Update `TokenService` to store/return `tenantId`**

Add:

```typescript
getTenantId(): number | null {
  const payload = this.getPayload();
  return payload ? Number(payload.tenantid ?? payload['http://www.aspnetboilerplate.com/identity/claims/tenantId'] ?? null) : null;
}
```

**Step 4: Update `LoginComponent` flow**

```typescript
login(): void {
  if (!this.model.userNameOrEmailAddress || !this.model.password) return;
  this.loading = true;
  this.auth.getAvailableTenants(this.model).subscribe({
    next: tenants => {
      this.loading = false;
      if (tenants.length === 0) {
        this.error = 'No tenant available for this user.';
      } else if (tenants.length === 1) {
        this.selectTenant(tenants[0]);
      } else {
        this.openTenantSelection(tenants);
      }
    },
    error: () => { this.loading = false; this.error = 'Invalid username or password.'; },
  });
}

private selectTenant(tenant: AvailableTenant): void {
  const selectModel: SelectTenantModel = {
    userNameOrEmailAddress: this.model.userNameOrEmailAddress,
    password: this.model.password,
    tenantId: tenant.tenantId,
  };
  this.loading = true;
  this.auth.selectTenant(selectModel).subscribe({
    next: result => {
      this.loading = false;
      this.tokenService.setToken(result.accessToken);
      this.router.navigateByUrl(this.returnUrl);
    },
    error: () => { this.loading = false; this.error = 'Could not switch to selected tenant.'; },
  });
}
```

**Step 5: Create `TenantSelectionComponent` modal**

Simple modal listing `AvailableTenant[]`, highlighting default, and confirming selection. Keep standalone unless route is required.

**Step 6: Update `developer.service.ts` and `gameplay-bridge.service.ts` to use tenant-scoped endpoints**

The backend will filter by `IAbpSession.TenantId` automatically. Ensure the `angular` public HTTP client sends the JWT in every request. No additional tenant header is needed.

**Test:**
Run:
```bash
cd /home/ubuntu/repos/gamehub/angular
npm run build
```
Expected: production build OK.

Manual/end-to-end test checklist:
1. Create tenant `acme` in angular-admin.
2. Create user `john` and associate to `Default` and `acme`.
3. Login in angular with `john` → see two tenants → select `acme`.
4. Access `/developer/games` → only games of `acme` appear.
5. Switch tenant via UI (add tenant switcher in header, optional Task 8).

---

## Task 8: `angular` público — Switch de tenant no header (opcional, mas recomendado)

**Files:**
- Create: `angular/src/app/core/services/tenant-switch.service.ts`
- Modify: `angular/src/app/app.html` or header component
- Modify: `angular/src/app/core/auth/auth.service.ts`

**Goal:** Permitir trocar de tenant sem refazer login. Implementar via `select-tenant` usando credenciais armazenadas? Não armazenar senha. Melhor: após login, armazenar `refresh token`? Não implementado. Alternativa: requer re-login ao trocar. Por segurança, o switch mostra modal pedindo senha.

Simplificação: Task 8 é *optional*; pode ser feito em fase 2. Para fase 1, basta logout/login.

---

## Task 9: Segurança e permissões

**Files:**
- Modify: `Api/src/GameHub.Core/Authorization/GameHubAuthorizationProvider.cs`
- Modify: `Api/src/GameHub.Core/Localization/GameHubLocalization/*` if adding new permission display names

**Step 1: Add permissions**

```csharp
context.CreatePermission("Pages.Tenants.ManageUsers", L("ManageTenantUsers"), multiTenancySides: MultiTenancySides.Host);
context.CreatePermission("Pages.Users.ManageTenants", L("ManageUserTenants"), multiTenancySides: MultiTenancySides.Host);
```

**Step 2: Authorize application service methods**

Add `[AbpAuthorize("Pages.Users.ManageTenants")]` on `UserTenantAssociationAppService` methods (or `[AbpAuthorize(PermissionNames.Pages_Users)]`).

**Test:**
- Unauthorized user cannot call `AssociateAsync`. Add test in `UserTenantAssociationAppService_Tests` using `LoginAsHostAdmin` vs `LoginAsTenantUser`.

---

## Task 10: Testes de integração e cobertura

**Files:**
- Create: `Api/test/GameHub.Tests/MultiTenancy/UserTenantAssociationAppService_Tests.cs`
- Create: `Api/test/GameHub.Tests/Domain/TenantUserManager_Tests.cs`
- Create: `Api/test/GameHub.Web.Tests/Controllers/HubAuthController_Tests.cs`
- Create: `angular-admin/GameHub.UI/src/app/admin/tenants/tenant-users-modal.component.spec.ts` (optional)

**Patterns:**
- Use `WithUnitOfWorkAsync` from `abp-testing` skill.
- Use `GetRequiredService<T>()`.
- Seed default tenant and admin in test `GameHubTestBase` if not already.
- Test cross-tenant data isolation: create game in tenant A, login as user in tenant B, assert game not visible.

Run all tests:
```bash
dotnet test Api/GameHub.sln -c Release
```
Expected: all pass.

Run Angular tests:
```bash
cd angular-admin/GameHub.UI && npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox
cd angular && npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox
```
Expected: all pass.

---

## Task 11: Documentação e runbooks

**Files:**
- Create: `docs/tenants-and-user-associations.md`
- Modify: `docs/agent-execution-log.md`
- Modify: `docs/superpowers/plans/2026-07-27-gamehub-tenant-companies-and-user-associations.md` (this file) if needed

**Conteúdo de `docs/tenants-and-user-associations.md`:**
- Visão geral do modelo tenant = empresa.
- Como criar uma empresa e associar usuários pelo angular-admin.
- Como funciona o login no hub com seleção de tenant.
- Diagrama de sequência (Mermaid) do fluxo de autenticação.
- Decisões arquiteturais: shadow user, `UserTenantMembership`, `Default` tenant.
- Checklist operacional: seed, migração, CORS, JWT claims.

**Conteúdo de `docs/agent-execution-log.md`:**
- Registrar arquivos alterados, comandos de build/teste e resultados.

---

## Task 12: Commit e PR

**Step 1: Verificar diff**

```bash
cd /home/ubuntu/repos/gamehub
git diff --stat
```

**Step 2: Commit separados (sugestão)**

```bash
git add Api/src/GameHub.Core/Domain/MultiTenancy/
git commit -m "feat(multi-tenancy): add UserTenantMembership and TenantUserManager"

git add Api/src/GameHub.Application/MultiTenancy/
git commit -m "feat(multi-tenancy): add user-tenant association application service"

git add Api/src/GameHub.Web.Host/Controllers/HubAuthController.cs
git commit -m "feat(auth): add hub login with tenant selection endpoints"

git add Api/src/GameHub.EntityFrameworkCore/Migrations/
git commit -m "feat(multi-tenancy): add GameHubUserTenantMemberships migration and Default tenant seed"

git add angular-admin/GameHub.UI/src/app/admin/tenants/ angular-admin/GameHub.UI/src/app/admin/users/
git commit -m "feat(admin): manage user-tenant associations"

git add angular/src/app/public/login/ angular/src/app/core/auth/
git commit -m "feat(hub): tenant selection after public login"

git add docs/
git commit -m "docs: tenant companies and user associations guide"
```

**Step 3: Push e PR**

```bash
git push -u origin feature/tenant-companies-and-user-associations
```

Create PR to `develop` (or `main` per repo convention) with summary and link to this plan.

---

## Self-review checklist

| Requirement | Task(s) |
|-------------|---------|
| Empresa = tenant ABP | Reuses existing Tenant/ITenantAppService; seeds `Default` (Task 5) |
| Tenant default | `Default` tenant seeded; `IsDefault` flag in `UserTenantMembership` (Tasks 1, 5) |
| Usuários associados por empresa | `UserTenantAssociationAppService` + `TenantUserManager` (Tasks 1–3) |
| Usuário em múltiplos tenants | `UserTenantMembership` many-to-many + shadow users (Tasks 1, 2) |
| Gerenciamento no angular-admin | Tenant-users modal + user tenants tab (Task 6) |
| Login no angular + seleção de tenant | `HubAuthController` + `TenantSelectionComponent` (Tasks 4, 7) |
| Jogos/dashboard filtrados por tenant | ABP `IMayHaveTenant` + JWT `TenantId` claim (all tasks) |
| Build/testes/documentação | Tasks 10–12 |

**Known open questions for implementation:**
1. Exact namespace and public API of `EAF.Middleware.Identity.LogInManager` and `UserManager.ClaimsFactory` must be verified against EAF 9.3.1 source or metadata before writing `HubAuthController`.
2. If `UserManager.ClaimsFactory` is not accessible, implement a small `ITenantClaimsPrincipalFactory` wrapper in `Web.Host` that replicates the standard ABP claims for a given tenant user.
3. Confirm `GameHubDbContext` already supports custom `DbSet` placement; if `GameHubModelCreatingExtensions` is the preferred configuration location, use it instead of `OnModelCreating`.
4. Confirm `MiddlewareControllerBase` vs `ControllerBase` base class in GameHub; if `MiddlewareControllerBase` is not available, inherit from `ControllerBase` and add `[ApiController]` if needed.

---

## Commands reference

```bash
# Backend build and test
dotnet build Api/GameHub.sln -c Release
dotnet test Api/GameHub.sln -c Release

# Migration
cd Api/src/GameHub.EntityFrameworkCore
dotnet ef migrations add GameHubUserTenantMemberships --startup-project ../GameHub.Web.Host

# Angular admin
cd angular-admin/GameHub.UI
npm run build
npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox

# Angular public
cd angular
npm run build
npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox

# Service proxies regeneration (if backend endpoint changed)
cd angular-admin/GameHub.UI
npm run service-update
```
