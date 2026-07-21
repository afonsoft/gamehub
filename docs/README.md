# Documentação do Sistema — GameHub

## Visão Geral

O GameHub é uma plataforma de catálogo, execução e gestão de jogos HTML5/WebGL, construída sobre EAF/ABP (.NET 10), Angular 20+ e PostgreSQL/Redis.

## Arquitetura

- **Camadas**: Domain (`Core`) → Application → Infrastructure (`EntityFrameworkCore`, `Web`) → Presentation (`Web.Host`).
- **Frontends**: Game Hub público (`angular/`) e Admin (`angular-admin/GameHub.UI/`).
- **Infraestrutura**: PostgreSQL (dados), Redis (cache/leaderboards/rate limit), MinIO/S3 (builds).
- **Observabilidade**: Serilog, OpenTelemetry, CorrelationId.

## Estrutura de Diretórios

```
docs/
├── README.md                  # Este arquivo
├── agent-execution-log.md     # Registro de execuções do agente
├── known-issues.md            # Problemas e soluções conhecidas
├── technologies.md            # Stack e versões
├── features.md               # Funcionalidades do sistema
├── packages.md               # Dependências NuGet/NPM
└── specs-improvements.md     # Melhorias identificadas nas specs
```

## Início Rápido

1. Copiar `.env.example` para `.env`.
2. Subir infra: `docker compose -f docker-compose.infra.yml up -d`.
3. Subir app: `docker compose -f docker-compose.yml up --build -d`.
4. API: http://localhost:5000, Hub: http://localhost:4200, Admin: http://localhost:4201.

## Referências

- [README en-US](../README.md)
- [README pt-BR](../README.pt-BR.md)
- [CHANGELOG](../CHANGELOG.md)
- [AGENTS.md](../AGENTS.md)
- [CLAUDE.md](../CLAUDE.md)
- [.specs/](../.specs/)
