import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, RegisterModel, TenantSelectionMode } from '../../core/auth/auth.service';
import { TenantService, AvailableTenant } from '../../core/services/tenant.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit {
  model: RegisterModel = {
    name: '',
    surname: '',
    userName: '',
    emailAddress: '',
    password: '',
    isDeveloper: false,
    tenantSelectionMode: 'PlayerDefault',
    existingTenantId: null,
  };
  confirmPassword = '';
  loading = false;
  error = '';
  availableTenants: AvailableTenant[] = [];
  tenantMode: TenantSelectionMode = 'PlayerDefault';

  private readonly auth = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.loadAvailableTenants();
  }

  loadAvailableTenants(): void {
    this.tenantService.getAvailableTenants().subscribe({
      next: tenants => {
        this.availableTenants = tenants;
      },
      error: () => {
        this.availableTenants = [];
      },
    });
  }

  onTenantModeChange(): void {
    this.model.tenantSelectionMode = this.tenantMode;
    if (this.tenantMode !== 'JoinExisting') {
      this.model.existingTenantId = null;
    }
    if (this.tenantMode !== 'CreateNew') {
      this.model.newTenantName = '';
    }
  }

  register(): void {
    this.error = '';
    if (!this.model.name || !this.model.surname || !this.model.userName || !this.model.emailAddress || !this.model.password) {
      this.error = 'Please fill in all fields.';
      return;
    }
    if (this.model.password !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }
    if (this.tenantMode === 'CreateNew' && !this.model.newTenantName?.trim()) {
      this.error = 'Please enter a company name.';
      return;
    }
    if (this.tenantMode === 'JoinExisting' && !this.model.existingTenantId) {
      this.error = 'Please select a company.';
      return;
    }

    this.model.tenantSelectionMode = this.tenantMode;
    this.loading = true;
    this.auth.register(this.model).subscribe({
      next: result => {
        this.loading = false;
        if (result.success) {
          if (this.tenantMode === 'JoinExisting' || !result.canLogin) {
            void this.router.navigate(['/'], { state: { pendingApproval: true } });
            return;
          }
          const target = this.model.isDeveloper && this.auth.isDeveloper() ? '/developer' : '/';
          void this.router.navigate([target]);
        } else {
          this.error = result.error || 'Registration failed. Please try again.';
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Unable to register. Please try again.';
      },
    });
  }
}
