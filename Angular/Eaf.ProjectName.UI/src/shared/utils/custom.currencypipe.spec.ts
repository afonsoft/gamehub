import { CustomCurrencyPipe } from './custom.currencypipe';

describe('CustomCurrencyPipe', () => {
  let pipe: CustomCurrencyPipe;

  beforeEach(() => {
    // Mock global eaf object
    (window as any).eaf = {
      localization: {
        currentLanguage: { name: 'en' },
      },
    };
    pipe = new CustomCurrencyPipe();
  });

  it('deve ser criado', () => {
    expect(pipe).toBeTruthy();
  });

  it('deve formatar valor com código de moeda USD', () => {
    const result = pipe.transform(1000, 'USD', 'symbol', '1.2-2', 'en');
    expect(result).toContain('1,000.00');
  });

  it('deve formatar valor com código de moeda EUR', () => {
    const result = pipe.transform(1234.56, 'EUR', 'symbol', '1.2-2', 'en');
    expect(result).toContain('1,234.56');
  });

  it('deve formatar valor zero', () => {
    const result = pipe.transform(0, 'USD', 'symbol', '1.2-2', 'en');
    expect(result).toContain('0.00');
  });

  it('deve formatar valor negativo', () => {
    const result = pipe.transform(-500.5, 'USD', 'symbol', '1.2-2', 'en');
    expect(result).toContain('500.50');
  });

  it('deve respeitar digitsInfo para casas decimais', () => {
    const result = pipe.transform(99.999, 'USD', 'symbol', '1.2-2', 'en');
    expect(result).toContain('100.00');
  });

  it('deve formatar valor grande corretamente', () => {
    const result = pipe.transform(1000000, 'USD', 'symbol', '1.2-2', 'en');
    expect(result).toContain('1,000,000.00');
  });
});
