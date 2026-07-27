import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, DeveloperTeam } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface TeamPageState {
  loading: boolean;
  saving: boolean;
  error: SdkError | null;
  saved: boolean;
}

@Component({
  selector: 'app-developer-team',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, TranslatePipe],
  templateUrl: './team.component.html',
  styleUrl: './team.component.css',
})
export class DeveloperTeamComponent implements OnInit, OnDestroy {
  team: DeveloperTeam = {
    name: '',
    primaryContactEmail: '',
    country: '',
  };

  readonly state = signal<TeamPageState>({
    loading: true,
    saving: false,
    error: null,
    saved: false,
  });

  private readonly developerService = inject(DeveloperService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadTeam();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTeam(): void {
    this.state.update(s => ({ ...s, loading: true, error: null, saved: false }));
    this.developerService.getTeamGeneralSettings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: team => {
          this.state.update(s => ({ ...s, loading: false }));
          if (team) {
            this.team = team;
          }
        },
        error: err => this.state.update(s => ({ ...s, loading: false, error: this.errorMapper.map(err) })),
      });
  }

  save(): void {
    this.state.update(s => ({ ...s, saving: true, error: null, saved: false }));

    if (!this.team.name || !this.team.primaryContactEmail) {
      this.state.update(s => ({
        ...s,
        saving: false,
        error: { code: 'validation_failed', message: 'dev.requiredFields', retryable: false },
      }));
      return;
    }

    this.developerService.updateTeamGeneralSettings(this.team)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: team => {
          this.state.update(s => ({ ...s, saving: false, saved: true }));
          if (team) {
            this.team = team;
          }
        },
        error: err => this.state.update(s => ({ ...s, saving: false, error: this.errorMapper.map(err) })),
      });
  }

  retry(): void {
    this.loadTeam();
  }
}
