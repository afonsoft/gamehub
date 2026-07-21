# Angular Modules - EAF Angular UI Template

## Module Hierarchy

The EAF Angular UI template follows a hierarchical module structure based on lazy loading for optimal performance:

```
RootModule (Bootstrap)
  -> AppModule (Main Application)
       -> AdminModule (Administration features - lazy loaded)
       -> MainModule (Business features - lazy loaded)
       -> SharedModule (Common components, directives, pipes)
  -> AccountModule (Login, Registration, Password Reset)
```

## Root Module

**Location**: `src/root.module.ts`

The RootModule is the entry point of the application. It:

- Bootstraps the Angular application
- Configures global providers and services
- Sets up routing and lazy loading
- Initializes EAF framework integration
- Configures SignalR connections
- Sets up localization

## App Module

**Location**: `src/app/app.module.ts`

The AppModule is the main application module that:

- Imports SharedModule for shared components
- Configures lazy loading for AdminModule and MainModule
- Sets up application-wide routing
- Provides core services
- Configures global error handling

### Lazy Loading Configuration

```typescript
const routes: Routes = [
  {
    path: 'account',
    loadChildren: () => import('./account/account.module').then(m => m.AccountModule)
  },
  {
    path: '',
    loadChildren: () => import('./app/app.module').then(m => m.AppModule),
    canActivate: [AppRouteGuard]
  }
];
```

## Account Module

**Location**: `src/account/account.module.ts`

The AccountModule handles authentication-related features:

- Login and logout
- Password reset (forgot/reset)
- Email activation
- External authentication (Google, Microsoft, Auth0, OpenID Connect)
- Multi-tenancy selection on login

### Components

- `LoginComponent` - Main login page
- `ForgotPasswordComponent` - Password reset request
- `ResetPasswordComponent` - Password reset with code
- `EmailActivationComponent` - Email activation request
- `ConfirmEmailComponent` - Email confirmation
- `SsoComponent` - Single Sign-On callback handler

### Services

- `LoginService` - Authentication logic
- `AccountServiceProxy` - Account API calls

## Admin Module

**Location**: `src/app/admin/admin.module.ts`

The AdminModule provides administrative features and is lazy-loaded for performance:

### Sub-Modules

#### Audit Logs Module

**Location**: `src/app/admin/audit-logs/`

- `AuditLogsComponent` - View audit logs with filtering
- `AuditLogDetailModalComponent` - Detailed log view

#### Hangfire Module

**Location**: `src/app/admin/hangfire/`

- `HangfireComponent` - Embedded Hangfire dashboard for background jobs

#### Languages Module

**Location**: `src/app/admin/languages/`

- `LanguagesComponent` - Language management
- `LanguageTextsComponent` - Translation text management
- `CreateOrEditLanguageModalComponent` - Add/edit languages
- `EditTextModalComponent` - Edit individual translations

#### Maintenance Module

**Location**: `src/app/admin/maintenance/`

- `MaintenanceComponent` - Cache clearing and system maintenance

#### Roles Module

**Location**: `src/app/admin/roles/`

- `RolesComponent` - Role listing with search
- `CreateOrEditRoleModalComponent` - Create/edit roles with permissions

#### Settings Module

**Location**: `src/app/admin/settings/`

- `SettingsComponent` - Host-level application settings
  - Email settings
  - User management settings
  - Security settings
  - Azure Active Directory integration
  - LDAP integration
  - External login providers
  - OpenID Connect claims mapping
  - Timezone management

#### Shared Admin Module

**Location**: `src/app/admin/shared/`

- `PermissionTreeComponent` - Hierarchical permission selection
- `PermissionComboComponent` - Permission dropdown
- `RoleComboComponent` - Role dropdown
- `FeatureTreeComponent` - Feature selection tree

#### Tenants Module

**Location**: `src/app/admin/tenants/`

- `TenantsComponent` - Tenant listing with CRUD
- `CreateTenantModalComponent` - New tenant creation
- `EditTenantModalComponent` - Edit existing tenant
- `TenantFeaturesModalComponent` - Manage tenant features

#### UI Customization Module

**Location**: `src/app/admin/ui-customization/`

- `UiCustomizationComponent` - Theme selection and customization
- Theme-specific settings components (default, theme2, theme3, theme4)

#### Users Module

**Location**: `src/app/admin/users/`

- `UsersComponent` - User listing with search and pagination
- `CreateOrEditUserModalComponent` - Create/edit user with role assignment
- `EditUserPermissionsModalComponent` - Granular permission management
- `ImpersonationService` - User impersonation across tenants

## Main Module

**Location**: `src/app/main/main.module.ts`

The MainModule contains business-specific features and is lazy-loaded:

### Sub-Modules

#### Dashboard Module

**Location**: `src/app/main/dashboard/`

- `DashboardComponent` - Main dashboard with statistics and widgets

#### Sample Entity Module

**Location**: `src/app/main/airplanes/`

- `AirplanesComponent` - Sample CRUD entity (Airplanes)
- `CreateOrEditAirplaneModalComponent` - Create/edit airplane
- Demonstrates CRUD patterns for custom entities

### Shared App Module

**Location**: `src/app/shared/`

#### Common Module

**Location**: `src/app/shared/common/`

- Authentication components
- Localization helpers
- Timing utilities
- Entity history components

#### Layout Module

**Location**: `src/app/shared/layout/`

- `TopbarComponent` - Top navigation bar
- `SideBarMenuComponent` - Side navigation menu
- `TopBarMenuComponent` - Top navigation menu
- `TitlebarComponent` - Page title and breadcrumbs
- `AdminBarComponent` - Admin quick access bar
- `ChatBarComponent` - Chat sidebar
- `NotificationsComponent` - Notification center
- Profile modals (password change, profile picture, settings)

#### Theme Modules

**Location**: `src/app/shared/layout/themes/`

- `DefaultThemeModule` - Default theme layout
- `Theme2Module` - Alternative theme 2
- `Theme3Module` - Alternative theme 3
- `Theme4Module` - Alternative theme 4

## Shared Module

**Location**: `src/shared/shared.module.ts`

The SharedModule provides components, directives, pipes, and services shared across all modules:

### Animations

**Location**: `src/shared/animations/`

- Route transition animations
- Component animations

### Common

**Location**: `src/shared/common/`

- `AppComponentBase` - Base class for all components
- `AppSessionService` - Session management
- `AppAuthService` - Authentication service
- `PermissionCheckerService` - Permission checking
- `AppLocalizationService` - Localization service
- `FeatureCheckerService` - Feature checking
- `AppUiCustomizationService` - UI customization
- `DateTimeService` - Date/time utilities
- `NotifyService` - Notification service
- `ModalHelper` - Modal helper utilities

### Helpers

**Location**: `src/shared/helpers/`

- `UrlHelper` - URL utilities
- `DomHelper` - DOM manipulation
- `DataTableHelper` - PrimeNG DataTable helpers
- `FileDownloadService` - File download utilities

### Service Proxies

**Location**: `src/shared/service-proxies/`

- Auto-generated TypeScript API clients via NSwag
- All backend API service proxies

### Utils

**Location**: `src/shared/utils/`

- Directives (busyIf, buttonBusy, autoFocus, etc.)
- Pipes (localize, momentFormat, momentFromNow, etc.)
- Validators (equalValidator, passwordComplexity)
- Services (LocalStorageService, ScriptLoaderService, StyleLoaderService)

## Module Dependencies

### Import Order

When creating new modules, follow this import order:

1. Angular core imports
2. Third-party library imports (PrimeNG, ngx-bootstrap)
3. EAF framework imports
4. Application-specific imports
5. Components
6. Services
7. Pipes
7. Directives

### Shared Module Pattern

The SharedModule should only contain:

- Components that are used in multiple modules
- Pipes that are used in multiple modules
- Directives that are used in multiple modules
- Services that are singleton across the application

Do NOT include:

- Components specific to a single module
- Business logic specific to a single module

## Creating New Modules

### Step 1: Generate Module

```bash
ng generate module modules/your-module --routing
```

### Step 2: Configure Lazy Loading

Add to parent module routing:

```typescript
{
  path: 'your-module',
  loadChildren: () => import('./modules/your-module/your-module.module')
    .then(m => m.YourModuleModule)
}
```

### Step 3: Extend AppComponentBase

Make your components extend `AppComponentBase`:

```typescript
export class YourComponent extends AppComponentBase {
  constructor(
    injector: Injector
  ) {
    super(injector);
  }
}
```

### Step 4: Use Service Proxies

Import and use auto-generated service proxies:

```typescript
import { YourEntityServiceProxy } from '@shared/service-proxies/service-proxies';

export class YourComponent extends AppComponentBase {
  constructor(
    injector: Injector,
    private _yourEntityService: YourEntityServiceProxy
  ) {
    super(injector);
  }
}
```

## Module Best Practices

1. **Lazy Load Feature Modules**: Always lazy load feature modules for better performance
2. **Keep Modules Focused**: Each module should have a single responsibility
3. **Use SharedModule**: Put shared components in SharedModule
4. **Follow Naming Conventions**: Use kebab-case for files, PascalCase for classes
5. **Separate Concerns**: Keep components, services, and pipes in separate folders
6. **Use Guards**: Protect routes with route guards
7. **Configure Routing**: Use forChild() for feature modules
