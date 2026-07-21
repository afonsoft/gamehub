import { AfterViewInit, Component, Injector, OnInit, ViewChild } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { LoginAttemptsModalComponent } from '@app/shared/layout/login-attempts-modal.component';
import { NotificationSettingsModalComponent } from '@app/shared/layout/notifications/notification-settings-modal.component';
import { UserNotificationHelper } from '@app/shared/layout/notifications/UserNotificationHelper';
import { ChangePasswordModalComponent } from '@app/shared/layout/profile/change-password-modal.component';
import { ChangeProfilePictureModalComponent } from '@app/shared/layout/profile/change-profile-picture-modal.component';
import { MySettingsModalComponent } from '@app/shared/layout/profile/my-settings-modal.component';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { AppAuthenticationService } from '@shared/common/auth/app-authentication-service';
import { NameValueDto } from '@shared/service-proxies/service-proxies';
import { ChatSignalrService } from 'app/shared/layout/chat/chat-signalr.service';
import { filter } from 'rxjs/operators';
import { AppComponentBase } from 'shared/common/app-component-base';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';

import { CommonLookupModalComponent } from './shared/common/lookup/common-lookup-modal.component';
import { ChatBarComponent } from './shared/layout/chat/chat-bar.component';

declare let gtag: Function;

@Component({
  standalone: false,
  templateUrl: './app.component.html',
})
export class AppComponent extends AppComponentBase implements OnInit, AfterViewInit {
  theme: string;
  chatConnected = false;

  @ViewChild('loginAttemptsModal', { static: true }) loginAttemptsModal: LoginAttemptsModalComponent;
  @ViewChild('changePasswordModal', { static: true }) changePasswordModal: ChangePasswordModalComponent;
  @ViewChild('changeProfilePictureModal', { static: true }) changeProfilePictureModal: ChangeProfilePictureModalComponent;
  @ViewChild('mySettingsModal', { static: true }) mySettingsModal: MySettingsModalComponent;
  @ViewChild('notificationSettingsModal', { static: true }) notificationSettingsModal: NotificationSettingsModalComponent;
  @ViewChild('chatBarComponent', { static: true }) chatBarComponent: ChatBarComponent;
  @ViewChild('userLookupModal', { static: true }) userLookupModal: CommonLookupModalComponent;

  public constructor(
    injector: Injector,
    private readonly _chatSignalrService: ChatSignalrService,
    private readonly _userNotificationHelper: UserNotificationHelper,
    private readonly _appAuthenticationService: AppAuthenticationService,
    private readonly _storageService: StorageService,
    private readonly router: Router,
      ) {
    super(injector);
  }

  ngOnInit(): void {
    this._userNotificationHelper.settingsModal = this.notificationSettingsModal;
    this.theme = eaf.setting.get('App.UiManagement.Theme').toLocaleLowerCase();

    this.registerModalOpenEvents();

    if (this.appSession.application) {
      SignalRHelper.init(this._storageService);
      SignalRHelper.initSignalR(() => {
        this._chatSignalrService.init();
      });
    }
    this.setUpAnalytics();
    this.setUpTagManager();
  }

  ngAfterViewInit(): void {
    eaf.signalr.autoConnect = false;
    this._appAuthenticationService.init();
  }

  setUpAnalytics(): void {
    if (AppConsts.googleAnalytics !== undefined) {
      this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((event: NavigationEnd) => {
        gtag('config', AppConsts.googleAnalytics, { page_path: event.urlAfterRedirects });
        gtag('set', 'page', event.urlAfterRedirects);
        gtag('send', 'pageview');
      });
    }
  }

  setUpTagManager(): void {
    if (AppConsts.googleTagManager !== undefined && gtag !== undefined) {
      this.router.events.forEach(item => {
        if (item instanceof NavigationEnd) {
          gtag('config', AppConsts.googleTagManager, {
            page_path: item.urlAfterRedirects
          });
        }
      });
    }
  }

  registerModalOpenEvents(): void {
    eaf.event.on('app.show.loginAttemptsModal', () => {
      this.loginAttemptsModal.show();
    });

    eaf.event.on('app.show.changePasswordModal', () => {
      this.changePasswordModal.show();
    });

    eaf.event.on('app.show.changeProfilePictureModal', () => {
      this.changeProfilePictureModal.show();
    });

    eaf.event.on('app.show.mySettingsModal', () => {
      this.mySettingsModal.show();
    });
    eaf.event.on('app.chat.connected', () => {
      this.chatConnected = true;
    });
  }

  onMySettingsModalSaved(): void {
    eaf.event.trigger('app.onMySettingsModalSaved');
  }

  addFriendSelected(item: NameValueDto): void {
    eaf.event.trigger('app.chat.addFriendSelected', item);
    if (this.chatBarComponent !== undefined) this.chatBarComponent.addFriendSelected(item);
  }
}
