# 37 — User Guide: documentação operacional do desenvolvedor

> **Status:** Especificação para execução
> **Base:** `angular/src/app/public/docs/user-guide` e PR #61
> **Objetivo:** transformar o User Guide em documentação operacional suficiente para um desenvolvedor publicar, validar, revisar e acompanhar um jogo.

## 1. Estrutura

O User Guide deve conter seções navegáveis para:

1. Encontrar e jogar jogos.
2. Criar uma conta e entender o modo anônimo.
3. Entrar no portal do desenvolvedor.
4. Criar e editar um jogo.
5. Enviar um build.
6. Interpretar a validação do build.
7. Usar Preview e Inspector.
8. Submeter para revisão e responder a pedidos de alteração.
9. Consultar métricas e Earnings.
10. Reportar problemas e solicitar suporte.

## 2. Conteúdo obrigatório

### 2.1 Publicação

- Requisitos mínimos do ZIP: `index.html`, assets referenciados e limite de
  tamanho vigente.
- Status possíveis do jogo e do build.
- Diferença entre `Approve build` e `Submit for review`.
- Explicação de que aprovação de build não significa publicação.

### 2.2 Preview e Inspector

- Preview é temporário e não altera publicação.
- Inspector mostra warnings de SDK/qualidade.
- O desenvolvedor deve corrigir warnings bloqueantes antes da submissão.

### 2.3 Earnings e métricas

- Valores são estimativas.
- Períodos e timezone.
- Diferença entre plays, jogadores únicos, breaks e receita.
- Dados podem atrasar por agregação.

### 2.4 Segurança e privacidade

- Nunca inserir API keys, tokens ou connection strings em builds.
- Declarar domínios externos e analytics quando exigido.
- Não coletar PII sem base legal e consentimento aplicável.
- Orientar o uso de suporte para remoção/correção de conteúdo.

## 3. Internacionalização

- Manter chaves em `angular/public/i18n/pt-BR.json` e `en-US.json`.
- Não inserir textos operacionais novos diretamente no template.
- Verificar que nenhuma chave aparece literalmente na interface.
- Datas, números e moeda devem respeitar locale.

## 4. Critérios de aceite

- Um novo desenvolvedor consegue completar o fluxo sem consultar código.
- Cada rota do portal possui pelo menos um link no User Guide.
- O guia não promete funcionalidades ainda não implementadas.
- PT-BR e EN-US possuem conteúdo equivalente.
- Links internos usam rotas Angular válidas.
- A documentação é revisada junto com cada alteração de fluxo.

## 5. Verificação

- `npm run build` em `angular/`.
- Busca por chaves ausentes nos arquivos de tradução.
- Verificação manual de `/docs/user-guide` em desktop e mobile.
