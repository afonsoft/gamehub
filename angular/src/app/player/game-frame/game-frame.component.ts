import { Component, OnInit, OnDestroy, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';
import { GameplayBridgeService, StartPlaySessionInput } from '../../core/services/gameplay-bridge.service';
import { PlayerService } from '../../core/services/player.service';
import { TokenService } from '../../core/auth/token.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-game-frame',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './game-frame.component.html',
  styleUrl: './game-frame.component.css',
})
export class GameFrameComponent implements OnInit, OnDestroy {
  @ViewChild('frame') frame!: ElementRef<HTMLIFrameElement>;
  safeUrl: SafeResourceUrl | null = null;
  started = false;
  starting = false;
  loadingError = false;
  startError: string | null = null;
  isFullscreen = false;
  isFocused = false;
  controlsHint = '';
  showSkip = false;
  privacyConsentNeeded = false;
  privacyConsentGiven = false;
  privacyConsentUrl = '';
  selectedLanguage = 'en-US';
  game: GameDetail | null = null;
  private sessionId: string | null = null;
  private gameId: string | null = null;
  private gameOrigin = '*';
  private skipTimeout?: number;
  private readonly messageHandler = (event: MessageEvent<unknown>) => this.bridge.handleMessage(event);
  private readonly fullscreenHandler = () => this.onFullscreenChange();
  private readonly focusHandler = (event: FocusEvent) => this.onFocusChange(event, true);
  private readonly blurHandler = (event: FocusEvent) => this.onFocusChange(event, false);
  private readonly keyHandler = (event: KeyboardEvent) => this.onKeyDown(event);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly http = inject(HttpClient);
  private readonly catalog = inject(GameCatalogService);
  private readonly bridge = inject(GameplayBridgeService);
  private readonly player = inject(PlayerService);
  private readonly token = inject(TokenService);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe({
      next: (game: GameDetail | null) => {
        if (!game?.publishedBuildUrl) {
          this.loadingError = true;
          return;
        }

        this.game = game;
        this.gameId = game.id;
        this.gameOrigin = new URL(game.publishedBuildUrl).origin;
        this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(game.publishedBuildUrl);
        this.controlsHint = this.buildControlsHint(game);
        this.privacyConsentUrl = game.privacyPolicyUrl ?? '';
        this.privacyConsentNeeded = this.privacyConsentUrl.length > 0 && !this.hasPrivacyConsent(game.slug);
        this.selectedLanguage = this.resolveLanguage(game);
        this.bridge.setGame(game.slug);
        this.bridge.setGameOrigin(this.gameOrigin);
        this.bridge.setReplyHandler(msg => this.postToGame(msg));
      },
      error: () => {
        this.loadingError = true;
      },
    });

    window.addEventListener('message', this.messageHandler);
    document.addEventListener('fullscreenchange', this.fullscreenHandler);
    window.addEventListener('focusin', this.focusHandler, true);
    window.addEventListener('focusout', this.blurHandler, true);
    window.addEventListener('keydown', this.keyHandler);
  }

  acceptPrivacyConsent(): void {
    if (!this.game) {
      return;
    }

    this.privacyConsentGiven = true;
    this.privacyConsentNeeded = false;
    this.savePrivacyConsent(this.game.slug);

    if (this.token.isValid()) {
      this.http.post('/api/services/app/Privacy/SaveConsent', {
        gameId: this.game.id,
        policyVersion: '1.0'
      }).subscribe();
    }
  }

  startGame(): void {
    if (!this.game || !this.safeUrl || this.starting || this.privacyConsentNeeded) {
      return;
    }

    this.starting = true;
    this.startError = null;

    const input: StartPlaySessionInput = {
      gameId: this.game.id,
      deviceType: this.detectDevice(),
      browser: navigator.userAgent,
      referrer: document.referrer,
    };

    this.bridge.startSession(input).subscribe({
      next: session => {
        this.starting = false;
        this.started = true;
        this.sessionId = session?.sessionId ?? null;
        if (this.sessionId && this.gameId) {
          this.bridge.setSession(this.sessionId, this.gameId);
          this.bridge.gameplayStart();
          this.player.trackPlay(this.gameId, this.token.isValid()).subscribe();
        }
      },
      error: () => {
        this.starting = false;
        this.startError = 'gameFrame.sessionError';
        this.started = true;
      },
    });
  }

  ngOnDestroy(): void {
    window.removeEventListener('message', this.messageHandler);
    document.removeEventListener('fullscreenchange', this.fullscreenHandler);
    window.removeEventListener('focusin', this.focusHandler, true);
    window.removeEventListener('focusout', this.blurHandler, true);
    window.removeEventListener('keydown', this.keyHandler);
    if (this.skipTimeout) {
      window.clearTimeout(this.skipTimeout);
    }
    if (this.sessionId) {
      this.bridge.gameplayStop();
      this.bridge.stopSession(this.sessionId).subscribe();
    }
    this.bridge.setReplyHandler(undefined);
  }

  back(): void {
    void this.router.navigate(['/games']);
  }

  toggleFullscreen(): void {
    const frame = this.frame?.nativeElement;
    if (!frame) return;

    if (!document.fullscreenElement) {
      frame.requestFullscreen?.().catch(() => {});
    } else {
      document.exitFullscreen?.().catch(() => {});
    }
  }

  skipCutscene(): void {
    this.postToGame({ channel: 'gamehub-bridge', action: 'skipCutscene' });
    this.showSkip = false;
    if (this.skipTimeout) {
      window.clearTimeout(this.skipTimeout);
    }
  }

  onFrameLoad(): void {
    this.bridge.gameLoadingFinished();
    this.postToGame({
      channel: 'gamehub-bridge',
      action: 'controlScheme',
      payload: {
        scheme: this.game?.controlScheme ?? 'Both',
        language: this.selectedLanguage,
      },
    });

    if (this.game?.cutscenesSkippable) {
      this.skipTimeout = window.setTimeout(() => {
        this.showSkip = true;
      }, 2000);
    }
  }

  private postToGame(message: unknown): void {
    const contentWindow = this.frame?.nativeElement?.contentWindow;
    if (contentWindow) {
      contentWindow.postMessage(message, this.gameOrigin);
    }
  }

  private detectDevice(): string {
    const ua = navigator.userAgent;
    if (/Mobi/i.test(ua)) return 'Mobile';
    if (/Tablet/i.test(ua) || /iPad/i.test(ua)) return 'Tablet';
    return 'Desktop';
  }

  private hasPrivacyConsent(slug: string): boolean {
    try {
      return localStorage.getItem(`gamehub-privacy-${slug}`) === 'true';
    } catch {
      return false;
    }
  }

  private savePrivacyConsent(slug: string): void {
    try {
      localStorage.setItem(`gamehub-privacy-${slug}`, 'true');
    } catch {
      // Ignore in private/incognito mode.
    }
  }

  private onFullscreenChange(): void {
    this.isFullscreen = !!document.fullscreenElement;
  }

  private onFocusChange(event: FocusEvent, focused: boolean): void {
    const target = event.target as HTMLElement | null;
    if (target?.tagName === 'IFRAME') {
      this.isFocused = focused;
      this.toggleParentScroll(!focused);
    }
  }

  private onKeyDown(event: KeyboardEvent): void {
    if (!this.isFocused || !this.started) {
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this.postToGame({ channel: 'gamehub-bridge', action: 'pauseRequested' });
      return;
    }

    if (event.code === 'Space') {
      event.preventDefault();
      this.postToGame({ channel: 'gamehub-bridge', action: 'resumeRequested' });
    }
  }

  private toggleParentScroll(enabled: boolean): void {
    const body = document.body;
    if (!body) {
      return;
    }

    if (enabled) {
      body.style.overflow = '';
    } else {
      body.style.overflow = 'hidden';
    }
  }

  onLanguageChange(language: string): void {
    this.selectedLanguage = language;
    void this.bridge.setLanguage('', language);
  }

  canSelectLanguage(): boolean {
    const languages = this.game?.supportedLanguages;
    return !!languages && languages.length > 1;
  }

  private resolveLanguage(game: GameDetail): string {
    const stored = this.bridge.getStoredLanguage();
    const supported = game.supportedLanguages ?? [];
    const defaultLanguage = game.defaultLanguage ?? 'en-US';
    if (stored && supported.includes(stored)) {
      return stored;
    }
    if (supported.includes(defaultLanguage)) {
      return defaultLanguage;
    }
    return supported[0] ?? defaultLanguage;
  }

  private buildControlsHint(game: GameDetail): string {
    const device = this.detectDevice();
    const scheme = (game.controlScheme ?? 'Both').toLowerCase();
    if (device === 'Desktop' && (scheme === 'keyboard' || scheme === 'both')) {
      return 'gameFrame.controlsKeyboard';
    }
    if ((device === 'Mobile' || device === 'Tablet') && (scheme === 'touch' || scheme === 'both')) {
      return 'gameFrame.controlsTouch';
    }
    return 'gameFrame.controlsGeneric';
  }
}
