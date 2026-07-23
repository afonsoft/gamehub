import { Component, OnInit, OnDestroy, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';
import { GameplayBridgeService, StartPlaySessionInput } from '../../core/services/gameplay-bridge.service';
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
  game: GameDetail | null = null;
  private sessionId: string | null = null;
  private gameId: string | null = null;
  private gameOrigin = '*';
  private readonly messageHandler = (event: MessageEvent<unknown>) => this.bridge.handleMessage(event);
  private readonly fullscreenHandler = () => this.onFullscreenChange();

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly catalog = inject(GameCatalogService);
  private readonly bridge = inject(GameplayBridgeService);

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
        this.bridge.setGameOrigin(this.gameOrigin);
        this.bridge.setReplyHandler(msg => this.postToGame(msg));
      },
      error: () => {
        this.loadingError = true;
      },
    });

    window.addEventListener('message', this.messageHandler);
    document.addEventListener('fullscreenchange', this.fullscreenHandler);
  }

  startGame(): void {
    if (!this.game || !this.safeUrl || this.starting) {
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

  onFrameLoad(): void {
    this.bridge.gameLoadingFinished();
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

  private onFullscreenChange(): void {
    this.isFullscreen = !!document.fullscreenElement;
  }
}
