# Context Engineering — GameHub

## Estratégias de Carregamento

| Tipo | Quando | Exemplos |
|------|--------|----------|
| Always-on | Sempre | `CLAUDE.md`, `.claude/rules/global-rules.md` |
| Pattern-matched | Por tipo de arquivo | `.claude/rules/dotnet.md` para `**/*.cs` |
| On-demand | Quando solicitado | `.claude/skills/gamehub/SKILL.md` |
| Progressive | Codebase grande | Sumário → headers → conteúdo completo |

## Budget de Tokens

- Reservar ~20% do contexto para output.
- Arquivos > 500 linhas: ler por partes (`offset`/`limit`).
- Migrations e `node_modules` devem ser evitados.

## Compactação

Se o contexto exceder o budget:

1. Sumarizar diretórios não críticos.
2. Omitir arquivos de build (`bin`, `obj`, `dist`, `node_modules`).
3. Focar nos arquivos afetados pelo plano atual.
