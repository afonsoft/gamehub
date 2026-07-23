import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-review-detail',
  templateUrl: './review-detail.component.html',
})
export class ReviewDetailComponent implements OnInit {
  review: any = {};
  modalOpen = false;
  modalDecision = '';
  notes = '';
  completing = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.review = data['review'] ?? {};
    });
  }

  openModal(decision: string): void {
    this.modalDecision = decision;
    this.modalOpen = true;
    this.notes = '';
  }

  closeModal(): void {
    this.modalOpen = false;
  }

  complete(): void {
    if (this.modalDecision === 'Rejected' && !this.notes) {
      return;
    }
    this.completing = true;
    this.adminService.completeReview(this.review.reviewId, this.modalDecision, this.notes).subscribe({
      next: () => {
        this.completing = false;
        this.router.navigate(['/app/main/gamehub/moderation']);
      },
      error: () => {
        this.completing = false;
      },
    });
  }
}
