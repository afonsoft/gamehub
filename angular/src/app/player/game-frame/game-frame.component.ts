import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GameplayBridgeService } from '../../core/services/gameplay-bridge.service';

@Component({
  selector: 'app-game-frame',
  standalone: true,
  imports: [CommonModule],
  template: `
    <iframe #frame *ngIf="url" [src]="url" width="100%" height="100%"></iframe>
  `,
  styles: [':host { display: block; height: 100vh; } iframe { border: 0; }']
})
export class GameFrameComponent implements OnInit {
  @ViewChild('frame') frame!: ElementRef<HTMLIFrameElement>;
  url: string | null = null;

  constructor(private route: ActivatedRoute, private bridge: GameplayBridgeService) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.url = `/games/${slug}/index.html`;
    this.bridge.startSession(slug).subscribe();
  }
}
