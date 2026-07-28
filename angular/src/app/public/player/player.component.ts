import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { forkJoin } from 'rxjs';
import { PlayerService, PlayerFavorite, PlayerRecentGame } from '../../core/services/player.service';
import { TokenService } from '../../core/auth/token.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { GameCard } from '../../core/services/game-catalog.service';
import { GameCardComponent } from '../../shared/ui/game-card/game-card.component';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';

@Component({
  selector: 'app-player',
  standalone: true,
  imports: [CommonModule, TranslatePipe, GameCardComponent, SkeletonComponent],
  templateUrl: './player.component.html',
  styleUrl: './player.component.css',
})
export class PlayerComponent implements OnInit {
  activeTab: 'favorites' | 'recent' = 'favorites';
  favorites: PlayerFavorite[] = [];
  recent: PlayerRecentGame[] = [];
  loading = false;
  isAuthenticated = false;

  private readonly playerService = inject(PlayerService);
  private readonly tokenService = inject(TokenService);

  ngOnInit(): void {
    this.isAuthenticated = this.tokenService.isValid();
    this.load();
  }

  setTab(tab: 'favorites' | 'recent'): void {
    this.activeTab = tab;
  }

  get totalFavorites(): number {
    return this.favorites.length;
  }

  get totalRecent(): number {
    return this.recent.length;
  }

  get totalSessions(): number {
    return this.recent.reduce((sum, item) => sum + (item.totalSessions || 0), 0);
  }

  load(): void {
    this.loading = true;
    forkJoin({
      favorites: this.playerService.getFavorites(),
      recent: this.playerService.getRecent(),
    }).subscribe({
      next: ({ favorites, recent }) => {
        this.favorites = favorites;
        this.recent = recent;
        this.loading = false;

        if (this.isAuthenticated) {
          this.playerService.mergeLocalData().subscribe(() => {
            this.playerService.clearLocalData();
            this.playerService.getFavorites().subscribe(f => (this.favorites = f));
            this.playerService.getRecent().subscribe(r => (this.recent = r));
          });
        }
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  removeFavorite(gameId: string): void {
    this.playerService.toggleFavorite(gameId, this.isAuthenticated).subscribe(() => this.load());
  }

  trackByGame(_: number, item: { game: GameCard }): string {
    return item.game.id;
  }
}
