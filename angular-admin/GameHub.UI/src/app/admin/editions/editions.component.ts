import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EditionServiceProxy } from '@shared/service-proxies/edition.service-proxy';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './editions.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class EditionsComponent extends AppComponentBase implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;

  filters: { filterText: string } = { filterText: '' };

  constructor(
    injector: Injector,
    private readonly _editionService: EditionServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.filters.filterText = '';
  }

  getEditions(event?: LazyLoadEvent): void {
    if (this.dataTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.dataTableHelper.showLoadingIndicator();

    this._editionService
      .getEditions(
        this.filters.filterText,
        this.dataTableHelper.getSorting(this.dataTable),
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
        this.dataTableHelper.getSkipCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
      });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }
}
