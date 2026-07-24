import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DeveloperService, UpdateGameMetadataInput, UploadImageResult } from '../../core/services/developer.service';
import { GameCatalogService, GameDetail, Category, Tag } from '../../core/services/game-catalog.service';

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
    suggestedDescription: '',
    seoDescription: '',
    ageRating: 'E',
    orientation: 'Both',
    aspectRatio: 'Aspect16x9',
    supportsDesktop: true,
    supportsMobile: true,
    supportsTablet: true,
    categoryIds: [],
    tagIds: [],
  };
  categories: Category[] = [];
  tags: Tag[] = [];
  status = '';
  thumbnailUrl = '';
  animatedThumbnailUrl = '';
  thumbnailStatus = '';
  heroImageUrl = '';
  loading = false;
  saving = false;
  submitting = false;
  uploadingThumbnail = false;
  uploadingAnimatedThumbnail = false;
  uploadingHero = false;
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
    this.catalog.getTags().subscribe({
      next: tagList => {
        this.tags = tagList ?? [];
      },
      error: () => {
        this.tags = [];
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

  submitForReview(): void {
    this.error = '';
    this.submitting = true;
    this.developerService.submitForReview(this.gameId).subscribe({
      next: () => {
        this.submitting = false;
        this.status = 'InReview';
        void this.router.navigate(['/developer/games']);
      },
      error: () => {
        this.submitting = false;
        this.error = 'Unable to submit for review. Please try again.';
      },
    });
  }

  canSubmitForReview(): boolean {
    return this.status === 'Draft' || this.status === 'Rejected';
  }

  toggleCategory(id: string): void {
    const list = this.input.categoryIds ?? [];
    this.input.categoryIds = this.toggle(list, id);
  }

  hasCategory(id: string): boolean {
    return (this.input.categoryIds ?? []).includes(id);
  }

  toggleTag(id: string): void {
    const list = this.input.tagIds ?? [];
    this.input.tagIds = this.toggle(list, id);
  }

  hasTag(id: string): boolean {
    return (this.input.tagIds ?? []).includes(id);
  }

  private toggle(list: string[], id: string): string[] {
    const index = list.indexOf(id);
    if (index >= 0) {
      list.splice(index, 1);
    } else {
      list.push(id);
    }
    return [...list];
  }

  onThumbnailSelected(event: Event): void {
    const file = this.extractFile(event);
    if (!file) return;
    this.uploadingThumbnail = true;
    this.developerService.uploadThumbnail(this.gameId, file).subscribe({
      next: (result: UploadImageResult) => {
        this.thumbnailUrl = result.url;
        this.uploadingThumbnail = false;
      },
      error: () => {
        this.uploadingThumbnail = false;
        this.error = 'Unable to upload thumbnail.';
      },
    });
  }

  onAnimatedThumbnailSelected(event: Event): void {
    const file = this.extractFile(event);
    if (!file) return;
    this.uploadingAnimatedThumbnail = true;
    this.developerService.uploadAnimatedThumbnail(this.gameId, file).subscribe({
      next: (result: UploadImageResult) => {
        this.animatedThumbnailUrl = result.url;
        this.thumbnailStatus = 'Pending';
        this.uploadingAnimatedThumbnail = false;
      },
      error: () => {
        this.uploadingAnimatedThumbnail = false;
        this.error = 'Unable to upload animated thumbnail.';
      },
    });
  }

  onHeroSelected(event: Event): void {
    const file = this.extractFile(event);
    if (!file) return;
    this.uploadingHero = true;
    this.developerService.uploadHero(this.gameId, file).subscribe({
      next: (result: UploadImageResult) => {
        this.heroImageUrl = result.url;
        this.uploadingHero = false;
      },
      error: () => {
        this.uploadingHero = false;
        this.error = 'Unable to upload hero image.';
      },
    });
  }

  isImageUrl(url: string): boolean {
    return /\.(gif|webp|png|jpg|jpeg)$/i.test(url);
  }

  private extractFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    return file ?? null;
  }

  private mapDetail(detail: GameDetail): void {
    this.status = detail.status ?? '';
    this.thumbnailUrl = detail.thumbnailUrl ?? '';
    this.animatedThumbnailUrl = detail.animatedThumbnailUrl ?? '';
    this.thumbnailStatus = detail.thumbnailStatus ?? 'Approved';
    this.heroImageUrl = detail.heroImageUrl ?? '';
    this.input = {
      gameId: detail.id,
      title: detail.title,
      shortDescription: detail.shortDescription,
      description: detail.description,
      instructions: detail.instructions,
      controls: detail.controls ?? '',
      suggestedDescription: detail.suggestedDescription ?? '',
      seoDescription: detail.seoDescription ?? '',
      ageRating: detail.ageRating || 'E',
      orientation: detail.orientation,
      aspectRatio: detail.aspectRatio || 'Aspect16x9',
      supportsDesktop: detail.supportsDesktop,
      supportsMobile: detail.supportsMobile,
      supportsTablet: detail.supportsTablet,
      categoryIds: (detail.categories ?? []).map(c => c.id),
      tagIds: (detail.tags ?? []).map(t => t.id),
    };
  }
}
