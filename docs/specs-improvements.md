# Análise de `.specs` + Poki — O que falta e o que podemos melhorar

> Estado de referência: repositório `afonsoft/gamehub` após os PRs #26 a #33 (`main`).

---

## 1. Resumo do que já está funcional

- **Backend**: domínio `Game`, `GameBuild`, `Category`, `Tag`, `DeveloperProfile`, `PlaySession`, `GameplayEvent`, `GameMetricSnapshot`, `LeaderboardEntry`, `ModerationReview`, `UserReport`.
- **Cadastro**: registro público com roles `Player` e `Developer` e criação automática de `DeveloperProfile`.
- **Fluxo desenvolvedor**: criação de rascunho, edição de metadados (com categorias/tags), upload de build `.zip`, aprovação/rejeição do build pelo dev, submissão para revisão, moderação e publicação.
- **Upload**: validação de `index.html`, SHA-256, tamanho máximo 100 MB, extração e armazenamento no MinIO/S3, listagem de arquivos extraídos para o admin.
- **Catálogo**: home, listagem, busca por texto/categoria/tag/dispositivo/orientação, detalhe por slug, jogos relacionados.
- **Gameplay**: sessão de play, eventos (10 tipos), `postMessage` bridge com origem validada, leaderboard em Redis Sorted Sets.
- **Admin**: dashboard com total de jogos/revisões pendentes/plays/usuários/desenvolvedores, fila de moderação, games, categorias, tags, uploads/arquivos, usuários, feature flags, audit logs.
- **Infra**: Redis substitui cache in-memory quando habilitado, Docker Compose full stack, CORS configurado para hub/admin e wildcard `*.afonsoft.dev`.
- **Testes**: 176 testes passando (1 skipped), builds Angular e Docker configs validados.

---

## 2. Gaps em relação à pasta `.specs`

### 2.1 Segurança e compliance (Fase 3 ainda incompleta)

- `SecurityHeadersMiddleware`, `ContentSecurityPolicyMiddleware` e `RateLimitingMiddleware` foram removidos do pipeline por causarem erros 504/CORS. Precisam ser reintroduzidos sem conflitar com o admin Angular/EAF.
- JWT ainda usa `localStorage` no frontend e `UseJwtTokenMiddleware` do EAF. O spec `08-seguranca-lgpd-compliance.md` exige:
  - Access token 30 min (memory/`sessionStorage`).
  - Refresh token 7 dias em cookie `HttpOnly`, `Secure`, `SameSite=Strict`.
  - Endpoint `POST /api/TokenAuth/RefreshToken` e blacklist no Redis.
- LGPD: `PrivacyAppService` existe, mas ainda falta consentimento explícito de cookies/analytics e política de privacidade no frontend.
- Rate limiting: nenhum no pipeline. Deve retornar `429` com headers `X-RateLimit-*`.

### 2.2 Frontend Game Hub (`angular/`)

- Não há design system próprio (componentes `button`, `card`, `table`, `badge`, `skeleton`, `toast`, `pagination` etc.) — a UI ainda é mínima/placeholder.
- Lazy loading está com `loadComponent` direto, não segue estritamente os módulos `public.routes.ts`, `player.routes.ts`, `developer.routes.ts` do spec.
- Faltam interceptors `JwtInterceptor`, `ErrorInterceptor`, `CorrelationIdInterceptor` alinhados com o spec.
- `GameplayBridgeService` não implementa confirmação de `gameplayStart` no primeiro input do jogador (Poki exige que o jogo dispare, não a plataforma no load).
- i18n pt-BR/en-US ainda não implementada no frontend (`@angular/localize` ou `ngx-translate`).

### 2.3 Frontend Admin (`angular-admin/GameHub.UI`)

- O menu GameHub foi criado, mas o template EAF ainda domina a navegação (users, roles, tenants, languages). A experiência ideal seria o menu GameHub como primário e EAF como secundário/configurações.
- Telas de dashboard carecem de gráfico de plays ao longo do tempo (`chart.js` ou similar).
- Faltam tela de feature flags funcional e filtros avançados na fila de moderação.
- Ações de `Publish`/`Suspend` no admin game list não estão necessariamente ligadas a confirmação modal.

### 2.4 Backend / API

- **Busca full-text**: `SearchAsync` usa `ToLower().Contains`, não o full-text search do PostgreSQL (`tsvector`/`tsquery`) previsto no spec.
- **Trending**: home usa `TotalPlays` como proxy de trending. Deveria usar crescimento recente (últimas 24h/7d) a partir de `GameMetricSnapshot`.
- **Recomendações**: fora do escopo MVP, mas o spec menciona recomendações simples.
- **Cache TTL**: home 5 min, categorias/tags 30 min, detalhe 10 min, leaderboard 1 min, busca 2 min — a implementação atual invalida home em mudanças, mas não aplica TTL granular.
- **Caching de detalhe/busca**: `GameCatalogAppService.GetBySlugAsync` e `SearchAsync` não usam cache.
- **Gameplay eventos brutos**: são persistidos todos os eventos. O spec sugere agregação para evitar volume excessivo. Já existe `GameMetricsAggregationJob`, mas os eventos brutos continuam no banco (retention 6 meses é aceitável).
- **PlaySession heartbeat**: sessão é encerrada em `ngOnDestroy`, mas não há timeout automático de 30s sem heartbeat.

### 2.5 DevOps / Infra

- **Domínio isolado para jogos**: o spec exige `games.afonsoft.dev` servindo os builds. Hoje o `MinIO` endpoint é usado diretamente; falta um proxy/CDN (Nginx/CloudFront) para `games.afonsoft.dev` com `Cache-Control` imutável e CSP adequada.
- **Health checks**: `UseEafHealthChecks` existe, mas não há endpoint `/health` retornando PostgreSQL/Redis/MinIO individualmente.
- **Scripts de bootstrap**: `install.sh` existe, mas `scripts/bootstrap.sh`, `run-local.sh`, `test-all.sh`, `lint-all.sh`, `migrate-db.sh`, `seed-dev.sh` do spec ainda não foram criados.
- **CI/CD**: workflows de build/teste existem, mas faltam publicação de imagens Docker e deploy.

### 2.6 Qualidade e UX

- **Thumbnails**: o spec fala em upload de thumbnail e hero image, mas não há endpoint de upload de imagens para jogos.
- **Imagens/animated thumbnails**: Poki exige thumbnail estático e animado (`.mp4`, 4-6s, 1080x1080). Hoje só existe `ThumbnailUrl`/`HeroImageUrl` como string sem upload.
- **Classificação indicativa**: `AgeRating` usa string livre (`"E"` padrão). Poderia ser value object `AgeRating` com valores padronizados (`E`, `E10+`, `T`, `M`).
- **Slug**: `Slug` value object existe, mas a lógica de `ToLowerInvariant().Replace(" ", "-")` é inglesa; poderia remover acentos e caracteres especiais.

---

## 3. Oportunidades inspiradas no Poki

### 3.1 Developer Portal (Poki for Developers)

- **Dashboard do dev com métricas reais**: DAU, DAU jogando vs não-jogando, engagement (tempo por DAU), earnings, ad performance, player feedback, erros, filtráveis por país e dispositivo.
- **Wizard de 5 passos**: progresso visual do jogo pelas fases de teste até publicação.
- **Versões e Preview**: listar todas as builds, com botão "Preview" para abrir o jogo como no ambiente real, e "Inspector" para QA.
- **Poki Inspector / QA checklist**: tela com módulos de QA (desktop/mobile, scaling, eventos, external resources, performance) e log de eventos do SDK.
- **Playtests**: solicitar/playtest recordings de moderadores/QA.
- **Thumbnails**: upload de thumbnail estático e animado, com status de revisão (`Pending`/`Approved`/`Rejected`).
- **Billing**: cadastro de dados bancários, moeda preferida e relatório de earnings.
- **Team/Usuários**: múltiplos usuários por time de desenvolvedor com roles `Developer` e `Developer Support`.

### 3.2 Gameplay / SDK

- **SDK JavaScript oficial**: fornecer `gamehub-sdk.js` que os jogos importam e chamam `GameHubSDK.gameplayStart()`, `GameHubSDK.commercialBreak()` etc. Hoje a comunicação é por `postMessage` bruto.
- **Eventos do SDK alinhados com Poki**:
  - `gameLoadingFinished()` → conversion to play.
  - `gameplayStart()` no **primeiro input** do jogador.
  - `gameplayStop()` em pausa, game over, menu, cutscene.
  - `commercialBreak()` apenas em pausas naturais (ex: entre fases, ao sair de menu).
  - `rewardedBreak()` com prompt claro e recompensa única.
- **Mobile mode**: no inspector/preview, alternar para mobile exibe QR code para teste em dispositivo real.
- **Scaling tests**: simular dimensões 640x360, 836x470, 1031x580 e dispositivos populares.
- **Fullscreen**: permitir fullscreen quando apropriado (plataforma decide).

### 3.3 Monetização

- **IAdProvider real**: hoje é `FakeAdProvider` (delay fixo). No futuro integrar com Google AdSense/AdMob for Games, ironSource, ou outro sem acoplar domínio.
- **Revenue share**: rastrear impressões/cliques por jogo e calcular split (ex: 50/50 quando o usuário vem da plataforma, 100% quando vem direto).
- **Relatório de earnings**: por jogo, por país, por dispositivo, por tipo de anúncio.
- **Ad policy**: bloquear anúncios próprios dentro do jogo (exceto os da plataforma), não exigir desbloqueio de adblocker, não recompensar quando adblocker detectado.

### 3.4 Segurança / Privacidade

- **External resources policy**: como Poki, bloquear por padrão requests de terceiros vindos do iframe do jogo. Jogos que precisarem de multiplayer/analytics devem declarar URLs e ter uma Privacy Statement.
- **Incognito support**: documentar que jogos não devem depender de `localStorage` sem `try/catch`.
- **CSP restritiva**: reativar com `frame-src https://games.afonsoft.dev` e `frame-ancestors` permitindo apenas os frontends.

### 3.5 User Accounts (jogador)

- **Login/avatar**: Poki User Accounts retorna `username` e `avatarUrl`. Podemos oferecer perfil público mínimo com avatar.
- **Cloud saves**: sincronizar `localStorage`/`IndexedDB` do jogo com backend para usuários logados, respeitando limite de 1MB.
- **Favoritos e ratings**: jogador salva jogos favoritos e dá nota/comentário.

---

## 4. Priorização sugerida

### 4.1 Curto prazo (próxima PR)

1. **Segurança sem quebrar o admin**:
   - Reintroduzir `SecurityHeadersMiddleware` e `ContentSecurityPolicyMiddleware` de forma condicional (desativáveis via config até estabilizar).
   - Reintroduzir `RateLimitingMiddleware` com políticas por recurso e headers `X-RateLimit-*`.
   - Manter `Cors:AllowAnyOrigin` como escape hatch, mas habilitar origens explícitas em produção.
2. **JWT HttpOnly + refresh token** (PR separado, exige interceptação do `TokenAuth` do EAF).
3. **Frontend Game Hub**:
   - Criar design system mínimo (botões, cards, tabela, badge).
   - Implementar `JwtInterceptor`, `ErrorInterceptor`, `CorrelationIdInterceptor`.
   - Melhorar `GameFrameComponent` para não chamar `gameplayStart()` automaticamente; deixar o jogo disparar via SDK/bridge.
   - i18n pt-BR/en-US.

### 4.2 Médio prazo

4. **Busca full-text no PostgreSQL**: substituir `.Contains` por `tsvector`/`tsquery`.
5. **Trending real** com `GameMetricSnapshot` (últimas 24h/7d).
6. **Upload de thumbnails/hero images** para jogos.
7. **Domínio isolado `games.afonsoft.dev`** para servir builds com cache imutável.
8. **Health check endpoint `/health`** detalhado.

### 4.3 Longo prazo

9. **SDK JavaScript oficial** (`gamehub-sdk.js`) e Poki-style QA/Inspector.
10. **Métricas de developer** e relatório de earnings.
11. **Cloud saves e user accounts**.
12. **Integração real de ads** e revenue share.

---

## 5. Arquivos de referência

- `.specs/16-plano-implementacao-gaps.md`
- `.specs/15-csp-security-headers.md`
- `.specs/08-seguranca-lgpd-compliance.md`
- `.specs/05-api-contratos.md`
- `.specs/06-frontend-angular.md`
- `.specs/06b-frontend-admin.md`
- `docs/agent-execution-log.md`
- Poki docs: https://sdk.poki.com/new-requirements.html, https://sdk.poki.com/what-is-p4d.html, https://sdk.poki.com/poki-inspector.html, https://sdk.poki.com/sdk-documentation
