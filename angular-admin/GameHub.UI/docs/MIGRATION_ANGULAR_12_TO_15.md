# Migration Guide: Angular 12 to Angular 15

## EAF Angular UI Template

This document provides a step-by-step guide for migrating the EAF Angular UI Template from Angular 12 to Angular 15.

---

## Table of Contents

1. [Pre-Migration Checklist](#pre-migration-checklist)
2. [Step 1: Angular 12 to 13](#step-1-angular-12-to-13)
3. [Step 2: Angular 13 to 14](#step-2-angular-13-to-14)
4. [Step 3: Angular 14 to 15](#step-3-angular-14-to-15)
5. [Dependency Updates](#dependency-updates)
6. [Breaking Changes Summary](#breaking-changes-summary)
7. [Testing Strategy](#testing-strategy)
8. [Rollback Procedures](#rollback-procedures)
9. [Post-Migration Verification](#post-migration-verification)

---

## Pre-Migration Checklist

Before starting the migration, ensure the following:

- [ ] All unit tests pass on the current Angular 12 version
- [ ] Production build succeeds (`npm run build`)
- [ ] All changes are committed to version control
- [ ] Create a dedicated migration branch: `git checkout -b migration/angular-12-to-15`
- [ ] Back up `package.json` and `package-lock.json`
- [ ] Review the Angular Update Guide: https://update.angular.io/
- [ ] Ensure Node.js version compatibility (Angular 15 requires Node 14.20+ or 16.13+)
- [ ] Document all custom webpack configurations (if any)
- [ ] List all third-party dependencies and check their Angular 15 compatibility

### Current Dependencies to Track

| Package | Current Version | Notes |
|---------|----------------|-------|
| `@angular/core` | 12.2.17 | Core framework |
| `primeng` | 12.0.1 | Must upgrade to 15.x |
| `ngx-bootstrap` | 6.2.0 | Must upgrade to 10.x+ |
| `@angular-devkit/build-angular` | 0.1102.7 | Must upgrade |
| `angular-oauth2-oidc` | 12.1.0 | Must upgrade to 15.x |
| `ng-recaptcha` | 8.0.1 | Must upgrade |
| `ngx-perfect-scrollbar` | 10.1.1 | Deprecated - replace with `ngx-scrollbar` |
| `moment` | 2.29.1 | Consider replacing with `date-fns` or `luxon` |

---

## Step 1: Angular 12 to 13

### 1.1 Update Node.js

```bash
# Angular 13 requires Node.js 12.20+ or 14.15+ or 16.10+
# Current: Node 16.20.2 (compatible)
node -v
```

### 1.2 Update Angular CLI and Core

```bash
# Update Angular CLI globally
npm install -g @angular/cli@13

# Update project dependencies
ng update @angular/core@13 @angular/cli@13 --force
```

### 1.3 Breaking Changes in Angular 13

#### 1.3.1 View Engine Removal
Angular 13 removes the deprecated View Engine. All libraries must use Ivy.

**Action Required:**
- Verify all third-party libraries support Ivy
- Remove `enableIvy: false` from `tsconfig.json` (if present)
- Remove `"aot": false` from `angular.json` (if present)

```json
// tsconfig.json - REMOVE if present:
{
  "angularCompilerOptions": {
    "enableIvy": false  // REMOVE THIS LINE
  }
}
```

#### 1.3.2 TypeScript 4.4 Required
Update TypeScript to 4.4.x:

```bash
npm install typescript@4.4 --save-dev --legacy-peer-deps
```

#### 1.3.3 RxJS 7 Support
Angular 13 supports RxJS 7 but still works with RxJS 6. Plan to upgrade later.

#### 1.3.4 IE11 Support Removed
- Remove IE-specific polyfills from `polyfills.ts`
- Remove `browserslist` IE entries
- Remove `es5BrowserSupport` from `angular.json`

**File: `src/polyfills.ts`**
```typescript
// REMOVE these IE-specific polyfills:
// import 'core-js/es/symbol';
// import 'core-js/es/object';
// import 'classlist.js';
```

#### 1.3.5 `TestBed.initTestEnvironment` Changes
The `teardown` option is now available:

```typescript
// test.ts - Add teardown for better test isolation
getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
  { teardown: { destroyAfterEach: true } }  // ADD THIS
);
```

#### 1.3.6 Forms Changes
- `FormControlStatus` type added
- Ensure forms code uses proper typing

### 1.4 Update Third-Party Dependencies for Angular 13

```bash
npm install primeng@13 --save --legacy-peer-deps
npm install ngx-bootstrap@8 --save --legacy-peer-deps
npm install ng-recaptcha@9 --save --legacy-peer-deps
```

### 1.5 Verify Step 1

```bash
npm run build
npm run test
```

---

## Step 2: Angular 13 to 14

### 2.1 Update Angular CLI and Core

```bash
ng update @angular/core@14 @angular/cli@14 --force
```

### 2.2 Breaking Changes in Angular 14

#### 2.2.1 TypeScript 4.6 Required

```bash
npm install typescript@4.6 --save-dev --legacy-peer-deps
```

#### 2.2.2 Strictly Typed Forms (Major Change)
Angular 14 introduces strictly typed reactive forms. This is the **biggest breaking change** for the EAF template.

**Impact on EAF Template:**
- `LoginComponent` forms
- `CreateOrEditUserModalComponent` forms
- `CreateOrEditRoleModalComponent` forms
- `SettingsComponent` forms
- All modal components with forms

**Migration Strategy:**
Option A - Use `UntypedFormGroup` (Quick Migration):
```typescript
// BEFORE (Angular 12):
import { FormGroup, FormControl } from '@angular/forms';
const form = new FormGroup({...});

// AFTER (Angular 14 - Quick Migration):
import { UntypedFormGroup, UntypedFormControl } from '@angular/forms';
const form = new UntypedFormGroup({...});
```

Option B - Adopt Typed Forms (Recommended for new code):
```typescript
// AFTER (Angular 14 - Full Migration):
import { FormGroup, FormControl } from '@angular/forms';

interface LoginForm {
  username: FormControl<string>;
  password: FormControl<string>;
  rememberMe: FormControl<boolean>;
}

const form = new FormGroup<LoginForm>({
  username: new FormControl('', { nonNullable: true }),
  password: new FormControl('', { nonNullable: true }),
  rememberMe: new FormControl(false, { nonNullable: true }),
});
```

#### 2.2.3 Standalone Components Preview
Angular 14 introduces standalone components as a developer preview. No migration needed yet, but consider for future refactoring.

#### 2.2.4 `@angular/cli` Update
The CLI configuration format changes slightly:

**File: `angular.json`**
```json
{
  "projects": {
    "eaf-gamehub-ui": {
      "architect": {
        "build": {
          "builder": "@angular-devkit/build-angular:browser",
          // Verify "defaultConfiguration" is set
          "defaultConfiguration": "production"
        }
      }
    }
  }
}
```

#### 2.2.5 Router Changes
- `initialNavigation` type changes
- `relativeLinkResolution` is removed

**Check `app-routing.module.ts` and other routing modules:**
```typescript
// BEFORE:
RouterModule.forRoot(routes, {
  relativeLinkResolution: 'legacy'  // REMOVE THIS
})

// AFTER:
RouterModule.forRoot(routes)
```

#### 2.2.6 `title` Property in Routes
Angular 14 adds a `title` property to routes for automatic page title management.

### 2.3 Update Third-Party Dependencies for Angular 14

```bash
npm install primeng@14 --save --legacy-peer-deps
npm install ngx-bootstrap@9 --save --legacy-peer-deps
npm install angular-oauth2-oidc@14 --save --legacy-peer-deps
```

### 2.4 Verify Step 2

```bash
npm run build
npm run test
```

---

## Step 3: Angular 14 to 15

### 3.1 Update Angular CLI and Core

```bash
ng update @angular/core@15 @angular/cli@15 --force
```

### 3.2 Breaking Changes in Angular 15

#### 3.2.1 TypeScript 4.8 Required

```bash
npm install typescript@4.8 --save-dev --legacy-peer-deps
```

#### 3.2.2 Standalone Components Stable
Standalone components are now stable. Consider migrating key components:

```typescript
// BEFORE (NgModule-based):
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent { }

// AFTER (Standalone):
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class DashboardComponent { }
```

**Note:** This is optional. The EAF template can continue using NgModules.

#### 3.2.3 `@angular/router` Changes
- `RouterModule.forRoot()` no longer needs `relativeLinkResolution`
- `CanLoad` guard is deprecated, use `canMatch` instead

#### 3.2.4 Image Directive (`NgOptimizedImage`)
Angular 15 adds `NgOptimizedImage` for better image performance. Consider using it for profile pictures and theme logos.

#### 3.2.5 Functional Guards and Resolvers
Angular 15 encourages functional guards/resolvers instead of class-based ones:

```typescript
// BEFORE (Class-based guard):
@Injectable()
export class AuthGuard implements CanActivate {
  canActivate(): boolean { return true; }
}

// AFTER (Functional guard - Angular 15):
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  return authService.isLoggedIn();
};
```

#### 3.2.6 `esbuild` Support (Experimental)
Angular 15 introduces `esbuild` as an experimental build option. Can provide significant build speed improvements.

#### 3.2.7 MDC-based Angular Material Components
If using Angular Material, all components now use Material Design Components (MDC). This causes visual changes.

**Note:** The EAF template uses PrimeNG, not Angular Material, so this doesn't directly apply.

#### 3.2.8 Directive Composition API
Angular 15 introduces the Directive Composition API for reusing directive logic:

```typescript
@Component({
  hostDirectives: [CdkDrag]
})
```

### 3.3 Update Third-Party Dependencies for Angular 15

```bash
# PrimeNG 15
npm install primeng@15 --save --legacy-peer-deps

# ngx-bootstrap 10+
npm install ngx-bootstrap@10 --save --legacy-peer-deps

# angular-oauth2-oidc 15
npm install angular-oauth2-oidc@15 --save --legacy-peer-deps

# Replace deprecated ngx-perfect-scrollbar
npm uninstall ngx-perfect-scrollbar --legacy-peer-deps
npm install ngx-scrollbar@11 --save --legacy-peer-deps

# ng-recaptcha 11+
npm install ng-recaptcha@11 --save --legacy-peer-deps

# angular-google-tag-manager (check compatibility)
npm install angular-google-tag-manager@latest --save --legacy-peer-deps

# Update @azure/msal-browser
npm install @azure/msal-browser@latest --save --legacy-peer-deps

# Update @auth0/auth0-spa-js
npm install @auth0/auth0-spa-js@latest --save --legacy-peer-deps
```

### 3.4 Verify Step 3

```bash
npm run build
npm run test
```

---

## Dependency Updates

### Complete `package.json` Dependencies Update

```json
{
  "dependencies": {
    "@angular/animations": "~15.2.0",
    "@angular/cdk": "~15.2.0",
    "@angular/common": "~15.2.0",
    "@angular/compiler": "~15.2.0",
    "@angular/core": "~15.2.0",
    "@angular/forms": "~15.2.0",
    "@angular/localize": "~15.2.0",
    "@angular/platform-browser": "~15.2.0",
    "@angular/platform-browser-dynamic": "~15.2.0",
    "@angular/router": "~15.2.0",
    "@angular/service-worker": "~15.2.0",
    "primeng": "~15.4.0",
    "ngx-bootstrap": "~10.3.0",
    "angular-oauth2-oidc": "~15.0.0",
    "@microsoft/signalr": "~7.0.0",
    "@azure/msal-browser": "~2.38.0",
    "@auth0/auth0-spa-js": "~2.1.0",
    "rxjs": "~7.8.0",
    "zone.js": "~0.12.0",
    "moment": "~2.29.4",
    "lodash": "~4.17.21",
    "localforage": "~1.10.0"
  },
  "devDependencies": {
    "@angular-devkit/build-angular": "~15.2.0",
    "@angular/cli": "~15.2.0",
    "@angular/compiler-cli": "~15.2.0",
    "typescript": "~4.9.5",
    "karma": "~6.4.0",
    "karma-chrome-launcher": "~3.2.0",
    "karma-coverage": "~2.2.0",
    "karma-jasmine": "~5.1.0",
    "karma-jasmine-html-reporter": "~2.1.0",
    "jasmine-core": "~4.6.0"
  }
}
```

---

## Breaking Changes Summary

### High-Impact Changes

| Version | Change | Impact | Files Affected |
|---------|--------|--------|----------------|
| 13 | View Engine removed | Must ensure all deps use Ivy | `tsconfig.json`, all libraries |
| 13 | IE11 support removed | Remove IE polyfills | `polyfills.ts`, `browserslist` |
| 14 | Typed Forms | All form components need update | All components with reactive forms |
| 14 | `relativeLinkResolution` removed | Update router config | `*-routing.module.ts` |
| 15 | TypeScript 4.8+ required | May cause new type errors | All `.ts` files |

### Medium-Impact Changes

| Version | Change | Impact | Files Affected |
|---------|--------|--------|----------------|
| 13 | `TestBed` teardown | Better test isolation | `test.ts` |
| 14 | Standalone components | Optional refactoring | Any component |
| 15 | Functional guards | Optional migration | Route guards |
| 15 | `ngx-perfect-scrollbar` deprecated | Replace with alternative | Layout components |

### Low-Impact Changes

| Version | Change | Impact | Files Affected |
|---------|--------|--------|----------------|
| 13 | RxJS 7 support | Mostly backward compatible | Services using RxJS |
| 14 | `title` in routes | Optional feature | Routing modules |
| 15 | `NgOptimizedImage` | Optional performance | Image-heavy components |
| 15 | Directive composition | Optional feature | Directives |

---

## EAF-Specific Migration Notes

### 1. Service Proxies (NSwag)

The NSwag-generated `service-proxies.ts` file (16,000+ lines) will need to be regenerated after migration:

```bash
# After migrating, regenerate service proxies
# Run the NSwag tool against the updated backend API
# The generated code should be compatible with Angular 15
nswag run nswag.json
```

### 2. AppComponentBase

The `AppComponentBase` class uses `Injector` pattern which is compatible across versions:

```typescript
// This pattern works in all Angular versions (12-15)
constructor(injector: Injector) {
  this.localization = injector.get(LocalizationService);
  // ...
}
```

### 3. EAF Framework Dependencies

Check that the following EAF-specific packages are compatible with Angular 15:
- `@eaf/localization`
- `@eaf/auth`
- `@eaf/features`
- `@eaf/message`
- `@eaf/notify`
- `@eaf/settings`
- `@eaf/multi-tenancy`
- `@eaf/session`
- `@eaf/utils`

**Note:** These packages are part of the EAF framework and may need to be updated separately by the EAF team.

### 4. SignalR Integration

The `@microsoft/signalr` package should be updated to the latest version:

```bash
npm install @microsoft/signalr@7 --save --legacy-peer-deps
```

The `ChatSignalrService` connection setup code should remain compatible.

### 5. PrimeNG Migration

PrimeNG 15 has some breaking changes from PrimeNG 12:

- Component API changes (check PrimeNG changelog)
- CSS class name changes
- Import path changes for some components

```typescript
// BEFORE (PrimeNG 12):
import { TableModule } from 'primeng/table';

// AFTER (PrimeNG 15) - Same import, but API changes:
import { TableModule } from 'primeng/table';
// Check for deprecated properties in templates
```

### 6. ngx-bootstrap Migration

ngx-bootstrap 10 has breaking changes from 6:

- Module imports may have changed
- Some component APIs updated
- Check for deprecated directives

```typescript
// Verify modal service usage
import { BsModalService } from 'ngx-bootstrap/modal';
// API remains similar but check for parameter changes
```

---

## Testing Strategy

### Phase 1: Pre-Migration Testing
1. Run all existing unit tests: `npm run test`
2. Run production build: `npm run build`
3. Document any existing failures
4. Create baseline test coverage report

### Phase 2: During Migration Testing
After each version upgrade (12→13, 13→14, 14→15):
1. Run `npm run build` - fix compilation errors
2. Run `npm run test` - fix test failures
3. Run `npm run eslint` - fix linting issues
4. Manual smoke test of key features:
   - Login/logout flow
   - Dashboard rendering
   - Admin panel navigation
   - Chat functionality
   - Theme switching

### Phase 3: Post-Migration Testing
1. Full regression testing
2. Performance comparison (build time, bundle size)
3. E2E testing of all critical paths
4. Cross-browser testing (Chrome, Firefox, Safari, Edge)

### Test Coverage Targets
- Components: 80%+ statement coverage
- Services: 90%+ statement coverage
- Pipes: 100% coverage
- Directives: 80%+ coverage

---

## Rollback Procedures

### Quick Rollback

If the migration fails at any step, rollback using git:

```bash
# Rollback to pre-migration state
git checkout migration/angular-12-to-15-backup
git branch -D migration/angular-12-to-15

# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install --legacy-peer-deps
```

### Staged Rollback

If you need to stay at an intermediate version:

```bash
# After completing Step 1 (Angular 13), create a checkpoint
git tag migration-checkpoint-angular-13

# After completing Step 2 (Angular 14), create a checkpoint
git tag migration-checkpoint-angular-14

# Rollback to a specific checkpoint
git checkout migration-checkpoint-angular-13
rm -rf node_modules package-lock.json
npm install --legacy-peer-deps
```

### Emergency Procedures

1. **Build Failure:** Check TypeScript version compatibility and dependency versions
2. **Test Failures:** Use `ng update` schematic fixes, check for deprecated APIs
3. **Runtime Errors:** Check browser console for missing polyfills or deprecated APIs
4. **Third-Party Library Issues:** Pin problematic libraries to compatible versions

---

## Post-Migration Verification

### Verification Checklist

- [ ] `npm run build` succeeds without errors
- [ ] `npm run test` passes all tests
- [ ] `npm run eslint` shows no new errors
- [ ] Login page renders correctly
- [ ] Login/logout flow works
- [ ] Dashboard loads and displays data
- [ ] Admin panel:
  - [ ] Users CRUD operations work
  - [ ] Roles CRUD operations work
  - [ ] Tenants CRUD operations work
  - [ ] Settings page loads and saves
  - [ ] Audit logs display correctly
  - [ ] Language management works
- [ ] Chat functionality:
  - [ ] SignalR connection establishes
  - [ ] Messages send and receive
  - [ ] Friend list loads
- [ ] Theme switching works for all 4 themes
- [ ] Notifications display correctly
- [ ] Profile picture upload works
- [ ] Multi-tenancy features work
- [ ] External login providers work (Google, Microsoft, Auth0, OpenID Connect)
- [ ] Service worker registers correctly
- [ ] No console errors in production build

### Performance Comparison

| Metric | Angular 12 | Angular 15 | Notes |
|--------|-----------|-----------|-------|
| Build Time | ___ sec | ___ sec | Should improve with Ivy optimizations |
| Bundle Size (main.js) | ___ KB | ___ KB | Should be similar or smaller |
| Bundle Size (vendor.js) | ___ KB | ___ KB | May change with dep updates |
| Initial Load Time | ___ ms | ___ ms | Test in production mode |
| Lighthouse Score | ___ | ___ | Performance audit |

---

## Timeline Estimate

| Phase | Estimated Duration | Description |
|-------|-------------------|-------------|
| Pre-Migration | 1 day | Backup, review, testing |
| Angular 12 → 13 | 2-3 days | View Engine removal, IE11 cleanup |
| Angular 13 → 14 | 3-5 days | Typed forms migration (most work) |
| Angular 14 → 15 | 1-2 days | Minor changes, dependency updates |
| Testing & QA | 3-5 days | Full regression testing |
| **Total** | **10-16 days** | Including buffer for issues |

---

## References

- [Angular Update Guide](https://update.angular.io/)
- [Angular 13 Release Notes](https://blog.angular.io/angular-v13-is-now-available-cce66f7bc296)
- [Angular 14 Release Notes](https://blog.angular.io/angular-v14-is-now-available-391a6db736af)
- [Angular 15 Release Notes](https://blog.angular.io/angular-v15-is-now-available-df7be7f2f4c8)
- [PrimeNG Migration Guide](https://primeng.org/installation)
- [ngx-bootstrap Migration Guide](https://valor-software.com/ngx-bootstrap/#/documentation)
- [RxJS 7 Migration Guide](https://rxjs.dev/guide/v6/migration)
- [TypeScript Release Notes](https://www.typescriptlang.org/docs/handbook/release-notes/overview.html)
