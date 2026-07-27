import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, DeveloperProfile, CreateOrUpdateProfileInput } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface ProfilePageState {
  loading: boolean;
  saving: boolean;
  error: SdkError | null;
  saved: boolean;
}

@Component({
  selector: 'app-developer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, TranslatePipe],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class DeveloperProfileComponent implements OnInit, OnDestroy {
  profile: CreateOrUpdateProfileInput = {
    displayName: '',
    legalName: '',
    websiteUrl: '',
    supportEmail: '',
  };

  readonly state = signal<ProfilePageState>({
    loading: true,
    saving: false,
    error: null,
    saved: false,
  });

  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadProfile();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProfile(): void {
    this.state.update(s => ({ ...s, loading: true, error: null, saved: false }));
    this.developerService.getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: p => {
          this.state.update(s => ({ ...s, loading: false }));
          if (p) {
            this.profile = {
              displayName: p.displayName,
              legalName: p.legalName ?? '',
              websiteUrl: p.websiteUrl ?? '',
              supportEmail: p.supportEmail ?? '',
            };
          }
        },
        error: err => this.state.update(s => ({ ...s, loading: false, error: this.errorMapper.map(err) })),
      });
  }

  save(): void {
    this.state.update(s => ({ ...s, saving: true, error: null, saved: false }));
    if (!this.profile.displayName) {
      this.state.update(s => ({
        ...s,
        saving: false,
        error: { code: 'validation_failed', message: 'dev.requiredFields', retryable: false },
      }));
      return;
    }

    this.developerService.createOrUpdateProfile(this.profile)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.state.update(s => ({ ...s, saving: false, saved: true }));
        },
        error: err => this.state.update(s => ({ ...s, saving: false, error: this.errorMapper.map(err) })),
      });
  }

  retry(): void {
    this.loadProfile();
  }
}
