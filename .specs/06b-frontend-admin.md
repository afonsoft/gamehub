# 06b - Frontend Angular Admin

## Visão geral

O Admin é a aplicação Angular separada para gestão, moderação e métricas. Consome a mesma API backend do Game Hub.

**DNS produção**: `gamehub-admin.afonsoft.dev`

**Dockerfile**: O template EAF já possui Dockerfile para o admin. Utilizar o Dockerfile existente.

**API URL**: `https://gamehub-api.afonsoft.dev`

**Design System**: Design system próprio (NOT Angular Material). Mesmos tokens e componentes base do Game Hub com variações de tema admin (sidebar fixa, tabelas densas).

## Estrutura

```text
angular-admin/src/app/
  core/
    auth/
      auth.service.ts
      auth.guard.ts
      token.service.ts
      role.guard.ts
    http/
      http-interceptor.service.ts
    guards/
      auth.guard.ts
      admin.guard.ts
      moderator.guard.ts
    interceptors/
      jwt.interceptor.ts
      error.interceptor.ts
      correlation-id.interceptor.ts
    services/
      api.service.ts
  shared/
    components/
      sidebar/
      header/
      data-table/
      confirm-dialog/
      toast/
      skeleton/
      badge/
      pagination/
    pipes/
      date.pipe.ts
      truncate.pipe.ts
    models/
  games/
    game-list/
    game-detail/
    game-edit/
  moderation/
    review-queue/
    review-detail/
  categories/
    category-list/
    category-edit/
  tags/
    tag-list/
    tag-edit/
  dashboard/
    dashboard/
    feature-flags/
    audit-log/
  login/
```

## Rotas e Lazy Loading

```typescript
const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./login/login.module').then(m => m.LoginModule),
  },
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'games', pathMatch: 'full' },
      {
        path: 'games',
        canActivate: [AdminGuard],
        loadChildren: () => import('./games/games.module').then(m => m.GamesModule),
      },
      {
        path: 'moderation',
        canActivate: [ModeratorGuard],
        loadChildren: () => import('./moderation/moderation.module').then(m => m.ModerationModule),
      },
      {
        path: 'categories',
        canActivate: [AdminGuard],
        loadChildren: () => import('./categories/categories.module').then(m => m.CategoriesModule),
      },
      {
        path: 'tags',
        canActivate: [AdminGuard],
        loadChildren: () => import('./tags/tags.module').then(m => m.TagsModule),
      },
      {
        path: 'dashboard',
        canActivate: [AdminGuard],
        loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
      },
    ],
  },
];
```

### Rotas detalhadas

| Rota | Componente | Guard | Roles |
|------|-----------|-------|-------|
| `/login` | LoginPageComponent | GuestGuard | — |
| `/` | redirect → `/games` | AuthGuard | any |
| `/games` | GameListComponent | AdminGuard | Admin |
| `/games/:id` | GameDetailComponent | AdminGuard | Admin |
| `/games/:id/edit` | GameEditComponent | AdminGuard | Admin |
| `/moderation` | ReviewQueueComponent | ModeratorGuard | Admin, Moderator |
| `/moderation/:id` | ReviewDetailComponent | ModeratorGuard | Admin, Moderator |
| `/categories` | CategoryListComponent | AdminGuard | Admin |
| `/categories/create` | CategoryEditComponent | AdminGuard | Admin |
| `/categories/:id/edit` | CategoryEditComponent | AdminGuard | Admin |
| `/tags` | TagListComponent | AdminGuard | Admin |
| `/tags/create` | TagEditComponent | AdminGuard | Admin |
| `/tags/:id/edit` | TagEditComponent | AdminGuard | Admin |
| `/dashboard` | DashboardComponent | AdminGuard | Admin |
| `/dashboard/flags` | FeatureFlagsComponent | AdminGuard | Admin |
| `/dashboard/audit` | AuditLogComponent | AdminGuard | Admin |

## Guards

### AuthGuard

Verifica se o usuário possui um token JWT válido no `TokenService`. Se não, redireciona para `/login`.

```typescript
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private tokenService: TokenService, private router: Router) {}

  canActivate(): boolean {
    if (this.tokenService.isValid()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}
```

### AdminGuard

Extends `AuthGuard`. Verifica se o token possui role `Admin`.

### ModeratorGuard

Extends `AuthGuard`. Verifica se o token possui role `Admin` OU `Moderator`.

### GuestGuard

Se o token é válido, redireciona para `/games`. Usado apenas na rota de login.

## Componentes

### LoginPageComponent

- Campos: UserNameOrEmailAddress, Password
- Botão "Login"
- Chama `POST /api/TokenAuth/Authenticate`
- Armazena token no `TokenService` (localStorage)
- Redireciona para `/games` após sucesso
- Exibe erro toast em falha

### GameListComponent

- **Tabela paginada** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | Title | `game.title` |
  | Developer | `game.developerName` |
  | Status | `game.status` (badge colorida) |
  | PublishedDate | `game.publishedDate` (formatada) |
  | Actions | Botões: View, Edit, Suspend |
- **Filtros**: dropdown de status (All, Draft, InReview, Published, Rejected, Suspended)
- **Paginação**: skipCount + maxResultCount, navegação de páginas
- **API**: `GET /api/services/app/AdminGame/GetAll`
- **Ações**:
  - View → navega para `/games/:id`
  - Edit → navega para `/games/:id/edit`
  - Suspend → confirmação modal, depois `POST /api/services/app/AdminGame/Suspend`

### GameDetailComponent

- **Cabeçalho**: título, slug, status badge, nome do desenvolvedor
- **Seções**:
  - Informações básicas (short description, description, instructions, age rating, orientation, platforms)
  - Thumbnail e hero image (preview)
  - Histórico de builds (tabela: version, status, upload date, size, hash)
  - Histórico de moderação (tabela: reviewer, decision, notes, dates)
- **Ações**: Approve Build, Reject Build, Publish, Suspend
- **API**: `GET /api/services/app/AdminGame/GetDetail?gameId={id}`

### GameEditComponent

- **Formulário**: todos os campos de metadados do jogo (title, shortDescription, description, instructions, ageRating, orientation, platforms, categories, tags)
- **Ações**: Save (chama `POST /api/services/app/DeveloperGame/UpdateMetadata`), Cancel
- **API**: `GET /api/services/app/AdminGame/GetDetail?gameId={id}` para carregar dados

### ReviewQueueComponent

- **Tabela paginada** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | GameTitle | `review.gameTitle` |
  | SubmittedDate | `review.createdAt` |
  | Developer | lookup por gameId |
  | BuildVersion | `review.gameBuildId` → version |
  | Actions | Botão "Review" |
- **Filtro**: apenas pendentes (status = Pending)
- **Ação Review**: navega para `/moderation/:id`
- **API**: `GET /api/services/app/Moderation/GetPendingReviews`

### ReviewDetailComponent

- **Cabeçalho**: game title, build version, developer name
- **Seções**:
  - Informações do jogo (metadados)
  - Relatório de validação do build (isValid, errors, warnings, package size, hash)
  - Histórico de moderação anterior do mesmo jogo
- **Ações**:
  - **Approve**: abre modal para notas opcionais → `POST /api/services/app/Moderation/CompleteReview` com decision=Approved
  - **Reject**: abre modal para motivo obrigatório → `POST /api/services/app/Moderation/CompleteReview` com decision=Rejected
  - **Require Changes**: abre modal para notas obrigatórias → `POST /api/services/app/Moderation/CompleteReview` com decision=RequiresChanges
- **API**: `GET /api/services/app/Moderation/GetDetail?reviewId={id}`

### CategoryListComponent

- **Tabela** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | Name | `category.name` |
  | Slug | `category.slug` |
  | SortOrder | `category.sortOrder` |
  | IsActive | toggle switch |
  | Actions | Edit, Delete |
- **Ação Create**: navega para `/categories/create`
- **Ação Edit**: navega para `/categories/:id/edit`
- **Ação Delete**: confirmação modal → `DELETE /api/services/app/Category/Delete?id={id}`
- **Ação Toggle**: `POST /api/services/app/Category/CreateOrUpdate` com isActive invertido
- **API**: `GET /api/services/app/Category/GetAll`

### CategoryEditComponent

- **Formulário**: name (required), slug (auto-gerado a partir de name, editável), sortOrder, isActive
- **Ações**: Save → `POST /api/services/app/Category/CreateOrUpdate`, Cancel
- **Modo Create vs Edit**: detectado pela presença de `:id` na rota

### TagListComponent

- **Tabela** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | Name | `tag.name` |
  | Slug | `tag.slug` |
  | Actions | Edit, Delete |
- **Ação Create**: navega para `/tags/create`
- **Ação Edit**: navega para `/tags/:id/edit`
- **Ação Delete**: confirmação modal → `DELETE /api/services/app/Tag/Delete?id={id}`
- **API**: `GET /api/services/app/Tag/GetAll`

### TagEditComponent

- **Formulário**: name (required), slug (auto-gerado, editável)
- **Ações**: Save → `POST /api/services/app/Tag/CreateOrUpdate`, Cancel

### DashboardComponent

- **Cards de métricas** (4 cards):
  | Card | Valor | API |
  |------|-------|-----|
  | Total Games | número | `GET /api/services/app/AdminDashboard/GetSummary` |
  | Pending Reviews | número | `GET /api/services/app/AdminDashboard/GetSummary` |
  | Total Plays | número formatado (1.2k, 3.5M) | `GET /api/services/app/AdminDashboard/GetSummary` |
  | Active Users (7d) | número | `GET /api/services/app/AdminDashboard/GetSummary` |
- **Gráfico de plays ao longo do tempo**: line chart (chart.js ou similar leve)
- **API**: `GET /api/services/app/AdminDashboard/GetPlaysOverTime?days=30`

### FeatureFlagsComponent

- **Tabela** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | Name | `flag.name` |
  | Description | `flag.description` |
  | IsEnabled | toggle switch |
  | Actions | Toggle |
- **Ação Toggle**: `PUT /api/services/app/FeatureFlag/Toggle?id={id}&isEnabled={bool}`
- **API**: `GET /api/services/app/FeatureFlag/GetAll`

### AuditLogComponent

- **Tabela paginada** com colunas:
  | Coluna | Campo |
  |--------|-------|
  | Timestamp | `log.creationTime` |
  | User | `log.userName` |
  | Action | `log.action` |
  | Entity | `log.entityType` + `log.entityId` |
  | Details | expandível (JSON) |
- **Filtros**: data início, data fim, action type, user
- **Paginação**: skipCount + maxResultCount
- **API**: `GET /api/services/app/AuditLog/GetAll`

## HTTP Interceptors

### JwtInterceptor

```typescript
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
      });
    }
    return next.handle(req);
  }
}
```

### ErrorInterceptor

```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.tokenService.clear();
          this.router.navigate(['/login']);
          this.toastService.error('Session expired. Please login again.');
        } else if (error.status === 403) {
          this.toastService.error('You do not have permission to perform this action.');
        } else if (error.status === 429) {
          this.toastService.warning('Too many requests. Please try again later.');
        } else if (error.error?.error?.message) {
          this.toastService.error(error.error.error.message);
        } else {
          this.toastService.error('An unexpected error occurred.');
        }
        return throwError(() => error);
      })
    );
  }
}
```

### CorrelationIdInterceptor

```typescript
@Injectable()
export class CorrelationIdInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const correlationId = crypto.randomUUID();
    req = req.clone({
      setHeaders: { 'X-Correlation-ID': correlationId },
    });
    return next.handle(req);
  }
}
```

**Ordem de interceptors**: CorrelationId → Jwt → Error

## Shared Components

Como é um app Angular separado, criar `shared/` local com componentes reutilizáveis:

```text
shared/
  components/
    sidebar/          ← navegação lateral fixa
    header/           ← barra superior com user info + logout
    data-table/       ← tabela genérica com sorting + pagination
    confirm-dialog/   ← modal de confirmação (Delete, Suspend, etc.)
    toast/            ← notificações toast (success, error, warning, info)
    skeleton/         ← loading skeleton para tabelas e cards
    badge/            ← badge de status (colors por status)
    pagination/       ← controles de paginação
  pipes/
    date.pipe.ts      ← formatação de datas
    truncate.pipe.ts  ← truncamento de texto
  models/
    api-response.model.ts
    paged-result.model.ts
```

## State Management

MVP: services + RxJS signals (Angular 20+).

```typescript
// Exemplo: game-list.service.ts
@Injectable({ providedIn: 'root' })
export class GameListService {
  private games = signal<GameCardDto[]>([]);
  private loading = signal(false);
  private total = signal(0);

  readonly games$ = this.games.asReadonly();
  readonly loading$ = this.loading.asReadonly();
  readonly total$ = this.total.asReadonly();

  async loadGames(page: number, status?: string): Promise<void> {
    this.loading.set(true);
    try {
      const result = await this.api.getGames(page, 20, status);
      this.games.set(result.items);
      this.total.set(result.totalCount);
    } finally {
      this.loading.set(false);
    }
  }
}
```

## Error Handling

### Toast Notifications

| Tipo | Quando |
|------|--------|
| success | Operação concluída (save, delete, approve, reject) |
| error | Erro de API, validação, permissão |
| warning | Rate limit, dados incompletos |
| info | Informação geral (e.g., "Build is being validated") |

### Confirmation Modals

Ações destrutivas exigem confirmação:

| Ação | Modal |
|------|-------|
| Delete Category | "Are you sure you want to delete category '{name}'?" |
| Delete Tag | "Are you sure you want to delete tag '{name}'?" |
| Suspend Game | "Are you sure you want to suspend game '{title}'? Reason required." |
| Reject Build | "Provide reason for rejection (required)." |

### Loading States

- Skeleton loading para tabelas durante carregamento inicial
- Spinner inline para botões de ação (submit, toggle)
- Empty state com mensagem quando tabela sem dados

## i18n

Mesmo approach do Game Hub:

- Backend: `IStringLocalizer` do ABP para mensagens de erro e labels
- Frontend: `@angular/localize` com arquivos `.xlf`
- Traduções: `src/assets/i18n/pt-BR.xlf`, `src/assets/i18n/en-US.xlf`
- Locale padrão: `pt-BR`
- Formatos de data/número: `DatePipe` e `DecimalPipe` com locale

## Testes

| Tipo | Cobertura | Framework |
|------|-----------|-----------|
| Unit | Services, pipes, guards, components isolados | Jest / Karma |
| Integration | Componentes com mock services | Jest / Karma |
| E2E | Login, CRUD categories, moderation flow | Playwright |

Cobertura mínima: 80% para services e guards.
