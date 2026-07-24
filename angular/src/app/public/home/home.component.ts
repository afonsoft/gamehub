import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { GameCatalogService, HomeResponse } from '../../core/services/game-catalog.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { LanguageSelectorComponent } from '../../shared/ui/language-selector/language-selector.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, ButtonComponent, LanguageSelectorComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  home: HomeResponse | null = null;
  loaded = false;
  searchQuery = '';
  selectedCategory: string | null = null;

  constructor(
    private catalog: GameCatalogService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.catalog.getHome().subscribe({
      next: response => {
        this.home = response ?? null;
        this.loaded = true;
      },
      error: () => {
        this.loaded = true;
      },
    });
  }

  doSearch(): void {
    const query = this.searchQuery.trim();
    if (query.length >= 2) {
      this.router.navigate(['/games'], { queryParams: { q: query } });
    }
  }

  selectCategory(slug: string): void {
    this.selectedCategory = this.selectedCategory === slug ? null : slug;
    this.router.navigate(['/games'], {
      queryParams: { category: this.selectedCategory },
    });
  }

  hasAnyGames(): boolean {
    return !!(
      this.home?.highlights?.length ||
      this.home?.mostPlayed?.length ||
      this.home?.trending?.length ||
      this.home?.newGames?.length ||
      this.home?.popularThisWeek?.length ||
      this.home?.topFree?.length ||
      this.home?.webExclusives?.length
    );
  }
}
