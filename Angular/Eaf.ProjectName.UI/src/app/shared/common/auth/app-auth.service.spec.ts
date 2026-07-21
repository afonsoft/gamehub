import { SessionServiceProxy } from '@shared/service-proxies/service-proxies';
import { CookieService } from 'ngx-cookie-service';
import { TestBed } from '@angular/core/testing';
import { AppAuthService } from './app-auth.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import {
  MockLocalizationService, MockPermissionCheckerService, MockFeatureCheckerService,
  MockMessageService, MockNotifyService, MockSettingService, MockEafMultiTenancyService,
  MockAppSessionService, MockAppUiCustomizationService, MockAppUrlService, MockCookieService,
  MockSessionServiceProxy,
  setupEafGlobals,
} from '../../../../test-helpers/mock-services';

describe('AppAuthService', () => {
  let service: AppAuthService;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      providers: [
        { provide: SessionServiceProxy, useClass: MockSessionServiceProxy },
        { provide: CookieService, useClass: MockCookieService },
        AppAuthService,
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
    });
    service = TestBed.inject(AppAuthService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  it('should have logout method', () => {
    expect(service.logout).toBeDefined();
  });
});
