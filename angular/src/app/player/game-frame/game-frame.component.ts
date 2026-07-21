import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { GameCatalogService, GameDetail } from '../../core/services/game-catalog.service';
import { GameplayBridgeService } from '../../core/services/gameplay-bridge.service';

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
        allowfullscreen>
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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer: DomSanitizer,
    private catalog: GameCatalogService,
    private bridge: GameplayBridgeService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.catalog.getBySlug(slug).subscribe({
      next: (game: GameDetail | null) => {
        this.loaded = true;
        if (!game?.publishedBuildUrl) {
          return;
        }
        this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(game.publishedBuildUrl);
        this.bridge.startSession(game.id).subscribe(session => {
          this.sessionId = session?.sessionId ?? null;
        });
      },
      error: () => {
        this.loaded = true;
      },
    });
  }

  ngOnDestroy(): void {
    if (this.sessionId) {
      this.bridge.sendEvent(this.sessionId, 'gameplayStop').subscribe();
    }
  }

  back(): void {
    this.router.navigate(['/games']);
  }
}
