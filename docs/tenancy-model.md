# GameHub Tenancy Model

## Overview

GameHub uses ABP multi-tenancy with two reserved tenants and a dynamic company model.

| Tenant | Purpose | Visibility |
|--------|---------|------------|
| `Default` | Built-in ABP host/seed tenant. | Internal |
| `Player` | Contains all player (gamer) accounts. | Public hub login |
| Custom company tenants | Each game development company is an isolated tenant. | Public company page + admin portal |

## Reserved Tenants

- `Player` is defined in `GameHubConsts.PlayerTenantName` and seeded by `PlayerTenantBuilder`.
- `Default` is the ABP seed tenant and cannot be managed as a company.

## User Model

- **Host users**: `TenantId == null`.
  - Can be associated with multiple company tenants.
  - Use `UserTenantMembership` to track the link and the default tenant.
- **Tenant users**: `TenantId != null`.
  - Player accounts live inside the `Player` tenant.
  - Company employees are shadow users created inside the company tenant by `TenantUserManager`.

## Domain Services

### `ITenantUserManager`

- `EnsureMembershipAsync(hostUserId, tenantId, isDefault)`: creates a shadow user in the tenant and links it to the host user.
- `RemoveMembershipAsync(hostUserId, tenantId)`: removes the membership and deletes the shadow user.
- `SetDefaultAsync(hostUserId, tenantId)`: marks a membership as the default for the host user.

## Application Services

- `CompanyAppService`: CRUD of tenants as companies, excluding `Player` and `Default`.
- `CompanyEmployeeAppService`: manage employees, invite by username/email, set default, remove.
- `CompanyEmployeeAppService.RegisterAndJoinAsync`: anonymous registration that creates a host developer user and associates it with a company tenant.
- `UserTenantAssociationAppService`: admin association of users to tenants.
- `HubAuthController`: public endpoints `available-tenants` and `select-tenant` to let a user pick the tenant after login.

## Chat & SDK

- `GameChatAppService` always resolves the `Player` tenant for the sender.
- If the current session is not in the `Player` tenant, it looks up the host user's `UserTenantMembership` for `Player` and uses the corresponding shadow user.
- `GameTokenProvider` emits tokens with `AbpClaimTypes.TenantId`, `AbpClaimTypes.UserId` and `tenantid` so SignalR/HTTP game endpoints are tenant-aware.

## Frontend

- `angular-admin`: `Companies` menu with list, create/edit and employee management screens.
- `angular` (public hub): `/company/:tenancyName` displays company info and allows new developers to register and join.
- Login flow (`/login` -> `/select-tenant`) lets the user pick the company or `Player` tenant.

## Migrations & Seeding

- `PlayerTenantBuilder` ensures the `Player` tenant exists in the host database.
- `GameHubPermissionSeeder` creates roles and permissions for the `Player` tenant.
- `CompanyAppService` seeds `Developer` and `Player` roles with permissions when a new company tenant is created.
- `GameHubTestBase` now seeds the `Player` tenant so chat and registration tests work.

## Implementation Notes

- `MayHaveTenant` filter must be explicitly enabled with `EnableFilter(AbpDataFilters.MayHaveTenant)`; `SetTenantId` only sets the filter parameter and does not re-enable a previously disabled filter.
- Shadow users are created inside the target tenant with `MayHaveTenant` enabled and `SetTenantId(tenantId)` so that `UserManager` validation runs against that tenant only.
- `CompanyEmployeeAppService` assigns the `Developer` role to the shadow user after `TenantUserManager.EnsureMembershipAsync`, ensuring company employees can access developer portal features.
