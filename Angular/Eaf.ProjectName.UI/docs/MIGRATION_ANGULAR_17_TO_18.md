# Migration Guide: Angular 17 to Angular 18

## EAF Angular UI Template

This document provides a step-by-step guide for migrating the EAF Angular UI Template from Angular 17 to Angular 18.

---

## Table of Contents

1. [Pre-Migration Checklist](#pre-migration-checklist)
2. [Step 1: Angular 17 to 18](#step-1-angular-17-to-18)
3. [Dependency Updates](#dependency-updates)
4. [Breaking Changes Summary](#breaking-changes-summary)
5. [Testing Strategy](#testing-strategy)
6. [Rollback Procedures](#rollback-procedures)

---

## Pre-Migration Checklist

### Prerequisites

- [ ] Ensure current Angular 17 application is stable and all tests pass
- [ ] Create a backup of the current codebase
- [ ] Update Node.js to compatible version (Node.js 18.x or 20.x recommended)
- [ ] Clear npm cache: `npm cache clean --force`
- [ ] Remove node_modules and package-lock.json: `rm -rf node_modules package-lock.json`

### Environment Requirements

- **Node.js**: ^18.13.0 || ^20.9.0
- **npm**: ^9.0.0 || ^10.0.0
- **Angular CLI**: ^18.0.0

### Current State Assessment

- [ ] Document current package versions
- [ ] Note any custom configurations or workarounds
- [ ] Identify deprecated APIs in use
- [ ] Check for third-party library compatibility

---

## Step 1: Angular 17 to 18

### 1.1 Update Core Dependencies

Update Angular core packages to version 18:

```json
{
  "@angular/animations": "^18.0.0",
  "@angular/common": "^18.0.0",
  "@angular/compiler": "^18.0.0",
  "@angular/core": "^18.0.0",
  "@angular/forms": "^18.0.0",
  "@angular/platform-browser": "^18.0.0",
  "@angular/platform-browser-dynamic": "^18.0.0",
  "@angular/router": "^18.0.0"
}
```

### 1.2 Update Angular CLI

```json
{
  "@angular/cli": "^18.0.0",
  "@angular-devkit/build-angular": "^18.0.0"
}
```

### 1.3 Update Angular CDK

```json
{
  "@angular/cdk": "^18.0.0"
}
```

### 1.4 Breaking Changes - Angular 17 to 18

#### New Control Flow Syntax (@if/@for/@switch)

- **Change**: New built-in control flow blocks replace *ngIf, *ngFor, *ngSwitch
- **Action**: Consider migrating to new syntax (optional but recommended)
- **Impact**: Performance improvements, better type safety
- **Migration**:

```typescript
// Before
<div *ngIf="isVisible">Content</div>
<div *ngFor="let item of items">{{ item.name }}</div>

// After
@if (isVisible) {
  <div>Content</div>
}
@for (item of items; track item.id) {
  <div>{{ item.name }}</div>
}
```

#### Deferred Loading

- **Change**: New @defer block for lazy loading content
- **Action**: No immediate action required, but consider for performance
- **Impact**: Improved initial load performance
- **Migration**:

```typescript
@defer (on viewport) {
  <app-heavy-component />
} @placeholder {
  <div>Loading...</div>
}
```

#### Signals Enhancements

- **Change**: Signals become more stable and feature-complete
- **Action**: Consider migrating from RxJS to Signals where appropriate
- **Impact**: Performance improvements, simplified reactive patterns

#### TypeScript Version Update

- **Change**: TypeScript 5.4+ required
- **Action**: Update TypeScript configuration
- **Impact**: Stricter type checking, improved developer experience

### 1.5 Third-Party Library Updates

Update commonly used libraries:

```json
{
  "ngx-bootstrap": "^10.2.0",
  "primeng": "^17.17.0",
  "rxjs": "^7.8.0",
  "zone.js": "^0.14.0"
}
```

### 1.6 Code Updates

#### Update SwUpdate API (if still using old API)

```typescript
// Angular 17+ already uses versionUpdates, ensure it's implemented
constructor(public updates: SwUpdate) {
  if (updates.isEnabled) {
    updates.versionUpdates.pipe(
      filter((event): event is VersionReadyEvent => event.type === 'VERSION_READY')
    ).subscribe(event => {
      console.log('current version is', event.currentVersion);
      console.log('available version is', event.latestVersion);
      updates.activateUpdate().then(() => this.updateApp());
    });
  }
}
```

---

## Dependency Updates

### Core Dependencies

#### Angular Packages (Angular 18)

```json
{
  "@angular/animations": "^18.0.0",
  "@angular/common": "^18.0.0",
  "@angular/compiler": "^18.0.0",
  "@angular/core": "^18.0.0",
  "@angular/forms": "^18.0.0",
  "@angular/platform-browser": "^18.0.0",
  "@angular/platform-browser-dynamic": "^18.0.0",
  "@angular/router": "^18.0.0"
}
```

#### Development Dependencies

```json
{
  "@angular/cli": "^18.0.0",
  "@angular-devkit/build-angular": "^18.0.0",
  "@angular/compiler-cli": "^18.0.0",
  "@angular-eslint/builder": "^18.0.0",
  "@angular-eslint/eslint-plugin": "^18.0.0",
  "@angular-eslint/eslint-plugin-template": "^18.0.0",
  "@angular-eslint/schematics": "^18.0.0",
  "@angular-eslint/template-parser": "^18.0.0",
  "@types/node": "^20.0.0",
  "typescript": "~5.4.0"
}
```

### Third-Party Libraries

#### UI Libraries

```json
{
  "ngx-bootstrap": "^10.2.0",
  "primeng": "^17.17.0",
  "@swimlane/ngx-charts": "^20.0.0"
}
```

#### Utility Libraries

```json
{
  "rxjs": "^7.8.0",
  "zone.js": "^0.14.0",
  "moment": "^2.30.0",
  "lodash": "^4.17.0"
}
```

#### Testing Libraries

```json
{
  "@types/jasmine": "^5.1.0",
  "jasmine-core": "^5.1.0",
  "karma": "^6.4.0",
  "karma-chrome-launcher": "^3.2.0",
  "karma-coverage": "^2.2.0"
}
```

---

## Breaking Changes Summary

### Major Breaking Changes

#### 1. New Control Flow Syntax (Angular 18)

- **Impact**: *ngIf, *ngFor, *ngSwitch still supported but new syntax recommended
- **Migration**: Gradual migration possible
- **Priority**: Medium

#### 2. Deferred Loading (Angular 18)

- **Impact**: New feature for lazy loading
- **Migration**: Optional for existing code
- **Priority**: Low

### Minor Breaking Changes

#### TypeScript Version

- **Change**: TypeScript 5.4+ required
- **Impact**: Stricter type checking
- **Action**: Update type definitions

#### RxJS Updates

- **Change**: RxJS 7.8+ recommended
- **Impact**: Deprecated operators removed
- **Action**: Update RxJS usage

#### Zone.js Updates

- **Change**: Zone.js 0.14+ required
- **Impact**: Performance improvements
- **Action**: No code changes required

---

## Testing Strategy

### Pre-Migration Testing

1. **Baseline Test Suite**
   - Run full test suite on Angular 17
   - Document any failing tests
   - Fix critical issues before migration

2. **Performance Benchmarking**
   - Measure build times
   - Test application startup time
   - Document memory usage

### Migration Testing

1. **Step-by-Step Validation**
   - Test after Angular 17 to 18 upgrade
   - Run full test suite after upgrade

2. **Component Testing**
   - Test all UI components
   - Verify form functionality
   - Check routing and navigation
   - Test control flow syntax if migrated

3. **Integration Testing**
   - Test API integrations
   - Verify authentication flows
   - Test third-party library integrations
   - Test SignalR real-time features

### Post-Migration Testing

1. **Regression Testing**
   - Full application test suite
   - Cross-browser compatibility
   - Mobile responsiveness testing

2. **Performance Testing**
   - Build performance comparison
   - Runtime performance metrics
   - Bundle size analysis

3. **User Acceptance Testing**
   - Core user workflows
   - Admin panel functionality
   - Data visualization components

---

## Rollback Procedures

### Immediate Rollback (Within 24 hours)

1. **Git Rollback**

```bash
git checkout main
git branch -D migration/angular-17-to-18
git checkout -b rollback/angular-17-restore
git reset --hard [pre-migration-commit]
```

2. **Environment Restoration**

```bash
rm -rf node_modules package-lock.json
npm install
npm run build
```

### Partial Rollback

**Angular 18 to 17**

```bash
# Update package.json to Angular 17 versions
npm install
npm run build
```

### Rollback Validation

1. **Build Verification**
   - Application builds successfully
   - No compilation errors
   - All tests pass

2. **Functionality Verification**
   - Core features working
   - No UI regressions
   - Performance acceptable

---

## Migration Timeline

### Phase 1: Preparation (1-2 days)

- [ ] Environment setup
- [ ] Codebase analysis
- [ ] Dependency compatibility check
- [ ] Backup creation

### Phase 2: Angular 17 to 18 (2-3 days)

- [ ] Dependency updates
- [ ] Code migration (control flow syntax if desired)
- [ ] Testing and validation
- [ ] Issue resolution

### Phase 3: Final Validation (1-2 days)

- [ ] Full regression testing
- [ ] Performance testing
- [ ] Documentation updates
- [ ] Production deployment preparation

---

## Common Issues and Solutions

### Dependency Conflicts

#### Issue: ng-recaptcha Peer Dependency Conflict

**Symptoms**: npm install fails with ERESOLVE error for ng-recaptcha@11.0.0 requiring @angular/core@"^15.0.0"

**Solution**: Update ng-recaptcha to version compatible with Angular 17+

```bash
# Update package.json
# Change "ng-recaptcha": "^11.0.0" to "ng-recaptcha": "^13.0.0"

npm install ng-recaptcha@^13.0.0
```

**Explanation**: ng-recaptcha v11 requires Angular 15, but the project uses Angular 17. Version 13+ supports Angular 17.

### Build Issues

#### Issue: TypeScript Compilation Errors

**Symptoms**: Type errors after upgrade

**Solution**:

```bash
# Update TypeScript configurations
npm install typescript@~5.4.0
# Check tsconfig.json for deprecated options
```

#### Issue: Module Resolution Problems

**Symptoms**: Cannot find module errors

**Solution**:

```json
// Update tsconfig.json
{
  "compilerOptions": {
    "moduleResolution": "node",
    "esModuleInterop": true
  }
}
```

### Runtime Issues

#### Issue: Control Flow Syntax Errors

**Symptoms**: Template syntax errors after migrating to @if/@for

**Solution**:
- Ensure Angular compiler is updated to v18+
- Check for proper syntax in templates
- Use ng generate @angular/common:control-flow to migrate automatically

### Performance Issues

#### Issue: Slow Build Times

**Symptoms**: Builds taking longer than before

**Solution**:
- Consider migrating to Vite build system
- Enable incremental builds
- Check for circular dependencies

#### Issue: Runtime Performance Degradation

**Symptoms**: Application slower after migration

**Solution**:
- Enable production mode
- Consider zoneless mode (Angular 19)
- Optimize change detection with Signals

---

## Post-Migration Optimizations

### Performance Enhancements

1. **Migrate to New Control Flow Syntax**
   - Replace *ngIf, *ngFor, *ngSwitch with @if, @for, @switch
   - Improved performance and type safety
   - Better tree-shaking

2. **Adopt Deferred Loading**
   - Lazy load heavy components
   - Improved initial load performance
   - Better user experience

### Code Quality Improvements

1. **Update to Modern APIs**
   - Use new Angular features
   - Implement best practices
   - Remove deprecated code

2. **Enhanced Type Safety**
   - Strict TypeScript configuration
   - Better type definitions
   - Improved developer experience

---

## Resources and References

### Official Documentation

- [Angular 18 Release Notes](https://github.com/angular/angular/releases/tag/18.0.0)
- [Angular Update Guide](https://update.angular.io/)
- [Control Flow Guide](https://angular.dev/guide/templates/control-flow)

### Community Resources

- [Angular Blog](https://blog.angular.dev/)
- [Angular Discord](https://discord.gg/angular)
- [Stack Overflow Angular Tag](https://stackoverflow.com/questions/tagged/angular)

### Migration Tools

- [Angular Schematics](https://angular.io/guide/schematics)
- [Angular CLI Migration Commands](https://angular.io/cli/update)
- [NG Update](https://ng-update.dev/)

---

## Support and Troubleshooting

### Getting Help

1. **Angular Documentation**: Official guides and API reference
2. **Community Forums**: Stack Overflow and GitHub discussions
3. **Angular Team**: Official support channels

### Reporting Issues

1. **Bug Reports**: Use GitHub issue templates
2. **Feature Requests**: Submit enhancement proposals
3. **Security Issues**: Follow responsible disclosure

---

## EAF Application Analysis - Current State vs Migration Requirements

### Current Application State Analysis

#### Package Version Assessment (Angular 17)

Based on current `package.json` analysis after Angular 17 migration:

| Dependency | Current Version | Target Version (18) | Status | Risk Level |
|------------|------------------|---------------------|--------|------------|
| @angular/core | 17.0.0 | 18.0.0 | **NEEDS UPDATE** | HIGH |
| @angular/cli | 17.0.0 | 18.0.0 | **NEEDS UPDATE** | HIGH |
| @angular/cdk | 17.0.0 | 18.0.0 | **NEEDS UPDATE** | MEDIUM |
| primeng | 17.0.0 | 17.17.0 | **NEEDS UPDATE** | MEDIUM |
| ngx-bootstrap | 10.2.0 | 10.2.0 | **COMPATIBLE** | LOW |
| zone.js | 0.14.0 | 0.14.0 | **COMPATIBLE** | LOW |
| rxjs | 7.8.0 | 7.8.0 | **COMPATIBLE** | LOW |
| @swimlane/ngx-charts | 20.0.0 | 20.0.0 | **COMPATIBLE** | LOW |
| typescript | 5.2 | 5.4 | **NEEDS UPDATE** | MEDIUM |
| @types/node | 20.0.0 | 20.0.0 | **COMPATIBLE** | LOW |

### Breaking Changes Found in EAF Codebase

#### 1. Structural Directives Usage (Angular 18)

**Files Affected:**
- All component templates using *ngIf, *ngFor, *ngSwitch

**Current Code:**

```html
<div *ngIf="isVisible">Content</div>
<div *ngFor="let item of items; trackBy: trackById">{{ item.name }}</div>
<div *ngSwitch="status">
  <div *ngSwitchCase="'active'">Active</div>
  <div *ngSwitchDefault>Unknown</div>
</div>
```

**Required Change (Optional but Recommended):**

```html
@if (isVisible) {
  <div>Content</div>
}
@for (item of items; track item.id) {
  <div>{{ item.name }}</div>
}
@switch (status) {
  @case ('active') { <div>Active</div> }
  @default { <div>Unknown</div> }
}
```

**Impact:** Low - Old syntax still supported
**Priority:** Medium - Performance improvement

#### 2. SwUpdate API (Already Fixed in Angular 17)

**Status:** Already migrated to versionUpdates Observable in Angular 17
**Impact:** None
**Priority:** N/A

---

## Migration Complications and Risks

### High-Risk Complications

#### 1. EafHttpInterceptor Blob Processing (Critical - Lesson from Angular 17)

**Issue:** EafHttpInterceptor uses a manual Subject-based pattern to control async flow for Blob responses
**Risk:** Attempting to refactor to RxJS operators (switchMap/map) causes conflicts with service-proxies.ts Blob processing

**Error Pattern:**

```
TypeError: Failed to execute 'readAsText' on 'FileReader': parameter 1 is not of type 'Blob'.
Unexpected authenticateResult!
```

**Root Cause:** The service-proxies.ts also processes Blob responses using blobToText. Dual processing with RxJS operators leads to the error.

**Solution (Angular 17):**
- Reverted to original Subject-based pattern in EafHttpInterceptor
- Kept manual Subject control for async Blob processing
- Added conditional debug logs for localhost only

**Angular 18 Migration Guidance:**
- **DO NOT** attempt to refactor EafHttpInterceptor to RxJS operators (switchMap/map)
- The Subject-based pattern is compatible with Angular 17 and should work with Angular 18
- If issues arise, test the interceptor with Blob responses first
- The pattern is:

```typescript
intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
  const interceptObservable = new Subject<HttpEvent<any>>();
  // ... processing
  return interceptObservable;
}
```

**Files Affected:**
- `src/assets/lib/eaf-ng2-module/src/eafHttpInterceptor.ts`
- `src/assets/lib/eaf-web-resources/Eaf/Framework/scripts/libs/eaf.jquery.js`
- `src/account/login/login.service.ts`

**Priority:** CRITICAL - This is a known breaking point

#### 2. EAF Custom Module System

**Issue:** EAF uses custom module loading with `@eaf/*` path mappings
**Risk:** Path resolution may be affected by Angular 18's stricter module resolution
**Mitigation:** Test all EAF module imports and path mappings

#### 3. jQuery Integration

**Issue:** Heavy jQuery usage alongside Angular
**Risk:** Angular 18's change detection may conflict with jQuery DOM manipulation
**Mitigation:** Isolate jQuery usage and test Angular change detection

#### 4. Custom EAF Framework Integration

**Issue:** Custom EAF.js framework integration
**Files:** `src/assets/lib/eaf-web-resources/`
**Risk:** Angular 18's bootstrap process may affect EAF initialization
**Mitigation:** Test EAF framework initialization sequence

#### 5. SignalR Integration

**Issue:** @microsoft/signalr version compatibility
**Current:** ^7.0.14
**Risk:** Real-time features may break with Angular 18
**Mitigation:** Test all SignalR connections and real-time updates

### Medium-Risk Complications

#### 6. PrimeNG Component Compatibility

**Issue:** PrimeNG 17.17.0 may have breaking changes
**Risk:** Component styling and behavior may break
**Mitigation:** Test all PrimeNG components visually and functionally

#### 7. ngx-bootstrap Compatibility

**Issue:** ngx-bootstrap 10.2.0 compatibility with Angular 18
**Risk:** Modal and calendar components may break
**Mitigation:** Test all ngx-bootstrap components

#### 8. Control Flow Syntax Migration

**Issue:** Migrating to @if/@for/@switch may introduce errors
**Risk:** Template syntax errors
**Mitigation:** Use Angular CLI schematic for automatic migration

### Low-Risk Complications

#### 9. Build Configuration

**Issue:** Angular 18 build system changes
**Risk:** Build process may need adjustment
**Mitigation:** Test build in both development and production

#### 10. Testing Framework

**Issue:** Karma/Jasmine compatibility with Angular 18
**Risk:** Tests may fail after upgrade
**Mitigation:** Update test configurations if needed

---

## Last Updated

**Date**: April 12, 2026
**Version**: 1.0
**Maintainer**: EAF Development Team
