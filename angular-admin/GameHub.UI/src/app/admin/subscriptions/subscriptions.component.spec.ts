import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SubscriptionsComponent } from './subscriptions.component';
import {
  EditionServiceProxy,
  PaymentServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { PaymentExtendedService } from '@shared/service-proxies/payment-extended.service';
import {
  MockEditionServiceProxy,
  MockPaymentServiceProxy,
  MockPaymentExtendedService,
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

describe('SubscriptionsComponent', () => {
  let component: SubscriptionsComponent;
  let fixture: ComponentFixture<SubscriptionsComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [FormsModule, ModalModule.forRoot(), NoopAnimationsModule],
      declarations: [SubscriptionsComponent, MockLocalizePipe],
      providers: [
        { provide: EditionServiceProxy, useClass: MockEditionServiceProxy },
        { provide: PaymentServiceProxy, useClass: MockPaymentServiceProxy },
        { provide: PaymentExtendedService, useClass: MockPaymentExtendedService },
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

    fixture = TestBed.createComponent(SubscriptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('Dado o componente, quando criado, então deve existir', () => {
    expect(component).toBeTruthy();
  });

  it('Dado uma lista de edições, quando buscado o nome da edição por id, então deve retornar o displayName', () => {
    component.editions = [{ id: 1, displayName: 'Standard' } as any];
    expect(component.getEditionDisplayName(1)).toBe('Standard');
  });

  it('Dado um status de pagamento, quando mapeado a classe de status, então deve conter a classe correspondente', () => {
    expect(component.getStatusClass('Pending')).toContain('badge-warning');
    expect(component.getStatusClass('Completed')).toContain('badge-success');
    expect(component.getStatusClass('Failed')).toContain('badge-danger');
    expect(component.getStatusClass('Canceled')).toContain('badge-secondary');
  });

  it('Dado um pagamento concluído e recorrente, quando verificado se pode cancelar recorrência, então deve permitir', () => {
    const payment: any = { isRecurring: true, status: 'Completed' };
    expect(component.canCancelRecurring(payment)).toBe(true);
  });

  it('Dado um pagamento não recorrente, quando verificado se pode cancelar recorrência, então deve negar', () => {
    const payment: any = { isRecurring: false, status: 'Completed' };
    expect(component.canCancelRecurring(payment)).toBe(false);
  });

  it('Dado um pagamento concluído, quando verificado se pode fazer upgrade, então deve permitir', () => {
    const payment: any = { status: 'Completed' };
    expect(component.canUpgrade(payment)).toBe(true);
  });

  it('Dado um pagamento pendente, quando verificado se pode fazer upgrade, então deve negar', () => {
    const payment: any = { status: 'Pending' };
    expect(component.canUpgrade(payment)).toBe(false);
  });
});
