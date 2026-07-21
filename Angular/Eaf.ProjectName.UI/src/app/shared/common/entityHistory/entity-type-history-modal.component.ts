import { Component, Injector, ViewChild } from '@angular/core';
import { EntityChangeDetailModalComponent } from './entity-change-detail-modal.component';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AuditLogServiceProxy, EntityChangeListDto } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';

import { finalize } from 'rxjs/operators';

export interface IEntityTypeHistoryModalOptions {
  entityTypeFullName: string;
  entityTypeDescription: string;
  entityId: string;
}

@Component({
  standalone: false,
  selector: 'entityTypeHistoryModal',
  templateUrl: './entity-type-history-modal.component.html',
})
export class EntityTypeHistoryModalComponent extends AppComponentBase {
  @ViewChild('entityChangeDetailModal', { static: true }) entityChangeDetailModal: EntityChangeDetailModalComponent;

  @ViewChild('modal', { static: true }) modal: ModalDirective;
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;

  options: IEntityTypeHistoryModalOptions;
  isShown = false;
  isInitialized = false;
  filterText = '';
  tenantId?: number;
  entityHistoryEnabled: false;

  constructor(
    injector: Injector,
    private readonly _auditLogService: AuditLogServiceProxy,
  ) {
    super(injector);
  }

  show(options: IEntityTypeHistoryModalOptions): void {
    this.options = options;
    this.modal.show();
  }

  refreshTable(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  close(): void {
    this.modal.hide();
  }

  shown(): void {
    this.isShown = true;
    this.getRecordsIfNeeds(null);
  }

  getRecordsIfNeeds(event?: LazyLoadEvent): void {
    if (!this.isShown) {
      return;
    }

    this.getRecords(event);
    this.isInitialized = true;
  }

  getRecords(event?: LazyLoadEvent): void {
    this.dataTableHelper.showLoadingIndicator();

    this._auditLogService
      .getEntityTypeChanges(
        this.options.entityTypeFullName,
        this.options.entityId,
        this.dataTableHelper.getSorting(this.dataTable),
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
        this.dataTableHelper.getSkipCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  showEntityChangeDetails(record: EntityChangeListDto): void {
    this.entityChangeDetailModal.show(record);
  }
}
