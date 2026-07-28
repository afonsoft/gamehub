# Execution Plan — Melhorias no angular-admin GameHub

## 1. Goal and context

Evoluir o `angular-admin` para um painel por perfil/tenant, corrigir navegação de empresas, padronizar tabelas, expor ações do ciclo de vida do jogo, adicionar audit/notification snippets e prover documentação/help in-app.

## 2. Impacted files and modules

| Área | Arquivos | Mudança |
|------|----------|---------|
| Dashboard por perfil/tenant | `dashboard/dashboard.component.ts`, `dashboard.component.html` | `canViewDeveloper`, `developerDashboard`, `currentTenantName`, `loadDeveloperDashboard`, cards de KPIs do dev |
| Serviço admin | `shared/services/gamehub-admin.service.ts` | `getDeveloperDashboard`, `publishGame`, `approveForPublishing`, `requestChanges`, `startReview` |
| Roteamento | `gamehub-routing.module.ts` | permissão da rota `dashboard` → `Pages.Developer.Games` |
| Menu lateral | `shared/layout/nav/app-navigation.service.ts` | ajustar `GameHub` para `Pages.Developer.Games`; corrigir rota `Companies` para `/app/main/gamehub/companies` |
| Empresas | `companies/company-list.component.ts/.html`, `company-edit.component.ts/.html`, `company-employees.component.ts/.html` | corrigir `routerLink`, melhorar UX, badges |
| Tabelas | `games/game-list.component.ts/.html`, `users/user-list.component.ts/.html`, `reports/report-list.component.ts/.html`, `moderation/review-queue.component.ts/.html`, `uploads/build-list.component.html` | paginação, lazy load, filtros, status badges, loading |
| Ciclo de vida do jogo | `games/game-list.component.ts/.html` | ações `StartReview`, `ApproveForPublishing`, `RequestChanges`, `Suspend` |
| Auditoria/notificações | `dashboard/dashboard.component.ts/.html`, serviço | exibir últimos audit logs no dashboard |
| Documentação/help | `docs/gamehub-admin-features.md`, `docs/gamehub-features.md`, `docs/agent-execution-log.md`; componente `help/help-panel.component.ts` e rota `/app/main/gamehub/help` |

## 3. Implementation strategy

1. **Roteamento e navegação**: corrigir permissões e `routerLink` de empresas primeiro para evitar telas quebradas.
2. **Dashboard**: adicionar `developerDashboard`, exibir tenant atual e seções condicionais por perfil.
3. **Empresas/funcionários**: corrigir rotas, adicionar loading/badges e confirmar exclusão.
4. **Padronização de tabelas**: aplicar `p-table` com paginator, lazy load, `loading`, `rowsPerPageOptions` e mensagens de vazio.
5. **Ciclo de vida do jogo**: adicionar métodos no serviço e botões contextuais por status.
6. **Audit/notification**: carregar últimos `AuditLog` no dashboard.
7. **Documentação/help**: criar docs bilíngues (PT/EN) e rota de help no admin.

## 4. Risks and mitigations

- **Permissão da rota dashboard**: trocar para `Pages.Developer.Games` pode esconder de moderadores sem essa permissão; mitigação: moderadores continuam acessando via outras rotas (`moderation`, `reports`).
- **Status inexistentes no filtro**: adicionar todos os valores do enum `GameStatus` para evitar badges sem classe.
- **Lazy load em tabelas sem backend paginado**: fallback para `paginator="false"` quando a API retornar lista completa.
- **RouterLink errado de empresas**: risco de links quebrados; mitigação: corrigir todos para `/app/main/gamehub/companies`.

## 5. Validation steps

- `dotnet test Api/GameHub.sln -c Release`
- `npm run build` em `angular-admin/GameHub.UI/`
- `npm test` se ChromeHeadless disponível
- Commit e PR para `main` via `feature/devin-20260728-gamehub-angular-admin-next`
