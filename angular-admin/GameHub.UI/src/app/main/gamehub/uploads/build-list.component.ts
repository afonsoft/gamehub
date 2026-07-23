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

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadBuilds();
  }

  loadBuilds(event?: LazyLoadEvent): void {
    this.skipCount = event?.first ?? 0;
    this.maxResultCount = event?.rows ?? this.maxResultCount;
    this.loading = true;
    this.adminService.getBuilds(this.skipCount, this.maxResultCount).subscribe({
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
}
