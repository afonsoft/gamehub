import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { GameCatalogService } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-game-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div *ngIf="game">
      <h1>{{ game.title }}</h1>
      <p>{{ game.description }}</p>
      <a [routerLink]="['/play', game.slug]">Play</a>
    </div>
  `
})
export class GameDetailComponent implements OnInit {
  game: any;

  constructor(private route: ActivatedRoute, private catalog: GameCatalogService) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe(g => this.game = g);
  }
}
