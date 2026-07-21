import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { TestBed } from '@angular/core/testing';
import { ImpersonationService } from './impersonation.service';
import { TokenAuthServiceProxy, AccountServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import {
  MockTokenAuthServiceProxy,
  MockAccountServiceProxy,
  MockAppUrlService,
  MockAppSessionService,
  MockAppAuthService,
  setupEafGlobals,
  MockLocalizationService,
  MockPermissionCheckerService,
  MockFeatureCheckerService,
  MockMessageService,
  MockNotifyService,
  MockSettingService,
  MockEafMultiTenancyService,
  MockAppUiCustomizationService,
} from '../../../test-helpers/mock-services';

describe('ImpersonationService', () => {
  let service: ImpersonationService;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      providers: [
        { provide: AppAuthService, useClass: MockAppAuthService },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        ImpersonationService,
        { provide: TokenAuthServiceProxy, useClass: MockTokenAuthServiceProxy },
        { provide: AccountServiceProxy, useClass: MockAccountServiceProxy },
        { provide: AppUrlService, useClass: MockAppUrlService },
        { provide: AppSessionService, useClass: MockAppSessionService },
      ],
    });
    service = TestBed.inject(ImpersonationService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });
});
