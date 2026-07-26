import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import {
  DeveloperReviewHistoryItem,
  DeveloperService,
  BuildItem,
  UploadResult,
} from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-builds',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './builds.component.html',
  styleUrl: './builds.component.css',
})
export class DeveloperBuildsComponent implements OnInit {
  gameId = '';
  gameTitle = '';
  builds: BuildItem[] = [];
  uploadResult: UploadResult | null = null;
  reviewHistory: DeveloperReviewHistoryItem[] = [];
  errorMessage = '';
  statusMessage = '';
  rejectingBuildId: string | null = null;
  rejectionReason = '';
  readonly busyBuildIds = new Set<string>();
  loading = false;
  uploading = false;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id') ?? '';
    this.gameTitle = history.state?.['title'] ?? 'Game';
    this.loadBuilds();
  }

  loadBuilds(): void {
    if (!this.gameId) return;
    this.loading = true;
    this.errorMessage = '';
    this.developerService.getBuilds(this.gameId).subscribe({
      next: result => this.loadReviewHistory(result ?? []),
      error: () => {
        this.loading = false;
        this.errorMessage = 'Unable to load build history.';
      },
    });
  }

  private loadReviewHistory(builds: BuildItem[]): void {
    this.builds = builds;
    this.developerService.getReviewHistory(this.gameId).subscribe({
      next: history => {
        this.reviewHistory = history ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Builds loaded, but review history is unavailable.';
      },
    });
  }

  approveBuild(build: BuildItem): void {
    if (!window.confirm(`Approve build ${build.version}?`)) return;
    this.setBusy(build.id, true);
    this.developerService.approveBuild(build.id).subscribe({
      next: () => {
        this.statusMessage = `Build ${build.version} approved.`;
        this.setBusy(build.id, false);
        this.loadBuilds();
      },
      error: err => {
        this.statusMessage = err?.error?.error?.message || 'Unable to approve build.';
        this.setBusy(build.id, false);
      },
    });
  }

  beginRejectBuild(build: BuildItem): void {
    this.rejectingBuildId = build.id;
    this.rejectionReason = '';
  }

  cancelRejectBuild(): void {
    this.rejectingBuildId = null;
    this.rejectionReason = '';
  }

  rejectBuild(build: BuildItem): void {
    const reason = this.rejectionReason.trim();
    if (!reason) {
      this.statusMessage = 'Provide a reason before rejecting the build.';
      return;
    }
    this.setBusy(build.id, true);
    this.developerService.rejectBuild(build.id, reason).subscribe({
      next: () => {
        this.statusMessage = `Build ${build.version} rejected.`;
        this.cancelRejectBuild();
        this.setBusy(build.id, false);
        this.loadBuilds();
      },
      error: err => {
        this.statusMessage = err?.error?.error?.message || 'Unable to reject build.';
        this.setBusy(build.id, false);
      },
    });
  }

  openInspector(build: BuildItem): void {
    this.setBusy(build.id, true);
    this.developerService.startInspectorSession(build.gameId, build.id, 'desktop', '1024x768').subscribe({
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
        this.statusMessage = err?.error?.error?.message || 'Unable to start inspector session.';
        this.setBusy(build.id, false);
      },
    });
  }

  previewOnGameHub(build: BuildItem): void {
    this.setBusy(build.id, true);
    this.developerService.createPreviewToken(build.gameId, build.version).subscribe({
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
        this.statusMessage = err?.error?.error?.message || 'Unable to create preview token.';
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
    this.uploading = true;
    this.developerService.uploadBuild(this.gameId, file).subscribe({
      next: result => {
        this.uploadResult = result ?? null;
        this.uploading = false;
        this.loadBuilds();
      },
      error: () => {
        this.uploading = false;
        this.statusMessage = 'Unable to upload the build.';
      },
    });
  }

  isBusy(buildId: string): boolean {
    return this.busyBuildIds.has(buildId);
  }

  private setBusy(buildId: string, busy: boolean): void {
    if (busy) {
      this.busyBuildIds.add(buildId);
      return;
    }
    this.busyBuildIds.delete(buildId);
  }
}
