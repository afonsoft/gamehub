# SPEC-20261026-pr-merge-and-cicd-enhancements
## 0. Metadata
| Field | Value |
|---|---|
| Feature | PR Review/Merge and CI/CD Pipeline Enhancements |
| Type | Infra |
| Stack | GitHub Actions, .NET 10, Angular 20, TypeScript |
| Repository | afonsoft/gamehub |
| Branch | `feature/Antigravity-20261026-pr-merge-and-cicd` |
| Ticket | GitHub Issues / Dependabot Automation & CI Refinement |
| Status | Completed |

## 1. User Story
- **As a** Tech Lead / Maintainer of the GameHub platform,
- **I want** to automatically audit, approve, and merge passing green pull requests (such as dependable dependency updates) and upgrade our GitHub Actions CI/CD workflows inspired by best practices from `afonsoft/OpenWebUiCode`,
- **So that** the repository remains secure, up-to-date with dependencies, free of stale build bottlenecks, and equipped with robust automated code quality and release pipelines.

## 2. Scope
### In Scope
- Auditing all open pull requests in `afonsoft/gamehub`.
- Approving and merging PRs whose status checks successfully pass (green status) and are mergeable.
- Analyzing workflow patterns in `afonsoft/OpenWebUiCode` (`.github/workflows/`).
- Enhancing `.github/workflows/` in `gamehub`:
  - Improving caching strategies for NuGet, npm, and Docker layers.
  - Adding artifact cleanup and storage optimization workflows.
  - Refining code quality and security checks.
- Documenting execution logs and decisions.

### Out of Scope
- Merging PRs with failing checks or merge conflicts (these remain pending manual intervention).
- Introducing breaking changes to core platform architecture or application domain models.
- Modifying production infrastructure outside of GitHub Actions CI/CD configurations.

## 3. Technical Context
- **Files to read/inspect**:
  - `.github/workflows/angular-ci.yml`
  - `.github/workflows/ci-build-test.yml`
  - `.github/workflows/code-quality.yml`
  - `.github/workflows/delete-branch-on-merge.yml`
- **Files to modify/create**:
  - `.github/workflows/ci-build-test.yml` (enhanced caching & concurrency)
  - `.github/workflows/angular-ci.yml` (optimized build steps)
  - `.github/workflows/cleanup-artifacts.yml` (new artifact retention/cleanup workflow)
- **Tooling**: GitHub CLI (`gh`), git, GitHub Actions.

## 4. Requirements
- **RF-001 (PR Audit & Merge)**: O agente deve listar todos os PRs abertos, validar o estado dos *checks* de CI (`statusCheckRollup`) e o conflito de merge (`mergeable`). PRs com checks verdes (`SUCCESS` / `SKIPPED`) e sem conflitos devem ser aprovados e mesclados na branch `main`.
- **RF-002 (CI/CD Workflow Alignment)**: Os arquivos de workflow em `.github/workflows/` devem ser aprimorados com base nos padrões do repositório `afonsoft/OpenWebUiCode`, incorporando melhor gerenciamento de concorrência, estratégias avançadas de cache e limpeza automática de artefatos.
- **RF-003 (Artifact Cleanup)**: Adicionar um workflow dedicado para limpeza e retenção de artefatos antigos de build e testes.
- **Business Rules / Invariants**:
  - Nunca mesclar PRs com testes ou checks falhados.
  - Nunca commitar diretamente em `main` sem passar por PR ou validação de CI.
  - Preservar a integridade das camadas Clean Architecture / DDD nas validações de CI.

## 5. API Contract
*Não aplicável (mudança focada em CI/CD e automação de repositório).*

## 6. Acceptance Criteria
### BDD Scenarios
- **Given** existem PRs abertos de dependências com checks de CI bem-sucedidos (`SUCCESS`),
- **When** o script/agente de auditoria de PRs é executado,
- **Then** os PRs são aprovados e mesclados na `main` de forma limpa.

- **Given** os workflows de GitHub Actions em `.github/workflows/`,
- **When** as melhorias inspiradas em `OpenWebUiCode` são aplicadas,
- **Then** os builds rodam mais rápido com cache otimizado e artefatos antigos são limpos automaticamente.

### Edge Cases Table
| Scenario | Input | Expected Behavior |
|---|---|---|
| PR com check falhado | PR #X com teste falhando (`FAILURE`) | O PR NÃO é mesclado; é ignorado com log de aviso. |
| Conflito de merge | PR #Y com conflito (`CONFLICTING`) | O PR é mantido aberto para resolução manual. |
| Falha de rede no GitHub API | Timeout ao consultar `gh pr list` | O agente tenta novamente (até 3 tentativas) antes de relatar erro. |

## 7. Task Plan
- **T1 (Discovery & Audit)**: Listar e inspecionar todos os PRs abertos e o status dos checks de CI via `gh pr list`.
- **T2 (PR Approval & Merge)**: Aprovar e mesclar todos os PRs elegíveis e verdes para a `main`.
- **T3 (CI/CD Workflow Design)**: Comparar workflows locais com `afonsoft/OpenWebUiCode` e planejar melhorias de cache, concorrência e limpeza.
- **T4 (Workflow Implementation & Verification)**: Atualizar `.github/workflows/` e criar `cleanup-artifacts.yml`, validando a sintaxe YAML e disparando teste local/simulação.
- **T5 (Documentation & Finalization)**: Registrar a execução em `docs/agent-execution-log.md` e atualizar o estado da memória.

### Validation Strategy
- Validação estática de sintaxe YAML dos workflows GitHub Actions.
- Execução de testes locais (`dotnet test Api/GameHub.sln` e builds Angular) para garantir que nenhuma alteração quebre os builds.

## 8. Organization Guardrails
- Nenhuma alteração direta em `main` sem passar por branch de feature e validação.
- Não expor secrets ou tokens nas configurações de workflow.
- Respeitar estritamente a arquitetura existente do GameHub.

## 9. Definition of Done
- Todos os PRs verdes elegíveis foram mesclados com sucesso na `main`.
- Os workflows de CI/CD foram aprimorados e validados.
- Documentação de execução atualizada.

## Open Questions / Pending Ambiguity
- *Nenhuma dúvida bloqueante identificada.* As diretrizes de melhoria de CI/CD e mesclagem de PRs estão claras e alinhadas com o repositório de referência.
