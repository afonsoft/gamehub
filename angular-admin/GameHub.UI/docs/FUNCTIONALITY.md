# EAF Angular UI Template - Functionality Documentation

## Overview

The **EAF (Enterprise Application Framework) Angular UI Template** is a full-featured Single Page Application (SPA) built with **Angular 18** and **TypeScript 5.2**. It serves as a starter template for enterprise applications, providing out-of-the-box functionality for authentication, authorization, multi-tenancy, administration, real-time communication, and theming.

---

## Table of Contents

1. [Architecture](#architecture)
2. [Directory Structure](#directory-structure)
3. [Authentication & Authorization](#authentication--authorization)
4. [Multi-Tenancy](#multi-tenancy)
5. [Admin Panel](#admin-panel)
6. [Theme System](#theme-system)
7. [Chat & Real-Time Communication](#chat--real-time-communication)
8. [Notifications](#notifications)
9. [Navigation & Layout](#navigation--layout)
10. [Shared Utilities](#shared-utilities)
11. [Service Proxies](#service-proxies)
12. [Localization](#localization)
13. [Configuration](#configuration)

---

## Architecture

### Core Stack
- **Framework**: Angular 18.0.0
- **Language**: TypeScript 5.2
- **UI Libraries**: PrimeNG (tables, paginator, file upload), ngx-bootstrap (modals, tabs, dropdowns), Metronic Admin Theme
- **State Management**: Services + RxJS Observables
- **Real-Time**: SignalR (@microsoft/signalr)
- **HTTP**: Angular HttpClient with NSwag-generated service proxies
- **Testing**: Karma + Jasmine
- **Styling**: SCSS with multiple theme support

### Module Hierarchy
```
RootModule (Bootstrap)
  -> AppModule (Main Application)
       -> AdminModule (Administration features - lazy loaded)
       -> MainModule (Business features - lazy loaded)
       -> SharedModule (Common components, directives, pipes)
  -> AccountModule (Login, Registration, Password Reset)
```

### Base Classes
- **`AppComponentBase`** (`src/shared/common/app-component-base.ts`): Abstract base class for all components. Provides:
  - Localization (`l()`, `ls()` methods)
  - Permission checking (`isGranted()`, `isGrantedAny()`)
  - Feature checking
  - Notification services
  - Multi-tenancy services
  - Session management
  - UI customization access
  - Data table helper

---

## Directory Structure

```
src/
├── account/                    # Account module (login, registration, password)
│   ├── login/                  # Login component and service
│   ├── password/               # Forgot/Reset password components
│   └── email-activation/       # Email activation and confirmation
├── app/                        # Main application module
│   ├── admin/                  # Admin panel (lazy loaded)
│   │   ├── audit-logs/         # Audit log viewer
│   │   ├── hangfire/           # Background job dashboard
│   │   ├── languages/          # Language management
│   │   ├── maintenance/        # System maintenance (cache)
│   │   ├── roles/              # Role management (CRUD)
│   │   ├── settings/           # Host settings
│   │   ├── shared/             # Shared admin components (permission tree, role combo)
│   │   ├── tenants/            # Tenant management (CRUD)
│   │   ├── ui-customization/   # Theme customization
│   │   └── users/              # User management (CRUD, impersonation)
│   ├── main/                   # Main business module (lazy loaded)
│   │   ├── dashboard/          # Dashboard component
│   │   └── airplanes/          # Sample CRUD entity (Airplanes)
│   └── shared/                 # Shared app components
│       ├── common/             # Auth, localization, timing, entity history
│       └── layout/             # Layout components (topbar, sidebar, chat, notifications, profile)
├── shared/                     # Shared across all modules
│   ├── animations/             # Route transition animations
│   ├── common/                 # Base classes, pipes, session, UI customization
│   ├── helpers/                # URL, DOM, DataTable helpers
│   ├── service-proxies/        # NSwag-generated API client proxies
│   └── utils/                  # Utility directives, pipes, services
└── root.module.ts              # Application bootstrap module
```

---

## Authentication & Authorization

### Login Flow (`src/account/login/`)
- **`LoginComponent`**: Main login page with username/password form
  - Multi-tenancy aware (tenant selection)
  - reCAPTCHA integration
  - Remember me functionality
  - External login provider buttons (Google, Microsoft, Auth0, OpenID Connect)

- **`LoginService`**: Core authentication service
  - Standard authentication via `TokenAuthServiceProxy.authenticate()`
  - External authentication providers:
    - **Google**: OAuth2 via `gapi.auth2`
    - **Microsoft**: MSAL (Microsoft Authentication Library) with popup/redirect modes
    - **Auth0**: Auth0 SPA SDK with popup/redirect modes
    - **OpenID Connect**: Generic OIDC via `angular-oauth2-oidc`
  - Token management (access token, encrypted token, expiration)
  - SSO callback handling for each provider
  - Password reset redirection

- **`SsoComponent`**: Single Sign-On callback handler

### Token Management
- Tokens stored in cookies via `StorageService`
- Encrypted access token stored separately
- Token expiration tracked with configurable duration
- Remember me extends token lifetime (10x normal expiration)

### Password Management (`src/account/password/`)
- **`ForgotPasswordComponent`**: Send password reset emails
- **`ResetPasswordComponent`**: Reset password with validation code

### Email Activation (`src/account/email-activation/`)
- **`EmailActivationComponent`**: Resend activation email
- **`ConfirmEmailComponent`**: Confirm email with activation code

### Authorization
- Permission-based access control via `PermissionCheckerService`
- Route guards for protected areas
- `isGranted()` and `isGrantedAny()` methods in `AppComponentBase`

---

## Multi-Tenancy

### Implementation
- **`EafMultiTenancyService`**: Core multi-tenancy service from EAF framework
- **Tenant Selection**: Available on the login page, users can switch tenants
- **Tenant Availability Check**: `AccountServiceProxy.isTenantAvailable()` validates tenant names
- **Tenant-Scoped Data**: All data is automatically filtered by tenant
- **Host vs Tenant**: Supports both host-level and tenant-level administration

### Key Features
- Tenant CRUD operations (create, edit, delete)
- Tenant feature management (enable/disable features per tenant)
- User impersonation across tenants
- Tenant admin unlock functionality
- Inter-tenant and tenant-to-host chat configuration

---

## Admin Panel

### User Management (`src/app/admin/users/`)
- **`UsersComponent`**: User listing with search, pagination, and filtering
  - PrimeNG DataTable with lazy loading
  - Export to Excel functionality
  - Entity history tracking
- **`CreateOrEditUserModalComponent`**: Create/edit user modal with role assignment
- **`EditUserPermissionsModalComponent`**: Granular permission management per user
- **`ImpersonationService`**: Login as another user (admin feature)
  - Impersonate users across tenants
  - Back-to-impersonator functionality

### Role Management (`src/app/admin/roles/`)
- **`RolesComponent`**: Role listing with search and entity history
- **`CreateOrEditRoleModalComponent`**: Create/edit roles with permission tree

### Tenant Management (`src/app/admin/tenants/`)
- **`TenantsComponent`**: Tenant listing with CRUD operations
- **`CreateTenantModalComponent`**: New tenant creation
- **`EditTenantModalComponent`**: Edit existing tenant
- **`TenantFeaturesModalComponent`**: Manage features per tenant

### Language Management (`src/app/admin/languages/`)
- **`LanguagesComponent`**: Language listing and management
- **`LanguageTextsComponent`**: Edit localization texts
- **`CreateOrEditLanguageModalComponent`**: Add/edit languages
- **`EditTextModalComponent`**: Edit individual translation keys

### Audit Logs (`src/app/admin/audit-logs/`)
- **`AuditLogsComponent`**: View application audit logs
  - Date range filtering
  - Filter by username, service, method, browser
  - Exception filtering
  - Execution duration filtering
  - Entity change tracking
  - Export to Excel
- **`AuditLogDetailModalComponent`**: Detailed audit log view

### Settings (`src/app/admin/settings/`)
- **`SettingsComponent`**: Host-level application settings
  - Email settings (SMTP configuration, test email)
  - User management settings (registration, login options)
  - Security settings (password complexity, lockout)
  - Azure Active Directory integration
  - LDAP integration
  - External login providers (Google, Microsoft, OpenID Connect, Auth0)
  - OpenID Connect claims mapping
  - Timezone management

### UI Customization (`src/app/admin/ui-customization/`)
- **`UiCustomizationComponent`**: Theme selection and customization
- Theme-specific settings components (default, theme2, theme3, theme4)

### Maintenance (`src/app/admin/maintenance/`)
- **`MaintenanceComponent`**: Cache clearing and system maintenance

### Hangfire (`src/app/admin/hangfire/`)
- **`HangfireComponent`**: Embedded Hangfire background job dashboard

### Shared Admin Components (`src/app/admin/shared/`)
- **`PermissionTreeComponent`**: Hierarchical permission selection tree
- **`PermissionComboComponent`**: Permission dropdown selector
- **`RoleComboComponent`**: Role dropdown selector
- **`FeatureTreeComponent`**: Feature selection tree for tenants

---

## Theme System

### Available Themes
The application supports 4 built-in themes, each with their own layout:

1. **Default Theme** (`src/app/shared/layout/themes/default/`)
   - `DefaultLayoutComponent`: Standard admin layout
   - `DefaultBrandComponent`: Brand/logo component
   
2. **Theme 2** (`src/app/shared/layout/themes/theme2/`)
   - `Theme2LayoutComponent`: Alternative layout
   - `Theme2BrandComponent`: Alternative brand

3. **Theme 3** (`src/app/shared/layout/themes/theme3/`)
   - `Theme3LayoutComponent`: Alternative layout
   - `Theme3BrandComponent`: Alternative brand

4. **Theme 4** (`src/app/shared/layout/themes/theme4/`)
   - `Theme4LayoutComponent`: Alternative layout
   - `Theme4BrandComponent`: Alternative brand

### Theme Configuration
- **`AppUiCustomizationService`**: Manages theme settings
- User-specific theme preferences stored via `AppConsts.themeUser`
- Theme settings include: aside skin, header skin, menu type (side/top)
- CSS classes dynamically applied based on theme selection

---

## Chat & Real-Time Communication

### Components (`src/app/shared/layout/chat/`)
- **`ChatBarComponent`**: Full-featured chat sidebar
  - Friend list management (add, block, unblock)
  - Real-time messaging via SignalR
  - Image and file upload support
  - Link sharing
  - Read/unread message tracking
  - Multi-tenancy aware (inter-tenant chat settings)
  - Persistent chat state (open/closed, pinned, selected user)
  - Previous message loading with pagination
  - User online/offline status tracking

- **`ChatSignalrService`**: SignalR hub connection management
  - Message sending/receiving
  - Friend request notifications
  - User connection state changes
  - Read state synchronization

- **`ChatFriendListItemComponent`**: Individual friend list entry
- **`ChatMessageComponent`**: Chat message bubble renderer
- **`ChatFriendDto`**: Extended friend model with message history

### Features
- One-to-one messaging
- File attachments (images, documents)
- Link sharing with preview
- User blocking/unblocking
- Unread message count badge
- Pinnable chat panel
- Cross-tenant chat support (configurable)

---

## Notifications

### Components (`src/app/shared/layout/notifications/`)
- **`NotificationsComponent`**: Full notification list page
  - Read/unread filtering
  - PrimeNG DataTable with pagination
  - Mark as read (individual and bulk)
  - Delete notifications
  - Notification settings management
  - Severity-based categorization (Info, Success, Warning, Error, Fatal)

- **`HeaderNotificationsComponent`**: Top bar notification bell
  - Unread count badge
  - Quick notification preview
  - Mark all as read

- **`NotificationSettingsModalComponent`**: Configure notification preferences

- **`UserNotificationHelper`**: Notification formatting and actions

---

## Navigation & Layout

### Top Bar (`src/app/shared/layout/topbar.component.ts`)
- User profile picture and name display
- Language switcher
- Chat toggle
- Notification bell
- User menu (profile, password, settings, logout)
- Impersonation indicator with "back to my account"
- Multi-tenancy info display

### Side Bar Menu (`src/app/shared/layout/nav/side-bar-menu.component.ts`)
- Hierarchical navigation menu
- Permission-based menu item visibility
- Active route highlighting

### Top Bar Menu (`src/app/shared/layout/nav/top-bar-menu.component.ts`)
- Horizontal navigation for top-menu themes

### Title Bar (`src/app/shared/layout/titlebar.component.ts`)
- Page title and breadcrumb display

### Admin Bar (`src/app/shared/layout/nav/adm-bar.component.ts`)
- Administrative quick access bar

### App Navigation Service (`src/app/shared/layout/nav/app-navigation.service.ts`)
- Centralized menu item definitions
- Permission-based filtering
- Route configuration

### Profile Modals
- **`ChangePasswordModalComponent`**: Password change form
- **`ChangeProfilePictureModalComponent`**: Profile picture upload
- **`MySettingsModalComponent`**: Personal settings

### Login Attempts (`login-attempts-modal.component.ts`)
- View recent login attempts history

---

## Shared Utilities

### Directives (`src/shared/utils/`)
- **`BusyIfDirective`** (`[busyIf]`): Shows loading overlay on elements
- **`ButtonBusyDirective`** (`[buttonBusy]`): Disables button and shows spinner during operations
- **`AutoFocusDirective`** (`[autoFocus]`): Auto-focuses element on view init
- **`NullDefaultValueDirective`**: Handles null default values in inputs
- **`DatePickerInitialValueDirective`**: Initializes date picker values
- **`DateRangePickerInitialValueDirective`**: Initializes date range picker values

### Pipes
- **`LocalizePipe`** (`| localize`): Translates localization keys
- **`MomentFormatPipe`** (`| momentFormat`): Formats dates using Moment.js
- **`MomentFromNowPipe`** (`| momentFromNow`): Shows relative time (e.g., "5 minutes ago")
- **`CustomCurrencyPipe`**: Currency formatting with locale support

### Services
- **`FileDownloadService`**: Download temporary files from the server
- **`LocalStorageService`**: Wrapper for local storage with localForage
- **`ScriptLoaderService`**: Dynamic script loading
- **`StyleLoaderService`**: Dynamic stylesheet loading
- **`ArrayToTreeConverterService`**: Converts flat arrays to tree structures
- **`TreeDataHelperService`**: Tree manipulation utilities
- **`DateTimeService`**: Date/time utilities and timezone handling
- **`AppLocalizationService`**: Localization helper service
- **`AppAuthService`**: Authentication/logout service
- **`CookieConsentService`**: GDPR cookie consent management

### Validators
- **`EqualValidator`**: Cross-field equality validation (e.g., confirm password)
- **`PasswordComplexityValidator`**: Password strength validation

### Helpers
- **`UrlHelper`**: URL parameter parsing, return URL, SSO parameters
- **`DomHelper`**: DOM manipulation utilities
- **`DataTableHelper`**: PrimeNG DataTable pagination, sorting, filtering helpers

---

## Service Proxies

### Generated API Clients (`src/shared/service-proxies/service-proxies.ts`)
Auto-generated by **NSwag** from the backend API (16,000+ lines). Key proxies include:

- **`TokenAuthServiceProxy`**: Authentication (login, external auth, 2FA)
- **`UserServiceProxy`**: User CRUD operations
- **`RoleServiceProxy`**: Role CRUD operations
- **`TenantServiceProxy`**: Tenant CRUD operations
- **`AuditLogServiceProxy`**: Audit log queries and exports
- **`LanguageServiceProxy`**: Language management
- **`HostSettingsServiceProxy`**: Application settings
- **`ProfileServiceProxy`**: User profile operations
- **`NotificationServiceProxy`**: Notification management
- **`ChatServiceProxy`**: Chat message operations
- **`FriendshipServiceProxy`**: Friend management
- **`AccountServiceProxy`**: Account operations (tenant check, impersonation)
- **`CommonLookupServiceProxy`**: Common lookups (user search)
- **`AirplanesServiceProxy`**: Sample entity CRUD
- **`AboutServiceProxy`**: Application version info
- **`AntiForgeryServiceProxy`**: CSRF token management

---

## Localization

### Implementation
- Multi-source localization with fallback chain:
  1. `GameHub` (application-specific)
  2. `EafCore` (framework core)
  3. `Abp` (base framework)
  4. `AbpWeb` (web-specific)
  5. `AbpZero` (multi-tenancy)
  6. `EafAzureActiveDirectory` (AD integration)
  7. `EafLdap` (LDAP integration)

- **`LocalizePipe`**: Template-level localization (`{{ 'Key' | localize }}`)
- **`l()` method**: Component-level localization (`this.l('Key')`)
- **Language Management**: Admin can add/edit languages and translation texts

---

## Configuration

### `AppConsts` (`src/shared/AppConsts.ts`)
Central configuration constants:
- `remoteServiceBaseUrl`: Backend API URL
- `appBaseUrl`: Frontend application URL
- `recaptchaSiteKey`: reCAPTCHA site key
- `googleAnalytics`: Google Analytics ID
- `googleTagManager`: Google Tag Manager ID
- `userManagement.defaultAdminUserName`: Default admin username
- `authorization.encrptedAuthTokenName`: Encrypted token cookie name
- `grid.defaultPageSize`: Default page size (30)
- `themeUser`: Theme preference storage keys
- `LocaleCurrency`: Currency/locale mappings for multiple countries (BRL, USD, ARS, CLP, etc.)

### Environment Files
- `environment.ts`: Development configuration
- `environment.prod.ts`: Production configuration
- `environment.hmr.ts`: Hot Module Replacement configuration

### Build Configuration
- **Angular CLI** (`angular.json`): Build, serve, test, and i18n configurations
- **Karma** (`karma.conf.js`): Test runner with Chrome launcher and coverage reporting
- **TypeScript** (`tsconfig.json`, `tsconfig.app.json`, `tsconfig.spec.json`): Compilation settings

---

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `@angular/core` | 17.0.0 | Core framework |
| `primeng` | 17.0.0 | UI components (tables, dialogs) |
| `ngx-bootstrap` | 10.2.0 | Bootstrap components |
| `@microsoft/signalr` | 7.0.14 | Real-time communication |
| `@azure/msal-browser` | 2.39.0 | Microsoft authentication |
| `@auth0/auth0-spa-js` | 2.0.7 | Auth0 authentication |
| `angular-oauth2-oidc` | 15.0.1 | OpenID Connect |
| `moment` | 2.30.1 | Date/time manipulation |
| `lodash` | 4.17.21 | Utility functions |
| `localforage` | 1.7.3 | Local storage |
| `ngx-scrollbar` | 11.0.0 | Custom scrollbars |
| `ng-recaptcha` | 11.0.0 | reCAPTCHA |
| `@swimlane/ngx-charts` | 20.0.0 | Data visualization |

---

## Development Scripts

```bash
# Development server
npm start                    # ng serve

# Production build
npm run build               # ng build --configuration=production

# Run unit tests
npm test                    # ng test (Karma + Jasmine)

# Lint
npm run eslint              # ESLint check
npm run eslint-fix          # ESLint auto-fix

# Format
npm run prettier            # Prettier check
npm run prettier-fix        # Prettier auto-fix
```
