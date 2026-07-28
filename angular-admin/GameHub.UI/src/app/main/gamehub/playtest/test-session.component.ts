import { Component } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppConsts } from '@shared/AppConsts';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

function derivePublicAppBase(remoteServiceBaseUrl: string): string {
  try {
    const url = new URL(remoteServiceBaseUrl);
    url.hostname = url.hostname.replace(/^([\w-]+)-api\./, '$1.');
    url.pathname = '';
    url.search = '';
    url.hash = '';
    return url.toString().replace(/\/$/, '');
  } catch {
    return window.location.origin;
  }
}

@Component({
  standalone: false,
  selector: 'app-gamehub-test-session',
  templateUrl: './test-session.component.html',
  animations: [appModuleAnimation()],
})
export class TestSessionComponent {
  gameId = '';
  version = '';
  notes = '';
  loading = false;
  error = '';
  previewUrl: SafeResourceUrl | null = null;
  private readonly publicBase = derivePublicAppBase(AppConsts.remoteServiceBaseUrl || '');

  constructor(
    private readonly adminService: GameHubAdminService,
    private readonly sanitizer: DomSanitizer,
  ) {}

  startTest(): void {
    if (!this.gameId || !this.version) {
      this.error = 'Game ID and version are required.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.previewUrl = null;

    this.adminService.createPreviewToken(this.gameId, this.version).subscribe({
      next: result => {
        this.loading = false;
        if (!result?.previewUrl) {
          this.error = 'Preview URL not available for this build.';
          return;
        }

        const url = result.previewUrl.startsWith('http')
          ? result.previewUrl
          : `${this.publicBase}${result.previewUrl}`;
        this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      },
      error: err => {
        this.loading = false;
        this.error = err?.message || 'Failed to create preview token.';
      },
    });
  }

  requestPlaytest(): void {
    if (!this.gameId) {
      this.error = 'Game ID is required.';
      return;
    }

    this.loading = true;
    this.adminService.requestPlaytest(this.gameId, this.notes).subscribe({
      next: () => {
        this.loading = false;
        this.startTest();
      },
      error: err => {
        this.loading = false;
        this.error = err?.message || 'Failed to request playtest.';
      },
    });
  }
}
