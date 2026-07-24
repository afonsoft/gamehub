import { Component, OnInit } from '@angular/core';
import { GameHubAdminService, ValidationReport } from '../shared/services/gamehub-admin.service';

@Component({
  selector: 'app-inspector',
  standalone: false,
  templateUrl: './inspector.component.html',
})
export class InspectorComponent implements OnInit {
  reports: ValidationReport[] = [];
  loading = false;

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.adminService.getValidationReports().subscribe({
      next: (result: ValidationReport[]) => {
        this.reports = result ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
