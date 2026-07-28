import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-game-list',
  templateUrl: './game-list.component.html',
  animations: [appModuleAnimation()],
})
export class GameListComponent implements OnInit {
  games: any[] = [];
  totalRecords = 0;
  loading = false;
  status = '';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadGames();
  }

  loadGames(event?: any): void {
    const skipCount = event?.first || 0;
    const maxResultCount = event?.rows || 25;
    this.loading = true;
    this.adminService.getGames(skipCount, maxResultCount, this.status).subscribe({
      next: result => {
        this.games = result?.items || [];
        this.totalRecords = result?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  onStatusChange(): void {
    this.loadGames({ first: 0, rows: 25 });
  }

  startReview(game: any): void {
    this.adminService.startReview(game.id).subscribe(() => {
      game.status = 'InReview';
    });
  }

  approveForPublishing(game: any): void {
    this.adminService.approveForPublishing(game.id).subscribe(() => {
      game.status = 'ApprovedForPublishing';
    });
  }

  publish(game: any): void {
    const buildId = window.prompt('Enter the build ID to publish:');
    if (!buildId) {
      return;
    }
    this.adminService.publishGame(game.id, buildId).subscribe(() => {
      game.status = 'Published';
    });
  }

  requestChanges(game: any): void {
    const reason = window.prompt('Reason for requesting changes:');
    if (!reason) {
      return;
    }
    this.adminService.requestChanges(game.id, reason).subscribe(() => {
      game.status = 'Rejected';
    });
  }

  suspend(game: any): void {
    this.adminService.suspendGame(game.id, 'Administrative suspension').subscribe(() => {
      game.status = 'Suspended';
    });
  }
}
