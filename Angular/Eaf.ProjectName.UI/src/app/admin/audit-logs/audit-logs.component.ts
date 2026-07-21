import { Component, Injector, ViewChild, ViewEncapsulation } from '@angular/core';
import { AuditLogDetailModalComponent } from '@app/admin/audit-logs/audit-log-detail-modal.component';
import { EntityChangeDetailModalComponent } from '@app/shared/common/entityHistory/entity-change-detail-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AuditLogListDto, AuditLogServiceProxy, EntityChangeListDto, NameValueDto } from '@shared/service-proxies/service-proxies';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as moment from 'moment';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { DataTableHelper } from '@shared/helpers/DataTableHelper';

@Component({
  standalone: false,
  templateUrl: './audit-logs.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class AuditLogsComponent extends AppComponentBase {
  @ViewChild('auditLogDetailModal', { static: true }) auditLogDetailModal: AuditLogDetailModalComponent;
  @ViewChild('entityChangeDetailModal', { static: true }) entityChangeDetailModal: EntityChangeDetailModalComponent;
  @ViewChild('dataTableAuditLogs', { static: true }) dataTableAuditLogs: Table;
  @ViewChild('dataTableEntityChanges', { static: true }) dataTableEntityChanges: Table;
  @ViewChild('paginatorAuditLogs', { static: true }) paginatorAuditLogs: Paginator;
  @ViewChild('paginatorEntityChanges', { static: true }) paginatorEntityChanges: Paginator;

  //Filters
  public dateRange: Date[] = [moment().startOf('day').toDate(), moment().endOf('day').toDate()];

  public usernameAuditLog: string;
  public usernameEntityChange: string;
  public serviceName: string;
  public methodName: string;
  public browserInfo: string;
  public hasException: boolean = undefined;
  public minExecutionDuration: number;
  public maxExecutionDuration: number;
  public entityTypeFullName: string;
  public objectTypes: NameValueDto[];

  dataTableHelperAuditLogs = new DataTableHelper();
  dataTableHelperEntityChanges = new DataTableHelper();

  constructor(
    injector: Injector,
    private readonly _auditLogService: AuditLogServiceProxy,
    private readonly _fileDownloadService: FileDownloadService,
  ) {
    super(injector);
  }

  showAuditLogDetails(record: AuditLogListDto): void {
    this.auditLogDetailModal.show(record);
  }

  showEntityChangeDetails(record: EntityChangeListDto): void {
    this.entityChangeDetailModal.show(record);
  }

  getAuditLogs(event?: LazyLoadEvent) {
    if (this.dataTableHelperAuditLogs.shouldResetPaging(event)) {
      this.paginatorAuditLogs.changePage(0);

      return;
    }

    this.dataTableHelperAuditLogs.showLoadingIndicator();

    this._auditLogService
      .getAuditLogs(
        this.browserInfo,
        moment(this.dateRange[1]),
        this.hasException,
        this.maxExecutionDuration,
        this.methodName,
        this.minExecutionDuration,
        this.serviceName,
        moment(this.dateRange[0]),
        this.usernameAuditLog,
        this.dataTableHelperAuditLogs.getSorting(this.dataTableAuditLogs),
        this.dataTableHelperAuditLogs.getMaxResultCount(this.paginatorAuditLogs, event),
        this.dataTableHelperAuditLogs.getSkipCount(this.paginatorAuditLogs, event),
      )
      .subscribe(result => {
        this.dataTableHelperAuditLogs.totalRecordsCount = result.totalCount;
        this.dataTableHelperAuditLogs.records = result.items;
        this.dataTableHelperAuditLogs.hideLoadingIndicator();
      });
  }

  getEntityChanges(event?: LazyLoadEvent) {
    this._auditLogService.getEntityHistoryObjectTypes().subscribe(result => {
      this.objectTypes = result;
    });

    if (this.dataTableHelperEntityChanges.shouldResetPaging(event)) {
      this.paginatorEntityChanges.changePage(0);

      return;
    }

    this.dataTableHelperEntityChanges.showLoadingIndicator();

    this._auditLogService
      .getEntityChanges(
        moment(this.dateRange[1]),
        this.entityTypeFullName,
        moment(this.dateRange[0]),
        this.usernameEntityChange,
        this.dataTableHelperEntityChanges.getSorting(this.dataTableEntityChanges),
        this.dataTableHelperEntityChanges.getMaxResultCount(this.paginatorEntityChanges, event),
        this.dataTableHelperEntityChanges.getSkipCount(this.paginatorEntityChanges, event),
      )
      .subscribe(result => {
        this.dataTableHelperEntityChanges.totalRecordsCount = result.totalCount;
        this.dataTableHelperEntityChanges.records = result.items;
        this.dataTableHelperEntityChanges.hideLoadingIndicator();
      });
  }

  exportToExcel(): void {
    this._auditLogService
      .getAuditLogsToExcel(
        this.browserInfo,
        moment(this.dateRange[1]),
        this.hasException,
        this.maxExecutionDuration,
        this.methodName,
        this.minExecutionDuration,
        this.serviceName,
        moment(this.dateRange[0]),
        this.usernameAuditLog,
        undefined,
        1,
        0,
      )
      .subscribe(result => {
        this._fileDownloadService.downloadTempFile(result);
      });

    this._auditLogService
      .getEntityChangesToExcel(
        moment(this.dateRange[1]),
        this.entityTypeFullName,
        moment(this.dateRange[0]),
        this.usernameEntityChange,
        undefined,
        1,
        0,
      )
      .subscribe(result => {
        this._fileDownloadService.downloadTempFile(result);
      });
  }

  truncateStringWithPostfix(text: string, length: number): string {
    return eaf.utils.truncateStringWithPostfix(text, length);
  }
}
