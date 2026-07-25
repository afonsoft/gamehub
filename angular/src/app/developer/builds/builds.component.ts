import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, BuildItem, UploadResult } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-builds',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './builds.component.html',
  styleUrl: './builds.component.css',
})
export class DeveloperBuildsComponent implements OnInit {
  gameId = '';
  gameTitle = '';
  builds: BuildItem[] = [];
  uploadResult: UploadResult | null = null;
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
    this.developerService.getBuilds(this.gameId).subscribe({
      next: result => {
        this.builds = result ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  approveBuild(build: BuildItem): void {
    this.developerService.approveBuild(build.id).subscribe({
      next: () => this.loadBuilds(),
      error: err => alert(err?.error?.error?.message || 'Unable to approve build.'),
    });
  }

  rejectBuild(build: BuildItem): void {
    const reason = window.prompt('Rejection reason:');
    if (!reason) return;
    this.developerService.rejectBuild(build.id, reason).subscribe({
      next: () => this.loadBuilds(),
      error: err => alert(err?.error?.error?.message || 'Unable to reject build.'),
    });
  }

  openInspector(build: BuildItem): void {
    this.developerService.startInspectorSession(build.gameId, build.id, 'desktop', '1024x768').subscribe({
      next: session => {
        const url = this.router.serializeUrl(
          this.router.createUrlTree(['/games', build.gameSlug], {
            queryParams: { inspector: '1', inspectorSession: session.id },
          })
        );
        window.open(url, '_blank');
      },
      error: err => alert(err?.error?.error?.message || 'Unable to start inspector session.'),
    });
  }

  previewOnGameHub(build: BuildItem): void {
    this.developerService.createPreviewToken(build.gameId, build.version).subscribe({
      next: result => {
        const url = this.router.serializeUrl(
          this.router.createUrlTree(['/preview', result.gameSlug, result.version], {
            queryParams: { token: result.token },
          })
        );
        window.open(url, '_blank');
      },
      error: err => alert(err?.error?.error?.message || 'Unable to create preview token.'),
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
      },
    });
  }
}
