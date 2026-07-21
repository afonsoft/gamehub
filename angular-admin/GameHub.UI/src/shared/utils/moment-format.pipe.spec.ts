import { MomentFormatPipe } from './moment-format.pipe';
import * as moment from 'moment';

describe('MomentFormatPipe', () => {
  let pipe: MomentFormatPipe;

  beforeEach(() => {
    pipe = new MomentFormatPipe();
  });

  it('deve ser criado', () => {
    expect(pipe).toBeTruthy();
  });

  it('deve retornar string vazia quando valor é null', () => {
    expect(pipe.transform(null, 'YYYY-MM-DD')).toBe('');
  });

  it('deve retornar string vazia quando valor é undefined', () => {
    expect(pipe.transform(undefined, 'YYYY-MM-DD')).toBe('');
  });

  it('deve retornar string vazia quando valor é string vazia', () => {
    expect(pipe.transform('', 'YYYY-MM-DD')).toBe('');
  });

  it('deve formatar data com formato YYYY-MM-DD', () => {
    const date = moment('2024-03-15T10:30:00');
    const result = pipe.transform(date, 'YYYY-MM-DD');
    expect(result).toBe('2024-03-15');
  });

  it('deve formatar data com formato DD/MM/YYYY', () => {
    const date = moment('2024-03-15T10:30:00');
    const result = pipe.transform(date, 'DD/MM/YYYY');
    expect(result).toBe('15/03/2024');
  });

  it('deve formatar data com hora HH:mm:ss', () => {
    const date = moment('2024-03-15T10:30:45');
    const result = pipe.transform(date, 'HH:mm:ss');
    expect(result).toBe('10:30:45');
  });

  it('deve formatar data completa com DD/MM/YYYY HH:mm', () => {
    const date = moment('2024-12-25T08:00:00');
    const result = pipe.transform(date, 'DD/MM/YYYY HH:mm');
    expect(result).toBe('25/12/2024 08:00');
  });

  it('deve aceitar string ISO como entrada', () => {
    const result = pipe.transform('2024-06-01T14:30:00.000Z', 'YYYY');
    expect(result).toBe('2024');
  });

  it('deve aceitar timestamp numérico como entrada', () => {
    const timestamp = moment('2024-01-01').valueOf();
    const result = pipe.transform(timestamp, 'YYYY');
    expect(result).toBe('2024');
  });
});
