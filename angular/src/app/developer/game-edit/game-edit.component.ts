import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, UpdateGameMetadataInput } from '../../core/services/developer.service';
import { GameCatalogService, GameDetail, Category } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-game-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './game-edit.component.html',
  styleUrl: './game-edit.component.css',
})
export class GameEditComponent implements OnInit {
  gameId = '';
  input: UpdateGameMetadataInput = {
    gameId: '',
    title: '',
    shortDescription: '',
    description: '',
    instructions: '',
    ageRating: 'Everyone',
    orientation: 'Both',
    supportsDesktop: true,
    supportsMobile: true,
    supportsTablet: true,
  };
  categories: Category[] = [];
  loading = false;
  saving = false;
  error = '';

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly developerService = inject(DeveloperService);
  private readonly catalog = inject(GameCatalogService);

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id') ?? '';
    this.input.gameId = this.gameId;
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.catalog.getHome().subscribe({
      next: home => {
        this.categories = home?.categories ?? [];
      },
      error: () => {
        this.categories = [];
      },
    });
    this.developerService.getMyGames(0, 100).subscribe({
      next: result => {
        const game = (result?.items ?? []).find(g => g.id === this.gameId);
        if (game?.slug) {
          this.catalog.getBySlug(game.slug).subscribe({
            next: detail => {
              this.loading = false;
              if (detail) {
                this.mapDetail(detail);
              }
            },
            error: () => {
              this.loading = false;
            },
          });
        } else {
          this.loading = false;
        }
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  save(): void {
    this.error = '';
    if (!this.input.title || !this.input.shortDescription) {
      this.error = 'Please fill in the required fields.';
      return;
    }
    this.saving = true;
    this.developerService.updateMetadata(this.input).subscribe({
      next: () => {
        this.saving = false;
        void this.router.navigate(['/developer/games']);
      },
      error: () => {
        this.saving = false;
        this.error = 'Unable to save game. Please try again.';
      },
    });
  }

  private mapDetail(detail: GameDetail): void {
    this.input = {
      gameId: detail.id,
      title: detail.title,
      shortDescription: detail.shortDescription,
      description: detail.description,
      instructions: detail.instructions,
      ageRating: detail.ageRating,
      orientation: detail.orientation,
      supportsDesktop: detail.supportsDesktop,
      supportsMobile: detail.supportsMobile,
      supportsTablet: detail.supportsTablet,
    };
  }
}
