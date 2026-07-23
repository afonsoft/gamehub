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
  recentUploads: any[] = [];
  recentGames: any[] = [];
  topGames: any[] = [];
  pendingReviews: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadSummary();
    this.loadPlaysOverTime();
    this.loadRecentUploads();
    this.loadRecentGames();
    this.loadTopGames();
    this.loadPendingReviews();
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

  loadRecentUploads(): void {
    this.adminService.getRecentUploads(5).subscribe(result => {
      this.recentUploads = result?.items || [];
    });
  }

  loadRecentGames(): void {
    this.adminService.getRecentGames(5).subscribe(result => {
      this.recentGames = result?.items || [];
    });
  }

  loadTopGames(): void {
    this.adminService.getTopGames(5).subscribe(result => {
      this.topGames = result?.items || [];
    });
  }

  loadPendingReviews(): void {
    this.adminService.getPendingReviews(5).subscribe(result => {
      this.pendingReviews = result?.items || [];
    });
  }

  buildChartPoints(items: any[]): string {
    if (!items || items.length === 0) {
      return '';
    }
    const width = 600;
    const height = 200;
    const values = items.map(i => i.plays || 0);
    const max = Math.max(...values, 1);
    const stepX = width / (items.length - 1 || 1);
    return items
      .map((item, index) => {
        const x = index * stepX;
        const y = height - (item.plays / max) * height;
        return `${x},${y}`;
      })
      .join(' ');
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
