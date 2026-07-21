import { Component } from '@angular/core';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { BusyIfDirective } from './busy-if.directive';
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
} from '../../test-helpers/mock-services';

@Component({
  standalone: false,
  template: `<div [busyIf]="isBusy"></div>`,
})
class TestHostComponent {
  isBusy = false;
}

describe('BusyIfDirective', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [BusyIfDirective, TestHostComponent, MockLocalizePipe],
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
      ],
    });
    fixture = TestBed.createComponent(TestHostComponent);
  });

  it('should create host component', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call eaf.ui.setBusy when isBusy is true', () => {
    spyOn((window as any).eaf.ui, 'setBusy');
    fixture.componentInstance.isBusy = true;
    fixture.detectChanges();
    expect((window as any).eaf.ui.setBusy).toHaveBeenCalled();
  });

  it('should call eaf.ui.clearBusy when isBusy is false', () => {
    spyOn((window as any).eaf.ui, 'clearBusy');
    fixture.componentInstance.isBusy = true;
    fixture.detectChanges();
    fixture.componentInstance.isBusy = false;
    fixture.detectChanges();
    expect((window as any).eaf.ui.clearBusy).toHaveBeenCalled();
  });
});
