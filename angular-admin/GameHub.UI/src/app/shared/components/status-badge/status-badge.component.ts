import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: false,
  template: `<span class="m-badge" [ngClass]="value ? trueClass : falseClass">{{ value ? trueLabel : falseLabel }}</span>`,
})
export class StatusBadgeComponent {
  @Input() value: boolean;
  @Input() trueLabel = 'Yes';
  @Input() falseLabel = 'No';
  @Input() trueClass = 'm-badge--success m-badge--wide';
  @Input() falseClass = 'm-badge--metal m-badge--wide';
}
