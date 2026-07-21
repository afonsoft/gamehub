import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UsersComponent } from './users.component';
import { ImpersonationService } from './impersonation.service';
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { FileDownloadService } from '@shared/utils/file-download.service';
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
  MockUserServiceProxy,
  MockFileDownloadService,
  MockImpersonationService,
  MockActivatedRoute,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../test-helpers/mock-services';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [UsersComponent, MockLocalizePipe],
      providers: [
        { provide: ImpersonationService, useClass: MockImpersonationService },
        { provide: UserServiceProxy, useClass: MockUserServiceProxy },
        { provide: FileDownloadService, useClass: MockFileDownloadService },
        { provide: ActivatedRoute, useClass: MockActivatedRoute },
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

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize filters on ngOnInit', () => {
    component.ngOnInit();
    expect(component.filters.filterText).toBeDefined();
  });

  it('should get roles as string', () => {
    const roles = [{ roleName: 'Admin' }, { roleName: 'User' }];
    const result = component.getRolesAsString(roles);
    expect(result).toBe('Admin, User');
  });

  it('should return empty string for empty roles', () => {
    const result = component.getRolesAsString([]);
    expect(result).toBe('');
  });

  it('should have entity type full name', () => {
    expect(component._entityTypeFullName).toBe('Eaf.Middleware.Authorization.Users.User');
  });

  it('should have entityHistoryEnabled set to false initially', () => {
    expect(component.entityHistoryEnabled).toBeFalsy();
  });
});
