import { Component, OnInit } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { GameHubAdminService, BuildListItem, PagedBuildList } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'app-build-list',
  templateUrl: './build-list.component.html',
})
export class BuildListComponent implements OnInit {
  builds: BuildListItem[] = [];
  totalCount = 0;
  loading = false;
  skipCount = 0;
  maxResultCount = 25;
  status = '';
  searchText = '';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadBuilds();
  }

  loadBuilds(event?: LazyLoadEvent): void {
    this.skipCount = event?.first ?? 0;
    this.maxResultCount = event?.rows ?? this.maxResultCount;
    this.loading = true;
    this.adminService.getBuilds(this.skipCount, this.maxResultCount, this.status, undefined, this.searchText).subscribe({
      next: (result: PagedBuildList) => {
        this.builds = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  onSearch(): void {
    this.skipCount = 0;
    this.loadBuilds();
  }

  onStatusChange(): void {
    this.skipCount = 0;
    this.loadBuilds();
  }

  formatBytes(bytes: number): string {
    if (!bytes) return '0 B';
    const units = ['B', 'KB', 'MB', 'GB'];
    let value = bytes;
    let unitIndex = 0;
    while (value >= 1024 && unitIndex < units.length - 1) {
      value /= 1024;
      unitIndex++;
    }
    return `${value.toFixed(2)} ${units[unitIndex]}`;
  }
}
