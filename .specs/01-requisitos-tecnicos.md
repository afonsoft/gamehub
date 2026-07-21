# 01 - Requisitos Técnicos, Runtimes e Infraestrutura

## Stack principal

### Backend (API)

- .NET 10 LTS.
- ASP.NET Core Web API.
- Template legado AspZero/EAF como base.
- Entity Framework Core.
- AutoMapper.
- Hangfire para jobs assíncronos.
- Serilog para logs estruturados.
- OpenTelemetry para traces, métricas e logs correlacionáveis.

> **Não usar**: FluentValidation (validação nativa do ABP), MediatR (CQRS nativo do ABP).

### Frontend Game Hub (`angular/`)

- Angular 20+.
- TypeScript strict mode.
- RxJS.
- Angular Router.
- Angular Reactive Forms.
- Design system próprio (não Angular Material).
- Service Worker/PWA opcional para cache de assets não sensíveis.

### Frontend Admin (`angular-admin/`)

- Angular 20+.
- TypeScript strict mode.
- RxJS.
- Angular Router.
- Angular Reactive Forms.
- Design system próprio (reutilizar componentes do game hub quando aplicável).

### Banco de dados

Produção: PostgreSQL 16+ já gerenciado no servidor.

Desenvolvimento local: PostgreSQL em container Docker.

Opção A - PostgreSQL (recomendado):

- PostgreSQL 16+.
- Provider EF Core Npgsql.
- Bom para custo, escalabilidade, JSONB, full-text search e workloads cloud-native.

Opção B - SQL Server:

- SQL Server 2022+ ou Azure SQL.
- Provider EF Core SQL Server.
- Bom para compatibilidade enterprise e ecossistema Microsoft.

### Cache, filas leves e ranking

Produção: Redis 7+ já gerenciado no servidor.

Desenvolvimento local: Redis em container Docker.

Usos:
  - Cache de catálogo, home, categorias e trending.
  - Distributed cache do ABP/EAF.
  - Rate limiting.
  - Sessões temporárias de gameplay.
  - Leaderboards com Sorted Sets.
  - Locks distribuídos para processamento de builds.

### Storage de assets

- Storage compatível com S3/Azure Blob/MinIO.
- Separar buckets/containers:
  - game-builds-original
  - game-builds-public
  - thumbnails
  - screenshots
  - moderation-evidence
  - exports

### CDN e entrega

- CDN para assets estáticos e builds de jogos.
- Headers recomendados:
  - Cache-Control versionado para builds imutáveis.
  - Content-Security-Policy restritiva.
  - Cross-Origin-Resource-Policy conforme necessidade.
  - X-Content-Type-Options: nosniff.

## Runtimes necessários

### Desenvolvimento local

- Git
- .NET SDK 10
- Node.js LTS (20+)
- npm ou pnpm
- Docker Desktop ou Docker Engine
- PostgreSQL em container
- Redis em container
- Make, bash ou PowerShell

### Containers mínimos

- backend-api (ASP.NET Core)
- angular-hub (Game Hub)
- angular-admin (Admin)
- postgres
- redis
- object-storage opcional com MinIO
- jaeger/otel-collector opcional para traces locais

## Requisitos não funcionais

### Performance

- Home pública abaixo de 2s em conexão média com cache ativo.
- Catálogo paginado e indexado.
- Imagens com lazy loading nativo (`loading="lazy"`).
- Game iframe carregado sob demanda.
- Redis cache para rankings e consultas frequentes.

### Escalabilidade

- Backend stateless com cache distribuído via Redis.
- Upload/processamento de builds via jobs (Hangfire).
- Separar leitura pública de escrita administrativa.

### Segurança

- JWT/OAuth2/OIDC.
- RBAC por permissões ABP/EAF.
- CSP para jogos embarcados.
- Sandbox em iframe para execução de jogos.
- CORS configurado para os dois frontends.
- Antivirus/malware scanning no pipeline de upload quando disponível.
- Validação de pacote de build: tamanho, arquivos permitidos, presença de index.html, ausência de scripts externos não autorizados.

### Observabilidade

- CorrelationId por request.
- Logs estruturados por tenant, usuário, gameId e buildId.
- Métricas de gameplay, carregamento, erro e retenção.
- Tracing em API, jobs e banco.
- Health checks para API, banco e Redis.

### Compliance

- LGPD desde o início.
- Consentimento quando houver cookies/analytics/ads.
- Política de privacidade para jogadores e desenvolvedores.
- Retenção e exclusão de dados pessoais.

## Versões de pacotes

```text
| Pacote                                           | Versão mínima           |
|--------------------------------------------------|-------------------------|
| .NET SDK                                         | 10.0.x LTS              |
| Angular CLI                                      | 20+                     |
| Node.js                                          | 22 LTS                  |
| npm                                              | 10+                     |
| PostgreSQL                                       | 16                      |
| Redis                                            | 7                       |
| Entity Framework Core                            | 10.0.x                  |
| Npgsql.EntityFrameworkCore.PostgreSQL            | 10.0.x                  |
| Microsoft.EntityFrameworkCore.SqlServer          | 10.0.x                  |
| ABP (AspZero/EAF)                                | conforme template       |
| Hangfire                                         | conforme template       |
| Serilog                                          | conforme template       |
| xUnit                                            | 2.9.x                   |
| FluentAssertions                                 | 7.x                     |
```

> **Nota**: .NET 10 LTS — usar a versão GA quando disponível. Se ainda em preview, usar .NET 8 LTS como fallback.
