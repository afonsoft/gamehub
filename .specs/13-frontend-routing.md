# 13 — Frontend Routing

> **Status:** Draft  
> **Stack:** Angular 20+ · Two SPAs · Angular Router  
> **DNS:** gamehub.afonsoft.dev (hub) · gamehub-admin.afonsoft.dev (admin)

---

## Part A — Game Hub (`angular/`)

### 1. Complete Route Table

| Route | Component | Module | Guard(s) | Description |
|---|---|---|---|---|
| `/` | `HomePageComponent` | PublicModule | none | Home with highlights, new, trending |
| `/games` | `CatalogPageComponent` | PublicModule | none | Catalog with filters |
| `/games/:slug` | `GameDetailPageComponent` | PublicModule | none | Game detail page |
| `/search` | `SearchPageComponent` | PublicModule | none | Search results |
| `/play/:slug` | `GameShellComponent` | PlayerModule | AuthGuard | Game player shell |
| `/leaderboard/:gameId` | `LeaderboardComponent` | PlayerModule | none | Game leaderboard |
| `/developer` | `DeveloperDashboardComponent` | DeveloperModule | AuthGuard + DeveloperGuard | Developer dashboard |
| `/developer/games` | `DeveloperGamesComponent` | DeveloperModule | AuthGuard + DeveloperGuard | Game list |
| `/developer/games/create` | `GameCreateComponent` | DeveloperModule | AuthGuard + DeveloperGuard | Create game |
| `/developer/games/:id/edit` | `GameEditComponent` | DeveloperModule | AuthGuard + DeveloperGuard | Edit game |
| `/developer/games/:id/builds` | `BuildListComponent` | DeveloperModule | AuthGuard + DeveloperGuard | Build history |
| `/developer/profile` | `DeveloperProfileComponent` | DeveloperModule | AuthGuard | Dev profile |
| `/login` | `LoginPageComponent` | PublicModule | GuestGuard | Login |
| `/register` | `RegisterPageComponent` | PublicModule | GuestGuard | Register |
| `/**` | `NotFoundComponent` | PublicModule | none | 404 |

### 2. Route Parameters

| Parameter | Type | Location | Notes |
|---|---|---|---|
| `:slug` | `string` | `/games/:slug`, `/play/:slug` | URL-safe game slug |
| `:id` | `uuid` | `/developer/games/:id/edit`, `/developer/games/:id/builds` | Game GUID |
| `:gameId` | `uuid` | `/leaderboard/:gameId` | Game GUID |
| `returnUrl` | `string` | query param on `/login` | Redirect target after login |

### 3. Resolvers

```typescript
// angular/src/app/public/resolvers/game-detail.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { GameService } from '../../core/services/game.service';
import { GameDetailDto } from '../../core/models/game.model';

export const GameDetailResolver: ResolveFn<GameDetailDto> = (route) => {
  const gameService = inject(GameService);
  return gameService.getBySlug(route.paramMap.get('slug')!);
};

// angular/src/app/developer/resolvers/build-list.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { BuildService } from '../../core/services/build.service';
import { BuildDto } from '../../core/models/build.model';

export const BuildListResolver: ResolveFn<BuildDto[]> = (route) => {
  const buildService = inject(BuildService);
  return buildService.listByGame(route.paramMap.get('id')!);
};
```

### 4. Lazy Loading Configuration

```typescript
// angular/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { DeveloperGuard } from './core/guards/developer.guard';
import { GameHubPermissions } from './core/constants/permissions';
import { PermissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  // ── Public routes (eager or lazy) ──
  {
    path: '',
    loadChildren: () =>
      import('./public/public.routes').then(m => m.PUBLIC_ROUTES)
  },

  // ── Player routes ──
  {
    path: 'play',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./player/player.routes').then(m => m.PLAYER_ROUTES)
  },
  {
    path: 'leaderboard',
    loadChildren: () =>
      import('./player/player.routes').then(m => m.PLAYER_ROUTES)
  },

  // ── Developer routes ──
  {
    path: 'developer',
    canActivate: [AuthGuard, DeveloperGuard],
    loadChildren: () =>
      import('./developer/developer.routes').then(m => m.DEVELOPER_ROUTES)
  },

  // ── Auth routes ──
  {
    path: 'login',
    canActivate: [GuestGuard],
    loadChildren: () =>
      import('./auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'register',
    canActivate: [GuestGuard],
    loadChildren: () =>
      import('./auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  // ── 404 ──
  {
    path: '**',
    loadChildren: () =>
      import('./public/public.routes').then(m => m.NOT_FOUND_ROUTE)
  }
];
```

```typescript
// angular/src/app/public/public.routes.ts
import { Routes } from '@angular/router';
import { GameDetailResolver } from './resolvers/game-detail.resolver';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home-page/home-page.component')
        .then(m => m.HomePageComponent)
  },
  {
    path: 'games',
    loadComponent: () =>
      import('./pages/catalog-page/catalog-page.component')
        .then(m => m.CatalogPageComponent)
  },
  {
    path: 'games/:slug',
    resolve: { game: GameDetailResolver },
    loadComponent: () =>
      import('./pages/game-detail-page/game-detail-page.component')
        .then(m => m.GameDetailPageComponent)
  },
  {
    path: 'search',
    loadComponent: () =>
      import('./pages/search-page/search-page.component')
        .then(m => m.SearchPageComponent)
  }
];

export const NOT_FOUND_ROUTE: Routes = [
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component')
        .then(m => m.NotFoundComponent)
  }
];
```

```typescript
// angular/src/app/developer/developer.routes.ts
import { Routes } from '@angular/router';
import { BuildListResolver } from './resolvers/build-list.resolver';

export const DEVELOPER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/developer-dashboard/developer-dashboard.component')
        .then(m => m.DeveloperDashboardComponent)
  },
  {
    path: 'games',
    loadComponent: () =>
      import('./pages/developer-games/developer-games.component')
        .then(m => m.DeveloperGamesComponent)
  },
  {
    path: 'games/create',
    loadComponent: () =>
      import('./pages/game-create/game-create.component')
        .then(m => m.GameCreateComponent)
  },
  {
    path: 'games/:id/edit',
    loadComponent: () =>
      import('./pages/game-edit/game-edit.component')
        .then(m => m.GameEditComponent)
  },
  {
    path: 'games/:id/builds',
    resolve: { builds: BuildListResolver },
    loadComponent: () =>
      import('./pages/build-list/build-list.component')
        .then(m => m.BuildListComponent)
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./pages/developer-profile/developer-profile.component')
        .then(m => m.DeveloperProfileComponent)
  }
];
```

```typescript
// angular/src/app/player/player.routes.ts
import { Routes } from '@angular/router';

export const PLAYER_ROUTES: Routes = [
  {
    path: 'play/:slug',
    loadComponent: () =>
      import('./pages/game-shell/game-shell.component')
        .then(m => m.GameShellComponent)
  },
  {
    path: ':gameId',
    loadComponent: () =>
      import('./pages/leaderboard/leaderboard.component')
        .then(m => m.LeaderboardComponent)
  }
];
```

```typescript
// angular/src/app/auth/auth.routes.ts
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component')
        .then(m => m.LoginPageComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register.component')
        .then(m => m.RegisterPageComponent)
  }
];
```

### 5. Guard Implementations

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

```typescript
// angular/src/app/core/guards/guest.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const GuestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/']);
};
```

```typescript
// angular/src/app/core/guards/developer.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const DeveloperGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.hasRole('Developer') ||
      authService.hasRole('Admin') ||
      authService.hasRole('SuperAdmin')) {
    return true;
  }

  return router.createUrlTree(['/']);
};
```

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

### 6. Navigation Flow Diagrams

#### Public user flow

```
┌─────────────────────────────────────────────────────────────────┐
│                          HOME (/)                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │Highlights │  │ New Games│  │ Most Played│ │ Trending │       │
│  └────┬─────┘  └────┬─────┘  └────┬──────┘ └────┬─────┘       │
│       │              │              │              │             │
│       ▼              ▼              ▼              ▼             │
│  /games/:slug   /games/:slug  /games/:slug  /games/:slug       │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                     CATALOG (/games)                             │
│  Filters: Category | Tag | Device | Orientation | Sort          │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐       │
│  │Game 1│ │Game 2│ │Game 3│ │Game 4│ │Game 5│ │Game 6│       │
│  └──┬───┘ └──┬───┘ └──┬───┘ └──┬───┘ └──┬───┘ └──┬───┘       │
└─────┼────────┼────────┼────────┼────────┼────────┼─────────────┘
      └────────┴────────┴────────┴────────┴────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                 GAME DETAIL (/games/:slug)                      │
│  ┌─────────────┐  ┌──────────────────────┐                     │
│  │ Hero Image  │  │ Title / Description  │                     │
│  │             │  │ Instructions         │                     │
│  │  [PLAY NOW] │  │ Categories / Tags    │                     │
│  └─────────────┘  │ Developer Info       │                     │
│                   │ Related Games        │                     │
│                   └──────────────────────┘                     │
└─────────────────────────────────────────────────────────────────┘
                           │
                    [PLAY NOW]
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                  GAME SHELL (/play/:slug)                        │
│  ┌─────────────────────────────────────────────────┐            │
│  │                                                 │            │
│  │              Game iframe                        │            │
│  │                                                 │            │
│  │   <iframe src="https://games.afonsoft.dev/..." │            │
│  │     sandbox="allow-scripts allow-pointer-lock  │            │
│  │              allow-same-origin allow-forms"     │            │
│  │     allow="fullscreen; gamepad">                │            │
│  │                                                 │            │
│  └─────────────────────────────────────────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

#### Developer flow

```
┌─────────────────────────────────────────────────────────────────┐
│                  DEVELOPER DASHBOARD (/developer)                │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                     │
│  │My Games  │  │ Upload   │  │ Profile  │                     │
│  │Count     │  │ Build    │  │          │                     │
│  └────┬─────┘  └────┬─────┘  └──────────┘                     │
└───────┼──────────────┼──────────────────────────────────────────┘
        │              │
        ▼              ▼
┌───────────────┐  ┌───────────────────────────────────────────┐
│ MY GAMES      │  │ CREATE GAME (/developer/games/create)     │
│ /developer/   │  │                                           │
│ games         │  │  Title, Description, Categories, Tags     │
│ ┌───────────┐ │  │  Age Rating, Orientation, Platform Flags  │
│ │ Game 1    │ │  │  [Save Draft] [Submit for Review]         │
│ │ Edit│Build │ │  └───────────────────────────────────────────┘
│ ├───────────┤ │                      │
│ │ Game 2    │ │              [Submit for Review]
│ │ Edit│Build │ │                      │
│ └───────────┘ │                      ▼
└───────────────┘  ┌───────────────────────────────────────────┐
                   │ REVIEW QUEUE (admin/moderator)            │
                   │ Build is now pending moderation            │
                   └───────────────────────────────────────────┘
```

---

## Part B — Admin (`angular-admin/`)

### 1. Complete Route Table

| Route | Component | Guard(s) | Description |
|---|---|---|---|
| `/login` | `LoginPageComponent` | GuestGuard | Admin login |
| `/` | redirect → `/games` | AuthGuard + AdminGuard | Default |
| `/games` | `GameListComponent` | AuthGuard + AdminGuard | All games |
| `/games/:id` | `GameDetailComponent` | AuthGuard + AdminGuard | Game detail |
| `/games/:id/edit` | `GameEditComponent` | AuthGuard + AdminGuard | Edit game |
| `/moderation` | `ReviewQueueComponent` | AuthGuard + ModeratorGuard | Pending reviews |
| `/moderation/:id` | `ReviewDetailComponent` | AuthGuard + ModeratorGuard | Review detail |
| `/categories` | `CategoryListComponent` | AuthGuard + AdminGuard | Categories |
| `/categories/create` | `CategoryEditComponent` | AuthGuard + AdminGuard | Create category |
| `/categories/:id/edit` | `CategoryEditComponent` | AuthGuard + AdminGuard | Edit category |
| `/tags` | `TagListComponent` | AuthGuard + AdminGuard | Tags |
| `/tags/create` | `TagEditComponent` | AuthGuard + AdminGuard | Create tag |
| `/tags/:id/edit` | `TagEditComponent` | AuthGuard + AdminGuard | Edit tag |
| `/dashboard` | `DashboardComponent` | AuthGuard + AdminGuard | Metrics |
| `/dashboard/flags` | `FeatureFlagsComponent` | AuthGuard + AdminGuard | Feature flags |
| `/dashboard/audit` | `AuditLogComponent` | AuthGuard + AdminGuard | Audit log |
| `/**` | `NotFoundComponent` | none | 404 |

### 2. Route Parameters

| Parameter | Type | Location | Notes |
|---|---|---|---|
| `:id` | `uuid` | `/games/:id`, `/moderation/:id`, `/categories/:id/edit`, `/tags/:id/edit` | Entity GUID |
| `page` | `number` | query param on list routes | Pagination page index |
| `pageSize` | `number` | query param on list routes | Items per page |
| `status` | `string` | query param on `/games` | Filter by game status |
| `search` | `string` | query param on list routes | Search term |

### 3. Resolvers

```typescript
// angular-admin/src/app/resolvers/game-detail.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { AdminGameService } from '../services/admin-game.service';
import { AdminGameDetailDto } from '../models/game.model';

export const GameDetailResolver: ResolveFn<AdminGameDetailDto> = (route) => {
  const service = inject(AdminGameService);
  return service.getById(route.paramMap.get('id')!);
};

// angular-admin/src/app/resolvers/moderation-detail.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { ModerationService } from '../services/moderation.service';
import { ModerationReviewDto } from '../models/moderation.model';

export const ModerationDetailResolver: ResolveFn<ModerationReviewDto> = (route) => {
  const service = inject(ModerationService);
  return service.getReviewById(route.paramMap.get('id')!);
};

// angular-admin/src/app/resolvers/category-edit.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { CategoryService } from '../services/category.service';
import { CategoryDto } from '../models/category.model';

export const CategoryEditResolver: ResolveFn<CategoryDto | null> = (route) => {
  const id = route.paramMap.get('id');
  if (!id) return null;
  const service = inject(CategoryService);
  return service.getById(id);
};

// angular-admin/src/app/resolvers/tag-edit.resolver.ts
import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { TagService } from '../services/tag.service';
import { TagDto } from '../models/tag.model';

export const TagEditResolver: ResolveFn<TagDto | null> = (route) => {
  const id = route.paramMap.get('id');
  if (!id) return null;
  const service = inject(TagService);
  return service.getById(id);
};
```

### 4. Lazy Loading Configuration

```typescript
// angular-admin/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { ModeratorGuard } from './core/guards/moderator.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [GuestGuard],
    loadComponent: () =>
      import('./pages/login/login.component')
        .then(m => m.LoginPageComponent)
  },
  {
    path: '',
    canActivate: [AuthGuard, AdminGuard],
    children: [
      {
        path: '',
        redirectTo: 'games',
        pathMatch: 'full'
      },
      {
        path: 'games',
        loadComponent: () =>
          import('./pages/game-list/game-list.component')
            .then(m => m.GameListComponent)
      },
      {
        path: 'games/:id',
        resolve: { game: GameDetailResolver },
        loadComponent: () =>
          import('./pages/game-detail/game-detail.component')
            .then(m => m.GameDetailComponent)
      },
      {
        path: 'games/:id/edit',
        resolve: { game: GameDetailResolver },
        loadComponent: () =>
          import('./pages/game-edit/game-edit.component')
            .then(m => m.GameEditComponent)
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./pages/category-list/category-list.component')
            .then(m => m.CategoryListComponent)
      },
      {
        path: 'categories/create',
        loadComponent: () =>
          import('./pages/category-edit/category-edit.component')
            .then(m => m.CategoryEditComponent)
      },
      {
        path: 'categories/:id/edit',
        resolve: { category: CategoryEditResolver },
        loadComponent: () =>
          import('./pages/category-edit/category-edit.component')
            .then(m => m.CategoryEditComponent)
      },
      {
        path: 'tags',
        loadComponent: () =>
          import('./pages/tag-list/tag-list.component')
            .then(m => m.TagListComponent)
      },
      {
        path: 'tags/create',
        loadComponent: () =>
          import('./pages/tag-edit/tag-edit.component')
            .then(m => m.TagEditComponent)
      },
      {
        path: 'tags/:id/edit',
        resolve: { tag: TagEditResolver },
        loadComponent: () =>
          import('./pages/tag-edit/tag-edit.component')
            .then(m => m.TagEditComponent)
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component')
            .then(m => m.DashboardComponent)
      },
      {
        path: 'dashboard/flags',
        loadComponent: () =>
          import('./pages/feature-flags/feature-flags.component')
            .then(m => m.FeatureFlagsComponent)
      },
      {
        path: 'dashboard/audit',
        loadComponent: () =>
          import('./pages/audit-log/audit-log.component')
            .then(m => m.AuditLogComponent)
      }
    ]
  },
  // ── 404 ──
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component')
        .then(m => m.NotFoundComponent)
  }
];
```

### 5. Guard Implementations

```typescript
// angular-admin/src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
```

```typescript
// angular-admin/src/app/core/guards/guest.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const GuestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/games']);
};
```

```typescript
// angular-admin/src/app/core/guards/admin.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.hasRole('Admin') || authService.hasRole('SuperAdmin')) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
```

```typescript
// angular-admin/src/app/core/guards/moderator.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const ModeratorGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (
    authService.hasRole('Moderator') ||
    authService.hasRole('Admin') ||
    authService.hasRole('SuperAdmin')
  ) {
    return true;
  }

  return router.createUrlTree(['/games']);
};
```

### 6. Navigation Sidebar Structure

```typescript
// angular-admin/src/app/core/models/sidebar.model.ts
export interface SidebarItem {
  label: string;
  icon: string;
  route?: string;
  permission?: string;
  children?: SidebarItem[];
}

export const SIDEBAR_ITEMS: SidebarItem[] = [
  {
    label: 'Games',
    icon: 'gamepad',
    route: '/games',
    permission: 'Pages.Games.View'
  },
  {
    label: 'Moderation',
    icon: 'shield',
    route: '/moderation',
    permission: 'Pages.Moderation.View'
  },
  {
    label: 'Categories',
    icon: 'folder',
    children: [
      {
        label: 'All Categories',
        icon: 'list',
        route: '/categories',
        permission: 'Pages.Categories.Manage'
      },
      {
        label: 'Create Category',
        icon: 'plus',
        route: '/categories/create',
        permission: 'Pages.Categories.Manage'
      }
    ]
  },
  {
    label: 'Tags',
    icon: 'tag',
    children: [
      {
        label: 'All Tags',
        icon: 'list',
        route: '/tags',
        permission: 'Pages.Tags.Manage'
      },
      {
        label: 'Create Tag',
        icon: 'plus',
        route: '/tags/create',
        permission: 'Pages.Tags.Manage'
      }
    ]
  },
  {
    label: 'Dashboard',
    icon: 'chart-bar',
    children: [
      {
        label: 'Overview',
        icon: 'home',
        route: '/dashboard',
        permission: 'Pages.Dashboard.View'
      },
      {
        label: 'Feature Flags',
        icon: 'flag',
        route: '/dashboard/flags',
        permission: 'Pages.Dashboard.FeatureFlags'
      },
      {
        label: 'Audit Log',
        icon: 'scroll',
        route: '/dashboard/audit',
        permission: 'Pages.Dashboard.AuditLog'
      }
    ]
  }
];
```

### 7. Admin Sidebar Component

```typescript
// angular-admin/src/app/core/components/sidebar/sidebar.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SidebarItem, SIDEBAR_ITEMS } from '../../models/sidebar.model';

@Component({
  selector: 'gh-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="sidebar">
      <div class="sidebar-brand">
        <a routerLink="/">
          <img src="assets/logo-admin.svg" alt="GameHub Admin" />
        </a>
      </div>

      <ul class="sidebar-nav">
        @for (item of filteredItems; track item.label) {
          <li class="sidebar-item" [class.has-children]="item.children?.length">
            @if (item.children?.length) {
              <span class="sidebar-label">
                <i class="icon-{{ item.icon }}"></i>
                {{ item.label }}
              </span>
              <ul class="sidebar-subnav">
                @for (child of item.children; track child.label) {
                  <li>
                    <a [routerLink]="child.route"
                       routerLinkActive="active">
                      <i class="icon-{{ child.icon }}"></i>
                      {{ child.label }}
                    </a>
                  </li>
                }
              </ul>
            } @else {
              <a [routerLink]="item.route"
                 routerLinkActive="active">
                <i class="icon-{{ item.icon }}"></i>
                {{ item.label }}
              </a>
            }
          </li>
        }
      </ul>

      <div class="sidebar-footer">
        <span class="admin-user">
          {{ authService.currentUser?.name }}
        </span>
        <button (click)="authService.logout()">Logout</button>
      </div>
    </nav>
  `,
  styles: [`
    .sidebar { width: 240px; background: #1a1a2e; color: #eee; }
    .sidebar-nav { list-style: none; padding: 0; }
    .sidebar-subnav { list-style: none; padding-left: 16px; }
    .sidebar-subnav a { color: #ccc; }
    .sidebar-subnav a.active { color: #4fc3f7; }
  `]
})
export class SidebarComponent implements OnInit {
  filteredItems: SidebarItem[] = [];

  constructor(public authService: AuthService) {}

  ngOnInit() {
    this.filteredItems = SIDEBAR_ITEMS.filter(item =>
      this.authService.hasPermission(item.permission)
    );
  }
}
```
