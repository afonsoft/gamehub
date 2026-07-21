import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { Auth0Client } from '@auth0/auth0-spa-js';
import * as msal from '@azure/msal-browser';
import { TokenService } from '@eaf/auth/token.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { LogService } from '@eaf/log/log.service';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { UrlHelper } from '@shared/helpers/UrlHelper';
import {
  AuthenticateModel,
  AuthenticateResultModel,
  ExternalAuthenticateModel,
  ExternalAuthenticateResultModel,
  ExternalLoginProviderInfoModel,
  TokenAuthServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { ScriptLoaderService } from '@shared/utils/script-loader.service';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';

import { finalize } from 'rxjs/operators';

declare const gapi: any;

export class ExternalLoginProvider extends ExternalLoginProviderInfoModel {
  static readonly GOOGLE: string = 'Google';
  static readonly MICROSOFT: string = 'Microsoft';
  static readonly OPENID: string = 'OpenIdConnect';
  static readonly Auth0: string = 'AuthZero';

  icon: string;
  initialized = false;

  constructor(providerInfo: ExternalLoginProviderInfoModel) {
    super();

    this.name = providerInfo.name;
    this.clientId = providerInfo.clientId;
    this.tenantId = providerInfo.tenantId;
    this.additionalParams = providerInfo.additionalParams;
    this.icon = providerInfo.name.toLowerCase();
  }
}

@Injectable()
export class LoginService {
  localizationSourceName = AppConsts.localization.defaultLocalizationSourceName;
  localizationSourceNameEaf = AppConsts.localization.defaultLocalizationSourceNameEaf;
  localization: LocalizationService;
  authenticateModel: AuthenticateModel;
  authenticateResult: AuthenticateResultModel;
  rememberMe: boolean;

  static readonly twoFactorRememberClientTokenName = 'TwoFactorRememberClientToken';
  private isSsoLoging = false;

  MSAL: msal.PublicClientApplication; // Microsoft API
  auth0: Auth0Client; //Auth0 API
  externalLoginProviders: ExternalLoginProvider[] = [];

  constructor(
    injector: Injector,
    private readonly _tokenAuthService: TokenAuthServiceProxy,
    private readonly _router: Router,
    private readonly _storageService: StorageService,
    private readonly _tokenService: TokenService,
    private readonly _logService: LogService,
    private readonly oauthService: OAuthService,
  ) {
    this.clear();
    this.localization = injector.get(LocalizationService);
  }

  l(key: string, ...args: any[]): string {
    return this.ls(this.localizationSourceName, key, ...args);
  }

  ls(sourcename: string, key: string, ...args: any[]): string {
    let localizedText = this.localization.localize(key, this.localizationSourceName);

    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.localizationSourceNameEaf);

    args.unshift(localizedText);

    return eaf.utils.formatString.call(null, ...args);
  }

  authenticate(finallyCallback?: () => void, redirectUrl?: string, captchaResponse?: string): void {
    finallyCallback = finallyCallback || (() => {});

    this.authenticateModel.singleSignIn = UrlHelper.getSingleSignIn();
    this.authenticateModel.returnUrl = UrlHelper.getReturnUrl();
    this.authenticateModel.captchaResponse = captchaResponse;

    this._tokenAuthService
      .authenticate(this.authenticateModel)
      .pipe(finalize(finallyCallback))
      .subscribe((result: AuthenticateResultModel) => {
        this.processAuthenticateResult(result, redirectUrl);
      });
  }

  private processAuthenticateResult(authenticateResult: AuthenticateResultModel, redirectUrl?: string) {
    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    if (isLocalhost) {
      this._logService.info('[LoginService] processAuthenticateResult called with: ' + JSON.stringify(authenticateResult));
      this._logService.info('[LoginService] shouldResetPassword: ' + authenticateResult.shouldResetPassword);
      this._logService.info('[LoginService] accessToken: ' + authenticateResult.accessToken);
    }
    this.authenticateResult = authenticateResult;

    if (authenticateResult.shouldResetPassword) {
      // Password reset

      this._router.navigate(['account/reset-password'], {
        queryParams: {
          userId: authenticateResult.userId,
          tenantId: eaf.session.tenantId,
          resetCode: authenticateResult.passwordResetCode,
        },
      });

      this.clear();
    } else if (authenticateResult.accessToken) {
      // Successfully logged in
      if (authenticateResult.returnUrl && !redirectUrl) {
        redirectUrl = authenticateResult.returnUrl;
      }

      this.login(
        authenticateResult.accessToken,
        authenticateResult.encryptedAccessToken,
        authenticateResult.expireInSeconds,
        this.rememberMe,
        redirectUrl,
      );
    } else {
      // Unexpected result!
      if (isLocalhost) {
        this._logService.error('[LoginService] Unexpected authenticateResult - no accessToken and no shouldResetPassword: ' + JSON.stringify(authenticateResult));
      }
      this._logService.warn('Unexpected authenticateResult!');
      this._router.navigate(['account/login']);
    }
  }

  private login(
    accessToken: string,
    encryptedAccessToken: string,
    expireInSeconds: number,
    rememberMe?: boolean,
    redirectUrl?: string,
  ): void {
    const tokenExpireDate = rememberMe
      ? new Date(Date.now() + 10000 * expireInSeconds)
      : new Date(Date.now() + 1000 * expireInSeconds);

    this._tokenService.setToken(accessToken, tokenExpireDate);

    this._storageService.setCookieValue(eaf.auth.tokenCookieName, accessToken, tokenExpireDate, eaf.appPath);

    this._storageService.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, encryptedAccessToken, tokenExpireDate, eaf.appPath);

    this._storageService.setCookieValue(AppConsts.expirationToken.keyName, expireInSeconds.toString(), null, eaf.appPath);

    if (redirectUrl) {
      setTimeout(() => {
        location.href = redirectUrl;
      }, 200);
    } else {
      let initialUrl = UrlHelper.initialUrl;

      if (initialUrl.indexOf('/account') > 0) {
        initialUrl = AppConsts.appBaseUrl;
      }
      setTimeout(() => {
        location.href = initialUrl;
      }, 200);
    }
  }

  private clear(): void {
    this.authenticateModel = new AuthenticateModel();
    this.authenticateModel.rememberClient = false;
    this.authenticateResult = null;
    this.rememberMe = false;
  }

  private initExternalLoginProviders(callback?: () => void) {
    callback = callback || (() => {});
    this._tokenAuthService.getExternalAuthenticationProviders().subscribe((providers: ExternalLoginProviderInfoModel[]) => {
      this.externalLoginProviders = providers.map(p => new ExternalLoginProvider(p));
      if (callback) {
        callback();
      }
    });
  }

  private ensureExternalLoginProviderInitialized(loginProvider: ExternalLoginProvider, callback: () => void) {
    if (loginProvider.initialized) {
      callback();
      return;
    }
    if (loginProvider.name === ExternalLoginProvider.Auth0) {
      this.auth0 = new Auth0Client({
        domain: loginProvider.additionalParams['Endpoint'],
        clientId: loginProvider.clientId,
        authorizationParams: {
          //redirect_uri: AppConsts.appBaseUrl + '/account/login/sso', //If Refirect
          redirect_uri: AppConsts.appBaseUrl, //If Popup
        },
      });

      //Callback from redirect
      this.auth0
        .handleRedirectCallback()
        .then(result => {
          if (result) {
            this.AuthZeroLoginCallback(result);
          }
        })
        .catch(error => {
          this._logService.error(error);
        });
      callback();
    } else if (loginProvider.name === ExternalLoginProvider.GOOGLE) {
      new ScriptLoaderService().load('https://apis.google.com/js/api.js').then(() => {
        gapi.load('client:auth2', () => {
          gapi.client
            .init({
              clientId: loginProvider.clientId,
              scope: 'openid profile email',
            })
            .then(() => {
              callback();
            });
        });
      });
    } else if (loginProvider.name === ExternalLoginProvider.MICROSOFT) {
      this.MSAL = new msal.PublicClientApplication({
        auth: {
          clientId: loginProvider.clientId,
          navigateToLoginRequestUrl: true,
          //redirectUri: AppConsts.appBaseUrl + '/account/login/sso', //If Refirect
          redirectUri: AppConsts.appBaseUrl, //If Popup
          authority: loginProvider.tenantId !== undefined ? 'https://login.microsoftonline.com/' + loginProvider.tenantId.trim() + '/' : '',
        },
        cache: {
          cacheLocation: 'sessionStorage',
          storeAuthStateInCookie: true, // Set this to "true" if you are having issues on IE11 or Edge
        },
        system: {
          loggerOptions: {
            loggerCallback: (level: msal.LogLevel, message: string, containsPii: boolean): void => {
              if (containsPii) {
                return;
              }
              if (level === msal.LogLevel.Error) {
                this._logService.error(message);
                eaf.message.error(message);
              } else {
                console.warn(message);
              }
            },
            piiLoggingEnabled: true,
          },
        },
      });

      //Callback from redirect
      this.MSAL.handleRedirectPromise()
        .then(result => {
          if (result) {
            this.microsoftLoginCallback(result);
          }
        })
        .catch(error => {
          this._logService.error(error);
          eaf.message.error(this.l('CouldNotValidateExternalUser', error));
        });
      callback();
    } else if (loginProvider.name === ExternalLoginProvider.OPENID) {
      const authConfig = this.getOpenIdConnectConfig(loginProvider);
      this.oauthService.configure(authConfig);
      this.oauthService.initImplicitFlow('openIdConnect=1');
    }
  }

  public openIdConnectLoginCallback(callback?: () => void) {
    this.initExternalLoginProviders(() => {
      const openIdProvider = this.externalLoginProviders.find(p => p.name === 'OpenIdConnect');
      const authConfig = this.getOpenIdConnectConfig(openIdProvider);

      this.oauthService.configure(authConfig);
      this.oauthService.tryLogin().then(() => {
        const claims = this.oauthService.getIdentityClaims();

        const model = new ExternalAuthenticateModel();
        model.authProvider = ExternalLoginProvider.OPENID;
        model.providerAccessCode = this.oauthService.getIdToken();
        model.providerKey = claims['sub'];
        model.singleSignIn = UrlHelper.getSingleSignIn();
        model.returnUrl = UrlHelper.getReturnUrl();

        this._tokenAuthService.externalAuthenticate(model).subscribe((result: ExternalAuthenticateResultModel) => {
          if (result.waitingForActivation) {
            eaf.message.info('You have successfully registered. Waiting for activation!');
            return;
          }
          this.authenticateResult = new AuthenticateResultModel();
          this.authenticateResult.accessToken = result.accessToken;
          this.authenticateResult.encryptedAccessToken = result.encryptedAccessToken;
          this.authenticateResult.returnUrl = result.returnUrl;
          this.authenticateResult.expireInSeconds = result.expireInSeconds;
          this.authenticateResult.userId = result.userId;

          this.login(result.accessToken, result.encryptedAccessToken, result.expireInSeconds, this.rememberMe, result.returnUrl);
        });
      });
    });
  }

  private googleLoginStatusChangeCallback(isSignedIn) {
    if (isSignedIn) {
      const model = new ExternalAuthenticateModel();
      model.authProvider = ExternalLoginProvider.GOOGLE;
      model.providerAccessCode = gapi.auth2.getAuthInstance().currentUser.get().getAuthResponse().access_token;
      model.providerKey = gapi.auth2.getAuthInstance().currentUser.get().getBasicProfile().getId();
      model.singleSignIn = UrlHelper.getSingleSignIn();
      model.returnUrl = UrlHelper.getReturnUrl();

      this._tokenAuthService.externalAuthenticate(model).subscribe((result: ExternalAuthenticateResultModel) => {
        if (result.waitingForActivation) {
          eaf.message.info('You have successfully registered. Waiting for activation!');
          return;
        }

        this.authenticateResult = new AuthenticateResultModel();
        this.authenticateResult.accessToken = result.accessToken;
        this.authenticateResult.encryptedAccessToken = result.encryptedAccessToken;
        this.authenticateResult.returnUrl = result.returnUrl;
        this.authenticateResult.expireInSeconds = result.expireInSeconds;
        this.authenticateResult.userId = result.userId;

        this.login(result.accessToken, result.encryptedAccessToken, result.expireInSeconds, this.rememberMe, result.returnUrl);
      });
    }
  }

  private microsoftLoginCallback(response: msal.AuthenticationResult) {
    if (!this.isSsoLoging) {
      this.isSsoLoging = true;
      const model = new ExternalAuthenticateModel();
      model.authProvider = ExternalLoginProvider.MICROSOFT;
      model.providerAccessCode = response.accessToken;
      model.providerKey = response.uniqueId == '' || response.uniqueId === null ? response.idToken : response.uniqueId;
      model.singleSignIn = UrlHelper.getSingleSignIn();
      model.returnUrl = UrlHelper.getReturnUrl();

      this._tokenAuthService.externalAuthenticate(model).subscribe((result: ExternalAuthenticateResultModel) => {
        if (result.waitingForActivation) {
          eaf.message.info('You have successfully registered. Waiting for activation!');
          return;
        }

        this.authenticateResult = new AuthenticateResultModel();
        this.authenticateResult.accessToken = result.accessToken;
        this.authenticateResult.encryptedAccessToken = result.encryptedAccessToken;
        this.authenticateResult.returnUrl = result.returnUrl;
        this.authenticateResult.expireInSeconds = result.expireInSeconds;
        this.authenticateResult.userId = result.userId;
        this.isSsoLoging = false;

        this.login(result.accessToken, result.encryptedAccessToken, result.expireInSeconds, this.rememberMe, result.returnUrl);
      });
      this.isSsoLoging = false;
    }
  }

  private AuthZeroLoginCallback(response: any) {
    if (!this.isSsoLoging) {
      this.isSsoLoging = true;

      this.auth0
        .getTokenSilently()
        .then(result => {
          this.auth0
            .getIdTokenClaims()
            .then(claims => {
              const model = new ExternalAuthenticateModel();
              model.authProvider = ExternalLoginProvider.Auth0;
              model.providerAccessCode = result;
              model.providerKey = claims.sub;
              model.singleSignIn = UrlHelper.getSingleSignIn();
              model.returnUrl = UrlHelper.getReturnUrl();

              this._tokenAuthService.externalAuthenticate(model).subscribe((result: ExternalAuthenticateResultModel) => {
                if (result.waitingForActivation) {
                  eaf.message.info('You have successfully registered. Waiting for activation!');
                  return;
                }

                this.authenticateResult = new AuthenticateResultModel();
                this.authenticateResult.accessToken = result.accessToken;
                this.authenticateResult.encryptedAccessToken = result.encryptedAccessToken;
                this.authenticateResult.returnUrl = result.returnUrl;
                this.authenticateResult.expireInSeconds = result.expireInSeconds;
                this.authenticateResult.userId = result.userId;
                this.isSsoLoging = false;

                this.login(result.accessToken, result.encryptedAccessToken, result.expireInSeconds, this.rememberMe, result.returnUrl);
              });
              this.isSsoLoging = false;
            })
            .catch(error => {
              this._logService.error(error);
            });
        })
        .catch(error => {
          this._logService.error(error);
        });
    }
  }

  init(callback?: () => void): void {
    this.initExternalLoginProviders(callback);
  }

  SSO_Microsoft_Callback() {
    this.initExternalLoginProviders(() => {
      const microsoftProvider = this.externalLoginProviders.find(p => p.name === 'Microsoft');

      this.ensureExternalLoginProviderInitialized(microsoftProvider, () => {
        this.MSAL.handleRedirectPromise()
          .then(result => {
            if (result) {
              this.microsoftLoginCallback(result);
            }
          })
          .catch(error => {
            this._logService.error(error);
            eaf.message.error(this.l('CouldNotValidateExternalUser', error));
          });
      });
    });
  }

  SSO_AuthZero_Callback() {
    this.initExternalLoginProviders(() => {
      const auth0Provider = this.externalLoginProviders.find(p => p.name === 'AuthZero');

      this.ensureExternalLoginProviderInitialized(auth0Provider, () => {
        this.auth0
          .handleRedirectCallback()
          .then(result => {
            if (result) {
              this.AuthZeroLoginCallback(result);
            }
          })
          .catch(error => {
            this._logService.error(error);
            eaf.message.error(this.l('CouldNotValidateExternalUser', error));
          });
      });
    });
  }

  private getOpenIdConnectConfig(loginProvider: ExternalLoginProvider): AuthConfig {
    const authConfig = new AuthConfig();
    authConfig.loginUrl = loginProvider.additionalParams['LoginUrl'];
    authConfig.issuer = loginProvider.additionalParams['Authority'];
    authConfig.skipIssuerCheck = loginProvider.additionalParams['ValidateIssuer'] === 'false';
    authConfig.clientId = loginProvider.clientId;
    authConfig.responseType = 'id_token';
    authConfig.redirectUri = window.location.origin + '/account/login/sso';
    authConfig.scope = 'openid profile';
    authConfig.requestAccessToken = false;
    return authConfig;
  }

  externalAuthenticate(provider: ExternalLoginProvider): void {
    this.ensureExternalLoginProviderInitialized(provider, () => {
      if (provider.name === ExternalLoginProvider.GOOGLE) {
        gapi.auth2
          .getAuthInstance()
          .signIn()
          .then(() => {
            this.googleLoginStatusChangeCallback(gapi.auth2.getAuthInstance().isSignedIn.get());
          })
          .catch(error => {
            eaf.message.error(error);
            eaf.log.error(error);
          });
      } else if (provider.name === ExternalLoginProvider.MICROSOFT) {
        const scopes = ['user.read'];
        //If Redirect
        //this.MSAL.loginRedirect({ scopes: scopes, prompt: 'select_account' });

        //If Popup
        this.MSAL.loginPopup({
          scopes: scopes,
          prompt: 'select_account',
        })
          .then((idTokenResponse: msal.AuthenticationResult) => {
            this.microsoftLoginCallback(idTokenResponse);
          })
          .catch(error => {
            eaf.message.error(error, 'External Login');
            eaf.log.error(error);
          });
      } else if (provider.name === ExternalLoginProvider.Auth0) {
        this.auth0
          .loginWithPopup()
          .then(result => {
            this.AuthZeroLoginCallback(true);
          })
          .catch(error => {
            eaf.message.error(error, 'External Login');
            eaf.log.error(error);
          });
      }
    });
  }
}
