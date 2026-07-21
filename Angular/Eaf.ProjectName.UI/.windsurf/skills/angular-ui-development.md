# Angular UI Development Skill - EAF Project Name UI

You are an expert in TypeScript, Angular 17-19, and the EAF (Enterprise Application Foundation) UI template. You write functional, maintainable, performant, and accessible code following Angular and TypeScript best practices specifically for the EAF Angular UI template.

## Project Context

This is the EAF Angular UI template located in `Templates/Angular/Eaf.ProjectName.UI`. It's a frontend application that communicates with a .NET 10.0 backend using ABP framework.

### Current State
- **Angular Version**: 17.0.0 (migrating to 19)
- **TypeScript**: 5.2 (migrating to 5.5)
- **UI Libraries**: PrimeNG 17.0.0, ngx-bootstrap 10.2.0
- **Node.js**: >=18 <22
- **Build Tool**: Angular CLI 17.0.0 (migrating to 19.0.0)
- **Components**: 37 components across account, admin, and main modules
- **Modules**: 16 modules including routing and feature modules

### Key Integrations
- **SignalR**: @microsoft/signalr@^7.0.14 for real-time updates
- **jQuery**: 3.7.1 for legacy components (gradual migration planned)
- **EAF Framework**: Custom framework in `src/assets/lib/eaf-web-resources/`
- **Service Proxies**: Auto-generated from backend in `src/shared/service-proxies/`

## Angular Best Practices (EAF UI Specific)

### Component Architecture
- Use standalone components (Angular 18+ default)
- Set `changeDetection: ChangeDetectionStrategy.OnPush`
- Use `input()` and `output()` functions instead of decorators
- Keep components focused on single responsibility
- Use `computed()` for derived state
- Prefer inline templates for small components (< 100 lines)

### Template Guidelines
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use `class` bindings instead of `ngClass`
- Use `style` bindings instead of `ngStyle`
- Use async pipe for observables
- Do NOT assume globals like `new Date()` are available
- Keep templates simple, avoid complex logic

### EAF UI Component Structure

#### Account Components (src/account/)
- `account.component.ts` - Main account layout
- `login.component.ts` - Login form with SSO support
- `forgot-password.component.ts` - Password recovery
- `reset-password.component.ts` - Password reset
- `email-activation.component.ts` - Email activation
- `confirm-email.component.ts` - Email confirmation
- `sso.component.ts` - Single Sign-On integration

#### Admin Components (src/app/admin/)
- `audit-logs.component.ts` - Audit log viewing
- `languages.component.ts` - Language management
- `roles.component.ts` - Role management
- `tenants.component.ts` - Tenant management
- `users.component.ts` - User management
- `ui-customization.component.ts` - UI theme settings
- `hangfire.component.ts` - Background job dashboard
- `maintenance.component.ts` - System maintenance

#### Shared Admin Components (src/app/admin/shared/)
- `permission-tree.component.ts` - Permission tree view
- `feature-tree.component.ts` - Feature tree view
- `permission-combo.component.ts` - Permission dropdown
- `role-combo.component.ts` - Role dropdown

#### Main Components (src/app/main/)
- Various feature-specific components

### Service Integration Patterns

#### Service Proxy Usage
```typescript
// Use auto-generated service proxies
import { UserServiceProxy } from './service-proxies/service-proxies';

@Component({
  standalone: true,
  providers: [UserServiceProxy]
})
export class UsersComponent {
  private userService = inject(UserServiceProxy);
  
  users = signal<UserDto[]>([]);
  
  loadUsers() {
    this.userService.getAll().subscribe(result => {
      this.users.set(result.items);
    });
  }
}
```

#### SignalR Integration
```typescript
// SignalR for real-time updates
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection: HubConnection | null = null;
  
  connect() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/signalr/notification')
      .build();
      
    this.hubConnection.start();
  }
}
```

### PrimeNG Component Usage

#### Common PrimeNG Patterns
```typescript
// PrimeNG table with signals
@Component({
  standalone: true,
  imports: [TableModule, CommonModule]
})
export class UsersComponent {
  users = signal<UserDto[]>([]);
  selectedUsers = signal<UserDto[]>([]);
  
  onRowSelect(event: any) {
    console.log('Selected:', event.data);
  }
}
```

```html
<p-table [value]="users()" [selection]="selectedUsers()" 
        selectionMode="multiple" dataKey="id">
  <ng-template pTemplate="header">
    <tr>
      <th pSortableColumn="name">Name <p-sortIcon field="name"></p-sortIcon></th>
    </tr>
  </ng-template>
  <ng-template pTemplate="body" let-user>
    <tr [pSelectableRow]="user">
      <td>{{user.name}}</td>
    </tr>
  </ng-template>
</p-table>
```

### Modal Component Pattern

```typescript
// Modal component pattern used throughout EAF
@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, BsModalRef]
})
export class CreateOrEditUserModalComponent {
  modalRef = inject(BsModalRef);
  user = signal<UserDto>({});
  
  save() {
    this.modalRef.hide();
  }
  
  close() {
    this.modalRef.hide();
  }
}
```

### Form Validation Pattern

```typescript
// Reactive forms with validation
@Component({
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class CreateUserComponent {
  private fb = inject(FormBuilder);
  
  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.pattern('^[0-9]*$')]]
  });
  
  onSubmit() {
    if (this.form.valid) {
      // Submit form
    }
  }
}
```

## EAF Framework Integration

### EAF.js Initialization
```typescript
// Initialize EAF framework after Angular bootstrap
export class AppComponent implements AfterViewInit {
  ngAfterViewInit() {
    // EAF framework initialization
    if (typeof (window as any).EAF !== 'undefined') {
      (window as any).EAF.initialize();
    }
  }
}
```

### EAF Path Mappings
```json
// tsconfig.json
{
  "compilerOptions": {
    "paths": {
      "@eaf/*": ["src/assets/lib/eaf-ng2-module/src/*"]
    }
  }
}
```

## Migration Guidelines (Angular 17 to 19)

### Control Flow Migration
```typescript
// Before
<div *ngIf="isVisible">Content</div>
<div *ngFor="let item of items; trackBy: trackById">{{ item.name }}</div>

// After
@if (isVisible) {
  <div>Content</div>
}
@for (item of items; track item.id) {
  <div>{{ item.name }}</div>
}
```

### Standalone Component Migration
```typescript
// Before (Module-based)
@NgModule({
  declarations: [MyComponent],
  imports: [CommonModule, FormsModule]
})
export class MyModule {}

// After (Standalone)
@Component({
  selector: 'app-my',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyComponent {}
```

## Testing Guidelines

### Test Structure
```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UsersComponent } from './users.component';
import { UserServiceProxy } from '../service-proxies/service-proxies';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  let mockUserService: jasmine.SpyObj<UserServiceProxy>;

  beforeEach(async () => {
    mockUserService = jasmine.createSpyObj('UserServiceProxy', ['getAll']);
    
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        BrowserAnimationsModule,
        UsersComponent
      ],
      providers: [
        { provide: UserServiceProxy, useValue: mockUserService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    const mockData = { items: [{ id: 1, name: 'Test' }] };
    mockUserService.getAll.and.returnValue(of(mockData));
    
    component.ngOnInit();
    fixture.detectChanges();
    
    expect(mockUserService.getAll).toHaveBeenCalled();
  });
});
```

### Coverage Target
- Aim for 90%+ code coverage
- Test all 37 components
- Test service integrations
- Test form validation
- Test modal interactions

## Material Design Migration (Planned)

### Material Installation
```bash
npm install @angular/material@^19.0.0 @angular/cdk@^19.0.0
npm install @angular/animations@^19.0.0
```

### Material Theme Configuration
```scss
// src/styles.scss
@use '@angular/material' as mat;
@use '@angular/material/theming';

$primary-palette: mat.$azure-palette;
$accent-palette: mat.$fuchsia-palette;

$theme: mat.define-theme((
  color: (
    primary: $primary-palette,
    accent: $accent-palette,
  ),
));

:root {
  @include mat.all-component-themes($theme);
}
```

### Component Migration Pattern
```typescript
// Before (PrimeNG)
<p-button label="Click me" (onClick)="handleClick()"></p-button>

// After (Material)
<button mat-raised-button (click)="handleClick()">Click me</button>
```

## Common Issues and Solutions

### jQuery Conflicts
- Isolate jQuery usage in specific components
- Use `ngAfterViewInit` for jQuery initialization
- Test change detection after jQuery operations

### SignalR Reconnection
```typescript
// Handle SignalR reconnection
this.hubConnection.onreconnecting(() => {
  console.log('Reconnecting...');
});

this.hubConnection.onreconnected(() => {
  console.log('Reconnected');
  // Refresh data
});
```

### EAF Framework Timing
- Ensure EAF initializes after Angular bootstrap
- Use `ngAfterViewInit` for EAF-dependent code
- Test EAF integration after Angular upgrades

## Performance Optimization

- Use OnPush change detection
- Implement lazy loading for routes
- Use signals for state management
- Optimize PrimeNG table with virtual scrolling
- Use deferred loading for heavy components
- Consider zoneless mode (Angular 19)

## Accessibility

- Ensure WCAG AA compliance
- Use semantic HTML
- Provide ARIA labels for interactive elements
- Test with screen readers
- Maintain keyboard navigation
- Ensure color contrast ratios

## File Naming Conventions

- Component files: `name.component.ts`
- Service files: `name.service.ts`
- Module files: `name.module.ts`
- Test files: `name.component.spec.ts`
- Use kebab-case for file names
- Match file name to class name (PascalCase)

## When in Doubt

- Follow the existing EAF patterns
- Check similar components for reference
- Test changes thoroughly
- Consult the migration guide in `docs/MIGRATION_ANGULAR_17_TO_19.md`
- Follow Angular official documentation
- Maintain consistency with existing code
