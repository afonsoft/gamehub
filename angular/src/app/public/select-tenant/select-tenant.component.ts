import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AvailableTenantResult, HubAuthService } from '../../core/auth/hub-auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface LoginState {
  userNameOrEmailAddress: string;
  password: string;
  tenants: AvailableTenantResult[];
}

@Component({
  selector: 'app-select-tenant',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './select-tenant.component.html',
  styleUrl: './select-tenant.component.css',
})
export class SelectTenantComponent implements OnInit {
  tenants: AvailableTenantResult[] = [];
  selectedTenantId: number | undefined;
  userNameOrEmailAddress = '';
  password = '';
  loading = false;
  error = '';
  returnUrl = '/';

  private readonly hubAuth = inject(HubAuthService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    const navigation = this.router.getCurrentNavigation();
    const state = (navigation?.extras?.state as LoginState) || (history.state as LoginState);

    if (!state?.tenants?.length) {
      void this.router.navigate(['/login']);
      return;
    }

    this.tenants = state.tenants;
    this.userNameOrEmailAddress = state.userNameOrEmailAddress;
    this.password = state.password;
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    const defaultTenant = this.tenants.find(t => t.isDefault);
    this.selectedTenantId = defaultTenant?.tenantId ?? this.tenants[0].tenantId;
  }

  selectTenant(): void {
    if (!this.selectedTenantId) {
      this.error = 'Please select a tenant.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.hubAuth
      .selectTenant({
        userNameOrEmailAddress: this.userNameOrEmailAddress,
        password: this.password,
        tenantId: this.selectedTenantId,
      })
      .subscribe({
        next: result => {
          this.loading = false;
          if (result?.accessToken) {
            this.auth.finalizeLogin(result.accessToken, this.returnUrl);
          } else {
            this.error = 'Unable to complete login.';
          }
        },
        error: err => {
          this.loading = false;
          this.error = err?.error?.details || err?.error?.message || 'Unable to complete login.';
        },
      });
  }
}
