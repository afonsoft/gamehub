import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, GameSummary } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-games',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './games.component.html',
  styleUrl: './games.component.css',
})
export class DeveloperGamesComponent implements OnInit {
  games: GameSummary[] = [];
  loading = false;
  errorMessage = '';
  statusFilter = 'All';
  submissionMessage = '';

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.loadGames();
  }

  loadGames(): void {
    this.loading = true;
    this.errorMessage = '';
    this.developerService.getMyGames(0, 100).subscribe({
      next: result => {
        this.games = result?.items ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Unable to load your games. Try again.';
      },
    });
  }

  get filteredGames(): GameSummary[] {
    return this.statusFilter === 'All'
      ? this.games
      : this.games.filter(game => game.status === this.statusFilter);
  }

  canSubmitForReview(game: GameSummary): boolean {
    return (game.status === 'Draft' || game.status === 'Rejected') && game.latestBuildStatus === 'Approved';
  }

  submitForReview(game: GameSummary): void {
    if (!window.confirm(`Submit ${game.title} for review?`)) {
      return;
    }
    this.submissionMessage = '';
    this.developerService.submitForReview(game.id).subscribe({
      next: () => {
        game.status = 'InReview';
        this.submissionMessage = `${game.title} was submitted for review.`;
      },
      error: err => {
        this.submissionMessage = err?.error?.error?.message || 'Unable to submit for review.';
      },
    });
  }
}
