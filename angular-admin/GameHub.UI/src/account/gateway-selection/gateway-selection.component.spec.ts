import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { GatewaySelectionComponent } from './gateway-selection.component';
import {
  EditionServiceProxy,
  PaymentServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { PaymentExtendedService } from '@shared/service-proxies/payment-extended.service';
import {
  MockEditionServiceProxy,
  MockPaymentServiceProxy,
  MockPaymentExtendedService,
  MockActivatedRoute,
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
} from '../../test-helpers/mock-services';
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

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

describe('GatewaySelectionComponent', () => {
  let component: GatewaySelectionComponent;
  let fixture: ComponentFixture<GatewaySelectionComponent>;
  const mockRouter = new MockRouter();

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [FormsModule, NoopAnimationsModule],
      declarations: [GatewaySelectionComponent, MockLocalizePipe],
      providers: [
        { provide: EditionServiceProxy, useClass: MockEditionServiceProxy },
        { provide: PaymentServiceProxy, useClass: MockPaymentServiceProxy },
        { provide: PaymentExtendedService, useClass: MockPaymentExtendedService },
        { provide: ActivatedRoute, useClass: MockActivatedRoute },
        { provide: Router, useValue: mockRouter },
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

    fixture = TestBed.createComponent(GatewaySelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('Dado o componente, quando criado, então deve existir', () => {
    expect(component).toBeTruthy();
  });

  it('Dado uma edição selecionada, quando solicitada a prévia do valor mensal, então deve retornar o MonthlyPrice', () => {
    component.editions = [{ id: 1, displayName: 'Standard', monthlyPrice: 99.9, isFree: false } as any];
    component.model.editionId = 1;
    component.model.paymentPeriodType = 30;
    expect(component.getAmountPreview()).toBe(99.9);
  });

  it('Dado uma edição gratuita, quando solicitada a prévia do valor, então deve retornar undefined', () => {
    component.editions = [{ id: 2, displayName: 'Free', isFree: true } as any];
    component.model.editionId = 2;
    expect(component.getAmountPreview()).toBeUndefined();
  });

  it('Dado um gateway diferente de Stripe, quando verificado suporte a recorrência, então deve retornar falso', () => {
    component.model.gateway = 'PayPal';
    expect(component.isRecurringSupported()).toBe(false);
  });

  it('Dado o gateway Stripe, quando verificado suporte a recorrência, então deve retornar verdadeiro', () => {
    component.model.gateway = 'Stripe';
    expect(component.isRecurringSupported()).toBe(true);
  });

  it('Dado parâmetros de retorno de sucesso, quando o componente inicia, então deve preencher a mensagem de sucesso', () => {
    const route = TestBed.inject(ActivatedRoute) as any;
    route.queryParams = of({ status: 'success', paymentId: '42' });

    fixture = TestBed.createComponent(GatewaySelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.resultType).toBe('success');
  });
});
