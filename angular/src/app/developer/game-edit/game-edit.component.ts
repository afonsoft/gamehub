import { Component, OnInit, OnDestroy, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, UpdateGameMetadataInput, UploadImageResult } from '../../core/services/developer.service';
import { GameCatalogService, GameDetail, Category, Tag } from '../../core/services/game-catalog.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface GameEditPageState {
  loading: boolean;
  saving: boolean;
  submitting: boolean;
  error: SdkError | null;
}

@Component({
  selector: 'app-game-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonComponent, ConfirmDialogComponent, TranslatePipe],
  templateUrl: './game-edit.component.html',
  styleUrl: './game-edit.component.css',
})
export class GameEditComponent implements OnInit, OnDestroy {
  @ViewChild('submitConfirm') submitConfirm!: ConfirmDialogComponent;

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
  latestBuildStatus = '';
  thumbnailUrl = '';
  animatedThumbnailUrl = '';
  thumbnailStatus = '';
  heroImageUrl = '';

  readonly state = signal<GameEditPageState>({
    loading: true,
    saving: false,
    submitting: false,
    error: null,
  });

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly developerService = inject(DeveloperService);
  private readonly catalog = inject(GameCatalogService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id') ?? '';
    this.input.gameId = this.gameId;
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.state.update(s => ({ ...s, loading: true, error: null }));
    this.catalog.getHome()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: home => {
          this.categories = home?.categories ?? [];
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, error: this.errorMapper.map(err) })),
      });

    this.catalog.getTags()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: tagList => {
          this.tags = tagList ?? [];
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, error: this.errorMapper.map(err) })),
      });

    this.developerService.getMyGames(0, 100)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          const game = (result?.items ?? []).find(g => g.id === this.gameId);
          if (game?.slug) {
            this.latestBuildStatus = game.latestBuildStatus ?? '';
            this.loadDetail(game.slug);
          } else {
            this.state.update(s => ({ ...s, loading: false }));
          }
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, loading: false, error: this.errorMapper.map(err) })),
      });
  }

  private loadDetail(slug: string): void {
    this.catalog.getBySlug(slug)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.state.update(s => ({ ...s, loading: false }));
          if (detail) {
            this.mapDetail(detail);
          }
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, loading: false, error: this.errorMapper.map(err) })),
      });
  }

  async save(): Promise<void> {
    this.state.update(s => ({ ...s, saving: true, error: null }));
    if (!this.input.title || !this.input.shortDescription) {
      this.state.update(s => ({
        ...s,
        saving: false,
        error: { code: 'validation_failed', message: 'dev.requiredFields', retryable: false },
      }));
      return;
    }
    this.developerService.updateMetadata(this.input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.state.update(s => ({ ...s, saving: false }));
          void this.router.navigate(['/developer/games']);
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, saving: false, error: this.errorMapper.map(err) })),
      });
  }

  async submitForReview(): Promise<void> {
    const confirmed = await this.submitConfirm.open('dev.submitForReviewConfirm', 'dev.submitForReviewMessage');
    if (!confirmed) {
      return;
    }
    this.state.update(s => ({ ...s, submitting: true, error: null }));
    this.developerService.submitForReview(this.gameId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.state.update(s => ({ ...s, submitting: false }));
          this.status = 'InReview';
          void this.router.navigate(['/developer/games']);
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, submitting: false, error: this.errorMapper.map(err) })),
      });
  }

  canSubmitForReview(): boolean {
    return (this.status === 'Draft' || this.status === 'Rejected') && this.latestBuildStatus === 'Approved';
  }

  retry(): void {
    this.state.update(s => ({ ...s, error: null }));
    this.loadData();
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

  onThumbnailSelected(event: Event): void {
    this.uploadImage(event, (file: File) => this.developerService.uploadThumbnail(this.gameId, file), url => (this.thumbnailUrl = url));
  }

  onAnimatedThumbnailSelected(event: Event): void {
    this.uploadImage(
      event,
      (file: File) => this.developerService.uploadAnimatedThumbnail(this.gameId, file),
      () => (this.thumbnailStatus = 'Pending')
    );
  }

  onHeroSelected(event: Event): void {
    this.uploadImage(event, (file: File) => this.developerService.uploadHero(this.gameId, file), url => (this.heroImageUrl = url));
  }

  private uploadImage(event: Event, upload: (file: File) => any, onSuccess: (url: string) => void): void {
    const file = this.extractFile(event);
    if (!file) return;
    upload(file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: UploadImageResult) => {
          onSuccess(result.url);
        },
        error: (err: unknown) => this.state.update(s => ({ ...s, error: this.errorMapper.map(err) })),
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

  private toggle(list: string[], id: string): string[] {
    const index = list.indexOf(id);
    if (index >= 0) {
      list.splice(index, 1);
    } else {
      list.push(id);
    }
    return [...list];
  }
}
