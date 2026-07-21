# Migration Guide: Angular 19 to Angular 20

## EAF Angular UI Template

This document provides a comprehensive step-by-step guide for migrating the EAF Angular UI Template from Angular 19 to Angular 20, including specific considerations for the EAF framework, standalone components migration, and third-party library compatibility.

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

| Aspect | Current State | Target (Angular 20) |
|--------|---------------|---------------------|
| Angular Version | ^19.0.0 | ^20.0.0 |
| TypeScript | ~5.5.0 | ~5.6.0 |
| PrimeNG | ^19.0.0 | ^20.0.0 |
| ngx-bootstrap | ^12.0.0 | ^13.0.0 (verify compatibility) |
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

- [ ] Ensure current Angular 19 application builds successfully: `npm run build`
- [ ] Run all tests and ensure they pass: `npm test`
- [ ] Verify application runs in development: `npm run start`
- [ ] Create a backup branch: `git checkout -b backup/before-angular-20-migration`
- [ ] Document current package versions in `package.json`

### Environment Requirements

- **Node.js**: ^20.11.1 (Angular 20 requires Node.js v20.11.1 or later - stricter than Angular 19)
- **npm**: ^9.0.0 || ^10.0.0
- **Angular CLI**: ^20.0.0
- **TypeScript**: ^5.6.0

**Critical Note**: Angular 20 is stricter about Node.js versions. Using Node.js 18 or versions below 20.11.1 will cause build failures.

### Current State Assessment

- [ ] Count total components: `find src -name "*.component.ts" | wc -l`
- [ ] Identify module-based components vs standalone
- [ ] Document custom EAF framework usages
- [ ] List all third-party libraries and their current versions
- [ ] Identify components extending `AppComponentBase`

---

## Step 1: Update Dependencies

### 1.1 Update Angular Core Packages

Update all Angular packages from ^19.0.0 to ^20.0.0:

```bash
# Update Angular core packages
npm install @angular/animations@^20.0.0 \
  @angular/common@^20.0.0 \
  @angular/compiler@^20.0.0 \
  @angular/core@^20.0.0 \
  @angular/forms@^20.0.0 \
  @angular/platform-browser@^20.0.0 \
  @angular/platform-browser-dynamic@^20.0.0 \
  @angular/platform-server@^20.0.0 \
  @angular/router@^20.0.0 \
  @angular/service-worker@^20.0.0 \
  @angular/pwa@^20.0.0 \
  @angular/cdk@^20.0.0 \
  @angular-devkit/core@^20.0.0
```

### 1.2 Update Angular CLI and DevKit - CRITICAL CHANGE

**MAJOR BREAKING CHANGE**: Angular 20 changes the default build package from `@angular-devkit/build-angular` to the new `@angular/build`. This new package no longer includes the Karma plugin used by legacy test setups.

```bash
# Update Angular CLI and build tools
npm install @angular/cli@^20.0.0 \
  @angular/build@^20.0.0 \
  @angular/compiler-cli@^20.0.0 \
  @angular-eslint/builder@^20.0.0 \
  @angular-eslint/eslint-plugin@^20.0.0 \
  @angular-eslint/eslint-plugin-template@^20.0.0 \
  @angular-eslint/schematics@^20.0.0 \
  @angular-eslint/template-parser@^20.0.0
```

**Karma Removal Impact:**
- The web ecosystem has moved to faster test runners like Vitest and Jest
- Karma had become a bottleneck and is no longer included by default
- Angular's experimental test runner, now powered by Vitest, is the future

**Temporary Fix for Karma (if needed):**
If you need to keep using Karma temporarily, you can force the CLI to fall back to the old compiler:

```bash
# Reinstall old builder with Karma support (temporary fix)
npm install @angular-devkit/build-angular@^19.0.0
```

**Note**: This is a compatibility bridge - start planning your migration to Jest or Vitest soon.

### 1.3 Update TypeScript

```bash
npm install typescript@~5.6.0
```

### 1.4 Update PrimeNG

PrimeNG 20 has significant changes for Angular 20 compatibility:

```bash
npm install primeng@^20.0.0
```

**PrimeNG 20 Breaking Changes:**
- All PrimeNG components are now standalone by default
- Module imports (e.g., `TableModule`) are deprecated in favor of direct component imports
- New import pattern:

```typescript
// Before (Angular 19 + PrimeNG 19)
import { TableModule } from 'primeng/table';

@NgModule({
  imports: [TableModule]
})

// After (Angular 20 + PrimeNG 20)
import { Table, Column, Row } from 'primeng/table';

@Component({
  standalone: true,
  imports: [Table, Column, Row]
})
```

### 1.5 Verify Third-Party Library Compatibility

| Library | Current | Angular 20 Compatible | Action |
|---------|---------|----------------------|--------|
| ngx-bootstrap | ^12.0.0 | Verify | Check for v13+ |
| ngx-image-cropper | ^9.1.6 | Yes | No change needed |
| angular-calendar | ^0.31.0 | Unknown | Verify before upgrade |
| @ng-select/ng-select | ^12.0.0 | Verify | Check for v13+ |
| ngx-mask | ^15.0.0 | Verify | Check for v17+ |
| ngx-cookie-service | ^18.0.0 | Yes | Update to ^20.0.0 |
| @microsoft/signalr | ^7.0.14 | Yes | No change needed |

### 1.6 Updated package.json Dependencies

```json
{
  "dependencies": {
    "@angular/animations": "^20.0.0",
    "@angular/common": "^20.0.0",
    "@angular/compiler": "^20.0.0",
    "@angular/core": "^20.0.0",
    "@angular/forms": "^20.0.0",
    "@angular/platform-browser": "^20.0.0",
    "@angular/platform-browser-dynamic": "^20.0.0",
    "@angular/platform-server": "^20.0.0",
    "@angular/pwa": "^20.0.0",
    "@angular/router": "^20.0.0",
    "@angular/service-worker": "^20.0.0",
    "@angular/cdk": "^20.0.0",
    "@angular-devkit/core": "^20.0.0",
    "primeng": "^20.0.0",
    "typescript": "~5.6.0"
  },
  "devDependencies": {
    "@angular/cli": "^20.0.0",
    "@angular-devkit/build-angular": "^20.0.0",
    "@angular/compiler-cli": "^20.0.0",
    "@angular-eslint/builder": "^20.0.0",
    "@angular-eslint/eslint-plugin": "^20.0.0",
    "@angular-eslint/eslint-plugin-template": "^20.0.0",
    "@angular-eslint/schematics": "^20.0.0",
    "@angular-eslint/template-parser": "^20.0.0"
  }
}
```

---

## Step 2: Breaking Changes

### Angular 20 Breaking Changes

#### 1. Enhanced Standalone Components

- **Impact**: Standalone components continue to be the default pattern
- **EAF Impact**: Continue migration of remaining module-based components
- **Priority**: Medium (can migrate gradually)

#### 2. TypeScript 5.6 Required

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

### PrimeNG 20 Breaking Changes

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

**Action**: Verify these paths work with Angular 20's stricter module resolution.

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

**Angular 20 Compatibility**: This pattern is fully compatible with Angular 20. No changes needed.

### 3.4 jQuery Integration

The EAF template uses jQuery for DOM manipulation:

```typescript
// File: src/assets/lib/eaf-web-resources/Eaf/Framework/scripts/libs/eaf.jquery.js
```

**Angular 20 Considerations**:
- jQuery integration should continue to work
- Test all jQuery-dependent features after migration
- Watch for change detection conflicts

### 3.5 Service Proxy Pattern

Auto-generated service proxies in `src/shared/service-proxies/`:

- Use Blob processing for file downloads
- Integrate with `EafHttpInterceptor`
- No changes required for Angular 20

---

## Step 4: Third-Party Library Updates

### 4.1 ngx-bootstrap

ngx-bootstrap 12.0.0 should be compatible with Angular 20, but verify:

```bash
# Check for updates
npm info ngx-bootstrap versions

# If v13+ is available with Angular 20 support:
npm install ngx-bootstrap@^13.0.0
```

### 4.2 ngx-image-cropper

ngx-image-cropper ^9.1.6 is already compatible with Angular 20:

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

angular-calendar ^0.31.0 compatibility with Angular 20 is unknown:

```bash
# Check latest version
npm info angular-calendar versions

# Verify Angular 20 support before updating
```

**Alternative**: If not compatible, consider:
- @fullcalendar/angular
- PrimeNG Calendar/DatePicker components

### 4.4 @ng-select/ng-select

```bash
# Check for Angular 20 compatible version
npm info @ng-select/ng-select versions

# Update if v13+ available
npm install @ng-select/ng-select@^13.0.0
```

### 4.5 ngx-mask

```bash
# Check for Angular 20 compatible version
npm info ngx-mask versions

# Update if v17+ available
npm install ngx-mask@^17.0.0
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
git checkout backup/before-angular-20-migration -- package.json

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
git checkout backup/before-angular-20-migration

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

**Cause**: Angular 20 stricter module resolution

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
// Correct import for PrimeNG 20
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

**Cause**: ngx-bootstrap version incompatible with Angular 20

**Solution**:
```bash
# Update to latest version
npm install ngx-bootstrap@latest

# Or use --legacy-peer-deps if needed
npm install ngx-bootstrap@latest --legacy-peer-deps
```

#### Issue 5: jQuery/$ not defined

**Cause**: jQuery types not recognized in Angular 20

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

- [ ] All dependencies updated to Angular 20 compatible versions
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

- [Angular 20 Release Notes](https://github.com/angular/angular/releases/tag/20.0.0)
- [Angular Update Guide](https://update.angular.io/?l=2&v=19.0-20.0)
- [Standalone Components Guide](https://angular.dev/guide/standalone-components)
- [PrimeNG 20 Migration](https://primeng.org/migration)

### EAF-Specific Resources

- `MIGRATION_SUMMARY_19.md` - Previous Angular 18→19 migration notes
- `MIGRATION_ANGULAR_18_TO_19.md` - Detailed 18→19 guide
- EAF Framework Documentation (if available)

---

## Last Updated

**Date**: May 23, 2026
**Version**: 1.0 (Initial EAF Template Edition)
**Angular Source**: 19.0.0
**Angular Target**: 20.0.0
**Components to Migrate**: 59+
**Maintainer**: EAF Development Team
