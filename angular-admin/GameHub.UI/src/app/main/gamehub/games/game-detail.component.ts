import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-game-detail',
  templateUrl: './game-detail.component.html',
})
export class GameDetailComponent implements OnInit {
  game: any = {};
  suspending = false;
  thumbnailAction = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadGame(id);
    }
  }

  loadGame(id: string): void {
    this.adminService.getGameDetail(id).subscribe(result => {
      this.game = result ?? {};
    });
  }

  suspend(): void {
    const reason = window.prompt('Suspend reason:');
    if (!reason || !this.game.id) return;
    this.suspending = true;
    this.adminService.suspendGame(this.game.id, reason).subscribe({
      next: () => {
        this.suspending = false;
        this.loadGame(this.game.id);
      },
      error: () => {
        this.suspending = false;
      },
    });
  }

  canSuspend(): boolean {
    return this.game && (this.game.status === 'Published' || this.game.status === 'InReview');
  }

  approveThumbnail(): void {
    if (!this.game.id) return;
    this.thumbnailAction = true;
    this.adminService.approveThumbnail(this.game.id).subscribe({
      next: () => {
        this.thumbnailAction = false;
        this.loadGame(this.game.id);
      },
      error: () => {
        this.thumbnailAction = false;
      },
    });
  }

  rejectThumbnail(): void {
    if (!this.game.id) return;
    this.thumbnailAction = true;
    this.adminService.rejectThumbnail(this.game.id).subscribe({
      next: () => {
        this.thumbnailAction = false;
        this.loadGame(this.game.id);
      },
      error: () => {
        this.thumbnailAction = false;
      },
    });
  }
}
