import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, CreateGameDraftInput } from '../../core/services/developer.service';
import { GameCatalogService, Category } from '../../core/services/game-catalog.service';

@Component({
  selector: 'app-game-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './game-create.component.html',
  styleUrl: './game-create.component.css',
})
export class GameCreateComponent implements OnInit {
  input: CreateGameDraftInput = {
    title: '',
    shortDescription: '',
    description: '',
    instructions: '',
    ageRating: 'E',
    orientation: 'Both',
    supportsDesktop: true,
    supportsMobile: true,
    supportsTablet: true,
    categoryIds: [],
    tagIds: [],
  };
  categories: Category[] = [];
  loading = false;
  error = '';

  private readonly developerService = inject(DeveloperService);
  private readonly catalog = inject(GameCatalogService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.catalog.getHome().subscribe({
      next: home => {
        this.categories = home?.categories ?? [];
      },
      error: () => {
        this.categories = [];
      },
    });
  }

  create(): void {
    this.error = '';
    if (!this.input.title || !this.input.shortDescription || !this.input.ageRating || !this.input.orientation) {
      this.error = 'Please fill in the required fields.';
      return;
    }
    this.loading = true;
    this.developerService.createDraft(this.input).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/developer/games']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Unable to create game. Please try again.';
      },
    });
  }

  toggleCategory(id: string): void {
    const list = this.input.categoryIds ?? [];
    const index = list.indexOf(id);
    if (index >= 0) {
      list.splice(index, 1);
    } else {
      list.push(id);
    }
    this.input.categoryIds = [...list];
  }

  hasCategory(id: string): boolean {
    return (this.input.categoryIds ?? []).includes(id);
  }
}
