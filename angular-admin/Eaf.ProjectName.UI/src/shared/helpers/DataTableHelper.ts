import { Paginator } from 'primeng/paginator';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';

export class DataTableHelper {
  predefinedRecordsCountPerPage = [10, 20, 30, 40, 50, 100];
  defaultRecordsCountPerPage = 10;
  resizableColumns: false;
  totalRecordsCount = 0;
  records: any[];
  isLoading = false;

  showLoadingIndicator(): void {
    setTimeout(() => {
      this.isLoading = true;
    }, 100);
  }

  hideLoadingIndicator(): void {
    setTimeout(() => {
      this.isLoading = false;
    }, 100);
  }

  getSorting(table: Table): string {
    let sorting;
    if (table !== undefined && table.sortField) {
      sorting = table.sortField;
      if (table.sortOrder === 1) {
        sorting += ' ASC';
      } else if (table.sortOrder === -1) {
        sorting += ' DESC';
      }
    }
    return sorting;
  }

  getMaxResultCount(paginator: Paginator, event: LazyLoadEvent): number {
    if (paginator !== undefined && paginator.rows) {
      if (paginator.rows > 0) return paginator.rows;
      else return 99999;
    }
    if (!event) {
      return 99999;
    }
    if (event.rows > 0) return event.rows;
    else return 99999;
  }

  getSkipCount(paginator: Paginator, event: LazyLoadEvent): number {
    if (paginator !== undefined && paginator.first) {
      if (event.first > 0) return paginator.first;
      else return this.defaultRecordsCountPerPage;
    }
    if (!event) {
      return this.defaultRecordsCountPerPage;
    }
    return event.first;
  }

  shouldResetPaging(event: LazyLoadEvent): boolean {
    if (!event /*|| event.sortField*/) {
      // if you want to reset after sorting, comment out parameter
      return true;
    }
    return false;
  }
}
