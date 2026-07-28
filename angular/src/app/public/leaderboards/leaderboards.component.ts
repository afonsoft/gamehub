import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GameCatalogService, GameCard } from '../../core/services/game-catalog.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';

@Component({
  selector: 'app-leaderboards',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe, SkeletonComponent],
  templateUrl: './leaderboards.component.html',
  styleUrl: './leaderboards.component.css',
})
export class LeaderboardsComponent implements OnInit {
  games: GameCard[] = [];
  loading = false;
  loaded = false;

  private readonly catalog = inject(GameCatalogService);

  ngOnInit(): void {
    this.loadGames();
  }

  loadGames(): void {
    this.loading = true;
    this.catalog.getGames(0, 24, 'MostPlayed').subscribe({
      next: result => {
        this.games = result?.items ?? [];
        this.loaded = true;
        this.loading = false;
      },
      error: () => {
        this.games = [];
        this.loaded = true;
        this.loading = false;
      },
    });
  }
}
