import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet, Router } from '@angular/router';
import { TokenService } from '../../../core/auth/token.service';

@Component({
  selector: 'app-developer-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './developer-shell.component.html',
  styleUrl: './developer-shell.component.css',
})
export class DeveloperShellComponent {
  isMobileNavOpen = signal(false);
  readonly isAuthenticated = signal(false);

  private readonly token = inject(TokenService);
  private readonly router = inject(Router);

  constructor() {
    this.isAuthenticated.set(this.token.isValid());
  }

  toggleMobileNav(): void {
    this.isMobileNavOpen.update(open => !open);
  }

  closeMobileNav(): void {
    this.isMobileNavOpen.set(false);
  }

  async logout(): Promise<void> {
    this.token.clearToken();
    await this.router.navigate(['/']);
  }
}
