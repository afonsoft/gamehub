# 06 - Frontend Angular (Game Hub)

## Visão geral

O Game Hub é a aplicação Angular voltada para jogadores e desenvolvedores. A aplicação admin é separada (ver `angular-admin/` e `06b-frontend-admin.md`).

Ambos usam Angular 20+ com design system próprio (NOT Angular Material).

**DNS produção**: `gamehub.afonsoft.dev`

**API URL**: `https://gamehub-api.afonsoft.dev`

**Dockerfile**: Utilizar Dockerfile existente do template EAF.

## Estrutura do Game Hub

```text
angular/src/app/
  core/
    auth/
      auth.service.ts
      token.service.ts
      auth.guard.ts
      developer.guard.ts
      guest.guard.ts
    http/
      http-interceptor.service.ts
    guards/
      auth.guard.ts
      developer.guard.ts
      guest.guard.ts
    interceptors/
      jwt.interceptor.ts
      error.interceptor.ts
      correlation-id.interceptor.ts
    telemetry/
      telemetry.service.ts
    services/
      api.service.ts
  shared/
    components/
      button/
      card/
      input/
      modal/
      table/
      badge/
      skeleton/
      toast/
      pagination/
      dropdown/
      tabs/
    pipes/
      date.pipe.ts
      truncate.pipe.ts
      safe-html.pipe.ts
    directives/
      lazy-image.directive.ts
    models/
      api-response.model.ts
      paged-result.model.ts
  public/
    home/
      home-page.component.ts
    catalog/
      catalog-page.component.ts
    game-detail/
      game-detail-page.component.ts
    search/
      search-page.component.ts
    login/
      login-page.component.ts
    register/
      register-page.component.ts
  player/
    game-shell/
      game-shell.component.ts
    game-frame/
      game-frame.component.ts
    gameplay-sdk/
      gameplay-bridge.service.ts
    leaderboard/
      leaderboard.component.ts
  developer/
    dashboard/
      developer-dashboard.component.ts
    games/
      developer-games.component.ts
      game-create.component.ts
      game-edit.component.ts
    builds/
      build-list.component.ts
      build-upload.component.ts
    profile/
      developer-profile.component.ts
```

## Rotas e Lazy Loading

```typescript
const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./public/public.module').then(m => m.PublicModule),
  },
  {
    path: 'play',
    canActivate: [AuthGuard],
    loadChildren: () => import('./player/player.module').then(m => m.PlayerModule),
  },
  {
    path: 'developer',
    canActivate: [AuthGuard, DeveloperGuard],
    loadChildren: () => import('./developer/developer.module').then(m => m.DeveloperModule),
  },
];
```

### Rotas detalhadas

| Rota | Componente | Module | Guard | Descrição |
|------|-----------|--------|-------|-----------|
| `/` | HomePageComponent | PublicModule | — | Home com seções (featured, trending, new) |
| `/games` | CatalogPageComponent | PublicModule | — | Catálogo paginado com filtros |
| `/games/:slug` | GameDetailPageComponent | PublicModule | — | Detalhe do jogo (público) |
| `/search` | SearchPageComponent | PublicModule | — | Busca com query e filtros |
| `/login` | LoginPageComponent | PublicModule | GuestGuard | Login |
| `/register` | RegisterPageComponent | PublicModule | GuestGuard | Registro |
| `/play/:slug` | GameShellComponent | PlayerModule | AuthGuard | Shell do jogo (iframe + SDK) |
| `/leaderboard/:gameId` | LeaderboardComponent | PlayerModule | AuthGuard | Ranking do jogo |
| `/developer` | DeveloperDashboardComponent | DeveloperModule | DeveloperGuard | Dashboard do dev |
| `/developer/games` | DeveloperGamesComponent | DeveloperModule | DeveloperGuard | Lista de jogos do dev |
| `/developer/games/create` | GameCreateComponent | DeveloperModule | DeveloperGuard | Criar novo jogo |
| `/developer/games/:id/edit` | GameEditComponent | DeveloperModule | DeveloperGuard | Editar metadados |
| `/developer/games/:id/builds` | BuildListComponent | DeveloperModule | DeveloperGuard | Histórico de builds |
| `/developer/profile` | DeveloperProfileComponent | DeveloperModule | DeveloperGuard | Perfil do dev |

## Guards

### AuthGuard

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

### DeveloperGuard

Verifica se o usuário possui role `Developer` ou `Admin`. Redireciona para `/` se não tiver.

```typescript
@Injectable({ providedIn: 'root' })
export class DeveloperGuard implements CanActivate {
  canActivate(): boolean {
    const roles = this.tokenService.getRoles();
    if (roles.includes('Developer') || roles.includes('Admin')) {
      return true;
    }
    this.router.navigate(['/']);
    return false;
  }
}
```

### GuestGuard

Se o token é válido, redireciona para `/`. Usado nas rotas de login e registro.

## HTTP Interceptors

### JwtInterceptor

```typescript
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token && !req.url.includes('/TokenAuth/')) {
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

**Ordem**: CorrelationId → Jwt → Error

## Game Shell

O `GameShellComponent` é responsável por:

1. Carregar dados públicos do jogo por slug (`GET /api/services/app/GameCatalog/GetBySlug?slug={slug}`)
2. Criar sessão de gameplay (`POST /api/services/app/Gameplay/StartSession`)
3. Renderizar iframe com sandbox apontando para `games.afonsoft.dev`
4. Injetar wrapper JS via `postMessage` bridge
5. Coletar eventos de gameplay via `GameplayBridgeService`
6. Encerrar sessão ao sair/fechar (`ngOnDestroy`)

### Sandboxing do iframe

```html
<iframe
  [src]="trustedGameUrl"
  sandbox="allow-scripts allow-pointer-lock allow-same-origin allow-forms"
  referrerpolicy="no-referrer"
  allow="fullscreen; gamepad"
  class="game-frame">
</iframe>
```

> `allow-same-origin` é necessário para jogos que usam APIs do browser. O isolamento real vem do domínio separado (`games.afonsoft.dev` vs `gamehub.afonsoft.dev`).

## GameplayBridge Interface (10 eventos)

```typescript
export interface GameplayBridge {
  gameLoadingStarted(): void;
  gameLoadingFinished(): void;
  gameplayStart(): void;
  gameplayStop(): void;
  commercialBreakRequested(): Promise<void>;
  commercialBreakCompleted(): void;
  rewardedBreakRequested(): Promise<boolean>;
  rewardedBreakCompleted(): void;
  gameErrorCaptured(error: Error | string): void;
  gameMeasuredEvent(category: string, what: string, action: string): void;
}
```

### GameplayBridgeService

```typescript
@Injectable({ providedIn: 'root' })
export class GameplayBridgeService implements GameplayBridge {
  private sessionId: string | null = null;
  private gameId: string | null = null;

  setSession(sessionId: string, gameId: string): void { ... }

  gameLoadingStarted(): void {
    this.sendEvent(GameplayEventType.GameLoadingStarted);
  }

  gameLoadingFinished(): void {
    this.sendEvent(GameplayEventType.GameLoadingFinished);
  }

  gameplayStart(): void {
    this.sendEvent(GameplayEventType.GameplayStarted);
  }

  gameplayStop(): void {
    this.sendEvent(GameplayEventType.GameplayStopped);
  }

  async commercialBreakRequested(): Promise<void> {
    this.sendEvent(GameplayEventType.CommercialBreakRequested);
    // Wait for ad completion via postMessage listener
    await this.waitForAdCompletion();
    this.sendEvent(GameplayEventType.CommercialBreakCompleted);
  }

  commercialBreakCompleted(): void {
    this.sendEvent(GameplayEventType.CommercialBreakCompleted);
  }

  async rewardedBreakRequested(): Promise<boolean> {
    this.sendEvent(GameplayEventType.RewardedBreakRequested);
    const completed = await this.waitForRewardedAd();
    this.sendEvent(GameplayEventType.RewardedBreakCompleted);
    return completed;
  }

  rewardedBreakCompleted(): void {
    this.sendEvent(GameplayEventType.RewardedBreakCompleted);
  }

  gameErrorCaptured(error: Error | string): void {
    this.sendEvent(GameplayEventType.GameErrorCaptured, error.toString());
  }

  gameMeasuredEvent(category: string, what: string, action: string): void {
    this.sendEvent(GameplayEventType.GameMeasuredEvent, JSON.stringify({ category, what, action }));
  }

  private sendEvent(type: GameplayEventType, payload?: string): void {
    this.api.post('/api/services/app/Gameplay/Event', {
      sessionId: this.sessionId,
      gameId: this.gameId,
      eventType: type,
      payloadJson: payload,
    }).subscribe();
  }
}
```

## Design Tokens

### Cores

| Token | Valor | Uso |
|-------|-------|-----|
| `--color-primary` | `#1a73e8` | Botões principais, links, ações |
| `--color-secondary` | `#5f6368` | Texto secundário, bordas |
| `--color-success` | `#34a853` | Status positivo, aprovações |
| `--color-warning` | `#fbbc04` | Alertas, pendências |
| `--color-error` | `#ea4335` | Erros, exclusões, rejeições |
| `--color-background` | `#ffffff` | Fundo da página |
| `--color-surface` | `#f8f9fa` | Cards, containers |
| `--color-text-primary` | `#202124` | Texto principal |
| `--color-text-secondary` | `#5f6368` | Texto secundário, labels |
| `--color-border` | `#dadce0` | Bordas de containers |

### Tipografia

| Token | Valor |
|-------|-------|
| `--font-family` | `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif` |
| `--font-size-h1` | `2rem` (32px) |
| `--font-size-h2` | `1.5rem` (24px) |
| `--font-size-h3` | `1.25rem` (20px) |
| `--font-size-body` | `1rem` (16px) |
| `--font-size-small` | `0.875rem` (14px) |
| `--font-weight-regular` | `400` |
| `--font-weight-medium` | `500` |
| `--font-weight-bold` | `700` |
| `--line-height` | `1.5` |

### Espaçamento (base 4px)

| Token | Valor |
|-------|-------|
| `--space-xs` | `4px` |
| `--space-sm` | `8px` |
| `--space-md` | `12px` |
| `--space-base` | `16px` |
| `--space-lg` | `24px` |
| `--space-xl` | `32px` |
| `--space-2xl` | `48px` |
| `--space-3xl` | `64px` |

### Border Radius

| Token | Valor |
|-------|-------|
| `--radius-sm` | `4px` |
| `--radius-md` | `8px` |
| `--radius-lg` | `12px` |
| `--radius-full` | `9999px` |

### Breakpoints

| Nome | Range |
|------|-------|
| mobile | 0 – 599px |
| tablet | 600 – 1023px |
| desktop | 1024px+ |

```scss
@mixin mobile { @media (max-width: 599px) { @content; } }
@mixin tablet { @media (min-width: 600px) and (max-width: 1023px) { @content; } }
@mixin desktop { @media (min-width: 1024px) { @content; } }
```

### Sombras

| Token | Valor |
|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(0,0,0,0.05)` |
| `--shadow-md` | `0 2px 4px rgba(0,0,0,0.1)` |
| `--shadow-lg` | `0 4px 8px rgba(0,0,0,0.15)` |

## Componentes do Design System

| Componente | Descrição |
|-----------|-----------|
| Button | Variants: primary, secondary, ghost, danger. Sizes: sm, md, lg |
| Card | Container com padding, border, shadow |
| Input | Text, number, textarea, select. Com label e error state |
| Modal | Dialog com backdrop, header, body, footer |
| Table | Genérica com columns config, sorting, pagination |
| Badge | Labels de status com cores (success, warning, error, info) |
| Skeleton | Loading placeholder para cards, textos, imagens |
| Toast | Notificações success, error, warning, info |
| Pagination | Navegação de páginas com prev/next e page numbers |
| Dropdown | Menu suspenso com opções |
| Tabs | Navegação por abas |

## UX Pública

- Home responsiva com cards leves
- Lazy loading de imagens nativo (`loading="lazy"`)
- Skeleton loading para todas as rotas assíncronas
- Paginação controlada (não infinite scroll no MVP)
- Busca com debounce (300ms)
- Filtros persistidos na URL (query params)
- Acessibilidade de teclado e foco visível
- Responsividade: mobile-first, tablet e desktop

## Developer Portal

- Wizard de submissão:
  1. Dados básicos (title, shortDescription, description, instructions, ageRating, orientation, platforms)
  2. Assets (thumbnail upload, hero image upload)
  3. Upload build (drag & drop, validação em tempo real)
  4. Validação (relatório de erros acionáveis)
  5. Envio para revisão (confirmação + notes opcionais)
- Exibir relatório de validação com erros acionáveis
- Status badges (Draft, InReview, Published, Rejected, Suspended)
- Build history com diff de versões

## i18n

### Backend

- `IStringLocalizer` do ABP para mensagens de erro e labels de entidades
- Localização via resource files no projeto `GameHub.Core`

### Frontend

- `@angular/localize` para traduções
- Arquivos `.xlf` em `src/assets/i18n/`
- Locale padrão: `pt-BR`

```
src/assets/i18n/
  pt-BR.xlf
  en-US.xlf
```

### Configuração

```typescript
// angular.json
"i18n": {
  "sourceLocale": "pt-BR",
  "locales": {
    "en-US": "src/assets/i18n/en-US.xlf"
  }
}
```

### Uso em componentes

```html
<h1 i18n="@@homeTitle">Welcome to GameHub</h1>
<p i18n="@@homeSubtitle">Discover and play the best web games</p>
```

## Error Handling

### Toast Notifications

| Tipo | Quando | Duração |
|------|--------|---------|
| success | Operação concluída (save, submit, upload) | 3s |
| error | Erro de API, validação, permissão | 5s (manual dismiss) |
| warning | Atenção (dados incompletos) | 4s |
| info | Informação geral | 3s |

### Confirmation Modals

Ações destrutivas exigem confirmação:

| Ação | Mensagem |
|------|----------|
| Submit for Review | "Are you sure you want to submit this game for review?" |
| Delete Draft | "Are you sure you want to delete this draft? This cannot be undone." |
| Report Game | "Are you sure you want to report this game?" |

### Loading States

- Skeleton loading para cards de jogos na home e catálogo
- Spinner inline para botões de ação
- Empty state com mensagem e CTA quando lista vazia
- Error state com botão retry quando falha na carga

## Estado

MVP: services + RxJS signals (Angular 20+).

```typescript
// Exemplo: catalog.service.ts
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private games = signal<GameCardDto[]>([]);
  private loading = signal(false);
  private total = signal(0);
  private filters = signal<SearchInput>({ skipCount: 0, maxResultCount: 24 });

  readonly games$ = this.games.asReadonly();
  readonly loading$ = this.loading.asReadonly();

  async loadGames(): Promise<void> {
    this.loading.set(true);
    try {
      const result = await firstValueFrom(
        this.http.get<SearchResultDto>('/api/services/app/GameCatalog/GetGames', {
          params: this.buildParams(this.filters()),
        })
      );
      this.games.set(result.items);
      this.total.set(result.totalCount);
    } finally {
      this.loading.set(false);
    }
  }
}
```

Avaliar Angular Signals antes de considerar NgRx. Para o MVP, signals + services são suficientes.

## Testes

| Tipo | Cobertura | Framework |
|------|-----------|-----------|
| Unit | Services, pipes, pure components, guards | Jest / Karma |
| Integration | Componentes connected com mock services | Jest / Karma |
| E2E | Login, browse catalog, play game, submit score | Playwright |

### Cobertura mínima

- Services: 90%
- Guards: 100%
- Components: 80%
- E2E: critical paths only (login → browse → play → leaderboard)

### Estratégia de mocks

- Services HTTP: `HttpTestingController` para testes unitários
- Components: `TestBed` com services mockados via `provideHttpClientTesting()`
- E2E: API mockada ou ambiente de staging dedicado
