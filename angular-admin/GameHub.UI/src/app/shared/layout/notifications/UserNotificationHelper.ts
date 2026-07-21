import { Injectable, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EntityDtoOfGuid, NotificationServiceProxy } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import * as Push from 'push.js'; // if using ES6
import { NotificationSettingsModalComponent } from './notification-settings-modal.component';
import { AppConsts } from '@shared/AppConsts';

export interface IFormattedUserNotification {
  userNotificationId: string;
  text: string;
  time: string;
  creationTime: Date;
  icon: string;
  state: string;
  data: any;
  url: string;
  isUnread: boolean;
}

@Injectable()
export class UserNotificationHelper extends AppComponentBase {
  settingsModal: NotificationSettingsModalComponent;

  constructor(
    injector: Injector,
    private readonly _notificationService: NotificationServiceProxy,
  ) {
    super(injector);
  }

  getUrl(userNotification: eaf.notifications.IUserNotification): string {
    switch (userNotification.notification.notificationName) {
      case 'App.NewUserRegistered':
        return '/app/admin/users?filterText=' + userNotification.notification.data.properties.emailAddress;
      case 'App.NewTenantRegistered':
        return '/app/admin/tenants?filterText=' + userNotification.notification.data.properties.tenancyName;
      case 'App.GdprDataPrepared':
        return (
          AppConsts.remoteServiceBaseUrl +
          '/File/DownloadBinaryFile?id=' +
          userNotification.notification.data.properties.binaryObjectId +
          '&contentType=application/zip&fileName=collectedData.zip'
        );
      //Add your custom notification names to navigate to a URL when user clicks to a notification.
    }

    //No url for this notification
    return '';
  }

  /* PUBLIC functions ******************************************/

  getUiIconBySeverity(severity: eaf.notifications.severity): string {
    switch (severity) {
      case eaf.notifications.severity.SUCCESS:
        return 'fa fa-check';
      case eaf.notifications.severity.WARN:
        return 'fa fa-exclamation-triangle';
      case eaf.notifications.severity.ERROR:
        return 'fa fa-bolt';
      case eaf.notifications.severity.FATAL:
        return 'fa fa-bomb';
      case eaf.notifications.severity.INFO:
      default:
        return 'fa fa-info';
    }
  }

  format(userNotification: eaf.notifications.IUserNotification, truncateText?: boolean): IFormattedUserNotification {
    const formatted: IFormattedUserNotification = {
      userNotificationId: userNotification.id,
      text: eaf.notifications.getFormattedMessageFromUserNotification(userNotification),
      time: moment(userNotification.notification.creationTime).format('YYYY-MM-DD HH:mm:ss'),
      creationTime: userNotification.notification.creationTime,
      icon: this.getUiIconBySeverity(userNotification.notification.severity),
      state: eaf.notifications.getUserNotificationStateAsString(userNotification.state),
      data: userNotification.notification.data,
      url: this.getUrl(userNotification),
      isUnread: userNotification.state === eaf.notifications.userNotificationState.UNREAD,
    };

    if (truncateText || truncateText === undefined) {
      formatted.text = eaf.utils.truncateStringWithPostfix(formatted.text, 40);
    }

    return formatted;
  }

  show(userNotification: eaf.notifications.IUserNotification): void {
    //Application notification
    eaf.notifications.showUiNotifyForUserNotification(userNotification, {
      onclick: () => {
        //Take action when user clicks to live toastr notification
        const url = this.getUrl(userNotification);
        if (url) {
          location.href = url;
        }
      },
    });

    //Desktop notification
    Push.default.create('ProjectName', {
      body: this.format(userNotification).text,
      icon: eaf.appPath + 'assets/common/images/eaf/eaf.png',
      timeout: 8000,
      onClick: function () {
        window.focus();
        this.close();
      },
    });
  }

  setAllAsRead(callback?: () => void): void {
    this._notificationService.setAllNotificationsAsRead().subscribe(() => {
      eaf.event.trigger('app.notifications.refresh');
      if (callback) {
        callback();
      }
    });
  }

  setAsRead(userNotificationId: string, callback?: (userNotificationId: string) => void): void {
    const input = new EntityDtoOfGuid();
    input.id = userNotificationId;
    this._notificationService.setNotificationAsRead(input).subscribe(() => {
      eaf.event.trigger('app.notifications.read', userNotificationId);
      if (callback) {
        callback(userNotificationId);
      }
    });
  }

  openSettingsModal(): void {
    this.settingsModal.show();
  }
}
