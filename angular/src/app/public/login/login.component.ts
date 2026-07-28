import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, AuthenticateModel } from '../../core/auth/auth.service';
import { HubAuthService, AvailableTenantResult } from '../../core/auth/hub-auth.service';
import { TokenService } from '../../core/auth/token.service';

interface LoginState {
  userNameOrEmailAddress: string;
  password: string;
  tenants: AvailableTenantResult[];
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  model: AuthenticateModel = { userNameOrEmailAddress: '', password: '' };
  loading = false;
  error = '';

  private readonly auth = inject(AuthService);
  private readonly hubAuth = inject(HubAuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly tokenService = inject(TokenService);

  login(): void {
    this.error = '';
    if (!this.model.userNameOrEmailAddress || !this.model.password) {
      this.error = 'Please fill in all fields.';
      return;
    }
    this.loading = true;
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    this.hubAuth.getAvailableTenants(this.model).subscribe({
      next: tenants => {
        this.loading = false;
        if (tenants.length === 0) {
          this.loginAsHost(returnUrl);
          return;
        }
        if (tenants.length === 1) {
          this.selectTenantAndLogin(tenants[0].tenantId, returnUrl);
          return;
        }
        const state: LoginState = {
          userNameOrEmailAddress: this.model.userNameOrEmailAddress,
          password: this.model.password,
          tenants,
        };
        void this.router.navigate(['/select-tenant'], { state, queryParams: { returnUrl } });
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.details || err?.error?.message || 'Invalid username or password.';
      },
    });
  }

  private selectTenantAndLogin(tenantId: number, returnUrl: string): void {
    this.loading = true;
    this.hubAuth
      .selectTenant({
        userNameOrEmailAddress: this.model.userNameOrEmailAddress,
        password: this.model.password,
        tenantId,
      })
      .subscribe({
        next: result => {
          this.loading = false;
          if (result?.accessToken) {
            this.auth.finalizeLogin(result.accessToken, returnUrl);
          } else {
            this.error = 'Unable to complete login.';
          }
        },
        error: () => {
          this.loading = false;
          this.error = 'Unable to complete login. Please try again.';
        },
      });
  }

  private loginAsHost(returnUrl: string): void {
    this.loading = true;
    this.auth.login(this.model).subscribe({
      next: success => {
        this.loading = false;
        const token = this.tokenService.getToken();
        if (success && token) {
          this.auth.finalizeLogin(token, returnUrl);
        } else {
          this.error = 'Invalid username or password.';
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Unable to complete login. Please try again.';
      },
    });
  }
}
