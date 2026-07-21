import { Injector } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { LocalizePipe } from './localize.pipe';
import { LocalizationService } from '@eaf/localization/localization.service';
import { setupEafGlobals, MockLocalizationService } from '../../../test-helpers/mock-services';

describe('LocalizePipe', () => {
  let pipe: LocalizePipe;
  let mockLocalizationService: MockLocalizationService;

  beforeEach(() => {
    setupEafGlobals();
    mockLocalizationService = new MockLocalizationService();

    TestBed.configureTestingModule({
      providers: [
        { provide: LocalizationService, useValue: mockLocalizationService },
      ],
    });

    const injector = TestBed.inject(Injector);
    pipe = new LocalizePipe(injector);
  });

  it('deve ser criado', () => {
    expect(pipe).toBeTruthy();
  });

  it('deve chamar transform e retornar texto localizado', () => {
    const result = pipe.transform('TestKey');
    expect(result).toBeDefined();
  });

  it('deve ter localizationSourceName definido', () => {
    expect(pipe.localizationSourceName).toBeDefined();
  });

  it('deve usar eaf.utils.formatString no resultado', () => {
    spyOn(eaf.utils, 'formatString').and.returnValue('formatted');
    const result = pipe.transform('SomeKey');
    expect(eaf.utils.formatString).toHaveBeenCalled();
  });

  it('deve buscar em múltiplas fontes de localização', () => {
    spyOn(mockLocalizationService, 'localize').and.callFake((key: string, source?: string) => {
      return key;
    });
    pipe.transform('NotFound');
    expect(mockLocalizationService.localize).toHaveBeenCalledTimes(8);
  });
});
