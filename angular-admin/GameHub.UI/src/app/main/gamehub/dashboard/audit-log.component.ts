import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-audit-log',
  templateUrl: './audit-log.component.html',
})
export class AuditLogComponent implements OnInit {
  logs: any[] = [];
  totalRecords = 0;

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(event?: any): void {
    const skipCount = event?.first || 0;
    const maxResultCount = event?.rows || 25;
    this.adminService.getAuditLogs(skipCount, maxResultCount).subscribe(result => {
      this.logs = result?.items || [];
      this.totalRecords = result?.totalCount || 0;
    });
  }
}
