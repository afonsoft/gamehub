import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { ActivatedRoute } from '@angular/router';
import { GameHubAdminService, BuildFile } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'app-build-files',
  templateUrl: './build-files.component.html',
  animations: [appModuleAnimation()],
})
export class BuildFilesComponent implements OnInit {
  buildId = '';
  files: BuildFile[] = [];
  loading = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    this.buildId = this.route.snapshot.paramMap.get('id') ?? '';
    if (this.buildId) {
      this.loadFiles();
    }
  }

  loadFiles(): void {
    this.loading = true;
    this.adminService.getBuildFiles(this.buildId).subscribe({
      next: result => {
        this.files = result.items ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
