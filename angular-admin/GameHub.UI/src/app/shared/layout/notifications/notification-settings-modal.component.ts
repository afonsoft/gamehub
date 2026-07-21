import { Component, Injector, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  GetNotificationSettingsOutput,
  NotificationServiceProxy,
  NotificationSubscriptionDto,
  UpdateNotificationSettingsInput,
} from '@shared/service-proxies/service-proxies';

import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'notificationSettingsModal',
  templateUrl: './notification-settings-modal.component.html',
})
export class NotificationSettingsModalComponent extends AppComponentBase {
  @ViewChild('modal', { static: true }) modal: ModalDirective;

  saving = false;

  settings: GetNotificationSettingsOutput;

  constructor(
    injector: Injector,
    private readonly _notificationService: NotificationServiceProxy,
  ) {
    super(injector);
  }

  show() {
    this.getSettings(() => {
      this.modal.show();
    });
  }

  save(): void {
    const input = new UpdateNotificationSettingsInput();
    input.receiveNotifications = this.settings.receiveNotifications;
    input.notifications = this.settings.notifications?.map(n => {
      const subscription = new NotificationSubscriptionDto();
      subscription.name = n.name;
      subscription.isSubscribed = n.isSubscribed;
      return subscription;
    });

    this.saving = true;
    this._notificationService
      .updateNotificationSettings(input)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.close();
      });
  }

  close(): void {
    this.modal.hide();
  }

  private getSettings(callback: () => void) {
    this._notificationService.getNotificationSettings().subscribe((result: GetNotificationSettingsOutput) => {
      this.settings = result;
      callback();
    });
  }
}
