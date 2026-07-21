import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GameHubAdminService } from '../shared/services/gamehub-admin.service';

@Component({
  standalone: false,
  selector: 'gamehub-game-detail',
  templateUrl: './game-detail.component.html',
})
export class GameDetailComponent implements OnInit {
  game: any = {};

  constructor(
    private readonly route: ActivatedRoute,
    private readonly adminService: GameHubAdminService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.adminService.getGameDetail(id).subscribe(result => {
        this.game = result;
      });
    }
  }
}
