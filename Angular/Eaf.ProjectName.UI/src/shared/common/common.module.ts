import { EafModule } from '@eaf/eaf.module';
import * as ngCommon from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { AppUrlService } from './nav/app-url.service';
import { AppUiCustomizationService } from './ui/app-ui-customization.service';
import { AppSessionService } from './session/app-session.service';
import { CookieConsentService } from './session/cookie-consent.service';
import { AppAuthenticationService } from './auth/app-authentication-service';

@NgModule({
  imports: [ngCommon.CommonModule, EafModule],
})
export class CommonModule {
  static forRoot(): ModuleWithProviders<CommonModule> {
    return {
      ngModule: CommonModule,
      providers: [AppUiCustomizationService, CookieConsentService, AppSessionService, AppUrlService, AppAuthenticationService],
    };
  }
}
