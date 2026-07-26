# 36 — Analytics e Earnings para desenvolvedores

> **Status:** Especificação para execução
> **Base:** `.specs/26-poki-proxima-fase.md` seção 26.12 e PR #61
> **Objetivo:** tornar os dados de desempenho e receita úteis para decisões de publicação, sem apresentar estimativas como valores pagos.

## 1. Escopo

### 1.1 Métricas de jogo

O portal deve exibir, por jogo e período:

- page views;
- play starts;
- loading finished;
- gameplay started;
- jogadores únicos;
- duração média;
- erros;
- commercial breaks;
- rewarded breaks;
- conversão de visita para início de jogo;
- conversão de carregamento para gameplay.

Filtros mínimos:

- período;
- jogo;
- país;
- dispositivo.

### 1.2 Receita estimada

- Manter separadas:
  - receita bruta estimada;
  - receita estimada do desenvolvedor;
  - receita estimada da plataforma;
  - impressões por tipo de anúncio;
  - share aplicado.
- Exibir data de atualização e período da consulta.
- Exibir aviso: `Valores estimados; não representam payout confirmado`.
- Preservar o detalhamento diário e permitir exportação CSV em uma etapa posterior.

### 1.3 Qualidade dos dados

- Informar quando não há dados suficientes para uma métrica.
- Diferenciar zero, ausente e indisponível.
- Não contar eventos duplicados.
- Usar timezone UTC no backend e explicitar a conversão no frontend.
- Aplicar isolamento por tenant e autorização do desenvolvedor.

## 2. Critérios de aceite

- O filtro de período não aceita `from > to`.
- A API retorna dados somente dos jogos autorizados ao desenvolvedor.
- A UI não mistura métricas de jogos diferentes sem identificar o jogo.
- A receita estimada não aparece como saldo disponível ou payout.
- Agregações reproduzem os valores do serviço de aplicação em testes.
- Testes cobrem período vazio, múltiplos jogos, filtros por dispositivo/país,
  deduplicação e isolamento multi-tenant.

## 3. Fora do escopo

- Processamento financeiro real.
- Integração com Stripe, PayPal ou provedor de pagamentos.
- Alteração do contrato de revenue share.
- Previsão de receita futura baseada em machine learning.
