///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LocalizationService {
  get languages(): eaf.localization.ILanguageInfo[] {
    return eaf.localization.languages;
  }

  get currentLanguage(): eaf.localization.ILanguageInfo {
    return eaf.localization.currentLanguage;
  }

  localize(key: string, sourceName: string): string {
    return eaf.localization.localize(key, sourceName);
  }

  getSource(sourceName: string): (key: string) => string {
    return eaf.localization.getSource(sourceName);
  }
}
