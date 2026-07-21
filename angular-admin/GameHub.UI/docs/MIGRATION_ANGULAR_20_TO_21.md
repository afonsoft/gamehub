# Migration Guide: Angular 20 to Angular 21

## EAF Angular UI Template

This document provides a comprehensive step-by-step guide for migrating the EAF Angular UI Template from Angular 20 to Angular 21, including specific considerations for the EAF framework, standalone components migration, and third-party library compatibility.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Pre-Migration Checklist](#pre-migration-checklist)
3. [Step 1: Update Dependencies](#step-1-update-dependencies)
4. [Step 2: Breaking Changes](#step-2-breaking-changes)
5. [Step 3: EAF Framework Considerations](#step-3-eaf-framework-considerations)
6. [Step 4: Third-Party Library Updates](#step-4-third-party-library-updates)
7. [Testing Strategy](#testing-strategy)
8. [Rollback Procedures](#rollback-procedures)
9. [Troubleshooting](#troubleshooting)

---

## Current State Analysis

### Repository Structure Overview

Based on analysis of the EAF Angular UI Template, the current state is:

| Aspect | Current State | Target (Angular 21) |
|--------|---------------|---------------------|
| Angular Version | ^20.0.0 | ^21.0.0 |
| TypeScript | ~5.6.0 | ~5.7.0 |
| PrimeNG | ^20.0.0 | ^21.0.0 |
| ngx-bootstrap | ^13.0.0 | ^14.0.0 (verify compatibility) |
| Standalone Components | Partially migrated | Continue migration |

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

- [ ] Ensure current Angular 20 application builds successfully: `npm run build`
- [ ] Run all tests and ensure they pass: `npm test`
- [ ] Verify application runs in development: `npm run start`
- [ ] Create a backup branch: `git checkout -b backup/before-angular-21-migration`
- [ ] Document current package versions in `package.json`

### Environment Requirements

- **Node.js**: ^20.11.1 || ^22.0.0 (Angular 21 requires Node.js v20.11.1 or later)
- **npm**: ^9.0.0 || ^10.0.0
- **Angular CLI**: ^21.0.0
- **TypeScript**: ^5.7.0

**Critical Note**: Angular 21 continues the stricter Node.js requirements from Angular 20. Using Node.js 18 or versions below 20.11.1 will cause build failures.

### Current State Assessment

- [ ] Count total components: `find src -name "*.component.ts" | wc -l`
- [ ] Identify module-based components vs standalone
- [ ] Document custom EAF framework usages
- [ ] List all third-party libraries and their current versions
- [ ] Identify components extending `AppComponentBase`

---

## Step 1: Update Dependencies

### 1.1 Update Angular Core Packages

Update all Angular packages from ^20.0.0 to ^21.0.0:

```bash
# Update Angular core packages
npm install @angular/animations@^21.0.0 \
  @angular/common@^21.0.0 \
  @angular/compiler@^21.0.0 \
  @angular/core@^21.0.0 \
  @angular/forms@^21.0.0 \
  @angular/platform-browser@^21.0.0 \
  @angular/platform-browser-dynamic@^21.0.0 \
  @angular/platform-server@^21.0.0 \
  @angular/router@^21.0.0 \
  @angular/service-worker@^21.0.0 \
  @angular/pwa@^21.0.0 \
  @angular/cdk@^21.0.0 \
  @angular-devkit/core@^21.0.0
```

### 1.2 Update Angular CLI and DevKit - CRITICAL CHANGES

**MAJOR BREAKING CHANGES in Angular 21:**

**1. Vitest is Now Default (Karma Deprecated)**
Angular 21 has officially swapped Karma for Vitest as the default test runner.

```bash
# Update Angular CLI and build tools
npm install @angular/cli@^21.0.0 \
  @angular/build@^21.0.0 \
  @angular/compiler-cli@^21.0.0 \
  @angular-eslint/builder@^21.0.0 \
  @angular-eslint/eslint-plugin@^21.0.0 \
  @angular-eslint/eslint-plugin-template@^21.0.0 \
  @angular-eslint/schematics@^21.0.0 \
  @angular-eslint/template-parser@^21.0.0
```

**What breaks:**
- If you have a custom `karma.conf.js` or rely on specific Karma plugins/reporters, your test suite is now legacy code
- The CLI will nag you to migrate

**Migration Options:**
- **New Projects**: You get Vitest out of the box (faster, cleaner, uses Vite)
- **Existing Projects**: You aren't forced to switch immediately, but migration is recommended
- **Auto-migration**: Run the schematic to attempt automatic conversion:
  ```bash
  ng generate @angular/core:karma-to-vitest
  ```
  This is remarkably good at converting standard configs, but custom Webpack hacks in your test setup will need manual rewriting for Vite.

**2. HttpClient is Now Default**
HttpClient is now injected by default in the root injector.

**What breaks:**
- Tests that mock HttpClient by expecting it not to be there might fail
- If you rely on HttpClientModule for complex interceptor ordering in a mixed NgModule/Standalone app, you might see subtle behavior changes

**The Fix:**
Remove explicit `provideHttpClient()` calls unless you are passing configuration options (like `withInterceptors` or `withFetch`). Check your interceptor execution order.

```typescript
// Before
provideHttpClient(withInterceptors([myInterceptor]))

// After (if no config needed)
// Remove the call entirely - HttpClient is now provided by default
```

**3. zone.js is Gone for New Apps**
New apps generated with `ng new` will exclude zone.js by default.

**What breaks:**
- Nothing for existing apps (yet). Your `polyfills.ts` will keep importing Zone
- If you copy-paste code from a new v21 tutorial into your existing v20 app, it might assume Zoneless behavior (using ChangeDetectorRef less often, relying on Signals)
- Mixing the two paradigms without understanding them can cause "changed after checked" errors or views that don't update

**Recommendation for EAF Template**: Keep zone.js for now. Zoneless migration should be a separate, planned refactoring effort.

### 1.3 Update TypeScript

```bash
npm install typescript@~5.7.0
```

### 1.4 Update PrimeNG

PrimeNG 21 has significant changes for Angular 21 compatibility:

```bash
npm install primeng@^21.0.0
```

**PrimeNG 21 Breaking Changes:**
- All PrimeNG components are now standalone by default
- Module imports (e.g., `TableModule`) are deprecated in favor of direct component imports
- New import pattern:

```typescript
// Before (Angular 20 + PrimeNG 20)
import { TableModule } from 'primeng/table';

@NgModule({
  imports: [TableModule]
})

// After (Angular 21 + PrimeNG 21)
import { Table, Column, Row } from 'primeng/table';

@Component({
  standalone: true,
  imports: [Table, Column, Row]
})
```

### 1.5 Verify Third-Party Library Compatibility

| Library | Current | Angular 21 Compatible | Action |
|---------|---------|----------------------|--------|
| ngx-bootstrap | ^13.0.0 | Verify | Check for v14+ |
| ngx-image-cropper | ^9.1.6 | Yes | No change needed |
| angular-calendar | ^0.31.0 | Unknown | Verify before upgrade |
| @ng-select/ng-select | ^13.0.0 | Verify | Check for v14+ |
| ngx-mask | ^17.0.0 | Verify | Check for v18+ |
| ngx-cookie-service | ^20.0.0 | Yes | Update to ^21.0.0 |
| @microsoft/signalr | ^7.0.14 | Yes | No change needed |

### 1.6 Updated package.json Dependencies

```json
{
  "dependencies": {
    "@angular/animations": "^21.0.0",
    "@angular/common": "^21.0.0",
    "@angular/compiler": "^21.0.0",
    "@angular/core": "^21.0.0",
    "@angular/forms": "^21.0.0",
    "@angular/platform-browser": "^21.0.0",
    "@angular/platform-browser-dynamic": "^21.0.0",
    "@angular/platform-server": "^21.0.0",
    "@angular/pwa": "^21.0.0",
    "@angular/router": "^21.0.0",
    "@angular/service-worker": "^21.0.0",
    "@angular/cdk": "^21.0.0",
    "@angular-devkit/core": "^21.0.0",
    "primeng": "^21.0.0",
    "typescript": "~5.7.0"
  },
  "devDependencies": {
    "@angular/cli": "^21.0.0",
    "@angular-devkit/build-angular": "^21.0.0",
    "@angular/compiler-cli": "^21.0.0",
    "@angular-eslint/builder": "^21.0.0",
    "@angular-eslint/eslint-plugin": "^21.0.0",
    "@angular-eslint/eslint-plugin-template": "^21.0.0",
    "@angular-eslint/schematics": "^21.0.0",
    "@angular-eslint/template-parser": "^21.0.0"
  }
}
```

---

## Step 2: Breaking Changes

### Angular 21 Breaking Changes

#### 1. Enhanced Standalone Components

- **Impact**: Standalone components continue to be the default pattern
- **EAF Impact**: Continue migration of remaining module-based components
- **Priority**: Medium (can migrate gradually)

#### 2. TypeScript 5.7 Required

- **Impact**: Stricter type checking, new TypeScript features
- **EAF Impact**: May require type definition updates
- **Priority**: High (must update)

#### 3. Router Updates

- **Impact**: Router improvements and new features
- **EAF Impact**: Minimal; current routing should continue to work
- **Priority**: Low

#### 4. Forms API Enhancements

- **Impact**: Enhanced reactive forms with better type safety
- **EAF Impact**: May require updates to custom form implementations
- **Priority**: Medium

#### 5. Performance Improvements

- **Impact**: Better runtime performance and build optimization
- **EAF Impact**: Positive impact, no code changes required
- **Priority**: Low

#### 6. SSR Improvements

- **Impact**: Enhanced server-side rendering capabilities
- **EAF Impact**: If using SSR, verify compatibility
- **Priority**: Low (if SSR is used)

### PrimeNG 21 Breaking Changes

#### 1. All Components Standalone

- **Impact**: Module imports deprecated
- **EAF Impact**: All PrimeNG imports must be updated
- **Priority**: High (must update)

#### 2. Component Import Changes

- **Impact**: Import individual components instead of modules
- **EAF Impact**: Update all component files
- **Priority**: High (must update)

#### 3. Theming Updates

- **Impact**: New theming system may require CSS updates
- **EAF Impact**: Check custom styles in `styles.css`
- **Priority**: Medium

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

**Action**: Verify these paths work with Angular 21's stricter module resolution.

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

**Angular 21 Compatibility**: This pattern is fully compatible with Angular 21. No changes needed.

### 3.4 jQuery Integration

The EAF template uses jQuery for DOM manipulation:

```typescript
// File: src/assets/lib/eaf-web-resources/Eaf/Framework/scripts/libs/eaf.jquery.js
```

**Angular 21 Considerations**:
- jQuery integration should continue to work
- Test all jQuery-dependent features after migration
- Watch for change detection conflicts

### 3.5 Service Proxy Pattern

Auto-generated service proxies in `src/shared/service-proxies/`:

- Use Blob processing for file downloads
- Integrate with `EafHttpInterceptor`
- No changes required for Angular 21

---

## Step 4: Third-Party Library Updates

### 4.1 ngx-bootstrap

ngx-bootstrap 13.0.0 should be compatible with Angular 21, but verify:

```bash
# Check for updates
npm info ngx-bootstrap versions

# If v14+ is available with Angular 21 support:
npm install ngx-bootstrap@^14.0.0
```

### 4.2 ngx-image-cropper

ngx-image-cropper ^9.1.6 is already compatible with Angular 21:

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

angular-calendar ^0.31.0 compatibility with Angular 21 is unknown:

```bash
# Check latest version
npm info angular-calendar versions

# Verify Angular 21 support before updating
```

**Alternative**: If not compatible, consider:
- @fullcalendar/angular
- PrimeNG Calendar/DatePicker components

### 4.4 @ng-select/ng-select

```bash
# Check for Angular 21 compatible version
npm info @ng-select/ng-select versions

# Update if v14+ available
npm install @ng-select/ng-select@^14.0.0
```

### 4.5 ngx-mask

```bash
# Check for Angular 21 compatible version
npm info ngx-mask versions

# Update if v18+ available
npm install ngx-mask@^18.0.0
```

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
git checkout backup/before-angular-21-migration -- package.json

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
git checkout backup/before-angular-21-migration

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

**Cause**: Angular 21 stricter module resolution

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
// Correct import for PrimeNG 21
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

**Cause**: ngx-bootstrap version incompatible with Angular 21

**Solution**:
```bash
# Update to latest version
npm install ngx-bootstrap@latest

# Or use --legacy-peer-deps if needed
npm install ngx-bootstrap@latest --legacy-peer-deps
```

#### Issue 5: jQuery/$ not defined

**Cause**: jQuery types not recognized in Angular 21

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
| 4. Component Migration | 5-10 days | Migrate remaining components to standalone |
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

- [ ] All dependencies updated to Angular 21 compatible versions
- [ ] Application builds without errors: `npm run build`
- [ ] All tests pass: `npm test`
- [ ] Development server runs: `npm run start`
- [ ] All components render correctly
- [ ] PrimeNG components updated to standalone imports
- [ ] EAF framework integration tested
- [ ] SignalR real-time features working
- [ ] jQuery-dependent features working
- [ ] Authentication flow tested
- [ ] Documentation updated

---

## Resources and References

### Official Documentation

- [Angular 21 Release Notes](https://github.com/angular/angular/releases/tag/21.0.0)
- [Angular Update Guide](https://update.angular.io/?l=2&v=20.0-21.0)
- [Standalone Components Guide](https://angular.dev/guide/standalone-components)
- [PrimeNG 21 Migration](https://primeng.org/migration)

### EAF-Specific Resources

- `MIGRATION_SUMMARY_20.md` - Previous Angular 19→20 migration notes
- `MIGRATION_ANGULAR_19_TO_20.md` - Detailed 19→20 guide
- EAF Framework Documentation (if available)

---

## Last Updated

**Date**: May 23, 2026
**Version**: 1.0 (Initial EAF Template Edition)
**Angular Source**: 20.0.0
**Angular Target**: 21.0.0
**Components to Migrate**: 59+
**Maintainer**: EAF Development Team
