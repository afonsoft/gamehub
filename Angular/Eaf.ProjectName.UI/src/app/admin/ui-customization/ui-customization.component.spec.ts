import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { UiCustomizationComponent } from './ui-customization.component';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { UiCustomizationSettingsServiceProxy } from '@shared/service-proxies/service-proxies';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
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
  MockLocalizePipe,
} from '../../../test-helpers/mock-services';
import { of } from 'rxjs';

class MockUiCustomizationSettingsServiceProxy {
  getUiCustomizationSettings(): any {
    return of({ settings: [] });
  }
  updateUiManagementSettings(input: any): any {
    return of(undefined);
  }
  updateDefaultUiManagementSettings(input: any): any {
    return of(undefined);
  }
  useSystemDefaultSettings(): any {
    return of(undefined);
  }
}

describe('UiCustomizationComponent', () => {
  let component: UiCustomizationComponent;
  let fixture: ComponentFixture<UiCustomizationComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [UiCustomizationComponent, MockLocalizePipe],
      providers: [
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: UiCustomizationSettingsServiceProxy, useClass: MockUiCustomizationSettingsServiceProxy },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(UiCustomizationComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
