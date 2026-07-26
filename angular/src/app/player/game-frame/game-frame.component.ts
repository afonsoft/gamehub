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
  isPreview = false;
  previewVersion: string | null = null;
  toast: { message: string; type: 'warning' | 'error' | 'info' } | null = null;
  pillTop: string | null = null;
  rewardedBreakPending: { resolve: (rewarded: boolean) => void } | null = null;
  rewardedBreakState: 'idle' | 'watching' | 'adblocked' | 'rewarded' | 'dismissed' = 'idle';
  private sessionId: string | null = null;
  private gameId: string | null = null;
  private gameOrigin = '*';
  private skipTimeout?: number;
  private toastTimeout?: number;
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
    this.isPreview = this.route.snapshot.url.some(s => s.path === 'preview');
    this.previewVersion = this.route.snapshot.paramMap.get('version');

    if (this.isPreview && this.previewVersion) {
      const token = this.route.snapshot.queryParamMap.get('token') ?? '';
      this.loadPreview(slug, this.previewVersion, token);
    } else {
      this.loadGame(slug);
    }

    window.addEventListener('message', this.messageHandler);
    document.addEventListener('fullscreenchange', this.fullscreenHandler);
    window.addEventListener('focusin', this.focusHandler, true);
    window.addEventListener('focusout', this.blurHandler, true);
    window.addEventListener('keydown', this.keyHandler);
  }

  private loadGame(slug: string): void {
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
        this.configureGame(game);
      },
      error: () => {
        this.loadingError = true;
      },
    });
  }

  private loadPreview(slug: string, version: string, token: string): void {
    this.catalog.validatePreview(token).subscribe({
      next: result => {
        if (!result.isValid || !result.previewUrl) {
          this.loadingError = true;
          return;
        }

        this.catalog.getBySlug(slug).subscribe({
          next: (game: GameDetail | null) => {
            if (!game) {
              this.loadingError = true;
              return;
            }

            this.game = game;
            this.gameId = game.id;
            this.gameOrigin = new URL(result.previewUrl).origin;
            this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(result.previewUrl);
            this.configureGame(game);
          },
          error: () => {
            this.loadingError = true;
          },
        });
      },
      error: () => {
        this.loadingError = true;
      },
    });
  }

  private configureGame(game: GameDetail): void {
    this.controlsHint = this.buildControlsHint(game);
    this.privacyConsentUrl = game.privacyPolicyUrl ?? '';
    this.privacyConsentNeeded = this.privacyConsentUrl.length > 0 && !this.bridge.getStoredPrivacyConsent().consented;
    this.selectedLanguage = this.resolveLanguage(game);
    this.bridge.setSession('', this.gameId ?? '');
    this.bridge.setGame(game.slug);
    this.bridge.setGameOrigin(this.gameOrigin);
    this.bridge.setReplyHandler(msg => this.postToGame(msg));
    this.bridge.setOnSaveError(() => this.showToast('Progresso local apenas', 'warning'));
    this.bridge.setOnMovePill((topPercent, topPx) => this.setPillPosition(topPercent, topPx));
    this.bridge.setOnRewardedBreak(resolve => this.showRewardedBreak(resolve));

    const inspector = this.route.snapshot.queryParamMap.get('inspector');
    const inspectorSession = this.route.snapshot.queryParamMap.get('inspectorSession');
    if (inspector === '1' && inspectorSession) {
      this.bridge.setInspectorMode(true, inspectorSession);
    }
  }

  acceptPrivacyConsent(): void {
    if (!this.game) {
      return;
    }

    this.privacyConsentGiven = true;
    this.privacyConsentNeeded = false;
    void this.bridge.setPrivacyConsent('', true, '1.0');
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

  private setPillPosition(topPercent?: number, topPx?: number): void {
    if (topPx !== undefined && topPx !== null) {
      this.pillTop = `${topPx}px`;
    } else if (topPercent !== undefined && topPercent !== null) {
      this.pillTop = `${topPercent}%`;
    } else {
      this.pillTop = null;
    }
  }

  showToast(message: string, type: 'warning' | 'error' | 'info' = 'info'): void {
    this.toast = { message, type };
    if (this.toastTimeout) {
      window.clearTimeout(this.toastTimeout);
    }
    this.toastTimeout = window.setTimeout(() => (this.toast = null), 4000);
  }

  hideToast(): void {
    this.toast = null;
    if (this.toastTimeout) {
      window.clearTimeout(this.toastTimeout);
      this.toastTimeout = undefined;
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

  showRewardedBreak(resolve: (rewarded: boolean) => void): void {
    this.rewardedBreakPending = { resolve };
    this.rewardedBreakState = 'idle';
  }

  async confirmRewardedBreak(): Promise<void> {
    if (!this.rewardedBreakPending) return;
    this.rewardedBreakState = 'watching';

    const { rewarded, adBlocked } = await this.bridge.requestRewardedAd();
    this.rewardedBreakState = adBlocked ? 'adblocked' : rewarded ? 'rewarded' : 'dismissed';

    window.setTimeout(() => {
      this.rewardedBreakPending?.resolve(rewarded);
      this.rewardedBreakPending = null;
      this.rewardedBreakState = 'idle';
    }, adBlocked ? 1200 : 800);
  }

  declineRewardedBreak(): void {
    this.rewardedBreakState = 'dismissed';
    window.setTimeout(() => {
      this.rewardedBreakPending?.resolve(false);
      this.rewardedBreakPending = null;
      this.rewardedBreakState = 'idle';
    }, 300);
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
