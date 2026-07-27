# 47 — Analytics completo, exportação e operação

> **Status:** Especificação para execução
> **Base:** Specs 36, 41, 44 e 68
> **Prioridade:** P0
> **Dependências:** nenhuma alteração no EAF

## Objetivo

Transformar os eventos de gameplay em métricas auditáveis, deduplicadas,
filtráveis e exportáveis para desenvolvedores e administradores.

## Contratos

```csharp
Task<GameMetricsResult> GetMetricsAsync(
    Guid gameId,
    GameMetricsFilter input);

Task<GameMetricsExportDto> ExportCsvAsync(
    Guid gameId,
    GameMetricsFilter input);

Task ExecuteAsync(GameMetricsAggregationArgs args);
```

Adicionar, quando suportado pelo modelo atual:

- `BuildId`;
- `DeviceType`;
- `CountryCode` anonimizado;
- `TrafficSource`;
- `UtmSource`, `UtmMedium`, `UtmCampaign`;
- page views, play starts, loading failures, gameplay starts, completions,
  ad breaks e conversões;
- `ClientEventId` para deduplicação;
- indicador de `IsPlaytest`.

## Regras de ingestão

- `ClientEventId` é opcional para compatibilidade, mas obrigatório para eventos
  novos do SDK;
- chaves de deduplicação incluem tenant, sessão e evento;
- eventos fora da sessão/jogo/build são rejeitados;
- timestamps são normalizados para UTC;
- playtests nunca entram nas métricas públicas de produção;
- payload bruto não é devolvido ao dashboard;
- campos de alta cardinalidade são agregados ou descartados.

## Agregação

- job idempotente por janela UTC;
- snapshots podem ser recalculados sem dupla contagem;
- reprocessamento de uma janela substitui somente o snapshot daquela janela;
- falhas parciais registram checkpoint e permitem retry;
- queries do portal usam projeção e paginação;
- exportação aplica exatamente os mesmos filtros do dashboard.

## CSV

- UTF-8 com cabeçalho estável e versionado;
- valores separados por vírgula com escaping RFC 4180;
- datas em ISO 8601 UTC;
- sem tokens, e-mails, IPs ou connection IDs;
- limite de período e tamanho de exportação;
- futura evolução pode retornar `FileDto` sem quebrar o contrato atual.

## Testes obrigatórios

- filtros combinados;
- timezone e limites inclusivos/exclusivos;
- eventos duplicados;
- replay do job;
- playtest excluído;
- ownership e multi-tenancy;
- CSV com vírgula, aspas, Unicode e dataset vazio;
- timeout e retry do job.

## Critérios de aceite

1. Dashboard e CSV apresentam os mesmos totais para os mesmos filtros.
2. Reprocessar a mesma janela não altera os totais.
3. Um evento repetido não aumenta plays, usuários únicos ou conversões.
4. Exportações não expõem PII.
5. Métricas de agregação possuem logs e alertas operacionais.
