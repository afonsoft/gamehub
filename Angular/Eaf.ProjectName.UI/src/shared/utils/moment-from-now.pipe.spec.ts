import { MomentFromNowPipe } from './moment-from-now.pipe';
import * as moment from 'moment';

describe('MomentFromNowPipe', () => {
  let pipe: MomentFromNowPipe;

  beforeEach(() => {
    pipe = new MomentFromNowPipe();
  });

  it('deve ser criado', () => {
    expect(pipe).toBeTruthy();
  });

  it('deve retornar string vazia quando valor é null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('deve retornar string vazia quando valor é undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('deve retornar string vazia quando valor é string vazia', () => {
    expect(pipe.transform('')).toBe('');
  });

  it('deve retornar texto relativo para data passada', () => {
    const pastDate = moment().subtract(5, 'minutes');
    const result = pipe.transform(pastDate);
    expect(result).toContain('minutes ago');
  });

  it('deve retornar texto relativo para data futura', () => {
    const futureDate = moment().add(1, 'hour');
    const result = pipe.transform(futureDate);
    expect(result).toContain('in');
  });

  it('deve aceitar string ISO como entrada', () => {
    const isoDate = moment().subtract(2, 'days').toISOString();
    const result = pipe.transform(isoDate);
    expect(result).toContain('days ago');
  });

  it('deve aceitar timestamp numérico como entrada', () => {
    const timestamp = moment().subtract(3, 'hours').valueOf();
    const result = pipe.transform(timestamp);
    expect(result).toContain('hours ago');
  });
});
