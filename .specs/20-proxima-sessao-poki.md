# Prompt para Próxima Sessão — GameHub Poki Phase

## Estado atual (feito nesta sessão)

A branch `feature/poki-next-phase` já contém:

- **19.11** — Web Exclusives, SEO de categoria e filtros de descoberta.
- **19.8** — Contas opcionais de jogador, favoritos e histórico recente.
- **19.9** — `IAdProvider` com `AdBreakResult`, `ConfigurableAdProvider`, opções por `appsettings.json` e regras de UX (mute/unmute, ad block, uma recompensa por rewarded break).
- Correção do bug de registro: mensagens de erro da API agora são exibidas, com hint de requisitos de senha.
- Testes para `PlayerAccountAppService`, `AdBreakAppService`, `GameCatalogAppService` (web exclusives/filters) e `FakeAdProvider`.
- `README.md`, `README.pt-BR.md` e `CHANGELOG.md` atualizados.
- Migrações EF Core geradas:
  - `AddCategorySeoAndGameRevenueRelation`
  - `AddPlayerFavoritesAndRecent`
  - `AddPlaySessionAdBreakCounts`

Validação:
- `dotnet build Api/GameHub.sln` ✅
- `dotnet test Api/GameHub.sln --no-build` ✅ (216 passed, 1 skipped)
- `npm run build` em `angular/` e `angular-admin/GameHub.UI/` ✅

> **Push ainda não foi feito.** O push final desta fase deve ser executado ao término da próxima sessão (ou quando o usuário confirmar).

---

## Próximas entregas prioritárias

Implementar **19.10** e **19.12**, seguindo a ordem:

1. `19.10-poki-inspector-qa-v2.md` — Inspector de QA v2 (scaling tests, log de eventos SDK, warnings detalhados).
2. `19.12-poki-privacidade-ugc-performance.md` — política de privacidade, filtro de profanidade, FPS e modo anônimo.

Depois disso, abrir PR para `main` a partir de `feature/poki-next-phase` e fazer o push.

---

## Checklist de início da sessão

- [ ] Fazer `git pull origin feature/poki-next-phase` caso outra sessão tenha empurrado algo.
- [ ] Re-ler `19.10-poki-inspector-qa-v2.md` e `19.12-poki-privacidade-ugc-performance.md`.
- [ ] Re-ler `Api/src/GameHub.Core/Domain/Catalog/Game.cs`, `PlaySession.cs`, `GameplayEvent.cs` e `GameMetricSnapshot.cs` para entender extensões de domínio.
- [ ] Verificar migrações pendentes com `dotnet ef migrations list`.

---

## 19.10 — Inspector de QA v2 (detalhado para execução)

### Backend

1. Criar entidades:
   - `SdkEventLog` (`Id`, `SessionId`, `EventType`, `Payload`, `SequenceNumber`, `ReceivedAt`, `TenantId`).
   - `InspectorReport` (`Id`, `GameId`, `ReportType`, `Warnings[]`, `CreatedAt`, `TenantId`).
2. Adicionar `DbSet`s e configurações em `GameHubDbContext`/`GameHubModelCreatingExtensions`.
3. Criar DTOs em `Api/src/GameHub.Application/Inspector/Dto/`.
4. Criar `IInspectorAppService` / `InspectorAppService`:
   - `RecordSdkEventAsync(RecordSdkEventInput)` — armazena evento e valida sequência.
   - `ValidateSessionAsync(Guid sessionId)` — retorna warnings: eventos duplicados, ordem inválida, `gameplayStart` antes de `gameLoadingFinished`, eventos durante ad breaks, etc.
   - `RunBuildValidationAsync(Guid gameId)` — combina `IGameBuildPackageValidator` + verifica splash screens, outgoing links, arquivo > 8 MB (Poki guideline), aspect ratio 16:9 e ausência de requests externos.
5. Expôr endpoints `/api/services/app/Inspector/*`.
6. Gerar migração `AddInspectorQaV2`.

### Frontend

1. Criar/expandir a página admin `/app/main/gamehub/inspector`:
   - Lista de relatórios por jogo.
   - Detalhe de log de eventos SDK com timeline.
   - Seção "Warnings" com severidade e sugestão.
   - Botão "Re-run validation".
2. Atualizar `GameHubAdminService` para chamar novos endpoints.

### Testes

- `InspectorAppService_Tests`: eventos fora de ordem, duplicação, validação de build, splash/outgoing links.
- `GameplayBridgeService` (Angular) não precisa de teste unitário se já houver cobertura de integração.

---

## 19.12 — Privacidade, UGC e Performance (detalhado para execução)

### Backend

1. Criar `PrivacyPolicy` entidade (`Id`, `GameId`, `Text`, `ExternalRequestsJson`, `RequiresConsent`, `Version`, `TenantId`) e `DbSet`.
2. Criar `ProfanityFilter` (domain service simples, blacklist inicial em `GameHubConsts` ou `ISettingProvider`).
3. Criar `UserContent` (UGC) entidade: comentários/avaliações moderadas.
   - `Id`, `GameId`, `UserId`, `Type` (Comment/Review), `Text`, `IsApproved`, `ModerationReason`, `TenantId`.
4. Adicionar `FpsMeasurement` em `PlaySession` ou `GameplayEvent` (campos `FpsSamples`/`AvgFps`) e coletar via bridge.
5. Modo anônimo:
   - Feature flag `AnonymousMode` (já existe infra de `FeatureFlag`).
   - `PlayerAccountAppService` e `GameplayAppService` respeitam modo anônimo: não exigem login, não persistem PII.
6. Criar `IPrivacyAppService` com:
   - `GetPrivacyPolicyAsync(Guid gameId)`
   - `AcceptPolicyAsync(Guid gameId, string deviceId)`
   - `ReportContentAsync(ReportContentInput)`
7. Criar `IProfanityAppService` ou expor `IUserContentAppService`:
   - `SubmitCommentAsync` (aplica filtro, marca para moderação se necessário).
   - `ModerateContentAsync` (admin).
8. Gerar migração `AddPrivacyUgcAndPerformance`.

### Frontend

1. Adicionar link "Privacy" no footer do Game Hub e página `/privacy/:gameSlug`.
2. Na página do jogo, exibir termo de privacidade quando `RequiresConsent` e armazenar aceite local + API.
3. Componente de comentários (UGC) com submissão e status de moderação.
4. Configuração de modo anônimo em `FeatureFlag` admin.

### Testes

- `PrivacyAppService_Tests`, `ProfanityFilter_Tests`, `UserContentAppService_Tests`, `AnonymousMode_Tests`.

---

## Novas specs a criar nesta sessão

Após terminar 19.10 e 19.12, analisar novamente:

- `https://sdk.poki.com/new-requirements`
- `https://sdk.poki.com/sdk-documentation`

Criar specs adicionais como:

- `21-poki-quality-guidelines.md` — checklist de aprovação da Poki (16:9, <8 MB, no splash, no outgoing links, incognito, external requests).
- `22-poki-marketing-ua.md` — suporte a UTM/tráfego orgânico, destaques manuais e campanhas (baseado em `TrafficSource`).
- `23-poki-multiplayer-auds.md` — Poki Netlib / Arbitrary User Data Store (multiplayer e saves cross-device).
- `24-poki-sdk-event-scanner.md` — Error Scanner e dashboard de eventos SDK.

---

## Push e PR

- [ ] Commit por feature (Conventional Commits em pt-BR).
- [ ] `dotnet build Api/GameHub.sln`
- [ ] `dotnet test Api/GameHub.sln --no-build`
- [ ] `npm run build` em ambos os Angulars
- [ ] Atualizar `docs/agent-execution-log.md`
- [ ] `git push origin feature/poki-next-phase` (push final, somente ao final).
- [ ] Abrir PR para `main`.
- [ ] Verificar CI no GitHub Actions.

---

## Notas de segurança

- Nunca commitar `.env`, connection strings ou secrets.
- Validar que external requests e PII estão desabilitados por padrão (LGPD).
- Não copiar texto, assets, marcas ou layout da Poki.
