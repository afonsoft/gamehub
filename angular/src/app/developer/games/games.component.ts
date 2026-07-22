import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, GameSummary } from '../../core/services/developer.service';

@Component({
  selector: 'app-developer-games',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './games.component.html',
  styleUrl: './games.component.css',
})
export class DeveloperGamesComponent implements OnInit {
  games: GameSummary[] = [];
  loading = false;

  private readonly developerService = inject(DeveloperService);

  ngOnInit(): void {
    this.loadGames();
  }

  loadGames(): void {
    this.loading = true;
    this.developerService.getMyGames(0, 100).subscribe({
      next: result => {
        this.games = result?.items ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
