import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, DeveloperEarnings, GameEarnings, DailyEarnings } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-earnings',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './earnings.component.html',
  styleUrl: './earnings.component.css',
})
export class DeveloperEarningsComponent implements OnInit {
  earnings: DeveloperEarnings | null = null;
  loading = false;

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.loadEarnings();
  }

  loadEarnings(): void {
    this.loading = true;
    this.developerService.getEarnings().subscribe({
      next: result => {
        this.earnings = result;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  formatPercent(value: number): string {
    return `${(value * 100).toFixed(0)}%`;
  }

  trackByGame(index: number, item: GameEarnings): string {
    return item.gameId;
  }

  trackByDay(index: number, item: DailyEarnings): string {
    return item.date;
  }
}
