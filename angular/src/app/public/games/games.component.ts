import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { GameCatalogService, GameCard, Category, PagedGames } from '../../core/services/game-catalog.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { BadgeComponent } from '../../shared/ui/badge/badge.component';

@Component({
  selector: 'app-games',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, SkeletonComponent, ButtonComponent, BadgeComponent],
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
  searchPage = false;
  skipCount = 0;
  readonly pageSize = 24;

  private readonly query$ = new Subject<string>();
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalog = inject(GameCatalogService);
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
    });
  }

  loadPage(): void {
    this.loading = true;
    const hasSearch = this.query.trim() || this.tag || this.category || this.device || this.orientation;
    const request$ = hasSearch
      ? this.catalog.search(
          this.query.trim(),
          this.category ? [this.category] : [],
          this.tag ? [this.tag] : [],
          this.skipCount,
          this.pageSize,
        )
      : this.catalog.getGames(
          this.skipCount,
          this.pageSize,
          'MostPlayed',
          this.category || undefined,
          this.tag || undefined,
          this.device || undefined,
          this.orientation || undefined,
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
}
