# Agent Harness do GameHub

Este diretório contém o harness de agente LLM para o projeto GameHub, seguindo convenções de File-based Context (`CLAUDE.md`) e compatível com **Claude Code** e **Devin CLI**.

## Estrutura

```
.claude/
├── settings.json              # Permissões e hooks
├── hooks/                     # Scripts de guardrail
├── rules/
│   ├── global-rules.md        # Regras always-on
│   └── dotnet.md              # Regras path-scoped para .NET
├── agents/
│   ├── review.md              # Sub-agent de revisão
│   ├── plan.md                # Sub-agent de planejamento
│   └── test.md                # Sub-agent de testes
├── skills/
│   └── gamehub/SKILL.md       # Skill do domínio GameHub
├── CONTEXT.md                 # Estratégia de contexto
├── RULES.md                   # Guardrails
├── MEMORY.md                  # Estado cross-session
├── TOOLS.md                   # Ferramentas e MCP
└── WORKFLOWS.md               # Automação e verification loop
```

## Carregamento de Contexto

1. **Always-on**: `CLAUDE.md` + `.claude/rules/global-rules.md`.
2. **Path-scoped**: `.claude/rules/*.md` com `paths:` são carregadas quando arquivos correspondentes são acessados.
3. **On-demand**: skills em `.claude/skills/` e knowledge em `.claude/knowledge/`.

## Sub-agents

- `review`: revisa código e PRs.
- `plan`: cria planos de execução.
- `test`: cria/executa testes e cobertura.

## Configuração Devin CLI

O arquivo `.devin/config.json` habilita `read_config_from.claude: true`, importando rules, skills e sub-agents do Claude Code.
