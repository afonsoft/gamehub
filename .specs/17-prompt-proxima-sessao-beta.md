# Prompt para Próxima Sessão — GameHub Beta

> **Objetivo:** preparar o GameHub para uma versão beta de teste focando em **funcionalidades e layout**.  
> NÃO incluir hardening de segurança (CSP/JWT HttpOnly/rate-limit) nem monetização — esses ficam para fases posteriores.

---

## 1. Contexto (estado da `main` após PR #38)

O fluxo básico já funciona via Docker:

- Cadastro de usuário (player/developer)
- Login com token JWT
- Criação/edição de jogo pelo desenvolvedor
- Upload de build `.zip` com `index.html`
- Aprovação do build pelo desenvolvedor
- Submissão para revisão
- Moderação/publicação pelo admin
- Execução do jogo no hub (`/play/:slug`)
- Dashboards de admin e de desenvolvedor
- Busca, cache e trending
- i18n e design system iniciados

Faltam vários polimentos e funcionalidades para um beta confiável.

---

## 2. Escopo desta sessão

### Incluir

1. **Documentação pública no GameHub** — como usar o sistema, API e admin.
2. **Melhorias de UX no GameHub público** — detalhe do jogo, execução, leaderboard, busca, filtros.
3. **Melhorias no Developer Portal** — upload de thumbnail/hero, wizard de submissão, relatório de validação, toasts/empty states.
4. **Melhorias no Admin** — fila de moderação completa, CRUD de categorias/tags, menu de reports/feature flags/audit log, telas de detalhe do jogo.
5. **Backend de suporte** — upload de imagens, report de jogos, suspensão de jogo, melhorias em leaderboards/favoritos.

### NÃO incluir

- Segurança (CSP, headers, JWT HttpOnly, rate limit) — PR futuro.
- Monetização/ads.
- Save na nuvem.
- Subdomínio isolado para jogos (`games.afonsoft.dev`) — manter `PublicEndpoint` configurável.

---

## 3. Entregáveis detalhados

### 3.1 Documentação pública no GameHub (`angular/src/app/public/docs/`)

Criar uma nova área `/docs` no GameHub com conteúdo estático (HTML/CSS) ou markdown (se preferir adicionar `marked`/`ngx-markdown`, justificar).

**Novos arquivos sugeridos:**

- `angular/src/app/public/docs/docs.routes.ts` — rotas lazy.
- `angular/src/app/public/docs/docs.component.ts/.html/.css` — shell com sidebar/índice.
- `angular/src/app/public/docs/user-guide/user-guide.component.ts/.html` — "Como usar o GameHub" (jogar, cadastrar, publicar).
- `angular/src/app/public/docs/api-guide/api-guide.component.ts/.html` — endpoints principais com exemplos de request/response.
- `angular/src/app/public/docs/admin-guide/admin-guide.component.ts/.html` — fluxo admin (login, moderação, categorias, usuários).
- `angular/src/app/public/docs/sdk-guide/sdk-guide.component.ts/.html` — como integrar `gamehub-sdk.js` no jogo.
- `angular/src/app/public/docs/docs.routes.ts` registrado em `angular/src/app/public/public.routes.ts`.
- Link "Docs" no header/footer do GameHub (`angular/src/app/app.component.html` ou `public/home/home.component.html`).

**Conteúdo mínimo:**

- User Guide: cadastro, login, navegação, jogar, reportar jogo, painel do dev.
- API Guide: base URL, autenticação, catálogo, gameplay, developer, admin (lista reduzida com exemplos).
- Admin Guide: login, dashboard, games, uploads, moderação, categorias/tags, usuários.
- SDK Guide: `GameHubSDK.init()`, eventos (`gameLoadingStarted`, `gameplayStart`, `submitScore`, etc.).

**Critérios de aceite:**

- `/docs` acessível sem autenticação.
- `/docs/user-guide`, `/docs/api-guide`, `/docs/admin-guide`, `/docs/sdk-guide` renderizam conteúdo legível em pt-BR e en-US.
- Menu/docs responsivo em mobile.
- Links no header/footer visíveis.

---

### 3.2 GameHub público — melhorias de UX

#### A. Página de detalhe do jogo (`angular/src/app/public/game-detail/`)

**Melhorias:**

- Botão **"Report game"** (abre modal com motivo e descrição, chama `POST /api/services/app/UserReport/Submit`).
- Botão **"Add to favorites"** (apenas visual/placeholder se não houver backend; se possível, persistir em localStorage/session).
- Exibir nota média (`averageRating`) e total de plays de forma mais visível.
- **Ações mobile**: play e leaderboard em layout responsivo.
- **Loader/skeleton** enquanto carrega.
- **Empty state** se jogo não encontrado.

**Arquivos:**

- `angular/src/app/public/game-detail/game-detail.component.ts`
- `angular/src/app/public/game-detail/game-detail.component.html`
- `angular/src/app/public/game-detail/game-detail.component.css`

#### B. Execução do jogo (`angular/src/app/player/game-frame/`)

**Melhorias:**

- Overlay **"Start Game"** antes de carregar o iframe. Ao clicar:
  1. Chama `GameplayBridgeService.gameplayStart()`.
  2. Mostra o iframe.
- Adicionar atributos `sandbox`, `allow`, `referrerpolicy` ao iframe conforme `.specs/15-csp-security-headers.md` §4.1.
- Loading spinner enquanto o jogo carrega.
- Tratamento de erro se `publishedBuildUrl` não existir (jogo não publicado).

**Arquivos:**

- `angular/src/app/player/game-frame/game-frame.component.ts/.html/.css`
- `angular/src/app/core/services/gameplay-bridge.service.ts`

#### C. Leaderboard (`angular/src/app/player/leaderboard/`)

**Melhorias:**

- Ocultar seção "Submit your score" se usuário não estiver autenticado (mostrar CTA "Login to submit").
- Paginação (pode ser simples: `Top 10`, `My rank` se autenticado).
- Skeleton/empty state.
- Usar design system `CardComponent`/`BadgeComponent`.

**Arquivos:**

- `angular/src/app/player/leaderboard/leaderboard.component.ts/.html/.css`
- `angular/src/app/core/services/leaderboard.service.ts`

#### D. Busca/Catálogo (`angular/src/app/public/games/` / `angular/src/app/public/search-page/`)

**Melhorias:**

- Adicionar filtros de **dispositivo** (desktop/mobile/tablet) e **orientação** (landscape/portrait).
- Persistir filtros na URL (já parcialmente feito; garantir que device/orientation apareçam).
- Debounce na busca por texto.
- Empty state quando não houver resultados.
- Skeleton cards enquanto carrega.
- Usar componentes do design system (`Card`, `Badge`, `Button`, `Pagination`).

**Arquivos:**

- `angular/src/app/public/games/games.component.ts/.html/.css`
- `angular/src/app/public/search-page/search-page.component.ts/.html/.css`
- `angular/src/app/core/services/game-catalog.service.ts`

#### E. i18n

- Traduzir todas as strings hardcoded dos componentes acima para `pt-BR.json` e `en-US.json`.
- Garantir que `TranslatePipe` seja usado em labels, botões, títulos.

**Arquivos:**

- `angular/src/assets/i18n/pt-BR.json`
- `angular/src/assets/i18n/en-US.json`

---

### 3.3 Developer Portal — melhorias

#### A. Upload de thumbnail e hero image

**Backend:**

- Novo endpoint `POST /api/services/app/DeveloperGame/UploadThumbnail?gameId={id}` (multipart, imagem ≤2MB, salva em MinIO e retorna URL pública).
- Novo endpoint `POST /api/services/app/DeveloperGame/UploadHero?gameId={id}`.
- Atualizar `Game.ThumbnailUrl`/`Game.HeroImageUrl`.
- Validar tipos: `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif`.
- Armazenar em bucket/prefixo separado: `gamehub-assets/thumbnails/{gameId}` e `gamehub-assets/heroes/{gameId}`.
- Adicionar `PublicEndpoint` em storage se necessário.

**Arquivos backend:**

- `Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs`
- `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`
- `Api/src/GameHub.Application/Developer/Dto/UploadImageResultDto.cs` (novo)
- `Api/src/GameHub.Web.Host/Storage/MinioGameAssetStorage.cs` (ajustar `StoreAssetAsync`)
- `Api/src/GameHub.Core/Storage/IGameAssetStorage.cs`

**Frontend:**

- Adicionar upload de thumbnail/hero em `game-create` e `game-edit`.
- Preview da imagem após upload.
- Usar `developer.service.ts`.

**Arquivos frontend:**

- `angular/src/app/developer/game-create/game-create.component.ts/.html/.css`
- `angular/src/app/developer/game-edit/game-edit.component.ts/.html/.css`
- `angular/src/app/core/services/developer.service.ts`

#### B. Relatório de validação de build

- Em `angular/src/app/developer/builds/builds.component.ts/.html`, exibir `ValidationSummary` (isValid, errors, warnings, package size, hash, indexHtml) em accordion/modal.
- Mostrar status do build com badge (`Validated`, `Approved`, `Published`, `Rejected`).

#### C. Wizard de submissão

- Opcional para beta: em `angular/src/app/developer/games/games.component.html`, adicionar botão "New Game" com fluxo simplificado:
  1. Dados básicos
  2. Upload de build
  3. Aprovar e submeter.
- Se já houver `game-create`, melhorar copy e adicionar hints/tooltips.

#### D. Toasts e confirmação

- Criar `ToastService` (ou reutilizar) e `ConfirmDialog`.
- Confirmar antes de submeter para revisão, aprovar/rejeitar build, deletar rascunho.
- Exibir toast de sucesso/erro.

**Arquivos:**

- `angular/src/app/core/services/toast.service.ts` (novo ou reutilizar)
- `angular/src/app/shared/ui/toast/toast.component.ts` (existente)

---

### 3.4 Admin — melhorias

#### A. Fila de moderação (`angular-admin/GameHub.UI/src/app/main/gamehub/moderation/`)

- `review-queue.component.html`: adicionar `routerLink` no botão "Review" para `/app/main/gamehub/moderation/:id`.
- Exibir desenvolvedor, data de submissão, versão do build.
- Filtro rápido por status (Pending, Approved, Rejected, RequiresChanges).

#### B. Tela de detalhe da revisão (`review-detail.component.ts/.html`)

- Mostrar metadados do jogo, relatório de validação do build, histórico de moderação.
- Botões: **Approve**, **Reject** (com modal de motivo obrigatório), **Require Changes** (com modal de notas).
- Após ação, redirecionar para fila.
- Chamar `POST /api/services/app/Moderation/CompleteReview`.

#### C. Categorias e Tags

- `category-list.component.html` / `tag-list.component.html`: adicionar ação **Delete** com confirmação.
- `category-edit.component.ts`: auto-gerar slug a partir do nome (permitir editar manualmente).
- `tag-edit.component.ts`: auto-gerar slug a partir do nome.
- Toast de sucesso/erro.

#### D. Menu admin

- Adicionar itens no menu de `angular-admin/GameHub.UI/src/app/shared/layout/nav/app-navigation.service.ts`:
  - Reports (`/app/main/gamehub/reports`, permissão `Pages.Reports.Manage`)
  - Feature flags (`/app/main/gamehub/dashboard/flags`)
  - Audit log (`/app/main/gamehub/dashboard/audit`)
- Garantir que as rotas existam em `gamehub-routing.module.ts`.

#### E. Tela de Reports (admin)

- Criar `angular-admin/GameHub.UI/src/app/main/gamehub/reports/report-list.component.ts/.html`.
- Listar reports com: jogo, motivo, status, data.
- Ação: Alterar status (Open, Investigating, Resolved, Dismissed) via `PUT /api/services/app/AdminReport/UpdateStatus`.

**Backend:** verificar se `AdminReportAppService` já expõe `GetAllAsync` paginado e `UpdateStatusAsync`.

#### F. Detalhe/Edição de jogo no admin

- `angular-admin/GameHub.UI/src/app/main/gamehub/games/game-detail.component.ts/.html`:
  - Mostrar todos os metadados, builds, moderações.
  - Ações: Publish, Suspend, View public page.
- `game-edit.component.ts/.html` (separado ou reaproveitando `game-detail`):
  - Formulário completo de metadados.
  - Categorias/tags via multiselect.

---

### 3.5 Backend de suporte

#### A. Upload de imagens para jogos

- Implementar `DeveloperGameAppService.UploadThumbnailAsync` e `UploadHeroAsync`.
- Armazenar em MinIO com `ContentType` correto.
- Atualizar `IGameAssetStorage` com `StoreAssetAsync(AssetUploadInput)` genérico se necessário.

#### B. Report de jogos

- `UserReportAppService.SubmitAsync` já existe. Verificar se há UI pública. Criar modal na tela de detalhe do jogo.
- Admin list já está no backend. Criar UI.

#### C. Suspensão de jogo

- `AdminGameAppService.SuspendAsync` já existe. Adicionar botão "Suspend" no admin game detail.

#### D. Leaderboard

- Verificar se `LeaderboardEntry` persiste melhor pontuação por usuário (já implementado).
- Garantir que `LeaderboardAppService.GetTopAsync` retorne display name do usuário (resolver via `IRepository<User, long>` se necessário).

---

## 4. Documentação do projeto

- Atualizar `docs/agent-execution-log.md` com as melhorias da sessão.
- Atualizar `docs/README.md` se novos arquivos de docs do projeto forem criados.
- Criar/ajustar `docs/user-guide.md`, `docs/api-guide.md`, `docs/admin-guide.md`, `docs/sdk-guide.md` (opcional, mas recomendado — podem ser a fonte de conteúdo para os componentes públicos).
- Atualizar `CHANGELOG.md`.

---

## 5. Critérios de aceite gerais

- `dotnet build Api/GameHub.sln -c Release --no-restore` OK.
- `dotnet test Api/GameHub.sln -c Release --no-build` — todos passam (199+, 1 skipped esperado).
- `docker compose -f docker-compose.yml config -q` e `docker compose -f docker-compose.all.yml config -q` OK.
- `npm run build` OK em `angular/` e `angular-admin/GameHub.UI/`.
- Todas as telas novas responsivas (mobile-first).
- Nenhum secret, `.env` ou connection string commitado.
- i18n: pt-BR e en-US sem strings hardcoded nas novas telas.

---

## 6. Sugestão de branches

Caso fique grande, dividir em PRs menores:

1. `feature/gamehub-docs` — documentação pública no hub.
2. `feature/gamehub-beta-ux` — detalhe, game-frame, leaderboard, busca, i18n.
3. `feature/developer-assets` — thumbnail/hero + relatório de validação.
4. `feature/admin-moderation-crud` — fila, detail, categorias/tags, reports, menu.

Se for uma única sessão, usar `feature/beta-readiness`.

---

## 7. Notas para o agente

- Priorizar **funcionalidades que desbloqueiam o beta**: docs no hub + execução do jogo com overlay + moderação completa.
- Seguir `CLAUDE.md` / `AGENTS.md`: Clean Architecture, DDD, testes, sem push para `main`.
- Reaproveitar design system existente (`angular/src/app/shared/ui/`).
- Não alterar CORS/middlewares de segurança salvo se for estritamente necessário para a funcionalidade.
- Manter `docker-compose.override.test.yml` fora do git (uso local).
