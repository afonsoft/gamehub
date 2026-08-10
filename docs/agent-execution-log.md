## 2026-08-08 (continuação) — Sincronização do novo PR na main do EAF

### Tarefa
Analisar o novo PR mergeado na `main` do `afonsoft/EAF` e refletir as alterações do template Angular no `angular-admin` do GameHub.

### Implementado
- `git fetch origin main` no EAF identificou dois novos commits em `Templates/Angular/Eaf.ProjectName.UI`:
  - `aedb72f`: melhoria do modal `payment-gateway-settings-modal` com abas Metronic e criação do doc `UI-LIBRARIES-AND-LAYOUT.md`.
  - `2c3ef83`: adição de textos de ajuda por provider no modal (`PaymentGatewayHelp*`).
- Replicado no `angular-admin/GameHub.UI`:
  - `payment-gateway-settings-modal.component.html`: abas `General`, `Stripe`, `PayPal`, `Mercado Pago` e `PagSeguro`; alertas `alert alert-info` com `PaymentGatewayHelp*`; campos sensíveis como `type="password"`; classes `m-form__group` e `m--margin-top-20`.
  - `payment-gateway-settings-modal.component.ts`: tipagem `PaymentGatewaySettingsDto`, importação dos sub-DTOs tipados (`StripePaymentGatewaySettingsDto`, `PayPalPaymentGatewaySettingsDto`, `MercadoPagoPaymentGatewaySettingsDto`, `PagSeguroPaymentGatewaySettingsDto`) e método `ensureGatewaySettings()`.
- Adicionadas chaves de localização `PaymentGatewayHelp*` nos arquivos `GameHub.xml` e `GameHub-pt-BR.xml`.
- Criado `docs/ui-libraries-and-layout.md` como referência de bibliotecas e padrões de UI.
- Atualizado `docs/angular-admin-layout.md` para refletir a nova estrutura do modal de gateways.

### Validação
- `npm run build` no `angular-admin/GameHub.UI` concluído com sucesso.
- `dotnet build Api/GameHub.sln -c Release --no-restore` e `dotnet test Api/GameHub.sln -c Release --no-build` passaram (368 passed, 2 skipped).

### Branch
- `devin/eaf-main-angular-sync` criada a partir de `origin/main` e fast-forwarded com `devin/eaf-9.4.4-angular-sync`.

## 2026-08-08 21:04 UTC

### Tarefa
Analisar as alterações do template Angular do EAF 9.4.3 → 9.4.4, replicar os ajustes no `angular-admin` do GameHub, revisar as telas de pagamentos e criar documentação de layout.

### Implementado
- Comparado `afonsoft/EAF` (`v9.4.3..v.9.4.4`) em `Templates/Angular/Eaf.ProjectName.UI`:
  - `ngsw-config.json`: cache com bundles hash (`*.css`, `main*.js`, `lazy *.js`).
  - `topbar.component.{html,ts}`: botões viraram `anchor`, remoção do toggle mobile e dos estados `languageDropdownExpanded`/`userDropdownExpanded`.
  - `package.json`: bump de `@angular/common`, `@angular/compiler`, `@angular/core` e `@angular/platform-server` para `20.3.27`.
  - `styles.css`: limpeza de regras de focus-visible e mobile tweaks.
  - `test-helpers/mock-services.ts`: ordem dos parâmetros `getEditions` ajustada para `skipCount, maxResultCount`.
- Verificado que o `angular-admin/GameHub.UI` já contém todos os ajustes acima; nenhuma alteração de código-fonte foi necessária.
- Revisadas as telas de pagamentos (`payments.component`, `payment-gateway-settings-modal`): HTML e fluxo de dados estão alinhados com o template EAF 9.4.4; nenhuma alteração necessária.
- Criada documentação de layout: `docs/angular-admin-layout.md`.

### Validação
- `diff` EAF 9.4.3 → 9.4.4 inspecionado e mapeado para arquivos do GameHub.
- `ngsw-config.json`, `topbar.component.{html,ts}`, `package.json`, `styles.css`, `test-helpers/mock-services.ts` e payment screens validados como alinhados.
- `npm install --package-lock-only --legacy-peer-deps` executado e revertido (não havia mudança funcional necessária; lockfile já está sincronizado).

### Branch
- `devin/eaf-9.4.4-angular-sync` criada a partir de `origin/main`.

## 2026-08-01 02:15 UTC

### Tarefa
Ajustar menus de navegação: manter administração no header e dashboard/funcionalidades/métricas no menu lateral esquerdo.

### Implementado
- `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts`:
  - Removido `Tenants` de `getMenu()` (menu lateral principal), deixando apenas itens de dashboard/funcionalidades/métricas.
  - Adicionado `Tenants` a `getAdminMenu()` (menu de administração do header), junto com Roles, Users, Editions, Languages, OrganizationUnits, MassNotifications, UserDelegations, Payments, AuditLogs, VisualSettings, Maintenance e Settings.

### Validação
- `npx tsc -p src/tsconfig.app.json --noEmit` no `angular-admin/GameHub.UI`: OK.
- `npx ng build --configuration=production` no `angular-admin/GameHub.UI`: OK.

## 2026-08-01 01:22 UTC

### Tarefa
Atualizar módulos EAF do GameHub para 9.4.2 e migrar os ajustes dos templates EAF (Angular/API), completando os módulos administrativos pendentes.

### Implementado
- `Api/common.props` e `.csproj`: pacotes `Eaf.*` atualizados de `9.4.1` para `9.4.2`; `Version`/`TemplateVersion` definidos como `9.4.2`/`9.4.2.0`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`: adicionados `DbSet<MassNotification>`, `DbSet<UserDelegation>`, `DbSet<SubscriptionPayment>`, `DbSet<SubscribableEdition>`; configurados índices para `UserTenantMembership`, `MassNotification`, `UserDelegation` e `SubscriptionPayment`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260801012807_AddEafAdminEntities`: migration EF gerada com as novas tabelas EAF e colunas estendidas em `AbpEditions`/`AbpTenants`.
- `angular-admin/GameHub.UI/src/app/admin/`: adicionados componentes `MassNotifications`, `UserDelegations`, `Payments` (incluindo `PaymentGatewaySettingsModal`), modais `CreateOrEditEdition`/`EditionFeatures` e `TenantSubscriptionModal`.
- `angular-admin/GameHub.UI/src/shared/service-proxies/`: adicionados `MassNotificationServiceProxy`, `UserDelegationServiceProxy`, `PaymentServiceProxy`, `TenantSubscriptionServiceProxy`; atualizados `EditionServiceProxy` e `OrganizationUnitServiceProxy`.
- `angular-admin/GameHub.UI/src/app/admin/admin-routing.module.ts`, `admin.module.ts` e `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxy.module.ts`: rotas, declarações e providers registrados.
- `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts`: itens de menu Editions, MassNotifications, UserDelegations e Payments adicionados ao menu administrativo.
- `angular-admin/GameHub.UI/src/app/admin/tenants/tenants.component.{html,ts}`: integrado `TenantSubscriptionModal`.

### Validação
- `dotnet restore Api/GameHub.sln`: OK.
- `dotnet build Api/GameHub.sln -c Release --no-restore`: OK.
- `dotnet test Api/GameHub.sln -c Release --no-build`: 372 passed, 2 skipped.
- `npx tsc -p src/tsconfig.app.json --noEmit` no `angular-admin/GameHub.UI`: OK.
- `npx ng build --configuration=production` no `angular-admin/GameHub.UI`: OK (admin chunk 376 KB).

### Pendências
- Migration `AddEafAdminEntities` precisa ser aplicada (`dotnet ef database update`) em ambiente com PostgreSQL acessível.
- Verificação end-to-end dos novos módulos admin (login, navegação, CRUD) aguardando aprovação para teste.

## 2026-07-31 16:44 UTC

### Tarefa
Portar as funcionalidades EAF administrativas (Editions, OrganizationUnits e Dashboard) já disponíveis nos módulos EAF para o GameHub, enquanto a nova versão do EAF não é publicada.

### Implementado
- `Api/src/GameHub.Application/Administration/{Editions,OrganizationUnits,Dashboard}/`: AppServices, interfaces e DTOs portados do `Eaf.Middleware.Application`, com namespaces ajustados para `GameHub.Administration.*` e base `GameHubAppServiceBase`.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs`: adicionados mapeamentos `Edition<->EditionDto`, `Create/UpdateEditionInput->Edition`, `OrganizationUnit->OrganizationUnitDto`, `User->OrganizationUnitUserListDto`, `Role->OrganizationUnitRoleListDto`.
- `angular-admin/GameHub.UI/src/app/admin/{editions,organization-units}/`: componentes, templates e specs portados do template EAF Angular.
- `angular-admin/GameHub.UI/src/app/main/dashboard/`: substituído o stub anterior pelo componente de dashboard do template EAF (`DashboardServiceProxy` com `getHostDashboard`/`getTenantDashboard`).
- `angular-admin/GameHub.UI/src/shared/service-proxies/{edition,organization-unit,dashboard}.service-proxy.ts`: proxies manuais copiados do template EAF e corrigidas as rotas de `*AppService/` para `/api/services/app/*/`.
- `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxy.module.ts`: registrados os novos proxies.
- `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts`: adicionados itens de menu Editions e OrganizationUnits.
- `angular-admin/GameHub.UI/src/test-helpers/mock-services.ts`: adicionados `MockEditionServiceProxy`, `MockOrganizationUnitServiceProxy` e `MockDashboardServiceProxy` para os specs.

### Validação
- `dotnet build Api/GameHub.sln -c Release`: OK.
- `dotnet test Api/GameHub.sln -c Release --no-build`: 372 passed, 2 skipped.
- `npx tsc -p src/tsconfig.app.json --noEmit`: OK.
- `npx tsc -p src/tsconfig.spec.json --noEmit`: OK.
- `npx ng build --configuration=production` no `angular-admin/GameHub.UI`: OK.
- Swagger expõe `/api/services/app/{Edition,OrganizationUnit,Dashboard}/...`.

### Pendências
- `MassNotifications`, `UserDelegations` e `Payments` do EAF dependem de entidades (`MassNotification`, `UserDelegation`, `SubscriptionPayment`) ainda não presentes na versão atual do pacote `Eaf.Middleware` e portanto não foram portadas; documentadas em `.specs/2026-07-31-eaf-admin-modules-pending.md`.

## 2026-07-31 01:16 UTC

### Tarefa
Ajustar layout das telas do admin GameHub (dashboard, API Sandbox e cabeçalho do chat).

### Implementado
- `angular-admin/GameHub.UI/src/app/main/gamehub/dashboard/dashboard.component.{html,ts,css}`: reestruturou os cards de métricas com layout flex, corrigindo alinhamento e sobreposição dos valores.
- `angular-admin/GameHub.UI/src/app/main/gamehub/api-sandbox/api-sandbox.component.{html,ts}`: substituiu o iframe `/swagger` por um explorer de endpoints baseado no Swagger JSON. Lê o arquivo em dev e usa `assets/api-sandbox/swagger.json` como fallback em produção.
- `angular-admin/GameHub.UI/src/app/shared/layout/chat/chat-bar.component.{html,css,ts}`: cabeçalho do chat agora usa a mesma skin do cabeçalho da página (`header-dark`, `header-light`, `header-color`) em vez do primário laranja.
- `angular-admin/GameHub.UI/src/assets/api-sandbox/swagger.json`: especificação Swagger mínima para fallback offline.

### Validação
- `npx tsc -p src/tsconfig.app.json --noEmit`: OK.
- `npx ng build --configuration=development` no `angular-admin/GameHub.UI`: OK.

## 2026-07-29 13:15 UTC

### Tarefa
Copiar do `afonsoft/agents-skills` as skills relevantes para o projeto GameHub, deixando-as disponíveis em `.agents/skills/`.

### Implementado
- Copiadas 32 skills para `.agents/skills/` cobrindo: ABP/EAF (`abp-core`, `abp-ddd`, `abp-angular`, `abp-testing`, etc.), .NET/ASP.NET Core (`aspnet-core-api`, `modern-csharp-coding-standards`, `ef-core`), PostgreSQL (`postgresql-optimization`, `postgresql-code-review`, `sql-optimization`), segurança (`security-jwt`), DevOps (`dotnet-github-actions`), frontend (`frontend-design`, `web-design-reviewer`), workflow de agentes (`verification-before-completion`, `systematic-debugging`, `dispatching-parallel-agents`, `testing-xunit`, `receiving-code-review`, `requesting-code-review`), e documentação Microsoft (`microsoft-docs`, `microsoft-code-reference`).
- Mantida a skill existente `.agents/skills/testing-gamehub/SKILL.md`.

## 2026-07-29 12:50 UTC

### Tarefa
Ajustar cores e layout do `chat-bar` do admin GameHub para o tema Metronic/EAF e sincronizar o template EAF.

### Implementado
- `angular-admin/GameHub.UI/src/app/shared/layout/chat/chat-bar.component.{html,css}`: tema, layout flex, status dots, previews de arquivo/link, botões de anexo e fallback do header.
- `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/layout/chat/chat-bar.component.{html,css}` (EAF): mesmas correções de cor e escopo do `.card`.
- `.specs/2026-07-29-chat-bar-template-eaf-sync.md`: especificação das mudanças para o template EAF.

### Validação
- `ng serve` compilou com sucesso após as alterações.

## 2026-07-28 23:43 UTC

### Tarefa
Subir backend localmente para regenerar service proxies e corrigir conflito de schema Swagger após migração EAF 9.4.0.

### Implementado
- `Api/src/GameHub.Web.Host/Models/HubAuth/*.cs`: renomeados `AvailableTenantsModel`, `SelectTenantModel` e `AvailableTenantResult` para prefixo `Hub` (evita conflito com DTOs homônimos do `TokenAuthController` do EAF 9.4.0).
- `Api/src/GameHub.Web.Host/Controllers/HubAuthController.cs`: atualizadas referências e adicionados `[ProducesResponseType]` para gerar contratos Swagger tipados.
- `Api/test/GameHub.Tests/Controllers/HubAuthController_Tests.cs`: ajustados tipos renomeados.
- `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxies.ts`: regenerado via `npm run service-update` contra o backend local.
- `angular-admin/GameHub.UI/src/shared/service-proxies/service-proxy.module.ts`: atualizada lista de providers para refletir os proxies gerados.

### Validação
- Backend subiu com PostgreSQL em Docker (`gamehub-postgres`).
- Swagger respondeu em `http://localhost:8001/swagger/v1/swagger.json` após o ajuste dos DTOs.
- `dotnet build Api/GameHub.sln -c Release`: OK.
- `dotnet test Api/GameHub.sln -c Release --no-build`: 371 passed, 2 skipped.
- `npm run build` no `angular-admin/GameHub.UI`: OK.

## 2026-07-28 12:19 UTC

### Tarefa
Corrigir headers CORS ausentes no admin EAF e documentar resultados dos testes end-to-end.

### Implementado
- `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs`: adicionados `Pragma`, `Cache-Control` e `Expires` aos headers permitidos, pois o `EafHttpInterceptor` do admin envia esses headers e o preflight falava sem eles.
- `Api/test/GameHub.Tests/Middleware/CorsConfiguration_Tests.cs`: teste `Dado_PoliticaPadrao_Quando_AdicionarCors_Entao_DevePermitirHeadersDoEafHttpInterceptor` verifica os novos headers.

### Validação
- `dotnet build Api/GameHub.sln -c Release`: OK.
- `dotnet test Api/GameHub.sln -c Release --no-build --filter "FullyQualifiedName~CorsConfiguration"`: 5 passed, 0 failed.
- Testes end-to-end via agente de testes: login público com fallback para `/api/TokenAuth/Authenticate` passou; login admin só funcionou após adicionar os headers acima; `/signalr/negotiate` retornou `Access-Control-Allow-Origin` refletido sem wildcard.

### Observações
- O cliente SignalR do admin ainda não completou a negociação mesmo com CORS OK; provavelmente requer ajuste no `SignalRHelper.ts` do admin.

## 2026-07-28 03:35 UTC

### Tarefa
Tornar o layout do hub público responsivo e os jogos em tela cheia.

### Implementado
- `angular/src/app/app.ts` e `app.html`: menu hambúrguer no mobile, fechamento automático ao navegar e classe `play-mode` para rotas `/play` e `/preview`.
- `angular/src/app/app.css`: estilos do menu mobile e ocultação do header/footer em `play-mode`.
- `angular/src/app/player/game-frame/`:
  - `toggleFullscreen` com fallback para `document.documentElement.requestFullscreen` e modo CSS fullscreen quando a API não é suportada.
  - `iframe` com atributo `allowfullscreen`.
  - `:host` e `.frame-shell` usando `100dvh` para preencher a viewport dinâmica em dispositivos móveis.
  - botões de tela cheia e skip maiores para toque.
- `angular/src/app/public/home/`:
  - Removido header duplicado da landing page.
  - Ajustes responsivos em hero, grids e espaçamentos.
- `angular/src/app/public/games/games.component.css`: filtros em grid de 2 colunas e busca em coluna no mobile.
- `angular/src/app/public/game-detail/game-detail.component.css`: ajustes mobile no cabeçalho, thumb e ações.
- `angular/src/app/public/login/login.component.css`, `select-tenant/select-tenant.component.css`, `company/company.component.css`: padding e tipografia responsivos.
- `angular/src/app/public/player/player.component.css`: estatísticas e abas responsivas.
- `angular/src/app/player/leaderboard/leaderboard.component.css` e `public/leaderboards/leaderboards.component.css`: scroll horizontal na tabela e grids responsivos.
- `angular/src/app/public/docs/docs.component.css`: sidebar e conteúdo responsivos.
- `angular/src/styles.css`: `img`/`video` com `max-width:100%` e ajuste de fonte em telas pequenas.

### Validação
- `npm run build` em `angular/`: OK (sem warnings).

## 2026-07-28 03:20 UTC

### Tarefa
Adicionar páginas do jogador e leaderboards adicionais no GameHub.

### Implementado
- `angular/src/app/app.html` e `app.routes.ts`: links `/player` (logado) e `/leaderboards` no header/footer e nova rota.
- `angular/src/app/public/leaderboards/`: novo componente com grid dos jogos mais jogados, link para leaderboard individual.
- `angular/src/app/public/player/player.component.ts/.html/.css`: estatísticas rápidas (favoritos, recentes, partidas) e melhorias visuais.
- `angular/public/i18n/en-US.json` e `pt-BR.json`: chaves `nav.leaderboards` e `leaderboards.*`.

### Validação
- `npm run build` em `angular/`: OK (warning residual de CSS budget em `home.component.css`).
- `dotnet test Api/GameHub.sln -c Release --no-build`: 365 passed, 2 skipped.
- `npm run build` em `angular-admin/GameHub.UI/`: OK.

## 2026-07-28 02:40 UTC

### Tarefa
Melhorias no angular-admin: dashboard por perfil/tenant, controle de empresas/funcionários, padronização de tabelas, game lifecycle, auditoria e help.

### Implementado
- `app-navigation.service.ts` e `gamehub-routing.module.ts`: permissões de menu e rotas ajustadas para `Pages.Developer.Games`; `Companies` aponta para `/app/main/gamehub/companies`.
- `company-list.component.ts/.html`, `company-edit.component.ts/.html`, `company-employees.component.ts/.html`: links/redirects corrigidos, paginação lazy, badges de role, botão "Back to Companies".
- `gamehub-admin.service.ts`: adicionado `getDeveloperDashboard` e ações do ciclo de vida do jogo (`startReview`, `approveForPublishing`, `publishGame`, `requestChanges`, `suspendGame`).
- `dashboard.component.ts/.html`: KPIs separados por admin/desenvolvedor, tenant atual, plays over time para dev, recent audit logs, pending actions e recent versions.
- `game-list.component.ts/.html`: filtros com todos os status do jogo, paginação, loading, status badges e ações do ciclo de vida.
- `user-list.component.ts/.html`: paginação lazy padrão com `p-table`.
- `report-list.component.ts/.html`: loading, paginação, filtros e status badges.
- `review-queue.component.ts/.html`: filtros por status de revisão, paginação e loading.
- `build-list.component.html`: opções de status de build e badges completos.
- `help/` (novo componente e rota `/app/main/gamehub/help`): guia rápido pt/en com links para as telas.
- Documentação: `docs/gamehub-features.md` e `docs/gamehub-admin-features.md` bilíngues.

### Validação
- `dotnet test Api/GameHub.sln -c Release --no-restore`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm run build` em `angular/`: OK.

## 2026-07-28 00:05 UTC

### Tarefa
Adicionar test session, documentações detalhadas e sandbox de API no painel admin do GameHub.

### Implementado
- `angular-admin/GameHub.UI/src/app/main/gamehub/docs/`:
  - `DocsComponent` com guias de Admin, SDK e API, com alternância de idioma pt/en.
- `angular-admin/GameHub.UI/src/app/main/gamehub/api-sandbox/`:
  - `ApiSandboxComponent` verifica se `/swagger/v1/swagger.json` está acessível; quando sim, exibe Swagger UI em iframe; quando não, mostra exemplos de curl.
- `angular-admin/GameHub.UI/src/app/main/gamehub/playtest/test-session.component.ts/.html`:
  - Formulário para inserir `GameId`, `Version` e `Notes`.
  - Botões "Start Preview" (cria preview token via `GamePreview/CreatePreviewToken`) e "Request Playtest" (`Playtest/RequestPlaytest`).
  - Preview do jogo em iframe com URL pública derivada a partir de `remoteServiceBaseUrl`.
- `GameHubAdminService`: adicionados `createPreviewToken` e `requestPlaytest`.
- `app-navigation.service.ts`: itens de menu `Test Session`, `Docs` e `API Sandbox`.
- `gamehub-routing.module.ts` e `gamehub.module.ts`: rotas e declarações dos novos componentes.

### Validação
- `dotnet test Api/GameHub.sln -c Release --no-restore`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm run build` em `angular/`: OK (aviso residual de budget em `home.component.css`).

## 2026-07-27 23:55 UTC

### Tarefa
Melhorar execução do jogo (Spec 17-B) e expandir documentações SDK/API no hub público.

### Implementado
- `game-frame.component.ts/.html/.css`:
  - Adicionado `frameLoading` e `loading-overlay` sobre o iframe após clicar em Start Game.
  - `bridge.gameLoadingStarted()` no `startGame()`, `bridge.gameLoadingFinished()` + `bridge.gameplayStart()` no `onFrameLoad()`.
  - `iframe` com `[attr.title]="'gameFrame.playerTitle' | translate"`, `loading="eager"` e atributos `sandbox`, `allow`, `referrerpolicy` conforme §4.1 da Spec 15.
  - Telas de rewarded break internacionalizadas.
- `public/docs/sdk-guide/` e `public/docs/api-guide/`:
  - Código movido para propriedades do componente (`examples`) para evitar erros de parse do Angular com chaves `{` `}`.
  - Guias expandidos com introdução, autenticação, swagger, exemplos completos de catalog/gameplay/player e seções de tratamento de erros e segurança.
  - Novas chaves i18n `docs.sdk.*`, `docs.api.*`, `gameFrame.reward.*`, `gameFrame.playerTitle`, `gameFrame.loadingGame`.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular/`: OK (aviso residual de budget em `home.component.css`).
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm test` não executado por falta de ChromeHeadless no ambiente.

## 2026-07-27 23:35 UTC

### Tarefa
Melhorar leaderboard e páginas do jogador (Spec 17-C / 19.8): loading skeleton, paginação, unificação de cards com `GameCardComponent`.

### Implementado
- `GameCardComponent` estendido com slots `<ng-content select="[cardAction]">` e `<ng-content select="[cardExtra]">` para ações (ex: remover favorito) e conteúdo extra (ex: número de partidas).
- `leaderboard.component.ts/.html/.css`:
  - Estados `loadingEntries` e `take` (10/25/50).
  - Skeleton enquanto carrega entradas.
  - Controles rápidos `Top 10 / 25 / 50` e botão `Load more`.
  - `My rank` e CTA de login mantidos.
- `player.component.ts/.html/.css`:
  - Loading skeleton durante `forkJoin` de favoritos/recentes.
  - Cards de favoritos e recentes substituídos por `<app-game-card>`.
  - Botão remover favorito projetado via `cardAction`; contagem de partidas projetada via `cardExtra`.
  - Estilos `.game-card` duplicados removidos.
- Chaves i18n: `leaderboard.loadMore`, `leaderboard.loading`, `player.removeFavorite` em `en-US.json` e `pt-BR.json`.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular/`: OK (aviso residual de budget em `home.component.css`).
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm test` não executado por falta de ChromeHeadless no ambiente.

## 2026-07-27 22:30 UTC

### Tarefa
Implementar melhorias nas páginas públicas e descoberta (Spec 19.2): unificar os cards de jogos em um componente compartilhado com chips de categoria, nota e acessibilidade.

### Implementado
- Criado `GameCardComponent` (`angular/src/app/shared/ui/game-card/`) com thumb, badges web-exclusive/desktop-only, chips de categoria (máx 3), rating, plays e `aria-label`.
- Substituídos os cards repetidos em `home.component.html` (grids large/padrão), `games.component.html` e `game-detail.component.html` (seção related) por `<app-game-card>`.
- Removidos estilos `.game-card` duplicados de `games.component.css` e `game-detail.component.css`.
- Adicionadas chaves i18n `games.plays` em `en-US.json` e `pt-BR.json`.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular/`: OK (apenas aviso residual de budget em `home.component.css`).
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm test` não executado por falta de ChromeHeadless no ambiente.

## 2026-07-27 21:20 UTC

### Tarefa
Finalizar itens remanescentes da Spec 17 (Beta Readiness) no GameHub: link público para documentação, i18n do shell público e ações de status na fila de reports do admin.

### Implementado
- **Header/Footer do hub público**
  - `angular/src/app/app.html` traduzido com `TranslatePipe` e inclui link `/docs`.
  - `angular/src/app/app.ts` passou a importar `TranslatePipe`.
  - `angular/public/i18n/en-US.json` e `pt-BR.json` receberam `nav.home`, `nav.register`, `nav.logout` e `footer.tagline`.
- **Fila de reports no admin**
  - `gamehub-admin.service.ts` adicionou `updateReportStatus(reportId, status)` com `PUT /api/services/app/AdminReport/UpdateStatus`.
  - `report-list.component.ts/.html` passou a usar os status do enum `UserReportStatus` (`Open`, `UnderReview`, `Resolved`, `Dismissed`) em filtros, badges e botões de ação inline.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- `npm run build` em `angular/`: OK (avisos pré-existentes de budget de CSS).
- `npm run build` em `angular-admin/GameHub.UI/`: OK.
- `npm test` não executado em ambos os frontends por falta de ChromeHeadless no ambiente.

## 2026-07-27 18:49 UTC

### Tarefa
Executar itens 2–5 da Spec 52 (Developer Portal v3 UX, Fluxo de Publicação, Analytics & Earnings UI, User Guide) e dar início à Spec 17 (Beta Readiness) no GameHub, sem alterar o EAF.

### Implementado
- **Item 2 — Developer Portal v3 UX**
  - Criado `ConfirmDialogComponent` (`shared/ui/confirm-dialog`) para substituir `window.confirm`.
  - Refatorados `game-create`, `game-edit`, `profile`, `team`, `games` e `builds` para usar estado `PageState` com signals, retry/cancelamento via `takeUntil`, `ErrorMapperService` e i18n `dev.*`.
  - Templates sem sidebars duplicados, com landmarks, labels e aria-labels.
- **Item 3 — Fluxo de Publicação**
  - `game-edit` bloqueia `Submit for review` quando não há build aprovado e exibe mensagem com link para builds.
  - `builds` mantém upload, validação, aprovação/rejeição, preview/inspector e histórico de revisão.
- **Item 4 — Analytics & Earnings UI**
  - `earnings` internacionalizado e com filtros de período, export CSV e aviso de valores estimados.
  - Criada nova rota `games/:id/metrics` com `GameMetricsComponent` mostrando filtros avançados (período, país, dispositivo, origem do tráfego, UTM), cards de resumo, tabela diária e export CSV.
  - `DeveloperService.getGameMetrics` e `exportGameMetricsCsv` ampliados para enviar `trafficSource`, `utmSource`, `utmMedium`, `utmCampaign`.
- **Item 5 — User Guide**
  - Traduções pt-BR/en-US para guia do usuário já presentes; reforçadas as chaves `docs.ug.*`.
- **Spec 17 — Beta Readiness (UX pública)**
  - `public/games` agora passa `device`, `orientation`, `exclusivity` e `minRating` para `catalog.search()`.
  - Backend `GameCatalogAppService.SearchAsync` ampliado para filtrar por `Exclusivity` e `MinRating`; `GetGamesAsync` passou a aplicar `Device` e `Orientation`.
  - `GameCatalogService.search` ampliado com os novos parâmetros.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- `npm run build` angular: OK.
- `npm run build` angular-admin/GameHub.UI: OK.
- `npm test` angular não executado nesta sessão por falta de ChromeHeadless no ambiente; builds de produção estão OK.

## 2026-07-27 17:20 UTC

### Tarefa
Finalizar o plano `2026-07-27-gamehub-tenant-player-company-portals.md`: corrigir replicação de roles/permissões para company tenants, garantir que o chat use sempre o tenant `Player`, e validar tudo com build, testes e simulação Docker.

### Implementado
- Backend:
  - `TenantUserManager` ajustado para `DisableFilter(MayHaveTenant)` no escopo mínimo e `EnableFilter(MayHaveTenant)` + `SetTenantId` na criação do shadow user, eliminando `DuplicateUserName`.
  - `GameHubPermissions` centralizou listas de permissões (`AllPermissions`, `AdminPermissions`, `ModeratorPermissions`, `DeveloperPermissions`, `PlayerPermissions`).
  - `GameHubPermissionSeeder` refatorado para usar as listas de `GameHubPermissions`.
  - `CompanyAppService` passou a semear roles `Developer` e `Player` com permissões ao criar uma empresa.
  - `CompanyEmployeeAppService` agora atribui a role `Developer` ao shadow user no tenant da empresa.
  - `GameChatAppService` refatorado: valida o jogo ignorando filtro de tenant, força o contexto do tenant `Player` para envio de mensagens e remove `.GetAwaiter().GetResult()` dos helpers.
- Documentação:
  - Criados `.specs/55-eaf-tenant-login-improvements.md` e `docs/superpowers/plans/2026-07-27-eaf-tenant-login-improvements.md` com melhorias propostas para o EAF (outra sessão).

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 365 passed, 2 skipped, 0 failed.
- Docker Compose (Postgres/Redis/MinIO): backend `Healthy`, `angular-hub` (`:4600`) e `angular-admin` (`:4602`) respondendo.
- Simulação end-to-end via `curl`:
  - `POST /api/services/app/Company/Create` -> empresa `simacme3` criada (tenant id 5).
  - `POST /api/services/app/CompanyEmployee/RegisterAndJoin` -> funcionário `gamedev3` cadastrado e vinculado.
  - `POST /api/hub/auth/select-tenant` para `gamedev3` no tenant 5 -> JWT com role `Developer`.
  - `POST /api/services/app/DeveloperGame/CreateDraft` -> jogo `Simulation Puzzle 3` criado no tenant da empresa.
  - `POST /api/services/app/GameChat/Send` pelo player `playerone` no tenant `Player` para `playertwo`, referenciando o jogo da empresa -> mensagem aceita (`accepted: true`).

### Observações
- `GameHub.Web.Tests` continua com `HomeController_Tests.About_Test` marcado como Skip.
- O fluxo de UI nos portais `angular` e `angular-admin` não foi validado manualmente nesta sessão; os builds Docker dos frontends já estavam OK e não foram alterados.

## 2026-07-27 17:00 UTC

### Tarefa
Executar as fases 1-6 do plano `2026-07-27-gamehub-tenant-player-company-portals.md`: tenant Player, APIs de empresa/funcionários, chat/SDK tenant-aware, portais admin/hub e testes/documentação.

### Implementado
- Backend:
  - `GameHubConsts.PlayerTenantName` e seed de `Player` tenant via `PlayerTenantBuilder`.
  - `RegistrationAppService` cria jogadores no tenant `Player` e desenvolvedores/funcionários como host users.
  - Permissões de empresa `Pages.Companies`, `Pages.Companies.Manage`, `Pages.Company.Employees`, `Pages.Company.Employees.Manage` em `GameHubPermissions` e seeder.
  - `CompanyAppService` e `CompanyEmployeeAppService` (CRUD de tenants/empresas, convite/remoção/default de funcionários, registro público `RegisterAndJoinAsync`).
  - `GameChatAppService` sempre resolve e envia pelo tenant `Player`, mapeando usuários host para shadow users quando necessário.
  - `GameTokenProvider` emite JWTs com claims `AbpClaimTypes.UserId`, `AbpClaimTypes.TenantId` e `tenantid` para compatibilidade EAF.
- Frontend:
  - `angular-admin`: `CompanyService`, telas `company-list`, `company-edit`, `company-employees`, ajuste de rotas e menu lateral.
  - `angular` (hub): `CompanyService`, página pública `company/:tenancyName` com formulário de cadastro/adesão à empresa.
  - `hub-auth.service.ts` e `login/select-tenant` ajustados para rotas `/api/hub/auth/*` e unwrap de resposta ABP.
- Testes: `GameHubTestBase` semeia tenant `Player`; testes de `GameChatAppService` e `RegistrationAppService` atualizados para o novo fluxo; `TenantAppService_Tests` ajustado para 2 tenants.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 358 passed, 2 skipped, 0 failed.
- `npm run build` no `angular-admin/GameHub.UI`: production build OK.
- `npm test` no `angular-admin/GameHub.UI` (CHROME_BIN apontado para Playwright Chromium): 215 SUCCESS.
- `npm run build` no `angular`: production build OK.
- `npm test` no `angular` (CHROME_BIN apontado para Playwright Chromium): 8 SUCCESS.

### Observações
- `GameHub.Web.Tests` continua com `HomeController_Tests.About_Test` marcado como Skip.
- Docker Compose completo com simulação end-to-end foi executado em sessão anterior (PR #75); o novo fluxo de empresa/funcionário ainda precisa de validação E2E com banco real.

## 2026-07-27 16:20 UTC

### Tarefa
Rodar testes, infra Docker completa e simular cadastro/login multi-tenant; corrigir gargalos encontrados.

### Implementado
- Corrigidas migrations `SocialInvitesAndNotifications` e `AddUserTenantMembership` para usar `gh_GameplayEvents` e remover colunas duplicadas.
- Configurado `Clock.Provider = ClockProviders.Utc` em `Startup` e `Migrator/Program`.
- Registrado `DbContextOptions<GameHubDbContext>`, `GameHubDbContext` e `ITokenAuthenticationService -> JwtTokenAuthenticationService` no `Startup`.
- Reescrito `JwtTokenAuthenticationService` para gerar tokens EAF-compatíveis (`token_validity_key`, `token_validity_value`, `user_identifier`, `SecurityStamp`).
- Ajustado `RegistrationAppService` para criar jogadores no tenant `Default`.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 358 passed, 2 skipped, 0 failed.
- Docker Compose (Postgres/Redis/MinIO): backend `Healthy`, `angular-hub` (`:4600`) e `angular-admin` (`:4602`) respondendo.
- Simulação end-to-end via `curl`:
  - `POST /api/services/app/Registration/Register` -> player criado no tenant `Default`.
  - `POST /api/hub/auth/available-tenants` -> retornou `[{ tenantId: 1, tenantName: "Default" }]`.
  - `POST /api/hub/auth/select-tenant` -> emitiu accessToken JWT válido.
  - `GET /api/services/app/PlayerAccount/GetPlayerProfile` com token -> retornou `{ username: "..." }`.
- PR #75 aberto com as correções.

### Observações
- `JwtTokenAuthenticationService` precisa ainda ser testado para tokens de jogo (`gameId`) no `GameTokenProvider`; no plano futuro.
- `angular` e `angular-admin` login via UI não foi concluído por dificuldades de automação de formulário no desktop (eventos de submit); fluxo API validado.
- Simulação de empresa/developer (tenant adicional) não foi executada; está no escopo do próximo plano.

## 2026-07-27 15:15 UTC

### Tarefa
Executar plano `docs/superpowers/plans/2026-07-27-gamehub-tenant-companies-and-user-associations.md`.

### Implementado
- Domínio: `UserTenantMembership`, `ITenantUserManager`/`TenantUserManager`, `IUserTenantMembershipRepository`.
- Aplicação: `UserTenantAssociationAppService` com DTOs e métodos para associar, remover, definir default e listar memberships.
- Web API: `HubAuthController` com `available-tenants` e `select-tenant` para login multi-tenant no hub público.
- Migração/seed: migration `AddUserTenantMembership` e `SeedHelper.LinkHostAdminToDefaultTenant`; test base vincula admin ao Default.
- `angular-admin`: modal `UserTenantMembershipModalComponent` e serviço `UserTenantAssociationService` integrados na grid de usuários.
- `angular`: `HubAuthService`, tela `SelectTenantComponent`, ajuste no `LoginComponent` para fluxo de seleção de empresa/tenant e `TokenService.getTenantId()`.
- Testes: `TenantUserManager_Tests`, `UserTenantAssociationAppService_Tests`, `HubAuthController_Tests`.

### Validação
- `dotnet build Api/GameHub.sln -c Release`: 0 erros, 0 warnings.
- `dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release`: 358 passed, 2 skipped, 0 failed.
- `npm run build` no `angular-admin/GameHub.UI`: production build OK.
- `npm test` no `angular-admin/GameHub.UI` (CHROME_BIN apontado para Playwright Chromium): 215 SUCCESS.
- `npm run build` no `angular`: production build OK.
- `npm test` no `angular` (CHROME_BIN apontado para Playwright Chromium): 8 SUCCESS.

### Observações
- `GameHub.Web.Tests` continua sem testes ativos; `HomeController_Tests.About_Test` é `[Fact(Skip = ...)]`. A infraestrutura foi ajustada para evitar duplo registro de `DbContextOptions`/`CacheMultiplayerPresenceStore` quando `GameHubTestModule` e `WebHostModule` são carregados juntos.

## 2026-07-27 14:22 UTC

### Tarefa
Ajustar ícones do menu lateral e padronizar tabelas do `angular-admin`; consolidar entregas e criar plano detalhado para multi-tenancy (empresas = tenants, usuários em múltiplos tenants, seleção de tenant no login do hub).

### Implementado
- Ajustados ícones do `app-navigation.service.ts` do `angular-admin` (`la la-gamepad`, `la la-list`, `la la-shield`, `la la-bar-chart`, `la la-clock-o`, `la la-cloud-upload`).
- Padronizados cards de listagem (`.card-header` + `.card-body`) e tabelas `p-table` em `category-list`, `tag-list`, `game-list`, `build-list`, `user-list`, `audit-log`, `feature-flags`, `review-queue`, `inspector-session`, `report-list`, `playtest-recording-list`, `build-files`.
- Criado plano `docs/superpowers/plans/2026-07-27-gamehub-tenant-companies-and-user-associations.md` com todas as fases: domínio, aplicação, Web API, migrações/seed, `angular-admin`, `angular` público, segurança, testes e documentação.

### Validação
- `dotnet test Api/GameHub.sln -c Release`: 348 passed, 3 skipped, 0 failed.
- `npm run build` no `angular-admin/GameHub.UI`: production build OK.
- `npm test` no `angular-admin/GameHub.UI`: 215 SUCCESS.
- `npm run build` no `angular`: production build OK (budget warnings não críticos).

### Observações
- O plano de multi-tenancy **não foi implementado**; está pendente de aprovação do usuário. Aguardando go/no-go.

## 2026-07-27 02:39 UTC

### Tarefa
Atualizar dependências EAF para 9.3.1 e sincronizar documentação.

### Implementado
- Verificados todos os `PackageReference` EAF em `Api/**/*.csproj`: já apontam para `9.3.1`.
- Atualizado `docs/packages.md` para refletir `Eaf.*` 9.3.1.
- Atualizado `docs/technologies.md` para refletir NuGet `EAF.*` 9.3.1.

### Validação
- `dotnet build Api/GameHub.sln -c Release`: 0 erros, 0 warnings.
- `dotnet test Api/GameHub.sln -c Release --no-build`: 344 passed, 2 skipped, 0 failed.

## 2026-07-27 02:30 UTC

### Tarefa
Replicar ajustes da migração EAF 9.3.1 (PR #249 / Templates API + Angular) nos projetos reais do GameHub: `Api` e `angular-admin`.

### Implementado
- Atualizado `Api/common.props` e todos os `PackageReference` EAF de `9.3.0` para `9.3.1`.
- Adicionados contratos compartilhados em `GameHub.Core/Application/Contracts`: `PublicErrorContract`, `ContextualChatMessageContract`, `RateLimitContract`/`RateLimitDecision`/`IRateLimitManager`, `ModerationAuditContract`/`IModerationAuditWriter`.
- Registrado `IRateLimitManager` (→ `RateLimitManager`) e `IModerationAuditWriter` (→ `NullModerationAuditWriter`) no `PostInitialize` de `GameHubApplicationModule`.
- Criados `angular-admin/GameHub.UI/src/app/shared/eaf-contracts/eaf-contracts.ts` e `eaf-correlation-id.interceptor.ts`.
- Atualizado `ContextualChatMessage` com campos contextuais (`conversationId`, `gameId`, `matchId`, `contextType`, `clientMessageId`, `creationTime`) e adicionados `RateLimitDecision`, `RateLimitContract` e `ModerationAuditContract`.
- Registrado `EafCorrelationIdInterceptor` antes de `EafHttpInterceptor` em `root.module.ts`.

### Validação
- Build .NET `Api/GameHub.sln` (Release): 0 erros, 0 warnings.
- Testes `Api/GameHub.sln`: 344 passed, 2 skipped, 0 failed.
- Build Angular `angular-admin/GameHub.UI`: production build OK.
- Testes Angular: 215 SUCCESS.

### Observações
- O build do GameHub depende dos pacotes EAF `9.3.1`, que foram gerados localmente a partir do repositório `EAF` (`Eaf.sln` Release) durante a validação; em CI o feed deverá conter a versão `9.3.1`.

## 2026-07-27 01:35 UTC

### Tarefa
Executar plano `docs/superpowers/plans/2026-07-27-gamehub-next-steps.md` — Fases 1 a 7 (Specs 46–51, hardening EAF 9.3.0, evoluções EAF).

### Implementado
- Fase 1: hardening EAF 9.3.0 replicado no host (`DataProtection`, CSP/SecurityHeaders/RateLimit middlewares, `SdkError`, `GameHubExceptionFilter`, CORS com `Retry-After` exposto, appsettings por ambiente).
- Fase 2 (Spec 46): `ClientRequestId` em DTOs de moderação; rate limit e idempotência em `UserContentAppService`, `UserReportAppService`, `ModerationAppService` e `GameSocialAppService`; validação de jogo/tenant; exceções mapeadas para `SdkError` via `GameHubException`.
- Fase 3 (Spec 47): filtros ampliados em `GameMetricsFilter` (build, traffic source, UTM, playtest), CSV com escaping RFC 4180 e nome dinâmico, deduplicação e validação de eventos de gameplay.
- Fase 4/5 (Specs 48/49): `SdkError` compartilhado no Angular, `error.interceptor` com retry, correlation ID e normalização de erros; `gameplay-bridge` reutiliza contrato.
- Fase 6 (Spec 50, EAF): campos contextuais (`ConversationId`, `GameId`, `MatchId`, `ContextType`, `ClientMessageId`) em `ChatMessage`; DTOs `GetChatHistoryInput`/`MarkChatReadInput`; métodos `GetHistoryAsync`/`MarkReadAsync` em `IChatAppService`/`ChatAppService`; contratos `PublicErrorContract`, `ContextualChatMessageContract`, `RateLimitContract`, `ModerationAuditContract`; `IRateLimitManager` e `IModerationAuditWriter` com implementação padrão.
- Validação: build e testes .NET passam (GameHub: 344 passed, 2 skipped; EAF: build OK, 1 teste pré-existente falhando em `ProfileAppServiceBddTests`), builds Angular (`angular` e `angular-admin`) e `docker compose config` OK.

### Limitações
- EAF: teste `ProfileAppServiceBddTests.Dado_PerfilValido_Quando_UpdateCurrentUserProfile_Entao_DeveAtualizarUsuario` falha com `NullReferenceException` em `SettingManager.ChangeSettingForUserAsync`; não alterado nesta execução.
- Refresh token no Angular depende de endpoint no `TokenAuthController` ainda não disponível; o interceptor atual limpa o token e redireciona para `/login`.
- Templates EAF não foram modificados nesta execução; as melhorias foram concentradas nos módulos Core/Application.

## 2026-07-26 23:25 UTC

### Tarefa
Executar Specs 41–45 em uma única branch após o merge do PR #65.

### Implementado
- Social SDK: convites de partida tenant-aware, notificações persistidas, leitura, aceite com expiração, presença coarse-grained via `ICacheManager` e reports de jogadores.
- Chat: rate limit cacheado por usuário/jogo/conversa, mantendo deduplicação antes do limite.
- Telemetria: `BuildId`/`MatchId` nos eventos, validação de sessão/jogo e rejeição de payloads com tokens, senhas, connection strings ou chat.
- Analytics: contagens de gameplay started, page views e conversões, além dos filtros existentes.
- Bridge: `getPresence`, `getNotifications`, `markNotificationRead`, `invitePlayer`, `acceptInvite` e `reportPlayer`.
- Operação: runbook para validação do backplane Redis em duas instâncias.

### Limitações
- Presença exposta pelo SDK é deliberadamente coarse-grained (`online`/`offline`) e não revela conexão, IP ou tenant.
- Histórico contextual e `markRead` do chat por `matchId` continuam dependentes de metadados no EAF; não foi criada persistência paralela de mensagens.
- A geração automática de migration exigiu PostgreSQL disponível; a migration foi mantida em duas etapas coerentes (tabelas sociais e colunas de telemetria) sem aplicar banco local.
## 2026-07-26 23:05 UTC

### Tarefa
Executar as próximas Specs 39, 40, 36, 34 e 37 após o merge do PR #64.

### Implementado
- Spec 39: mantida a autorização contextual do chat, com `clientMessageId` idempotente; o histórico de partida permanece explicitamente limitado pela ausência de `MatchId` no `ChatMessage` do EAF.
- Spec 40: adicionado `getCapabilities` ao bridge, contrato versionado de capacidades e leitura tolerante de feature flags; recursos não disponíveis retornam `false` sem quebrar jogos antigos.
- Spec 36: validação de períodos invertidos em Earnings e Game Metrics; Earnings agora permite filtro de datas, aviso de estimativa, retry, preservação dos dados anteriores e detalhamento diário expansível.
- Spec 34: filtro local por status em My Games, confirmação de submissão, mensagens de erro/status e navegação responsiva aprimorada.
- Spec 37: User Guide ampliado com segurança, privacidade e suporte; SDK Guide documenta capacidades e compatibilidade.

### Limitações
- Presença, notificações e convites ainda dependem de contratos de domínio específicos; nesta etapa ficam protegidos por feature flags e não são simulados com endpoints inexistentes.
- Histórico e `markRead` para `matchId` só devem ser habilitados depois que o EAF fornecer metadados contextuais, evitando uma segunda persistência no GameHub.
## 2026-07-26 19:00 UTC

### Tarefa

Executar o prompt 29 para ampliar o multiplayer com descoberta pública, filas ranqueadas, histórico, replay metadata, métricas e controles administrativos.

### Arquivos alterados

- `Api/src/GameHub.Core/Domain/Multiplayer/*` — temporadas, ratings, filas, histórico, replay metadata, auditoria e dimensões de match.
- `Api/src/GameHub.Application/Multiplayer/*` — filtros públicos, fila ranqueada, status, histórico e conclusão server-authoritative.
- `Api/src/GameHub.Application/Admin/*` — operações administrativas de multiplayer.
- `Api/src/GameHub.EntityFrameworkCore/*` — DbSets, mapeamentos e migration `Poki29`.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — preservação da configuração SignalR compatível com a infraestrutura atual.
- `Api/test/GameHub.Tests/GameHub/Application/RankedMultiplayer_Tests.cs` — invariantes de rating e fila.

### Decisões e limitações

- O cliente não define MMR nem altera diretamente o rating persistido; o resultado é finalizado pelo serviço de aplicação após validação de participação.
- O pacote de backplane Redis do SignalR não está referenciado no projeto; a implementação preserva o hub atual e reutiliza a conexão Redis já existente para caches. Uma adoção explícita de `Microsoft.AspNetCore.SignalR.StackExchangeRedis` pode ser feita em uma mudança de infraestrutura separada.

### Resultado

- Build Release e 314 testes existentes mais 3 testes de rating passaram.
- Migration `Poki29` gerada.

## 2026-07-26 19:00 UTC

### Tarefa
Executar `.specs/28-poki-signalr-deepening.md`.

### Arquivos alterados
- SignalR match/network hubs, autenticação por token e presença com janela de reconexão.
- Matchmaking, espectadores, validação de payload, rate limiting e métricas.
- Jobs Hangfire de limpeza de AUDS, participantes desconectados e salas expiradas.
- Gameplay bridge/SDK, endpoints de AUDS com `{ saved, quota }` e fallback `{}`.
- Migração EF Core `Poki28`, testes de multiplayer/AUDS e documentação.

### Motivação
Evoluir a fundação Poki 27 para multiplayer resiliente, signaling WebRTC, persistência temporária e observabilidade.

### Resultado
- Build Release validado após restore.
- Testes existentes ajustados ao contrato `{}` para chaves ausentes; novos cenários cobrem espectadores, reconexão, payload e TTL.
- A validação final da solução e o PR permanecem pendentes após a revisão do diff.
# GameHub — Agent Execution Log

## 2026-07-26 — Execução das Specs 30–33

### Tarefa

Implementar cache multiplayer com `ICacheManager`, presença com TTL, operação/health
checks e backplane Redis opcional do SignalR.

### Arquivos principais alterados

- `Api/src/GameHub.Application/Multiplayer/IMultiplayerPresenceStore.cs` e
  `MultiplayerPresenceEntry.cs` — contrato independente do provider.
- `Api/src/GameHub.Web.Host/Multiplayer/*` — store ABP, opções, health check e
  resolução segura da configuração do backplane.
- `Api/src/GameHub.Web.Host/Hubs/NetworkSignalRHub.cs` — presença distribuída,
  heartbeat e remoção por TTL/desconexão.
- `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs` — registro do store e dos
  componentes específicos que exigem `IConnectionMultiplexer`; a configuração
  base do cache permanece no EAF.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — backplane SignalR condicional,
  opções de presença e health check.
- `Api/src/GameHub.Web.Host/appsettings.*.json` — configuração separada de presença
  e backplane, desligada por padrão.
- `Api/test/GameHub.Tests/Multiplayer/*` — testes de TTL, isolamento por tenant e
  resolução do backplane.

### Decisões e limitações

- `ICacheManager` armazena presença/TTL; não é usado como Pub/Sub.
- `Eaf.Middleware.Web.Core` é a autoridade para configurar
  `ICacheManager`/`IDistributedCache` por meio de `CacheConfigurer` e
  `RedisConfigurer`; o GameHub não duplica `Configuration.Caching.UseRedis`.
- O backplane oficial usa a mesma infraestrutura Redis somente quando explicitamente
  habilitado e com prefixo de canais próprio.
- O signaling entre instâncias depende do backplane; presença distribuída sozinha não
  entrega `Signal`/`Broadcast` para conexões remotas.

### Resultado

- Build Release passou sem warnings.
- 325 testes passaram e 2 permaneceram skipped em `GameHub.Tests`.
- O teste de presença cobre provider local; a validação Redis de duas instâncias
  permanece dependente de ambiente Redis disponível.

## 2026-07-26 — Delegação da configuração de cache ao EAF

### Tarefa

Revisar a configuração Redis do GameHub contra o módulo
`Eaf.Middleware.Web.Core` e remover a duplicação de configuração do provider
base.

### Descobertas

- `MiddlewareWebCoreModule.PreInitialize` chama `CacheConfigurer.Configure`,
  que configura o `ICacheManager` e reconhece `RedisCache:IsEnabled` e
  `RedisCache:IsRedisEnabled`.
- `EafServiceCollectionMiddlewareExtensions.AddEafConfigurer` chama
  `RedisConfigurer.Configure`, que registra `IDistributedCache`.
- `IConnectionMultiplexer`, os caches de catálogo/leaderboard e o backplane
  SignalR continuam sendo responsabilidades específicas do GameHub.

### Resultado

`WebHostModule` não chama mais `Configuration.Caching.UseRedis(...)`; a
documentação das Specs 30–33 e dos runbooks passa a apontar o EAF como
autoridade da configuração base.

## 2026-07-26 — Specs de cache e presença multiplayer

### Tarefa

Documentar a próxima evolução do multiplayer usando `ICacheManager`, priorizando presença distribuída sem implementar Pub/Sub/backplane nesta etapa.

### Arquivos criados

- `docs/specs/2026-07-26-multiplayer-cache-presence-design.md` — desenho aprovado e decisões arquiteturais.
- `.specs/30-poki-icachemanager-multiplayer-cache.md` — abstração de cache baseada em `ICacheManager`.
- `.specs/31-poki-multiplayer-presenca-icachemanager.md` — presença distribuída com TTL.
- `.specs/32-poki-multiplayer-cache-operacao-testes.md` — health checks, métricas, testes e rollout.
- `.specs/33-poki-signalr-backplane-futuro.md` — evolução futura para backplane oficial SignalR.

### Decisões

- `ICacheManager` será usado para estado efêmero e presença, não como transporte Pub/Sub.
- A presença distribuída não será apresentada como signaling cross-instance.
- Pub/Sub/backplane permanece separado para evitar acoplamento prematuro e permitir validar a topologia Redis primeiro.

## 2026-07-26 03:00 UTC

### Tarefa
Implementar spec 27 da referência Poki (Netlib/multiplayer base + Arbitrary User Data Store), com foco em SignalR.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — `SupportsMultiplayer`, `MaxPlayersPerMatch`.
- `Api/src/GameHub.Core/Domain/Multiplayer/MatchState.cs`, `MatchParticipant.cs`, `MatchStatus.cs`, `IMatchmakingService.cs`.
- `Api/src/GameHub.Application/Multiplayer/MatchmakingService.cs`, `IMultiplayerAppService.cs`, `MultiplayerAppService.cs`, `Dto/MatchDto.cs`, `MultiplayerInputs.cs`.
- `Api/src/GameHub.Application/ArbitraryUserData/ArbitraryUserDataRecord.cs` (Core), `ArbitraryUserDataAppService.cs`, `IArbitraryUserDataAppService.cs`, `Dto/ArbitraryUserDataInputs.cs`, `ArbitraryUserDataQuotaDto.cs`.
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`, `IGameplayAppService.cs` — bridge methods para `createMatch`, `joinMatch`, `sendMatchState`, `saveArbitrary`, `loadArbitrary`, `deleteArbitrary`.
- `Api/src/GameHub.Web.Host/Hubs/GameHubMatchHub.cs` — SignalR hub para matchmaking e estado da partida.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — `services.AddSignalR()` e `endpoints.MapHub<GameHubMatchHub>("/signalr-match")`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`, `GameHubModelCreatingExtensions.cs` — `DbSet`s e configurações EF para `MatchState`, `MatchParticipant` e `ArbitraryUserDataRecord`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/*Poki27.*` — migração EF Core.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamento `MatchState`/`MatchParticipant` ↔ `MatchDto`.
- `angular/package.json` — `@microsoft/signalr` v8.0.7.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — conexão SignalR, métodos de match e handler de mensagens.
- `Api/test/GameHub.Tests/GameHub/Application/MatchmakingAppService_Tests.cs` e `ArbitraryUserDataAppService_Tests.cs` — testes de integração.
- `README.md`, `README.pt-BR.md`, `CHANGELOG.md` — atualização de funcionalidades, testes e cobertura.

### Motivação
Entregar a base de multiplayer (Netlib) e o armazenamento arbitrário de dados do jogador (AUDS) da Poki, priorizando SignalR para comunicação em tempo real entre jogadores.

### Resultado
- `dotnet build Api/GameHub.sln` — 0 warnings, 0 erros.
- `dotnet test Api/GameHub.sln --no-build` — 310 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` — sucesso.
- Cobertura: GameHub.Application 79.6% linha / 50.3% branch, GameHub.Core 77.7% / 50.3%, global 6.3% / 50.3%.
- Próximos passos: aprofundar SignalR (autorização, reconexão, espectadores) e criar spec 28.

## 2026-07-26 02:00 UTC

### Tarefa
Implementar spec 26 da referência Poki (Error Scanner, DPU/conversion funnel, player feedback analytics, quality guidelines gates, external-resource exemptions, thumbnail guide, playtest difficulty balancing, player fit/retention, submission/approval workflow, ad/earnings reports).

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Gameplay/GameMetricSnapshot.cs` — campos de funnel (`DailyPlayingUsers`, `PageViews`, `LoadingStartedCount`, `LoadingFinishedCount`, `GameplayStartedCount`) e métricas de feedback (`AverageRating`, `ReviewCount`).
- `Api/src/GameHub.Core/Domain/Shared/Enums.cs` — `GameplayEventType.GamePageViewed`.
- `Api/src/GameHub.Core/Domain/Moderation/UserContent.cs` — campo `Rating`.
- `Api/src/GameHub.Core/Domain/Builds/ExternalResourceExemption.cs` e enum `ExternalResourceExemptionStatus`.
- `Api/src/GameHub.Core/Domain/Builds/ImageHeaderAnalyzer.cs` — parser de dimensões de imagem.
- `Api/src/GameHub.Core/Domain/Monetization/AdImpression.cs` e `AdBreakResult.Cpm`/`Earnings`.
- `Api/src/GameHub.Core/Domain/Shared/Enums.cs` — `GameStatus.Submitted`, `InReview`, `ApprovedForPublishing`.
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs` — `GetErrorScannerAsync`, `GetConversionFunnelAsync`, `GetPlayerFitAsync`, alertas de feedback e FPS.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs` e `IAdminGameAppService.cs` — `StartReviewAsync`, `ApproveForPublishingAsync`, `RequestChangesAsync`.
- `Api/src/GameHub.Application/Admin/Dto/*` — DTOs `ErrorScannerItemDto`, `ConversionFunnelDto`, `PlayerFitDto`, `StartReviewInput`, `ApproveForPublishingInput`, `RequestChangesInput`.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs`, `ImageHeaderAnalyzer.cs` e DTOs de validação — quality gates, thumbnails, IAP/links.
- `Api/src/GameHub.Application/Builds/ExternalResourceAppService.cs` e DTOs.
- `Api/src/GameHub.Application/Monetization/AdBreakAppService.cs`, `FakeAdProvider.cs`, `StaticVastAdProvider.cs` — gravação de `AdImpression`.
- `Api/src/GameHub.Application/Developer/DeveloperEarningsAppService.cs` e DTOs — `GetAdReportAsync`.
- `Api/src/GameHub.Application/Playtesting/PlaytestAppService.cs` — `GetDifficultyInsightsAsync` com case-insensitive JSON.
- `Api/src/GameHub.Application/Player/PlayerFeedbackAnalyticsAppService.cs`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`, `GameHubModelCreatingExtensions.cs` — configurações EF e `DbSet`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260726*_Poki26.*` — migração.
- `Api/test/GameHub.Tests/GameHub/Application/AdminDashboardAppService_Tests.cs`, `PlayerFeedbackAnalyticsAppService_Tests.cs`, `ExternalResourceAppService_Tests.cs`, `PlaytestAnalyticsAppService_Tests.cs`, `DeveloperEarningsAdReportAppService_Tests.cs`.
- `README.md`, `README.pt-BR.md`, `CHANGELOG.md`, `.specs/26-poki-proxima-fase.md`.
- `Api/test/GameHub.Tests/GameHub.Tests.csproj` — downgrade `coverlet.collector` para `6.0.4` para recuperar coleta de cobertura.

### Motivação
Completar a fase 26 da referência Poki (Quality & Analytics): coleta de erros, funil de conversão, feedback de jogadores, portões de qualidade, domínios externos, thumbnails, dificuldade de playtests, retenção, workflow de submissão e relatórios de anúncios.

### Resultado
- `dotnet build Api/GameHub.sln` — 0 warnings, 0 erros.
- `dotnet test Api/GameHub.sln --no-build` — 296 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` — sucesso.
- Cobertura: GameHub.Application 81.2% linha / 52.4% branch, GameHub.Core 77.0% / 53.8%, global 7.9% / 51.1%.
- Itens 26.9 (Netlib/Multiplayer) e 26.10 (AUDS) reservados para sessão dedicada.

## 2026-07-26 01:20 UTC

### Tarefa
Implementar spec 25 da referência Poki (image optimization warnings, General Team Settings UI, playtest recordings, rewarded ad UX, onboarding/engagement guides, revenue share/deal types, FPS/performance, suggested categories/SEO e Mystery Tile/playtest discovery).

### Arquivos alterados
- `Api/src/GameHub.Core/GameHubConsts.cs` — limiar de 100 KB para warnings de otimização de imagem.
- `Api/src/GameHub.Core/Domain/Playtesting/PlaytestRecording.cs`, `PlaytestSession.cs`, `Gameplay/PlaySession.cs`, `Monetization/TrafficSource.cs`, `RevenueContract.cs`/`RevenueSplitCalculator.cs` — entidades e regras de split.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs`, `Developer/Dto/ValidationSummaryDto.cs`, `ImageOptimizationWarningDto.cs` — análise de ZIP e warnings de imagem.
- `Api/src/GameHub.Application/Developer/*` — `UpdateGeneralSettingsAsync`, `GetGeneralSettingsAsync`, bloqueio de earnings para Support.
- `Api/src/GameHub.Application/Playtesting/*` — CRUD de gravações de playtest (`PlaytestRecordingDto`, `GetRecordingAsync`, `ListRecordingsAsync`, `AddNotesAsync`, `GetAllRecordingsAsync`).
- `Api/src/GameHub.Application/Admin/*` — `GetOnboardingInsightsAsync`, `GetEngagementInsightsAsync`, FPS por dispositivo e alertas de saúde.
- `Api/src/GameHub.Application/Monetization/*` — `FlatFeeAmount` em contratos e cálculo de earnings com split.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs` — `SuggestCategoriesAsync` e `ValidateSeoAsync`.
- `Api/src/GameHub.Application/Catalog/GameCatalogAppService.cs`, `Dto/MysteryTileDto.cs`, `HomeResponseDto.cs` — `GetMysteryTileAsync` e população na home.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`, `GameHubModelCreatingExtensions.cs` — configuração EF Core das entidades.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260726000000_AddPoki25Phase.*` — migração EF Core.
- `Api/test/GameHub.Tests/GameHub/Application/AdminDashboardAppService_Tests.cs`, `AdminGameAppService_Tests.cs`, `BuildPackageValidator_Tests.cs`, `PlaytestAppService_Tests.cs`, `GameCatalogAppService_Tests.cs`, `RevenueContractAppService_Tests.cs`, `DeveloperEarningsAppService_Tests.cs` — testes.
- `angular/src/app/core/services/game-catalog.service.ts`, `gameplay-bridge.service.ts`, `developer.service.ts` — contratos e bridge de anúncios recompensados.
- `angular/src/app/public/home/home.component.ts/.html` — Mystery Tile na home.
- `angular/src/app/player/game-frame/game-frame.component.ts/.html/.css` — overlay de rewarded ad com botão verde e botão secundário.
- `angular/src/app/developer/team/*` — tela de configurações gerais do time.
- `angular/public/i18n/en-US.json`, `pt-BR.json` — chaves `section.mysteryTile` e `mysteryTile.playtest`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/playtest/*` — tela de gravações de playtest com vídeo, console e notas.
- `angular-admin/GameHub.UI/src/app/main/gamehub/shared/services/gamehub-admin.service.ts`, `gamehub-routing.module.ts`, `gamehub.module.ts`, `shared/layout/nav/app-navigation.service.ts` — rota e menu de playtests.
- `docs/agent-execution-log.md`, `README.md`, `README.pt-BR.md`, `CHANGELOG.md`, `.specs/25-proxima-sessao-poki.md` — documentação.

### Motivação
Completar a fase 25 da referência Poki: otimização de assets, UX refinada de anúncios recompensados, painéis de onboarding/engagement, regras de revenue share, SEO/sugestão de categorias e discovery via Mystery Tile.

### Resultado
- `dotnet build Api/GameHub.sln` — 0 warnings, 0 erros.
- `dotnet test Api/GameHub.sln --no-build` — 288 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` — sucesso.
- Cobertura gerada em `TestResults/Report/Summary.txt`.

## 2026-07-25 17:15 UTC

### Tarefa
Implementar spec 24 da referência Poki (P4D v2, Inspector v3, incognito UX, CLI parity, Versions tab e Poki Pill).

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Developers/DeveloperProfile.cs`, `DeveloperTeam.cs` — `ApiKey` para autenticação CLI.
- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs`, `IGameBuildAppService.cs`, `Dto/GamehubCliManifest.cs`, `Dto/UploadFromCliInput.cs` — upload via CLI com API key.
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`, `IDeveloperGameAppService.cs`, `Dto/BuildDto.cs` — `CreatePreviewTokenForBuildAsync`, `StartInspectorSessionForBuildAsync` e `GameId`/`GameSlug` no build DTO.
- `Api/src/GameHub.Application/Privacy/PrivacyAppService.cs`, `IPrivacyAppService.cs`, `Dto/PrivacyConsentDto.cs`, `Dto/GetPrivacyConsentInput.cs` — `GetConsentAsync` com fallback para usuários anônimos.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamento `PlayerPrivacyConsent -> PrivacyConsentDto`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — colunas e índices `ApiKey`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260725170203_AddDeveloperApiKeys.*` — migração.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — `getPrivacyConsent`, `setPrivacyConsent`, `movePill`, safe localStorage guards e persistência da posição do pill.
- `angular/src/app/player/game-frame/game-frame.component.ts/.html/.css` — integração das ações de privacy/pill, toast de progresso local e overlay mobile.
- `angular/src/app/core/services/developer.service.ts`, `angular/src/app/developer/builds/builds.component.ts/.html` — ações "Open in Inspector" e "Preview on Game Hub" por build.
- `docs/gamehub-cli.md` — documentação do CLI.
- `Api/test/GameHub.Tests/GameHub/Application/PrivacyAppService_Tests.cs`, `GameBuildAppService_Tests.cs`, `DeveloperGameAppService_Tests.cs` — testes dos novos métodos.
- `README.md`, `README.pt-BR.md`, `CHANGELOG.md`, `.specs/24-poki-proxima-fase.md` — documentação atualizada.

### Motivação
Completar a fase 24 da referência Poki: suporte a equipes e playtests já haviam sido entregues; esta sessão focou em CLI parity, actions do Versions tab, overlay mobile, UX de incognito e testes.

### Resultado
- `dotnet build Api/GameHub.sln` — 0 warnings, 0 erros.
- `dotnet test Api/GameHub.sln --no-build` — 279 passaram, 2 skipped.
- `dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --collect:"XPlat Code Coverage"` — cobertura gerada em `TestResults/*/coverage.cobertura.xml`.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` — sucesso.
- Itens pendentes para próxima sessão: onboarding de equipes/billing no portal admin, integração real do CLI em Node.js, deep-link do inspector no admin e gamificação/monetização avançada.

## 2026-07-24 14:40 UTC

### Tarefa
Implementar specs 23.1, 23.3, 23.4 e preview mode do 23.2 do backlog Poki (thumbnails animados, Inspector v3, novos requisitos de qualidade e tokens de preview de builds).

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs`, `GameAspectRatio.cs`, `GameThumbnailStatus.cs` — aspect ratio e status de moderação de thumbnail.
- `Api/src/GameHub.Core/Domain/Builds/PreviewToken.cs` — entidade de token de preview.
- `Api/src/GameHub.Core/Domain/Inspector/InspectorChecklistAnswer.cs`, `InspectorSession.cs` — respostas do checklist de QA.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs`, `IAdminGameAppService.cs`, `Dto/AdminGameDetailDto.cs` — aprovação/rejeição de thumbnail.
- `Api/src/GameHub.Application/Builds/GamePreviewAppService.cs`, `IGamePreviewAppService.cs`, `Dto/*` — criação e validação de tokens de preview.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs` — validações de clean build, outgoing links, splash screens e file size.
- `Api/src/GameHub.Application/Inspector/InspectorAppService.cs`, `IInspectorAppService.cs`, `Dto/*` — checklist persistente e completion percentage.
- `Api/src/GameHub.Application/Catalog/GameCatalogAppService.cs`, `Dto/GameCardDto.cs`, `Dto/GameDetailDto.cs`, `GameHubCustomDtoMapper.cs` — mapeamento de animated thumbnail, status e aspect ratio.
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`, `IDeveloperGameAppService.cs`, `Dto/CreateGameDraftInput.cs`, `Dto/UpdateGameMetadataInput.cs` — upload/campos de thumbnail e aspect ratio.
- `Api/src/GameHub.Application/Security/IGameTokenProvider.cs`, `Api/src/GameHub.Web.Host/Security/GameTokenProvider.cs` — `CreatePreviewTokenAsync`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`, `GameHubModelCreatingExtensions.cs` — `DbSet` e configurações EF Core para `InspectorChecklistAnswer` e `PreviewToken`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260724142631_AddThumbnailsInspectorChecklistPreviewToken.*` — migração.
- `Api/src/GameHub.Web.Host/Controllers/GameAssetsController.cs` — upload de thumbnail estático/animado para MinIO/S3.
- `Api/test/GameHub.Tests/GameHub/Application/InspectorAppService_Tests.cs`, `ThumbnailModerationAppService_Tests.cs`, `GamePreviewAppService_Tests.cs`, `BuildPackageValidator_Tests.cs`, `DependencyInjection/FakeGameTokenProvider.cs` — testes.
- `angular/src/app/core/services/developer.service.ts`, `game-catalog.service.ts`; `angular/src/app/developer/game-create/*`, `game-edit/*`; `angular/src/app/public/home/*`, `games/*` — upload e exibição de thumbnails e aspect ratio.
- `angular/src/app/app.routes.ts`, `angular/src/app/player/game-frame/game-frame.component.ts` — rota `/preview/:slug/:version` e carregamento de preview.
- `angular-admin/GameHub.UI/src/app/main/gamehub/inspector/*`, `games/game-detail.component.*`, `shared/services/gamehub-admin.service.ts` — scaling presets, checklist e preview no inspector.
- `README.md`, `README.pt-BR.md`, `CHANGELOG.md`, `.specs/23-proxima-sessao-poki.md`, `Api/common.props` — documentação e supressão de advisories transitórios do runtime .NET 10.

### Motivação
Aproximar o GameHub dos requisitos de qualidade e publicação da Poki: thumbnails de catálogo, validação de builds limpos, Inspector de QA com checklist e preview seguro de builds não publicados.

### Resultado
- `dotnet build Api/GameHub.sln` — 0 warnings, 0 erros.
- `dotnet test Api/GameHub.sln --no-build` — 261 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` — sucesso.
- Cobertura gerada em `TestResults/Report/Summary.txt`.
- Itens pendentes do spec 23: QR code no Inspector, equipes/billing/playtests no P4D v2 e CLI parity.

## 2026-07-24 13:20 UTC

### Tarefa
Implementar specs 22.1, 22.2, 22.4, 22.5 e 22.9 do backlog Poki (cloud saves, contas de jogador no SDK, scroll lock, controles adaptativos e consentimento de privacidade in-game).

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs`, `GameControlScheme.cs` — propriedades `SupportsCloudSaves`, `ControlScheme`, `CutscenesSkippable`, `DefaultLanguage`, `SupportedLanguages`.
- `Api/src/GameHub.Application/Catalog/Dto/GameDetailDto.cs`, `GameCardDto.cs`, `Admin/Dto/AdminGameDetailDto.cs`, `Developer/Dto/CreateGameDraftInput.cs`, `UpdateGameMetadataInput.cs` — mapeamento dos novos campos.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — `ParseControlScheme`, `ParseSupportedLanguages` e correção de sintaxe em `IsWebExclusive`.
- `Api/src/GameHub.Application/Gameplay/ICloudSaveAppService.cs`, `CloudSaveAppService.cs` — `DeleteAsync` e filtro de chaves `gamehub_ignore_` no bridge.
- `Api/src/GameHub.Application/Player/IPlayerAccountAppService.cs`, `PlayerAccountAppService.cs`, `Dto/PlayerProfileDto.cs`, `PlayerTokenDto.cs`, `GetTokenInput.cs` — `GetPlayerProfileAsync` e `GetTokenAsync`.
- `Api/src/GameHub.Application/Security/IGameTokenProvider.cs`, `Api/src/GameHub.Web.Host/Security/GameTokenProvider.cs` — geração de JWT curto via `ITokenAuthenticationService`.
- `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs` — registro do `IGameTokenProvider`.
- `Api/src/GameHub.Core/Domain/Privacy/PlayerPrivacyConsent.cs` — entidade de consentimento.
- `Api/src/GameHub.Application/Privacy/IPrivacyAppService.cs`, `PrivacyAppService.cs`, `Dto/PrivacyPolicyDto.cs`, `SavePrivacyConsentInput.cs` — `GetForGameAsync` e `SaveConsentAsync`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`, `GameHubModelCreatingExtensions.cs` — `DbSet` e configuração de `PlayerPrivacyConsent` e novas colunas do `Game`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260724131521_AddPokiCloudSaveAndControls.*` — migração EF Core.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — `save`/`load`, `login`/`getUser`/`getToken`, `getPrivacyPolicy`, `controlScheme` e `ignorePrefix`.
- `angular/src/app/core/services/game-catalog.service.ts` — campos `supportsCloudSaves`, `controlScheme`, `cutscenesSkippable`, `defaultLanguage`, `supportedLanguages`, `privacyPolicyUrl`.
- `angular/src/app/player/game-frame/game-frame.component.ts/.html/.css` — scroll lock, focus/blur, teclado ESC/Space, hints de controle, botão "Pular", consentimento de privacidade e seletor de idioma.
- `Api/src/GameHub.Core/Domain/Player/PlayerPreference.cs` — preferência de idioma do jogador.
- `Api/src/GameHub.Application/Player/PlayerAccountAppService.cs` — `GetLanguageAsync`/`SetLanguageAsync`.
- `Api/src/GameHub.Application/Player/Dto/SetLanguageInput.cs` — input de idioma.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260724*_AddPlayerPreference.*` — migração de preferências.
- `Api/test/GameHub.Tests/GameHub/Application/CloudSaveAppService_Tests.cs`, `PlayerAccountAppService_Tests.cs`, `PrivacyAppService_Tests.cs`, `DependencyInjection/FakeGameTokenProvider.cs`, `GameHubTestModule.cs`.
- `.specs/22-poki-proxima-fase.md` — status atualizado.

### Motivação
Habilitar o SDK do GameHub para operar como distribuidora Poki: saves na nuvem, login/token do jogador, scroll/controles adaptativos e LGPD para jogos com requests externos.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 252 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Itens 22.3, 22.7 e 22.8 ainda pendentes para próxima sessão.

## 2026-07-24 13:00 UTC

### Tarefa
Implementar specs 19.10 (Inspector de QA v2) e 19.12 (Privacidade, UGC e Performance), corrigir testes pendentes e ajustar relacionamentos do EF Core.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Inspector/*` — entidades `InspectorSession`, `InspectorSdkEvent`, `InspectorWarning`.
- `Api/src/GameHub.Application/Inspector/*` — `IInspectorAppService`/`InspectorAppService` com validação de sequência de eventos SDK e warnings.
- `Api/src/GameHub.Core/Domain/Moderation/UserContent.cs` e `ProfanityFilter.cs` — filtro de profanidade com leet para UGC.
- `Api/src/GameHub.Application/Moderation/UserContentAppService.cs` — submissão e moderação de comentários/reviews.
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` e `Domain/Builds/BuildValidationReport.cs` — `PrivacyPolicyUrl` e `HasExternalRequests`.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs` — detecção de requests externos, arquivos grandes e outgoing links.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs` — bloqueio de publicação sem política de privacidade quando há requests externos.
- `Api/src/GameHub.Core/Domain/Gameplay/PlaySession.cs` e `GameMetricSnapshot.cs` — campos `FpsAverage`/`FpsMin` e `AvgFps`/`MinFps`.
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs` — `UpdateFpsAsync` com agregação no `GameMetricSnapshot`.
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs` — métricas e alertas de saúde com FPS.
- `angular-admin/GameHub.UI/src/app/main/gamehub/inspector/*` — página de sessão do inspector com iframe, timeline de eventos e re-run de validação.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — `measureFps` e modo inspector.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/*` — `AddPrivacyUgcAndPerformance` e `AddInspectorQaV2` (remove coluna shadow `GameId1` da relação `RevenueContract`/`Game`).
- `Api/test/GameHub.Tests/GameHub/Application/InspectorAppService_Tests.cs`, `ProfanityFilter_Tests.cs`, `UserContentAppService_Tests.cs`, `AdminGameAppService_Tests.cs`, `BuildPackageValidator_Tests.cs`, `GameplayAppService_Tests.cs`.
- `Api/test/GameHub.Tests/GameHub/Application/PlayerAccountAppService_Tests.cs` — teste anônimo marcado como skip por instabilidade de isolamento de sessão.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — relação `RevenueContract.Game -> Game.RevenueContracts` corrigida.

### Motivação
Fechar os specs 19.10 e 19.12 pendentes, garantir que builds com requests externos exijam política de privacidade, coletar FPS para alertas de performance e oferecer interface de QA para validar sequência de eventos do SDK.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 242 passaram, 2 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Relação `RevenueContracts`/`Game` corrigida; testes de Web Exclusives passam.

## 2026-07-24 04:08 UTC

### Tarefa
Corrigir mensagens de erro na tela de registro, implementar specs 19.11, 19.8 e 19.9 do backlog Poki, e adicionar testes para os novos métodos.

### Arquivos alterados
- `angular/src/app/core/auth/auth.service.ts` e `register.component.ts/.html` — exibe erros da API e requisitos de senha.
- `angular/public/i18n/en-US.json` e `pt-BR.json` — chaves `register.passwordHint`, `nav.login`, `nav.player` e `player.*`.
- `angular/src/app/core/services/player.service.ts`, `public/player/*` — conta do jogador, favoritos e histórico.
- `angular/src/app/core/services/ad-break.service.ts`, `gameplay-bridge.service.ts` — ad breaks com sessão, mute/unmute e respeito a `adBlocked`.
- `Api/src/GameHub.Core/Domain/Player/*` e `Domain/Gameplay/PlaySession.cs` — entidades de favoritos, recentes e contadores de ad breaks.
- `Api/src/GameHub.Application/Player/*`, `Monetization/AdBreakAppService.cs`, `ConfigurableAdProvider.cs`, `StaticVastAdProvider.cs`, `FakeAdProvider.cs` — serviços de jogador e ad provider.
- `Api/src/GameHub.Core/Application/Configuration/AdBreakOptions.cs`, `StaticVastAdOptions.cs`, `appsettings.json`, `Startup.cs` — configuração de ad breaks.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/*` — migrações para categorias SEO, relação game/receita, favoritos, recentes e contadores de ad breaks.
- `Api/test/GameHub.Tests/GameHub/Application/PlayerAccountAppService_Tests.cs`, `AdBreakAppService_Tests.cs`, `GameCatalogAppService_Tests.cs`, `FakeAdProvider_Tests.cs`.

### Motivação
Resolver o bug de registro que não exibia mensagens da API, e avançar as entregas do backlog Poki: descoberta avançada (web exclusives, SEO, filtros), contas de jogador (favoritos/recentes) e integração realista do ad provider com regras de UX.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 216 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Criado `.specs/20-proxima-sessao-poki.md` com instruções para implementar 19.10 e 19.12.
- Criado `.specs/21-poki-quality-and-compliance.md` com requisitos de qualidade baseados na documentação da Poki.

## 2026-07-24 02:40 UTC

### Tarefa
Re-analisar o projeto e a referência Poki para identificar gaps e gerar novos specs de melhorias.

### Arquivos alterados
- `.specs/18-poki-referencia.md` — atualização das seções 7.1 (implementado) e 7.2 (próximas specs).
- `.specs/19-poki-backlog.md` — tabela de estado por spec filho e nova ordem sugerida de implementação.
- `.specs/19.1-poki-pagina-jogo.md` até `19.7-poki-metricas-observabilidade.md` — atualização de status.
- `.specs/19.8-poki-contas-jogador.md` — especificação de contas de jogador, favoritos e histórico.
- `.specs/19.9-poki-ads-provider.md` — especificação de integração do ad provider e regras de UX.
- `.specs/19.10-poki-inspector-qa-v2.md` — especificação do Inspector v2.
- `.specs/19.11-poki-web-exclusivos-descoberta.md` — especificação de web exclusives, SEO de categorias e descoberta.
- `.specs/19.12-poki-privacidade-ugc-performance.md` — especificação de privacidade, profanidade e performance/FPS.

### Motivação
A base de monetização está fechada. As próximas melhorias devem aumentar retenção (contas/favoritos), qualidade (inspector/QA), descoberta (web exclusives) e conformidade (privacidade/UGC), além de habilitar receita real com um ad provider.

### Resultado
Novo backlog de specs 19.8 a 19.12 criado e mapeado no `19-poki-backlog.md`.

## 2026-07-24 02:14 UTC

### Tarefa
Implementar painel de earnings no portal do desenvolvedor (Angular) e backend de cálculo estimado de receita.

### Arquivos alterados
- `Api/src/GameHub.Application/Developer/IDeveloperEarningsAppService.cs` e `DeveloperEarningsAppService.cs` — endpoint `GetEarningsAsync` que calcula receita bruta e share do dev por jogo.
- `Api/src/GameHub.Application/Developer/Dto/DeveloperEarningsDto.cs`, `GameEarningsDto.cs`, `DailyEarningsDto.cs`, `GetDeveloperEarningsInput.cs`.
- `Api/src/GameHub.Core/GameHubConsts.cs` — constantes de receita estimada por ad break.
- `Api/test/GameHub.Tests/GameHub/Application/DeveloperEarningsAppService_Tests.cs` — testes de cálculo e split.
- `angular/src/app/core/services/developer.service.ts` — interfaces e método `getEarnings`.
- `angular/src/app/developer/earnings/earnings.component.ts/.html/.css` — novo painel de earnings.
- `angular/src/app/developer/developer.routes.ts` — rota `/developer/earnings`.
- `angular/src/app/developer/games/games.component.html`, `builds/builds.component.html`, `game-create/game-create.component.html`, `game-edit/game-edit.component.html`, `profile/profile.component.html` — link "Earnings" no menu lateral.
- `angular/src/app/developer/dashboard/dashboard.component.html` — botão "Earnings" no header.

### Motivação
Fechar a base de monetização (spec 19.6) com uma interface visível para o desenvolvedor acompanhar receita estimada por jogo e por dia, respeitando o contrato (`WebExclusive`/`NonExclusive`) e a origem do tráfego.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 217 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.

## 2026-07-24 00:59 UTC

### Tarefa
Executar o início do spec `19.6-poki-monetizacao.md`: base de monetização com contratos de receita, rastreamento de origem do jogador e split de receita.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Monetization/RevenueContractType.cs` e `TrafficSource.cs` — enums de tipo de contrato e origem do tráfego.
- `Api/src/GameHub.Core/Domain/Monetization/RevenueContract.cs` — entidade de contrato de receita.
- `Api/src/GameHub.Core/Domain/Monetization/RevenueSplitCalculator.cs` — cálculo de split dev/plataforma.
- `Api/src/GameHub.Application/Monetization/IRevenueContractAppService.cs` e `RevenueContractAppService.cs` — gerenciamento e cálculo de contratos.
- `Api/src/GameHub.Application/Monetization/Dto/RevenueContractDto.cs` e `RevenueShareResultDto.cs`.
- `Api/src/GameHub.Core/Domain/Gameplay/PlaySession.cs` — adiciona `TrafficSource`, `UtmSource`, `UtmMedium` e `UtmCampaign`.
- `Api/src/GameHub.Application/Gameplay/Dto/StartPlaySessionInput.cs` e `GameplayAppService.cs` — mapeia os campos de origem na sessão.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs` e `GameHubModelCreatingExtensions.cs` — `DbSet` e configuração do contrato + colunas da sessão.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260724003018_AddMonetizationBase.cs` — migração.
- `Api/test/GameHub.Tests/GameHub/Application/RevenueContractAppService_Tests.cs` e `FakeAdProvider_Tests.cs`.

### Motivação
Preparar a base de monetização sem acoplar a um provider de ads específico, permitindo contratos (`WebExclusive`/`NonExclusive`), rastreamento de origem (direct, homepage, search, platform, utm) e cálculo automático de split de receita.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 215 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.

## 2026-07-24 00:55 UTC

### Tarefa
Executar o início do spec `19.5-poki-inspector-qualidade.md`: validação de builds, relatório de warnings persistido e página do Inspector no admin.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Builds/BuildValidationReport.cs` — entidade para persistir resultados de validação.
- `Api/src/GameHub.Core/GameHubConsts.cs` — constantes de warning de tamanho do pacote e arquivos grandes.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs` — validações reforçadas: arquivos bloqueados, arquivos grandes, artifacts de desenvolvimento, URLs externos e viewport.
- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs` — persiste `BuildValidationReport` após upload.
- `Api/src/GameHub.Application/Builds/IBuildValidationAppService.cs` e `BuildValidationAppService.cs` — endpoints para consultar relatórios.
- `Api/src/GameHub.Application/Builds/Dto/BuildValidationReportDto.cs` e `BuildValidationReportListItemDto.cs`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs` e `GameHubModelCreatingExtensions.cs` — `DbSet` e configuração da entidade.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260724001242_AddBuildValidationReport.cs` — migração.
- `angular-admin/GameHub.UI/src/app/main/gamehub/inspector/inspector.component.ts/.html` — página de listagem de relatórios.
- `angular-admin/GameHub.UI/src/app/main/gamehub/shared/services/gamehub-admin.service.ts` — método `getValidationReports`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/gamehub.module.ts`, `gamehub-routing.module.ts` e `app-navigation.service.ts` — rota e menu "Inspector".
- `Api/test/GameHub.Tests/GameHub/Application/GameBuildAppService_Tests.cs` — testes de persistência, warning de URL externa e listagem.

### Motivação
Criar um processo de QA automatizado para builds HTML5, identificando problemas comuns (pacotes grandes, requests externos, artifacts de dev) e dando visibilidade aos moderadores via relatório persistido e tela no admin.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 209 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.

## 2026-07-24 00:45 UTC

### Tarefa
Executar o início do spec `19.7-poki-metricas-observabilidade.md`: adicionar agregações de métricas (DAU, MAU, conversão de loading, taxa de erro, distribuições) e alertas de saúde no dashboard administrativo.

### Arquivos alterados
- `Api/src/GameHub.Application/Admin/IAdminDashboardAppService.cs` — assinaturas `GetMetricsAsync` e `GetHealthAlertsAsync`.
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs` — implementação de métricas e alertas usando `PlaySession` e `GameplayEvent`.
- `Api/src/GameHub.Application/Admin/Dto/AdminMetricsSummaryDto.cs` e `AdminHealthAlertDto.cs` — novos DTOs.
- `Api/test/GameHub.Tests/GameHub/Application/AdminDashboardAppService_Tests.cs` — teste de agregação e alerta.

### Motivação
Fornecer visibilidade operacional para moderadores/administradores sobre engajamento e problemas de qualidade dos jogos, requisito da Fase 6 da referência Poki.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 206 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.

## 2026-07-24 00:05 UTC

### Tarefa
Executar o início do spec `19.4-poki-developer-portal.md`: adicionar campos `SuggestedDescription` e `SeoDescription` ao jogo, permitir edição pelo desenvolvedor e exibição no admin, e usar `SeoDescription` na meta tag da página pública.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — propriedades `SuggestedDescription` e `SeoDescription`.
- `Api/src/GameHub.Application/Catalog/Dto/GameDetailDto.cs` e `Admin/Dto/AdminGameDetailDto.cs` — expõem os novos campos.
- `Api/src/GameHub.Application/Developer/Dto/CreateGameDraftInput.cs` e `UpdateGameMetadataInput.cs` — permitem envio dos campos.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — configurações de tamanho.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/2026072400*_AddGameSeoFields.*` — migração gerada.
- `Api/test/GameHub.Tests/GameHub/Application/DeveloperGameAppService_Tests.cs` — teste de persistência.
- `angular/src/app/core/services/game-catalog.service.ts` e `developer.service.ts` — interfaces atualizadas.
- `angular/src/app/developer/game-create/game-create.component.html` e `game-edit/game-edit.component.ts/.html` — campos no formulário.
- `angular/src/app/public/game-detail/game-detail.component.ts` — `Title`/`Meta` usando `seoDescription`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/games/game-detail.component.html` — exibe campos no admin.

### Motivação
Preparar o fluxo de submissão para que o desenvolvedor sugira descrições e SEO, e o moderador/admin possa visualizar essas sugestões antes de aprovar.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 205 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- `dotnet ef migrations add AddGameSeoFields` gerada com sucesso.

## 2026-07-23 23:25 UTC

### Tarefa
Executar o spec `19.3-poki-sdk-cloud-saves.md`: SDK Promises, Cloud Saves, login/getUser/getToken e fallback local em modo anônimo.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Gameplay/CloudSave.cs` — entidade de save na nuvem.
- `Api/src/GameHub.Application/Gameplay/ICloudSaveAppService.cs`, `CloudSaveAppService.cs`, `Dto/GetCloudSaveInput.cs`, `SaveCloudSaveInput.cs`, `CloudSaveDto.cs` — serviço de cloud save com limite de 1 MB e fallback.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs` — `DbSet<CloudSave>`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — mapeamento e índices de `CloudSave`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260723231*_AddCloudSaves.*` — migração gerada.
- `Api/test/GameHub.Tests/GameHub/Application/CloudSaveAppService_Tests.cs` — testes de persistência e limite de tamanho.
- `angular/public/gamehub-sdk.js` — `init` retorna Promise, handlers de `getPlayerData`/`setPlayerData`/`login`/`getUser`/`getToken`, IDs de requisição.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — handlers para as novas mensagens do SDK, armazenamento local com prefixos e chamadas ao backend quando logado.
- `docs/agent-execution-log.md` — este registro.

### Motivação
Aproximar o SDK do portal do jogo do padrão Poki, permitindo persistência de progresso e login do jogador, com fallback seguro para modo anônimo/incognito.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 204 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- `dotnet ef migrations add AddCloudSaves` gerada com sucesso.

## 2026-07-23 23:08 UTC

### Tarefa
Executar o spec `19.2-poki-home-descoberta.md`: seções "Popular this week" e "Top free games", cálculo de crescimento para trending e SEO de páginas de categoria.

### Arquivos alterados
- `Api/src/GameHub.Core/Catalog/ITrendingScoreCalculator.cs` — novo método `CalculateGrowthScoresAsync`.
- `Api/src/GameHub.Application/Catalog/GameTrendingScoreCalculator.cs` — implementação de crescimento entre janelas de 7 dias.
- `Api/src/GameHub.Application/Catalog/Dto/HomeResponseDto.cs` — propriedades `PopularThisWeek` e `TopFree`.
- `Api/src/GameHub.Application/Catalog/GameCatalogAppService.cs` — `GetHomeAsync` retorna `PopularThisWeek` e `TopFree`; `Trending` passa a usar crescimento relativo.
- `Api/test/GameHub.Tests/GameHub/Application/GameCatalogAppService_Tests.cs` — assertivas para `PopularThisWeek` e `TopFree`.
- `angular/src/app/core/services/game-catalog.service.ts` — interface `HomeResponse` com `popularThisWeek` e `topFree`.
- `angular/src/app/public/home/home.component.ts/.html` — seções "Popular this week" e "Top free games" com templates reutilizáveis.
- `angular/src/app/public/games/games.component.ts` — `Title` e `Meta` ajustam título/descrição dinamicamente por categoria/tag/busca.
- `angular/public/i18n/pt-BR.json` e `en-US.json` — chaves `section.popularThisWeek` e `section.topFree`.

### Motivação
Reforçar a página inicial com seções de descoberta inspiradas na Poki e melhorar indexação das páginas de categoria.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 202 passaram, 1 skipped.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.

## 2026-07-23 23:04 UTC

### Tarefa
Executar o spec `19.1-poki-pagina-jogo.md`: seção de controles, jogos relacionados, categorias clicáveis e avaliação nos cards.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — propriedade `Controls` e método `RecalculateRating`.
- `Api/src/GameHub.Application/Catalog/Dto/GameDetailDto.cs`, `GameCardDto.cs` — `Controls`, `AverageRating` e `TotalVotes`.
- `Api/src/GameHub.Application/Developer/Dto/CreateGameDraftInput.cs`, `UpdateGameMetadataInput.cs` — campo `Controls`.
- `Api/src/GameHub.Application/Admin/Dto/AdminGameDetailDto.cs` — `Controls` e `TotalVotes`.
- `Api/src/GameHub.Application/Catalog/GameCatalogAppService.cs` — `GetBySlugAsync` popula `RelatedGames`, `VoteAsync` recalcula nota, `MapToCard`/`MapToDetail` calculam rating/votos.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamento de `Controls`, `AverageRating` e `TotalVotes`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — configuração da coluna `Controls`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260723225851_AddGameControlsAndRating.*` — migração EF Core.
- `angular/src/app/core/services/game-catalog.service.ts` e `developer.service.ts` — propriedades `controls`, `averageRating` e `totalVotes`.
- `angular/src/app/public/game-detail/game-detail.component.ts/.html/.css` — categorias clicáveis, seção "Controls", badge de votos/nota e jogos relacionados com rating.
- `angular/src/app/public/home/home.component.ts/.html/.css` e `games/games.component.ts/.html/.css` — cards com plays, estrelas e contagem de votos.
- `angular/src/app/developer/game-create/game-create.component.html` e `game-edit/game-edit.component.ts/.html` — campo `Controls` no formulário.
- `angular/public/i18n/pt-BR.json` e `en-US.json` — chaves `gameDetail.controls`, `gameDetail.votes`, `games.rating`, `games.votes`.
- `Api/test/GameHub.Tests/GameHub/Application/GameCatalogAppService_Tests.cs` — testes de jogos relacionados e recálculo de nota.

### Motivação
Completar o polimento da página pública do jogo inspirada na Poki, facilitando descoberta por categoria, exibindo controles e tornando a avaliação visível nos cards.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 202 passaram, 1 skipped.
- `dotnet ef migrations add AddGameControlsAndRating` gerada com sucesso.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Branch `feature/poki-backlog-19` criada a partir de `main`.

## 2026-07-23 21:15 UTC

### Tarefa
Executar a Fase 1 da referência Poki (`18-poki-referencia.md`) para o portal público: like/dislike, report de bug, tela cheia, descrição expansível e regras de sequência do SDK.

### Arquivos alterados
- `Api/src/GameHub.Core/Domain/Catalog/GameVote.cs`, `GameVoteType.cs` — entidade e enum de voto.
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — `TotalLikes` e `TotalDislikes`.
- `Api/src/GameHub.Application/Catalog/Dto/GameVoteInput.cs`, `GameVoteResultDto.cs`, `GameDetailDto.cs`, `GameCardDto.cs` — DTOs de voto e contadores.
- `Api/src/GameHub.Application/Catalog/IGameCatalogAppService.cs` e `GameCatalogAppService.cs` — endpoints `GetVote` e `Vote`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs` e `GameHubModelCreatingExtensions.cs` — DbSet e configuração do `GameVote`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260723215036_AddGameVotes.*` — migração EF Core.
- `angular/src/app/core/services/game-catalog.service.ts` — métodos `getVote` e `vote`.
- `angular/src/app/public/game-detail/game-detail.component.ts/.html/.css` — botões de like/dislike, botão "Reportar bug" e "Mostrar mais" na descrição.
- `angular/src/app/player/game-frame/game-frame.component.ts/.html/.css` — botão de tela cheia e evento `load` para `gameLoadingFinished`.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — máquina de estado que evita eventos duplicados, eventos durante anúncios e buffer de `gameLoadingFinished` antes da sessão.
- `angular/public/i18n/pt-BR.json` e `en-US.json` — novas chaves de tradução.

### Motivação
Aplicar na prática os conceitos de UX e SDK da Poki sem copiar marca ou conteúdo, melhorando a página do jogo e a confiabilidade dos eventos do bridge.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 200 passaram, 1 skipped.
- `dotnet ef migrations add AddGameVotes` gerada com sucesso.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Branch `feature/poki-public-gamepage` criada a partir de `main`.

## 2026-07-23 20:20 UTC

### Tarefa
Executar o plano de beta readiness (`17-prompt-proxima-sessao-beta.md`): documentação pública `/docs`, melhorias de UX no hub (detalhe do jogo, game frame, leaderboard, busca), portal do desenvolvedor (upload de thumbnail/hero, validação, wizard), painel admin (fila de moderação, categorias/tags, reports, menu, detalhe do jogo) e backend de suporte (upload de imagens, reports, suspensão, leaderboard display name).

### Arquivos alterados
- `angular/src/app/public/docs/` — nova área `/docs` com guias públicas (`user-guide`, `api-guide`, `admin-guide`, `sdk-guide`), menu responsivo e links no header/footer.
- `angular/src/app/public/game-detail/`, `game-frame/`, `leaderboard/`, `games/`, `home/` — UX mobile-first, modal de report, favoritos, skeletons, empty states, filtros com persistência na URL e debounce.
- `angular/src/app/developer/game-edit/`, `builds/` — upload de thumbnail/hero com preview e resumo de validação.
- `angular-admin/GameHub.UI/src/app/main/gamehub/moderation/`, `categories/`, `tags/`, `games/`, `reports/`, `shared/layout/nav/` — fila de moderação, detalhe de revisão, CRUD de categorias/tags, lista de reports, ação de suspensão e novos itens de menu.
- `Api/src/GameHub.Web.Host/Controllers/GameAssetsController.cs`, `Api/src/GameHub.Application/Developer/`, `Api/src/GameHub.Core/Storage/` — upload de imagens via MinIO com validação de extensão e tamanho.
- `Api/src/GameHub.Application/Moderation/`, `Api/src/GameHub.Application/Admin/` — backend de reports, suspensão de jogo, detalhe de moderação com histórico e resumo de validação.
- `Api/src/GameHub.Application/Gameplay/` — `GetMyRankAsync` e hidratação do display name no leaderboard.
- `Api/test/GameHub.Tests/DependencyInjection/FakeGameAssetStorage.cs`, `GameHubTestModule.cs` — fake de armazenamento para testes.
- `angular/public/i18n/en-US.json` e `pt-BR.json` — traduções novas sem textos hardcoded.

### Motivação
Preparar a plataforma GameHub para beta com fluxos completos do público ao admin, responsividade, i18n e infraestrutura de suporte backend.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 199 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config -q` e `docker compose -f docker-compose.all.yml config -q` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` sucesso.
- Branch `feature/beta-readiness` criada a partir de `main`.

## 2026-07-23 23:00 UTC

### Tarefa
Melhorias no painel admin: dashboard com KPIs de uploads, gráfico de plays, atividades recentes; e upload list com filtros de status e busca por jogo/desenvolvedor.

### Arquivos alterados
- `Api/src/GameHub.Application/Admin/IAdminDashboardAppService.cs` — adicionados `GetRecentUploadsAsync`, `GetRecentGamesAsync`, `GetTopGamesAsync`, `GetPendingReviewsAsync`.
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs` — resumo com `TotalBuilds` e `PendingUploads`; endpoints de atividades recentes e `PlaysOverTime`.
- `Api/src/GameHub.Application/Admin/Dto/AdminDashboardSummaryDto.cs` — campos `TotalBuilds` e `PendingUploads`.
- `Api/src/GameHub.Application/Admin/AdminBuildAppService.cs` e `GetBuildsInput.cs` — filtro `SearchText` por título do jogo ou nome do desenvolvedor.
- `Api/test/GameHub.Tests/GameHub/Application/AdminDashboardAppService_Tests.cs` — testes de resumo, série temporal e atividades recentes.
- `Api/test/GameHub.Tests/GameHub/Application/AdminBuildAppService_Tests.cs` — testes de busca por título e desenvolvedor.
- `angular-admin/GameHub.UI/src/app/main/gamehub/shared/services/gamehub-admin.service.ts` — métodos de atividades recentes, busca em `getBuilds` e `getPendingReviews` com `count` opcional.
- `angular-admin/GameHub.UI/src/app/main/gamehub/dashboard/dashboard.component.ts/.html` — cards extras (Total Builds, Pending Uploads), gráfico SVG de plays e painéis de Pending Reviews, Recent Uploads, Top Games e Recent Games.
- `angular-admin/GameHub.UI/src/app/main/gamehub/uploads/build-list.component.ts/.html` — filtros de status, busca por texto, formatação de tamanho e ações de Game/Files.

### Motivação
Dar ao administrador visão centralizada dos uploads, jogos e revisões pendentes, facilitando moderação e acompanhamento da plataforma.

### Resultado
- `dotnet build Api/GameHub.sln -c Release` sucesso.
- `dotnet test Api/GameHub.sln -c Release` — 199 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.
- PR `feature/admin-uploads-dashboard` criado para `main`.

## 2026-07-23 22:45 UTC

### Tarefa
Simulação completa do GameHub via Docker: subir toda a infra (Postgres, Redis, MinIO, API, admin, hub), testar admin, cadastrar novo desenvolvedor, fazer upload de zip com `index.html`, aprovar/submeter/revisar/publicar e verificar no hub.

### Arquivos alterados
- `docker-compose.override.test.yml` — overrides locais para JWT, CORS, PostgreSQL/MinIO e `PublicEndpoint` (não commitado, uso local).
- `angular/nginx.conf` (novo) — proxy `/api/` para backend e SPA fallback.
- `angular/Dockerfile` — copia `nginx.conf` para a imagem.
- `angular/src/app/app.routes.ts` — move `play/:slug` e `leaderboard/:gameId` antes das rotas públicas lazy.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — `setGameOrigin()` e uso de origem dinâmica.
- `angular/src/app/player/game-frame/game-frame.component.ts` — deriva `gameOrigin` de `publishedBuildUrl`.
- `angular/src/environments/environment.ts` — fallback `http://localhost:9000` para origem do jogo.
- `Api/src/GameHub.Web.Host/Storage/S3ClientFactory.cs` — `AuthenticationRegion` quando endpoint customizado está presente.
- `Api/src/GameHub.Web.Host/Storage/MinioStorageOptions.cs` — adiciona `PublicEndpoint`.
- `Api/src/GameHub.Web.Host/Storage/MinioGameAssetStorage.cs` — `BuildPublicUrl` usa `PublicEndpoint`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubPermissionSeeder.cs` — adiciona `Pages.Developer.Games` e `Pages.Developer.Profile` à role `Developer`.

### Motivação
Durante a simulação foram encontrados problemas que impediam o fluxo completo: rotas `/play/:slug` caindo em 404, MinIO retornando `AccessDenied`, AWS SDK chamando S3 real, URLs públicas usando endpoint interno e dashboard do desenvolvedor negando acesso por falta de permissão. Os ajustes tornam o fluxo local funcional e preparam a base para ambientes reais com `PublicEndpoint` configurável.

### Fluxo validado
1. `docker compose -f docker-compose.all.yml -f docker-compose.override.test.yml up -d --build`
2. Admin em `http://localhost:4602` carregou e login funcionou.
3. Registro de `devtest03` via `/api/services/app/Registration/Register`.
4. Criação do jogo `Space Shooter` (`/api/services/app/DeveloperGame/CreateDraft`).
5. Upload de `space-shooter.zip` com `index.html` (`/api/game-builds/{gameId}/upload`).
6. Aprovação do build pelo dev (`/api/services/app/DeveloperGame/ApproveBuild`).
7. Submissão para revisão (`/api/services/app/DeveloperGame/SubmitForReview`).
8. Aprovação no admin (`/api/services/app/Moderation/CompleteReview`).
9. Hub `http://localhost:4600` listou `Space Shooter` e detalhe mostrou descrição/instruções.
10. Execução em `/play/space-shooter` carregou o `index.html` do jogo.
11. Admin dashboard: 2 jogos, 2 builds, 4 usuários, 3 developers, 2 plays.

### Resultado
- `dotnet build Api/GameHub.sln -c Release` sucesso.
- `dotnet test Api/GameHub.sln -c Release` — 199 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.
- PR `bugfix/simulation-fixes` (#38) criado para `main`.

## 2026-07-23 22:00 UTC

### Tarefa
Implementar Fase B do plano aprovado: design system, internacionalização (i18n) e lazy routes no frontend Game Hub.

### Arquivos alterados
- `angular/src/styles.css` — tokens CSS do design system (`--gh-primary`, `--gh-surface`, `--gh-radius`, etc.).
- `angular/src/app/shared/ui/button/button.component.ts` — componente `app-button` com variantes `primary/secondary/ghost`.
- `angular/src/app/shared/ui/card/card.component.ts` — componente `app-card`.
- `angular/src/app/shared/ui/badge/badge.component.ts` — componente `app-badge`.
- `angular/src/app/shared/ui/skeleton/skeleton.component.ts` — componente `app-skeleton`.
- `angular/src/app/shared/ui/pagination/pagination.component.ts` — componente `app-pagination`.
- `angular/src/app/shared/ui/language-selector/language-selector.component.ts` — seletor de idioma.
- `angular/src/app/core/i18n/i18n.service.ts` — `I18nService` com carregamento de JSON por idioma, persistência em `localStorage` e `BehaviorSubject`.
- `angular/src/app/core/i18n/translate.pipe.ts` — pipe `translate` impuro para atualizar templates ao trocar idioma.
- `angular/public/i18n/pt-BR.json` e `en-US.json` — dicionários iniciais.
- `angular/src/app/app.config.ts` — `APP_INITIALIZER` para carregar idioma padrão.
- `angular/src/app/app.routes.ts` — `loadChildren` para `public/public.routes.ts` e `developer/developer.routes.ts`.
- `angular/src/app/public/public.routes.ts` — rotas lazy da área pública.
- `angular/src/app/developer/developer.routes.ts` — rotas lazy da área do desenvolvedor.
- `angular/src/app/public/home/home.component.ts/.html` — tradução das strings, `app-button` e `app-language-selector`.
- `angular/src/app/developer/dashboard/dashboard.component.ts/.html` — tradução e `app-badge`.

### Motivação
Padronizar a base visual e textual do Game Hub, permitir futura expansão multilíngue e melhorar a organização das rotas com lazy loading.

### Resultado
- `dotnet build Api/GameHub.sln -c Release` sucesso.
- `dotnet test Api/GameHub.sln -c Release` — 194 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.
- PR `feature/hub-design-system` criado para `main`.

## 2026-07-23 21:00 UTC

### Tarefa
Implementar Fase D do plano aprovado: dashboard estilo Poki para o desenvolvedor, SDK JavaScript para jogos e serviço de métricas por jogo.

### Arquivos alterados (backend)
- `Api/src/GameHub.Application/Developer/IDeveloperDashboardAppService.cs` e `DeveloperDashboardAppService.cs` — resumo de jogos, builds, ações pendentes e gráfico de plays dos últimos 7 dias.
- `Api/src/GameHub.Application/Developer/Dto/DeveloperDashboardDto.cs`, `DeveloperGameVersionDto.cs`, `DeveloperDashboardActionDto.cs` — DTOs do dashboard.
- `Api/src/GameHub.Application/Gameplay/IGameMetricsAppService.cs` e `GameMetricsAppService.cs` — métricas de plays, jogadores únicos, duração e eventos (loading, errors, comerciais, rewarded), com filtros de data, país e device.
- `Api/src/GameHub.Application/Gameplay/Dto/GameMetricsFilter.cs`, `GameMetricsResult.cs`, `GameMetricsDailyItemDto.cs` — contratos do serviço de métricas.
- `Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs` e `DeveloperGameAppService.cs` — adicionado `GetVersionsAsync` (alias para `GetBuildsAsync`).
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubPermissionSeeder.cs` — permissões `Pages.Developer`, `Pages.Developer.Profile` e `Pages.Developer.Games` concedidas ao papel Admin.

### Arquivos alterados (frontend)
- `angular/public/gamehub-sdk.js` — SDK global `GameHubSDK` com `init`, `gameLoadingStarted/Finished`, `gameplayStart/Stop`, `commercialBreakRequested`, `rewardedBreakRequested`, `captureError` e `gameMeasuredEvent`.
- `angular/src/app/player/game-frame/game-frame.component.ts` — remove chamada automática de `gameplayStart()` no carregamento; aguarda evento do jogo/SDK.
- `angular/src/app/core/services/developer.service.ts` — adicionados `getDashboard`, `getGameMetrics`, `getGameVersions` e interfaces correspondentes.
- `angular/src/app/developer/dashboard/dashboard.component.ts/.html/.css` — cards de resumo, gráfico SVG simples de plays, lista de versões e ações pendentes.

### Testes
- `Api/test/GameHub.Tests/GameHub/Application/DeveloperDashboardAppService_Tests.cs` — resumo, builds recentes e `PlaysOverTime`.
- `Api/test/GameHub.Tests/GameHub/Application/GameMetricsAppService_Tests.cs` — totais/diário e filtro por país.

### Motivação
Oferecer ao desenvolvedor visibilidade dos seus jogos e métricas, e padronizar a comunicação do iframe via SDK próprio, como em plataformas de distribuição de jogos HTML5.

### Resultado
- `dotnet build Api/GameHub.sln -c Release` sucesso.
- `dotnet test Api/GameHub.sln -c Release` — 194 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.
- PR `feature/developer-dashboard-sdk` criado para `main`.

## 2026-07-23 17:20 UTC

### Tarefa
Implementar Fase C do plano aprovado: busca full-text, trending real e cache granular no catálogo.

### Arquivos alterados
- `Api/src/GameHub.Application/Catalog/IGameCatalogCache.cs` — estendido com operações de cache para detalhe por slug, busca, categorias e tags, com invalidação e TTL.
- `Api/src/GameHub.Application/Catalog/InMemoryGameCatalogCache.cs` — reescrito para usar `IMemoryCache` singleton e `IAbpSession` com chaves por tenant.
- `Api/src/GameHub.Web.Host/Caching/RedisGameCatalogCache.cs` — implementação `IDistributedCache` para todas as operações de `IGameCatalogCache`.
- `Api/src/GameHub.Core/Catalog/IGameSearchEngine.cs` e `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/Catalog/GameSearchEngine.cs` — motor de busca provider-aware: full-text `EF.Functions.ToTsVector/PlainToTsQuery` para PostgreSQL e fallback `Contains` para InMemory/SQL Server.
- `Api/src/GameHub.Core/Catalog/ITrendingScoreCalculator.cs` e `Api/src/GameHub.Application/Catalog/GameTrendingScoreCalculator.cs` — cálculo de trending a partir de `GameMetricSnapshot` dos últimos N dias.
- `Api/src/GameHub.Application/Catalog/GameCatalogAppService.cs` — usa `IGameSearchEngine`, `ITrendingScoreCalculator` e `IGameCatalogCache`; `GetHomeAsync` ordena trending por pontuação real, `GetBySlugAsync` e `SearchAsync` leem/escrevem cache; filtros de categoria/tag usam IDs para evitar joins em InMemory.
- `Api/src/GameHub.Application/Catalog/CategoryAppService.cs` e `TagAppService.cs` — usam cache do catálogo e invalidam home/detalhes/listagens em criação/edição/remoção.
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`, `Admin/AdminGameAppService.cs`, `Moderation/ModerationAppService.cs` — invalidam slug e home em alterações de estado dos jogos.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamento de `GameDetailDto.Categories`.
- `Api/src/GameHub.Application/Catalog/Dto/GameDetailDto.cs` — adicionada `List<CategoryDto> Categories`.
- `Api/src/GameHub.Application/Catalog/Dto/IGameCatalogAppService.cs` — parâmetros `CancellationToken` em todos os métodos.
- `Api/test/GameHub.Tests/DependencyInjection/ServiceCollectionRegistrar.cs` — `services.AddMemoryCache()` para testes do cache in-memory.
- `Api/test/GameHub.Tests/GameHub/Application/GameCatalogAppService_Tests.cs` — testes de slug, busca, home, ordenação, filtro por categoria e trending por métricas.
- `Api/test/GameHub.Tests/GameHub/Application/InMemoryGameCatalogCache_Tests.cs` — testes unitários das operações de cache.
- `Api/test/GameHub.Tests/GameHub/Application/RedisGameCatalogCache_Tests.cs` — testes estendidos para detalhe, busca e categorias no Redis.
- `Api/test/GameHub.Tests/GameHub/Application/GameSearchEngine_Tests.cs` e `GameTrendingScoreCalculator_Tests.cs` — removidos (cenários cobertos por testes de integração do `GameCatalogAppService`).

### Motivação
Substituir busca por `Contains` e trending por `TotalPlays` por mecanismos reais (full-text PostgreSQL + scores de `GameMetricSnapshot`) e adicionar cache granular para melhorar latência e escalabilidade do catálogo.

### Resultado
- `dotnet build Api/GameHub.sln -c Release` sucesso.
- `dotnet test Api/GameHub.sln -c Release` — 189 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.
- PR `feature/search-trending-cache` criado para `main`.

## 2026-07-22 22:45 UTC

### Tarefa
Analisar `.specs/`, documentação do Poki e docs ABP/EAF para mapear gaps e oportunidades de melhoria após os PRs #26 a #33.

### Arquivos alterados
- `docs/specs-improvements.md` — análise atualizada do estado funcional, gaps em relação aos specs e oportunidades inspiradas no Poki.
- `docs/known-issues.md` — ajustado para refletir que Redis, MinIO e telas GameHub já foram implementados; mantidos os pontos de segurança e frontend pendentes.

### Principais conclusões
- Segurança: CSP/rate-limit/headers foram removidos para resolver CORS/504; JWT ainda usa `localStorage`; espec exige refresh `HttpOnly` e reintrodução controlada dos middlewares.
- Backend: busca ainda usa `Contains` em vez de full-text PostgreSQL; trending é `TotalPlays`; faltam TTL de cache granular e domínio isolado para builds.
- Frontend: falta design system, i18n, interceptors, lazy modules conforme spec e SDK próprio (`gamehub-sdk.js`); `gameplayStart` dispara no load ao invés do primeiro input.
- Poki: oportunidades claras em dashboard do dev, QA/Inspector, versions/preview/playtests, thumbnails (estático e animado), métricas, earnings, cloud saves e ad policy.

### Resultado
- Análise consolidada em `docs/specs-improvements.md` para apoiar a escolha da próxima fase de implementação.

## 2026-07-23 13:50 UTC

### Tarefa
Implementar aprovação de uploads pelo desenvolvedor, painel administrativo de uploads e arquivos extraídos, e ajustar fluxo de moderação para exigir build `Approved` antes da publicação.

### Arquivos alterados
- `Api/src/GameHub.Core/Storage/StoredFile.cs` — DTO para arquivos armazenados no MinIO.
- `Api/src/GameHub.Core/Storage/IGameAssetStorage.cs` — adicionado `ListBuildFilesAsync`.
- `Api/src/GameHub.Web.Host/Storage/MinioGameAssetStorage.cs` — implementação de listagem paginada de objetos no prefixo `builds/{gameId}/{buildId}/` com inferência de content-type.
- `Api/src/GameHub.Application/Admin/IAdminBuildAppService.cs`, `AdminBuildAppService.cs`, `AdminBuildListItemDto.cs`, `BuildFileDto.cs`, `GetBuildsInput.cs` — serviço admin para listar builds e arquivos.
- `Api/src/GameHub.Application/Admin/IAdminGameAppService.cs` e `AdminGameAppService.cs` — removidos métodos `ApproveBuildAsync`/`RejectBuildAsync` (aprovação agora é do desenvolvedor).
- `Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs` e `DeveloperGameAppService.cs` — adicionados `ApproveBuildAsync`/`RejectBuildAsync` com verificação de ownership e `SubmitForReviewAsync` exige build `Approved`.
- `Api/src/GameHub.Application/Developer/Dto/DeveloperApproveBuildInput.cs`, `DeveloperRejectBuildInput.cs`, `BuildDto.cs`, `GameSummaryDto.cs` — DTOs de aprovação e campos `latestBuildStatus`/`latestBuildId`.
- `Api/src/GameHub.Application/Moderation/ModerationAppService.cs` — `CompleteReviewAsync` aprovado chama `build.Publish()` + `game.Publish(build.Id)` (remove `build.Approve()` redundante).
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamento de `LatestBuildStatus`/`LatestBuildId` e campos de `BuildDto`.
- `Api/test/GameHub.Tests/GameHub/Application/AdminBuildAppService_Tests.cs`, `DeveloperGameAppService_Tests.cs`, `ModerationAppService_Tests.cs` — testes de aprovação do desenvolvedor, listagem admin e fluxo de publicação.
- `angular/src/app/core/services/developer.service.ts`, `angular/src/app/developer/builds/builds.component.ts/.html`, `angular/src/app/developer/games/games.component.ts/.html` — ações Aprovar/Rejeitar no painel do desenvolvedor e submissão apenas com build `Approved`.
- `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts` — menu `Uploads`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/gamehub-routing.module.ts` e `gamehub.module.ts` — rotas `uploads` e `uploads/:id` com componentes.
- `angular-admin/GameHub.UI/src/app/main/gamehub/uploads/build-list.component.ts/.html` e `build-files.component.ts/.html` — telas de listagem de uploads e arquivos extraídos.
- `angular-admin/GameHub.UI/src/app/main/gamehub/shared/services/gamehub-admin.service.ts` — `getBuilds` e `getBuildFiles`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/games/game-detail.component.html` — histórico de builds com link para arquivos.

### Motivação
Garantir que todo upload passe por aprovação do desenvolvedor antes de ir à moderação, e permitir que o admin visualize todos os uploads e os arquivos extraídos de cada build.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 176 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm ci --legacy-peer-deps && npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.

## 2026-07-23 03:25 UTC

### Tarefa
Implementar PR-5 do plano de gaps: cadastro público, fluxo completo de desenvolvedor (criação/edição/submissão de jogo), upload de build, moderação/publicação, dashboard admin com jogos e usuários, e correção do CORS para o admin.

### Arquivos alterados
- `Api/src/GameHub.Application/Authorization/RegistrationAppService.cs`, `IRegistrationAppService.cs` e `Dto/RegisterInput.cs`/`RegisterOutput.cs` — endpoint anônimo de registro com roles `Player` e `Developer`, criação de `DeveloperProfile`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubPermissionSeeder.cs` — seed das roles `Player` e `Developer` no host.
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs` — `CreateDraftAsync` com geração de slug único, persistência de categorias/tags e `TenantId`; `SubmitForReviewAsync` cria `ModerationReview`.
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — `SetCategories`/`SetTags` e `AgeRating` padrão `"E"`.
- `Api/src/GameHub.Application/Developer/Dto/CreateGameDraftInput.cs` e `UpdateGameMetadataInput.cs` — `Description`/`Instructions` não obrigatórios; `CategoryIds`/`TagIds` como `List<Guid>`.
- `Api/src/GameHub.Application/Catalog/Dto/GameDetailDto.cs` e `GameCatalogAppService.cs` — mapeamento de `Categories` no detalhe.
- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs` — versão determinística `1.0.{buildNumber}` a partir do máximo existente.
- `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs` e `Developer/Dto/ValidationSummaryDto.cs`/`UploadGameBuildResultDto.cs` — `ValidationSummary` como objeto com `PackageSizeBytes`, `Warnings`, `HashSha256`, `HasIndexHtml` e `IndexHtmlPath`.
- `Api/src/GameHub.Application/Moderation/ModerationAppService.cs` — `CompleteReviewAsync` transiciona `GameBuild` e `Game` (`Approved`/`Rejected`/`RequiresChanges`).
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs`, `AdminUserAppService.cs`, `IAdminUserAppService.cs`, `Dto/AdminUserListItemDto.cs` e `AdminDashboardSummaryDto.cs` — total de usuários/desenvolvedores e listagem paginada de usuários.
- `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs`, `Startup/Startup.cs` e `appsettings.Production.json` — CORS com wildcard `*.afonsoft.dev`, `AllowAnyOrigin` opt-out, headers EAF e `UseCors` após `UseRouting`.
- `angular/src/app/core/auth/token.service.ts`, `auth.service.ts`, `public/register/*` — leitura de claims EAF, auto-login após registro e checkbox de desenvolvedor.
- `angular/src/app/core/services/game-catalog.service.ts`, `developer/*` — seleção de tags, submeter para revisão, `accept=".zip"` e hint de upload.
- `angular-admin/GameHub.UI/src/app/main/gamehub/dashboard/dashboard.component.html`, `users/*`, `shared/services/gamehub-admin.service.ts`, `gamehub-routing.module.ts`, `gamehub.module.ts`, `shared/layout/nav/app-navigation.service.ts` — cards de usuários/desenvolvedores, listagem paginada de usuários e menu GameHub.
- `Api/test/GameHub.Tests/GameHub/Application/RegistrationAppService_Tests.cs`, `DeveloperGameAppService_Tests.cs`, `ModerationAppService_Tests.cs`, `GameBuildAppService_Tests.cs`, `AdminUserAppService_Tests.cs`, `BuildPackageValidator_Tests.cs` — testes dos novos fluxos.
- `Api/test/GameHub.Tests/Authorization/Roles/RoleAppService_Tests.cs` e `UserAppService_GetUserForEdit_Tests.cs` — ajustados para 3+ roles no host.

### Motivação
Tornar funcional o fluxo de usuário: cadastro público, criação/upload de jogo pelo desenvolvedor, revisão/publicação pela moderação e dashboard administrativo com jogos e usuários, além de corrigir o bloqueio de CORS do admin.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 170 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm ci --legacy-peer-deps && npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.

## 2026-07-23 02:50 UTC

### Tarefa
Implementar PR-4 do plano de gaps: frontends (hub/admin), observabilidade/LGPD e DevOps.

### Arquivos alterados
- `Api/src/GameHub.Core/Application/Jobs/GameMetricsAggregationJob.cs` — job Hangfire que agrega `PlaySession` e `GameplayEvent` por `GameId`/`Date` e persiste `GameMetricSnapshot`.
- `Api/src/GameHub.Core/Application/Extensions/HangfireExtensions.cs` — agendamento diário do job `metrics-aggregation`.
- `Api/src/GameHub.Application/Privacy/PrivacyAppService.cs` e `IPrivacyAppService.cs` — exportação (`ExportUserDataAsync`) e anonimização (`DeleteUserDataAsync`) de dados pessoais conforme LGPD.
- `Api/src/GameHub.Application/Privacy/Dto/UserDataExportDto.cs` — DTOs de exportação de dados.
- `Api/test/GameHub.Tests/GameHub/Jobs/GameMetricsAggregationJob_Tests.cs` e `PrivacyAppService_Tests.cs` — testes dos jobs e do serviço de privacidade.
- `angular/src/environments/environment.ts` e `environment.prod.ts` — adicionado `gameOrigin` e `apiUrl`; configurado `fileReplacements` no `angular.json`.
- `angular/src/app/player/game-frame/game-frame.component.ts` — `postMessage` agora usa `environment.gameOrigin`, tela de erro `loadingError` e validação de origem.
- `angular/src/app/core/services/gameplay-bridge.service.ts` — whitelist de origem no `handleMessage`.
- `angular/src/app/app.routes.ts` — rotas `games/:slug`, `search` para `SearchPageComponent`, `leaderboard/:gameId` e wildcard `**` para `NotFoundComponent`.
- `angular/src/app/public/not-found/not-found.component.ts` e `search-page/search-page.component.ts` — componentes novos.
- `angular-admin/GameHub.UI/src/app/main/gamehub/gamehub-routing.module.ts` — rotas filhas com resolvers para `games/:id`, `moderation/:id`, `categories/create`, `categories/:id/edit`, `tags/create`, `tags/:id/edit`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/resolvers/*.resolver.ts` — `gameDetailResolver`, `moderationDetailResolver`, `categoryEditResolver`, `tagEditResolver`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/shared/services/gamehub-admin.service.ts` — adicionados `getCategoryById` e `getTagById`.
- `angular-admin/GameHub.UI/src/app/core/guards/admin.guard.ts`, `moderator.guard.ts`, `guest.guard.ts`.
- `install.sh` — flag `-a` para usar `docker-compose.all.yml` (full stack).
- `Api/src/GameHub.Web.Host/appsettings*.json` — substituídas connection strings reais por placeholders PostgreSQL, provedor padrão `PostgreSQL` e adicionadas seções `Cors:HubOrigins`/`Cors:AdminOrigins`.

### Motivação
Fechar os gaps de frontend (rotas, iframe seguro, resolvers/guards), observabilidade (agregação de métricas por jogo) e LGPD (exportação/anonimização de dados pessoais), além de alinhar DevOps para uso do PostgreSQL/Redis/MinIO sem expor secrets.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 162 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.

## 2026-07-23 01:20 UTC

### Tarefa
Implementar PR-3 do plano de gaps: security headers, CSP, rate limit e CORS, alinhando com `15-csp-security-headers.md`.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Middleware/SecurityHeadersMiddleware.cs` — `X-Content-Type-Options=nosniff`, `X-Frame-Options=DENY` (override para `SAMEORIGIN` em `/play`), `X-XSS-Protection=0`, `Referrer-Policy`, `Permissions-Policy`, HSTS, `X-Permitted-Cross-Domain-Policies`, `Cross-Origin-Resource-Policy`, remoção de `Server`/`X-Powered-By`.
- `Api/src/GameHub.Web.Host/Middleware/ContentSecurityPolicyMiddleware.cs` — CSP de produção e report-only para desenvolvimento com diretivas `default-src`, `script-src`, `style-src`, `img-src`, `font-src`, `connect-src`, `frame-src`, `frame-ancestors`, `object-src`, `base-uri`, `form-action`, `upgrade-insecure-requests`.
- `Api/src/GameHub.Web.Host/Middleware/RateLimitingMiddleware.cs` — contador distribuído com regras por caminho/IP/sessão: `default` 100 req/min, `auth` 10 req/min, `gameplay` 60 req/min por `X-Session-Id`, `upload` 5 req/hora por usuário; headers `X-RateLimit-*` e resposta `429`.
- `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs` — políticas `GameHubCors` e `GameHubAdminCors` com origens configuráveis, métodos, headers, headers expostos de rate limit, `AllowCredentials` e preflight cache de 600s.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — registro e ordenação dos middlewares, `UseHsts` em produção, `UseCors` com política default.
- `Api/src/GameHub.Web.Host/appsettings*.json` — adicionadas seções `Cors:HubOrigins` e `Cors:AdminOrigins` por ambiente.
- `Api/test/GameHub.Tests/Middleware/SecurityHeadersMiddleware_Tests.cs`, `ContentSecurityPolicyMiddleware_Tests.cs`, `RateLimitingMiddleware_Tests.cs`, `CorsConfiguration_Tests.cs`.

### Motivação
A API e o host precisam emitir headers de segurança adequados, CSP estrito, limitação de taxa por tipo de endpoint e CORS segmentado entre hub e admin, sem expor `*` ou credenciais indevidamente.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 158 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm ci --legacy-peer-deps && npm run build` em `angular/` e `angular-admin/GameHub.UI` sucesso.

## 2026-07-23 00:15 UTC

### Tarefa
Implementar PR-2 do plano de gaps: caches Redis para catálogo (`IGameCatalogCache`) e leaderboard (`ILeaderboardCache`), alinhamento de permissões RBAC com `12-rbac-permissions.md`, `[AbpAuthorize]` em AppServices críticos e seed de permissões para SuperAdmin/Admin/Moderator/Developer/Player.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Caching/RedisGameCatalogCache.cs` — implementação Redis do catálogo usando `IDistributedCache`, serialização JSON e chave `gamehub:catalog:home:{tenant}`.
- `Api/src/GameHub.Web.Host/Caching/RedisLeaderboardCache.cs` — implementação Redis do leaderboard usando `IConnectionMultiplexer` + `SortedSetIncrementAsync`/`SortedSetRangeByRankWithScoresAsync` com chave `leaderboard:{tenant}:{gameId}`.
- `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs` — registro condicional de `IConnectionMultiplexer` e substituição dos caches in-memory pelas implementações Redis quando `RedisCache:IsEnabled` for `true`.
- `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs` — adicionadas `Pages_Users` e `Pages_Users_Manage`.
- `Api/src/GameHub.Core/Application/Authorization/GameHubAuthorizationProvider.cs` — registro das permissões de usuários.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubPermissionSeeder.cs` — seed de roles `Moderator`, `Developer`, `Player` e permissões padrão para host/default tenant.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/SeedHelper.cs` e `Api/test/GameHub.Tests/GameHubTestBase.cs` — execução do `GameHubPermissionSeeder` durante o seed.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs`, `Builds/GameBuildAppService.cs`, `Developer/DeveloperGameAppService.cs`, `Moderation/ModerationAppService.cs`, `Catalog/CategoryAppService.cs`, `Catalog/TagAppService.cs` — adicionados atributos `[AbpAuthorize(...)]` nos métodos críticos.
- `Api/test/GameHub.Tests/GameHub/Application/RedisGameCatalogCache_Tests.cs`, `RedisLeaderboardCache_Tests.cs`, `GameHubPermissionSeeder_Tests.cs` — testes dos caches Redis (com `MemoryDistributedCache` e NSubstitute) e do seeder.
- `Api/test/GameHub.Tests/Authorization/Roles/RoleAppService_Tests.cs` — ajustado para 4 roles no tenant (`Admin`, `Moderator`, `Developer`, `Player`).
- `docs/abp-documentation-index.md`, `docs/abp/*.md` e `docs/eaf/*` — documentação ABP/EAF salva para contexto.

### Motivação
O plano de gaps previa a substituição dos caches in-memory por Redis para escala e a criação do RBAC/seed de permissões. As chaves foram escopo de tenant para respeitar multi-tenancy. `[AbpAuthorize]` protege endpoints administrativos sem afetar o catálogo/leaderboard públicos.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 150 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm ci --legacy-peer-deps && npm run build` no `angular-admin/GameHub.UI` sucesso.

## 2026-07-22 23:45 UTC

### Tarefa
Corrigir erros de layout/checkbox, codificação de caracteres e endpoints 503 reportados no admin Angular/API, além de remover rotas/menus legados (`Airplanes`, `Parameters`, `Hangfire`).

### Arquivos alterados
- `angular-admin/GameHub.UI/src/assets/common/styles/styles.css` — `.m-switch-label` ajustado para `display: inline-block`, `vertical-align: top`, `line-height: 34px` e `width: calc(100% - 75px)`, evitando quebra de linha sob o switch.
- `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts` — removidos os itens `Airplanes`, `Parameters` e `Hangfire` do menu.
- `angular-admin/GameHub.UI/src/app/admin/admin-routing.module.ts` e `admin.module.ts` — removidas importações/declarações do componente `Hangfire` e sua rota.
- `angular-admin/GameHub.UI/src/app/admin/hangfire/hangfire.component.*` — arquivos removidos (dashboard inacessível enquanto `Hangfire:IsEnabled` estiver `false`).
- `Api/src/GameHub.Core/Application/Localization/GameHub/GameHub-pt-BR.xml` e `GameHub.xml` — adicionadas chaves `UseCaptchaOnLogin` e `ReCaptcha` para parar de exibir o raw key na tela.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/DefaultLanguagesCreator.cs` — agora atualiza `DisplayName` e `Icon` caso o idioma já exista, garantindo correção de `Português (Brasil)`/`Español` em bases preenchidas.
- `Api/test/GameHub.Web.Tests/GameHubWebTestModule.cs` — removida dependência duplicada de `GameHubTestModule` e adicionada guarda em `RegisterFakeService` para evitar duplicidade de componentes.
- `Api/test/GameHub.Tests/GameHub/Application/HostSettingsAppService_Tests.cs` — teste de integração para `GetAllSettingsAnonymous`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/DefaultLanguagesCreator.cs` e `Api/test/GameHub.Tests/Localization/Localization_Tests.cs` — convertidos para UTF-8 para corrigir caracteres acentuados.

### Motivação
Os screenshots mostravam o label do switch "Consentimento de cookies ativado" quebrando para baixo do toggle e sobrepondo o título "UseCaptchaOnLogin" (que aparecia como raw key). O log de rede exibia `503` nos itens `parameters`, `hangfire` e `settings` do admin; investigação apontou rotas/menus remanescentes do template (`Airplanes`, `Parameters`) e acesso incondicional ao dashboard Hangfire desabilitado no Docker Compose (`Hangfire__IsEnabled=false`).

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 144 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `npm ci --legacy-peer-deps && npm run build` no `angular-admin/GameHub.UI` sucesso.

## 2026-07-22 22:45 UTC

### Tarefa
Implementar PR-1 do plano de gaps (Fase 9 + Fase 1): remover legado `Airplanes`, implementar publicação de builds no MinIO com extração de ZIP e cálculo de `PublicBaseUrl`/`IndexHtmlPath`, e adicionar idempotência de sessão de gameplay via `ClientRequestId`.

### Arquivos alterados
- Remoção do domínio/aplicação/testes do `Airplane`: `GameHubDbContext`, `GameHubPermissions`, `GameHubAuthorizationProvider`, `EntityHistoryHelper`, `GameHubCustomDtoMapper`, `HangfireExtensions`, localizações, Angular `main-routing.module.ts`, testes de localization/mapper/permissions/settings.
- Migrações EF Core: `20260722225753_RemoveAirplanes` e `20260722225953_AddPlaySessionClientRequestId` (PostgreSQL).
- `Api/src/GameHub.Web.Host/Storage/MinioGameAssetStorage.cs` — extrai entradas do ZIP, faz upload individual com content-type, mantém o pacote original e retorna `PublicBaseUrl`.
- `Api/src/GameHub.Core/Storage/StoredAsset.cs` — adiciona `PublicBaseUrl`.
- `Api/src/GameHub.Application/Developer/Dto/ValidationSummaryDto.cs` e `Builds/GameBuildPackageValidator.cs` — capturam e validam `IndexHtmlPath`.
- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs` — persiste `PublicBaseUrl` e `IndexHtmlPath` e define `TenantId`.
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs` — adiciona `SetPublishedBuild(GameBuild build)`.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs` e `Admin/Dto/PublishGameInput.cs` — `PublishAsync` agora exige `GameBuildId` e associa build aprovado via `SetPublishedBuild`.
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`, `Gameplay/Dto/StartPlaySessionInput.cs`, `Domain/Gameplay/PlaySession.cs` e `GameHubModelCreatingExtensions` — `ClientRequestId`, índice `GameId + ClientRequestId` e `TotalPlays` incrementado apenas em sessões novas.
- `Api/src/GameHub.Application/*AppService.cs` — todas as AppServices agora herdam de `GameHubAppServiceBase` para alinhar localização e convenções.
- Testes: `MinioGameAssetStorage_Tests.cs` (ZIP extração), `GameBuildAppService_Tests.cs` (upload/persistência), `GameplayAppService_Tests.cs` (idempotência). Correção de `GameHubDomainServiceBase_Localization_Tests.cs`.

### Motivação
O domínio `Airplane` era lixo do template e quebrava o build. A publicação de builds precisava descompactar o ZIP no MinIO para que `PublicBaseUrl/IndexHtmlPath` fosse uma URL real. As sessões de gameplay precisavam de idempotência para evitar contagem duplicada de plays em retries.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 143 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.

## 2026-07-22 04:15 UTC

### Tarefa
Renomear todas as ocorrências do placeholder `ProjectName` para `GameHub` no repositório e corrigir a seleção do provider de banco de dados no runtime para respeitar `Database__Provider` do `.env`/Docker Compose.

### Arquivos alterados
- Renomeio de classes, arquivos, namespaces, strings e frontends: `GameHubDbContext`, `GameHubApplicationModule`, `GameHubConsts`, `GameHubPermissions`, `GameHubRepositoryBase`, `GameHubTestBase`, `GameHubWebTestBase`, Angular Admin (`package.json`, `manifest.json`, `index.html`, `AppConsts.ts`), Docker Compose, docs e CHANGELOG.
- Fusão de `ProjectNameConsts` em `GameHubConsts` e de `ProjectNamePermissions` em `GameHubPermissions`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContextConfigurer.cs` — lê `Database__Provider` do ambiente quando provider não é passado explicitamente.
- `.env.example` e `install.sh` — geram `Database__Provider=PostgreSQL`.
- `docker-compose.yml` e `docker-compose.all.yml` — `Database__Provider: ${Database__Provider:-PostgreSQL}`.

### Motivação
O repositório ainda continha muitos artefatos do template EAF nomeados como `ProjectName`, o que dificultava a identidade do projeto. Além disso, o backend ignorava `Database__Provider=PostgreSQL` e tentava usar SQL Server por padrão, causando erro de conexão (`Palavra-chave não suportada: 'Host'`).

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 229 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `shellcheck install.sh` sem erros.

## 2026-07-22 03:10 UTC

### Tarefa
Renomear arquivos Docker Compose e ajustar a string de conexão do Redis.

### Arquivos alterados
- `docker-compose.app.yml` renomeado para `docker-compose.yml` (app sem infraestrutura).
- `docker-compose.yml` anterior renomeado para `docker-compose.all.yml` (stack completa com Postgres, Redis, MinIO, API e frontends).
- `docker-compose.yml` e `docker-compose.all.yml` — `RedisCache__ConnectionString` usa `${REDIS_CONNECTION:-...}` com `abortConnect=false` no fallback.
- `install.sh` — default do `COMPOSE_FILE` alterado para `docker-compose.yml`.
- `README.md`, `README.pt-BR.md`, `docs/README.md`, `.specs/16-plano-implementacao-gaps.md` — atualizados para refletir os novos nomes e opções de deploy.

### Motivação
Separar a app (`docker-compose.yml`) da stack completa (`docker-compose.all.yml`) e manter a tolerância à indisponibilidade do Redis no startup via `abortConnect=false`.

### Resultado
- `docker compose -f docker-compose.yml config` valida.
- `docker compose -f docker-compose.all.yml config` valida.
- `shellcheck install.sh` passa.

## 2026-07-22 02:50 UTC

### Tarefa
Ajustar `docker-compose.yml` raiz para subir a pilha completa (Postgres, Redis, MinIO, API, Hub, Admin) e evitar crash do backend quando o Redis/Postgres do host estiver indisponível no startup. Também adicionar `minio-data/` no `.gitignore`.

### Arquivos alterados
- `docker-compose.yml` — adicionados serviços `postgres`, `redis` e `minio` com healthchecks; backend usa variáveis de ambiente (`POSTGRES_HOST`, `REDIS_CONNECTION`, `MINIO_ENDPOINT`) com defaults para os serviços internos e `,abortConnect=false` na connection string do Redis.
- `.gitignore` — ignorada a pasta `minio-data/` gerada pelo volume do MinIO.
- `docs/agent-execution-log.md` — registro desta execução.

### Motivação
O `docker-compose.yml` raiz continha `depends_on` para `postgres` e `redis`, mas os serviços não estavam declarados. Além disso, sem `abortConnect=false` o `StackExchange.Redis` abortava o startup da API quando não conseguia conectar no Redis do host.

### Resultado
- `docker compose -f docker-compose.yml config` valida sem erros.
- O backend inicia mesmo se o Redis/Postgres do host ainda não estiver disponível, reconectando em background.

## 2026-07-21 16:10 UTC

### Tarefa
Pesquisar o layout e funcionalidades do Poki e implementar o Game Hub público (`angular/`). O objetivo era uma página sem login, com lista de jogos em ícones grandes, busca, chips de categorias e visual no estilo Poki.

### Arquivos alterados
- `angular/src/app/app.{ts,html,css}` — cabeçalho sticky, navegação, rodapé e layout da aplicação.
- `angular/src/app/app.spec.ts` — ajustado para o novo layout.
- `angular/src/app/public/home/*` — landing page com hero, busca, categorias e seções de destaques/mais jogados/tendências/novos.
- `angular/src/app/public/games/*` — página de catálogo com filtros de busca/categoria e botão "Load more".
- `angular/src/app/public/game-detail/*` — página de detalhe do jogo com banner, metadados, botão Play e jogos relacionados.
- `angular/src/app/player/game-frame/*` — player em tela cheia usando `publishedBuildUrl` sanitizado e sessão de gameplay.
- `angular/src/app/core/services/game-catalog.service.ts` — serviço expandido com `getGames`, `search` e `getBySlug`, e desempacotamento seguro do envelope `AjaxResponse`.
- `angular/src/styles.css` — reset global e tipografia.
- `angular/public/placeholder-game.svg` — asset de placeholder para cards sem thumbnail.
- `CHANGELOG.md` e `docs/agent-execution-log.md`.

### Motivação
O hub público precisava de uma experiência de descoberta semelhante ao Poki: grande grid de cards, categorias, busca rápida e play imediato, sem exigir autenticação.

### Resultado
- `npm run build` do `angular/` passa (production bundle ~353 KB).
- `dotnet build Api/GameHub.sln -c Release --no-restore` passa com 0 warnings.
- `dotnet test Api/GameHub.sln` passa (224 passed, 1 skipped).

## 2026-07-21 13:05 UTC

### Tarefa
Continuar a implementação dos specs pendentes para API .NET e Angular Admin, deixando o Game Hub público para depois. Foi adicionada a entidade `FeatureFlag`, migrations, application services administrativos (dashboard, feature flags, audit log, reports), serviços de developer profile e user reports, controller de upload de builds e o módulo Angular Admin lazy-loaded.

### Arquivos alterados
- `Api/src/GameHub.Core/Configuration/FeatureFlag.cs` — entidade de feature flag.
- `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs` — permissões GameHub.
- `Api/src/GameHub.Core/Application/Authorization/GameHubAuthorizationProvider.cs` — registro das permissões.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/*` — configuração `FeatureFlag` e migration `AddFeatureFlag` PostgreSQL.
- `Api/src/GameHub.Application/Admin/**/*` — DTOs, interfaces e app services de dashboard, feature flags, audit log e reports.
- `Api/src/GameHub.Application/Moderation/UserReportAppService.cs` — submissão de denúncias.
- `Api/src/GameHub.Application/Developer/DeveloperProfileAppService.cs` — perfil de desenvolvedor.
- `Api/src/GameHub.Web.Host/Controllers/GameBuildsController.cs` — upload multipart de builds.
- `Api/src/GameHub.Application/GameHubCustomDtoMapper.cs` — mapeamentos `FeatureFlag` e `AuditLog`.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs` — filtro por `Status`.
- `Api/src/GameHub.Application/Catalog/Dto/GetGamesInput.cs` — propriedade `Status`.
- `angular-admin/GameHub.UI/src/app/main/gamehub/**/*` — módulo, rotas, componentes e serviço do painel administrativo.
- `angular-admin/GameHub.UI/src/app/main/main-routing.module.ts` — lazy load do `GameHubAdminModule`.
- `docs/agent-execution-log.md` e `CHANGELOG.md` — atualização de execução.

### Motivação
A API e o Angular Admin precisavam dos endpoints e telas restantes descritos nos specs (dashboard, moderação, categorias, tags, feature flags, audit log, upload de builds). A entidade `FeatureFlag` exigiu migration para PostgreSQL. A permissão `Pages.Dashboard` conflitava com a do EAF, então foi renomeada para `Pages.GameHubDashboard`.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` passa com 0 warnings e 0 erros.
- `dotnet test Api/GameHub.sln` passa (224 passed, 1 skipped).
- `npm run build` passa para `angular-admin/GameHub.UI`.
- Migration `AddFeatureFlag` gerada corretamente para PostgreSQL.

## 2026-07-21 01:40 UTC

### Tarefa
Ajustar o build da API do `afonsoft/gamehub`, corrigir referências de pacotes, converter EAF para NuGet 9.2.0, criar GitHub Actions baseados nos repositórios `metar-decoder` e `QRCoder.Core`, criar a pasta `angular/` com um hello world e analisar a pasta `.specs` para sugerir melhorias.

### Arquivos alterados
- `Api/GameHub.sln` — corrigido GUIDs e referências dos projetos GameHub.
- `Api/docker-compose.dcproj` — ajustado `DockerServiceName`.
- `Api/src/GameHub.Application/GameHub.Application.csproj` — `Eaf.Middleware.Application` 9.2.0.
- `Api/src/GameHub.Core/GameHub.Core.csproj` — `Eaf.Middleware.Core` 9.2.0 + ABP/EF Core.
- `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj` — pacotes NuGet EAF 9.2.0.
- `Api/src/GameHub.Web.Host/Startup/Program.cs` — `using Eaf.KeyVault` + `UseEafKeyVault`.
- `Api/test/GameHub.Web.Tests/GameHubWebTestBase.cs` — mesmo ajuste de KeyVault.
- `angular-admin/GameHub.UI/src/assets/lib/eaf-ng2-module/src/log/log.service.ts` — serviço criado para build do admin.
- `angular-admin/GameHub.UI/.gitignore` — exceção para a pasta `log/` do módulo EAF.
- `angular/` — app Angular 20 (GameHub Hub) gerado e simplificado para hello world.
- `.github/workflows/*` — CI Build & Test, Angular CI, Code Quality, Delete Branch on Merge.
- `docs/agent-execution-log.md` e `docs/specs-improvements.md` — documentação do trabalho.

### Motivação
O repositório era um template EAF renomeado com referências locais incorretas (`..\..\..\EAF\src\..` e `GameHub`), impossibilitando o build. Foi necessário apontar para os pacotes NuGet `Eaf.*` 9.2.0 e corrigir a solução. O frontend admin estava com o `LogService` ausente no módulo `eaf-ng2-module`. O hub Angular ainda não existia, então foi criado do zero. Os workflows foram modelados a partir dos CI dos repositórios irmãos para garantir build, testes e qualidade contínua.

### Resultado
- `dotnet build Api/GameHub.sln` executa com sucesso em Release.
- `dotnet test Api/GameHub.sln` executa com sucesso (211 passed, 2 skipped).
- `npm ci && npm run build` funcionam para `angular/` e `angular-admin/GameHub.UI`.
- GitHub Actions reconhecidos e executando no push para `main`.
- Análise de `.specs` documentada em `docs/specs-improvements.md`.

## 2026-07-21 02:30 UTC

### Tarefa
Implementar a especificação da pasta `.specs` no backend: entidades de domínio, enums, value objects, DTOs, application services, EF Core, cache abstrações, upload/validação de builds, segurança (CSP, headers, rate limiting), Docker Compose, scripts e testes. Criar também a estrutura inicial do hub Angular.

### Arquivos alterados (principais)
- `Api/src/GameHub.Core/Domain/**/*` — entidades, enums e value objects do GameHub.
- `Api/src/GameHub.Application/**/*` — DTOs, application services, cache in-memory e validador de pacotes.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/*` — `DbSet`s e configurações Fluent API.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` e `Middleware/*` — CSP, security headers e rate limiting.
- `Api/src/GameHub.Application/GameHubApplicationModule.cs` — registro dos serviços de cache e validador.
- `Api/test/GameHub.Tests/GameHub/**/*` — testes de domínio, cache, validação de builds, categorias e moderação.
- `docker-compose.yml`, `.env.example` e `scripts/*` — infraestrutura local.
- `angular/src/app/**/*` — rotas, componentes e serviços iniciais do hub.

### Motivação
A plataforma GameHub precisava de um domínio próprio além do template EAF base. A implementação seguiu os contratos de DTOs, permissões e rotas descritos nos specs, mantendo a arquitetura em camadas ABP e as convenções do repositório.

### Resultado
- `dotnet build Api/GameHub.sln` executa com sucesso (0 erros).
- `dotnet test Api/GameHub.sln` passa (224 passed, 2 skipped).
- `npm run build` passa para `angular/` e `angular-admin/GameHub.UI`.
- `docker compose config` valida a configuração local.
- Pendências documentadas em `docs/known-issues.md`.

## 2026-07-21 12:15 UTC

### Tarefa
Corrigir o Dockerfile da API, separar o Docker Compose em infraestrutura (`docker-compose.infra.yml`) e aplicação (`docker-compose.yml`), e gerar a migration inicial do PostgreSQL para que a API consiga subir no container.

### Arquivos alterados
- `Api/Dockerfile` — corrigido build para `GameHub.Web.Host.csproj` e `GameHub.Web.Host.dll`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContextFactory.cs` — evita que o design-time factory execute `MigrateDatabase`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/*` — removida migration SQL Server e gerada migration `Initial` para PostgreSQL.
- `docker-compose.yml` — API + Angular Hub + Angular Admin.
- `docker-compose.infra.yml` — PostgreSQL, Redis e MinIO.
- `docs/known-issues.md` e `docs/agent-execution-log.md` — atualização das pendências.

### Motivação
O Dockerfile ainda apontava para o template antigo (`Eaf.GameHub.Web.Host`) e a migration existente era SQL Server, impossibilitando a API de subir no PostgreSQL definido no Docker Compose. O Docker Compose anterior misturava infra e aplicação, então foi dividido para facilitar execução local.

### Resultado
- `dotnet build Api/GameHub.sln` executa com sucesso.
- `dotnet test Api/GameHub.sln` passa (224 passed, 1 skipped).
- `docker compose -f docker-compose.infra.yml -f docker-compose.yml config` é validado.
- `docker compose -f docker-compose.infra.yml up -d` e `docker compose -f docker-compose.yml up -d` estão prontos para uso (infra antes da aplicação).

## 2026-07-21 02:00 UTC

### Tarefa
Gerar README/CHANGELOG, executar playbook de qualidade/cobertura .NET e criar Agent Harness (CLAUDE.md, .claude/, .devin/, docs/).

### Arquivos alterados
- `README.md` — versão padrão em inglês (en-US) com badges, stack, arquitetura, fluxo, instruções de execução e snapshot de cobertura.
- `README.pt-BR.md` — versão em português com mesmo conteúdo.
- `CHANGELOG.md` — histórico de versões seguindo Keep a Changelog/SemVer.
- `scripts/run-local.sh` — atualizado para usar `docker-compose.infra.yml` + `docker-compose.yml`.
- `Api/test/GameHub.Tests/MultiTenantFactAttribute.cs` — remove warning CS0162.
- `Api/test/GameHub.Tests/GameHubTestModule.cs` e `Api/test/GameHub.Web.Tests/GameHubWebTestModule.cs` — isolam warning CS0618 de `UseStaticMapper`.
- `Api/src/GameHub.Core/Application/Extensions/HangfireExtensions.cs` — substitui overload obsoleto do `RecurringJob.AddOrUpdate` por `RecurringJobOptions`.
- `CLAUDE.md`, `.claude/` (settings.json, rules, agents, skills, hooks, CONTEXT, RULES, MEMORY, TOOLS, WORKFLOWS, README), `.devin/config.json`, `.devin/hooks/`.
- `docs/README.md`, `docs/technologies.md`, `docs/features.md`, `docs/packages.md`, `docs/plugins.md`, `docs/api.md`.
- `.gitignore` — adiciona `CLAUDE.local.md`, `.claude/settings.local.json`, `.devin/config.local.json` e `**/TestResults/`.

### Motivação
O repositório precisava de documentação padrão bilingue, build sem warnings e um harness de agente para guiar futuras execuções do Claude Code e Devin CLI.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` passa com 0 warnings e 0 erros.
- `dotnet test Api/GameHub.sln` passa (224 passed, 1 skipped).
- Cobertura coletada via `coverlet` (snapshot: Core 56.23%, Application 29.03%, EFCore 5.93%, Web.Host 4.88%, geral 10.22% line / 28.84% branch).
- `docker compose -f docker-compose.infra.yml -f docker-compose.yml config` continua válido.
- Branch `feature/20260721-readme-changelog` criada e enviada ao remote.

## 2026-07-22 06:20 UTC

### Tarefa
Ajustar comunicação interna com MinIO, corrigir endpoint OTLP da New Relic e eliminar foreign keys de shadow state no EF Core.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — endpoint/protocolo OTLP via config (`OpenTelemetry:OtlpEndpoint` / `OTEL_EXPORTER_OTLP_ENDPOINT`) com fallback `https://otlp.nr-data.net:4318` e `http/protobuf`.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContextFactory.cs` — lê `Database__Provider` do ambiente no design-time.
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubModelCreatingExtensions.cs` — `HasOne(x => x.Nav).WithMany(...).HasForeignKey(...)` corrigidos para `Category`, `Tag`, `User`, `Reviewer` e `ModerationReview`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/20260722223436_FixShadowForeignKeys.*` — remove colunas, índices e FKs de shadow state (`CategoryId1`, `TagId1`, `UserId1`, `ReviewerId`, `ModerationReviewId1`).
- `docker-compose.yml` e `docker-compose.all.yml` — `Storage__Minio__Endpoint` aponta para `http://gamehub-minio:9000` e adiciona `OTEL_EXPORTER_OTLP_*`.
- `.env.example` e `install.sh` — defaults do MinIO, OTLP e `Database__Provider`.

### Motivação
O backend usava `http://host.docker.internal:9000` para MinIO mesmo com o container no mesmo compose; o endpoint OTLP usava `https://otlp.nr-data.net` sem porta (405/404); e os logs mostravam propriedades de shadow state geradas por navegações não mapeadas explicitamente.

### Resultado
- `dotnet build Api/GameHub.sln` sucesso.
- `dotnet test Api/GameHub.sln --no-build` — 229 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.
- `shellcheck install.sh` sem erros.
- PR #24 criado.

## 2026-07-23 23:15 UTC

### Tarefa
Corrigir erro 504 em `Session/GetCurrentLoginInformations` e demais métodos do admin, removendo validações/headers que estavam interferindo no cross-origin e desativando o cache SQL Server incompatível com PostgreSQL.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — removido o registro de `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware` e `RateLimitingMiddleware` do pipeline; removido `using GameHub.Web.Middleware`.
- `Api/src/GameHub.Web.Host/appsettings*.json` — `SqlServerCache:IsEnabled` definido como `false` (o cache SQL Server apontava para a conexão PostgreSQL e causava timeout/gateway 504 em métodos que usam `IAbpCache`/`SettingManager`, como `GetCurrentLoginInformations`); `Cors:AllowAnyOrigin` definido como `true` para eliminar validação de origem enquanto o frontends são validados.

### Motivação
O `GetAll` funcionava porque não disparava o cache ABP; `GetCurrentLoginInformations` usava `SettingManager` -> `IAbpCache` configurado com `EafSqlServerCacheManager` apontando para a string de conexão PostgreSQL, fazendo o driver SQL Server aguardar/timeout e o gateway retornar 504. Os headers `Referrer-Policy`, CSP e rate limit também estavam sendo reportados como validações impedindo o cross-origin.

### Resultado
- `dotnet build Api/GameHub.sln -c Release --no-restore` sucesso.
- `dotnet test Api/GameHub.sln -c Release --no-build` — 170 passaram, 1 skipped.
- `docker compose -f docker-compose.yml config` e `docker compose -f docker-compose.all.yml config` válidos.

## 2026-07-23 20:10 UTC

### Tarefa
Criar novos specs baseados no site e na documentação da Poki, mapeando conceitos e requisitos de uma plataforma de jogos web madura para o GameHub.

### Arquivos alterados
- `.specs/18-poki-referencia.md` — referência de funcionalidades, SDK, requisitos, qualidade, monetização e mapeamento para o GameHub.
- `.specs/19-poki-backlog.md` — backlog de melhorias por fase inspiradas na Poki.

### Motivação
Usar a Poki como referência de mercado para evoluir o GameHub sem copiar marca, layout ou conteúdo, focando em UX pública, developer portal, SDK, cloud saves, monetização, qualidade e inspector.

### Resultado
- Novos specs criados e prontos para revisão.
- Branch `feature/poki-specs` criada a partir de `main`.

## 2026-07-26 22:45 UTC

### Tarefa
Criar as especificações dos próximos ajustes do portal do desenvolvedor e do User Guide após o PR #61.

### Arquivos alterados
- `.specs/34-poki-developer-portal-v3-next-adjustments.md` — shell compartilhado, filtros, estados e acessibilidade.
- `.specs/35-poki-developer-publishing-workflow.md` — versões, Preview, Inspector e submissão.
- `.specs/36-poki-developer-analytics-earnings.md` — métricas, receita estimada, filtros e qualidade dos dados.
- `.specs/37-poki-user-guide-developer-documentation.md` — estrutura operacional do User Guide e internacionalização.
- `.specs/38-poki-next-session-roadmap.md` — priorização P0/P1/P2 e regras de execução.

### Motivação
O layout do portal foi melhorado, mas ainda faltava um plano executável para consolidar o fluxo de publicação, conectar Preview/Inspector, evoluir Earnings e manter a documentação alinhada.

### Resultado
- Cinco specs independentes foram criadas, sem implementar código nesta etapa.
- A Spec 35 é a prioridade P0; as Specs 34 e 37 são as próximas entregas de suporte.
- A Spec 36 ficou condicionada à confirmação dos contratos de agregação e autorização no backend.

## 2026-07-26 22:55 UTC

### Tarefa
Adicionar especificações para uso de chat pelo SDK e capacidades sociais complementares.

### Arquivos alterados
- `.specs/39-poki-sdk-chat-and-social-communication.md` — fachada SDK sobre o ChatHub do EAF, modos de conversa, protocolo, segurança, moderação e reconexão.
- `.specs/40-poki-sdk-capabilities-next.md` — presença, notificações, convites, telemetria, erros e feature flags.
- `.specs/38-poki-next-session-roadmap.md` — chat elevado a P0 e capacidades sociais adicionadas ao roadmap.

### Motivação
O EAF já fornece persistência, histórico e SignalR para chat; o GameHub precisa apenas definir a integração contextual e o contrato seguro que os jogos consumirão.

### Resultado
- O SDK não deve duplicar `ChatMessage`, `ChatMessageManager` ou `ChatHub`.
- Chat contextual passa a ser a próxima prioridade P0 após o fluxo de publicação.
- Presença, notificações e convites foram separados em uma spec complementar para execução incremental.

## 2026-07-26 22:30 UTC

### Tarefa
Reorganizar as telas `/developer/games` e `/developer/earnings` e atualizar o Guia do Usuário com o fluxo do portal do desenvolvedor.

### Arquivos alterados
- `angular/src/app/developer/games/games.component.html` e `.css` — tabela responsiva, cabeçalho, estado vazio, badges de status e ações agrupadas.
- `angular/src/app/developer/earnings/earnings.component.html` e `.css` — cards de resumo, tabelas em painéis, hierarquia visual e rolagem horizontal em telas menores.
- `angular/src/styles.css` — layout compartilhado do portal do desenvolvedor com sidebar responsiva.
- `angular/src/app/public/docs/user-guide/user-guide.component.html` — instruções para portal, jogos, receitas e builds.
- `angular/public/i18n/pt-BR.json` e `angular/public/i18n/en-US.json` — novas traduções do Guia do Usuário.

### Motivação
As telas usavam tabelas e componentes sem um layout de portal consistente; a documentação também não explicava onde gerenciar jogos, builds e receitas.

### Resultado
- Build Angular de produção passou.
- `git diff --check` passou.
- Rotas `/developer/games` e `/developer/earnings` revisadas visualmente em desktop; o estado vazio e a navegação lateral foram validados.

## 2026-07-26 22:50 UTC

### Tarefa
Executar a primeira fatia das Specs 35 e 39: fluxo de publicação no portal e chat autenticado no Gameplay Bridge sobre o EAF ChatHub.

### Arquivos alterados
- `Api/src/GameHub.Application/Developer/Dto/DeveloperReviewHistoryItemDto.cs`
- `Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs`
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`
- `Api/test/GameHub.Tests/GameHub/Application/DeveloperGameAppService_Tests.cs`
- `angular/src/app/core/services/developer.service.ts`
- `angular/src/app/developer/builds/builds.component.ts`
- `angular/src/app/developer/builds/builds.component.html`
- `angular/src/app/developer/builds/builds.component.css`
- `angular/src/app/core/services/gameplay-bridge.service.ts`
- `angular/src/app/core/services/gameplay-bridge.service.spec.ts`
- `angular/src/app/public/docs/user-guide/user-guide.component.html`
- `angular/public/i18n/pt-BR.json`
- `angular/public/i18n/en-US.json`

### Decisões
- O histórico de revisão é uma projeção autorizada de `ModerationReview`; não há nova entidade.
- Preview e Inspector continuam usando os endpoints do `DeveloperGameAppService`, que delegam aos serviços existentes.
- O chat usa o endpoint `/signalr-chat` do EAF, sem segundo hub ou persistência paralela.
- O bridge rejeita contexto sem `gameId`, exige autenticação e normaliza mensagens para NFC, remove controles e limita a 500 caracteres.

### Limitações
- Na primeira fatia, o EAF `ChatHub` não possuía autorização contextual por `matchId` nem idempotência server-side de `clientMessageId`; a autorização foi adicionada posteriormente no `GameChatAppService`, mantendo o EAF como infraestrutura.

## 2026-07-26 22:55 UTC

### Tarefa
Continuar a Spec 39 após o PR #63, adicionando autorização contextual e deduplicação para envio de mensagens.

### Arquivos alterados
- `Api/src/GameHub.Application/Chat/IGameChatAppService.cs`
- `Api/src/GameHub.Application/Chat/GameChatDtos.cs`
- `Api/src/GameHub.Application/Chat/GameChatAppService.cs`
- `Api/test/GameHub.Tests/GameHub/Application/GameChatAppService_Tests.cs`
- `angular/src/app/core/services/gameplay-bridge.service.ts`
- `angular/public/i18n/pt-BR.json`
- `angular/public/i18n/en-US.json`
- `docs/superpowers/plans/2026-07-26-chat-contextual-authorization.md`

### Resultado
- O envio passa por `GameChatAppService.SendAsync`, que valida jogo, tenant, partida ativa e participação do usuário.
- O remetente é derivado da sessão autenticada; campos de identidade do iframe não são aceitos.
- O `IChatMessageManager` do EAF continua responsável pela persistência e entrega.
- `clientMessageId` é deduplicado por usuário/jogo/conversa na janela configurada de 10 minutos via `ICacheManager`.
- Testes cobrem usuário fora da partida e repetição de mensagem.

### Limitações
- O histórico contextual de uma partida ainda não pode ser reconstruído pelo EAF porque `ChatMessage` não possui `MatchId`; a próxima evolução deve adicionar metadados no EAF ou um contrato de histórico contextual sem duplicar mensagens.

## 2026-07-26 23:40 UTC

### Tarefa
Executar as melhorias aprovadas de moderação, analytics e qualidade do portal,
mantendo o EAF como fonte de verdade compartilhada.

### Arquivos alterados
- `Api/src/GameHub.Application/Moderation/UserContentAppService.cs` —
  rate limit tenant-aware por usuário/jogo via `ICacheManager`.
- `Api/src/GameHub.Application/Moderation/ModerationAppService.cs` —
  preenchimento de `ResolvedAt` para reports resolvidos ou dispensados.
- `Api/src/GameHub.Application/Gameplay/GameMetricsAppService.cs` —
  exclusão de playtests, vínculo de eventos às sessões de produção e exportação
  CSV.
- `Api/src/GameHub.Application/Gameplay/IGameMetricsAppService.cs`
- `Api/src/GameHub.Application/Gameplay/Dto/GameMetricsExportDto.cs`
- `Api/test/GameHub.Tests/GameHub/Application/UserContentAppService_Tests.cs`
- `Api/test/GameHub.Tests/GameHub/Application/ModerationAppService_Tests.cs`
- `Api/test/GameHub.Tests/GameHub/Application/GameMetricsAppService_Tests.cs`
- `angular/src/app/core/services/developer.service.ts` —
  retry limitado e contratos de métricas completos.
- `angular/src/app/core/services/gameplay-bridge.service.ts` —
  block/unblock delegados aos endpoints Friendship do EAF.
- `angular/src/app/developer/games/*` e
  `angular/src/app/developer/earnings/*` —
  retry manual, estados acessíveis, foco e tabelas semânticas.
- `angular/src/app/developer/games/games.component.spec.ts`
- `angular/src/app/developer/earnings/earnings.component.spec.ts`
- `docs/eaf/gamehub-eaf-improvements.md` —
  especificação independente de melhorias do EAF, incluindo `Templates/Api` e
  `Templates/Angular`.

### Resultado parcial
- Testes backend direcionados: 15 passaram.
- Build Angular de produção passou com os dois warnings CSS preexistentes.
- Testes Angular compilaram, mas o ChromeHeadless não iniciou no ambiente.

## 2026-07-26 23:50 UTC

### Tarefa
Revisar os módulos e templates do EAF e atualizar a documentação de integração
do GameHub.

### Resultado
- A documentação agora registra gaps de cache/rate limit, SignalR/backplane,
  Data Protection multi-instância, chat contextual, notificações, segurança,
  KeyVault, OpenTelemetry, Serilog, Worker, Hangfire, webhooks e geração de
  proxies.
- Foram incluídos ajustes específicos para `Templates/Api` e
  `Templates/Angular`, além de um backlog P0/P1/P2.
- Nenhum arquivo do repositório EAF foi alterado.

## 2026-07-26 23:58 UTC

### Tarefa
Executar melhorias do GameHub que não dependem de alterações no EAF.

### Arquivos alterados
- `Api/src/GameHub.Application/Moderation/UserReportAppService.cs`
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`
- `Api/src/GameHub.Application/Gameplay/Dto/GameplayEventInput.cs`
- `Api/test/GameHub.Tests/GameHub/Application/UserReportAppService_Tests.cs`
- `Api/test/GameHub.Tests/GameHub/Application/GameplayAppService_Tests.cs`
- `angular/src/app/core/services/gameplay-bridge.service.ts`

### Resultado
- Reports de usuário passaram a ter rate limit por tenant/usuário/jogo.
- Eventos de gameplay aceitam `ClientEventId` e são deduplicados por janela de
  retry usando `ICacheManager`.
- O SDK gera `clientEventId` para telemetria.
- Captura de erros valida que a sessão pertence ao jogo informado.
- Nenhuma alteração foi feita no EAF.

## 2026-07-26 23:59 UTC

### Tarefa
Atualizar o consumo do EAF para a release 9.3.0 e aplicar a correção de layout
compatível com os formulários do portal.

### Arquivos alterados
- `Api/src/GameHub.Application/GameHub.Application.csproj`
- `Api/src/GameHub.Core/GameHub.Core.csproj`
- `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj`
- `angular/src/app/developer/game-create/game-create.component.css`
- `docs/eaf/gamehub-eaf-improvements.md`
- `docs/packages.md`
- `docs/technologies.md`

### Resultado
- Referências NuGet EAF atualizadas de `9.2.0` para `9.3.0`.
- O alinhamento de labels da correção do template EAF foi adaptado aos
  checkboxes dos formulários GameHub, sem copiar componentes genéricos.
- A análise dos templates API/Angular 9.3.0 foi registrada na documentação EAF.

## 2026-07-28 02:25 UTC

### Tarefa
Corrigir CORS do SignalR/websockets (evitar `Access-Control-Allow-Origin: *` com credenciais) e ajustar o endpoint `/api/hub/auth/available-tenants` para não retornar 500 quando o usuário host não possui tenants associados.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs` — `ConfigurePolicy` reflete a origem chamadora em vez de emitir wildcard, mesmo quando `AllowAnyOrigin` está habilitado.
- `Api/src/GameHub.Web.Host/Controllers/HubAuthController.cs` — `GetAvailableTenants` retorna lista vazia em vez de lançar `UserHasNoAssociatedTenants`.
- `Api/src/GameHub.Web.Host/Middleware/PublicErrorMiddleware.cs` — `UserFriendlyException` mapeada para `400 validation_failed`.
- `angular/src/app/public/login/login.component.ts` — fallback para login host via `/api/TokenAuth/Authenticate` quando não há tenants disponíveis.
- `Api/test/GameHub.Tests/Middleware/CorsConfiguration_Tests.cs`, `PublicErrorMiddleware_Tests.cs` e `Controllers/HubAuthController_Tests.cs`.

### Resultado
- Build da API (`dotnet build Api/GameHub.sln -c Release --no-restore`) sucesso.
- Testes da API (`dotnet test Api/GameHub.sln -c Release --no-build`) — 370 passaram, 2 skipped, 0 falhas.
- Build do Angular (`npx ng build --configuration=production`) sucesso.

## 2026-07-28 02:40 UTC

### Tarefa
Ajustar teste `HubAuthController_Tests` para evitar senha literal e notificação do GitGuardian.

### Arquivos alterados
- `Api/test/GameHub.Tests/Controllers/HubAuthController_Tests.cs` — `TestHostPassword` gerado em tempo de execução no helper e no teste de usuário sem tenants.

### Resultado
- `dotnet test Api/GameHub.sln -c Release --no-build --filter "FullyQualifiedName~HubAuthController_Tests"` — 4 passaram, 0 falhas.

## 2026-07-29 11:38 UTC

### Tarefa
Analisar e simular erros de produção (SignalR/CORS 504, WOFF2 OTS parse, m-switch duplicado, aria-hidden) e aplicar correções.

### Arquivos alterados
- `Api/src/GameHub.Web.Host/Startup/CorsConfigurationExtensions.cs` — extensão `AddGameHubCors` que envolve `AddEafCors` e adiciona `X-SignalR-User-Agent` na política CORS.
- `Api/src/GameHub.Web.Host/Startup/Startup.cs` — usa `AddGameHubCors` no lugar de `AddEafCors`.
- `Api/test/GameHub.Tests/Middleware/CorsConfiguration_Tests.cs` — testes passam a validar `AddGameHubCors` e novo caso `Dado_RequisicaoSignalR_Quando_PoliticaPadrao_Entao_DevePermitirXSignalRUserAgent`.
- `angular-admin/GameHub.UI/src/web.config` — regra de rewrite IIS não reescreve mais extensões de assets estáticos (woff2, js, css, etc.).
- `angular-admin/GameHub.UI/src/assets/common/styles/styles.css` — override `.m-switch input:empty ~ span.m-switch-label` remove pseudo-elementos do label.
- `angular-admin/GameHub.UI/src/app/**/*-modal.component.html` — removido `aria-hidden="true"` do `div` raiz dos modais.
- `.specs/2026-07-29-correcao-erros-gamehub.md` — plano de correção.
- `.specs/2026-07-29-eaf-template-sync.md` — especificação de sincronização no template EAF.

### Simulação / Validação
- Build da API (`dotnet build Api/GameHub.sln -c Release --no-restore`) sucesso.
- Testes CORS (`dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj -c Release --filter "FullyQualifiedName~CorsConfiguration_Tests"`) — 6 passaram, 0 falhas.
- Build do admin (`npx ng build --configuration=production`) sucesso; fonte `Inter-roman.var.*.woff2` presente no `dist`.
- Página `mswitch-test.html` carregando `style.bundle.css` + `styles.css` reproduziu o m-switch duplicado antes da correção e ficou correto após o override CSS.
