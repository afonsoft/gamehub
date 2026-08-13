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

interface HasExceptionOption {
  label: string;
  value: boolean | null;
}

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

  // Audit log filters
  public dateRange: Date[] = [moment().startOf('day').toDate(), moment().endOf('day').toDate()];
  public usernameAuditLog: string;
  public serviceName: string;
  public methodName: string;
  public browserInfo: string;
  public hasException: boolean | null = null;
  public minExecutionDuration: number;
  public maxExecutionDuration: number;

  // Entity change filters
  public dateRangeEntityChanges: Date[] = [moment().startOf('day').toDate(), moment().endOf('day').toDate()];
  public usernameEntityChange: string;
  public entityTypeFullName: string;
  public objectTypes: NameValueDto[] = [];

  hasExceptionOptions: HasExceptionOption[] = [
    { label: 'All', value: null },
    { label: 'Error', value: true },
    { label: 'Success', value: false },
  ];

  activeTabIndex = 0;

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

  searchAuditLogs(): void {
    if (this.dataTableHelperAuditLogs.shouldResetPaging(undefined)) {
      this.paginatorAuditLogs.changePage(0);
      return;
    }
    this.getAuditLogs();
  }

  searchEntityChanges(): void {
    if (this.dataTableHelperEntityChanges.shouldResetPaging(undefined)) {
      this.paginatorEntityChanges.changePage(0);
      return;
    }
    this.getEntityChanges();
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
        this.dateRange && this.dateRange[1] ? moment(this.dateRange[1]) : undefined,
        this.hasException,
        this.maxExecutionDuration,
        this.methodName,
        this.minExecutionDuration,
        this.serviceName,
        this.dateRange && this.dateRange[0] ? moment(this.dateRange[0]) : undefined,
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
        this.dateRangeEntityChanges && this.dateRangeEntityChanges[1] ? moment(this.dateRangeEntityChanges[1]) : undefined,
        this.entityTypeFullName,
        this.dateRangeEntityChanges && this.dateRangeEntityChanges[0] ? moment(this.dateRangeEntityChanges[0]) : undefined,
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

  exportAuditLogsToExcel(): void {
    this._auditLogService
      .getAuditLogsToExcel(
        this.browserInfo,
        this.dateRange && this.dateRange[1] ? moment(this.dateRange[1]) : undefined,
        this.hasException,
        this.maxExecutionDuration,
        this.methodName,
        this.minExecutionDuration,
        this.serviceName,
        this.dateRange && this.dateRange[0] ? moment(this.dateRange[0]) : undefined,
        this.usernameAuditLog,
        undefined,
        1,
        0,
      )
      .subscribe(result => {
        this._fileDownloadService.downloadTempFile(result);
      });
  }

  exportEntityChangesToExcel(): void {
    this._auditLogService
      .getEntityChangesToExcel(
        this.dateRangeEntityChanges && this.dateRangeEntityChanges[1] ? moment(this.dateRangeEntityChanges[1]) : undefined,
        this.entityTypeFullName,
        this.dateRangeEntityChanges && this.dateRangeEntityChanges[0] ? moment(this.dateRangeEntityChanges[0]) : undefined,
        this.usernameEntityChange,
        undefined,
        1,
        0,
      )
      .subscribe(result => {
        this._fileDownloadService.downloadTempFile(result);
      });
  }

  onTabChange(index: number): void {
    this.activeTabIndex = index;
    if (index === 0) {
      this.searchAuditLogs();
    } else {
      this.searchEntityChanges();
    }
  }

  resetAuditLogFilters(): void {
    this.dateRange = [moment().startOf('day').toDate(), moment().endOf('day').toDate()];
    this.usernameAuditLog = undefined;
    this.serviceName = undefined;
    this.methodName = undefined;
    this.browserInfo = undefined;
    this.hasException = null;
    this.minExecutionDuration = undefined;
    this.maxExecutionDuration = undefined;
    this.searchAuditLogs();
  }

  resetEntityChangeFilters(): void {
    this.dateRangeEntityChanges = [moment().startOf('day').toDate(), moment().endOf('day').toDate()];
    this.usernameEntityChange = undefined;
    this.entityTypeFullName = undefined;
    this.searchEntityChanges();
  }

  truncateStringWithPostfix(text: string, length: number): string {
    return eaf.utils.truncateStringWithPostfix(text, length);
  }
}
