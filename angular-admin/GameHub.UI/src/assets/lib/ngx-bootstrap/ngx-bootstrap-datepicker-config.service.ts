import { BsDatepickerConfig, BsDaterangepickerConfig, BsLocaleService } from 'ngx-bootstrap/datepicker';
import { NgxBootstrapLocaleMappingService } from 'assets/lib/ngx-bootstrap/ngx-bootstrap-locale-mapping.service';
import { defineLocale } from 'ngx-bootstrap/chronos';
import * as allLocales from 'ngx-bootstrap/chronos';
import { ThemeHelper } from '@app/shared/layout/themes/ThemeHelper';

export class NgxBootstrapDatePickerConfigService {
  static getDaterangepickerConfig(): BsDaterangepickerConfig {
    return Object.assign(new BsDaterangepickerConfig(), {
      containerClass: 'theme-' + NgxBootstrapDatePickerConfigService.getThemeColor(),
    });
  }

  static getDatepickerConfig(): BsDatepickerConfig {
    return Object.assign(new BsDatepickerConfig(), {
      containerClass: 'theme-' + NgxBootstrapDatePickerConfigService.getThemeColor(),
    });
  }

  static getThemeColor(): string {
    return ThemeHelper.getThemeColor();
  }

  static getDatepickerLocale(): BsLocaleService {
    const localeService = new BsLocaleService();
    localeService.use(eaf.localization.currentLanguage.name);
    return localeService;
  }

  static registerNgxBootstrapDatePickerLocales(): Promise<boolean> {
    if (eaf.localization.currentLanguage.name === 'en') {
      return Promise.resolve(true);
    }

    const moduleLocaleName = new NgxBootstrapLocaleMappingService().getModuleName(eaf.localization.currentLanguage.name);
    const localeData = (allLocales as any)[`${moduleLocaleName}Locale`];

    if (localeData) {
      defineLocale(eaf.localization.currentLanguage.name.toLowerCase(), localeData);
    }

    return Promise.resolve(true);
  }
}
