import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `<div class="skeleton" [style.width]="width" [style.height]="height" [style.borderRadius]="radius"></div>`,
  styles: [
    `.skeleton {
      background: linear-gradient(90deg, var(--gh-muted) 25%, var(--gh-surface) 50%, var(--gh-muted) 75%);
      background-size: 200% 100%;
      animation: shimmer 1.2s infinite linear;
    }`,
    `@keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }`,
  ],
})
export class SkeletonComponent {
  @Input() width = '100%';
  @Input() height = '1rem';
  @Input() radius = 'var(--gh-radius)';
}
