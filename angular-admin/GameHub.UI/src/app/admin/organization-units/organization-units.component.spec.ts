import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap/modal';
import { OrganizationUnitsComponent } from './organization-units.component';
import { OrganizationUnitServiceProxy, RoleServiceProxy, UserServiceProxy } from '@shared/service-proxies/service-proxies';
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
  MockOrganizationUnitServiceProxy,
  MockUserServiceProxy,
  MockRoleServiceProxy,
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

describe('OrganizationUnitsComponent', () => {
  let component: OrganizationUnitsComponent;
  let fixture: ComponentFixture<OrganizationUnitsComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [FormsModule, ModalModule.forRoot()],
      declarations: [OrganizationUnitsComponent, MockLocalizePipe],
      providers: [
        { provide: OrganizationUnitServiceProxy, useClass: MockOrganizationUnitServiceProxy },
        { provide: UserServiceProxy, useClass: MockUserServiceProxy },
        { provide: RoleServiceProxy, useClass: MockRoleServiceProxy },
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

    fixture = TestBed.createComponent(OrganizationUnitsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should indent based on level', () => {
    expect(component.indent(2)['padding-left']).toBe('48px');
  });

  it('should reset active OU on create modal', () => {
    component.showCreateModal();
    expect(component.activeOu.displayName).toBe('');
    expect(component.isEdit).toBe(false);
  });
});
