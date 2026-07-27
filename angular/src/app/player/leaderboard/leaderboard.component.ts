import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LeaderboardEntry, LeaderboardService } from '../../core/services/leaderboard.service';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { CardComponent } from '../../shared/ui/card/card.component';
import { BadgeComponent } from '../../shared/ui/badge/badge.component';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { SkeletonComponent } from '../../shared/ui/skeleton/skeleton.component';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, TranslatePipe, CardComponent, BadgeComponent, ButtonComponent, SkeletonComponent],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
})
export class LeaderboardComponent implements OnInit {
  game: GameDetail | null = null;
  entries: LeaderboardEntry[] = [];
  myRank: LeaderboardEntry | null = null;
  score = 0;
  loaded = false;
  loadingEntries = false;
  submitting = false;

  readonly pageSizes = [10, 25, 50];
  take = 10;

  private readonly route = inject(ActivatedRoute);
  private readonly leaderboardService = inject(LeaderboardService);
  private readonly catalog = inject(GameCatalogService);
  private readonly auth = inject(AuthService);

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
    this.loadingEntries = true;
    this.leaderboardService.getTop(gameId, this.take).subscribe({
      next: entries => {
        this.entries = entries ?? [];
        this.loadingEntries = false;
      },
      error: () => {
        this.entries = [];
        this.loadingEntries = false;
      },
    });

    if (this.isLoggedIn()) {
      this.leaderboardService.getMyRank(gameId).subscribe({
        next: rank => {
          this.myRank = rank ?? null;
        },
        error: () => {
          this.myRank = null;
        },
      });
    } else {
      this.myRank = null;
    }
  }

  setTake(take: number): void {
    if (this.take === take || !this.game) {
      return;
    }
    this.take = take;
    this.loadLeaderboard(this.game.id);
  }

  loadMore(): void {
    if (!this.game || !this.canLoadMore) {
      return;
    }
    const next = this.pageSizes.find(size => size > this.take) ?? this.pageSizes[this.pageSizes.length - 1];
    this.take = next;
    this.loadLeaderboard(this.game.id);
  }

  get canLoadMore(): boolean {
    return this.game !== null && this.take < this.pageSizes[this.pageSizes.length - 1] && this.entries.length >= this.take;
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

  isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }
}
