import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { timeout } from 'rxjs/operators';

import { TokenService } from './auth/token.service';
import { LogService } from './log/log.service';
import { MessageService } from './message/message.service';
import { StorageService } from './utils/storage.service';

export interface IValidationErrorInfo {
  message: string;
  members: string[];
}

export interface IErrorInfo {
  code: number;
  message: string;
  details: string;
  validationErrors: IValidationErrorInfo[];
}

export interface IAjaxResponse {
  success: boolean;
  result?: any;
  targetUrl?: string;
  error?: IErrorInfo;
  unAuthorizedRequest: boolean;
  __abp: boolean;
}

@Injectable()
export class EafHttpConfiguration {
  constructor(
    private readonly _messageService: MessageService,
    private readonly _logService: LogService,
  ) {}

  defaultError = <IErrorInfo>{
    message: 'An error has occurred!',
    details: 'Error details were not sent by server.',
  };

  defaultError401 = <IErrorInfo>{
    message: 'You are not authenticated!',
    details: 'You should be authenticated (sign in) in order to perform this operation.',
  };

  defaultError403 = <IErrorInfo>{
    message: 'You are not authorized!',
    details: 'You are not allowed to perform this operation.',
  };

  defaultError404 = <IErrorInfo>{
    message: 'Resource not found!',
    details: 'The resource requested could not be found on the server.',
  };

  logError(error: IErrorInfo): void {
    this._logService.error(error);
  }

  showError(error: IErrorInfo): any {
    if (error.details) {
      return this._messageService.error(error.details, error.message || this.defaultError.message);
    } else {
      return this._messageService.error(error.message || this.defaultError.message);
    }
  }

  handleTargetUrl(targetUrl: string): void {
    if (!targetUrl) {
      location.href = '/';
    } else {
      location.href = targetUrl;
    }
  }

  handleUnAuthorizedRequest(messagePromise: any, targetUrl?: string) {


    if (messagePromise) {
      messagePromise.done(() => {
        this.handleTargetUrl(targetUrl || '/');
      });
    } else {
      this.handleTargetUrl(targetUrl || '/');
    }
  }

  handleNonEafErrorResponse(response: any) {
    const body = response.error ?? response.body;
    if (body?.message) {
      return this._messageService.error(body.message, body.code || 'Error');
    }

    switch (response.status) {
      case 401:
        this.handleUnAuthorizedRequest(this.showError(this.defaultError401), '/');
        break;
      case 403:
        this.showError(this.defaultError403);
        break;
      case 404:
        this.showError(this.defaultError404);
        break;
      default:
        this.showError(this.defaultError);
        break;
    }
  }

  handleEafResponse(response: HttpResponse<any>, ajaxResponse: IAjaxResponse): HttpResponse<any> {
    let newResponse: HttpResponse<any>;

    if (ajaxResponse.success) {
      newResponse = response.clone({
        body: ajaxResponse.result,
      });

      if (ajaxResponse.targetUrl) {
        this.handleTargetUrl(ajaxResponse.targetUrl);
      }
    } else {
      newResponse = response.clone({
        body: ajaxResponse.result,
      });

      if (!ajaxResponse.error) {
        ajaxResponse.error = this.defaultError;
      }

      this.logError(ajaxResponse.error);
      this.showError(ajaxResponse.error);

      if (response.status === 401) {
        this.handleUnAuthorizedRequest(null, ajaxResponse.targetUrl);
      }
    }

    return newResponse;
  }

  getEafAjaxResponseOrNull(response: HttpResponse<any>): IAjaxResponse | null {
    if (!response?.headers) {
      return null;
    }

    const contentType = response.headers.get('Content-Type');
    if (!contentType) {
      this._logService.warn('Content-Type is not sent!');
      return null;
    }

    if (contentType.indexOf('application/json') < 0) {
      this._logService.warn('Content-Type is not application/json: ' + contentType);
      return null;
    }

    const responseObj = JSON.parse(JSON.stringify(response.body));
    if (!responseObj.__abp) {
      return null;
    }

    return responseObj as IAjaxResponse;
  }

  handleResponse(response: HttpResponse<any>): HttpResponse<any> {
    const ajaxResponse = this.getEafAjaxResponseOrNull(response);
    if (ajaxResponse == null) {
      return response;
    }

    return this.handleEafResponse(response, ajaxResponse);
  }

  blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
      if (!blob) {
        observer.next('');
        observer.complete();
      } else {
        blob.text().then(text => {
          observer.next(text);
          observer.complete();
        });
      }
    });
  }
}

@Injectable()
export class EafHttpInterceptor implements HttpInterceptor {
  protected configuration: EafHttpConfiguration;
  private pendingRequests = 0;

  constructor(
    configuration: EafHttpConfiguration,
    private readonly _storageService: StorageService,
    private readonly _tokenService: TokenService,
  ) {
    this.configuration = configuration;
  }

  private setBusy(): void {
    this.pendingRequests++;
    if (this.pendingRequests === 1) {
      (window as any).eaf.ui.setBusy(document.body);
    }
  }

  private clearBusy(): void {
    this.pendingRequests--;
    if (this.pendingRequests <= 0) {
      this.pendingRequests = 0;
      (window as any).eaf.ui.clearBusy(document.body);
    }
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const interceptObservable = new Subject<HttpEvent<any>>();
    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

    if (isLocalhost) {
      (window as any).eaf.log.info('[EafHttpInterceptor] Intercepting request: ' + request.url);
    }

    const modifiedRequest = this.normalizeRequestHeaders(request);

    if (isLocalhost) {
      (window as any).eaf.log.info('[EafHttpInterceptor] Modified request headers: ' + Array.from(modifiedRequest.headers.keys()).join(', '));
    }

    const timeoutValue = request.headers.get('timeout') || '600000';
    const timeoutValueNumeric = Number(timeoutValue);

    this.setBusy();

    next
      .handle(modifiedRequest)
      .pipe(timeout(timeoutValueNumeric))
      .subscribe(
        (event: HttpEvent<any>) => {
          if (isLocalhost && event instanceof HttpResponse) {
            (window as any).eaf.log.info('[EafHttpInterceptor] Response received: ' + event.url + ' ' + event.status);
          }
          this.handleSuccessResponse(event, interceptObservable);
          if (event instanceof HttpResponse) {
            this.clearBusy();
          }
        },
        (error: any) => {
          if (isLocalhost) {
            (window as any).eaf.log.error('[EafHttpInterceptor] Error: ' + error);
          }
          this.clearBusy();
          return this.handleErrorResponse(error, interceptObservable);
        },
      );

    return interceptObservable;
  }

  protected handleSuccessResponse(event: HttpEvent<any>, interceptObservable: Subject<HttpEvent<any>>): void {

    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

    if (event instanceof HttpResponse) {
      if (event.body instanceof Blob && event.body.type && event.body.type.indexOf('application/json') >= 0) {
        this.configuration.blobToText(event.body).subscribe(json => {
          const responseBody = json == 'null' ? {} : JSON.parse(json);

          if (isLocalhost) {
            (window as any).eaf.log.info('[EafHttpInterceptor] Blob JSON parsed: ' + JSON.stringify(responseBody));
          }

          const modifiedResponse = this.configuration.handleResponse(
            event.clone({
              body: responseBody,
            }),
          );

          interceptObservable.next(
            modifiedResponse.clone({
              body: new Blob([JSON.stringify(modifiedResponse.body)], { type: 'application/json' }),
            }),
          );

          interceptObservable.complete();
        });
      } else {
        interceptObservable.next(event);
        interceptObservable.complete();
      }
    } else {
      interceptObservable.next(event);
    }
  }

  protected handleErrorResponse(error: any, interceptObservable: Subject<HttpEvent<any>>): Observable<any> {
    const errorObservable = new Subject<any>();

    if (!(error.error instanceof Blob)) {
      this.configuration.logError(error);
      this.configuration.handleNonEafErrorResponse(error);
      interceptObservable.error(error);
      interceptObservable.complete();
      return of({});
    }

    this.configuration.blobToText(error.error).subscribe(json => {
      const errorBody = json == '' || json == 'null' ? {} : JSON.parse(json);
      const errorResponse = new HttpResponse({
        headers: error.headers,
        status: error.status,
        body: errorBody,
      });

      const ajaxResponse = this.configuration.getEafAjaxResponseOrNull(errorResponse);

      if (ajaxResponse != null) {
        this.configuration.handleEafResponse(errorResponse, ajaxResponse);
      } else {
        this.configuration.handleNonEafErrorResponse(errorResponse);
      }

      errorObservable.complete();

      interceptObservable.error(error);
      interceptObservable.complete();
    });

    return errorObservable;
  }

  private itemExists<T>(items: T[], predicate: (item: T) => boolean): boolean {
    for (const item of items) {
      if (predicate(item)) {
        return true;
      }
    }

    return false;
  }

  protected normalizeRequestHeaders(request: HttpRequest<any>): HttpRequest<any> {
    let modifiedHeaders = request.headers
      .set('Pragma', 'no-cache')
      .set('Cache-Control', 'no-cache')
      .set('Expires', 'Sat, 01 Jan 2000 00:00:00 GMT');

    modifiedHeaders = this.addXRequestedWithHeader(modifiedHeaders);
    modifiedHeaders = this.addAuthorizationHeaders(modifiedHeaders);
    modifiedHeaders = this.addAspNetCoreCultureHeader(modifiedHeaders);
    modifiedHeaders = this.addAcceptLanguageHeader(modifiedHeaders);
    modifiedHeaders = this.addTenantIdHeader(modifiedHeaders);

    return request.clone({
      headers: modifiedHeaders,
    });
  }

  protected addXRequestedWithHeader(headers: HttpHeaders): HttpHeaders {
    if (headers) {
      headers = headers.set('X-Requested-With', 'XMLHttpRequest');
      headers = headers.set('Accept', '*/*');
    }

    return headers;
  }

  protected addAspNetCoreCultureHeader(headers: HttpHeaders): HttpHeaders {
    const cookieLangValue = this._storageService.getCookieValue('Abp.Localization.CultureName');
    if (cookieLangValue && headers && !headers.has('.AspNetCore.Culture')) {
      headers = headers.set('.AspNetCore.Culture', 'c=' + cookieLangValue + '|uic=' + cookieLangValue);
      headers = headers.set('Abp.Localization.CultureName', cookieLangValue);
    }

    return headers;
  }

  protected addAcceptLanguageHeader(headers: HttpHeaders): HttpHeaders {
    const cookieLangValue = this._storageService.getCookieValue('Abp.Localization.CultureName');
    if (cookieLangValue && headers && !headers.has('Accept-Language')) {
      headers = headers.set('Accept-Language', cookieLangValue);
    }

    return headers;
  }

  protected addTenantIdHeader(headers: HttpHeaders): HttpHeaders {
    const tenantIdCookieName = (window as any).eaf?.multiTenancy?.tenantIdCookieName || 'Abp-TenantId';
    const cookieTenantIdValue = this._storageService.getCookieValue(tenantIdCookieName);
    if (cookieTenantIdValue && headers && !headers.has(tenantIdCookieName)) {
      headers = headers.set(tenantIdCookieName, cookieTenantIdValue);
    }

    return headers;
  }

  protected addAuthorizationHeaders(headers: HttpHeaders): HttpHeaders {
    let authorizationHeaders = headers ? headers.getAll('Authorization') : null;
    if (!authorizationHeaders) {
      authorizationHeaders = [];
    }

    if (!this.itemExists(authorizationHeaders, (item: string) => item.startsWith('Bearer '))) {
      const token = this._tokenService.getToken();
      if (headers && token) {
        headers = headers.set('Authorization', 'Bearer ' + token);
      }
    }

    return headers;
  }
}
