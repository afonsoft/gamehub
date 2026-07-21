import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameCard, GameCatalogService } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-games',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Games</h1>
    <div class="grid">
      <div *ngFor="let game of games">
        <strong>{{ game.title }}</strong>
        <p>{{ game.shortDescription }}</p>
      </div>
    </div>
  `,
  styles: ['.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 1rem; }']
})
export class GamesComponent implements OnInit {
  games: GameCard[] = [];

  constructor(private catalog: GameCatalogService) {}

  ngOnInit(): void {
    this.catalog.getGames().subscribe(r => this.games = r.items);
  }
}
