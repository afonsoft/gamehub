import { ChangeDetectorRef, Component, Injector, ViewChild, ViewEncapsulation } from '@angular/core';
import { AppUserNotificationState } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { NotificationServiceProxy, UserNotification } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { IFormattedUserNotification, UserNotificationHelper } from './UserNotificationHelper';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './notifications.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class NotificationsComponent extends AppComponentBase {
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;

  readStateFilter = 'ALL';
  loading = false;

  constructor(
    injector: Injector,
    private readonly _notificationService: NotificationServiceProxy,
    private readonly _userNotificationHelper: UserNotificationHelper,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {
    super(injector);
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  setAsRead(record: any): void {
    this.setNotificationAsRead(record, () => {
      this.reloadPage();
    });
  }

  isRead(record: any): boolean {
    return record.formattedNotification.state === 'READ';
  }

  fromNow(date: moment.Moment): string {
    return moment(date).fromNow();
  }

  formatRecord(record: any): IFormattedUserNotification {
    return this._userNotificationHelper.format(record, false);
  }

  formatNotification(record: any): string {
    const formattedRecord = this.formatRecord(record);
    return eaf.utils.truncateStringWithPostfix(formattedRecord.text, 120);
  }

  formatNotifications(records: any[]): any[] {
    const formattedRecords = [];
    for (const record of records) {
      record.formattedNotification = this.formatRecord(record);
      formattedRecords.push(record);
    }
    return formattedRecords;
  }

  truncateString(text: any, length: number): string {
    return eaf.utils.truncateStringWithPostfix(text, length);
  }

  getNotifications(event?: LazyLoadEvent): void {
    if (this.dataTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);

      return;
    }

    this.dataTableHelper.showLoadingIndicator();

    this._notificationService
      .getUserNotifications(
        this.readStateFilter === 'ALL' ? undefined : AppUserNotificationState.Unread,
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
        this.dataTableHelper.getSkipCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = this.formatNotifications(result.items);
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  setAllNotificationsAsRead(): void {
    this._userNotificationHelper.setAllAsRead(() => {
      this.getNotifications();
    });
  }

  openNotificationSettingsModal(): void {
    this._userNotificationHelper.openSettingsModal();
  }

  setNotificationAsRead(userNotification: UserNotification, callback: () => void): void {
    this._userNotificationHelper.setAsRead(userNotification.id, () => {
      if (callback) {
        callback();
      }
    });
  }

  deleteNotification(userNotification: UserNotification): void {
    this.message.confirm(this.l('NotificationDeleteWarningMessage'), this.l('AreYouSure'), isConfirmed => {
      if (isConfirmed) {
        this._notificationService.deleteNotification(userNotification.id).subscribe(() => {
          this.reloadPage();
          this.notify.success(this.l('SuccessfullyDeleted'));
        });
      }
    });
  }

  public getRowClass(formattedRecord: IFormattedUserNotification): string {
    return formattedRecord.state === 'READ' ? 'notification-read' : '';
  }

  getNotificationTextBySeverity(severity: eaf.notifications.severity): string {
    switch (severity) {
      case eaf.notifications.severity.SUCCESS:
        return this.l('Success');
      case eaf.notifications.severity.WARN:
        return this.l('Warning');
      case eaf.notifications.severity.ERROR:
        return this.l('Error');
      case eaf.notifications.severity.FATAL:
        return this.l('Fatal');
      case eaf.notifications.severity.INFO:
      default:
        return this.l('Info');
    }
  }
}
