import { Component, OnDestroy, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription, filter } from 'rxjs';
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
export class App implements OnDestroy {
  protected readonly title = 'GameHub';
  mobileMenuOpen = false;
  playMode = false;

  private readonly auth = inject(AuthService);
  private readonly token = inject(TokenService);
  private readonly router = inject(Router);
  private readonly subs = new Subscription();

  constructor() {
    this.playMode = this.isPlayUrl(this.router.url);

    this.subs.add(
      this.router.events
        .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
        .subscribe(event => {
          this.playMode = this.isPlayUrl(event.urlAfterRedirects);
          this.mobileMenuOpen = false;
        })
    );
  }

  get isLoggedIn(): boolean {
    return this.token.isValid();
  }

  get userName(): string | null {
    return this.token.getUserName();
  }

  get isDeveloper(): boolean {
    return this.token.getRoles().map(r => r.toLowerCase()).includes('developer') || this.token.getRoles().map(r => r.toLowerCase()).includes('admin');
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  logout(): void {
    this.auth.logout('/');
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  private isPlayUrl(url: string): boolean {
    return url.startsWith('/play') || url.startsWith('/preview');
  }
}
