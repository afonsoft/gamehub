import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PlayerService, PlayerFavorite, PlayerRecentGame } from '../../core/services/player.service';
import { TokenService } from '../../core/auth/token.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { GameCard } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-player',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
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

  load(): void {
    this.loading = true;
    this.playerService.getFavorites().subscribe({
      next: favorites => {
        this.favorites = favorites;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });

    this.playerService.getRecent().subscribe({
      next: recent => {
        this.recent = recent;
      },
    });

    if (this.isAuthenticated) {
      this.playerService.mergeLocalData().subscribe(() => {
        this.playerService.clearLocalData();
        this.playerService.getFavorites().subscribe(f => (this.favorites = f));
        this.playerService.getRecent().subscribe(r => (this.recent = r));
      });
    }
  }

  removeFavorite(gameId: string): void {
    this.playerService.toggleFavorite(gameId, this.isAuthenticated).subscribe(() => this.load());
  }

  trackByGame(_: number, item: { game: GameCard }): string {
    return item.game.id;
  }
}
