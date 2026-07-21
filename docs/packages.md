# Pacotes e Dependências — GameHub

## Backend (NuGet)

Principais pacotes definidos em `common.props` e `.csproj`:

- `Eaf.Middleware.*` 9.2.0 — camadas EAF/ABP.
- `Abp.*` 10.4.0 — framework ABP.
- `Microsoft.EntityFrameworkCore.*` 10.0.8 — EF Core.
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.0 — provider PostgreSQL.
- `AutoMapper` 14.0.0 — mapeamento.
- `Hangfire.*` 1.8.x — background jobs.
- `Serilog` 4.x — logging estruturado.
- `OpenTelemetry.*` 1.x — observabilidade.
- `xunit`, `Shouldly`, `NSubstitute` — testes.
- `coverlet.collector` 10.0.0 — cobertura.

## Frontend (NPM)

- `angular/*` 20+ — framework SPA.
- `rxjs` 7.x — programação reativa.
- `typescript` 5.x — linguagem com strict mode.
- `primeflex`, `primeicons`, `primeng` — Admin UI.
- `bootstrap` — Admin styles.

Para listas completas, consulte:

- `angular/package.json`
- `angular-admin/GameHub.UI/package.json`
- `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj`
