import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-review-queue',
  templateUrl: './review-queue.component.html',
})
export class ReviewQueueComponent implements OnInit {
  reviews: any[] = [];
  filter = 'Pending';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.adminService.getPendingReviews().subscribe(result => {
      this.reviews = (result?.items || []).filter((r: any) =>
        this.filter === 'All' ? true : (r.status || '').toLowerCase() === this.filter.toLowerCase(),
      );
    });
  }

  setFilter(status: string): void {
    this.filter = status;
    this.loadReviews();
  }
}
