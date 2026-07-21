import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { HangfireComponent } from './hangfire.component';
import { AppConsts } from '@shared/AppConsts';
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
  setupEafGlobals,
  MockLocalizePipe,
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
} from '../../../test-helpers/mock-services';

class MockDomSanitizer {
  bypassSecurityTrustResourceUrl(url: string): any {
    return url;
  }
}

describe('HangfireComponent', () => {
  let component: HangfireComponent;
  let fixture: ComponentFixture<HangfireComponent>;

  beforeEach(() => {
    setupEafGlobals();
    AppConsts.remoteServiceBaseUrl = 'http://localhost:5000';
    TestBed.configureTestingModule({
      declarations: [HangfireComponent, MockLocalizePipe],
      providers: [
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
        { provide: DomSanitizer, useClass: MockDomSanitizer },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(HangfireComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
