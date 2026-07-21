import { TestBed } from '@angular/core/testing';
import { AppNavigationService } from './app-navigation.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
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
  MockAppSessionService,
  MockAppUiCustomizationService,
  MockAppUrlService,
  setupEafGlobals,
} from '../../../../test-helpers/mock-services';

describe('AppNavigationService', () => {
  let service: AppNavigationService;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      providers: [
        AppNavigationService,
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
    });
    service = TestBed.inject(AppNavigationService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  it('should return menu', () => {
    const menu = service.getMenu();
    expect(menu).toBeDefined();
    expect(menu.items).toBeDefined();
  });

  it('should have at least one menu item', () => {
    const menu = service.getMenu();
    expect(menu.items.length).toBeGreaterThan(0);
  });
});
