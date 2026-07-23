import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="pagination" *ngIf="totalPages > 1">
      <button (click)="goTo(currentPage - 1)" [disabled]="currentPage <= 1">&lsaquo;</button>
      <span>{{ currentPage }} / {{ totalPages }}</span>
      <button (click)="goTo(currentPage + 1)" [disabled]="currentPage >= totalPages">&rsaquo;</button>
    </nav>
  `,
  styles: [
    `.pagination { display: flex; align-items: center; justify-content: center; gap: 0.75rem; }`,
    `button { padding: 0.375rem 0.75rem; border: 1px solid var(--gh-border); background: var(--gh-surface); border-radius: var(--gh-radius); cursor: pointer; }`,
    `button:disabled { opacity: 0.5; cursor: not-allowed; }`,
  ],
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Output() pageChange = new EventEmitter<number>();

  goTo(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.pageChange.emit(page);
    }
  }
}
