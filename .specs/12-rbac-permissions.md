# 12 — RBAC Permissions

> **Status:** Draft  
> **Stack:** .NET 10 LTS · ASP.NET Boilerplate/EAF · PostgreSQL 16+ · Redis 7+  
> **Domains:** gamehub.afonsoft.dev · gamehub-admin.afonsoft.dev · gamehub-api.afonsoft.dev

---

## 1. Overview

GameHub leverages the ABP/EAF permission system. The system has two pillars:

- **PermissionManager** — runtime service that resolves the current user's effective permissions by merging role assignments with user-level overrides.
- **AuthorizationProvider** — declarative registration of every permission in the system, organized hierarchically under page-based namespaces.

### How it works

1. **Define** permissions as string constants (§2).
2. **Register** them in an `AuthorizationProvider` subclass (§4).
3. **Seed** initial data so every permission exists in the database (§7).
4. **Assign** permissions to roles via the admin UI or migration data.
5. **Check** permissions at service/controller level via `IAuthorizationService` (§5) and at UI level via route guards (§6).

### Permission hierarchy

```
Pages.Games
├── Pages.Games.View
├── Pages.Games.Create
├── Pages.Games.Edit
├── Pages.Games.Delete
├── Pages.Games.Publish
└── Pages.Games.Suspend
```

Granting a parent permission automatically grants all children unless a child is explicitly denied for a specific user.

---

## 2. Permission Constants

```csharp
namespace GameHub.Authorization;

/// <summary>
/// Central definition of every permission in the GameHub platform.
/// Permission strings follow the ABP convention: "Pages.{Area}.{Action}".
/// </summary>
public static class GameHubPermissions
{
    // ─────────────────────────────────────────────
    // Games
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for all game operations.</summary>
    public const string Games = "Pages.Games";

    /// <summary>View game listings and details.</summary>
    public const string GamesView = "Pages.Games.View";

    /// <summary>Create a new game draft.</summary>
    public const string GamesCreate = "Pages.Games.Create";

    /// <summary>Edit game metadata (title, description, categories, tags).</summary>
    public const string GamesEdit = "Pages.Games.Edit";

    /// <summary>Delete (soft-delete) a game.</summary>
    public const string GamesDelete = "Pages.Games.Delete";

    /// <summary>Publish an approved build to production.</summary>
    public const string GamesPublish = "Pages.Games.Publish";

    /// <summary>Suspend a live game.</summary>
    public const string GamesSuspend = "Pages.Games.Suspend";

    // ─────────────────────────────────────────────
    // Builds
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for all build operations.</summary>
    public const string Builds = "Pages.Builds";

    /// <summary>Upload a new build zip for a game.</summary>
    public const string BuildsUpload = "Pages.Builds.Upload";

    /// <summary>View build details and validation results.</summary>
    public const string BuildsView = "Pages.Builds.View";

    /// <summary>Approve a build after moderation review.</summary>
    public const string BuildsApprove = "Pages.Builds.Approve";

    /// <summary>Reject a build after moderation review.</summary>
    public const string BuildsReject = "Pages.Builds.Reject";

    // ─────────────────────────────────────────────
    // Moderation
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for all moderation operations.</summary>
    public const string Moderation = "Pages.Moderation";

    /// <summary>View the moderation review queue.</summary>
    public const string ModerationView = "Pages.Moderation.View";

    /// <summary>Start or continue a moderation review.</summary>
    public const string ModerationReview = "Pages.Moderation.Review";

    /// <summary>Mark a moderation review as complete.</summary>
    public const string ModerationComplete = "Pages.Moderation.Complete";

    // ─────────────────────────────────────────────
    // Categories
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for category management.</summary>
    public const string Categories = "Pages.Categories";

    /// <summary>Create, edit, delete, and reorder categories.</summary>
    public const string CategoriesManage = "Pages.Categories.Manage";

    // ─────────────────────────────────────────────
    // Tags
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for tag management.</summary>
    public const string Tags = "Pages.Tags";

    /// <summary>Create, edit, and delete tags.</summary>
    public const string TagsManage = "Pages.Tags.Manage";

    // ─────────────────────────────────────────────
    // Dashboard / Admin
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for dashboard access.</summary>
    public const string Dashboard = "Pages.Dashboard";

    /// <summary>View dashboard metrics and KPIs.</summary>
    public const string DashboardView = "Pages.Dashboard.View";

    /// <summary>Manage feature flags.</summary>
    public const string FeatureFlags = "Pages.Dashboard.FeatureFlags";

    /// <summary>View the audit log.</summary>
    public const string AuditLog = "Pages.Dashboard.AuditLog";

    // ─────────────────────────────────────────────
    // Users
    // ─────────────────────────────────────────────

    /// <summary>Parent permission for user management.</summary>
    public const string Users = "Pages.Users";

    /// <summary>Create, edit, suspend, and delete user accounts.</summary>
    public const string UsersManage = "Pages.Users.Manage";

    // ─────────────────────────────────────────────
    // Gameplay (public)
    // ─────────────────────────────────────────────

    /// <summary>Access gameplay features (play games, submit scores).</summary>
    public const string Gameplay = "Pages.Gameplay";

    /// <summary>View leaderboards.</summary>
    public const string Leaderboard = "Pages.Leaderboard";
}
```

---

## 3. Role–Permission Matrix

| Permission | SuperAdmin | Admin | Moderator | Developer | Player (default) |
|---|:---:|:---:|:---:|:---:|:---:|
| **Pages.Games** | ✅ | ✅ | ✅ | ✅ | — |
| Pages.Games.View | ✅ | ✅ | ✅ | ✅ | ✅ |
| Pages.Games.Create | ✅ | ✅ | — | ✅ | — |
| Pages.Games.Edit | ✅ | ✅ | — | ✅ (own) | — |
| Pages.Games.Delete | ✅ | ✅ | — | ✅ (own) | — |
| Pages.Games.Publish | ✅ | ✅ | — | — | — |
| Pages.Games.Suspend | ✅ | ✅ | — | — | — |
| **Pages.Builds** | ✅ | ✅ | ✅ | ✅ | — |
| Pages.Builds.Upload | ✅ | — | — | ✅ | — |
| Pages.Builds.View | ✅ | ✅ | ✅ | ✅ (own) | — |
| Pages.Builds.Approve | ✅ | ✅ | ✅ | — | — |
| Pages.Builds.Reject | ✅ | ✅ | ✅ | — | — |
| **Pages.Moderation** | ✅ | ✅ | ✅ | — | — |
| Pages.Moderation.View | ✅ | ✅ | ✅ | — | — |
| Pages.Moderation.Review | ✅ | — | ✅ | — | — |
| Pages.Moderation.Complete | ✅ | — | ✅ | — | — |
| **Pages.Categories** | ✅ | ✅ | — | — | — |
| Pages.Categories.Manage | ✅ | ✅ | — | — | — |
| **Pages.Tags** | ✅ | ✅ | — | — | — |
| Pages.Tags.Manage | ✅ | ✅ | — | — | — |
| **Pages.Dashboard** | ✅ | ✅ | — | — | — |
| Pages.Dashboard.View | ✅ | ✅ | — | — | — |
| Pages.Dashboard.FeatureFlags | ✅ | ✅ | — | — | — |
| Pages.Dashboard.AuditLog | ✅ | ✅ | — | — | — |
| **Pages.Users** | ✅ | ✅ | — | — | — |
| Pages.Users.Manage | ✅ | ✅ | — | — | — |
| **Pages.Gameplay** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Pages.Leaderboard** | ✅ | ✅ | ✅ | ✅ | ✅ |

> **Legend:** ✅ = granted, — = not granted, ✅ (own) = granted only for resources owned by the user.

---

## 4. ABP AuthorizationProvider Configuration

```csharp
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using GameHub.Authorization;
using GameHub.Authorization.Roles;
using GameHub.Authorization.Users;
using GameHub.MultiTenancy;

namespace GameHub.Authorization;

public class GameHubAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        var pages = context.GetPermissionOrNull("Pages")
                   ?? context.CreatePermission("Pages", L("Permission:Pages"));

        // ── Games ──────────────────────────────────
        var games = pages.CreateChildPermission(
            GameHubPermissions.Games, L("Permission:Games"));
        games.CreateChildPermission(
            GameHubPermissions.GamesView, L("Permission:Games.View"));
        games.CreateChildPermission(
            GameHubPermissions.GamesCreate, L("Permission:Games.Create"));
        games.CreateChildPermission(
            GameHubPermissions.GamesEdit, L("Permission:Games.Edit"));
        games.CreateChildPermission(
            GameHubPermissions.GamesDelete, L("Permission:Games.Delete"));
        games.CreateChildPermission(
            GameHubPermissions.GamesPublish, L("Permission:Games.Publish"));
        games.CreateChildPermission(
            GameHubPermissions.GamesSuspend, L("Permission:Games.Suspend"));

        // ── Builds ─────────────────────────────────
        var builds = pages.CreateChildPermission(
            GameHubPermissions.Builds, L("Permission:Builds"));
        builds.CreateChildPermission(
            GameHubPermissions.BuildsUpload, L("Permission:Builds.Upload"));
        builds.CreateChildPermission(
            GameHubPermissions.BuildsView, L("Permission:Builds.View"));
        builds.CreateChildPermission(
            GameHubPermissions.BuildsApprove, L("Permission:Builds.Approve"));
        builds.CreateChildPermission(
            GameHubPermissions.BuildsReject, L("Permission:Builds.Reject"));

        // ── Moderation ──────────────────────────────
        var moderation = pages.CreateChildPermission(
            GameHubPermissions.Moderation, L("Permission:Moderation"));
        moderation.CreateChildPermission(
            GameHubPermissions.ModerationView, L("Permission:Moderation.View"));
        moderation.CreateChildPermission(
            GameHubPermissions.ModerationReview, L("Permission:Moderation.Review"));
        moderation.CreateChildPermission(
            GameHubPermissions.ModerationComplete, L("Permission:Moderation.Complete"));

        // ── Categories ──────────────────────────────
        var categories = pages.CreateChildPermission(
            GameHubPermissions.Categories, L("Permission:Categories"));
        categories.CreateChildPermission(
            GameHubPermissions.CategoriesManage, L("Permission:Categories.Manage"));

        // ── Tags ────────────────────────────────────
        var tags = pages.CreateChildPermission(
            GameHubPermissions.Tags, L("Permission:Tags"));
        tags.CreateChildPermission(
            GameHubPermissions.TagsManage, L("Permission:Tags.Manage"));

        // ── Dashboard ───────────────────────────────
        var dashboard = pages.CreateChildPermission(
            GameHubPermissions.Dashboard, L("Permission:Dashboard"));
        dashboard.CreateChildPermission(
            GameHubPermissions.DashboardView, L("Permission:Dashboard.View"));
        dashboard.CreateChildPermission(
            GameHubPermissions.FeatureFlags, L("Permission:Dashboard.FeatureFlags"));
        dashboard.CreateChildPermission(
            GameHubPermissions.AuditLog, L("Permission:Dashboard.AuditLog"));

        // ── Users ───────────────────────────────────
        var users = pages.CreateChildPermission(
            GameHubPermissions.Users, L("Permission:Users"));
        users.CreateChildPermission(
            GameHubPermissions.UsersManage, L("Permission:Users.Manage"));

        // ── Gameplay (public) ───────────────────────
        pages.CreateChildPermission(
            GameHubPermissions.Gameplay, L("Permission:Gameplay"));
        pages.CreateChildPermission(
            GameHubPermissions.Leaderboard, L("Permission:Leaderboard"));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, GameHubConsts.LocalizationSourceName);
    }
}
```

Register the provider in the module:

```csharp
// In GameHubCoreModule.ConfigureServices
Configuration.Authorization.Providers.Add<GameHubAuthorizationProvider>();
```

---

## 5. Checking Permissions in Application Services

### Using `IAuthorizationService` directly

```csharp
using Abp.Authorization;
using GameHub.Authorization;

namespace GameHub.Games;

public class GameAppService : ApplicationService, IGameAppService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IRepository<Game, Guid> _gameRepository;

    public GameAppService(
        IAuthorizationService authorizationService,
        IRepository<Game, Guid> gameRepository)
    {
        _authorizationService = authorizationService;
        _gameRepository = gameRepository;
    }

    public async Task PublishGameAsync(PublishGameInput input)
    {
        _authorizationService.IsGranted(GameHubPermissions.GamesPublish);

        var game = await _gameRepository.GetAsync(input.GameId);

        game.Publish();

        await _gameRepository.UpdateAsync(game);

        Logger.LogInformation(
            "Game {GameId} published by {UserId}",
            game.Id,
            AbpSession.UserId);
    }
}
```

### Using the `[AbpAuthorize]` attribute

```csharp
[AbpAuthorize(GameHubPermissions.GamesCreate)]
public async Task<GameSummaryDto> CreateGameDraftAsync(CreateGameDraftInput input)
{
    // …
}

[AbpAuthorize(GameHubPermissions.GamesEdit)]
public async Task UpdateGameMetadataAsync(UpdateGameMetadataInput input)
{
    // …
}
```

### Combining multiple permissions (AND)

```csharp
[AbpAuthorize(GameHubPermissions.ModerationView)]
[AbpAuthorize(GameHubPermissions.ModerationComplete)]
public async Task CompleteReviewAsync(CompleteReviewInput input)
{
    // …
}
```

### Granting for any of multiple permissions (OR)

```csharp
if (_authorizationService.IsGranted(GameHubPermissions.GamesPublish) ||
    _authorizationService.IsGranted(GameHubPermissions.GamesSuspend))
{
    // User has at least one of the two permissions
}
```

### Checking at the controller level

```csharp
[HttpPost]
[Authorize(GameHubPermissions.BuildsUpload)]
public async Task<UploadGameBuildResultDto> UploadBuild(IFormFile file)
{
    // …
}
```

---

## 6. Checking Permissions in Angular

### AuthGuard (route guard)

```typescript
// angular/src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: router.routerState.snapshot.url }
  });
};
```

### PermissionGuard (role/permission check)

```typescript
// angular/src/app/core/guards/permission.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const PermissionGuard = (requiredPermission: string): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.hasPermission(requiredPermission)) {
      return true;
    }

    return router.createUrlTree(['/unauthorized']);
  };
};
```

### AuthService (JWT + permission resolution)

```typescript
// angular/src/app/core/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  unique_name: string;
  role: string[];
  exp: number;
  iss: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API_URL = 'https://gamehub-api.afonsoft.dev';
  private token: string | null = null;

  constructor(private http: HttpClient) {}

  login(userNameOrEmailAddress: string, password: string): Observable<any> {
    return this.http.post(`${this.API_URL}/api/TokenAuth/Authenticate`, {
      userNameOrEmailAddress,
      password
    }).pipe(
      tap((response: any) => {
        this.token = response.result.accessToken;
        localStorage.setItem('auth_token', this.token);
      })
    );
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const decoded = jwtDecode<JwtPayload>(token);
    return decoded.exp * 1000 > Date.now();
  }

  hasPermission(permission: string): boolean {
    const token = this.getToken();
    if (!token) return false;

    const decoded = jwtDecode<JwtPayload>(token);
    return decoded.role?.includes(permission) ?? false;
  }

  getToken(): string | null {
    if (this.token) return this.token;
    this.token = localStorage.getItem('auth_token');
    return this.token;
  }

  logout(): void {
    this.token = null;
    localStorage.removeItem('auth_token');
  }
}
```

### Route configuration with guards

```typescript
// angular/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { PermissionGuard } from './core/guards/permission.guard';
import { GameHubPermissions } from './core/constants/permissions';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./public/public.routes').then(m => m.PUBLIC_ROUTES)
  },
  {
    path: 'play',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./player/player.routes').then(m => m.PLAYER_ROUTES)
  },
  {
    path: 'developer',
    canActivate: [AuthGuard, PermissionGuard(GameHubPermissions.GamesCreate)],
    loadChildren: () =>
      import('./developer/developer.routes').then(m => m.DEVELOPER_ROUTES)
  }
];
```

### Permission constants

```typescript
// angular/src/app/core/constants/permissions.ts
export const GameHubPermissions = {
  Games:              'Pages.Games',
  GamesView:          'Pages.Games.View',
  GamesCreate:        'Pages.Games.Create',
  GamesEdit:          'Pages.Games.Edit',
  GamesDelete:        'Pages.Games.Delete',
  GamesPublish:       'Pages.Games.Publish',
  GamesSuspend:       'Pages.Games.Suspend',
  Builds:             'Pages.Builds',
  BuildsUpload:       'Pages.Builds.Upload',
  BuildsView:         'Pages.Builds.View',
  BuildsApprove:      'Pages.Builds.Approve',
  BuildsReject:       'Pages.Builds.Reject',
  Moderation:         'Pages.Moderation',
  ModerationView:     'Pages.Moderation.View',
  ModerationReview:   'Pages.Moderation.Review',
  ModerationComplete: 'Pages.Moderation.Complete',
  Categories:         'Pages.Categories',
  CategoriesManage:   'Pages.Categories.Manage',
  Tags:               'Pages.Tags',
  TagsManage:         'Pages.Tags.Manage',
  Dashboard:          'Pages.Dashboard',
  DashboardView:      'Pages.Dashboard.View',
  FeatureFlags:       'Pages.Dashboard.FeatureFlags',
  AuditLog:           'Pages.Dashboard.AuditLog',
  Users:              'Pages.Users',
  UsersManage:        'Pages.Users.Manage',
  Gameplay:           'Pages.Gameplay',
  Leaderboard:        'Pages.Leaderboard'
} as const;
```

---

## 7. Permission Data Seeder

```csharp
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Threading;
using GameHub.Authorization;
using GameHub.Authorization.Roles;
using GameHub.Authorization.Users;
using GameHub.MultiTenancy;

namespace GameHub.Seeding;

public class PermissionDataSeeder : DataSeeder
{
    private readonly RoleManager _roleManager;
    private readonly UserManager _userManager;

    public PermissionDataSeeder(
        RoleManager roleManager,
        UserManager userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public override void Seed(DataSeedingContext context)
    {
        AsyncHelper.RunSync(() => SeedAsync(context));
    }

    private async Task SeedAsync(DataSeedingContext context)
    {
        await SeedRolesAsync();
        await AssignDefaultPermissionsAsync();
    }

    private async Task SeedRolesAsync()
    {
        await CreateRoleIfNotExistsAsync(
            StaticRoleNames.Tenants.Admin, "Admin");
        await CreateRoleIfNotExistsAsync(
            StaticRoleNames.Tenants.Moderator, "Moderator");
        await CreateRoleIfNotExistsAsync(
            StaticRoleNames.Tenants.Developer, "Developer");
        await CreateRoleIfNotExistsAsync(
            StaticRoleNames.Tenants.Player, "Player");
    }

    private async Task AssignDefaultPermissionsAsync()
    {
        // ── SuperAdmin gets everything ──
        var superAdminRole = await _roleManager.GetRoleByIdAsync(
            StaticRoleNames.Tenants.SuperAdmin);
        if (superAdminRole != null)
        {
            var allPermissions = new[]
            {
                GameHubPermissions.Games,
                GameHubPermissions.GamesView,
                GameHubPermissions.GamesCreate,
                GameHubPermissions.GamesEdit,
                GameHubPermissions.GamesDelete,
                GameHubPermissions.GamesPublish,
                GameHubPermissions.GamesSuspend,
                GameHubPermissions.Builds,
                GameHubPermissions.BuildsUpload,
                GameHubPermissions.BuildsView,
                GameHubPermissions.BuildsApprove,
                GameHubPermissions.BuildsReject,
                GameHubPermissions.Moderation,
                GameHubPermissions.ModerationView,
                GameHubPermissions.ModerationReview,
                GameHubPermissions.ModerationComplete,
                GameHubPermissions.Categories,
                GameHubPermissions.CategoriesManage,
                GameHubPermissions.Tags,
                GameHubPermissions.TagsManage,
                GameHubPermissions.Dashboard,
                GameHubPermissions.DashboardView,
                GameHubPermissions.FeatureFlags,
                GameHubPermissions.AuditLog,
                GameHubPermissions.Users,
                GameHubPermissions.UsersManage,
                GameHubPermissions.Gameplay,
                GameHubPermissions.Leaderboard
            };

            foreach (var permission in allPermissions)
            {
                await _roleManager.GrantPermissionAsync(superAdminRole, permission);
            }
        }

        // ── Admin ──
        var adminRole = await _roleManager.GetRoleByIdAsync(
            StaticRoleNames.Tenants.Admin);
        if (adminRole != null)
        {
            var adminPermissions = new[]
            {
                GameHubPermissions.Games,
                GameHubPermissions.GamesView,
                GameHubPermissions.GamesPublish,
                GameHubPermissions.GamesSuspend,
                GameHubPermissions.Builds,
                GameHubPermissions.BuildsView,
                GameHubPermissions.BuildsApprove,
                GameHubPermissions.BuildsReject,
                GameHubPermissions.Moderation,
                GameHubPermissions.ModerationView,
                GameHubPermissions.Categories,
                GameHubPermissions.CategoriesManage,
                GameHubPermissions.Tags,
                GameHubPermissions.TagsManage,
                GameHubPermissions.Dashboard,
                GameHubPermissions.DashboardView,
                GameHubPermissions.FeatureFlags,
                GameHubPermissions.AuditLog,
                GameHubPermissions.Users,
                GameHubPermissions.UsersManage,
                GameHubPermissions.Gameplay,
                GameHubPermissions.Leaderboard
            };

            foreach (var permission in adminPermissions)
            {
                await _roleManager.GrantPermissionAsync(adminRole, permission);
            }
        }

        // ── Moderator ──
        var moderatorRole = await _roleManager.GetRoleByIdAsync(
            StaticRoleNames.Tenants.Moderator);
        if (moderatorRole != null)
        {
            var moderatorPermissions = new[]
            {
                GameHubPermissions.GamesView,
                GameHubPermissions.Builds,
                GameHubPermissions.BuildsView,
                GameHubPermissions.BuildsApprove,
                GameHubPermissions.BuildsReject,
                GameHubPermissions.Moderation,
                GameHubPermissions.ModerationView,
                GameHubPermissions.ModerationReview,
                GameHubPermissions.ModerationComplete,
                GameHubPermissions.Gameplay,
                GameHubPermissions.Leaderboard
            };

            foreach (var permission in moderatorPermissions)
            {
                await _roleManager.GrantPermissionAsync(moderatorRole, permission);
            }
        }

        // ── Developer ──
        var developerRole = await _roleManager.GetRoleByIdAsync(
            StaticRoleNames.Tenants.Developer);
        if (developerRole != null)
        {
            var developerPermissions = new[]
            {
                GameHubPermissions.Games,
                GameHubPermissions.GamesView,
                GameHubPermissions.GamesCreate,
                GameHubPermissions.GamesEdit,
                GameHubPermissions.GamesDelete,
                GameHubPermissions.Builds,
                GameHubPermissions.BuildsUpload,
                GameHubPermissions.BuildsView,
                GameHubPermissions.Gameplay,
                GameHubPermissions.Leaderboard
            };

            foreach (var permission in developerPermissions)
            {
                await _roleManager.GrantPermissionAsync(developerRole, permission);
            }
        }

        // ── Player (default) ──
        var playerRole = await _roleManager.GetRoleByIdAsync(
            StaticRoleNames.Tenants.Player);
        if (playerRole != null)
        {
            var playerPermissions = new[]
            {
                GameHubPermissions.GamesView,
                GameHubPermissions.Gameplay,
                GameHubPermissions.Leaderboard
            };

            foreach (var permission in playerPermissions)
            {
                await _roleManager.GrantPermissionAsync(playerRole, permission);
            }
        }
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName, string displayName)
    {
        var role = await _roleManager.GetRoleByNameAsync(roleName);
        if (role == null)
        {
            role = new Role
            {
                Name = roleName,
                DisplayName = displayName,
                IsDefault = false,
                IsStatic = true
            };

            await _roleManager.CreateAsync(role);
        }
    }
}
```

Register in the module:

```csharp
// In GameHubCoreModule.ConfigureServices
Configuration.Authorization.Providers.Add<GameHubAuthorizationProvider>();
```

---

## 8. Permission Caching (Redis)

Permissions are cached per-user in Redis with a configurable TTL.

| Key Pattern | Value | TTL |
|---|---|---|
| `perm:{tenantId}:{userId}` | JSON array of granted permission strings | 10 min |

Cache is invalidated on:
- Role assignment change
- User role change
- Permission grant/revoke via admin

```csharp
// PermissionCacheInvalidator.cs
public class PermissionCacheInvalidator : IPermissionCacheInvalidator
{
    private readonly IDistributedCache _cache;

    public PermissionCacheInvalidator(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task InvalidateAsync(int? tenantId, long userId)
    {
        var key = $"perm:{tenantId}:{userId}";
        await _cache.RemoveAsync(key);
    }
}
```

---

## 9. Migration Data

Seed permissions via ABP migration system:

```csharp
// In a dedicated seed migration
public class GameHubPermissionSeedData : DataSeedContributor, ITransientDependency
{
    public override void Seed(DataSeedingContext context)
    {
        // Uses PermissionDataSeeder above
    }
}
```

The `PermissionDataSeeder` runs automatically on first database migration and on subsequent deployments.
