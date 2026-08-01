import * as ngCommon from '@angular/common';
import { A11yModule } from '@angular/cdk/a11y';
import { HttpClientJsonpModule, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ServiceWorkerModule, SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter } from 'rxjs/operators';
import { ChatSignalrService } from '@app/shared/layout/chat/chat-signalr.service';
import { LoginAttemptsModalComponent } from '@app/shared/layout/login-attempts-modal.component';
import { ChangePasswordModalComponent } from '@app/shared/layout/profile/change-password-modal.component';
import { ChangeProfilePictureModalComponent } from '@app/shared/layout/profile/change-profile-picture-modal.component';
import { MySettingsModalComponent } from '@app/shared/layout/profile/my-settings-modal.component';
import { EafModule } from '@eaf/eaf.module';
import { CoreModule } from '@metronic/app/core/core.module';
import { LayoutConfigService } from '@metronic/app/core/services/layout-config.service';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
import { UtilsService } from '@metronic/app/core/services/utils.service';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { UtilsModule } from '@shared/utils/utils.module';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { PopoverModule } from 'ngx-bootstrap/popover';
import { NgxCaptchaModule } from 'ngx-captcha';
import { CookieService } from 'ngx-cookie-service';
import { NgScrollbarModule } from 'ngx-scrollbar';
import { NgxFileDropModule } from 'ngx-file-drop';
import { FileUploadModule as PrimeNgFileUploadModule } from 'primeng/fileupload';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressBarModule } from 'primeng/progressbar';
import { TableModule } from 'primeng/table';
import { AppConsts } from 'shared/AppConsts';

import { environment } from '../environments/environment';
import { ImpersonationService } from './admin/users/impersonation.service';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AppCommonModule } from './shared/common/app-common.module';
import { ChatBarComponent } from './shared/layout/chat/chat-bar.component';
import { ChatFriendListItemComponent } from './shared/layout/chat/chat-friend-list-item.component';
import { ChatMessageComponent } from './shared/layout/chat/chat-message.component';
import { AdmBarComponent } from './shared/layout/nav/adm-bar.component';
import { SideBarMenuComponent } from './shared/layout/nav/side-bar-menu.component';
import { TopBarMenuComponent } from './shared/layout/nav/top-bar-menu.component';
import { HeaderNotificationsComponent } from './shared/layout/notifications/header-notifications.component';
import { NotificationSettingsModalComponent } from './shared/layout/notifications/notification-settings-modal.component';
import { NotificationsComponent } from './shared/layout/notifications/notifications.component';
import { UserNotificationHelper } from './shared/layout/notifications/UserNotificationHelper';
import { DefaultBrandComponent } from './shared/layout/themes/default/default-brand.component';
import { DefaultLayoutComponent } from './shared/layout/themes/default/default-layout.component';
import { Theme2BrandComponent } from './shared/layout/themes/theme2/theme2-brand.component';
import { Theme2LayoutComponent } from './shared/layout/themes/theme2/theme2-layout.component';
import { Theme3BrandComponent } from './shared/layout/themes/theme3/theme3-brand.component';
import { Theme3LayoutComponent } from './shared/layout/themes/theme3/theme3-layout.component';
import { Theme4BrandComponent } from './shared/layout/themes/theme4/theme4-brand.component';
import { Theme4LayoutComponent } from './shared/layout/themes/theme4/theme4-layout.component';
import { TitleBarComponent } from './shared/layout/titlebar.component';
import { TopBarComponent } from './shared/layout/topbar.component';

// Metronic

export const googleTagManager = () => AppConsts.googleTagManager;

@NgModule({
  declarations: [
    AppComponent,
    DefaultLayoutComponent,
    Theme4LayoutComponent,
    Theme2LayoutComponent,
    Theme3LayoutComponent,
    HeaderNotificationsComponent,
    SideBarMenuComponent,
    AdmBarComponent,
    TopBarMenuComponent,
    LoginAttemptsModalComponent,
    ChangePasswordModalComponent,
    ChangeProfilePictureModalComponent,
    MySettingsModalComponent,
    NotificationsComponent,
    NotificationSettingsModalComponent,
    TopBarComponent,
    TitleBarComponent,
    DefaultBrandComponent,
    Theme4BrandComponent,
    Theme2BrandComponent,
    Theme3BrandComponent,
    ChatBarComponent,
    ChatFriendListItemComponent,
    ChatMessageComponent,
  ],
  imports: [
    ngCommon.CommonModule,
    A11yModule,
    FormsModule,
    HttpClientModule,
    HttpClientJsonpModule,
    ModalModule.forRoot(),
    TooltipModule.forRoot(),
    TabsModule.forRoot(),
    BsDropdownModule.forRoot(),
    PopoverModule.forRoot(),
    NgxFileDropModule,
    EafModule,
    AppRoutingModule,
    UtilsModule,
    AppCommonModule.forRoot(),
    ServiceProxyModule,
    TableModule,
    PaginatorModule,
    PrimeNgFileUploadModule,
    ProgressBarModule,
    NgScrollbarModule,
    CoreModule,
    NgxChartsModule,
    NgxCaptchaModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production }),
  ],
  providers: [
    ImpersonationService,
    UserNotificationHelper,
    ChatSignalrService,
    LayoutConfigService,
    UtilsService,
    LayoutRefService,
    CookieService,
    { provide: 'googleTagManagerId', useValue: googleTagManager() },
  ],
})
export class AppModule {
  constructor(public updates: SwUpdate) {
    if (updates.isEnabled) {
      updates.versionUpdates.pipe(
        filter((event): event is VersionReadyEvent => event.type === 'VERSION_READY')
      ).subscribe(event => {
        (window as any).eaf.log.info('current version is ' + event.currentVersion);
        (window as any).eaf.log.info('available version is ' + event.latestVersion);
        updates.activateUpdate().then(() => this.updateApp());
      });
    }
  }

  updateApp() {
    window.location.reload();
    (window as any).eaf.log.info('The app is updating right now');
  }
}
