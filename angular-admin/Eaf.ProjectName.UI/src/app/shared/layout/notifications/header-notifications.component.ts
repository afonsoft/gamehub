import { Component, Injector, OnInit, ViewEncapsulation, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { NotificationServiceProxy, UserNotification, UserNotificationState } from '@shared/service-proxies/service-proxies';
import { IFormattedUserNotification, UserNotificationHelper } from './UserNotificationHelper';
import * as _ from 'lodash';
import * as Push from 'push.js'; // if using ES6
import { environment } from 'environments/environment';

@Component({
  standalone: false,
  templateUrl: './header-notifications.component.html',
  selector: '[headerNotifications]',
  encapsulation: ViewEncapsulation.None,
})
export class HeaderNotificationsComponent extends AppComponentBase implements OnInit {
  notifications: IFormattedUserNotification[] = [];
  unreadUserNotification: UserNotification[] = [];
  unreadNotificationCount = 0;

  constructor(
    injector: Injector,
    private readonly _notificationService: NotificationServiceProxy,
    private readonly _userNotificationHelper: UserNotificationHelper,
    public _zone: NgZone,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.registerToEvents();
    this.loadNotifications();
  }

  loadNotifications(): void {

    this._notificationService.getUserNotifications(undefined, 3, 0).subscribe(result => {
      this.unreadNotificationCount = result.unreadCount;
      this.notifications = [];

      _.forEach(result.items, (item: UserNotification) => {
        this.notifications.push(this._userNotificationHelper.format(<any>item));
        if (item.state == UserNotificationState.Unread) {
          this.unreadUserNotification.push(item);
        }
      });

      _.forEach(this.unreadUserNotification, (item: UserNotification) => {
        this._zone.run(() => {
          this._userNotificationHelper.show(<any>item);
        });
      });
    });
  }

  registerToEvents() {

    if (environment.production) {
      Push.default.config({ serviceWorker: './ngsw-worker.js' });
    }
    const onNotificationReceived = (userNotification) => {
      this._userNotificationHelper.show(userNotification);
      this.loadNotifications();
    }

    eaf.event.on('eaf.notifications.received', userNotification => {
      this._zone.run(() => {
        onNotificationReceived(userNotification);
      });
    });

    eaf.event.on('app.notifications.refresh', () => {
      this._zone.run(() => {
        this.onNotificationsRefresh();
      });
    });

    eaf.event.on('app.notifications.read', userNotificationId => {
      this._zone.run(() => {
        this.onNotificationsRead(userNotificationId);
      });
    });
  }

  private onNotificationsRefresh(): void {
    this.loadNotifications();
  }

  private onNotificationsRead(userNotificationId): void {
    for (const notification of this.notifications) {
      if (notification.userNotificationId === userNotificationId) {
        notification.state = 'READ';
        notification.isUnread = false;
      }
    }

    this.unreadNotificationCount -= 1;
  }

  setAllNotificationsAsRead(): void {
    this._userNotificationHelper.setAllAsRead();
  }

  openNotificationSettingsModal(): void {
    this._userNotificationHelper.openSettingsModal();
  }

  setNotificationAsRead(userNotification: IFormattedUserNotification): void {
    if (userNotification.state !== 'READ') {
      this._userNotificationHelper.setAsRead(userNotification.userNotificationId);
    }
  }

  gotoUrl(url): void {
    if (url) {
      location.href = url;
    }
  }
}
