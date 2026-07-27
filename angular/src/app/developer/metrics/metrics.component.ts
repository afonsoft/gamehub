import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, GameMetricsFilter, GameMetricsResult } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface MetricsPageState {
  loading: boolean;
  error: SdkError | null;
}

@Component({
  selector: 'app-game-metrics',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonComponent, TranslatePipe],
  templateUrl: './metrics.component.html',
  styleUrl: './metrics.component.css',
})
export class GameMetricsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  gameId = '';
  gameTitle = '';

  readonly state = signal<MetricsPageState>({ loading: true, error: null });
  readonly result = signal<GameMetricsResult | null>(null);

  readonly from = signal<string>('');
  readonly to = signal<string>('');
  readonly countryCode = signal<string>('');
  readonly deviceType = signal<string>('');
  readonly trafficSource = signal<string>('');
  readonly utmSource = signal<string>('');
  readonly utmMedium = signal<string>('');
  readonly utmCampaign = signal<string>('');

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id') ?? '';
    this.gameTitle = history.state?.['title'] ?? 'Game';
    this.loadMetrics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMetrics(): void {
    if (!this.gameId) {
      this.state.update(s => ({ ...s, loading: false, error: { code: 'not_found', message: 'Game not found.', retryable: false } }));
      return;
    }

    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.developerService.getGameMetrics(this.gameId, this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.result.set(result ?? null);
          this.state.update(s => ({ ...s, loading: false }));
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, loading: false, error: this.errorMapper.map(err) })),
      });
  }

  applyFilter(): void {
    if (this.from() && this.to() && this.from() > this.to()) {
      this.state.update(s => ({ ...s, error: { code: 'validation_failed', message: 'dev.dateRangeError', retryable: false } }));
      return;
    }
    this.loadMetrics();
  }

  resetFilter(): void {
    this.from.set('');
    this.to.set('');
    this.countryCode.set('');
    this.deviceType.set('');
    this.trafficSource.set('');
    this.utmSource.set('');
    this.utmMedium.set('');
    this.utmCampaign.set('');
    this.loadMetrics();
  }

  exportCsv(): void {
    if (!this.gameId) return;
    this.developerService.exportGameMetricsCsv(this.gameId, this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => this.downloadBlob(blob, `metrics-${this.gameId}-${this.formatDateForFile()}.csv`),
        error: (err: unknown) => this.state.update(s => ({ ...s, error: this.errorMapper.map(err) })),
      });
  }

  formatDuration(seconds: number): string {
    return `${Math.round(seconds || 0)}s`;
  }

  private buildFilter(): GameMetricsFilter {
    return {
      from: this.from() || undefined,
      to: this.to() || undefined,
      countryCode: this.countryCode() || undefined,
      deviceType: this.deviceType() || undefined,
      trafficSource: this.trafficSource() || undefined,
      utmSource: this.utmSource() || undefined,
      utmMedium: this.utmMedium() || undefined,
      utmCampaign: this.utmCampaign() || undefined,
    };
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
