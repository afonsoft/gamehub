import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaymentsComponent } from './payments.component';
import { EditionServiceProxy, PaymentServiceProxy } from '@shared/service-proxies/service-proxies';
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

  it('should identify Stripe as the only recurring gateway', () => {
    expect(component.isRecurringSupported('Stripe')).toBe(true);
    expect(component.isRecurringSupported('PayPal')).toBe(false);
  });

  it('should preview amount based on edition and payment period', () => {
    component.editions = [{ id: 1, monthlyPrice: 10, quarterlyPrice: 25, annualPrice: 100 } as any];
    component.newPayment.editionId = 1;
    component.newPayment.paymentPeriodType = 30;
    expect(component.getAmountPreview()).toBe(10);
    component.newPayment.paymentPeriodType = 365;
    expect(component.getAmountPreview()).toBe(100);
  });
});
