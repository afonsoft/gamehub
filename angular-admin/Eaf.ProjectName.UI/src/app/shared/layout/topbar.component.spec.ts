import { CookieService } from 'ngx-cookie-service';
import { SessionServiceProxy } from '@shared/service-proxies/service-proxies';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { TopBarComponent } from './topbar.component';
import { ImpersonationService } from '@app/admin/users/impersonation.service';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { EafSessionService } from '@eaf/session/eaf-session.service';
import { StorageService } from '@eaf/utils/storage.service';
import { ProfileServiceProxy } from '@shared/service-proxies/service-proxies';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import {
  MockLocalizationService,
  MockPermissionCheckerService,
  MockFeatureCheckerService,
  MockMessageService,
  MockNotifyService,
  MockSettingService,
  MockEafMultiTenancyService,
  MockEafSessionService,
  MockAppSessionService,
  MockAppUiCustomizationService,
  MockAppUrlService,
  MockProfileServiceProxy,
  MockImpersonationService,
  MockAppAuthService,
  MockLayoutRefService,
  MockSessionServiceProxy,
  MockCookieService,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../test-helpers/mock-services';

class MockStorageService {
  setCookieValue(key: string, value: string, date: Date, path: string): void {}
}

describe('TopBarComponent', () => {
  let component: TopBarComponent;
  let fixture: ComponentFixture<TopBarComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [TopBarComponent, MockLocalizePipe],
      providers: [
        { provide: CookieService, useClass: MockCookieService },
        { provide: SessionServiceProxy, useClass: MockSessionServiceProxy },
        { provide: LayoutRefService, useClass: MockLayoutRefService },
        { provide: EafSessionService, useClass: MockEafSessionService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: ProfileServiceProxy, useClass: MockProfileServiceProxy },
        { provide: AppAuthService, useClass: MockAppAuthService },
        { provide: ImpersonationService, useClass: MockImpersonationService },
        { provide: StorageService, useClass: MockStorageService },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(TopBarComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have unread chat message count initialized to 0', () => {
    expect(component.unreadChatMessageCount).toBe(0);
  });

  it('should have chatConnected set to false initially', () => {
    expect(component.chatConnected).toBeFalsy();
  });

  it('should logout', () => {
    const authService = TestBed.inject(AppAuthService);
    spyOn(authService, 'logout');
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });

  it('should show login attempts', () => {
    spyOn((window as any).eaf.event, 'trigger');
    component.showLoginAttempts();
    expect((window as any).eaf.event.trigger).toHaveBeenCalledWith('app.show.loginAttemptsModal');
  });

  it('should change password', () => {
    spyOn((window as any).eaf.event, 'trigger');
    component.changePassword();
    expect((window as any).eaf.event.trigger).toHaveBeenCalledWith('app.show.changePasswordModal');
  });

  it('should change profile picture', () => {
    spyOn((window as any).eaf.event, 'trigger');
    component.changeProfilePicture();
    expect((window as any).eaf.event.trigger).toHaveBeenCalledWith('app.show.changeProfilePictureModal');
  });

  it('should change my settings', () => {
    spyOn((window as any).eaf.event, 'trigger');
    component.changeMySettings();
    expect((window as any).eaf.event.trigger).toHaveBeenCalledWith('app.show.mySettingsModal');
  });
});
