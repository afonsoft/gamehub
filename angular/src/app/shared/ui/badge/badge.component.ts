import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [class]="variant">{{ label }}</span>`,
  styles: [
    `.badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: var(--gh-radius);
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      background: var(--gh-muted);
      color: var(--gh-text-secondary);
    }`,
    `.badge.primary { background: var(--gh-primary); color: #fff; }`,
    `.badge.success { background: var(--gh-success); color: #fff; }`,
    `.badge.warning { background: var(--gh-warning); color: #000; }`,
    `.badge.danger { background: var(--gh-danger); color: #fff; }`,
  ],
})
export class BadgeComponent {
  @Input() label = '';
  @Input() variant: 'default' | 'primary' | 'success' | 'warning' | 'danger' = 'default';
}
