# EAF Template Migration & Update Spec

## Purpose

This specification describes how to migrate/update the **EAF API Template** and **EAF Angular UI Template** from one repository to another, or from an older version of the templates to the latest state. It is intended to be consumed by a future Devin session working on a different repository that uses the same `Templates/Api` and `Templates/Angular` folders.

## Scope

- `Templates/Api` — ASP.NET Core 10 / ABP backend project template.
- `Templates/Angular` — Angular 18+ / PrimeNG / Metronic frontend project template.

Out of scope:
- New backend features inside `src/Eaf.Middleware.*` modules.
- UI themes/layouts that require Metronic 8 licensing.

## Migration Steps

### 1. Preparation

1.1. Identify the source and target template versions.

| Source | Target |
|--------|--------|
| Current EAF repository (`afonsoft/EAF`) | Destination repository that contains the same `Templates/Api` and `Templates/Angular` layout |

1.2. Ensure the target repository has the same EAF middleware NuGet packages / project references. If the target uses a local copy of `Eaf.Middleware.*` source, keep it in sync first.

1.3. Create a working branch:

```bash
git checkout -b feature/devin-{YYYYMMDD}-eaf-template-migration
```

---

### 2. API Template (`Templates/Api`)

#### 2.1 Application Services / DTOs

The API template should expose the same application services that the Angular template expects. Copy or generate:

- `Controllers/Editions/EditionController.cs` (or use `IEditionAppService`)
- `Controllers/OrganizationUnits/OrganizationUnitController.cs`
- `Controllers/MassNotifications/MassNotificationController.cs`
- `Controllers/UserDelegations/UserDelegationController.cs`
- `Controllers/Payments/PaymentController.cs`
- `Controllers/Dashboard/DashboardController.cs`

> Note: the actual AppService implementations live in `src/Eaf.Middleware.Application`. The API template only needs controllers and DTOs. If the target repository does not reference `Eaf.Middleware.Application`, port the AppServices first.

#### 2.2 DTOs required in API template

If the target does not include `Eaf.Middleware.Application` as a project reference, copy the DTO files from the source:

- `Editions/Dto/*.cs`
- `OrganizationUnits/Dto/*.cs`
- `MassNotifications/Dto/*.cs`
- `UserDelegations/Dto/*.cs`
- `Payments/Dto/*.cs`
- `Dashboard/Dto/*.cs`

#### 2.3 Configuration & DI

Ensure the API template's `Startup.cs` / `Program.cs` registers the new AppServices and controllers:

```csharp
Configuration.Modules.AbpAspNetCore()
    .CreateControllersForAppServices(
        typeof(MiddlewareApplicationModule).GetAssembly()
    );
```

For ABP 10.5 with `Eaf.Middleware.Web.Core`, this is usually already present. If controllers are missing, add `[Route("api/services/app/[service]")]` attributes.

#### 2.4 Localization

Merge the new localization keys from `src/Eaf.Middleware.Core/Localization/Source/EafCore.xml` (and `EafCore-pt-BR.xml`) into the target's localization source. The minimum set of keys is listed in section 5.

#### 2.5 Migrations (optional)

If the API template has its own `DbContext` / `EntityFrameworkCore` project, ensure migrations exist for the new entities:

- `SubscriptionPayment`
- `UserDelegation`
- `MassNotification`
- `OrganizationUnit` related link tables (already in ABP Zero)

If the target uses the same `Eaf.Middleware.EntityFrameworkCore` module, no new migration is required inside the API template.

---

### 3. Angular Template (`Templates/Angular/Eaf.ProjectName.UI`)

#### 3.1 Service Proxies

The Angular template consumes service proxies. Do **not** edit `service-proxies.ts` files by hand. Instead:

1. Run the backend with `swagger.json` exposed.
2. Run the NSwag / ` nswag ` / `refresh.sh` script in the Angular template.
3. Verify the following proxies are regenerated:
   - `edition.service-proxy.ts`
   - `organization-unit.service-proxy.ts`
   - `mass-notification.service-proxy.ts`
   - `user-delegation.service-proxy.ts`
   - `payment.service-proxy.ts`
   - `dashboard.service-proxy.ts`

If NSwag is not available, manually copy the interfaces and service classes from the source template.

#### 3.2 Components

Copy the following component folders and their `.ts`, `.html`, `.spec.ts` files (if they exist):

- `src/app/admin/editions/`
- `src/app/admin/organization-units/`
- `src/app/admin/mass-notifications/`
- `src/app/admin/user-delegations/`
- `src/app/admin/payments/`
- `src/app/main/dashboard/`

Update `src/app/admin/admin.module.ts` and `src/app/admin/admin-routing.module.ts` with the new routes and component declarations.

Update `src/app/main/main.module.ts` and `src/app/main/main-routing.module.ts` with the Dashboard route.

#### 3.3 Shared Service Proxy Module

If the target has a `ServiceProxyModule`, register the new service proxies there, or confirm they are tree-shakeable as provided-in-root. In the source template the proxies are provided as injectable services.

#### 3.4 Menu

Add menu entries in the target's navigation JSON / TypeScript file. Example:

```typescript
{
  label: 'Administration',
  items: [
    { label: 'Editions', route: '/app/admin/editions', permission: 'Pages.Administration.Editions' },
    { label: 'OrganizationUnits', route: '/app/admin/organization-units', permission: 'Pages.Administration.OrganizationUnits' },
    { label: 'MassNotifications', route: '/app/admin/mass-notifications', permission: 'Pages.Administration.MassNotifications' },
    { label: 'UserDelegations', route: '/app/admin/user-delegations', permission: 'Pages.Administration.Users.Delegation' },
    { label: 'Payments', route: '/app/admin/payments', permission: 'Pages.Administration.Payments' },
  ]
}
```

#### 3.5 Localization

Merge the same keys from section 5 into the Angular `src/assets/i18n` or XML source used by the target.

#### 3.6 Tests

Copy `src/test-helpers/mock-services.ts` additions (or merge the new mock classes) and the `*.component.spec.ts` files. Run:

```bash
npx tsc -p src/tsconfig.spec.json --noEmit
```

#### 3.7 Styling

Merge the mobile/responsive CSS additions from `src/assets/common/styles/styles.css` into the target's global styles.

---

### 4. Verification

#### 4.1 Backend

```bash
cd Templates/Api
# or the API project root
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

#### 4.2 Frontend

```bash
cd Templates/Angular/Eaf.ProjectName.UI
nvm use 18
npm install --legacy-peer-deps
npx tsc -p src/tsconfig.app.json --noEmit
npx tsc -p src/tsconfig.spec.json --noEmit
npx ng build --configuration=production
```

> `npx ng test` is optional because it requires a Chrome/Chromium binary. If available, run `npx ng test --no-watch --browsers=ChromeHeadlessNoSandbox`.

---

### 5. Minimum Localization Keys

The following keys must exist in the target localization source (English + pt-BR):

- `Editions`, `CreatingNewEdition`, `EditingEdition`
- `OrganizationUnits`, `CreatingNewOrganizationUnit`, `EditingOrganizationUnit`, `DeletingOrganizationUnit`, `ManagingOrganizationUnitMembers`, `ManagingOrganizationUnitRoles`, `ManageMembers`, `ManageRoles`, `OrganizationUnitDeleteWarningMessage`, `OrganizationUnitUserRemoveWarningMessage`, `OrganizationUnitRoleRemoveWarningMessage`
- `MassNotifications`, `CreatingNewMassNotification`, `CancelingMassNotification`
- `UserDelegations`, `MyDelegations`, `DelegatedUsers`, `CreatingNewUserDelegation`, `UserDelegationCancelWarningMessage`, `StartTimeMustBeLessThanEndTime`
- `Payments`, `CreatingNewPayment`, `ProcessingPayment`, `ProcessPayment`, `Gateway`, `GatewayResponse`
- `HostDashboard`, `TenantDashboard`
- `Add`, `Close`, `Role`, `Parent`, `Description`, `Amount`, `ExternalPaymentId`, `PaymentTime`, `PaymentType`, `Status`, `Severity`, `Subject`, `ScheduledTime`, `SendToAllUsers`, `TargetUserIds`, `TargetRoleIds`, `TargetOrganizationUnitIds`, `StartTime`, `EndTime`, `SourceUser`, `TargetUser`
- `Active`, `Pending`, `Processing`, `Completed`, `Canceled`, `Failed`
- `RequiredField`, `ThisFieldIsRequired`, `SuccessfullyDeleted`, `SavedSuccessfully`

---

### 6. Permissions

Ensure the following permission names are declared in the target backend authorization provider:

- `Pages.Administration.Editions`
- `Pages.Administration.OrganizationUnits`
- `Pages.Administration.OrganizationUnits.Create`
- `Pages.Administration.OrganizationUnits.Edit`
- `Pages.Administration.OrganizationUnits.Delete`
- `Pages.Administration.OrganizationUnits.ManageMembers`
- `Pages.Administration.OrganizationUnits.ManageRoles`
- `Pages.Administration.MassNotifications`
- `Pages.Administration.Users.Delegation`
- `Pages.Administration.Payments`
- `Pages.Administration.Payments.Create`
- `Pages.Administration.Payments.Process`

---

### 7. Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| `404` on `/api/services/app/...` | Controllers not generated or module not loaded | Verify `CreateControllersForAppServices` includes the application module assembly |
| Angular proxy method not found | `service-proxies.ts` stale | Regenerate from swagger or copy from source |
| `A dictionary can not contain same key twice` | Duplicate localization key | Grep both `EafCore.xml` and `EafCore-pt-BR.xml` for duplicates |
| Sonar `S5906` in specs | Use `.toHaveSize(n)` instead of `.length` | Replace `expect(x.length).toBe(n)` with `expect(x).toHaveSize(n)` |
| Sonar `S6819` on modals | `role="dialog"` / `role="document"` on ngx-bootstrap modals | Remove those attributes |
| Mobile buttons too small | Missing responsive CSS | Add `min-width: 44px; min-height: 44px` for `.btn-sm` inside `@media (max-width: 768px)` |

---

### 8. References

- Source repository: `afonsoft/EAF`
- Source folders:
  - `Templates/Api`
  - `Templates/Angular/Eaf.ProjectName.UI`
  - `src/Eaf.Middleware.Application`
  - `src/Eaf.Middleware.Core/Localization/Source`
