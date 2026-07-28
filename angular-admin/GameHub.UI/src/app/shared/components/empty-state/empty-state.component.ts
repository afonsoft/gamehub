import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: false,
  template: `
    <div class="text-center p-4 text-muted">
      <i class="fa fa-inbox fa-2x mb-3"></i>
      <p class="mb-0">{{ message }}</p>
    </div>
  `,
})
export class EmptyStateComponent {
  @Input() message = '';
}
