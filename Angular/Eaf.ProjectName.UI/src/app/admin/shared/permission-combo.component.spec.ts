import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { PermissionComboComponent } from './permission-combo.component';
import { PermissionServiceProxy } from '@shared/service-proxies/service-proxies';
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
import { of } from 'rxjs';
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

class MockPermissionServiceProxy {
  getAllPermissions(): any {
    return of({ items: [] });
  }
}

describe('PermissionComboComponent', () => {
  let component: PermissionComboComponent;
  let fixture: ComponentFixture<PermissionComboComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [PermissionComboComponent, MockLocalizePipe],
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
        { provide: PermissionServiceProxy, useClass: MockPermissionServiceProxy },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(PermissionComboComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have permissions array initialized', () => {
    expect(component.permissions).toBeDefined();
  });
});
