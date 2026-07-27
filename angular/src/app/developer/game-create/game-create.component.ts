import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, CreateGameDraftInput } from '../../core/services/developer.service';
import { GameCatalogService, Category, Tag } from '../../core/services/game-catalog.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface CreateGamePageState {
  loading: boolean;
  saving: boolean;
  categoriesLoading: boolean;
  error: SdkError | null;
}

@Component({
  selector: 'app-game-create',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, TranslatePipe],
  templateUrl: './game-create.component.html',
  styleUrl: './game-create.component.css',
})
export class GameCreateComponent implements OnInit, OnDestroy {
  input: CreateGameDraftInput = {
    title: '',
    shortDescription: '',
    description: '',
    instructions: '',
    controls: '',
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
  saved = false;

  readonly state = signal<CreateGamePageState>({
    loading: true,
    saving: false,
    categoriesLoading: true,
    error: null,
  });

  private readonly developerService = inject(DeveloperService);
  private readonly catalog = inject(GameCatalogService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadCategories();
    this.loadTags();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.catalog.getHome()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: home => {
          this.categories = home?.categories ?? [];
          this.updateLoadingState();
        },
        error: err => {
          this.categories = [];
          this.setError(err);
          this.updateLoadingState();
        },
      });
  }

  loadTags(): void {
    this.catalog.getTags()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: tagList => {
          this.tags = tagList ?? [];
          this.updateLoadingState();
        },
        error: err => {
          this.tags = [];
          this.setError(err);
          this.updateLoadingState();
        },
      });
  }

  create(): void {
    this.state.update(s => ({ ...s, saving: true, error: null }));
    if (!this.input.title || !this.input.shortDescription || !this.input.ageRating || !this.input.orientation) {
      this.state.update(s => ({
        ...s,
        saving: false,
        error: { code: 'validation_failed', message: 'dev.requiredFields', retryable: false },
      }));
      return;
    }

    this.developerService.createDraft(this.input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.saved = true;
          this.state.update(s => ({ ...s, saving: false }));
          void this.router.navigate(['/developer/games']);
        },
        error: err => {
          this.state.update(s => ({ ...s, saving: false, error: this.errorMapper.map(err) }));
        },
      });
  }

  retry(): void {
    this.state.update(s => ({ ...s, error: null }));
    this.loadCategories();
    this.loadTags();
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

  private setError(err: unknown): void {
    this.state.update(s => ({ ...s, error: this.errorMapper.map(err) }));
  }

  private updateLoadingState(): void {
    this.state.update(s => ({ ...s, loading: false }));
  }
}
