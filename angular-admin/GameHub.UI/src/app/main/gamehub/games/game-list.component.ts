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
  status = '';

  constructor(private readonly adminService: GameHubAdminService) {}

  ngOnInit(): void {
    this.loadGames();
  }

  loadGames(event?: any): void {
    const skipCount = event?.first || 0;
    const maxResultCount = event?.rows || 25;
    this.adminService.getGames(skipCount, maxResultCount, this.status).subscribe(result => {
      this.games = result?.items || [];
      this.totalRecords = result?.totalCount || 0;
    });
  }

  suspend(game: any): void {
    this.adminService.suspendGame(game.id, 'Administrative suspension').subscribe(() => {
      game.status = 'Suspended';
    });
  }
}
