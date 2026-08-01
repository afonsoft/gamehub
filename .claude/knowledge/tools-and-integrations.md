# TOOLS.md — GameHub

## Ferramentas Disponíveis

| Categoria | Tool | Uso |
|-----------|------|-----|
| Read | Read, Grep, Glob | Leitura de código, busca e listagem |
| Write | Edit, Write, MultiEdit | Modificação de arquivos |
| Exec | Bash/Exec | Build, testes, git, docker |
| Browser | Computer, Browser | Interações com UI e web |
| MCP | devin_mcp, mcp_tool | Integrações externas |

## Comandos Comuns

```bash
# Build e testes
dotnet build Api/GameHub.sln
dotnet test Api/GameHub.sln

# Qualidade
dotnet test Api/GameHub.sln --collect:"XPlat Code Coverage" --results-directory ./TestResults
docker compose -f docker-compose.infra.yml -f docker-compose.yml config

# Docker
docker compose -f docker-compose.infra.yml up -d
docker compose -f docker-compose.yml up --build -d

# Git
git checkout -b feature/{descricao-curta}
git add -A && git commit -m "type(scope): descricao"
```

## MCPs e APIs Externas

- `devin_mcp`: gestão de sessões, playbooks e knowledge.
- `mcp_tool`: integrações externas (Notion, Monday, SonarQube, etc.) conforme configurado.

## Políticas de Uso

- Read: livre para arquivos não bloqueados.
- Write: confirmar impacto antes de aplicar.
- Exec: build/testes em sandbox; evitar deploys para produção sem validação.
