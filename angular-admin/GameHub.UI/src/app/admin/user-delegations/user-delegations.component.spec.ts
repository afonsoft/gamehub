import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap/modal';
import { UserDelegationsComponent } from './user-delegations.component';
import { UserDelegationServiceProxy } from '@shared/service-proxies/user-delegation.service-proxy';
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';
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
  MockUserDelegationServiceProxy,
  MockUserServiceProxy,
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
  MockLocalizePipe,
  setupEafGlobals,
} from '../../../test-helpers/mock-services';

describe('UserDelegationsComponent', () => {
  let component: UserDelegationsComponent;
  let fixture: ComponentFixture<UserDelegationsComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [FormsModule, ModalModule.forRoot()],
      declarations: [UserDelegationsComponent, MockLocalizePipe],
      providers: [
        { provide: UserDelegationServiceProxy, useClass: MockUserDelegationServiceProxy },
        { provide: UserServiceProxy, useClass: MockUserServiceProxy },
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

    fixture = TestBed.createComponent(UserDelegationsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default to myDelegations tab', () => {
    expect(component.activeTab).toBe('myDelegations');
  });

  it('should not save without target user', () => {
    component.newDelegation = { targetUserId: undefined, startTime: '2026-01-01T00:00', endTime: '2026-01-02T00:00', description: '' } as any;
    component.save();
    expect(component.saving).toBe(false);
  });

  it('should validate start before end', () => {
    component.newDelegation = { targetUserId: 1, startTime: '2026-01-02T00:00', endTime: '2026-01-01T00:00', description: '' } as any;
    component.save();
    expect(component.saving).toBe(false);
  });
});
