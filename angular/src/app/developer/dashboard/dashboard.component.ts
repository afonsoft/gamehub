import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { DeveloperService, DeveloperDashboard, DeveloperGameVersion, DeveloperDashboardAction, DashboardDailyPlays } from '../../core/services/developer.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { BadgeComponent } from '../../shared/ui/badge/badge.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-developer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe, BadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DeveloperDashboardComponent implements OnInit {
  dashboard$!: Observable<DeveloperDashboard>;
  readonly adminUrl = environment.adminUrl;

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.dashboard$ = this.developerService.getDashboard();
  }

  trackByVersion(index: number, item: DeveloperGameVersion): string {
    return item.id;
  }

  trackByAction(index: number, item: DeveloperDashboardAction): string {
    return item.gameId;
  }

  getMaxPlays(items: DashboardDailyPlays[]): number {
    return Math.max(1, ...items.map(i => i.plays));
  }

  buildChartPoints(items: DashboardDailyPlays[]): string {
    const max = this.getMaxPlays(items);
    const width = 600;
    const height = 200;
    const count = items.length || 1;
    const stepX = count > 1 ? width / (count - 1) : width;
    return items
      .map((item, index) => {
        const x = index * stepX;
        const y = max > 0 ? height - (item.plays / max) * height : height;
        return `${x},${y}`;
      })
      .join(' ');
  }
}
