import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [type]="type"
      [disabled]="disabled"
      [class]="'btn ' + variant"
      (click)="onClick()">
      <ng-content></ng-content>
    </button>
  `,
  styles: [
    `:host { display: inline-block; }`,
    `.btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 0.625rem 1.25rem;
      border: none;
      border-radius: var(--gh-radius);
      font-weight: 600;
      cursor: pointer;
      transition: filter 0.15s, transform 0.05s;
    }`,
    `.btn:disabled { opacity: 0.5; cursor: not-allowed; }`,
    `.btn.primary { background: var(--gh-primary); color: #fff; }`,
    `.btn.secondary { background: var(--gh-secondary); color: #fff; }`,
    `.btn.ghost { background: transparent; color: var(--gh-primary); border: 1px solid var(--gh-primary); }`,
  ],
})
export class ButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'ghost' = 'primary';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Output() appClick = new EventEmitter<void>();

  onClick(): void {
    if (!this.disabled) {
      this.appClick.emit();
    }
  }
}
