import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID, Renderer2, RendererFactory2 } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export type EafTheme = 'light' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly storageKey = 'eaf-theme';
  private readonly renderer: Renderer2;
  private readonly platformId: Object;
  private readonly theme = new BehaviorSubject<EafTheme>('light');

  theme$: Observable<EafTheme> = this.theme.asObservable();

  constructor(rendererFactory: RendererFactory2, @Inject(PLATFORM_ID) platformId: Object) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.platformId = platformId;
  }

  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const saved = this.getSavedTheme();
    this.apply(saved);
  }

  toggle(): void {
    const next: EafTheme = this.theme.value === 'dark' ? 'light' : 'dark';
    this.apply(next);
  }

  setTheme(theme: EafTheme): void {
    this.apply(theme);
  }

  currentTheme(): EafTheme {
    return this.theme.value;
  }

  private apply(theme: EafTheme): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.renderer.setAttribute(document.documentElement, 'data-theme', theme);
    localStorage.setItem(this.storageKey, theme);
    this.theme.next(theme);
    this.loadPrimeNgTheme(theme);
  }

  private getSavedTheme(): EafTheme {
    if (!isPlatformBrowser(this.platformId)) {
      return 'light';
    }

    const stored = localStorage.getItem(this.storageKey) as EafTheme | null;
    return stored === 'dark' ? 'dark' : 'light';
  }

  private loadPrimeNgTheme(theme: EafTheme): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    if (theme === 'light') {
      const existing = document.getElementById('eaf-primeng-theme') as HTMLLinkElement | null;
      if (existing) {
        this.renderer.removeChild(existing.parentNode, existing);
      }
      return;
    }

    const darkThemeHref = 'assets/primeng/themes/lara-dark-blue/theme.css';
    const existing = document.getElementById('eaf-primeng-theme') as HTMLLinkElement | null;

    if (existing) {
      existing.href = darkThemeHref;
    } else {
      const link = this.renderer.createElement('link') as HTMLLinkElement;
      link.id = 'eaf-primeng-theme';
      link.rel = 'stylesheet';
      link.href = darkThemeHref;
      this.renderer.appendChild(document.head, link);
    }
  }
}
