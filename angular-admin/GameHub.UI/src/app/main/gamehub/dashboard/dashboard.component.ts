import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-dashboard',
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  summary: any = {};
  playsOverTime: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadSummary();
    this.loadPlaysOverTime();
  }

  loadSummary(): void {
    this.adminService.getDashboardSummary().subscribe(result => {
      this.summary = result;
    });
  }

  loadPlaysOverTime(): void {
    this.adminService.getPlaysOverTime(30).subscribe(result => {
      this.playsOverTime = result?.items || [];
    });
  }
}
