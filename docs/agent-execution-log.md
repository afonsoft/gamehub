# GameHub — Agent Execution Log

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
