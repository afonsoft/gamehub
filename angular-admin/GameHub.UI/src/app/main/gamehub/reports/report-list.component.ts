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
  allReports: any[] = [];
  filter = 'Open';
  loading = false;
  updating: { [reportId: string]: boolean } = {};

  readonly statuses = ['All', 'Open', 'UnderReview', 'Resolved', 'Dismissed'];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.adminService.getReports().subscribe({
      next: result => {
        this.allReports = result?.items || [];
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  applyFilter(): void {
    this.reports = this.filter === 'All'
      ? this.allReports
      : this.allReports.filter((r: any) => (r.status || '') === this.filter);
  }

  setFilter(status: string): void {
    this.filter = status;
    this.loadReports();
  }

  updateStatus(report: any, status: string): void {
    if (!report || this.updating[report.reportId]) {
      return;
    }

    this.updating[report.reportId] = true;
    this.adminService.updateReportStatus(report.reportId, status).subscribe({
      next: () => {
        this.updating[report.reportId] = false;
        report.status = status;
        this.loadReports();
      },
      error: () => {
        this.updating[report.reportId] = false;
      },
    });
  }
}
