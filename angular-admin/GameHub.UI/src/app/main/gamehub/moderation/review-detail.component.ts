import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-review-detail',
  templateUrl: './review-detail.component.html',
})
export class ReviewDetailComponent implements OnInit {
  review: any = {};

  constructor(
    private readonly route: ActivatedRoute,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.adminService.getReviewDetail(id).subscribe(result => {
        this.review = result;
      });
    }
  }

  complete(decision: string): void {
    this.adminService.completeReview(this.review.reviewId, decision, '').subscribe(() => {
      this.review.status = 'Completed';
      this.review.decision = decision;
    });
  }
}
