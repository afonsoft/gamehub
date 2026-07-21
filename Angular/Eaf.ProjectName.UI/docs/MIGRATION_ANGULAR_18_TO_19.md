# Migration Guide: Angular 18 to Angular 19

## EAF Angular UI Template

This document provides a comprehensive step-by-step guide for migrating the EAF Angular UI Template from Angular 18 to Angular 19, including specific considerations for the EAF framework, standalone components migration, and third-party library compatibility.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Pre-Migration Checklist](#pre-migration-checklist)
3. [Step 1: Update Dependencies](#step-1-update-dependencies)
4. [Step 2: Standalone Components Migration](#step-2-standalone-components-migration)
5. [Step 3: EAF Framework Considerations](#step-3-eaf-framework-considerations)
6. [Step 4: Third-Party Library Updates](#step-4-third-party-library-updates)
7. [Breaking Changes Summary](#breaking-changes-summary)
8. [Testing Strategy](#testing-strategy)
9. [Rollback Procedures](#rollback-procedures)
10. [Troubleshooting](#troubleshooting)

---

## Current State Analysis

### Repository Structure Overview

Based on analysis of the EAF Angular UI Template, the current state is:

| Aspect | Current State | Target (Angular 19) |
|--------|---------------|---------------------|
| Angular Version | ^18.0.0 | ^19.0.0 |
| TypeScript | ~5.4.0 | ~5.5.0 |
| PrimeNG | ^17.17.0 | ^19.0.0 |
| ngx-bootstrap | ^12.0.0 | ^12.0.0 (verify compatibility) |
| Standalone Components | 0 (all module-based) | 59+ components |

### Component Inventory

The application has **59+ components** that need migration consideration:

**Admin Module (23 components):**
- `UsersComponent`, `CreateOrEditUserModalComponent`, `EditUserPermissionsModalComponent`
- `RolesComponent`, `CreateOrEditRoleModalComponent`
- `TenantsComponent`, `CreateTenantModalComponent`, `EditTenantModalComponent`, `TenantFeaturesModalComponent`
- `AuditLogsComponent`, `AuditLogDetailModalComponent`
- `LanguagesComponent`, `CreateOrEditLanguageModalComponent`, `EditTextModalComponent`, `LanguageTextsComponent`
- `SettingsComponent`, `MaintenanceComponent`, `HangfireComponent`
- `UiCustomizationComponent`, `DefaultThemeUiSettingsComponent`, `Theme2ThemeUiSettingsComponent`, `Theme3ThemeUiSettingsComponent`, `Theme4ThemeUiSettingsComponent`
- Shared: `PermissionTreeComponent`, `PermissionComboComponent`, `RoleComboComponent`, `FeatureTreeComponent`

**Main Module (3 components):**
- `DashboardComponent`, `AirplanesComponent`, `CreateOrEditAirplaneModalComponent`

**App Module (21 components):**
- `AppComponent`, `DefaultLayoutComponent`, `Theme2LayoutComponent`, `Theme3LayoutComponent`, `Theme4LayoutComponent`
- `HeaderNotificationsComponent`, `NotificationsComponent`, `NotificationSettingsModalComponent`
- `SideBarMenuComponent`, `AdmBarComponent`, `TopBarMenuComponent`, `TopBarComponent`, `TitleBarComponent`
- `DefaultBrandComponent`, `Theme2BrandComponent`, `Theme3BrandComponent`, `Theme4BrandComponent`
- `ChatBarComponent`, `ChatFriendListItemComponent`, `ChatMessageComponent`
- `LoginAttemptsModalComponent`, `ChangePasswordModalComponent`, `ChangeProfilePictureModalComponent`, `MySettingsModalComponent`

**Shared/Common (7+ components):**
- `CommonLookupModalComponent`, `EntityTypeHistoryModalComponent`, `EntityChangeDetailModalComponent`
- `KeyValueListManagerComponent`, `TimeZoneComboComponent`
- Directives: `DatePickerInitialValueSetterDirective`, `DateRangePickerInitialValueSetterDirective`

**Account Module (7 components):**
- `AccountComponent`, `LoginComponent`, `SsoComponent`
- `ForgotPasswordComponent`, `ResetPasswordComponent`
- `EmailActivationComponent`, `ConfirmEmailComponent`

### Module Structure

```
AppModule (root)
├── AccountModule (lazy-loaded, separate entry point)
├── AppRoutingModule
│   ├── MainModule (lazy-loaded)
│   ├── AdminModule (lazy-loaded)
│   └── NotificationsComponent
├── AppCommonModule (shared, with forRoot())
├── UtilsModule (shared utilities)
└── ServiceProxyModule (API services)
```

### EAF Framework Integration

The EAF framework has several custom integrations that require special attention:

1. **Custom Path Mappings** (`@app/*`, `@shared/*`, `@eaf/*`, `@metronic/*`)
2. **AppComponentBase** - Base class used by 68+ components
3. **EAF Module** - Core framework module with providers
4. **jQuery Integration** - `eaf.jquery.js` for DOM manipulation
5. **SignalR Service** - Real-time communication
6. **Service Proxy Pattern** - Auto-generated API clients with Blob handling

---

## Pre-Migration Checklist

### Prerequisites

- [ ] Ensure current Angular 18 application builds successfully: `npm run build`
- [ ] Run all tests and ensure they pass: `npm test`
- [ ] Verify application runs in development: `npm run start`
- [ ] Create a backup branch: `git checkout -b backup/before-angular-19-migration`
- [ ] Document current package versions in `package.json`

### Environment Requirements

- **Node.js**: ^18.19.1 || ^20.11.1 || ^22.0.0 (Angular 19 is stricter about versions than previous releases)
- **npm**: ^9.0.0 || ^10.0.0
- **Angular CLI**: ^19.0.0
- **TypeScript**: ^5.4.0 (Angular 19 won't work with older versions)

**Critical Note**: Angular 19 is pickier about versions than previous releases. Using Node.js 18.10 or older may cause esbuild errors.

### Current State Assessment

- [ ] Count total components: `find src -name "*.component.ts" | wc -l`
- [ ] Identify module-based components vs standalone (currently 0 standalone)
- [ ] Document custom EAF framework usages
- [ ] List all third-party libraries and their current versions
- [ ] Identify components extending `AppComponentBase`

---

## Step 1: Update Dependencies

### 1.1 Update Angular Core Packages

Update all Angular packages from ^18.0.0 to ^19.0.0:

```bash
# Update Angular core packages
npm install @angular/animations@^19.0.0 \
  @angular/common@^19.0.0 \
  @angular/compiler@^19.0.0 \
  @angular/core@^19.0.0 \
  @angular/forms@^19.0.0 \
  @angular/platform-browser@^19.0.0 \
  @angular/platform-browser-dynamic@^19.0.0 \
  @angular/platform-server@^19.0.0 \
  @angular/router@^19.0.0 \
  @angular/service-worker@^19.0.0 \
  @angular/pwa@^19.0.0 \
  @angular/cdk@^19.0.0 \
  @angular-devkit/core@^19.0.0
```

### 1.2 Update Angular CLI and DevKit

```bash
# Update Angular CLI and build tools
npm install @angular/cli@^19.0.0 \
  @angular-devkit/build-angular@^19.0.0 \
  @angular/compiler-cli@^19.0.0 \
  @angular-eslint/builder@^19.0.0 \
  @angular-eslint/eslint-plugin@^19.0.0 \
  @angular-eslint/eslint-plugin-template@^19.0.0 \
  @angular-eslint/schematics@^19.0.0 \
  @angular-eslint/template-parser@^19.0.0
```

### 1.3 Update TypeScript

```bash
npm install typescript@~5.5.0
```

### 1.4 Update PrimeNG (Breaking Changes Expected)

PrimeNG 19 has significant changes for Angular 19 compatibility:

```bash
npm install primeng@^19.0.0
```

**PrimeNG 19 Breaking Changes:**
- All PrimeNG components are now standalone by default
- Module imports (e.g., `TableModule`) are deprecated in favor of direct component imports
- New import pattern:

```typescript
// Before (Angular 18 + PrimeNG 17)
import { TableModule } from 'primeng/table';

@NgModule({
  imports: [TableModule]
})

// After (Angular 19 + PrimeNG 19)
import { Table, Column, Row } from 'primeng/table';

@Component({
  standalone: true,
  imports: [Table, Column, Row]
})
```

### 1.5 Verify Third-Party Library Compatibility

| Library | Current | Angular 19 Compatible | Action |
|---------|---------|----------------------|--------|
| ngx-bootstrap | ^12.0.0 | Verify | Check for v13+ |
| ngx-image-cropper | ^9.1.6 | Yes | No change needed |
| angular-calendar | ^0.31.0 | Unknown | Verify before upgrade |
| @ng-select/ng-select | ^12.0.0 | Verify | Check for v13+ |
| ngx-mask | ^15.0.0 | Verify | Check for v17+ |
| ngx-cookie-service | ^18.0.0 | Yes | Update to ^19.0.0 |
| @microsoft/signalr | ^7.0.14 | Yes | No change needed |

### 1.6 Updated package.json Dependencies

```json
{
  "dependencies": {
    "@angular/animations": "^19.0.0",
    "@angular/common": "^19.0.0",
    "@angular/compiler": "^19.0.0",
    "@angular/core": "^19.0.0",
    "@angular/forms": "^19.0.0",
    "@angular/platform-browser": "^19.0.0",
    "@angular/platform-browser-dynamic": "^19.0.0",
    "@angular/platform-server": "^19.0.0",
    "@angular/pwa": "^19.0.0",
    "@angular/router": "^19.0.0",
    "@angular/service-worker": "^19.0.0",
    "@angular/cdk": "^19.0.0",
    "@angular-devkit/core": "^19.0.0",
    "primeng": "^19.0.0",
    "typescript": "~5.5.0"
  },
  "devDependencies": {
    "@angular/cli": "^19.0.0",
    "@angular-devkit/build-angular": "^19.0.0",
    "@angular/compiler-cli": "^19.0.0",
    "@angular-eslint/builder": "^19.0.0",
    "@angular-eslint/eslint-plugin": "^19.0.0",
    "@angular-eslint/eslint-plugin-template": "^19.0.0",
    "@angular-eslint/schematics": "^19.0.0",
    "@angular-eslint/template-parser": "^19.0.0"
  }
}
```

---

## Step 2: Standalone Components Migration

### 2.1 Critical Breaking Change: Standalone Default

**IMPORTANT**: Angular 19 changed the default value for `standalone` from `false` to `true`. This is a **breaking change** that affects every NgModule-based component.

**Error you will encounter:**
```
Component AppComponent is standalone, and cannot be declared in an NgModule
```

**Solution Options:**

**Option A: Keep NgModule-based (Recommended for EAF Template)**
Add `standalone: false` explicitly to all components that use NgModules:

```typescript
// Before (Angular 18 - default was false)
@Component({
  selector: 'app-example',
  templateUrl: './example.component.html'
})
export class ExampleComponent {}

// After (Angular 19 - explicitly set for NgModule use)
@Component({
  standalone: false,  // Add this line
  selector: 'app-example',
  templateUrl: './example.component.html'
})
export class ExampleComponent {}
```

**Bulk fix script for smaller projects:**
```bash
# Add standalone: false to all components that use NgModules
find src -name "*.component.ts" -exec grep -L "standalone:" {} \; | \
  xargs grep -l "@Component" | \
  xargs sed -i '' 's/@Component({/@Component({\n standalone: false,/g'
```

**Option B: Migrate to Standalone (Long-term approach)**
Migrate components to standalone pattern, but this is complex for EAF template due to:
- AppComponentBase inheritance
- Custom EAF module integration
- jQuery integration patterns

**Recommendation for EAF Template**: Use Option A (add `standalone: false`) to maintain compatibility with existing EAF framework patterns. Standalone migration should be done as a separate, planned refactoring effort.

### 2.2 Migration Strategy

For the EAF template with 59+ components, a gradual migration approach is recommended:

**Phase 1: Simple Components (Low Risk)**
- Leaf components with minimal dependencies
- Modal components: `ChangePasswordModalComponent`, `MySettingsModalComponent`
- Display components: `Theme2BrandComponent`, `DefaultBrandComponent`

**Phase 2: Feature Components (Medium Risk)**
- Admin module components: `UsersComponent`, `RolesComponent`, `TenantsComponent`
- Main module components: `DashboardComponent`, `AirplanesComponent`

**Phase 3: Complex Components (High Risk)**
- `AppComponent` (root component with router outlet)
- Layout components: `DefaultLayoutComponent`, `Theme2LayoutComponent`
- Components with heavy jQuery integration

**Phase 4: Base Class Migration (Optional)**
- Migrate `AppComponentBase` to standalone-compatible pattern
- This is complex and may require significant refactoring

### 2.2 Basic Component Migration Pattern

**Example: Migrating a Simple Component**

```typescript
// BEFORE: Module-based component
// users.component.ts
import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent extends AppComponentBase implements OnInit {
  constructor(injector: Injector) {
    super(injector);
  }
  
  ngOnInit(): void {
    // Component logic
  }
}

// admin.module.ts
@NgModule({
  declarations: [UsersComponent, /* ... */],
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    // ...
  ]
})
export class AdminModule {}
```

```typescript
// AFTER: Standalone component
// users.component.ts
import { Component, Injector, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Table, Column, Row } from 'primeng/table';
import { Paginator } from 'primeng/paginator';
import { Button } from 'primeng/button';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  selector: 'app-users',
  standalone: true,
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  imports: [
    // Angular modules
    CommonModule,
    FormsModule,
    // PrimeNG standalone components
    Table,
    Column,
    Row,
    Paginator,
    Button
  ]
})
export class UsersComponent extends AppComponentBase implements OnInit {
  constructor(injector: Injector) {
    super(injector);
  }
  
  ngOnInit(): void {
    // Component logic (unchanged)
  }
}

// admin.module.ts
@NgModule({
  declarations: [
    // Remove UsersComponent from declarations
    /* Other non-standalone components */
  ],
  imports: [
    CommonModule,
    // Import standalone component as a module
    UsersComponent,
    // ... other imports
  ]
})
export class AdminModule {}
```

### 2.3 AppComponentBase Consideration

**Critical**: Components extending `AppComponentBase` require special handling:

```typescript
// AppComponentBase typically provides:
// - Localization services
// - Permission checking
// - Notification helpers
// - Busy state management

// When migrating to standalone, ensure base class services are available:
@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, Table],
  providers: [
    // Services provided by AppComponentBase must be available
    // These are typically provided in root or imported via AppCommonModule
  ]
})
export class UsersComponent extends AppComponentBase implements OnInit {
  // Component logic
}
```

**Recommendation**: Keep using `AppComponentBase` during migration. The base class services should remain available through the root injector or imported modules.

### 2.4 Routing with Standalone Components

**Lazy Loading with Standalone Components:**

```typescript
// BEFORE: Lazy load module
const routes: Routes = [
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
  }
];

// AFTER: Lazy load standalone component (optional optimization)
const routes: Routes = [
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
    // Or eventually:
    // loadComponent: () => import('./admin/users.component').then(c => c.UsersComponent)
  }
];
```

**Note**: For the EAF template, keep module-based lazy loading initially. Migrate to `loadComponent` gradually after all components in a module are standalone.

### 2.5 Module Migration Checklist

For each module, follow this process:

1. **Select a target module** (start with MainModule - fewer components)
2. **Migrate components one by one**:
   - Add `standalone: true` to @Component decorator
   - Add required imports (CommonModule, FormsModule, PrimeNG components)
   - Remove from module declarations
   - Add to module imports
   - Test component functionality
3. **Update module imports**:
   - Add migrated standalone components to imports array
   - Keep non-migrated components in declarations
4. **Test the entire module**

### 2.6 PrimeNG 19 Component Import Mapping

| PrimeNG 17 Module | PrimeNG 19 Standalone Components |
|-------------------|----------------------------------|
| `TableModule` | `Table`, `Column`, `Row`, `Cell` |
| `TreeModule` | `Tree`, `TreeNode` |
| `PaginatorModule` | `Paginator` |
| `ButtonModule` | `Button` |
| `DialogModule` | `Dialog`, `DialogHeader`, `DialogFooter` |
| `DropdownModule` | `Dropdown`, `DropdownItem` |
| `InputTextModule` | `InputText` |
| `CalendarModule` | `DatePicker` |
| `CheckboxModule` | `Checkbox` |
| `RadioButtonModule` | `RadioButton` |
| `AutoCompleteModule` | `AutoComplete` |
| `FileUploadModule` | `FileUpload` |
| `EditorModule` | `Editor` |
| `InputMaskModule` | `InputMask` |
| `ContextMenuModule` | `ContextMenu` |
| `DragDropModule` | `Drag`, `Drop` |
| `ProgressBarModule` | `ProgressBar` |

---

## Step 3: EAF Framework Considerations

### 3.1 Path Mappings (tsconfig.json)

The EAF template uses custom path mappings that must be preserved:

```json
{
  "compilerOptions": {
    "paths": {
      "@app/*": ["src/app/*"],
      "@shared/*": ["src/app/shared/*"],
      "@eaf/*": ["src/assets/lib/eaf-ng2-module/src/*"],
      "@metronic/*": ["src/assets/lib/metronic-ng/*"]
    }
  }
}
```

**Action**: Verify these paths work with Angular 19's stricter module resolution.

### 3.2 EAF Module Integration

The `EafModule` provides core framework services:

```typescript
// EafModule is typically imported in AppModule and AccountModule
@NgModule({
  imports: [
    EafModule,  // Keep this import
    // ...
  ]
})
```

**Standalone Migration Impact**:
- `EafModule` should remain as a module import
- Its providers are registered in root scope
- Standalone components will still have access to EAF services

### 3.3 EafHttpInterceptor - Critical

**WARNING**: The `EafHttpInterceptor` uses a manual Subject-based pattern for Blob response handling.

```typescript
// DO NOT refactor this to use RxJS operators (switchMap/map)
// The current implementation is:
intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
  const interceptObservable = new Subject<HttpEvent<any>>();
  // ... manual async handling for Blob processing
  return interceptObservable;
}
```

**Angular 19 Compatibility**: This pattern is fully compatible with Angular 19. No changes needed.

### 3.4 jQuery Integration

The EAF template uses jQuery for DOM manipulation:

```typescript
// File: src/assets/lib/eaf-web-resources/Eaf/Framework/scripts/libs/eaf.jquery.js
```

**Angular 19 Considerations**:
- jQuery integration should continue to work
- Test all jQuery-dependent features after migration
- Watch for change detection conflicts

### 3.5 Service Proxy Pattern

Auto-generated service proxies in `src/shared/service-proxies/`:

- Use Blob processing for file downloads
- Integrate with `EafHttpInterceptor`
- No changes required for Angular 19

---

## Step 4: Third-Party Library Updates

### 4.1 ngx-bootstrap

ngx-bootstrap 12.0.0 should be compatible with Angular 19, but verify:

```bash
# Check for updates
npm info ngx-bootstrap versions

# If v13+ is available with Angular 19 support:
npm install ngx-bootstrap@^13.0.0
```

### 4.2 ngx-image-cropper

ngx-image-cropper ^9.1.6 is already compatible with Angular 19, 20, and 21:

```bash
# No update needed, but can update to latest:
npm install ngx-image-cropper@^9.1.6
```

**Usage in Standalone Components**:

```typescript
import { ImageCropperComponent } from 'ngx-image-cropper';

@Component({
  standalone: true,
  imports: [ImageCropperComponent],
  template: `
    <image-cropper
      [imageChangedEvent]="imageChangedEvent"
      [maintainAspectRatio]="true"
      [aspectRatio]="4/4"
      format="png"
      (imageCropped)="imageCropped($event)"
    ></image-cropper>
  `
})
export class ChangeProfilePictureModalComponent {}
```

### 4.3 angular-calendar

angular-calendar ^0.31.0 compatibility with Angular 19 is unknown:

```bash
# Check latest version
npm info angular-calendar versions

# Verify Angular 19 support before updating
```

**Alternative**: If not compatible, consider:
- @fullcalendar/angular
- PrimeNG Calendar/DatePicker components

### 4.4 @ng-select/ng-select

```bash
# Check for Angular 19 compatible version
npm info @ng-select/ng-select versions

# Update if v13+ available
npm install @ng-select/ng-select@^13.0.0
```

### 4.5 ngx-mask

```bash
# Check for Angular 19 compatible version
npm info ngx-mask versions

# Update if v17+ available
npm install ngx-mask@^17.0.0
```

---

## Breaking Changes Summary

### Angular 19 Breaking Changes

#### 1. Standalone Components Default

- **Impact**: New projects use standalone by default; existing NgModule projects still work
- **EAF Impact**: All 59+ components are module-based; migration is optional but recommended
- **Priority**: Medium (can migrate gradually)

#### 2. TypeScript 5.5 Required

- **Impact**: stricter type checking, new TypeScript features
- **EAF Impact**: May require type definition updates
- **Priority**: High (must update)

#### 3. Zone.js Optional (Zoneless)

- **Impact**: Zone.js becomes optional for new projects
- **EAF Impact**: The EAF template relies heavily on Zone.js for change detection
- **Recommendation**: Keep Zone.js for now; zoneless migration is a major undertaking
- **Priority**: Low (optional optimization)

#### 4. Router Updates

- **Impact**: Router improvements and new features
- **EAF Impact**: Minimal; current routing should continue to work
- **Priority**: Low

#### 5. New Forms APIs

- **Impact**: Enhanced reactive forms with better type safety
- **EAF Impact**: May require updates to custom form implementations
- **Priority**: Medium

### PrimeNG 19 Breaking Changes

#### 1. All Components Standalone

- **Impact**: Module imports deprecated
- **EAF Impact**: All PrimeNG imports must be updated
- **Priority**: High (must update)

#### 2. Component Import Changes

- **Impact**: Import individual components instead of modules
- **EAF Impact**: Update all 59+ component files
- **Priority**: High (must update)

#### 3. Theming Updates

- **Impact**: New theming system may require CSS updates
- **EAF Impact**: Check custom styles in `styles.css`
- **Priority**: Medium

---

## Testing Strategy

### Phase 1: Build Verification

After dependency updates:

```bash
# Clean install
rm -rf node_modules package-lock.json
npm install

# Build verification
npm run build

# Development server
npm run start
```

### Phase 2: Component Testing

Test each migrated component:

1. **Visual Rendering**
   - Component displays correctly
   - No console errors
   - Styles applied properly

2. **Functionality**
   - All buttons work
   - Forms submit correctly
   - Modals open/close properly

3. **PrimeNG Components**
   - Tables load data
   - Dropdowns populate
   - Date pickers work
   - File uploads function

### Phase 3: Integration Testing

Test EAF-specific features:

1. **Authentication**
   - Login/logout flow
   - Token refresh
   - Permission checking

2. **SignalR**
   - Real-time notifications
   - Chat functionality

3. **Service Proxies**
   - API calls work
   - Blob downloads work
   - Error handling functions

4. **jQuery Integration**
   - Layout components work
   - Theme switching functions

### Phase 4: Regression Testing

Test complete user workflows:

1. **Admin Workflow**
   - Login → Dashboard → Users → Edit User → Save
   - Check permissions display
   - Verify audit logs

2. **Main Module Workflow**
   - Navigate to Airplanes
   - Create/Edit/Delete records
   - Verify data tables

3. **Account Module**
   - Password change
   - Profile picture upload (with cropper)
   - Settings update

---

## Rollback Procedures

### Immediate Rollback (If Build Fails)

```bash
# Restore package.json from git
git checkout backup/before-angular-19-migration -- package.json

# Clean and reinstall
rm -rf node_modules package-lock.json
npm install

# Verify build
npm run build
```

### Component-Level Rollback

If a specific component fails after standalone migration:

```typescript
// Remove standalone: true
@Component({
  selector: 'app-users',
  // standalone: true,  // Remove this
  imports: [],  // Remove imports array
  templateUrl: './users.component.html'
})

// Add back to module declarations
@NgModule({
  declarations: [UsersComponent],  // Add back
  imports: [
    // Remove UsersComponent from imports
  ]
})
```

### Full Migration Rollback

```bash
# Reset to pre-migration branch
git checkout backup/before-angular-19-migration

# Clean environment
rm -rf node_modules package-lock.json
npm install

# Verify
npm run build
npm test
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue 1: "Cannot resolve module" errors

**Cause**: Angular 19 stricter module resolution

**Solution**:
```json
// tsconfig.json
{
  "compilerOptions": {
    "moduleResolution": "node",
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true
  }
}
```

#### Issue 2: PrimeNG components not rendering

**Cause**: Importing modules instead of standalone components

**Solution**:
```typescript
// Correct import for PrimeNG 19
import { Table, Column } from 'primeng/table';

@Component({
  standalone: true,
  imports: [Table, Column]  // Import components, not TableModule
})
```

#### Issue 3: AppComponentBase services not available

**Cause**: Services not provided in standalone component scope

**Solution**:
```typescript
// Ensure services are provided in root or imported module
@Component({
  standalone: true,
  imports: [
    // ...
    AppCommonModule  // Provides base services
  ]
})
```

#### Issue 4: ngx-bootstrap components not working

**Cause**: ngx-bootstrap version incompatible with Angular 19

**Solution**:
```bash
# Update to latest version
npm install ngx-bootstrap@latest

# Or use --legacy-peer-deps if needed
npm install ngx-bootstrap@latest --legacy-peer-deps
```

#### Issue 5: jQuery/$ not defined

**Cause**: jQuery types not recognized in Angular 19

**Solution**:
```typescript
// Add to component or global types
declare const $: any;
declare const jQuery: any;
```

#### Issue 6: EafHttpInterceptor Blob processing fails

**Cause**: Changes to interceptor pattern

**Solution**: 
- Keep original Subject-based implementation
- Do not refactor to RxJS operators
- Verify interceptor is provided in root

---

## Migration Timeline Estimate

### Conservative Timeline (Recommended)

| Phase | Duration | Activities |
|-------|----------|------------|
| 1. Preparation | 1 day | Backup, dependency analysis, environment setup |
| 2. Dependency Updates | 1-2 days | Update Angular, PrimeNG, TypeScript |
| 3. Build Fixes | 1-2 days | Resolve build errors, type issues |
| 4. Component Migration | 5-10 days | Migrate 59+ components to standalone |
| 5. Testing | 3-5 days | Component, integration, regression testing |
| 6. Documentation | 1 day | Update docs, changelog |
| **Total** | **12-21 days** | |

### Minimal Timeline (Dependencies Only)

If skipping standalone migration:

| Phase | Duration | Activities |
|-------|----------|------------|
| 1. Preparation | 0.5 day | Backup, dependency updates |
| 2. Build Fixes | 1-2 days | Resolve build errors |
| 3. Testing | 1-2 days | Basic testing |
| **Total** | **2.5-4.5 days** | |

---

## Post-Migration Checklist

- [ ] All dependencies updated to Angular 19 compatible versions
- [ ] Application builds without errors: `npm run build`
- [ ] All tests pass: `npm test`
- [ ] Development server runs: `npm run start`
- [ ] All 59+ components render correctly
- [ ] PrimeNG components updated to standalone imports
- [ ] EAF framework integration tested
- [ ] SignalR real-time features working
- [ ] jQuery-dependent features working
- [ ] Authentication flow tested
- [ ] Documentation updated

---

## Resources and References

### Official Documentation

- [Angular 19 Release Notes](https://github.com/angular/angular/releases/tag/19.0.0)
- [Angular Update Guide](https://update.angular.io/?l=2&v=18.0-19.0)
- [Standalone Components Guide](https://angular.dev/guide/standalone-components)
- [PrimeNG 19 Migration](https://primeng.org/migration)

### EAF-Specific Resources

- `MIGRATION_SUMMARY_18.md` - Previous Angular 17→18 migration notes
- `MIGRATION_ANGULAR_17_TO_18.md` - Detailed 17→18 guide
- EAF Framework Documentation (if available)

---

## Last Updated

**Date**: May 23, 2026
**Version**: 2.1 (Updated with Angular 19 release notes)
**Angular Source**: 18.0.0
**Angular Target**: 19.0.0
**Components to Migrate**: 59+
**Maintainer**: EAF Development Team
