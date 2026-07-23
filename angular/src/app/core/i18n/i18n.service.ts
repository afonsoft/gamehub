import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom } from 'rxjs';

export type SupportedLanguage = 'pt-BR' | 'en-US';

const STORAGE_KEY = 'gamehub-lang';
const DEFAULT_LANG: SupportedLanguage = 'pt-BR';

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly _currentLang = new BehaviorSubject<SupportedLanguage>(DEFAULT_LANG);
  private readonly _dictionary = new BehaviorSubject<Record<string, string>>({});

  currentLang$ = this._currentLang.asObservable();

  constructor(private http: HttpClient) {
    const saved = (typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null) as SupportedLanguage | null;
    if (saved === 'pt-BR' || saved === 'en-US') {
      this._currentLang.next(saved);
    }
  }

  async init(): Promise<void> {
    await this.loadLanguage(this._currentLang.value);
  }

  async setLanguage(lang: SupportedLanguage): Promise<void> {
    if (lang !== this._currentLang.value) {
      await this.loadLanguage(lang);
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, lang);
      }
    }
  }

  private async loadLanguage(lang: SupportedLanguage): Promise<void> {
    try {
      const data = await firstValueFrom(this.http.get<Record<string, string>>(`/i18n/${lang}.json`));
      this._dictionary.next(data ?? {});
      this._currentLang.next(lang);
    } catch {
      this._dictionary.next({});
    }
  }

  translate(key: string): string {
    return this._dictionary.value[key] ?? key;
  }

  getCurrentLang(): SupportedLanguage {
    return this._currentLang.value;
  }
}
