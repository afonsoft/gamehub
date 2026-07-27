import { Component, OnInit, OnDestroy, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  DeveloperReviewHistoryItem,
  DeveloperService,
  BuildItem,
  UploadResult,
} from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

export interface BuildsPageState {
  loading: boolean;
  empty: boolean;
  error: SdkError | null;
  builds: BuildItem[];
}

@Component({
  selector: 'app-developer-builds',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonComponent, ConfirmDialogComponent, TranslatePipe],
  templateUrl: './builds.component.html',
  styleUrl: './builds.component.css',
})
export class DeveloperBuildsComponent implements OnInit, OnDestroy {
  @ViewChild('approveConfirm') approveConfirm!: ConfirmDialogComponent;

  gameId = '';
  gameTitle = '';
  uploadResult = signal<UploadResult | null>(null);
  reviewHistory = signal<DeveloperReviewHistoryItem[]>([]);
  rejectingBuildId = signal<string | null>(null);
  rejectionReason = signal<string>('');
  statusMessage = signal<string>('');
  busyBuildIds = signal<Set<string>>(new Set<string>());
  uploading = signal<boolean>(false);

  readonly state = signal<BuildsPageState>({
    loading: false,
    empty: false,
    error: null,
    builds: [],
  });

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id') ?? '';
    this.gameTitle = history.state?.['title'] ?? 'Game';
    this.loadBuilds();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBuilds(): void {
    if (!this.gameId) return;

    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.developerService.getBuilds(this.gameId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => this.loadReviewHistory(result ?? []),
        error: err => {
          this.state.set({
            loading: false,
            empty: false,
            error: this.errorMapper.map(err),
            builds: [],
          });
        },
      });
  }

  private loadReviewHistory(builds: BuildItem[]): void {
    this.state.update(s => ({ ...s, builds }));
    this.developerService.getReviewHistory(this.gameId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: history => {
          this.reviewHistory.set(history ?? []);
          this.state.update(s => ({ ...s, loading: false, empty: s.builds.length === 0 }));
        },
        error: err => {
          this.state.update(s => ({
            ...s,
            loading: false,
            empty: s.builds.length === 0,
            error: this.errorMapper.map(err),
          }));
        },
      });
  }

  async approveBuild(build: BuildItem): Promise<void> {
    const confirmed = await this.approveConfirm.open('dev.approveBuildConfirm', 'dev.approveBuildConfirm');
    if (!confirmed) return;
    this.setBusy(build.id, true);
    this.developerService.approveBuild(build.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.statusMessage.set(`Build ${build.version} approved.`);
          this.setBusy(build.id, false);
          this.loadBuilds();
        },
        error: err => {
          this.statusMessage.set(this.errorMapper.map(err).message);
          this.setBusy(build.id, false);
        },
      });
  }

  beginRejectBuild(build: BuildItem): void {
    this.rejectingBuildId.set(build.id);
    this.rejectionReason.set('');
  }

  cancelRejectBuild(): void {
    this.rejectingBuildId.set(null);
    this.rejectionReason.set('');
  }

  rejectBuild(build: BuildItem): void {
    const reason = this.rejectionReason().trim();
    if (!reason) {
      this.statusMessage.set('Provide a reason before rejecting the build.');
      return;
    }
    this.setBusy(build.id, true);
    this.developerService.rejectBuild(build.id, reason)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.statusMessage.set(`Build ${build.version} rejected.`);
          this.cancelRejectBuild();
          this.setBusy(build.id, false);
          this.loadBuilds();
        },
        error: err => {
          this.statusMessage.set(this.errorMapper.map(err).message);
          this.setBusy(build.id, false);
        },
      });
  }

  openInspector(build: BuildItem): void {
    this.setBusy(build.id, true);
    this.developerService.startInspectorSession(build.gameId, build.id, 'desktop', '1024x768')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: session => {
          const url = this.router.serializeUrl(
            this.router.createUrlTree(['/games', build.gameSlug], {
              queryParams: { inspector: '1', inspectorSession: session.id },
            })
          );
          window.open(url, '_blank');
          this.setBusy(build.id, false);
        },
        error: err => {
          this.statusMessage.set(this.errorMapper.map(err).message);
          this.setBusy(build.id, false);
        },
      });
  }

  previewOnGameHub(build: BuildItem): void {
    this.setBusy(build.id, true);
    this.developerService.createPreviewToken(build.gameId, build.version)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          const url = this.router.serializeUrl(
            this.router.createUrlTree(['/preview', result.gameSlug, result.version], {
              queryParams: { token: result.token },
            })
          );
          window.open(url, '_blank');
          this.setBusy(build.id, false);
        },
        error: err => {
          this.statusMessage.set(this.errorMapper.map(err).message);
          this.setBusy(build.id, false);
        },
      });
  }

  uploadFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.gameId) {
      return;
    }
    this.uploading.set(true);
    this.developerService.uploadBuild(this.gameId, file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.uploadResult.set(result ?? null);
          this.uploading.set(false);
          this.loadBuilds();
        },
        error: err => {
          this.uploading.set(false);
          this.statusMessage.set(this.errorMapper.map(err).message);
        },
      });
  }

  isBusy(buildId: string): boolean {
    return this.busyBuildIds().has(buildId);
  }

  private setBusy(buildId: string, busy: boolean): void {
    this.busyBuildIds.update(set => {
      const next = new Set(set);
      if (busy) {
        next.add(buildId);
      } else {
        next.delete(buildId);
      }
      return next;
    });
  }
}
