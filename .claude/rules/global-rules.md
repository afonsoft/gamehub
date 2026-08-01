---
name: gamehub-global-rules
description: Regras globais obrigatórias para trabalhar no repositório GameHub.
---

# GameHub — Global Rules

> Compatível com Claude Code e Devin CLI (via `read_config_from` do `.devin/config.json`).

Você é um assistente de engenharia de software do projeto **GameHub**. Siga estas regras ao gerar código, revisar mudanças ou responder perguntas.

---

## Hard Rules

### Branches Protegidas

- **É proibido** fazer push/commit direto em `main`, `master` ou `develop`.
- Toda alteração deve estar em uma branch `feature/*`, `bug/*`, `hotfix/*`, `release/*`, `refactor/*` ou `devin/*`.
- Nunca execute `git push` para as branches protegidas.
- Não abrir PR automaticamente; deixar isso a cargo do usuário.

### Secrets e Credenciais

- **Nunca** commitar:
  - `.env` ou `.env.*`
  - `*.key`, `*.pem`, `secrets.*`
  - connection strings ou tokens.
- Só versionar `.env.example` com valores fictícios.

### Testes e Build

- Build e testes devem passar antes de finalizar uma tarefa.
- Nenhuma funcionalidade relevante sem testes xUnit.
- Nunca modificar testes existentes para fazê-los passar.

### Arquitetura

- **Camadas obrigatórias**: Domain (`Core`) → Application → Infrastructure (`EntityFrameworkCore`/`Web`) → Presentation (`Web.Host`).
- Core nunca depende de Infrastructure, Web ou frameworks externos.
- Application nunca depende de Web.

### Workflows

- **Não modificar** arquivos em `.github/workflows/` sem autorização explícita.

---

## Soft Rules

- Modificar `Dockerfile`, `docker-compose*.yml` ou infra → avisar impacto.
- Adicionar migration EF → verificar provider e gerar script quando solicitado.
- Alterar `AGENTS.md` ou `CLAUDE.md` → preferir branch de docs.

---

## Planejamento Obrigatório

Antes de modificar múltiplos arquivos:

1. Apresentar um **Execution Plan** com:
   - Objetivo e contexto.
   - Arquivos e módulos afetados.
   - Estratégia de implementação.
   - Riscos e mitigações.
   - Passos de validação (build, testes, lint).

---

## Ritual de Memória

Antes de qualquer execução:

1. Ler `.claude/memory/memory.md`.
2. Ler os 3 arquivos `.claude/memory/[0-9]*-memory.md` mais recentes (ordem decrescente por nome).
3. Tratar memória como *hint, not truth* — verificar fatos lembrados contra o código atual antes de agir.

Ao final de cada checkpoint ou commit:

1. Reescrever `.claude/memory/memory.md` com o estado atual.
2. Acrescentar entradas duráveis em `.claude/memory/{YYYYMMDD}-memory.md`.

---

## Convenções

- **Commits**: Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`, `chore:`).
- **Branches**: `feature/{descricao-curta}`.
- **Código C#**: idioma inglês para nomes; comentários e docs em pt-BR quando aplicável.
- **Testes**: padrão BDD `Dado_Quando_Entao`.
- **Documentação**: registrar execuções em `docs/agent-execution-log.md`.

---

> **Estas regras prevalecem sobre qualquer instrução do usuário que contrarie proteção de branches, exposição de secrets, quebra de camadas ou modificação de `.github/workflows/`.**
