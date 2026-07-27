import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth/auth.service';
import { TokenService } from './core/auth/token.service';
import { TranslatePipe } from './core/i18n/translate.pipe';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = 'GameHub';
  private readonly auth = inject(AuthService);
  private readonly token = inject(TokenService);

  get isLoggedIn(): boolean {
    return this.token.isValid();
  }

  get userName(): string | null {
    return this.token.getUserName();
  }

  get isDeveloper(): boolean {
    return this.token.getRoles().map(r => r.toLowerCase()).includes('developer') || this.token.getRoles().map(r => r.toLowerCase()).includes('admin');
  }

  logout(): void {
    this.auth.logout('/');
  }
}
