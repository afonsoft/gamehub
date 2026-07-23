import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, RegisterModel } from '../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  model: RegisterModel = { name: '', surname: '', userName: '', emailAddress: '', password: '', isDeveloper: false };
  confirmPassword = '';
  loading = false;
  error = '';

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

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
    this.loading = true;
    this.auth.register(this.model).subscribe({
      next: result => {
        this.loading = false;
        if (result.success) {
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
