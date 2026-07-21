import { FormsModule } from '@angular/forms';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { EmailActivationComponent } from './email-activation.component';
import { AccountServiceProxy } from '@shared/service-proxies/service-proxies';
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
  MockAccountServiceProxy,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../test-helpers/mock-services';

describe('EmailActivationComponent', () => {
  let component: EmailActivationComponent;
  let fixture: ComponentFixture<EmailActivationComponent>;
  const mockRouter = { navigate: jasmine.createSpy('navigate') };

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [EmailActivationComponent, MockLocalizePipe],
      imports: [FormsModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AccountServiceProxy, useClass: MockAccountServiceProxy },
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

    fixture = TestBed.createComponent(EmailActivationComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
