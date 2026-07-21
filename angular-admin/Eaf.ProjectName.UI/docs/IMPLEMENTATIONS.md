# Implementations - EAF Angular UI Template

## Overview

This document describes the key implementation patterns and practices used in the EAF Angular UI template.

## Base Classes

### AppComponentBase

**Location**: `src/shared/common/app-component-base.ts`

All components in the EAF template extend `AppComponentBase`, which provides:

- **Localization**: `l()` and `ls()` methods for translating text
- **Permission Checking**: `isGranted()` and `isGrantedAny()` methods
- **Feature Checking**: `isFeatureEnabled()` method
- **Notification Services**: `notify.info()`, `notify.success()`, `notify.warn()`, `notify.error()`
- **Multi-tenancy Services**: Tenant information and switching
- **Session Management**: Current user and tenant information
- **UI Customization**: Access to theme settings
- **Data Table Helper**: Helper for PrimeNG DataTables

### Usage Example

```typescript
export class MyComponent extends AppComponentBase {
  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit() {
    if (this.isGranted('Pages.MyEntity.Create')) {
      this.notify.info(this.l('PermissionGranted'));
    }
  }
}
```

## Authentication Implementation

### Login Flow

**Location**: `src/account/login/`

The login flow involves:

1. **LoginComponent**: Displays login form with username/password
2. **LoginService**: Handles authentication logic
3. **TokenAuthServiceProxy**: Calls backend API for authentication
4. **StorageService**: Stores tokens in cookies
5. **Multi-tenancy**: Tenant selection on login page

### External Authentication

The template supports multiple external authentication providers:

#### Google Authentication

```typescript
// Uses gapi.auth2 for OAuth2
this._loginService.authenticate(
  this.authenticateModel,
  (result) => {
    // Handle success
  },
  (error) => {
    // Handle error
  }
);
```

#### Microsoft Authentication

```typescript
// Uses MSAL (Microsoft Authentication Library)
this._loginService.authenticateExternal(
  'Microsoft',
  (result) => {
    // Handle success
  }
);
```

#### Auth0 Authentication

```typescript
// Uses Auth0 SPA SDK
this._loginService.authenticateExternal(
  'Auth0',
  (result) => {
    // Handle success
  }
);
```

#### OpenID Connect

```typescript
// Uses angular-oauth2-oidc
this._loginService.authenticateExternal(
  'OpenIdConnect',
  (result) => {
    // Handle success
  }
);
```

### Token Management

Tokens are managed through:

- **StorageService**: Stores access token and encrypted token in cookies
- **Token Expiration**: Tracks token expiration with configurable duration
- **Remember Me**: Extends token lifetime (10x normal expiration)

## CRUD Implementation Pattern

### Entity List Component

**Location**: `src/app/main/airplanes/airplanes.component.ts`

Standard CRUD list component pattern:

```typescript
export class AirplanesComponent extends AppComponentBase {
  airplanes: ListResultDtoOfAirplaneListDto;
  advancedFiltersVisible = false;

  constructor(
    injector: Injector,
    private _airplanesService: AirplanesServiceProxy
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getAirplanes();
  }

  getAirplanes(): void {
    this._airplanesService.getAll()
      .subscribe(result => {
        this.airplanes = result;
      });
  }

  deleteAirplane(airplane: AirplaneListDto): void {
    this.message.confirm(
      this.l('AreYouSure', airplane.name),
      this.l('Delete'),
      isConfirmed => {
        if (isConfirmed) {
          this._airplanesService.delete(airplane.id)
            .subscribe(() => {
              this.notify.success(this.l('SuccessfullyDeleted'));
              this.getAirplanes();
            });
        }
      }
    );
  }
}
```

### Create/Edit Modal Component

**Location**: `src/app/main/airplanes/create-or-edit-airplane-modal.component.ts`

Standard modal component pattern:

```typescript
export class CreateOrEditAirplaneModalComponent extends AppComponentBase {
  airplane = new AirplaneEditDto();
  saving = false;

  constructor(
    injector: Injector,
    public activeModal: NgbActiveModal,
    private _airplanesService: AirplanesServiceProxy
  ) {
    super(injector);
  }

  save(): void {
    this.saving = true;
    this._airplanesService.createOrUpdate(this.airplane)
      .pipe(finalize(() => { this.saving = false; }))
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.close();
      });
  }

  close(): void {
    this.activeModal.dismiss();
  }
}
```

## Data Table Implementation

### PrimeNG DataTable with Lazy Loading

**Location**: `src/app/admin/users/users.component.ts`

Standard data table pattern:

```typescript
export class UsersComponent extends AppComponentBase {
  table: PagedResultDtoOfUserListDto;
  advancedFiltersVisible = false;

  constructor(
    injector: Injector,
    private _usersService: UserServiceProxy,
    private _dataTableHelper: DataTableHelper
  ) {
    super(injector);
  }

  ngOnInit() {
    this._dataTableHelper.init();
    this.getUsers();
  }

  getUsers(): void {
    this._dataTableHelper.showLoading(this.primengTableHelper);
    this._usersService.getAll(
      this._dataTableHelper.getSorting(),
      this._dataTableHelper.getSkipCount(),
      this._dataTableHelper.getMaxResultCount()
    ).pipe(finalize(() => {
      this._dataTableHelper.hideLoading(this.primengTableHelper);
    })).subscribe(result => {
      this.table = result;
      this._dataTableHelper.totalRecordsCount = result.totalCount;
      this._dataTableHelper.records = result.items;
      this._dataTableHelper.hideLoading(this.primengTableHelper);
    });
  }
}
```

## SignalR Implementation

### Chat SignalR Service

**Location**: `src/app/shared/layout/chat/chat-signalr.service.ts`

SignalR connection management:

```typescript
@Injectable()
export class ChatSignalrService {
  private _connection: signalR.HubConnection;

  constructor(private _chatService: ChatServiceProxy) {}

  connect(): void {
    this._connection = new signalR.HubConnectionBuilder()
      .withUrl(AppConsts.remoteServiceBaseUrl + '/signalr')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this._connection.on('getMessage', (message) => {
      // Handle incoming message
    });

    this._connection.start().then(() => {
      // Connection started
    });
  }

  sendMessage(message: string, targetUserId: number): void {
    this._connection.invoke('SendMessage', message, targetUserId);
  }
}
```

## Localization Implementation

### Localize Pipe

**Location**: `src/shared/utils/pipes/localize.pipe.ts`

Pipe for template-level localization:

```typescript
@Pipe({
  name: 'localize',
  pure: true
})
export class LocalizePipe implements PipeTransform {
  constructor(private _localizationService: AppLocalizationService) {}

  transform(key: string): string {
    return this._localizationService.localize(key);
  }
}
```

### Usage in Templates

```html
<h1>{{ 'Users' | localize }}</h1>
```

### Component-Level Localization

```typescript
export class MyComponent extends AppComponentBase {
  ngOnInit() {
    this.notify.info(this.l('WelcomeMessage'));
  }
}
```

## Permission Implementation

### Permission Checker Service

**Location**: `src/shared/common/auth/permission-checker.service.ts`

Check permissions in components:

```typescript
export class MyComponent extends AppComponentBase {
  ngOnInit() {
    if (this.isGranted('Pages.Users.Create')) {
      // User has permission
    }

    if (this.isGrantedAny(['Pages.Users.Create', 'Pages.Users.Edit'])) {
      // User has any of these permissions
    }
  }
}
```

### Route Guards

**Location**: `src/app/shared/auth/auth-route-guard.ts`

Protect routes with permissions:

```typescript
@Injectable()
export class AppRouteGuard implements CanActivate {
  constructor(
    private _permissionChecker: PermissionCheckerService,
    private _router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (!route.data || !route.data['permission']) {
      return true;
    }

    if (this._permissionChecker.isGranted(route.data['permission'])) {
      return true;
    }

    this._router.navigate(['/']);
    return false;
  }
}
```

### Route Configuration

```typescript
{
  path: 'users',
  component: UsersComponent,
  canActivate: [AppRouteGuard],
  data: { permission: 'Pages.Users' }
}
```

## Multi-Tenancy Implementation

### Tenant Selection

**Location**: `src/account/login/`

Tenant selection on login page:

```typescript
export class LoginComponent extends AppComponentBase {
  tenantId: number;

  login(): void {
    this.authenticateModel.tenancyName = this.tenancyName;
    this._loginService.authenticate(this.authenticateModel, ...);
  }
}
```

### Tenant Service

**Location**: `src/shared/common/session/app-session.service.ts`

Access tenant information:

```typescript
export class MyComponent extends AppComponentBase {
  ngOnInit() {
    const tenantId = this.appSession.tenantId;
    const tenantName = this.appSession.tenant?.name;
  }
}
```

## Notification Implementation

### Notify Service

**Location**: `src/shared/common/notify/notify.service.ts`

Display notifications:

```typescript
export class MyComponent extends AppComponentBase {
  showSuccess(): void {
    this.notify.success(this.l('OperationSuccessful'));
  }

  showError(): void {
    this.notify.error(this.l('OperationFailed'));
  }

  showWarning(): void {
    this.notify.warn(this.l('OperationWarning'));
  }

  showInfo(): void {
    this.notify.info(this.l('OperationInfo'));
  }
}
```

## Service Proxy Implementation

### Auto-Generated Proxies

**Location**: `src/shared/service-proxies/service-proxies.ts`

Service proxies are auto-generated by NSwag from the backend API Swagger definition.

### Using Service Proxies

```typescript
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';

export class MyComponent extends AppComponentBase {
  constructor(
    injector: Injector,
    private _userService: UserServiceProxy
  ) {
    super(injector);
  }

  getUser(id: number): void {
    this._userService.get(id).subscribe(user => {
      this.user = user;
    });
  }

  createUser(user: CreateUserDto): void {
    this._userService.create(user).subscribe(result => {
      this.notify.success(this.l('UserCreated'));
    });
  }
}
```

## Modal Implementation

### NgbModal

The template uses ngx-bootstrap (NgbModal) for modals.

### Opening a Modal

```typescript
export class MyComponent extends AppComponentBase {
  constructor(
    injector: Injector,
    private _modalService: NgbModal
  ) {
    super(injector);
  }

  openModal(): void {
    const modalRef = this._modalService.open(CreateOrEditUserModalComponent, {
      size: 'lg'
    });
    modalRef.componentInstance.user = this.selectedUser;
    modalRef.result.then(() => {
      // Modal closed
    }, () => {
      // Modal dismissed
    });
  }
}
```

## File Upload Implementation

### File Upload Service

**Location**: `src/shared/common/file/file-upload.service.ts`

Upload files to the server:

```typescript
export class MyComponent extends AppComponentBase {
  constructor(
    injector: Injector,
    private _fileUploadService: FileUploadService
  ) {
    super(injector);
  }

  onFileSelect(event: any): void {
    const file = event.files[0];
    this._fileUploadService.upload(file, (result) => {
      this.fileId = result;
      this.notify.success(this.l('FileUploaded'));
    });
  }
}
```

## Form Validation Implementation

### Reactive Forms

```typescript
export class CreateOrEditUserModalComponent extends AppComponentBase {
  userForm: FormGroup;

  constructor(
    injector: Injector,
    private _fb: FormBuilder
  ) {
    super(injector);
    this.userForm = this._fb.group({
      userName: ['', [Validators.required, Validators.maxLength(32)]],
      emailAddress: ['', [Validators.required, Validators.email]],
      name: ['', [Validators.required, Validators.maxLength(32)]]
    });
  }

  save(): void {
    if (this.userForm.invalid) {
      return;
    }
    // Save logic
  }
}
```

### Custom Validators

**Location**: `src/shared/utils/validators/equal-validator.ts`

Cross-field validation:

```typescript
export class EqualValidator implements Validator {
  validate(control: AbstractControl): ValidationErrors | null {
    if (!control.parent) {
      return null;
    }
    const password = control.parent.get('password').value;
    const confirmPassword = control.value;
    return password === confirmPassword ? null : { notEqual: true };
  }
}
```

## Theme Implementation

### Theme Switching

**Location**: `src/app/shared/layout/themes/`

Theme selection and application:

```typescript
export class UiCustomizationComponent extends AppComponentBase {
  currentTheme: AppUiCustomizationDto;

  changeTheme(theme: string): void {
    this.currentTheme.baseTheme.name = theme;
    this._uiCustomizationService.updateSettings(this.currentTheme)
      .subscribe(() => {
        window.location.reload();
      });
  }
}
```

## Best Practices

### Component Structure

```typescript
export class MyComponent extends AppComponentBase implements OnInit, OnDestroy {
  // Properties
  items: MyItemDto[];
  loading = false;

  // Constructor
  constructor(
    injector: Injector,
    private _myService: MyServiceProxy
  ) {
    super(injector);
  }

  // Lifecycle hooks
  ngOnInit(): void {
    this.loadItems();
  }

  ngOnDestroy(): void {
    // Cleanup
  }

  // Public methods
  loadItems(): void {
    this.loading = true;
    this._myService.getAll()
      .pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.items = result.items;
      });
  }
}
```

### Error Handling

```typescript
export class MyComponent extends AppComponentBase {
  loadData(): void {
    this._myService.getAll()
      .pipe(
        catchError(error => {
          this.notify.error(this.l('LoadFailed'));
          return of([]);
        })
      )
      .subscribe(result => {
        this.items = result;
      });
  }
}
```

### Unsubscribing

```typescript
export class MyComponent extends AppComponentBase implements OnDestroy {
  private subscription: Subscription;

  ngOnInit(): void {
    this.subscription = this._myService.getAll()
      .subscribe(result => {
        this.items = result;
      });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
```
