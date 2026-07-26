# 35 — Fluxo de publicação, versões, preview e Inspector

> **Status:** Especificação para execução
> **Base:** `.specs/22-poki-proxima-fase.md`, `.specs/24-poki-proxima-fase.md` e `.specs/26-poki-proxima-fase.md`
> **Objetivo:** conectar o ciclo `draft → build → validation → preview → review → publish` em um fluxo único para o desenvolvedor.

## 1. Problema

Hoje o desenvolvedor navega entre jogos, builds, edição e revisão, mas não possui
uma visão única de versão nem ações consistentes para verificar um build antes da
submissão. A plataforma já possui contratos de upload, validação, preview e
Inspector que devem ser reutilizados.

## 2. Fluxo alvo

```text
Create draft
  → Edit metadata/assets
  → Upload version
  → Validate package
  → Preview version
  → Open Inspector
  → Approve build
  → Submit for review
  → Review feedback
  → Publish
```

## 3. Requisitos funcionais

### 3.1 Versões

- Criar uma aba ou seção `Versions` por jogo.
- Listar todas as versões com:
  - versão;
  - build number;
  - status;
  - tamanho;
  - data de upload;
  - data de publicação;
  - resultado da validação.
- Destacar a versão ativa/publicada.
- Permitir repetir upload sem perder o histórico anterior.

### 3.2 Preview

- Reutilizar `GamePreviewAppService.CreatePreviewTokenAsync`.
- Abrir `/preview/:gameSlug/:version?token=...` em nova aba.
- O token deve ser curto, vinculado ao jogo/build e expirar.
- A UI deve informar que preview não publica o jogo.
- Não expor tokens em logs, analytics ou mensagens de erro.

### 3.3 Inspector

- Reutilizar a sessão existente do Inspector.
- Permitir abrir o build selecionado diretamente no Inspector.
- Exibir a última validação e warnings antes de submeter.
- Permitir executar novamente a validação sem criar uma versão duplicada.

### 3.4 Feedback da revisão

- Mostrar histórico de decisões e pedidos de alteração.
- Associar cada feedback ao jogo e, quando possível, ao build.
- Permitir nova submissão somente quando os requisitos de qualidade forem
  satisfeitos.

## 4. Critérios de aceite

- Um desenvolvedor consegue identificar qual build está publicado sem abrir outra
  tela.
- Preview e Inspector funcionam sem alterar o status de publicação.
- Builds inválidos não podem ser submetidos.
- Ações de preview, Inspector e submissão respeitam loading, erro e retry.
- Os endpoints existentes são reutilizados antes de criar novos contratos.
- Testes cobrem token expirado, build inválido, revalidação e histórico de status.
- A documentação do User Guide explica o fluxo completo.

## 5. Segurança

- Tokens de preview nunca são persistidos no frontend além da abertura da janela.
- O backend valida `gameId`, `buildId`, tenant e usuário do desenvolvedor.
- Builds de outro tenant ou desenvolvedor devem retornar autorização negada.
- Preview não deve permitir mutações administrativas.
