import { Injectable } from '@angular/core';
import { AppLocalizationService } from '@app/shared/common/localization/app-localization.service';

@Injectable()
export class CookieConsentService {
  constructor(private readonly _appLocalizationService: AppLocalizationService) {}

  public init() {
    if (eaf.setting.getBoolean('App.UserManagement.IsCookieConsentEnabled')) {
      (window as any).cookieconsent.initialise({
        palette: {
          popup: {
            background: '#cdcdcd',
          },
          button: {
            background: '#FF7020',
          },
        },
        showLink: false,
        content: {
          message: this._appLocalizationService.l('CookieConsent_Message'),
          dismiss: this._appLocalizationService.l('CookieConsent_Dismiss'),
        },
      });
    }
  }
}
