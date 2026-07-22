import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameCatalogService, GameCard, Category, PagedGames } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-games',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './games.component.html',
  styleUrl: './games.component.css',
})
export class GamesComponent implements OnInit {
  games: GameCard[] = [];
  categories: Category[] = [];
  totalCount = 0;
  loading = false;
  query = '';
  category = '';
  tag = '';
  searchPage = false;
  skipCount = 0;
  readonly pageSize = 24;

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

  constructor(
    private catalog: GameCatalogService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.searchPage = this.router.url.startsWith('/search');
    this.route.queryParams.subscribe(params => {
      this.query = params['q'] || '';
      this.category = params['category'] || '';
      this.tag = params['tag'] || '';
      this.skipCount = 0;
      this.games = [];
      this.loadCategories();
      this.loadPage();
    });
  }

  loadCategories(): void {
    this.catalog.getHome().subscribe(h => {
      this.categories = h?.categories ?? [];
    });
  }

  loadPage(): void {
    this.loading = true;
    const hasSearch = this.query.trim() || this.tag;
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
          undefined,
          undefined,
          undefined,
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
      queryParams: { q: this.query.trim() || null, category: this.category || null, tag: this.tag || null },
      queryParamsHandling: 'merge',
    });
  }

  clear(): void {
    this.query = '';
    this.category = '';
    this.tag = '';
    this.router.navigate(['/games']);
  }

  loadMore(): void {
    this.skipCount += this.pageSize;
    this.loadPage();
  }
}
