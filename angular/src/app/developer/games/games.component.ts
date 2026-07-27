import { Component, OnInit, OnDestroy, inject, signal, computed, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, GameSummary } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

export interface GamesPageState {
  loading: boolean;
  empty: boolean;
  error: SdkError | null;
  games: GameSummary[];
}

@Component({
  selector: 'app-developer-games',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonComponent, ConfirmDialogComponent, TranslatePipe],
  templateUrl: './games.component.html',
  styleUrl: './games.component.css',
})
export class DeveloperGamesComponent implements OnInit, OnDestroy {
  @ViewChild('submitConfirm') submitConfirm!: ConfirmDialogComponent;

  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  readonly state = signal<GamesPageState>({
    loading: false,
    empty: false,
    error: null,
    games: [],
  });

  statusFilter = signal<string>('All');
  submittingGameId = signal<string | null>(null);
  submissionMessage = signal<string>('');

  readonly filteredGames = computed(() => {
    const filter = this.statusFilter();
    const games = this.state().games;
    return filter === 'All'
      ? games
      : games.filter(game => game.status === filter);
  });

  ngOnInit(): void {
    this.loadGames();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadGames(): void {
    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.developerService.getMyGames(0, 100)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          const games = result?.items ?? [];
          this.state.set({
            loading: false,
            empty: games.length === 0,
            error: null,
            games,
          });
        },
        error: err => {
          this.state.set({
            loading: false,
            empty: false,
            error: this.errorMapper.map(err),
            games: [],
          });
        },
      });
  }

  canSubmitForReview(game: GameSummary): boolean {
    return (game.status === 'Draft' || game.status === 'Rejected') && game.latestBuildStatus === 'Approved';
  }

  async submitForReview(game: GameSummary): Promise<void> {
    const confirmed = await this.submitConfirm.open('dev.submitForReviewConfirm', 'dev.submitForReviewMessage');
    if (!confirmed) {
      return;
    }

    this.submissionMessage.set('');
    this.submittingGameId.set(game.id);

    this.developerService.submitForReview(game.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          game.status = 'InReview';
          this.submissionMessage.set(`${game.title} was submitted for review.`);
          this.submittingGameId.set(null);
        },
        error: err => {
          const error = this.errorMapper.map(err);
          this.submissionMessage.set(error.message);
          this.submittingGameId.set(null);
        },
      });
  }
}
