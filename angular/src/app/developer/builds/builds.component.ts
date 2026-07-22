import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
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
