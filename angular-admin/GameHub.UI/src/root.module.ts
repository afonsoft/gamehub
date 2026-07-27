import { PlatformLocation, registerLocaleData } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { APP_INITIALIZER, Injector, LOCALE_ID, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { EafModule } from '@eaf/eaf.module';
import { EafHttpInterceptor } from '@eaf/eafHttpInterceptor';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { CommonModule } from '@shared/common/common.module';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { CookieConsentService } from '@shared/common/session/cookie-consent.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { EafCorrelationIdInterceptor } from '@app/shared/eaf-contracts/eaf-correlation-id.interceptor';
import { DomHelper } from '@shared/helpers/DomHelper';
import { UrlHelper } from '@shared/helpers/UrlHelper';
import { API_BASE_URL, UiCustomizationSettingsDto } from '@shared/service-proxies/service-proxies';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { NgxBootstrapDatePickerConfigService } from 'assets/lib/ngx-bootstrap/ngx-bootstrap-datepicker-config.service';
import * as localForage from 'localforage';


import { AppModule } from './app/app.module';
import { AppPreBootstrap } from './AppPreBootstrap';
import { RootRoutingModule } from './root-routing.module';
import { RootComponent } from './root.component';

export function appInitializerFactory(injector: Injector, platformLocation: PlatformLocation, storageService: StorageService) {
  return async () => {
    eaf.ui.setBusy();
    await new Promise<boolean>((resolve, reject) => {
      AppConsts.appBaseHref = getBaseHref(platformLocation);
      const appBaseUrl = getDocumentOrigin() + AppConsts.appBaseHref;
      AppPreBootstrap.Init(storageService);
      AppPreBootstrap.run(
        appBaseUrl,
        async () => {
          handleLogoutRequest(injector.get(AppAuthService));
          initializeLocalForage();

          const appSessionService: AppSessionService = injector.get(AppSessionService);
          try {
            const result = await appSessionService.init();
            initializeAppCssClasses(injector, result);
            initializeTenantResources(injector);
            initializeCookieConsent(injector);
            registerLocales(resolve, reject);
          } catch (err) {
            eaf.ui.clearBusy();
            reject(err);
          }
        },
        resolve,
        reject,
      );
    });
  };
}

function initializeLocalForage() {
  localForage.config({
    driver: localForage.LOCALSTORAGE,
    name: 'GameHub',
    version: 1.0,
    storeName: 'gameHub',
    description: 'Cached data for GameHub',
  });
}

function initializeAppCssClasses(injector: Injector, theme: UiCustomizationSettingsDto) {
  const appUiCustomizationService = injector.get(AppUiCustomizationService);
  appUiCustomizationService.init(theme);

  //Css classes based on the layout
  if (eaf.session.userId) {
    document.body.className = appUiCustomizationService.getAppModuleBodyClass();
  } else {
    document.body.className = appUiCustomizationService.getAccountModuleBodyClass();
  }
}

function initializeTenantResources(injector: Injector) {
  const appSessionService: AppSessionService = injector.get(AppSessionService);

  const metaImage = DomHelper.getElementByAttributeValue('meta', 'property', 'og:image');
  if (metaImage && appSessionService.theme?.baseSettings?.theme) {
    //set og share image meta tag
    metaImage.setAttribute(
      'content',
      window.location.origin +
        '/assets/common/images/eaf/eaf-' +
        eaf.setting.get(appSessionService.theme.baseSettings.theme + '.' + 'App.UiManagement.Left.AsideSkin') +
        '.png',
    );
  }
}

function initializeCookieConsent(injector: Injector) {
  const cookieConsentService: CookieConsentService = injector.get(CookieConsentService);
  cookieConsentService.init();
}

function getDocumentOrigin() {
  if (!document.location.origin) {
    return document.location.protocol + '//' + document.location.hostname + (document.location.port ? ':' + document.location.port : '');
  }

  return document.location.origin;
}

function loadAngularLocaleModule(locale: string): Promise<any> {
  switch (locale) {
    case 'pt':
    case 'pt-BR':
      return import('@angular/common/locales/pt');
    case 'es':
      return import('@angular/common/locales/es');
    case 'es-AR':
      return import('@angular/common/locales/es-AR');
    case 'es-CL':
      return import('@angular/common/locales/es-CL');
    case 'es-BO':
      return import('@angular/common/locales/es-BO');
    case 'es-DO':
      return import('@angular/common/locales/es-DO');
    case 'es-PE':
      return import('@angular/common/locales/es-PE');
    case 'es-PY':
      return import('@angular/common/locales/es-PY');
    case 'es-EC':
      return import('@angular/common/locales/es-EC');
    case 'es-UY':
      return import('@angular/common/locales/es-UY');
    default:
      return import('@angular/common/locales/en');
  }
}

function registerLocales(resolve: (value?: boolean | Promise<boolean>) => void, reject: any) {
  if (shouldLoadLocale()) {
    const angularLocale = convertEafLocaleToAngularLocale(eaf.localization.currentLanguage.name);
    loadAngularLocaleModule(angularLocale).then(module => {
      registerLocaleData(module.default);
      NgxBootstrapDatePickerConfigService.registerNgxBootstrapDatePickerLocales().then(_ => {
        resolve(true);
      });
    }, reject);
  } else {
    NgxBootstrapDatePickerConfigService.registerNgxBootstrapDatePickerLocales().then(_ => {
      resolve(true);
    });
  }
}

export function shouldLoadLocale(): boolean {
  return eaf.localization.currentLanguage.name && eaf.localization.currentLanguage.name !== 'en-US';
}

export function convertEafLocaleToAngularLocale(locale: string): string {
  if (!AppConsts.localeMappings) {
    return locale;
  }

  const localeMapings = AppConsts.localeMappings?.filter(m => m.from === locale) || [];
  if (localeMapings?.length) {
    return localeMapings[0]['to'];
  }

  return locale;
}

export function getRemoteServiceBaseUrl(): string {
  return AppConsts.remoteServiceBaseUrl;
}

export function getCurrentLanguage(): string {
  return eaf.localization.currentLanguage.name;
}

export function getBaseHref(platformLocation: PlatformLocation): string {
  const baseUrl = platformLocation.getBaseHrefFromDOM();
  if (baseUrl) {
    return baseUrl;
  }

  return '/';
}

function handleLogoutRequest(authService: AppAuthService) {
  const currentUrl = UrlHelper.initialUrl;
  const returnUrl = UrlHelper.getReturnUrl();
  if (currentUrl.includes('account/logout') && returnUrl) {
    authService.logout(true, returnUrl);
  }
}

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule.forRoot(),
    EafModule,
    HttpClientModule,
    RootRoutingModule,
    AppModule,
    ServiceProxyModule,
  ],
  declarations: [RootComponent],
  providers: [
    EafCorrelationIdInterceptor,
    { provide: HTTP_INTERCEPTORS, useClass: EafCorrelationIdInterceptor, multi: true },
    EafHttpInterceptor,
    { provide: HTTP_INTERCEPTORS, useClass: EafHttpInterceptor, multi: true },
    { provide: API_BASE_URL, useFactory: getRemoteServiceBaseUrl },
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializerFactory,
      deps: [Injector, PlatformLocation, StorageService],
      multi: true,
    },
    {
      provide: LOCALE_ID,
      useFactory: getCurrentLanguage,
    },
  ],
  bootstrap: [RootComponent],
})
export class RootModule {}
