import { Component, OnInit, OnDestroy, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';
import { GameplayBridgeService, StartPlaySessionInput } from '../../core/services/gameplay-bridge.service';

@Component({
  selector: 'app-game-frame',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="frame-shell">
      <iframe
        #frame
        *ngIf="safeUrl"
        [src]="safeUrl"
        title="Game"
        width="100%"
        height="100%"
        sandbox="allow-scripts allow-pointer-lock allow-same-origin allow-forms"
        allow="fullscreen; gamepad"
        referrerpolicy="no-referrer">
      </iframe>
      <div *ngIf="!safeUrl && loaded" class="error">
        <p>This game is not available to play right now.</p>
        <a (click)="back()">Back to games</a>
      </div>
    </div>
  `,
  styles: [
    ':host { display: block; height: 100vh; }',
    '.frame-shell { width: 100%; height: 100%; background: #000; }',
    'iframe { border: 0; display: block; }',
    '.error { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #fff; gap: 1rem; }',
    '.error a { color: #ff5e57; cursor: pointer; text-decoration: underline; font-weight: 700; }',
  ],
})
export class GameFrameComponent implements OnInit, OnDestroy {
  @ViewChild('frame') frame!: ElementRef<HTMLIFrameElement>;
  safeUrl: SafeResourceUrl | null = null;
  loaded = false;
  private sessionId: string | null = null;
  private gameId: string | null = null;
  private readonly messageHandler = (event: MessageEvent<unknown>) => this.bridge.handleMessage(event);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly catalog = inject(GameCatalogService);
  private readonly bridge = inject(GameplayBridgeService);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe({
      next: (game: GameDetail | null) => {
        this.loaded = true;
        if (!game?.publishedBuildUrl) {
          return;
        }
        this.gameId = game.id;
        this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(game.publishedBuildUrl);
        this.bridge.setReplyHandler(msg => this.postToGame(msg));

        const input: StartPlaySessionInput = {
          gameId: game.id,
          deviceType: 'Desktop',
          browser: navigator.userAgent,
          referrer: document.referrer,
        };
        this.bridge.startSession(input).subscribe({
          next: session => {
            this.sessionId = session?.sessionId ?? null;
            if (this.sessionId && this.gameId) {
              this.bridge.setSession(this.sessionId, this.gameId);
              this.bridge.gameplayStart();
            }
          },
          error: () => {
            // Session creation failed; still allow the game to load.
          },
        });
      },
      error: () => {
        this.loaded = true;
      },
    });

    window.addEventListener('message', this.messageHandler);
  }

  ngOnDestroy(): void {
    window.removeEventListener('message', this.messageHandler);
    if (this.sessionId) {
      this.bridge.gameplayStop();
      this.bridge.stopSession(this.sessionId).subscribe();
    }
    this.bridge.setReplyHandler(undefined);
  }

  back(): void {
    void this.router.navigate(['/games']);
  }

  private postToGame(message: unknown): void {
    const contentWindow = this.frame?.nativeElement?.contentWindow;
    if (contentWindow) {
      contentWindow.postMessage(message, '*');
    }
  }
}
