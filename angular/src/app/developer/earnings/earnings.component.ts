import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, DeveloperEarnings, GameEarnings, DailyEarnings } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

export interface EarningsPageState {
  loading: boolean;
  empty: boolean;
  error: SdkError | null;
  earnings: DeveloperEarnings | null;
}

@Component({
  selector: 'app-developer-earnings',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, TranslatePipe],
  templateUrl: './earnings.component.html',
  styleUrl: './earnings.component.css',
})
export class DeveloperEarningsComponent implements OnInit, OnDestroy {
  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  readonly state = signal<EarningsPageState>({
    loading: false,
    empty: false,
    error: null,
    earnings: null,
  });

  from = signal<string>('');
  to = signal<string>('');
  expandedGames = signal<Set<string>>(new Set<string>());

  readonly anyExpanded = computed(() => this.expandedGames().size > 0);
  readonly earnings = computed(() => this.state().earnings);

  ngOnInit(): void {
    this.loadEarnings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadEarnings(): void {
    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.developerService.getEarnings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.state.set({
            loading: false,
            empty: !result || result.games.length === 0,
            error: null,
            earnings: result,
          });
        },
        error: err => {
          this.state.set({
            loading: false,
            empty: false,
            error: this.errorMapper.map(err),
            earnings: null,
          });
        },
      });
  }

  applyFilter(): void {
    if (this.from() && this.to() && this.from() > this.to()) {
      this.state.update(s => ({
        ...s,
        error: {
          code: 'validation_failed',
          message: 'The start date cannot be after the end date.',
          retryable: false,
        },
      }));
      return;
    }

    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.developerService.getEarnings({
      from: this.from() || undefined,
      to: this.to() || undefined,
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.state.set({
            loading: false,
            empty: !result || result.games.length === 0,
            error: null,
            earnings: result,
          });
        },
        error: err => {
          this.state.set({
            loading: false,
            empty: false,
            error: this.errorMapper.map(err),
            earnings: null,
          });
        },
      });
  }

  exportCsv(): void {
    const filter = {
      from: this.from() || undefined,
      to: this.to() || undefined,
    };
    this.developerService.exportEarningsCsv(filter).subscribe({
      next: blob => this.downloadBlob(blob, `earnings-${this.formatDateForFile()}.csv`),
      error: () => {
        this.state.update(s => ({
          ...s,
          error: { code: 'temporarily_unavailable', message: 'Unable to export earnings. Try again.', retryable: true },
        }));
      },
    });
  }

  toggleDaily(gameId: string): void {
    this.expandedGames.update(set => {
      const next = new Set(set);
      if (next.has(gameId)) {
        next.delete(gameId);
      } else {
        next.add(gameId);
      }
      return next;
    });
  }

  isDailyExpanded(gameId: string): boolean {
    return this.expandedGames().has(gameId);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat(navigator.language || 'en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  formatPercent(value: number): string {
    return `${(value * 100).toFixed(0)}%`;
  }

  trackByGame(index: number, item: GameEarnings): string {
    return item.gameId;
  }

  trackByDay(index: number, item: DailyEarnings): string {
    return item.date;
  }

  private formatDateForFile(): string {
    const now = new Date();
    return `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
