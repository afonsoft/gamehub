import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GamesComponent } from '../games/games.component';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, GamesComponent],
  template: `<app-games></app-games>`,
})
export class SearchPageComponent {}
