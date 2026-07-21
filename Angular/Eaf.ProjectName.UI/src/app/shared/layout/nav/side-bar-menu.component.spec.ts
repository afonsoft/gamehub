import { Subject } from 'rxjs';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { SideBarMenuComponent } from './side-bar-menu.component';
import { AppNavigationService } from './app-navigation.service';
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
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
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
  MockActivatedRoute,
  MockLayoutRefService,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../../test-helpers/mock-services';

class MockAppNavigationService {
  getMenu(): any[] {
    return [];
  }
  checkChildMenuItemPermission(menuItem: any): boolean {
    return true;
  }
  showMenuItem(menuItem: any): boolean {
    return true;
  }
}

describe('SideBarMenuComponent', () => {
  let component: SideBarMenuComponent;
  let fixture: ComponentFixture<SideBarMenuComponent>;
  const mockRouter = {
    navigate: jasmine.createSpy('navigate'),
    events: new Subject(),
    url: '/app/dashboard',
  };

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [SideBarMenuComponent, MockLocalizePipe],
      providers: [
        { provide: AppNavigationService, useClass: MockAppNavigationService },
        { provide: Router, useValue: mockRouter },
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
        { provide: LayoutRefService, useClass: MockLayoutRefService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(SideBarMenuComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have menu defined after init', () => {
    component.ngOnInit();
    expect(component.menu).toBeDefined();
  });
});
