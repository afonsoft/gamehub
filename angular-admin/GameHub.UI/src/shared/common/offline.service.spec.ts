import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';
import * as localForage from 'localforage';
import { firstValueFrom } from 'rxjs';
import { OfflineService } from './offline.service';

describe('OfflineService', () => {
  let service: OfflineService;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    Object.defineProperty(navigator, 'onLine', { value: true, configurable: true, writable: true });
    localForage.config({ driver: localForage.LOCALSTORAGE });
    await localForage.ready();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OfflineService, { provide: API_BASE_URL, useValue: '' }],
    });
    service = TestBed.inject(OfflineService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(async () => {
    httpMock.verify();
    await service.clearQueue();
    Object.defineProperty(navigator, 'onLine', { value: true, configurable: true, writable: true });
  });

  it('Dado uma ação offline, Quando enfileirada, Então deve persistir na fila', async () => {
    await service.queueAction({ url: '/api/services/app/Test/Create', method: 'POST', body: { name: 'Test' } });

    const queue = await service.getQueue();
    expect(queue.length).toBe(1);
    expect(queue[0].url).toBe('/api/services/app/Test/Create');
    expect(await firstValueFrom(service.pending$)).toBe(1);
  });

  it('Dado itens na fila, Quando online, Então deve sincronizar e remover os itens processados', async () => {
    await service.queueAction({ url: '/api/services/app/Test/Create', method: 'POST', body: { name: 'Test' } });

    const syncPromise = service.syncQueue();
    await new Promise(resolve => setTimeout(resolve, 0));

    const req = httpMock.expectOne(r => r.url.endsWith('/api/services/app/Test/Create'));
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });

    await syncPromise;

    const queue = await service.getQueue();
    expect(queue.length).toBe(0);
    expect(await firstValueFrom(service.pending$)).toBe(0);
  });

  it('Dado uma falha de rede na sincronização, Então deve manter o item na fila', async () => {
    await service.queueAction({ url: '/api/services/app/Test/Create', method: 'POST', body: { name: 'Test' } });

    const syncPromise = service.syncQueue();
    await new Promise(resolve => setTimeout(resolve, 0));

    const req = httpMock.expectOne(r => r.url.endsWith('/api/services/app/Test/Create'));
    req.error(new ErrorEvent('Network error'));

    await syncPromise;

    const queue = await service.getQueue();
    expect(queue.length).toBe(1);
    expect(await firstValueFrom(service.pending$)).toBe(1);
  });
});
