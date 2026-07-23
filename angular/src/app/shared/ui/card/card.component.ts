import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `<div class="card"><ng-content></ng-content></div>`,
  styles: [
    `.card {
      background: var(--gh-surface);
      border-radius: var(--gh-radius-lg);
      box-shadow: var(--gh-shadow);
      padding: 1rem;
    }`,
  ],
})
export class CardComponent {}
