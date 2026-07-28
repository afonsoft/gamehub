import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-review-queue',
  templateUrl: './review-queue.component.html',
  animations: [appModuleAnimation()],
})
export class ReviewQueueComponent implements OnInit {
  reviews: any[] = [];
  allReviews: any[] = [];
  filter = 'Pending';
  loading = false;

  readonly filters = ['All', 'Pending', 'InProgress', 'Completed'];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.loading = true;
    this.adminService.getPendingReviews().subscribe({
      next: result => {
        this.allReviews = result?.items || [];
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  applyFilter(): void {
    if (this.filter === 'All') {
      this.reviews = this.allReviews;
      return;
    }
    this.reviews = this.allReviews.filter((r: any) => (r.status || '') === this.filter);
  }

  setFilter(status: string): void {
    this.filter = status;
    this.applyFilter();
  }
}
