import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameCatalogService, HomeResponse } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>GameHub</h1>
    <section *ngIf="home">
      <h2>Highlights</h2>
      <div class="grid">
        <div *ngFor="let game of home.highlights">
          <strong>{{ game.title }}</strong>
          <p>{{ game.shortDescription }}</p>
        </div>
      </div>
    </section>
  `,
  styles: ['.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 1rem; }']
})
export class HomeComponent implements OnInit {
  home: HomeResponse | null = null;

  constructor(private catalog: GameCatalogService) {}

  ngOnInit(): void {
    this.catalog.getHome().subscribe(h => this.home = h);
  }
}
