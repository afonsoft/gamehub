import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AppConsts } from '@shared/AppConsts';
import { PaymentExtendedService } from './payment-extended.service';
import {
  MockLocalizationService,
  MockPermissionCheckerService,
  MockFeatureCheckerService,
  MockNotifyService,
  MockSettingService,
  MockEafMultiTenancyService,
  MockAppSessionService,
  MockAppUiCustomizationService,
  MockAppUrlService,
  setupEafGlobals,
} from '../../test-helpers/mock-services';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';

describe('PaymentExtendedService', () => {
  let service: PaymentExtendedService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    setupEafGlobals();
    AppConsts.remoteServiceBaseUrl = 'http://localhost:8001';

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        PaymentExtendedService,
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
    });

    service = TestBed.inject(PaymentExtendedService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('Dado o serviço, quando criado, então deve existir', () => {
    expect(service).toBeTruthy();
  });

  it('Dado um filtro, quando getAll for chamado, então deve requisitar GetAll com os parâmetros', () => {
    const expected = { totalCount: 1, items: [{ id: 1, gateway: 'Stripe' }] };

    service.getAll({ filter: 'Stripe', sorting: 'id desc', skipCount: 0, maxResultCount: 10 }).subscribe(result => {
      expect(result).toEqual(expected);
    });

    const req = httpMock.expectOne(r =>
      r.url === 'http://localhost:8001/api/services/app/Payment/GetAll' &&
      r.params.get('Filter') === 'Stripe' &&
      r.params.get('Sorting') === 'id desc' &&
      r.params.get('SkipCount') === '0' &&
      r.params.get('MaxResultCount') === '10',
    );
    expect(req.request.method).toBe('GET');
    req.flush(expected);
  });

  it('Dado um identificador, quando getPayment for chamado, então deve requisitar GetPayment', () => {
    const expected = { id: 5, gateway: 'Stripe' };

    service.getPayment(5).subscribe(result => {
      expect(result).toEqual(expected);
    });

    const req = httpMock.expectOne(r =>
      r.url === 'http://localhost:8001/api/services/app/Payment/GetPayment' &&
      r.params.get('id') === '5',
    );
    expect(req.request.method).toBe('GET');
    req.flush(expected);
  });

  it('Dado uma solicitação de pagamento, quando createPayment for chamado, então deve postar para CreatePayment', () => {
    const input = { editionId: 1, gateway: 'Stripe' };
    const expected = { isSuccess: true, subscriptionPaymentId: 10 };

    service.createPayment(input).subscribe(result => {
      expect(result).toEqual(expected);
    });

    const req = httpMock.expectOne('http://localhost:8001/api/services/app/Payment/CreatePayment');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush(expected);
  });

  it('Dado uma solicitação de upgrade, quando upgradeSubscription for chamado, então deve postar para UpgradeSubscription', () => {
    const input = { newEditionId: 2, gateway: 'Stripe' };
    const expected = { isSuccess: true };

    service.upgradeSubscription(input).subscribe(result => {
      expect(result).toEqual(expected);
    });

    const req = httpMock.expectOne('http://localhost:8001/api/services/app/Payment/UpgradeSubscription');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush(expected);
  });

  it('Dado um paymentId, quando cancelRecurring for chamado, então deve postar para CancelRecurring', () => {
    const expected = { id: 8, status: 'Canceled' };

    service.cancelRecurring(8).subscribe(result => {
      expect(result).toEqual(expected);
    });

    const req = httpMock.expectOne(r =>
      r.url === 'http://localhost:8001/api/services/app/Payment/CancelRecurring' &&
      r.params.get('paymentId') === '8',
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();
    req.flush(expected);
  });
});
