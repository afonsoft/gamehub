# Plugins e Extensões — GameHub

## Plugins de Build

Nenhum plugin customizado de build no momento. O build é feito via:

- `dotnet build` / `dotnet publish` para o backend.
- `ng build` para os frontends Angular.
- Docker e Docker Compose para ambientes local e produção.

## Extensões do ABP

- `Abp.AutoMapper`
- `Abp.ZeroCore.EntityFrameworkCore`
- `Abp.Hangfire`
- `Abp.RedisCache`

## Integrações Planejadas

- MinIO/S3 para armazenamento de builds.
- Providers de anúncios (monetização).
- OpenTelemetry collectors.
