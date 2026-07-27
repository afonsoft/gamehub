import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { GameCatalogService, GameCard, Category, PagedGames } from '../../core/services/game-catalog.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { GameCardComponent } from '../../shared/ui/game-card/game-card.component';

@Component({
  selector: 'app-games',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, SkeletonComponent, ButtonComponent, GameCardComponent],
  templateUrl: './games.component.html',
  styleUrl: './games.component.css',
})
export class GamesComponent implements OnInit, OnDestroy {
  games: GameCard[] = [];
  categories: Category[] = [];
  totalCount = 0;
  loading = false;
  query = '';
  category = '';
  tag = '';
  device = '';
  orientation = '';
  exclusivity: 'All' | 'WebExclusive' | 'NonExclusive' = 'All';
  minRating = 0;
  searchPage = false;
  skipCount = 0;
  readonly pageSize = 24;

  private readonly query$ = new Subject<string>();
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalog = inject(GameCatalogService);
  private readonly titleService = inject(Title);
  private readonly metaService = inject(Meta);
  private subscription?: Subscription;

  get title(): string {
    if (this.searchPage && !this.query && !this.category && !this.tag) return 'Search Games';
    if (this.query) return `Results for "${this.query}"`;
    if (this.category) {
      const cat = this.categories.find(c => c.slug === this.category);
      return cat ? cat.name : 'Games';
    }
    if (this.tag) return `Games tagged #${this.tag}`;
    return 'All Games';
  }

  ngOnInit(): void {
    this.searchPage = this.router.url.startsWith('/search');
    this.subscription = this.query$.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => this.reload());
    this.route.queryParams.subscribe(params => {
      this.query = params['q'] || '';
      this.category = params['category'] || '';
      this.tag = params['tag'] || '';
      this.device = params['device'] || '';
      this.orientation = params['orientation'] || '';
      this.exclusivity = params['exclusivity'] || 'All';
      this.minRating = Number(params['minRating'] || 0);
      this.skipCount = 0;
      this.games = [];
      this.loadCategories();
      this.loadPage();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onQueryChange(value: string): void {
    this.query = value;
    this.query$.next(value);
  }

  loadCategories(): void {
    this.catalog.getHome().subscribe(h => {
      this.categories = h?.categories ?? [];
      this.setSeo();
    });
  }

  loadPage(): void {
    this.loading = true;
    this.setSeo();
    const hasSearch = this.query.trim() || this.tag || this.category || this.device || this.orientation || this.exclusivity !== 'All' || this.minRating > 0;
    const request$ = hasSearch
      ? this.catalog.search(
          this.query.trim(),
          this.category ? [this.category] : [],
          this.tag ? [this.tag] : [],
          this.skipCount,
          this.pageSize,
          this.device || undefined,
          this.orientation || undefined,
          this.exclusivity,
          this.minRating,
        )
      : this.catalog.getGames(
          this.skipCount,
          this.pageSize,
          'MostPlayed',
          this.category || undefined,
          this.tag || undefined,
          this.device || undefined,
          this.orientation || undefined,
          this.exclusivity,
          this.minRating,
        );

    request$.subscribe({
      next: (result: PagedGames) => {
        const items = result?.items ?? [];
        this.totalCount = result?.totalCount ?? 0;
        this.games = this.skipCount === 0 ? items : [...this.games, ...items];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  reload(): void {
    this.router.navigate(['/games'], {
      queryParams: {
        q: this.query.trim() || null,
        category: this.category || null,
        tag: this.tag || null,
        device: this.device || null,
        orientation: this.orientation || null,
        exclusivity: this.exclusivity === 'All' ? null : this.exclusivity,
        minRating: this.minRating || null,
      },
      queryParamsHandling: 'merge',
    });
  }

  clear(): void {
    this.router.navigate(['/games']);
  }

  loadMore(): void {
    this.skipCount += this.pageSize;
    this.loadPage();
  }

  private setSeo(): void {
    const title = this.buildSeoTitle();
    this.titleService.setTitle(title);
    this.metaService.updateTag({ name: 'description', content: this.buildSeoDescription() });
    this.metaService.updateTag({ name: 'keywords', content: this.buildSeoKeywords() });
  }

  private buildSeoTitle(): string {
    if (this.query) return `GameHub - Search: ${this.query}`;
    if (this.category) {
      const cat = this.categories.find(c => c.slug === this.category);
      return cat ? `GameHub - ${cat.name} Games` : 'GameHub - Games by Category';
    }
    if (this.tag) return `GameHub - #${this.tag} Games`;
    return 'GameHub - Free Online Games';
  }

  private buildSeoDescription(): string {
    if (this.category) {
      const cat = this.categories.find(c => c.slug === this.category);
      if (cat?.description) return cat.description;
      return cat ? `Play the best ${cat.name} games on GameHub. No downloads, no login required.` : '';
    }
    if (this.tag) return `Explore free games tagged #${this.tag} on GameHub.`;
    if (this.query) return `Search results for "${this.query}" on GameHub.`;
    return 'Discover and play free HTML5 games on GameHub. No downloads required.';
  }

  private buildSeoKeywords(): string {
    if (this.category) {
      const cat = this.categories.find(c => c.slug === this.category);
      return cat?.keywords ?? '';
    }
    return '';
  }
}
