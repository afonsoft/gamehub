import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Theme4LayoutComponent } from './theme4-layout.component';
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
  MockLocalizationService, MockPermissionCheckerService, MockFeatureCheckerService,
  MockMessageService, MockNotifyService, MockSettingService, MockEafMultiTenancyService,
  MockAppSessionService, MockAppUiCustomizationService, MockAppUrlService, MockLayoutRefService,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../../../test-helpers/mock-services';

describe('Theme4LayoutComponent', () => {
  let component: Theme4LayoutComponent;
  let fixture: ComponentFixture<Theme4LayoutComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [Theme4LayoutComponent, MockLocalizePipe],
      providers: [
        { provide: LayoutRefService, useClass: MockLayoutRefService },
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
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
    fixture = TestBed.createComponent(Theme4LayoutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => { expect(component).toBeTruthy(); });
});
