# Changelog — EAF Improvements v2.0

> Data: 06/06/2026
> Branch: `feature/eaf-improvements` (merged em `main` via PR #61)

Este documento descreve todas as melhorias implementadas no EAF, organizadas por fase e spec.

---

## Fase 1 & 4 — Performance Backend (Specs 01–05, 12–14)

### Spec 01-02: Remoção do BinaryFormatter

**Problema:** `BinaryFormatter` é obsoleto, inseguro (vulnerabilidade de desserialização) e removido no .NET 8+.

**Solução:**
- Substituído por `ExtendedXmlSerializer` (primário) + `System.Text.Json` (fallback)
- Afeta: `EafSqlServerCache`, `EafSqliteCache`
- Serialização XML para tipos simples, JSON para tipos complexos

**Arquivos alterados:**
- `src/Eaf.SqlServerCache/Runtime/Caching/SqlServer/EafSqlServerCache.cs`
- `src/Eaf.SqliteCache/Runtime/Caching/Sqlite/EafSqliteCache.cs`

---

### Spec 03: Correção de Fire-and-Forget no Cache

**Problema:** `EafSqlServerCache.Set()` chamava operações async sem await, causando falhas silenciosas.

**Solução:**
- Adicionado `.GetAwaiter().GetResult()` nas chamadas async dentro de `Set()`
- Documentado que sync-over-async é necessário pois `CacheBase` do ABP não define `SetAsync`

**Arquivo alterado:**
- `src/Eaf.SqlServerCache/Runtime/Caching/SqlServer/EafSqlServerCache.cs`

---

### Spec 04: Batch Delete no Audit Log Worker

**Problema:** Loop `foreach` com `DeleteAsync` individual — N operações de I/O para N registros.

**Solução:**
- Substituído por operação batch usando `DeleteManyAsync` ou query direta

**Arquivo alterado:**
- `src/Eaf.Middleware.Application/Auditing/` (ExpiredAuditLogDeleterWorker)

---

### Spec 05: IHttpClientFactory nos Auth Providers

**Problema:** `new HttpClient()` direto causa socket exhaustion e não respeita DNS TTL.

**Solução:**
- Injetado `IHttpClientFactory` via construtor
- Criação de clients via `_httpClientFactory.CreateClient("ExternalAuth")`

**Arquivos alterados:**
- `src/Eaf.Middleware.Core/Authorization/External/Google/GoogleAuthProviderApi.cs`
- `src/Eaf.Middleware.Core/Authorization/External/Microsoft/MicrosoftAuthProviderApi.cs`
- `src/Eaf.Middleware.Core/Authorization/External/AuthZero/AuthZeroAuthProviderApi.cs`

---

### Spec 12: Response Compression (Brotli + Gzip)

**Problema:** Respostas HTTP sem compressão aumentam latência e consumo de banda.

**Solução:**
- Adicionado middleware `UseResponseCompression()` com Brotli (primário) e Gzip (fallback)
- Configurado no `Startup.cs` do template API

**Arquivo alterado:**
- `Templates/Api/src/Eaf.ProjectName.Web.Host/Startup/Startup.cs`

---

### Spec 13: AsNoTracking para Queries Read-Only

**Problema:** EF Core rastreia todas as entidades por padrão, consumindo memória desnecessariamente em leituras.

**Solução:**
- Adicionado `.AsNoTracking()` em queries que apenas leem dados (GET endpoints)

**Arquivo alterado:**
- `Templates/Api/src/Eaf.ProjectName.Application/Airplanes/AirplanesAppService.cs`

---

### Spec 14: Correções Diversas de Performance

**Problema:** Métodos retornando `Task.FromResult(0)` ao invés de `Task.CompletedTask`, sync wrappers desnecessários, chamadas async não aguardadas.

**Solução:**
- Substituído `Task.FromResult(0)` por `Task.CompletedTask`
- Marcados métodos sync obsoletos com `[Obsolete]`
- Corrigidas chamadas async não aguardadas

---

## Fase 2 — Suporte Multi-Database (Specs 06–08)

### Spec 06: Provider Switch no DbContextConfigurer

**Problema:** `DbContextConfigurer` hardcoded para SQL Server apenas.

**Solução:**
- Implementado switch baseado em `Database:Provider` da configuração
- Suporta: `SqlServer` (padrão), `PostgreSQL`, `MySQL`

**Arquivo alterado:**
- `Templates/Api/src/Eaf.ProjectName.EntityFrameworkCore/EntityFrameworkCore/ProjectNameDbContextConfigurer.cs`

---

### Spec 07: Pacotes NuGet para PostgreSQL e MySQL

**Problema:** Apenas pacotes SQL Server disponíveis.

**Solução:**
- Adicionado `Npgsql.EntityFrameworkCore.PostgreSQL`
- Adicionado `Pomelo.EntityFrameworkCore.MySql`

**Arquivo alterado:**
- `Templates/Api/src/Eaf.ProjectName.EntityFrameworkCore/Eaf.ProjectName.EntityFrameworkCore.csproj`

---

### Spec 08: Column Types Provider-Aware

**Problema:** Configurações de colunas usavam tipos específicos de SQL Server (`nvarchar(max)`, etc.).

**Solução:**
- Warnings condicionais de SQL Server (apenas aplicados quando provider é SqlServer)
- Column types adaptados por provider

**Arquivo alterado:**
- `Templates/Api/src/Eaf.ProjectName.EntityFrameworkCore/EntityFrameworkCore/ProjectNameDbContext.cs`
- `Templates/Api/src/Eaf.ProjectName.EntityFrameworkCore/EntityFrameworkCore/ProjectNameDbContextFactory.cs`

---

## Fase 3 — Performance Angular (Specs 09–11)

### Spec 09: Subscription Cleanup com takeUntilDestroyed

**Problema:** `router.events.subscribe()` sem cleanup causa memory leaks.

**Solução:**
- Adicionado `takeUntilDestroyed()` (Angular 16+) em todas as subscrições de router

**Arquivos alterados:**
- `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/layout/nav/side-bar-menu.component.ts`
- `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/layout/nav/top-bar-menu.component.ts`

---

### Spec 10: Lazy Loading e Bundle Budgets

**Problema:** Módulo admin pré-carregado desnecessariamente, sem controle de tamanho de bundle.

**Solução:**
- Removida estratégia de preload para módulos admin
- Adicionados budgets no `angular.json`: warning 5MB, error 8MB

**Arquivos alterados:**
- `Templates/Angular/Eaf.ProjectName.UI/src/app/app-routing.module.ts`
- `Templates/Angular/Eaf.ProjectName.UI/angular.json`

---

### Spec 11: ChangeDetectionStrategy.OnPush

**Problema:** Componentes stateless usando change detection padrão (verificação em todo ciclo).

**Solução:**
- Aplicado `ChangeDetectionStrategy.OnPush` em 9 componentes stateless:
  - `timezone-combo`, `top-bar-menu`, `side-bar-menu`
  - `permission-combo`, `role-combo`
  - `default-theme-ui-settings`, `theme2-theme-ui-settings`, `theme3-theme-ui-settings`, `theme4-theme-ui-settings`

---

## Fase 5 — Refatoração SOLID (Specs 80–86)

### Spec 86: Error Handling no ServiceBusQueueAppender

**Problema:**
1. Fire-and-forget: `queueClient.SendAsync(messages)` sem await
2. Exception swallowing: `catch (Exception) { //bypass }`
3. `async void` anti-pattern no `OnClose()`
4. `Task.Run(() => AppendBuffer(events))` — thread pool desnecessário

**Solução:**
1. Adicionado `.GetAwaiter().GetResult()` após `SendAsync`
2. Exception handling específico com log
3. Convertido para `void` com `.GetAwaiter().GetResult()` na chamada interna
4. Chamada direta `AppendBuffer(events)` sem `Task.Run`

**Arquivo alterado:**
- `src/Eaf.Log4NetServiceBus/Logging/ServiceBusQueueAppender.cs`

---

### Spec 83: Interface Segregation — IEafWorkerBase

**Problema:** `IEafWorkerBase` expunha `IIocManager` — forçava dependências desnecessárias nos consumidores.

**Solução:**
- Removido `IIocManager IocManager { get; set; }` da interface pública
- Property mantida apenas na classe base `EafWorkerBase`

**Arquivo alterado:**
- `src/Eaf.Middleware.Worker/IEafWorkerBase.cs`

---

### Spec 80: Remoção do Service Locator

**Problema:** `IocManager.Instance.Resolve<T>()` viola DIP (Dependency Inversion Principle) e dificulta testes.

**Solução:**
- `EafWorkerBase`: Removido `SetDependencies()` — confia no property injection do Castle Windsor
- `KeyVaultSecretManager`: Usa `KeyVaultManagerFactory` internamente
- `TokenAuthController`: Injetado `IPrincipalAccessor` via construtor

**Arquivos alterados:**
- `src/Eaf.Middleware.Worker/EafWorkerBase.cs`
- `src/Eaf.KeyVault/KeyVaultSecretManager.cs`
- `src/Eaf.Middleware.Web.Core/Controllers/TokenAuthController.cs`

---

### Spec 84: Factory Pattern — KeyVaultManagerFactory

**Problema:** `KeyVaultSecretManager` tinha lógica de criação inline (if/else para provider) — viola OCP.

**Solução:**
- Extraído `IKeyVaultManagerFactory` + `KeyVaultManagerFactory`
- Switch expression: `Azure → AzureKeyVaultManager`, `OCI → OCIKeyVaultManager`, `_ → NullKeyVaultManager`
- Null Object Pattern para provider desconhecido

**Arquivos criados:**
- `src/Eaf.KeyVault/KeyVault/IKeyVaultManagerFactory.cs`
- `src/Eaf.KeyVault/KeyVault/KeyVaultManagerFactory.cs`

---

### Spec 85: Interface de Serialização — ICacheSerializer

**Problema:** Lógica de serialização acoplada diretamente nas classes de cache — viola OCP.

**Solução:**
- Extraída interface `ICacheSerializer` com `Serialize(object)` e `Deserialize(byte[])`
- Implementação `JsonCacheSerializer` usando `System.Text.Json`

**Arquivos criados:**
- `src/Eaf.SqlServerCache/Serialization/ICacheSerializer.cs`
- `src/Eaf.SqlServerCache/Serialization/JsonCacheSerializer.cs`

---

### Spec 81: SRP — Extração de Configurers do MiddlewareWebCoreModule

**Problema:** `MiddlewareWebCoreModule.PreInitialize()` tinha 300+ linhas com múltiplas responsabilidades.

**Solução:**
- `CacheConfigurer`: Configura Redis e SQL Server cache
- `AuditConfigurer`: Configura auditoria e histórico de entidades
- `ExternalAuthConfigurer`: Registra providers de autenticação externa
- Module delega para configurers ao invés de config inline

**Arquivos criados:**
- `src/Eaf.Middleware.Web.Core/Configuration/CacheConfigurer.cs`
- `src/Eaf.Middleware.Web.Core/Configuration/AuditConfigurer.cs`
- `src/Eaf.Middleware.Web.Core/Configuration/ExternalAuthConfigurer.cs`

---

### Spec 82: Interfaces para Decomposição do TokenAuthController

**Problema:** `TokenAuthController` com 1215 linhas e 22 dependências — viola SRP massivamente.

**Solução (fundacional):**
- Criadas interfaces de serviço para decomposição incremental futura:
  - `ITokenAuthenticationService` — autenticação local (login, token, refresh)
  - `IExternalAuthenticationService` — autenticação via providers externos
  - `IImpersonationService` — impersonação de usuários e tenants

**Nota:** A extração completa do controller (de 1215 para ≤300 linhas) foi deferida por complexidade MUITO ALTA. As interfaces fornecem a base SRP para extração incremental em iterações futuras.

**Arquivos criados:**
- `src/Eaf.Middleware.Web.Core/Authentication/ITokenAuthenticationService.cs`
- `src/Eaf.Middleware.Web.Core/Authentication/IExternalAuthenticationService.cs`
- `src/Eaf.Middleware.Web.Core/Authentication/IImpersonationService.cs`

---

## Verificação Final

| Verificação | Resultado |
|-------------|-----------|
| `dotnet build Eaf.sln --configuration Release` | Build succeeded |
| `dotnet test Eaf.sln` | 1159 testes, 0 falhas |
| `grep -r "BinaryFormatter" src/` | 0 ocorrências |
| `grep -r "new HttpClient()" src/` | 0 ocorrências |
| `ng build --configuration=production` | Build succeeded |
| `ng test --browsers=ChromeHeadlessNoSandbox` | 222 testes, 0 falhas |

---

## Complexidade e Notas

| Spec | Complexidade | Nota |
|------|-------------|------|
| 82 | MUITO ALTA | Interfaces criadas; extração completa do controller deferida para iteração futura |
| 81 | ALTA | 3 configurers extraídos; Hangfire e AppFolders mantidos inline |
| 80 | ALTA | 3 classes refatoradas para constructor DI |
| Demais | MÉDIA/BAIXA | Executadas sem impedimentos |
