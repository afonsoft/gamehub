import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, AuthenticateModel } from '../../core/auth/auth.service';

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
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  login(): void {
    this.error = '';
    if (!this.model.userNameOrEmailAddress || !this.model.password) {
      this.error = 'Please fill in all fields.';
      return;
    }
    this.loading = true;
    this.auth.login(this.model).subscribe({
      next: success => {
        this.loading = false;
        if (success) {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
          void this.router.navigateByUrl(returnUrl);
        } else {
          this.error = 'Invalid username or password.';
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Unable to login. Please try again.';
      },
    });
  }
}
