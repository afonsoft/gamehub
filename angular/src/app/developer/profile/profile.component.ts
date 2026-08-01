import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeveloperService, CreateOrUpdateProfileInput } from '../../core/services/developer.service';
import { ErrorMapperService, SdkError } from '../../core/services/error-mapper.service';
import { TenantService, AvailableTenant, TenantJoinRequest } from '../../core/services/tenant.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface ProfilePageState {
  loading: boolean;
  saving: boolean;
  error: SdkError | null;
  saved: boolean;
  tenantLoading: boolean;
  tenantError: SdkError | null;
  tenantRequestLoading: boolean;
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
    tenantLoading: true,
    tenantError: null,
    tenantRequestLoading: false,
  });

  availableTenants: AvailableTenant[] = [];
  myRequests: TenantJoinRequest[] = [];
  selectedTenantId: number | null = null;
  requestMessage = '';

  private readonly developerService = inject(DeveloperService);
  private readonly tenantService = inject(TenantService);
  private readonly errorMapper = inject(ErrorMapperService);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadProfile();
    this.loadTenantData();
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

  loadTenantData(): void {
    this.state.update(s => ({ ...s, tenantLoading: true, tenantError: null }));
    this.tenantService.getAvailableTenants()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: tenants => {
          this.tenantService.getMyRequests()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: requests => {
                this.availableTenants = tenants;
                this.myRequests = requests;
                this.state.update(s => ({ ...s, tenantLoading: false }));
              },
              error: err => this.state.update(s => ({ ...s, tenantLoading: false, tenantError: this.errorMapper.map(err) })),
            });
        },
        error: err => this.state.update(s => ({ ...s, tenantLoading: false, tenantError: this.errorMapper.map(err) })),
      });
  }

  requestToJoin(tenant: AvailableTenant): void {
    this.state.update(s => ({ ...s, tenantRequestLoading: true, tenantError: null }));
    this.tenantService.createRequest({ tenantId: tenant.id, message: this.requestMessage })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.requestMessage = '';
          this.selectedTenantId = null;
          this.loadTenantData();
          this.state.update(s => ({ ...s, tenantRequestLoading: false }));
        },
        error: err => this.state.update(s => ({ ...s, tenantRequestLoading: false, tenantError: this.errorMapper.map(err) })),
      });
  }

  isPending(tenantId: number): boolean {
    return this.myRequests.some(r => r.tenantId === tenantId && r.status === 'Pending');
  }
}
