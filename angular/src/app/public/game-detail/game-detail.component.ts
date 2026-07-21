import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameCatalogService, GameDetail, GameCard } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-game-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './game-detail.component.html',
  styleUrl: './game-detail.component.css',
})
export class GameDetailComponent implements OnInit {
  game: GameDetail | null = null;
  loaded = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private catalog: GameCatalogService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe({
      next: g => {
        this.game = g;
        this.loaded = true;
        if (!g) {
          this.router.navigate(['/games']);
        }
      },
      error: () => {
        this.loaded = true;
        this.router.navigate(['/games']);
      },
    });
  }

  trackGame(_index: number, game: GameCard): string {
    return game.id;
  }
}
