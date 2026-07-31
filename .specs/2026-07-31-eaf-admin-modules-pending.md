# Pendências da Migração de Módulos EAF Admin

## Contexto

A migração dos módulos administrativos do `afonsoft/EAF` para o `afonsoft/gamehub` foi iniciada. A versão atual do pacote `Eaf.Middleware` (9.4.1) ainda não publicou as entidades e AppServices mais recentes dos módulos `MassNotifications`, `UserDelegations` e `Payments`. Por isso, esses módulos foram **documentados como pendentes** neste arquivo.

## O que já foi implementado

- Backend (`Api/src/GameHub.Application/Administration/`):
  - `Editions` — AppService, DTOs, mapeamentos AutoMapper.
  - `OrganizationUnits` — AppService, DTOs, mapeamentos AutoMapper.
  - `Dashboard` — AppService host/tenant e DTOs.
- Frontend (`angular-admin/GameHub.UI`):
  - Componentes `admin/editions`, `admin/organization-units` e `main/dashboard`.
  - Service proxies manuais (`edition`, `organization-unit`, `dashboard`).
  - Registro dos proxies em `service-proxy.module.ts`.
  - Itens de menu Editions e OrganizationUnits.
  - Mocks para os specs de teste.

## Módulos pendentes

### 1. MassNotifications

#### Backend
- Copiar/adapter para `Api/src/GameHub.Application/Administration/MassNotifications/`:
  - `MassNotificationAppService.cs`
  - `IMassNotificationAppService.cs`
  - DTOs (`MassNotificationDto`, `CreateMassNotificationInput`, `GetMassNotificationsInput`, etc.)
- A entidade `MassNotification` precisa existir no modelo. Opções:
  - Aguardar novo pacote `Eaf.Middleware` que já contenha a entidade.
  - Ou copiar `src/Eaf.Middleware.Core/MassNotifications/MassNotification.cs` para `Api/src/GameHub.Core/MassNotifications/`, adicionar `DbSet<MassNotification>` em `GameHubDbContext` e criar uma nova migration EF.
- Adicionar mapeamentos AutoMapper em `GameHubCustomDtoMapper.cs`:
  - `MassNotification -> MassNotificationDto`
  - `CreateMassNotificationInput -> MassNotification`
- Permissões já estão no `MiddlewarePermissions` (`Pages.Administration.MassNotifications`, `.Create`, `.Delete`).

#### Frontend
- Copiar `Templates/Angular/Eaf.ProjectName.UI/src/app/admin/mass-notifications/` para `angular-admin/GameHub.UI/src/app/admin/mass-notifications/`.
- Adicionar rota e declaração em `admin-routing.module.ts` e `admin.module.ts`.
- Copiar/regenerar `mass-notification.service-proxy.ts`.
- Adicionar entrada de menu `MassNotifications`.
- Incluir chaves de localização (`MassNotifications`, `CreatingNewMassNotification`, `CancelingMassNotification`, etc.).

### 2. UserDelegations

#### Backend
- Copiar/adapter para `Api/src/GameHub.Application/Administration/UserDelegations/`:
  - `UserDelegationAppService.cs`
  - `IUserDelegationAppService.cs`
  - `UserDelegationManager.cs`
  - DTOs (`UserDelegationDto`, `CreateUserDelegationInput`, `GetUserDelegationsInput`, etc.)
- A entidade `UserDelegation` precisa existir no modelo. Opções idênticas ao `MassNotification`.
- Adicionar mapeamentos AutoMapper em `GameHubCustomDtoMapper.cs`.
- Permissão `Pages.Administration.Users.Delegation`.

#### Frontend
- Copiar `Templates/Angular/Eaf.ProjectName.UI/src/app/admin/user-delegations/` para `angular-admin/GameHub.UI/src/app/admin/user-delegations/`.
- Registrar rota/componente.
- Copiar/regenerar `user-delegation.service-proxy.ts`.
- Adicionar entrada de menu.
- Incluir chaves de localização (`UserDelegations`, `MyDelegations`, `DelegatedUsers`, etc.).

### 3. Payments

#### Backend
- Copiar/adapter para `Api/src/GameHub.Application/Administration/Payments/`:
  - `PaymentAppService.cs`
  - `IPaymentAppService.cs`
  - Gateways e DTOs (`SubscriptionPaymentDto`, `CreatePaymentInput`, `ProcessPaymentInput`, etc.)
- A entidade `SubscriptionPayment` precisa existir no modelo.
- Adicionar mapeamentos AutoMapper.
- Permissões (`Pages.Administration.Payments`, `.Create`, `.Process`).

#### Frontend
- Copiar `Templates/Angular/Eaf.ProjectName.UI/src/app/admin/payments/`.
- Registrar rota/componente.
- Copiar/regenerar `payment.service-proxy.ts`.
- Adicionar entrada de menu.
- Incluir chaves de localização (`Payments`, `CreatingNewPayment`, `ProcessingPayment`, etc.).

## Itens transversais pendentes

### Regeneração dos service proxies
- O comando `npm run service-update` depende do `nswag` npm package 13.4.2, que traz binários para .NET 7. A VM atual não possui o runtime .NET 7; por isso os proxies foram copiados manualmente e ajustados.
- Quando o ambiente tiver o runtime .NET 7 ou quando o `nswag` for atualizado para uma versão compatível com .NET 8/10, executar:
  ```bash
  cd angular-admin/GameHub.UI
  nvm use 18
  npm install --legacy-peer-deps
  npm run service-update
  ```
- Após a regeneração, `service-proxy.module.ts` deve ser atualizado para listar os novos proxies (`EditionServiceProxy`, `OrganizationUnitServiceProxy`, `DashboardServiceProxy`, etc.).

### Localização
- As chaves mínimas listadas na especificação original (`.specs/2026-07-31-eaf-template-migration.md`) devem ser adicionadas ao source `GameHub` (`Api/src/GameHub.Core/Application/Localization/GameHub/GameHub-pt-BR.xml` e/ou `GameHub-en.xml`).
- Muitas das chaves já existem no `EafCore` e são resolvidas por fallback, mas para garantir cobertura pt-BR/EN deve-se explicitá-las no `GameHub`.

### Testes
- Os specs Angular dos módulos pendentes dependem dos `service-proxies` correspondentes.
- Para o backend, recomenda-se criar testes de integração/xUnit para os novos AppServices seguindo o padrão BDD `Dado_Quando_Entao` já adotado no projeto.

### Migrações EF
- Se optar por copiar as entidades (`MassNotification`, `UserDelegation`, `SubscriptionPayment`) antes do pacote EAF, gerar migration:
  ```bash
  cd Api/src/GameHub.EntityFrameworkCore
  dotnet ef migrations add AddEafAdminEntities --startup-project ../GameHub.Web.Host
  ```
- Verificar se o provider é PostgreSQL e se a migration gera tabelas corretamente (`AbpMassNotifications`, `AbpUserDelegations`, `AbpSubscriptionPayments` ou nomes equivalentes).

## Referências

- EAF source:
  - `src/Eaf.Middleware.Application/{MassNotifications,UserDelegations,Payments}/`
  - `src/Eaf.Middleware.Core/{MassNotifications,UserDelegations,Payments}/`
  - `Templates/Angular/Eaf.ProjectName.UI/src/app/admin/{mass-notifications,user-delegations,payments}/`
- GameHub:
  - `.specs/2026-07-31-eaf-template-migration.md` (especificação completa)
  - `docs/agent-execution-log.md` (registro da implementação atual)
