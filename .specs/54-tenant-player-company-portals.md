# Spec 54 — Tenant Player, Company Portal e Chat Tenant-Aware

**Escopo**: Implementar o plano `docs/superpowers/plans/2026-07-27-gamehub-tenant-player-company-portals.md`.

**Motivação**: Jogadores devem residir em um tenant `Player` dedicado; empresas são tenants distintos gerenciados pelo admin; chat e SDK usam o tenant `Player`; portal público mostra dados básicos da empresa e cadastro de funcionários.

---

## 1. Domain / Seed

- `GameHubConsts.PlayerTenantName = "Player"`
- `InitialHostDbBuilder` deve garantir tenant `Player` no seed.
- `RegistrationAppService` deve criar players no tenant `Player`.

## 2. Application

- `ICompanyAppService` e `CompanyAppService` (CRUD de empresas como tenants + `DeveloperTeam`).
- `ICompanyEmployeeAppService` e `CompanyEmployeeAppService` (convite, remoção, role, default).
- `GameChatAppService` deve resolver e usar o tenant `Player` para mensagens.
- `IUserTenantAssociationAppService` action names devem casar com o frontend (`GetUserMemberships`, `Associate`, `RemoveAssociation`, `SetDefault`).

## 3. Web Host

- `HubAuthController` valida tenant `Player` para players.
- `GameTokenProvider` emite token com tenant `Player` para jogos.

## 4. Frontend — angular-admin

- `CompanyService`, `CompanyListComponent`, `CompanyCreateComponent`, `CompanyDetailComponent`, `CompanyEditComponent`, `CompanyEmployeesComponent`.
- Menu `Companies` e `Employees`.
- Corrigir `UserTenantAssociationService` action names.
- Padronizar layouts de listas/tabelas.

## 5. Frontend — angular (hub)

- `CompanyService` (público), `CompanyPublicComponent`.
- Tela de convite de funcionário no fluxo `developer/team`.
- `GameplayBridgeService` usa tenant do token (Player) para chat/friendship.

## 6. Permissions

- `Pages.Companies`, `Pages.Companies.Manage`, `Pages.Company.Employees`, `Pages.Company.Employees.Manage`.

## 7. Tests

- Backend integration tests para `CompanyAppService`, `CompanyEmployeeAppService`, `GameChatAppService` tenant-aware, `GameTokenProvider`.
- Frontend specs para componentes e serviços novos.
- Docker end-to-end: registro player, criação de empresa, convite de funcionário, chat.

## 8. Docs

- `docs/tenancy-model.md`
- `docs/agent-execution-log.md`
