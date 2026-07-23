import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameCatalogService, GameDetail, GameCard, GameVoteResult } from '../../core/services/game-catalog.service';
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
  userVote: 'Like' | 'Dislike' | null = null;
  voting = false;
  reportOpen = false;
  reportReason = '';
  reportDescription = '';
  reportSending = false;
  reportSent = false;
  descriptionExpanded = false;

  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly catalog = inject(GameCatalogService);
  private readonly reportService = inject(ReportService);
  private readonly auth = inject(AuthService);
  private readonly titleService = inject(Title);
  private readonly metaService = inject(Meta);

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
        this.setSeo(g);
        if (g) {
          this.isFavorite = this.checkFavorite(g.id);
          this.loadVote(g.id);
        }
      },
      error: () => {
        this.loaded = true;
      },
    });
  }

  vote(type: 'Like' | 'Dislike'): void {
    if (!this.game || this.voting) return;
    this.voting = true;
    this.catalog.vote(this.game.id, type, this.getDeviceId()).subscribe({
      next: (result: GameVoteResult) => {
        this.voting = false;
        this.userVote = result.userVote ?? null;
        this.game!.totalLikes = result.totalLikes;
        this.game!.totalDislikes = result.totalDislikes;
        this.updateLocalRating();
      },
      error: () => {
        this.voting = false;
      },
    });
  }

  goToCategory(slug: string): void {
    void this.router.navigate(['/games'], { queryParams: { category: slug } });
  }

  toggleDescription(): void {
    this.descriptionExpanded = !this.descriptionExpanded;
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

  openReport(reason = ''): void {
    this.reportOpen = true;
    this.reportSent = false;
    this.reportReason = reason;
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

  private loadVote(gameId: string): void {
    this.catalog.getVote(gameId, this.getDeviceId()).subscribe({
      next: (result: GameVoteResult) => {
        this.userVote = result.userVote ?? null;
      },
      error: () => {
        this.userVote = null;
      },
    });
  }

  private getDeviceId(): string {
    try {
      let id = localStorage.getItem('gamehub-device-id');
      if (!id) {
        id = this.generateId();
        localStorage.setItem('gamehub-device-id', id);
      }
      return id;
    } catch {
      return '';
    }
  }

  private updateLocalRating(): void {
    if (!this.game) return;
    const totalVotes = this.game.totalLikes + this.game.totalDislikes;
    this.game.totalVotes = totalVotes;
    this.game.averageRating = totalVotes > 0 ? (this.game.totalLikes / totalVotes) * 5 : 0;
  }

  private setSeo(game: GameDetail | null): void {
    if (!game) return;
    const title = `${game.title} - GameHub`;
    const description = game.seoDescription?.trim() || game.shortDescription;
    this.titleService.setTitle(title);
    this.metaService.updateTag({ name: 'description', content: description });
  }

  private generateId(): string {
    if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
      return crypto.randomUUID();
    }
    return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
  }
}
