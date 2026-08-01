import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaymentsComponent } from './payments.component';
import { PaymentServiceProxy } from '@shared/service-proxies/payment.service-proxy';
import { EditionServiceProxy } from '@shared/service-proxies/edition.service-proxy';
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
  MockPaymentServiceProxy,
  MockEditionServiceProxy,
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

describe('PaymentsComponent', () => {
  let component: PaymentsComponent;
  let fixture: ComponentFixture<PaymentsComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [FormsModule, ModalModule.forRoot()],
      declarations: [PaymentsComponent, MockLocalizePipe],
      providers: [
        { provide: PaymentServiceProxy, useClass: MockPaymentServiceProxy },
        { provide: EditionServiceProxy, useClass: MockEditionServiceProxy },
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

    fixture = TestBed.createComponent(PaymentsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return edition display name from lookup', () => {
    component.editions = [{ id: 1, displayName: 'Standard' } as any];
    expect(component.getEditionDisplayName(1)).toBe('Standard');
  });

  it('should fallback to id when edition not found', () => {
    expect(component.getEditionDisplayName(99)).toBe('99');
  });

  it('should map status classes correctly', () => {
    expect(component.getStatusClass('Pending')).toContain('badge-warning');
    expect(component.getStatusClass('Completed')).toContain('badge-success');
    expect(component.getStatusClass('Failed')).toContain('badge-danger');
  });

  it('should reset filters on init', () => {
    component.filters = { filterText: 'test', status: 'Pending' };
    component.resetFilters();
    expect(component.filters.filterText).toBe('');
    expect(component.filters.status).toBe('');
  });
});
