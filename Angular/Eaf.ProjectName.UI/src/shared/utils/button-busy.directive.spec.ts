import { Component } from '@angular/core';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ButtonBusyDirective } from './button-busy.directive';
import { AppLocalizationService } from '@app/shared/common/localization/app-localization.service';
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
  MockAppLocalizationService,
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
  template: `<button [buttonBusy]="isBusy" busyText="Saving...">Save</button>`,
})
class TestHostComponent {
  isBusy = false;
}

describe('ButtonBusyDirective', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ButtonBusyDirective, TestHostComponent, MockLocalizePipe],
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
        { provide: AppUrlService, useClass: MockAppUrlService },{ provide: AppLocalizationService, useClass: MockAppLocalizationService }],
    });
    fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();
  });

  it('should create host component', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should disable button when busy', () => {
    fixture.componentInstance.isBusy = true;
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBeTruthy();
  });

  it('should re-enable button when not busy', () => {
    fixture.componentInstance.isBusy = true;
    fixture.detectChanges();
    fixture.componentInstance.isBusy = false;
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBeFalsy();
  });

  it('should show spinner when busy', () => {
    fixture.componentInstance.isBusy = true;
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.innerHTML).toContain('fa-spinner');
  });
});
