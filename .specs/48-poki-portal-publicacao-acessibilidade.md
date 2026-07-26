# 48 — Portal do desenvolvedor, publicação e acessibilidade

> **Status:** Especificação para execução
> **Base:** Specs 34–37 e PRs 61, 65 e 67
> **Prioridade:** P1
> **Dependências:** Specs 46 e 47 recomendadas

## Objetivo

Fechar o fluxo de criação, build, Preview, Inspector, revisão e publicação com
uma experiência acessível, previsível e auditável.

## Fluxo de dados

```text
CreateDraft → UploadBuild → Validate → Preview → Inspector
→ SubmitForReview → AdminReview → Publish → CatalogCache.Invalidate
```

## Backend

Revisar:

```csharp
Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input);
Task<UploadGameBuildResultDto> UploadAsync(UploadFromCliInput input);
Task<BuildValidationReportDto> ValidateAsync(Guid buildId);
Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input);
Task PublishAsync(PublishGameInput input);
```

- validar `index.html`, tamanho, hash, extensões e recursos externos;
- exigir política de privacidade quando o build solicitar rede externa;
- tornar upload e submit retry-safe;
- registrar histórico de transições e motivo de rejeição;
- invalidar cache somente após persistência bem-sucedida;
- retornar erros públicos com `correlationId`;
- separar permissões de desenvolvedor, revisor e administrador.

## Angular

Revisar:

- `DeveloperGamesComponent`;
- `DeveloperEarningsComponent`;
- `BuildsComponent`;
- `DeveloperService`;
- `GameplayBridgeService`.

Requisitos:

- estados loading, empty, error, retry e success;
- `aria-live` para transições;
- headings, landmarks, labels e captions;
- foco visível e navegação sem mouse;
- tabelas com `caption`, `thead` e `scope`;
- cancelamento de requisições concorrentes;
- retry somente para erros transitórios;
- preservação de dados anteriores durante refresh;
- tratamento de `401`, `403`, `409` e `429`.

## Testes

- testes Jasmine para cada estado;
- fluxo de publicação feliz e rejeitado;
- retry e refresh concorrente;
- teclado e foco;
- semântica DOM mínima;
- validação de build inválido;
- compatibilidade com jogo antigo sem capabilities novas.

## Critérios de aceite

1. Um desenvolvedor consegue identificar por que um build falhou.
2. Nenhuma transição de publicação ocorre sem autorização e validação.
3. Todas as ações importantes têm feedback acessível.
4. O fluxo funciona em desktop e viewport mobile.
5. Service proxies continuam gerados pelo OpenAPI, sem edição manual.
