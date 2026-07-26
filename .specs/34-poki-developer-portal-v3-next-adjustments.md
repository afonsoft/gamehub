# 34 — Portal do Desenvolvedor v3: próximos ajustes de UX e operação

> **Status:** Especificação para execução
> **Base:** PR #61 e `.specs/19.4-poki-developer-portal.md`
> **Objetivo:** transformar as telas atuais do portal em um fluxo operacional consistente, acessível e previsível em desktop e mobile.

## 1. Contexto

O portal já possui Dashboard, My Games, Builds, Earnings, Profile e Team. O PR #61
corrigiu a estrutura visual de `My Games` e `Earnings`, mas ainda existem diferenças
de comportamento entre telas: carregamento, erros, paginação, ações de build,
feedback de submissão e navegação mobile.

Esta spec é de UX e integração de frontend. Não deve duplicar regras de domínio nem
alterar contratos da API sem uma spec específica.

## 2. Escopo

### 2.1 Shell compartilhado

- Extrair o shell do portal para um componente compartilhado ou diretiva:
  - sidebar desktop;
  - navegação horizontal mobile;
  - título, descrição e ações da página;
  - estado de carregamento;
  - estado de erro com ação de retry.
- Preservar as rotas existentes:
  - `/developer`;
  - `/developer/games`;
  - `/developer/games/:id/builds`;
  - `/developer/games/:id/edit`;
  - `/developer/earnings`;
  - `/developer/profile`;
  - `/developer/team`.

### 2.2 My Games

- Adicionar filtros locais por status: `All`, `Draft`, `InReview`, `Published`,
  `Rejected`.
- Exibir a data completa e o status do último build.
- Desabilitar ações incompatíveis com o status do jogo.
- Substituir `alert()` por notificação visual reutilizável.
- Adicionar confirmação antes de enviar um jogo para revisão.
- Mostrar uma mensagem clara quando não houver jogos após aplicar um filtro.

### 2.3 Earnings

- Adicionar filtros de período (`from`/`to`) usando o contrato existente
  `DeveloperEarningsFilter`.
- Exibir o período consultado e o estado de atualização.
- Permitir expandir/recolher o detalhamento diário por jogo.
- Formatar moeda, data e números conforme o idioma selecionado.
- Exibir estado de erro sem apagar os dados carregados anteriormente.

### 2.4 Builds

- Aplicar o mesmo shell e a mesma tabela responsiva de `My Games`.
- Separar visualmente upload, validação e histórico.
- Desabilitar aprovação/rejeição enquanto uma operação estiver em andamento.
- Exibir ações de preview e Inspector apenas quando houver build compatível.

## 3. Acessibilidade e responsividade

- Todos os botões devem possuir texto ou `aria-label`.
- Foco de teclado deve permanecer visível.
- Tabelas devem ter cabeçalhos semânticos e rolagem horizontal no mobile.
- A navegação mobile não pode depender somente de hover.
- Contraste mínimo WCAG AA para texto e estados.

## 4. Critérios de aceite

- As telas usam o mesmo shell sem duplicar markup de sidebar.
- Filtros não fazem chamadas extras quando a operação é local.
- Filtros de Earnings fazem uma chamada com `from` e `to` válidos.
- Erros de rede exibem retry e não deixam a tela vazia silenciosamente.
- `npm run build` passa para `angular/`.
- Testes de componente cobrem:
  - filtro por status;
  - confirmação de submissão;
  - retry após erro;
  - expansão do detalhamento de Earnings.
- Teste manual em viewport desktop e largura inferior a 800px.

## 5. Fora do escopo

- Alterar regras de split, CPM, payout ou contratos.
- Criar novos endpoints de métricas.
- Implementar billing ou pagamentos reais.
- Alterar o layout do portal administrativo.
