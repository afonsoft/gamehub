# Components - EAF Angular UI Template

## Overview

This document describes the key components available in the EAF Angular UI template.

## Layout Components

### TopbarComponent

**Location**: `src/app/shared/layout/topbar.component.ts`

The top navigation bar that appears on every page.

**Features:**
- User profile picture and name display
- Language switcher dropdown
- Chat toggle button
- Notification bell with unread count
- User menu (profile, password, settings, logout)
- Impersonation indicator with "back to my account" button
- Multi-tenancy information display

**Usage:**
Automatically included in all layout themes. No manual usage required.

### SideBarMenuComponent

**Location**: `src/app/shared/layout/nav/side-bar-menu.component.ts`

The side navigation menu for themes with sidebar layout.

**Features:**
- Hierarchical navigation menu
- Permission-based menu item visibility
- Active route highlighting
- Collapsible menu items
- Icon support for menu items

**Usage:**
Automatically included in sidebar layout themes. Menu items are defined in `AppNavigationService`.

### TopBarMenuComponent

**Location**: `src/app/shared/layout/nav/top-bar-menu.component.ts`

The horizontal navigation menu for top-menu themes.

**Features:**
- Horizontal navigation layout
- Permission-based menu item visibility
- Active route highlighting
- Dropdown submenus

**Usage:**
Automatically included in top-menu layout themes.

### TitlebarComponent

**Location**: `src/app/shared/layout/titlebar.component.ts`

Displays the page title and breadcrumb navigation.

**Features:**
- Dynamic page title based on current route
- Breadcrumb navigation
- Theme-aware styling

**Usage:**
Automatically included in all layout themes.

### AdminBarComponent

**Location**: `src/app/shared/layout/nav/adm-bar.component.ts`

Administrative quick access bar for host administrators.

**Features:**
- Quick access to admin features
- Host-specific functionality
- Permission-based visibility

**Usage:**
Automatically included for host administrators.

## Account Components

### LoginComponent

**Location**: `src/account/login/login.component.ts`

Main login page with username/password form.

**Template:**
```html
<div class="login-form">
  <form [formGroup]="loginForm">
    <input formControlName="userName" />
    <input formControlName="password" type="password" />
    <button (click)="login()">{{ 'Login' | localize }}</button>
  </form>
</div>
```

**Features:**
- Username/password authentication
- Multi-tenancy selection (tenant dropdown)
- Remember me functionality
- reCAPTCHA integration
- External login provider buttons (Google, Microsoft, Auth0, OpenID Connect)
- Password reset link
- Email activation link

### ForgotPasswordComponent

**Location**: `src/account/password/forgot-password.component.ts`

Password reset request form.

**Features:**
- Email input for password reset
- Sends password reset email
- Tenant-aware

### ResetPasswordComponent

**Location**: `src/account/password/reset-password.component.ts`

Password reset form with validation code.

**Features:**
- New password input
- Confirm password input
- Validation code input
- Password complexity validation

### EmailActivationComponent

**Location**: `src/account/email-activation/email-activation.component.ts`

Email activation request form.

**Features:**
- Email input
- Sends activation email

### ConfirmEmailComponent

**Location**: `src/account/email-activation/confirm-email.component.ts`

Email confirmation form with activation code.

**Features:**
- Activation code input
- Confirms email address

## Admin Components

### UsersComponent

**Location**: `src/app/admin/users/users.component.ts`

User listing with search, pagination, and filtering.

**Template:**
```html
<p-table [value]="table.items" [lazy]="true" (onLazyLoad)="getUsers($event)">
  <ng-template pTemplate="header">
    <tr>
      <th>{{ 'UserName' | localize }}</th>
      <th>{{ 'EmailAddress' | localize }}</th>
      <th>{{ 'Actions' | localize }}</th>
    </tr>
  </ng-template>
  <ng-template pTemplate="body" let-user>
    <tr>
      <td>{{ user.userName }}</td>
      <td>{{ user.emailAddress }}</td>
      <td>
        <button (click)="editUser(user)">{{ 'Edit' | localize }}</button>
        <button (click)="deleteUser(user)">{{ 'Delete' | localize }}</button>
      </td>
    </tr>
  </ng-template>
</p-table>
```

**Features:**
- PrimeNG DataTable with lazy loading
- Search by username or email
- Pagination
- Export to Excel
- Entity history tracking
- Create, edit, delete actions

### CreateOrEditUserModalComponent

**Location**: `src/app/admin/users/create-or-edit-user-modal.component.ts`

Modal for creating or editing users.

**Template:**
```html
<div class="modal-header">
  <h4>{{ title }}</h4>
</div>
<div class="modal-body">
  <form [formGroup]="userForm">
    <div class="form-group">
      <label>{{ 'UserName' | localize }}</label>
      <input formControlName="userName" class="form-control" />
    </div>
    <div class="form-group">
      <label>{{ 'EmailAddress' | localize }}</label>
      <input formControlName="emailAddress" class="form-control" />
    </div>
    <div class="form-group">
      <label>{{ 'Name' | localize }}</label>
      <input formControlName="name" class="form-control" />
    </div>
  </form>
</div>
<div class="modal-footer">
  <button (click)="save()" [disabled]="saving">{{ 'Save' | localize }}</button>
  <button (click)="close()">{{ 'Cancel' | localize }}</button>
</div>
```

**Features:**
- User name, email, name fields
- Role assignment with multi-select
- Active/inactive toggle
- Phone number
- Save and cancel buttons

### EditUserPermissionsModalComponent

**Location**: `src/app/admin/users/edit-user-permissions-modal.component.ts`

Modal for managing user permissions.

**Features:**
- Permission tree with hierarchical permissions
- Grant/revoke permissions
- Reset to role permissions
- Save permissions

### RolesComponent

**Location**: `src/app/admin/roles/roles.component.ts`

Role listing and management.

**Features:**
- PrimeNG DataTable with lazy loading
- Search by role name
- Create, edit, delete actions
- Entity history tracking

### CreateOrEditRoleModalComponent

**Location**: `src/app/admin/roles/create-or-edit-role-modal.component.ts`

Modal for creating or editing roles.

**Features:**
- Role name input
- Display name input
- Description input
- Permission tree for assigning permissions
- Active/inactive toggle
- Default role toggle

### TenantsComponent

**Location**: `src/app/admin/tenants/tenants.component.ts`

Tenant listing and management.

**Features:**
- PrimeNG DataTable with lazy loading
- Search by tenant name
- Create, edit, delete actions
- Entity history tracking

### CreateTenantModalComponent

**Location**: `src/app/admin/tenants/create-tenant-modal.component.ts`

Modal for creating new tenants.

**Features:**
- Tenant name input
- Admin email input
- Admin password input
- Connection string input (optional)
- Database name input (optional)

### EditTenantModalComponent

**Location**: `src/app/admin/tenants/edit-tenant-modal.component.ts`

Modal for editing existing tenants.

**Features:**
- Tenant name (read-only)
- Admin email
- Active/inactive toggle
- Edition (for multi-edition licensing)

### TenantFeaturesModalComponent

**Location**: `src/app/admin/tenants/tenant-features-modal.component.ts`

Modal for managing tenant features.

**Features:**
- Feature tree with all available features
- Enable/disable features per tenant
- Save feature settings

### LanguagesComponent

**Location**: `src/app/admin/languages/languages.component.ts`

Language listing and management.

**Features:**
- PrimeNG DataTable with lazy loading
- Search by language name
- Create, edit, delete actions
- Set as default language

### LanguageTextsComponent

**Location**: `src/app/admin/languages/language-texts.component.ts`

Edit localization texts for a language.

**Features:**
- Search by base text key
- Edit translation text
- Save translations
- Reset to default

### CreateOrEditLanguageModalComponent

**Location**: `src/app/admin/languages/create-or-edit-language-modal.component.ts`

Modal for creating or editing languages.

**Features:**
- Language name
- Display name
- Icon (flag)
- Is default toggle

### EditTextModalComponent

**Location**: `src/app/admin/languages/edit-text-modal.component.ts`

Modal for editing individual translation keys.

**Features:**
- Base text key (read-only)
- Translation text input
- Save translation

### AuditLogsComponent

**Location**: `src/app/admin/audit-logs/audit-logs.component.ts`

View application audit logs.

**Features:**
- Date range filtering
- Filter by username, service, method, browser
- Exception filtering
- Execution duration filtering
- Entity change tracking
- Export to Excel
- PrimeNG DataTable with pagination

### AuditLogDetailModalComponent

**Location**: `src/app/admin/audit-logs/audit-log-detail-modal.component.ts`

Detailed audit log view.

**Features:**
- Full audit log details
- Parameters display
- Exception details (if any)
- Entity changes display

### SettingsComponent

**Location**: `src/app/admin/settings/settings.component.ts`

Host-level application settings.

**Features:**
- Tabbed interface for different settings sections:
  - Email settings (SMTP configuration, test email)
  - User management settings (registration, login options)
  - Security settings (password complexity, lockout)
  - Azure Active Directory integration
  - LDAP integration
  - External login providers (Google, Microsoft, OpenID Connect, Auth0)
  - OpenID Connect claims mapping
  - Timezone management

### UiCustomizationComponent

**Location**: `src/app/admin/ui-customization/ui-customization.component.ts`

Theme selection and customization.

**Features:**
- Theme selection (default, theme2, theme3, theme4)
- Theme-specific settings (aside skin, header skin, menu type)
- Save theme settings
- Preview theme

### MaintenanceComponent

**Location**: `src/app/admin/maintenance/maintenance.component.ts`

Cache clearing and system maintenance.

**Features:**
- Clear all caches
- Clear specific cache types
- Clear application cache
- Clear session cache
- Clear database cache

### HangfireComponent

**Location**: `src/app/admin/hangfire/hangfire.component.ts`

Embedded Hangfire background job dashboard.

**Features:**
- View background jobs
- Monitor job status
- Retry failed jobs
- Delete jobs

## Shared Admin Components

### PermissionTreeComponent

**Location**: `src/app/admin/shared/permission-tree.component.ts`

Hierarchical permission selection tree.

**Template:**
```html
<p-tree [value]="permissions" selectionMode="checkbox"
  [(selection)]="selectedPermissions" (onNodeSelect)="onNodeSelect($event)">
  <ng-template pTemplate="default" let-node>
    {{ node.data.displayName | localize }}
  </ng-template>
</p-tree>
```

**Features:**
- Hierarchical permission tree
- Checkbox selection
- Parent-child relationship
- Expand/collapse nodes

### PermissionComboComponent

**Location**: `src/app/admin/shared/permission-combo.component.ts`

Permission dropdown selector.

**Features:**
- Searchable dropdown
- Multi-select support
- Permission display with localization

### RoleComboComponent

**Location**: `src/app/admin/shared/role-combo.component.ts`

Role dropdown selector.

**Features:**
- Searchable dropdown
- Multi-select support
- Role display with localization

### FeatureTreeComponent

**Location**: `src/app/admin/shared/feature-tree.component.ts`

Feature selection tree for tenants.

**Features:**
- Hierarchical feature tree
- Checkbox selection
- Parent-child relationship
- Expand/collapse nodes

## Chat Components

### ChatBarComponent

**Location**: `src/app/shared/layout/chat/chat-bar.component.ts`

Full-featured chat sidebar.

**Features:**
- Friend list management
- Add friend by username
- Block/unblock friends
- Real-time messaging via SignalR
- Image and file upload support
- Link sharing
- Read/unread message tracking
- Multi-tenancy aware
- Persistent chat state
- Previous message loading with pagination
- User online/offline status tracking

**Template:**
```html
<div class="chat-bar">
  <div class="chat-header">
    <button (click)="toggleChat()">{{ 'Chat' | localize }}</button>
  </div>
  <div class="chat-body" *ngIf="isOpen">
    <div class="friend-list">
      <div *ngFor="let friend of friends" class="friend-item">
        <span>{{ friend.friendUserName }}</span>
        <span *ngIf="friend.unreadMessageCount > 0" class="badge">
          {{ friend.unreadMessageCount }}
        </span>
      </div>
    </div>
    <div class="chat-messages">
      <div *ngFor="let message of messages" class="message">
        <span>{{ message.senderUserName }}: {{ message.message }}</span>
      </div>
    </div>
    <div class="chat-input">
      <input [(ngModel)]="newMessage" (keyup.enter)="sendMessage()" />
      <button (click)="sendMessage()">{{ 'Send' | localize }}</button>
    </div>
  </div>
</div>
```

### ChatFriendListItemComponent

**Location**: `src/app/shared/layout/chat/chat-friend-list-item.component.ts`

Individual friend list entry.

**Features:**
- Friend user name
- Online status indicator
- Unread message count badge
- Click to open chat

### ChatMessageComponent

**Location**: `src/app/shared/layout/chat/chat-message.component.ts`

Chat message bubble renderer.

**Features:**
- Message text display
- Sender name
- Timestamp
- Message styling (sent/received)

## Notification Components

### NotificationsComponent

**Location**: `src/app/shared/layout/notifications/notifications.component.ts`

Full notification list page.

**Features:**
- Read/unread filtering
- PrimeNG DataTable with pagination
- Mark as read (individual and bulk)
- Delete notifications
- Notification settings management
- Severity-based categorization (Info, Success, Warning, Error, Fatal)

### HeaderNotificationsComponent

**Location**: `src/app/shared/layout/notifications/header-notifications.component.ts`

Top bar notification bell.

**Features:**
- Unread count badge
- Quick notification preview
- Mark all as read
- View all notifications link

### NotificationSettingsModalComponent

**Location**: `src/app/shared/layout/notifications/notification-settings-modal.component.ts`

Configure notification preferences.

**Features:**
- Notification type selection
- Enable/disable notifications by type
- Save notification settings

## Profile Components

### ChangePasswordModalComponent

**Location**: `src/app/shared/layout/profile/change-password-modal.component.ts`

Password change form.

**Features:**
- Current password input
- New password input
- Confirm password input
- Password complexity validation
- Save password

### ChangeProfilePictureModalComponent

**Location**: `src/app/shared/layout/profile/change-profile-picture-modal.component.ts`

Profile picture upload.

**Features:**
- Image preview
- File upload
- Crop image
- Save profile picture

### MySettingsModalComponent

**Location**: `src/app/shared/layout/profile/my-settings-modal.component.ts`

Personal settings.

**Features:**
- Name input
- Surname input
- Email address input
- Phone number input
- Timezone selection
- Save settings

## Utility Components

### BusyIndicatorComponent

**Location**: `src/shared/utils/components/busy-indicator.component.ts`

Loading overlay for busy states.

**Usage:**
```html
<div [busyIf]="loading">
  <!-- Content -->
</div>
```

### ModalComponent

**Location**: `src/shared/utils/components/modal.component.ts`

Base modal component.

**Features:**
- Modal size configuration
- Close on backdrop click
- Animation support

## Directives

### BusyIfDirective

**Location**: `src/shared/utils/directives/busy-if.directive.ts`

Shows loading overlay on elements when busy.

**Usage:**
```html
<div [busyIf]="isLoading">
  <!-- Content -->
</div>
```

### ButtonBusyDirective

**Location**: `src/shared/utils/directives/button-busy.directive.ts`

Disables button and shows spinner during operations.

**Usage:**
```html
<button [buttonBusy]="isSaving" (click)="save()">
  {{ 'Save' | localize }}
</button>
```

### AutoFocusDirective

**Location**: `src/shared/utils/directives/auto-focus.directive.ts`

Auto-focuses element on view init.

**Usage:**
```html
<input autoFocus />
```

## Pipes

### LocalizePipe

**Location**: `src/shared/utils/pipes/localize.pipe.ts`

Translates localization keys.

**Usage:**
```html
<h1>{{ 'Users' | localize }}</h1>
```

### MomentFormatPipe

**Location**: `src/shared/utils/pipes/moment-format.pipe.ts`

Formats dates using Moment.js.

**Usage:**
```html
<span>{{ date | momentFormat:'YYYY-MM-DD' }}</span>
```

### MomentFromNowPipe

**Location**: `src/shared/utils/pipes/moment-from-now.pipe.ts`

Shows relative time.

**Usage:**
```html
<span>{{ date | momentFromNow }}</span>
```

### CustomCurrencyPipe

**Location**: `src/shared/utils/pipes/custom-currency.pipe.ts`

Currency formatting with locale support.

**Usage:**
```html
<span>{{ amount | customCurrency:'BRL' }}</span>
```

## Creating Custom Components

### Step 1: Generate Component

```bash
ng generate component components/my-component
```

### Step 2: Extend AppComponentBase

```typescript
export class MyComponent extends AppComponentBase implements OnInit {
  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit(): void {
    // Initialization logic
  }
}
```

### Step 3: Add to Module

```typescript
@NgModule({
  declarations: [MyComponent],
  imports: [CommonModule, ...]
})
export class MyModule { }
```

### Step 4: Use in Template

```html
<app-my-component></app-my-component>
```

## Component Best Practices

1. **Extend AppComponentBase**: Always extend `AppComponentBase` for access to EAF utilities
2. **Use Injector**: Use `Injector` for dependency injection in base class
3. **Localization**: Use `l()` method for localization in components
4. **Permissions**: Check permissions before showing sensitive UI
5. **Loading States**: Show loading indicators during async operations
6. **Error Handling**: Handle errors gracefully with user-friendly messages
7. **Lifecycle Hooks**: Implement `ngOnDestroy` to clean up subscriptions
8. **Change Detection**: Use `OnPush` change detection strategy for better performance
