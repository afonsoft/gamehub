# Migration Guide: Angular 15 to Angular 17

## EAF Angular UI Template

This document provides a step-by-step guide for migrating the EAF Angular UI Template from Angular 15 to Angular 17.

---

## Table of Contents

1. [Pre-Migration Checklist](#pre-migration-checklist)
2. [Step 1: Angular 15 to 16](#step-1-angular-15-to-16)
3. [Step 2: Angular 16 to 17](#step-2-angular-16-to-17)
4. [Dependency Updates](#dependency-updates)
5. [Breaking Changes Summary](#breaking-changes-summary)
6. [Testing Strategy](#testing-strategy)
7. [Rollback Procedures](#rollback-procedures)

---

## Pre-Migration Checklist

### Prerequisites
- [ ] Ensure current Angular 15 application is stable and all tests pass
- [ ] Create a backup of the current codebase
- [ ] Update Node.js to compatible version (Node.js 18.x or 20.x recommended)
- [ ] Clear npm cache: `npm cache clean --force`
- [ ] Remove node_modules and package-lock.json: `rm -rf node_modules package-lock.json`

### Environment Requirements
- **Node.js**: ^18.13.0 || ^20.9.0
- **npm**: ^9.0.0 || ^10.0.0
- **Angular CLI**: ^17.0.0

### Current State Assessment
- [ ] Document current package versions
- [ ] Note any custom configurations or workarounds
- [ ] Identify deprecated APIs in use
- [ ] Check for third-party library compatibility

---

## Step 1: Angular 15 to 16

### 1.1 Update Core Dependencies

Update Angular core packages to version 16:

```json
{
  "@angular/animations": "^16.0.0",
  "@angular/common": "^16.0.0", 
  "@angular/compiler": "^16.0.0",
  "@angular/core": "^16.0.0",
  "@angular/forms": "^16.0.0",
  "@angular/platform-browser": "^16.0.0",
  "@angular/platform-browser-dynamic": "^16.0.0",
  "@angular/router": "^16.0.0"
}
```

### 1.2 Update Angular CLI

```json
{
  "@angular/cli": "^16.0.0",
  "@angular-devkit/build-angular": "^16.0.0"
}
```

### 1.3 Update Angular CDK

```json
{
  "@angular/cdk": "^16.0.0"
}
```

### 1.4 Breaking Changes - Angular 15 to 16

#### Standalone Components
- **Change**: Standalone components are now stable
- **Action**: Consider migrating to standalone components where appropriate
- **Impact**: Optional but recommended for future compatibility

#### Signals
- **Change**: New reactive primitive introduced
- **Action**: No immediate action required, but consider for new features
- **Impact**: New feature, not breaking

#### Required Properties
- **Change**: `@Input` properties without `required` are deprecated
- **Action**: Add `required: true` to essential `@Input` properties
- **Impact**: Warnings in development mode

#### Router Transitions
- **Change**: Router transitions simplified
- **Action**: Update custom router transition implementations
- **Impact**: May affect animation sequences

### 1.5 Third-Party Library Updates

Update commonly used libraries:

```json
{
  "ngx-bootstrap": "^9.0.0",
  "primeng": "^16.0.0",
  "rxjs": "^7.8.0",
  "zone.js": "^0.13.0"
}
```

### 1.6 Code Updates

#### Update Imports
```typescript
// Before
import { ModuleWithProviders } from '@angular/core';

// After (if needed)
import { Provider } from '@angular/core';
```

#### Update TestBed Configuration
```typescript
// Before
TestBed.configureTestingModule({
  imports: [MyModule]
});

// After (if using standalone)
TestBed.configureTestingModule({
  imports: [MyComponent]
});
```

---

## Step 2: Angular 16 to 17

### 2.1 Update Core Dependencies

Update Angular core packages to version 17:

```json
{
  "@angular/animations": "^17.0.0",
  "@angular/common": "^17.0.0", 
  "@angular/compiler": "^17.0.0",
  "@angular/core": "^17.0.0",
  "@angular/forms": "^17.0.0",
  "@angular/platform-browser": "^17.0.0",
  "@angular/platform-browser-dynamic": "^17.0.0",
  "@angular/router": "^17.0.0"
}
```

### 2.2 Update Angular CLI

```json
{
  "@angular/cli": "^17.0.0",
  "@angular-devkit/build-angular": "^17.0.0"
}
```

### 2.3 Breaking Changes - Angular 16 to 17

#### Signals as Default
- **Change**: Signals become the default reactive primitive
- **Action**: Consider migrating from RxJS to Signals where appropriate
- **Impact**: Performance improvements, simplified reactive patterns

#### View Encapsulation
- **Change**: Default view encapsulation changes
- **Action**: Review component styles for unexpected behavior
- **Impact**: May affect component styling

#### Forms API Updates
- **Change**: Form validation API improvements
- **Action**: Update custom form validators
- **Impact**: Enhanced form validation capabilities

#### Router Updates
- **Change**: Router guards and resolvers updated
- **Action**: Update custom guard implementations
- **Impact**: Improved navigation handling

### 2.4 Third-Party Library Updates

```json
{
  "ngx-bootstrap": "^10.0.0",
  "primeng": "^17.0.0",
  "rxjs": "^7.8.0",
  "zone.js": "^0.14.0"
}
```

### 2.5 Build System Updates

#### Vite Support
- **Change**: Enhanced Vite support
- **Action**: Consider migrating from Webpack to Vite
- **Impact**: Improved build performance

#### ESBuild Updates
- **Change**: Updated ESBuild integration
- **Action**: No action required, but performance improvements expected
- **Impact**: Faster builds and better tree-shaking

---

## Dependency Updates

### Core Dependencies

#### Angular Packages
```json
{
  "@angular/animations": "^17.0.0",
  "@angular/common": "^17.0.0",
  "@angular/compiler": "^17.0.0", 
  "@angular/core": "^17.0.0",
  "@angular/forms": "^17.0.0",
  "@angular/platform-browser": "^17.0.0",
  "@angular/platform-browser-dynamic": "^17.0.0",
  "@angular/router": "^17.0.0"
}
```

#### Development Dependencies
```json
{
  "@angular/cli": "^17.0.0",
  "@angular-devkit/build-angular": "^17.0.0",
  "@angular/compiler-cli": "^17.0.0",
  "@types/node": "^20.0.0",
  "typescript": "^5.2.0"
}
```

### Third-Party Libraries

#### UI Libraries
```json
{
  "ngx-bootstrap": "^10.0.0",
  "primeng": "^17.0.0",
  "@swimlane/ngx-charts": "^20.0.0"
}
```

#### Utility Libraries
```json
{
  "rxjs": "^7.8.0",
  "zone.js": "^0.14.0",
  "moment": "^2.29.0",
  "lodash": "^4.17.0"
}
```

#### Testing Libraries
```json
{
  "@types/jasmine": "^4.3.0",
  "jasmine-core": "^4.6.0",
  "karma": "^6.4.0",
  "karma-chrome-launcher": "^3.2.0",
  "karma-coverage": "^2.2.0"
}
```

---

## Breaking Changes Summary

### Major Breaking Changes

#### 1. Standalone Components (Angular 16)
- **Impact**: Module-based components still supported but standalone recommended
- **Migration**: Gradual migration possible
- **Priority**: Medium

#### 2. Signals Introduction (Angular 16)
- **Impact**: New reactive pattern, RxJS still supported
- **Migration**: Optional for existing code
- **Priority**: Low

#### 3. View Encapsulation Changes (Angular 17)
- **Impact**: May affect component styling
- **Migration**: Review and test component styles
- **Priority**: High

#### 4. Forms API Updates (Angular 17)
- **Impact**: Enhanced validation, potential breaking changes
- **Migration**: Update custom validators
- **Priority**: Medium

#### 5. Router Updates (Angular 17)
- **Impact**: Guard and resolver interface changes
- **Migration**: Update navigation guards
- **Priority**: Medium

### Minor Breaking Changes

#### TypeScript Version
- **Change**: TypeScript 5.2+ required
- **Impact**: Stricter type checking
- **Action**: Update type definitions

#### Zone.js Updates
- **Change**: Zone.js 0.14+ required
- **Impact**: Performance improvements
- **Action**: No code changes required

#### RxJS Updates
- **Change**: RxJS 7.8+ recommended
- **Impact**: Deprecated operators removed
- **Action**: Update RxJS usage

---

## Testing Strategy

### Pre-Migration Testing
1. **Baseline Test Suite**
   - Run full test suite on Angular 15
   - Document any failing tests
   - Fix critical issues before migration

2. **Performance Benchmarking**
   - Measure build times
   - Test application startup time
   - Document memory usage

### Migration Testing
1. **Step-by-Step Validation**
   - Test after Angular 15 to 16 upgrade
   - Test after Angular 16 to 17 upgrade
   - Run full test suite after each step

2. **Component Testing**
   - Test all UI components
   - Verify form functionality
   - Check routing and navigation

3. **Integration Testing**
   - Test API integrations
   - Verify authentication flows
   - Test third-party library integrations

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
   git branch -D migration/angular-15-to-17
   git checkout -b rollback/angular-15-restore
   git reset --hard [pre-migration-commit]
   ```

2. **Environment Restoration**
   ```bash
   rm -rf node_modules package-lock.json
   npm install
   npm run build
   ```

### Partial Rollback
1. **Angular 17 to 16**
   ```bash
   # Update package.json to Angular 16 versions
   npm install
   npm run build
   ```

2. **Angular 16 to 15**
   ```bash
   # Update package.json to Angular 15 versions  
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

### Phase 2: Angular 15 to 16 (2-3 days)
- [ ] Dependency updates
- [ ] Code migration
- [ ] Testing and validation
- [ ] Issue resolution

### Phase 3: Angular 16 to 17 (2-3 days)
- [ ] Dependency updates
- [ ] Code migration  
- [ ] Testing and validation
- [ ] Issue resolution

### Phase 4: Final Validation (1-2 days)
- [ ] Full regression testing
- [ ] Performance testing
- [ ] Documentation updates
- [ ] Production deployment preparation

---

## Common Issues and Solutions

### Build Issues

#### Issue: TypeScript Compilation Errors
**Symptoms**: Type errors after upgrade
**Solution**: 
```bash
# Update TypeScript configurations
npm install typescript@latest
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

#### Issue: Component Rendering Problems
**Symptoms**: Components not displaying correctly
**Solution**:
- Check view encapsulation changes
- Verify CSS selector specificity
- Update component styles

#### Issue: Router Navigation Issues
**Symptoms**: Navigation not working
**Solution**:
- Update router guard implementations
- Check resolver return types
- Verify route configurations

### Performance Issues

#### Issue: Slow Build Times
**Symptoms**: Builds taking longer than before
**Solution**:
- Enable incremental builds
- Check for circular dependencies
- Optimize bundle size

#### Issue: Runtime Performance Degradation
**Symptoms**: Application slower after migration
**Solution**:
- Enable production mode
- Check for memory leaks
- Optimize change detection

---

## Post-Migration Optimizations

### Performance Enhancements
1. **Enable Standalone Components**
   - Gradual migration from modules
   - Reduced bundle sizes
   - Improved tree-shaking

2. **Implement Signals**
   - Replace RxJS where appropriate
   - Improved reactivity performance
   - Simplified reactive patterns

3. **Optimize Build Configuration**
   - Enable differential loading
   - Optimize bundle splitting
   - Implement lazy loading strategies

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
- [Angular 16 Release Notes](https://github.com/angular/angular/releases/tag/16.0.0)
- [Angular 17 Release Notes](https://github.com/angular/angular/releases/tag/17.0.0)
- [Angular Update Guide](https://update.angular.io/)

### Community Resources
- [Angular Blog](https://blog.angular.io/)
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

---

## EAF Application Analysis - Current State vs Migration Requirements

### **Current Application State Analysis**

#### **Package Version Assessment**
Based on current `package.json` analysis:

| Dependency | Current Version | Target Version | Status | Risk Level |
|------------|------------------|----------------|--------|------------|
| @angular/core | 15.2.10 | 17.0.0 | **NEEDS UPDATE** | HIGH |
| @angular/cli | 15.2.11 | 17.0.0 | **NEEDS UPDATE** | HIGH |
| @angular/cdk | 15.2.9 | 16.0.0+ | **NEEDS UPDATE** | MEDIUM |
| primeng | 15.4.1 | 17.0.0 | **COMPATIBLE** | LOW |
| ngx-bootstrap | 10.3.0 | 10.0.0 | **AHEAD OF TARGET** | LOW |
| ngx-mask | 15.0.0 | Compatible | **ALREADY COMPATIBLE** | LOW |
| zone.js | 0.12.0 | 0.14.0 | **NEEDS UPDATE** | MEDIUM |
| rxjs | 7.8.2 | 7.8.0 | **COMPATIBLE** | LOW |
| @swimlane/ngx-charts | 20.0.0 | 20.0.0 | **COMPATIBLE** | LOW |
| typescript | 4.9 | 5.2.0 | **NEEDS UPDATE** | MEDIUM |
| @types/node | 13.13.4 | 20.0.0 | **NEEDS UPDATE** | MEDIUM |

#### **Breaking Changes Found in EAF Codebase**

##### **1. ModuleWithProviders Usage (Angular 16)**
**Files Affected:**
- `src/shared/common/common.module.ts:14`
- `src/app/shared/common/app-common.module.ts:56`

**Current Code:**
```typescript
static forRoot(): ModuleWithProviders<CommonModule> {
  return {
    ngModule: CommonModule,
    providers: [...]
  };
}
```

**Required Change:**
```typescript
static forRoot(): ModuleWithProviders<CommonModule> {
  return {
    ngModule: CommonModule,
    providers: [...]
  };
}
// OR (recommended for Angular 16+)
static forRoot(): Provider[] {
  return [...];
}
```

**Impact:** Medium - Module loading patterns
**Priority:** High - Must be updated

##### **2. @Input Properties Without Required (Angular 16)**
**File Affected:** `src/shared/utils/validation/password-complexity-validator.directive.ts`

**Current Code:**
```typescript
@Input('requireDigit') requireDigit: boolean;
@Input('requireUppercase') requireUppercase: boolean;
@Input('requireLowercase') requireLowercase: boolean;
@Input('requireNonAlphanumeric') requireNonAlphanumeric: boolean;
@Input('requiredLength') requiredLength: number;
```

**Required Change:**
```typescript
@Input('requireDigit') requireDigit: boolean;
@Input('requireUppercase') requireUppercase: boolean;
@Input('requireLowercase') requireLowercase: boolean;
@Input('requireNonAlphanumeric') requireNonAlphanumeric: boolean;
@Input('requiredLength') requiredLength: number;
// Add required where appropriate:
@Input({ required: true }) essentialInput: string;
```

**Impact:** Low - Development warnings only
**Priority:** Medium - Code quality improvement

##### **3. View Encapsulation Changes (Angular 17)**
**Potential Impact Areas:**
- Component styling in `src/app/main/airplanes/`
- Form components in `src/app/admin/users/`
- Table components across all admin modules
- Custom CSS in `src/assets/common/styles/`

**Risk Assessment:** High - Visual regressions possible
**Priority:** High - Must test thoroughly

---

## **Migration Complications and Risks**

### **High-Risk Complications**

#### **1. EAF Custom Module System**
**Issue:** EAF uses custom module loading with `@eaf/*` path mappings
**Risk:** Path resolution may break with Angular 17's stricter module resolution
**Mitigation:** Test all EAF module imports and path mappings

#### **2. jQuery Integration**
**Issue:** Heavy jQuery usage alongside Angular
**Files:** Multiple components use jQuery directly
**Risk:** Angular 17's change detection may conflict with jQuery DOM manipulation
**Mitigation:** Isolate jQuery usage and test Angular change detection

#### **3. Custom EAF Framework Integration**
**Issue:** Custom EAF.js framework integration
**Files:** `src/assets/lib/eaf-web-resources/`
**Risk:** Angular 17's bootstrap process may affect EAF initialization
**Mitigation:** Test EAF framework initialization sequence

#### **4. SignalR Integration**
**Issue:** @microsoft/signalr version compatibility
**Current:** ^7.0.14
**Risk:** Real-time features may break
**Mitigation:** Test all SignalR connections and real-time updates

### **Medium-Risk Complications**

#### **5. Form Validation System**
**Issue:** Custom validation directives with Angular 17 forms API
**Risk:** Validation logic may behave differently
**Mitigation:** Test all form validations, especially password complexity

#### **6. PrimeNG Component Styling**
**Issue:** PrimeNG 17 may have CSS changes
**Risk:** Component styling may break
**Mitigation:** Test all PrimeNG components visually

#### **7. ngx-bootstrap Datepicker**
**Issue:** Calendar positioning previously fixed
**Risk:** Angular 17 may reintroduce positioning issues
**Mitigation:** Test all datepicker components

### **Low-Risk Complications**

#### **8. Build Configuration**
**Issue:** Angular 17 build system changes
**Risk:** Build process may need adjustment
**Mitigation:** Test build in both development and production

#### **9. Testing Framework**
**Issue:** Karma/Jasmine compatibility
**Risk:** Tests may fail after upgrade
**Mitigation:** Update test configurations if needed

---

## **Comprehensive Testing Requirements**

### **Phase 1: Pre-Migration Testing (1 day)**

#### **1.1 Baseline Functionality Tests**
- [ ] **Application Startup**
  - [ ] Development server starts without errors
  - [ ] Production build completes successfully
  - [ ] No console warnings or errors

- [ ] **Authentication Flow**
  - [ ] Login functionality works
  - [ ] Token refresh works
  - [ ] Logout functionality works
  - [ ] Session management works

- [ ] **Navigation**
  - [ ] All menu items navigate correctly
  - [ ] Browser back/forward works
  - [ ] Route guards function properly
  - [ ] Deep linking works

#### **1.2 Core Feature Tests**
- [ ] **Data Tables (PrimeNG)**
  - [ ] Tables load data correctly
  - [ ] Sorting works
  - [ ] Pagination works
  - [ ] Filtering works
  - [ ] Actions (edit/delete) work

- [ ] **Forms and Validation**
  - [ ] All forms submit correctly
  - [ ] Validation messages display properly
  - [ ] Required field validation works
  - [ ] Custom validators work

- [ ] **Modals and Dialogs**
  - [ ] Modal open/close works
  - [ ] Data passes correctly
  - [ ] Form submission in modals works

### **Phase 2: Angular 15 to 16 Testing (2-3 days)**

#### **2.1 Dependency Compatibility Tests**
- [ ] **Angular Core Functionality**
  - [ ] Components render correctly
  - [ ] Services inject properly
  - [ ] Dependency injection works
  - [ ] Change detection works

- [ ] **Third-Party Library Integration**
  - [ ] PrimeNG components work
  - [ ] ngx-bootstrap components work
  - [ ] ngx-mask works
  - [ ] ngx-charts work

- [ ] **ModuleWithProviders Changes**
  - [ ] Custom modules load correctly
  - [ ] forRoot() methods work
  - [ ] Provider injection works

#### **2.2 Visual Regression Tests**
- [ ] **Component Styling**
  - [ ] All components display correctly
  - [ ] CSS classes apply properly
  - [ ] Responsive design works
  - [ ] Theme styling works

- [ ] **Layout and Positioning**
  - [ ] Calendar positioning works
  - [ ] Modal positioning works
  - [ ] Tooltip positioning works
  - [ ] Dropdown positioning works

#### **2.3 Performance Tests**
- [ ] **Application Performance**
  - [ ] Initial load time acceptable
  - [ ] Route transitions smooth
  - [ ] Memory usage stable
  - [ ] No memory leaks

### **Phase 3: Angular 16 to 17 Testing (2-3 days)**

#### **3.1 Breaking Changes Tests**
- [ ] **View Encapsulation**
  - [ ] Component styles don't leak
  - [ ] Global styles work correctly
  - [ ] Theme styles apply properly
  - [ ] Dynamic styling works

- [ ] **Forms API Changes**
  - [ ] Form validation works
  - [ ] Custom validators work
  - [ ] Form submission works
  - [ ] Reactive forms work

- [ ] **Router Changes**
  - [ ] Route guards work
  - [ ] Route resolvers work
  - [ ] Navigation events work
  - [ ] Lazy loading works

#### **3.2 Integration Tests**
- [ ] **EAF Framework Integration**
  - [ ] EAF.js initializes correctly
  - [ ] Custom EAF components work
  - [ ] EAF services work
  - [ ] EAF event system works

- [ ] **SignalR Integration**
  - [ ] Real-time updates work
  - [ ] Connection management works
  - [ ] Message handling works
  - [ ] Error handling works

- [ ] **jQuery Integration**
  - [ ] jQuery plugins work
  - [ ] DOM manipulation works
  - [ ] Event handling works
  - [ ] No conflicts with Angular

#### **3.3 Advanced Feature Tests**
- [ ] **File Upload**
  - [ ] File selection works
  - [ ] Upload progress works
  - [ ] File validation works
  - [ ] Error handling works

- [ ] **Data Visualization**
  - [ ] Charts render correctly
  - [ ] Chart interactions work
  - [ ] Data updates reflect
  - [ ] Responsive charts work

- [ ] **Calendar Components**
  - [ ] Date selection works
  - [ ] Range selection works
  - [ ] Event handling works
  - [ ] Localization works

### **Phase 4: Comprehensive Regression Testing (1-2 days)**

#### **4.1 Cross-Browser Testing**
- [ ] **Chrome (Latest)**
  - [ ] All features work
  - [ ] Performance acceptable
  - [ ] No console errors

- [ ] **Firefox (Latest)**
  - [ ] All features work
  - [ ] Performance acceptable
  - [ ] No console errors

- [ ] **Edge (Latest)**
  - [ ] All features work
  - [ ] Performance acceptable
  - [ ] No console errors

- [ ] **Safari (if applicable)**
  - [ ] All features work
  - [ ] Performance acceptable
  - [ ] No console errors

#### **4.2 Device Testing**
- [ ] **Desktop (1920x1080)**
  - [ ] Layout responsive
  - [ ] All features accessible
  - [ ] Performance acceptable

- [ ] **Tablet (768x1024)**
  - [ ] Layout responsive
  - [ ] Touch interactions work
  - [ ] Performance acceptable

- [ ] **Mobile (375x667)**
  - [ ] Layout responsive
  - [ ] Touch interactions work
  - [ ] Performance acceptable

#### **4.3 User Workflow Testing**
- [ ] **Admin User Workflow**
  - [ ] Login to dashboard
  - [ ] Navigate to admin section
  - [ ] Manage users
  - [ ] Manage roles
  - [ ] Manage tenants
  - [ ] View audit logs

- [ ] **Regular User Workflow**
  - [ ] Login to dashboard
  - [ ] View personal profile
  - [ ] Manage notifications
  - [ ] Access allowed features

- [ ] **Data Management Workflow**
  - [ ] Create new records
  - [ ] Edit existing records
  - [ ] Delete records
  - [ ] Search and filter
  - [ ] Export data

---

## **Risk Mitigation Strategies**

### **High-Risk Mitigations**

#### **1. EAF Framework Compatibility**
**Strategy:** Create compatibility layer
```typescript
// Create eaf-compatibility.service.ts
@Injectable()
export class EafCompatibilityService {
  initializeEafFramework(): Promise<void> {
    // Ensure EAF initializes after Angular bootstrap
  }
}
```

#### **2. jQuery Conflicts Prevention**
**Strategy:** Isolate jQuery usage
```typescript
// Create jquery-wrapper.service.ts
@Injectable()
export class JQueryWrapperService {
  executeJQueryCode(selector: string, operation: string): any {
    // Safely execute jQuery code
  }
}
```

#### **3. Gradual Component Migration**
**Strategy:** Migrate components incrementally
- Start with non-critical components
- Test each component individually
- Roll back individual components if issues arise

### **Medium-Risk Mitigations**

#### **1. Feature Flags**
**Strategy:** Use feature flags for new Angular 17 features
```typescript
// Create feature-flags.service.ts
@Injectable()
export class FeatureFlagsService {
  useAngular17Signals(): boolean {
    return this.featureFlags['angular17-signals'] || false;
  }
}
```

#### **2. Fallback Components**
**Strategy:** Create fallback implementations
```typescript
// Create fallback components for critical features
@Component({
  selector: 'app-table-fallback',
  template: '...'
})
export class TableFallbackComponent implements OnInit {
  // Fallback table implementation
}
```

---

## **Rollback and Recovery Procedures**

### **Immediate Rollback (Within 24 hours)**
1. **Git Rollback**
```bash
git checkout main
git branch -D migration/angular-15-to-17
git checkout -b rollback/angular-15-restore
git reset --hard [pre-migration-commit-hash]
```

2. **Environment Restoration**
```bash
rm -rf node_modules package-lock.json
npm install
npm run build
npm start
```

3. **Database Rollback**
```bash
# If database migrations were applied
npm run migrate:rollback
```

### **Partial Rollback Strategies**

#### **Angular 17 to 16 Rollback**
```json
// Update package.json to Angular 16 versions
{
  "@angular/core": "^16.2.0",
  "@angular/cli": "^16.2.0",
  // ... other Angular 16 versions
}
```

#### **Component-Level Rollback**
```typescript
// Use feature flags to disable problematic components
if (!featureFlags.useAngular17Component()) {
  // Use Angular 15 component version
}
```

### **Recovery Testing**
1. **Build Verification**
   - [ ] Application builds successfully
   - [ ] No compilation errors
   - [ ] All tests pass

2. **Functionality Verification**
   - [ ] Core features working
   - [ ] No UI regressions
   - [ ] Performance acceptable

3. **Data Integrity**
   - [ ] No data loss
   - [ ] Database consistency
   - [ ] API compatibility

---

## **Migration Timeline - Detailed Breakdown**

### **Phase 1: Preparation (1 day)**
- **Morning (4 hours):**
  - [ ] Create backup branch
  - [ ] Document current state
  - [ ] Run baseline tests
  - [ ] Create rollback plan

- **Afternoon (4 hours):**
  - [ ] Update development environment
  - [ ] Clear npm cache
  - [ ] Update Node.js if needed
  - [ ] Test current application

### **Phase 2: Angular 15 to 16 (2-3 days)**
- **Day 1: Core Dependencies**
  - [ ] Update Angular packages to v16
  - [ ] Update Angular CLI to v16
  - [ ] Fix ModuleWithProviders issues
  - [ ] Test basic functionality

- **Day 2: Third-Party Libraries**
  - [ ] Update Zone.js to v0.13
  - [ ] Test PrimeNG components
  - [ ] Test ngx-bootstrap components
  - [ ] Test visual components

- **Day 3: Integration Testing**
  - [ ] Test EAF framework integration
  - [ ] Test SignalR integration
  - [ ] Test jQuery integration
  - [ ] Performance testing

### **Phase 3: Angular 16 to 17 (2-3 days)**
- **Day 1: Core Dependencies**
  - [ ] Update Angular packages to v17
  - [ ] Update Angular CLI to v17
  - [ ] Update TypeScript to v5.2
  - [ ] Update @types/node to v20

- **Day 2: Breaking Changes**
  - [ ] Fix view encapsulation issues
  - [ ] Update form validators
  - [ ] Test router changes
  - [ ] Test component styling

- **Day 3: Advanced Features**
  - [ ] Test SignalR functionality
  - [ ] Test real-time features
  - [ ] Test file uploads
  - [ ] Test data visualization

### **Phase 4: Final Validation (1-2 days)**
- **Day 1: Comprehensive Testing**
  - [ ] Cross-browser testing
  - [ ] Device testing
  - [ ] Performance testing
  - [ ] Security testing

- **Day 2: User Acceptance**
  - [ ] User workflow testing
  - [ ] Admin panel testing
  - [ ] Documentation updates
  - [ ] Production preparation

---

## **Success Criteria**

### **Technical Success Criteria**
- [ ] Application builds without errors
- [ ] All tests pass (90%+ coverage)
- [ ] No console warnings or errors
- [ ] Performance within acceptable limits
- [ ] All critical features working

### **User Experience Criteria**
- [ ] No visual regressions
- [ ] Responsive design maintained
- [ ] Accessibility features working
- [ ] Loading times acceptable
- [ ] Error handling working

### **Business Criteria**
- [ ] No data loss
- [ ] API compatibility maintained
- [ ] Security features intact
- [ ] Compliance requirements met
- [ ] Stakeholder approval

---

## **Automation Scripts for LLM Agent Execution**

This section provides executable scripts and configurations that enable automated execution of the migration process by an LLM agent.

### **1. Pre-Migration Automation Scripts**

#### **1.1 Environment Setup Script**
```bash
#!/bin/bash
# scripts/setup-environment.sh
# Configura ambiente para migração automatizada

set -e

echo "=== Setting up Migration Environment ==="

setup_node() {
  echo "Setting up Node.js 20..."
  if command -v nvm &> /dev/null; then
    nvm install 20
    nvm use 20
  else
    echo "WARNING: nvm not found. Please ensure Node.js 20 is installed."
  fi
}

setup_npm() {
  echo "Cleaning npm cache..."
  npm cache clean --force
  
  echo "Installing Angular CLI 17 globally..."
  npm install -g @angular/cli@17
}

setup_git() {
  echo "Setting up git branch for migration..."
  git checkout -b migration/angular-15-to-17 || git checkout migration/angular-15-to-17
  git add .
  git commit -m "Pre-migration snapshot" || echo "No changes to commit"
}

# Execute setup
setup_node
setup_npm
setup_git

echo "=== Environment Setup Complete ==="
```

#### **1.2 Pre-Migration Validation Script**
```bash
#!/bin/bash
# scripts/pre-migration-check.sh
# Validação automatizada de pré-migração

set -e

echo "=== Running Pre-Migration Checks ==="

check_node_version() {
  NODE_VERSION=$(node -v)
  echo "Node.js version: $NODE_VERSION"
  if [[ ! "$NODE_VERSION" =~ ^v1[89]\. ]] && [[ ! "$NODE_VERSION" =~ ^v20\. ]]; then
    echo "ERROR: Node.js version must be 18.x or 20.x"
    exit 1
  fi
  echo "✓ Node.js version compatible"
}

check_angular_version() {
  ANGULAR_VERSION=$(ng version | grep "@angular/core" | awk '{print $3}' || echo "unknown")
  echo "Angular version: $ANGULAR_VERSION"
  if [[ ! "$ANGULAR_VERSION" =~ ^15\. ]]; then
    echo "ERROR: Angular version must be 15.x"
    exit 1
  fi
  echo "✓ Angular version compatible"
}

check_tests_pass() {
  echo "Running baseline tests..."
  if ! ng test --watch=false --browsers=ChromeHeadless; then
    echo "ERROR: Baseline tests must pass before migration"
    exit 1
  fi
  echo "✓ All tests pass"
}

check_build_success() {
  echo "Checking build..."
  if ! ng build --configuration production; then
    echo "ERROR: Application must build successfully before migration"
    exit 1
  fi
  echo "✓ Build successful"
}

# Execute checks
check_node_version
check_angular_version
check_tests_pass
check_build_success

echo "=== All Pre-Migration Checks Passed ==="
```

### **2. File Mapping with Regex Patterns**

#### **2.1 Code Pattern Detection Configuration**
```yaml
# patterns/migration-patterns.yml
file_patterns:
  ModuleWithProviders:
    pattern: 'ModuleWithProviders<<(\w+)>>'
    files:
      - src/shared/common/common.module.ts
      - src/app/shared/common/app-common.module.ts
    action: replace_with_provider_array
    replacement: |
      # Before
      static forRoot(): ModuleWithProviders<CommonModule> {
        return { ngModule: CommonModule, providers: [...] };
      }
      # After
      static forRoot(): Provider[] {
        return [...];
      }

  @Input_without_required:
    pattern: '@Input\([\'"](\w+)[\'"]\)\s+(\w+):\s*(\w+);'
    files:
      - src/shared/utils/validation/password-complexity-validator.directive.ts
      - src/shared/utils/validation/*.ts
    action: add_required_flag
    replacement: |
      # Add required where appropriate
      @Input({ required: true }) essentialInput: string;

  deprecated_imports:
    pattern: 'import\s+\{\s*ModuleWithProviders\s*\}\s+from'
    files:
      - src/**/*.ts
    action: replace_import
    replacement: |
      import { Provider } from '@angular/core';

  router_guards:
    pattern: 'CanActivate\s*:\s*(\w+)'
    files:
      - src/app/**/guards/*.ts
    action: update_guard_signature
    replacement: |
      # Angular 17 guard signature
      canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree
```

### **3. Specific Executable Commands**

#### **3.1 Angular 15 to 16 Migration Commands**
```bash
#!/bin/bash
# scripts/migrate-to-angular16.sh

set -e

echo "=== Migrating to Angular 16 ==="

# Step 1.1: Update Angular Core
echo "Updating Angular core packages to v16..."
npm install @angular/animations@^16.2.0 \
  @angular/common@^16.2.0 \
  @angular/compiler@^16.2.0 \
  @angular/core@^16.2.0 \
  @angular/forms@^16.2.0 \
  @angular/platform-browser@^16.2.0 \
  @angular/platform-browser-dynamic@^16.2.0 \
  @angular/router@^16.2.0

# Step 1.2: Update Angular CLI
echo "Updating Angular CLI to v16..."
npm install @angular/cli@^16.2.0 @angular-devkit/build-angular@^16.2.0

# Step 1.3: Update Angular CDK
echo "Updating Angular CDK to v16..."
npm install @angular/cdk@^16.2.0

# Step 1.5: Update Third-Party Libraries
echo "Updating third-party libraries..."
npm install ngx-bootstrap@^9.0.0 \
  primeng@^16.0.0 \
  rxjs@^7.8.0 \
  zone.js@^0.13.0

echo "=== Angular 16 Migration Complete ==="
```

#### **3.2 Angular 16 to 17 Migration Commands**
```bash
#!/bin/bash
# scripts/migrate-to-angular17.sh

set -e

echo "=== Migrating to Angular 17 ==="

# Step 2.1: Update Angular Core
echo "Updating Angular core packages to v17..."
npm install @angular/animations@^17.0.0 \
  @angular/common@^17.0.0 \
  @angular/compiler@^17.0.0 \
  @angular/core@^17.0.0 \
  @angular/forms@^17.0.0 \
  @angular/platform-browser@^17.0.0 \
  @angular/platform-browser-dynamic@^17.0.0 \
  @angular/router@^17.0.0

# Step 2.2: Update Angular CLI
echo "Updating Angular CLI to v17..."
npm install @angular/cli@^17.0.0 @angular-devkit/build-angular@^17.0.0

# Step 2.3: Update TypeScript
echo "Updating TypeScript to v5.2..."
npm install typescript@^5.2.0 @types/node@^20.0.0

# Step 2.4: Update Third-Party Libraries
echo "Updating third-party libraries..."
npm install ngx-bootstrap@^10.0.0 \
  primeng@^17.0.0 \
  rxjs@^7.8.0 \
  zone.js@^0.14.0

echo "=== Angular 17 Migration Complete ==="
```

### **4. Automated Validation Scripts**

#### **4.1 Angular 16 Validation Script**
```bash
#!/bin/bash
# scripts/validate-angular16.sh

set -e

echo "=== Validating Angular 16 Migration ==="

validate_angular_version() {
  VERSION=$(ng version | grep "@angular/core" | awk '{print $3}')
  if [[ ! "$VERSION" =~ ^16\. ]]; then
    echo "ERROR: Angular version is not 16.x"
    exit 1
  fi
  echo "✓ Angular 16 installed correctly"
}

validate_build() {
  echo "Testing build..."
  if ! ng build --configuration production; then
    echo "BUILD FAILED: Angular 16 build failed"
    exit 1
  fi
  echo "✓ Build successful"
}

validate_tests() {
  echo "Running tests..."
  if ! ng test --watch=false --browsers=ChromeHeadless; then
    echo "TESTS FAILED: Some tests failed"
    exit 1
  fi
  echo "✓ All tests passed"
}

validate_serve() {
  echo "Testing development server..."
  timeout 30 ng serve --port 4200 || {
    echo "SERVE FAILED: Development server failed to start"
    exit 1
  }
  echo "✓ Development server starts correctly"
}

# Execute validations
validate_angular_version
validate_build
validate_tests
validate_serve

echo "=== Angular 16 Validation Complete ==="
```

#### **4.2 Angular 17 Validation Script**
```bash
#!/bin/bash
# scripts/validate-angular17.sh

set -e

echo "=== Validating Angular 17 Migration ==="

validate_angular_version() {
  VERSION=$(ng version | grep "@angular/core" | awk '{print $3}')
  if [[ ! "$VERSION" =~ ^17\. ]]; then
    echo "ERROR: Angular version is not 17.x"
    exit 1
  fi
  echo "✓ Angular 17 installed correctly"
}

validate_typescript_version() {
  TS_VERSION=$(tsc --version)
  if [[ ! "$TS_VERSION" =~ ^5\.2 ]]; then
    echo "ERROR: TypeScript version is not 5.2.x"
    exit 1
  fi
  echo "✓ TypeScript 5.2 installed correctly"
}

validate_build() {
  echo "Testing build..."
  if ! ng build --configuration production; then
    echo "BUILD FAILED: Angular 17 build failed"
    exit 1
  fi
  echo "✓ Build successful"
}

validate_tests() {
  echo "Running tests..."
  if ! ng test --watch=false --browsers=ChromeHeadless; then
    echo "TESTS FAILED: Some tests failed"
    exit 1
  fi
  echo "✓ All tests passed"
}

validate_lint() {
  echo "Running linter..."
  if ! ng lint; then
    echo "LINT FAILED: Linter found issues"
    exit 1
  fi
  echo "✓ No linting errors"
}

# Execute validations
validate_angular_version
validate_typescript_version
validate_build
validate_tests
validate_lint

echo "=== Angular 17 Validation Complete ==="
```

### **5. Automated Issue Detection**

#### **5.1 Pre-Migration Issue Detection Script**
```bash
#!/bin/bash
# scripts/detect-issues.sh

echo "=== Detecting Potential Migration Issues ==="

detect_deprecated_apis() {
  echo "Checking for deprecated APIs..."
  echo "ModuleWithProviders usage:"
  grep -r "ModuleWithProviders" src/ --include="*.ts" || echo "  None found"
  
  echo "@Input without required:"
  grep -r "@Input" src/ --include="*.ts" | grep -v "required" || echo "  None found"
}

detect_jquery_conflicts() {
  echo "Checking for jQuery usage..."
  grep -r "\$(" src/ --include="*.ts" --include="*.js" || echo "  None found"
}

detect_third_party_issues() {
  echo "Checking for outdated dependencies..."
  npm outdated || echo "  All dependencies up to date"
  
  echo "Checking for security vulnerabilities..."
  npm audit --audit-level=high || echo "  No high-severity vulnerabilities found"
}

detect_type_errors() {
  echo "Checking for TypeScript errors..."
  npx tsc --noEmit || echo "  TypeScript compilation errors found"
}

# Execute detection
detect_deprecated_apis
detect_jquery_conflicts
detect_third_party_issues
detect_type_errors

echo "=== Issue Detection Complete ==="
```

### **6. Custom Schematics**

#### **6.1 ModuleWithProviders Migration Schematic**
```typescript
// schematics/migrate-module-with-providers/index.ts
import { Rule, SchematicContext, Tree, SchematicsException } from '@angular-devkit/schematics';
import { normalize, strings } from '@angular-devkit/core';

export default function(): Rule {
  return (tree: Tree, context: SchematicContext) => {
    context.logger.info('Migrating ModuleWithProviders to Provider[]...');
    
    const files = tree.getDir('src').subfiles;
    let changesCount = 0;
    
    tree.getDir('src').visit((path) => {
      if (!path.endsWith('.ts')) return;
      
      const content = tree.read(path)!.toString();
      if (content.includes('ModuleWithProviders')) {
        // Apply transformation
        const newContent = content.replace(
          /static\s+(\w+)\(\):\s*ModuleWithProviders<(\w+)>/,
          'static $1(): Provider[]'
        );
        
        tree.overwrite(path, newContent);
        changesCount++;
        context.logger.info(`Updated: ${path}`);
      }
    });
    
    context.logger.info(`Migration complete. Updated ${changesCount} files.`);
  };
}
```

#### **6.2 @Input Required Flag Schematic**
```typescript
// schematics/add-input-required/index.ts
import { Rule, SchematicContext, Tree } from '@angular-devkit/schematics';

export default function(options: { pattern: string }): Rule {
  return (tree: Tree, context: SchematicContext) => {
    context.logger.info('Adding required flags to @Input properties...');
    
    tree.getDir('src').visit((path) => {
      if (!path.endsWith('.ts')) return;
      
      const content = tree.read(path)!.toString();
      const newContent = content.replace(
        /@Input\('(\w+)'\)\s+(\w+):\s*(string|number|boolean);/g,
        '@Input({ required: true }) $2: $3;'
      );
      
      if (content !== newContent) {
        tree.overwrite(path, newContent);
        context.logger.info(`Updated: ${path}`);
      }
    });
  };
}
```

### **7. Verification Checkpoints**

#### **7.1 Checkpoint Configuration**
```yaml
# checkpoints/migration-checkpoints.yml
checkpoints:
  - name: "pre_migration_complete"
    description: "Pre-migration checks completed"
    validation:
      - command: "node -v"
        expected_pattern: "v1[89]\\.|v20\\."
      - command: "ng version"
        expected_pattern: "@angular/core.*15\\."
      - command: "ng build --configuration production"
        expected_exit_code: 0
      - command: "ng test --watch=false --browsers=ChromeHeadless"
        expected_exit_code: 0
  
  - name: "angular_16_upgrade_complete"
    description: "Angular 16 upgrade completed"
    validation:
      - command: "ng version"
        expected_pattern: "@angular/core.*16\\."
      - command: "npm list @angular/core"
        expected_pattern: "16\\."
      - command: "ng build --configuration production"
        expected_exit_code: 0
  
  - name: "angular_17_upgrade_complete"
    description: "Angular 17 upgrade completed"
    validation:
      - command: "ng version"
        expected_pattern: "@angular/core.*17\\."
      - command: "tsc --version"
        expected_pattern: "Version 5\\.2"
      - command: "ng build --configuration production"
        expected_exit_code: 0
      - command: "ng test --watch=false --browsers=ChromeHeadless"
        expected_exit_code: 0
      - command: "ng lint"
        expected_exit_code: 0
  
  - name: "migration_complete"
    description: "Full migration completed"
    validation:
      - command: "ng version"
        expected_pattern: "@angular/core.*17\\."
      - command: "ng build --configuration production"
        expected_exit_code: 0
      - command: "ng test --watch=false --browsers=ChromeHeadless"
        expected_exit_code: 0
      - command: "ng e2e"
        expected_exit_code: 0
```

### **8. Report Generation**

#### **8.1 Migration Progress Report Script**
```bash
#!/bin/bash
# scripts/generate-report.sh

REPORT_FILE="migration-report.md"
PROGRESS_FILE=".migration-progress"
ISSUES_FILE=".migration-issues"

generate_report() {
  cat > $REPORT_FILE << EOF
# Migration Progress Report

## Timestamp
$(date)

## Migration Phase
$(cat .migration-phase 2>/dev/null || echo "Not started")

## Progress Log
$(cat $PROGRESS_FILE 2>/dev/null || echo "No progress recorded")

## Issues Found
$(cat $ISSUES_FILE 2>/dev/null || echo "No issues recorded")

## Current State
$(ng version)

## Build Status
$(ng build --configuration production 2>&1 | tail -5)

## Test Status
$(ng test --watch=false --browsers=ChromeHeadless 2>&1 | tail -5)

## Next Steps
$(cat .next-steps 2>/dev/null || echo "No next steps defined")
EOF

  echo "Report generated: $REPORT_FILE"
}

generate_report
```

### **9. Automated Error Handling**

#### **9.1 Error Handling Configuration**
```yaml
# error-handling/error-strategies.yml
error_handling:
  typescript_compilation_error:
    detection:
      - pattern: "error TS"
        severity: "high"
    auto_fix:
      - command: "npm install typescript@latest"
        description: "Update TypeScript to latest version"
      - command: "npx tsc --noEmit"
        description: "Check TypeScript errors"
    fallback:
      - "Review tsconfig.json for deprecated options"
      - "Check for incompatible type definitions"
      - "Update @types packages"
  
  module_resolution_error:
    detection:
      - pattern: "Cannot find module"
        severity: "high"
    auto_fix:
      - command: "npm install --save-dev @types/node@latest"
        description: "Install latest @types/node"
      - command: "Update tsconfig.json moduleResolution to 'node'"
        description: "Update module resolution"
    fallback:
      - "Check tsconfig.json paths configuration"
      - "Verify @eaf/* path mappings"
      - "Clear npm cache and reinstall"
  
  build_error:
    detection:
      - pattern: "Build failed"
        severity: "critical"
    auto_fix:
      - command: "rm -rf node_modules package-lock.json"
        description: "Clean dependencies"
      - command: "npm install"
        description: "Reinstall dependencies"
      - command: "ng cache clean"
        description: "Clean Angular cache"
    fallback:
      - "Check Angular CLI version compatibility"
      - "Review angular.json configuration"
      - "Check for circular dependencies"
  
  test_failure:
    detection:
      - pattern: "FAILED"
        severity: "medium"
    auto_fix:
      - command: "npm test -- --watch=false --browsers=ChromeHeadless"
        description: "Rerun tests"
    fallback:
      - "Review test failures for false positives"
      - "Check for API changes affecting tests"
      - "Update test expectations if needed"
```

### **10. CI/CD Integration**

#### **10.1 GitHub Actions Workflow**
```yaml
# .github/workflows/angular-migration.yml
name: Angular Migration

on:
  workflow_dispatch:
    inputs:
      target_version:
        description: 'Target Angular version'
        required: true
        default: '17'
        type: choice
        options:
          - '16'
          - '17'
      skip_tests:
        description: 'Skip tests (not recommended)'
        required: false
        default: false
        type: boolean

env:
  NODE_VERSION: '20'

jobs:
  pre-migration-checks:
    name: Pre-Migration Checks
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
      
      - name: Cache node modules
        uses: actions/cache@v4
        with:
          path: node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run pre-migration checks
        run: ./scripts/pre-migration-check.sh
      
      - name: Detect issues
        run: ./scripts/detect-issues.sh
      
      - name: Upload issue report
        uses: actions/upload-artifact@v4
        with:
          name: issue-report
          path: migration-report.md

  migrate-angular:
    name: Migrate to Angular ${{ inputs.target_version }}
    runs-on: ubuntu-latest
    needs: pre-migration-checks
    if: inputs.target_version == '16'
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
      
      - name: Install dependencies
        run: npm ci
      
      - name: Migrate to Angular 16
        run: ./scripts/migrate-to-angular16.sh
      
      - name: Validate Angular 16
        run: ./scripts/validate-angular16.sh
      
      - name: Run tests
        if: inputs.skip_tests == false
        run: ng test --watch=false --browsers=ChromeHeadless
      
      - name: Build application
        run: ng build --configuration production
      
      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: angular16-build
          path: dist/

  migrate-angular17:
    name: Migrate to Angular 17
    runs-on: ubuntu-latest
    needs: pre-migration-checks
    if: inputs.target_version == '17'
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
      
      - name: Install dependencies
        run: npm ci
      
      - name: Migrate to Angular 16 (intermediate)
        run: ./scripts/migrate-to-angular16.sh
      
      - name: Validate Angular 16
        run: ./scripts/validate-angular16.sh
      
      - name: Migrate to Angular 17
        run: ./scripts/migrate-to-angular17.sh
      
      - name: Validate Angular 17
        run: ./scripts/validate-angular17.sh
      
      - name: Run tests
        if: inputs.skip_tests == false
        run: ng test --watch=false --browsers=ChromeHeadless
      
      - name: Run linter
        run: ng lint
      
      - name: Build application
        run: ng build --configuration production
      
      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: angular17-build
          path: dist/
      
      - name: Generate migration report
        run: ./scripts/generate-report.sh
      
      - name: Upload migration report
        uses: actions/upload-artifact@v4
        with:
          name: migration-report
          path: migration-report.md

  rollback-on-failure:
    name: Rollback on Failure
    runs-on: ubuntu-latest
    needs: [migrate-angular, migrate-angular17]
    if: failure()
    steps:
      - uses: actions/checkout@v4
      
      - name: Execute rollback
        run: ./scripts/auto-rollback.sh
      
      - name: Notify team
        uses: actions/github-script@v7
        with:
          script: |
            github.rest.issues.create({
              owner: context.repo.owner,
              repo: context.repo.repo,
              title: 'Migration Failed - Rollback Executed',
              body: 'Angular migration failed and rollback was executed automatically.',
              labels: ['migration', 'failed']
            })
```

### **11. Dependency Validation**

#### **11.1 Dependency Compatibility Check Script**
```bash
#!/bin/bash
# scripts/check-dependencies.sh

echo "=== Checking Dependency Compatibility ==="

check_angular_dependency_compatibility() {
  DEPENDENCY=$1
  TARGET_VERSION=$2
  
  CURRENT_VERSION=$(npm list $DEPENDENCY --depth=0 2>/dev/null | grep $DEPENDENCY | awk '{print $2}' || echo "not installed")
  
  echo "Checking $DEPENDENCY:"
  echo "  Current: $CURRENT_VERSION"
  echo "  Target: $TARGET_VERSION"
  
  # Check if compatible version exists
  AVAILABLE_VERSIONS=$(npm view $DEPENDENCY versions --json 2>/dev/null | grep -E "\"1[67]\." || echo "none")
  
  if [[ "$AVAILABLE_VERSIONS" != "none" ]]; then
    echo "  ✓ Compatible versions available"
  else
    echo "  ✗ No compatible versions found - may need alternative"
  fi
}

# Check key dependencies
check_angular_dependency_compatibility "@angular/core" "17.0.0"
check_angular_dependency_compatibility "@angular/cdk" "16.0.0"
check_angular_dependency_compatibility "primeng" "17.0.0"
check_angular_dependency_compatibility "ngx-bootstrap" "10.0.0"
check_angular_dependency_compatibility "@microsoft/signalr" "7.0.0"
check_angular_dependency_compatibility "zone.js" "0.14.0"

echo "=== Dependency Check Complete ==="
```

### **12. Automated Rollback**

#### **12.1 Auto-Rollback Script**
```bash
#!/bin/bash
# scripts/auto-rollback.sh

set -e

echo "=== Executing Automated Rollback ==="

# Store pre-migration commit hash
PRE_MIGRATION_COMMIT=$(git log --oneline -1 .migration-snapshot 2>/dev/null || echo "")

if [[ -z "$PRE_MIGRATION_COMMIT" ]]; then
  echo "ERROR: No pre-migration snapshot found"
  exit 1
fi

echo "Rolling back to commit: $PRE_MIGRATION_COMMIT"

# Git rollback
git checkout main
git branch -D migration/angular-15-to-17 2>/dev/null || true
git reset --hard $PRE_MIGRATION_COMMIT

# Environment restoration
echo "Restoring environment..."
rm -rf node_modules package-lock.json
npm install

# Verify rollback
echo "Verifying rollback..."
if ! ng build --configuration production; then
  echo "ERROR: Rollback build failed"
  exit 1
fi

if ! ng test --watch=false --browsers=ChromeHeadless; then
  echo "ERROR: Rollback tests failed"
  exit 1
fi

echo "=== Rollback Complete ==="
echo "Application restored to pre-migration state"
```

### **13. Progress Monitoring**

#### **13.1 Progress Tracking Script**
```bash
#!/bin/bash
# scripts/track-progress.sh

PROGRESS_FILE=".migration-progress"
PHASE_FILE=".migration-phase"

track_step() {
  STEP_NAME=$1
  STATUS=$2
  TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
  
  echo "$TIMESTAMP - $STEP_NAME: $STATUS" >> $PROGRESS_FILE
  echo "Progress tracked: $STEP_NAME - $STATUS"
}

set_phase() {
  PHASE=$1
  echo "$PHASE" > $PHASE_FILE
  echo "Phase set to: $PHASE"
}

get_progress() {
  if [[ -f $PROGRESS_FILE ]]; then
    cat $PROGRESS_FILE
  else
    echo "No progress recorded"
  fi
}

get_current_phase() {
  if [[ -f $PHASE_FILE ]]; then
    cat $PHASE_FILE
  else
    echo "No phase set"
  fi
}

# Usage examples
# track_step "Pre-migration checks" "completed"
# set_phase "Angular 16 migration"
# get_progress
```

### **14. Environment Configuration**

#### **14.1 Migration Environment Configuration**
```json
{
  "migrationConfig": {
    "sourceVersion": "15.2.10",
    "targetVersion": "17.0.0",
    "intermediateVersion": "16.2.0",
    "nodeVersion": "20.9.0",
    "npmVersion": "10.0.0",
    "typescriptVersion": "5.2.0"
  },
  "paths": {
    "source": "src/",
    "scripts": "scripts/",
    "patterns": "patterns/",
    "schematics": "schematics/",
    "checkpoints": "checkpoints/",
    "errorHandling": "error-handling/"
  },
  "options": {
    "skipTests": false,
    "skipLint": false,
    "skipBuild": false,
    "autoRollback": true,
    "generateReport": true
  },
  "customizations": {
    "eafPathMappings": ["@eaf/*"],
    "jqueryIntegration": true,
    "signalrIntegration": true,
    "customFramework": "EAF.js"
  }
}
```

### **15. Automated Testing**

#### **15.1 Critical Path Tests Script**
```bash
#!/bin/bash
# scripts/run-critical-tests.sh

echo "=== Running Critical Path Tests ==="

run_auth_tests() {
  echo "Testing authentication flow..."
  ng test --include="**/auth/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

run_navigation_tests() {
  echo "Testing navigation..."
  ng test --include="**/navigation/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

run_form_tests() {
  echo "Testing forms and validation..."
  ng test --include="**/forms/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

run_service_tests() {
  echo "Testing services..."
  ng test --include="**/services/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

# Execute critical tests
run_auth_tests
run_navigation_tests
run_form_tests
run_service_tests

echo "=== Critical Path Tests Complete ==="
```

#### **15.2 Integration Tests Script**
```bash
#!/bin/bash
# scripts/run-integration-tests.sh

echo "=== Running Integration Tests ==="

run_eaf_framework_tests() {
  echo "Testing EAF framework integration..."
  ng test --include="**/eaf/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

run_signalr_tests() {
  echo "Testing SignalR integration..."
  ng test --include="**/signalr/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

run_jquery_tests() {
  echo "Testing jQuery integration..."
  ng test --include="**/jquery/**/*.spec.ts" --watch=false --browsers=ChromeHeadless
}

# Execute integration tests
run_eaf_framework_tests
run_signalr_tests
run_jquery_tests

echo "=== Integration Tests Complete ==="
```

---

## **LLM Agent Execution Guide**

This section provides instructions for LLM agents to execute the migration process.

### **Execution Order for LLM Agent**

1. **Preparation Phase**
   - Run `scripts/setup-environment.sh`
   - Run `scripts/pre-migration-check.sh`
   - Run `scripts/detect-issues.sh`
   - Review generated report

2. **Angular 15 to 16 Migration**
   - Set phase: `scripts/track-progress.sh set_phase "Angular 16 migration"`
   - Run `scripts/migrate-to-angular16.sh`
   - Apply schematics: `npx schematic .:migrate-module-with-providers`
   - Run `scripts/validate-angular16.sh`
   - Track step: `scripts/track-progress.sh "Angular 16 migration" "completed"`

3. **Angular 16 to 17 Migration**
   - Set phase: `scripts/track-progress.sh set_phase "Angular 17 migration"`
   - Run `scripts/migrate-to-angular17.sh`
   - Apply schematics: `npx schematic .:add-input-required`
   - Run `scripts/validate-angular17.sh`
   - Track step: `scripts/track-progress.sh "Angular 17 migration" "completed"`

4. **Final Validation**
   - Set phase: `scripts/track-progress.sh set_phase "Final validation"`
   - Run `scripts/run-critical-tests.sh`
   - Run `scripts/run-integration-tests.sh`
   - Run `scripts/generate-report.sh`
   - Track step: `scripts/track-progress.sh "Migration" "completed"`

### **Error Handling for LLM Agent**

If any step fails:
1. Check error type against `error-handling/error-strategies.yml`
2. Apply auto-fix commands if available
3. If auto-fix fails, apply fallback strategies
4. If critical failure, execute `scripts/auto-rollback.sh`
5. Generate error report and notify

### **Checkpoint Validation**

After each major phase, validate against `checkpoints/migration-checkpoints.yml`:
- Run validation commands
- Check expected patterns
- Verify exit codes
- Proceed only if all validations pass

---

*Last Updated: [Current Date]*
*Version: 3.0 - Enhanced with LLM Agent Automation*
*Maintainer: EAF Development Team*
