import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-report-list',
  templateUrl: './report-list.component.html',
  animations: [appModuleAnimation()],
})
export class ReportListComponent implements OnInit {
  reports: any[] = [];
  filter = 'Open';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.adminService.getReports().subscribe(result => {
      this.reports = (result?.items || []).filter((r: any) =>
        this.filter === 'All' ? true : (r.status || '').toLowerCase() === this.filter.toLowerCase(),
      );
    });
  }

  setFilter(status: string): void {
    this.filter = status;
    this.loadReports();
  }
}
