import { Component, OnInit } from '@angular/core';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-review-queue',
  templateUrl: './review-queue.component.html',
})
export class ReviewQueueComponent implements OnInit {
  reviews: any[] = [];

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.adminService.getPendingReviews().subscribe(result => {
      this.reviews = result?.items || [];
    });
  }
}
