# EAF — Melhorias em Multi-Tenancy, Login Angular/API e SDK

Este documento é o plano executável correspondente ao spec `.specs/55-eaf-tenant-login-improvements.md`.

## Objetivo

Tornar o EAF capaz de suportar o cenário GameHub de multi-tenancy avançado:
- Usuário host pertencente a múltiplos tenants.
- Login em duas etapas (host → seleção de tenant → token escopado).
- Chat/SDK sempre operando no tenant `Player`.
- Replicação automática de roles/permissões para novos tenants.

## Fases

### Fase 1 — Core e Domínio
- Criar `UserTenantMembership` em `Eaf.Middleware.Core`.
- Criar `ITenantUserManager` e `TenantUserManager` com controle correto do `MayHaveTenant` filter.
- Criar `ITenantRolePermissionReplicationService` e implementação.
- Adicionar testes de domínio.

### Fase 2 — API
- Adicionar `GetAvailableTenants` e `SelectTenant` no `TokenAuthController` (Eaf.Middleware.Web.Core).
- Garantir que `LogInManager` autentique usuários host sem `tenancyName`.
- Adicionar testes de controller/API.

### Fase 3 — Template Angular
- Refatorar `login.component.ts` para fluxo de duas etapas.
- Criar `select-tenant.component.ts`.
- Atualizar `login.service.ts` e `eaf-auth.service.ts`.
- Adicionar specs Angular.

### Fase 4 — SDK / Bridge
- Atualizar `GameplayBridgeService` para enviar `tenantId`.
- Atualizar `HubAuthService` para consumir novos endpoints.

### Fase 5 — Infra e Documentação
- Criar migrations/seed para `UserTenantMembership`.
- Atualizar docs do EAF (`eaf-multi-tenant-login.md`, `eaf-tenant-user-manager.md`).
- Rodar simulação end-to-end no Docker Compose do template EAF.

## Critérios de Aceite

- `dotnet test Eaf.sln` passando.
- `ng test --no-watch` do template Angular passando.
- Docker Compose do template subindo backend + frontend.
- Simulação completa: player registration, criação de empresa, convite de funcionário, login tenant, criação de draft de jogo e chat.
