import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LeaderboardEntry, LeaderboardService } from '../../core/services/leaderboard.service';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
})
export class LeaderboardComponent implements OnInit {
  game: GameDetail | null = null;
  entries: LeaderboardEntry[] = [];
  score = 0;
  loaded = false;
  submitting = false;

  private readonly route = inject(ActivatedRoute);
  private readonly leaderboardService = inject(LeaderboardService);
  private readonly catalog = inject(GameCatalogService);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe({
      next: game => {
        this.game = game;
        this.loaded = true;
        if (game) {
          this.loadLeaderboard(game.id);
        }
      },
      error: () => {
        this.loaded = true;
      },
    });
  }

  loadLeaderboard(gameId: string): void {
    this.leaderboardService.getTop(gameId, 50).subscribe({
      next: entries => {
        this.entries = entries ?? [];
      },
      error: () => {
        this.entries = [];
      },
    });
  }

  submitScore(): void {
    if (!this.game || this.score <= 0 || this.submitting) {
      return;
    }
    this.submitting = true;
    this.leaderboardService.submitScore(this.game.id, this.score).subscribe({
      next: () => {
        this.submitting = false;
        this.score = 0;
        this.loadLeaderboard(this.game!.id);
      },
      error: () => {
        this.submitting = false;
      },
    });
  }
}
