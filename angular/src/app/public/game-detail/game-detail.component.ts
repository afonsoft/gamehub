import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameCatalogService, GameDetail, GameCard } from '../../core/services/game-catalog.service';
import { ReportService } from '../../core/services/report.service';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { BadgeComponent } from '../../shared/ui/badge/badge.component';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';

@Component({
  selector: 'app-game-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, TranslatePipe, BadgeComponent, SkeletonComponent, ButtonComponent],
  templateUrl: './game-detail.component.html',
  styleUrl: './game-detail.component.css',
})
export class GameDetailComponent implements OnInit {
  game: GameDetail | null = null;
  loaded = false;
  isFavorite = false;
  reportOpen = false;
  reportReason = '';
  reportDescription = '';
  reportSending = false;
  reportSent = false;

  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly catalog = inject(GameCatalogService);
  private readonly reportService = inject(ReportService);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.load(slug);
  }

  load(slug: string): void {
    this.loaded = false;
    this.catalog.getBySlug(slug).subscribe({
      next: g => {
        this.game = g;
        this.loaded = true;
        if (g) {
          this.isFavorite = this.checkFavorite(g.id);
        }
      },
      error: () => {
        this.loaded = true;
      },
    });
  }

  toggleFavorite(): void {
    if (!this.game) return;
    this.isFavorite = !this.isFavorite;
    const favorites = new Set(this.getFavorites());
    if (this.isFavorite) {
      favorites.add(this.game.id);
    } else {
      favorites.delete(this.game.id);
    }
    localStorage.setItem('gamehub-favorites', JSON.stringify([...favorites]));
  }

  openReport(): void {
    this.reportOpen = true;
    this.reportSent = false;
  }

  closeReport(): void {
    this.reportOpen = false;
  }

  submitReport(): void {
    if (!this.game || !this.reportReason || this.reportSending) return;
    this.reportSending = true;
    this.reportService.submit({
      gameId: this.game.id,
      reason: this.reportReason,
      description: this.reportDescription,
    }).subscribe({
      next: () => {
        this.reportSending = false;
        this.reportSent = true;
        this.reportReason = '';
        this.reportDescription = '';
        setTimeout(() => this.closeReport(), 1500);
      },
      error: () => {
        this.reportSending = false;
      },
    });
  }

  isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  trackGame(_index: number, game: GameCard): string {
    return game.id;
  }

  private checkFavorite(gameId: string): boolean {
    return this.getFavorites().includes(gameId);
  }

  private getFavorites(): string[] {
    try {
      const raw = localStorage.getItem('gamehub-favorites');
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }
}
