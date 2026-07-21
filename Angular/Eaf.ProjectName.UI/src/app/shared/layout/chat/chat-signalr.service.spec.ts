import { ChatServiceProxy } from '@shared/service-proxies/service-proxies';
import { TestBed } from '@angular/core/testing';
import { ChatSignalrService } from './chat-signalr.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import {
  MockChatServiceProxy,
  setupEafGlobals,
  MockLocalizationService,
  MockPermissionCheckerService,
  MockFeatureCheckerService,
  MockMessageService,
  MockNotifyService,
  MockSettingService,
  MockEafMultiTenancyService,
  MockAppSessionService,
  MockAppUiCustomizationService,
  MockAppUrlService,
} from '../../../../test-helpers/mock-services';

describe('ChatSignalrService', () => {
  let service: ChatSignalrService;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      providers: [
        { provide: ChatServiceProxy, useClass: MockChatServiceProxy },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },ChatSignalrService],
    });
    service = TestBed.inject(ChatSignalrService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  it('should have isChatConnected set to false initially', () => {
    expect(service.isChatConnected).toBeFalsy();
  });
});
