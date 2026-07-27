import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, delay, of, retry, throwError } from 'rxjs';
import { TokenService } from '../auth/token.service';
import { isRetryableRequest, normalizeSdkError, parseRetryAfter } from '../../shared/models/sdk-error.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  return next(req).pipe(
    retry({
      count: isRetryableRequest(req.method) ? 2 : 0,
      delay: (error, retryCount) => {
        if (error instanceof HttpErrorResponse) {
          const retryAfter = parseRetryAfter(error.headers.get('Retry-After'));
          if (retryAfter !== null) {
            return of(null).pipe(delay(retryAfter * 1000));
          }
        }
        return of(null).pipe(delay(Math.min(1000 * Math.pow(2, retryCount - 1), 4000)));
      },
    }),
    catchError(error => {
      const sdkError = error instanceof HttpErrorResponse
        ? normalizeSdkError(
            error.error,
            error.status,
            error.headers.get('X-Correlation-ID') ?? undefined
          )
        : {
            code: 'temporarily_unavailable',
            message: 'An unexpected error occurred.',
            retryable: true,
          };

      if (sdkError.code === 'not_authenticated' || error?.status === 401) {
        tokenService.clearToken();
        void router.navigate(['/login']);
      }

      const enriched = new HttpErrorResponse({
        error: sdkError,
        headers: error.headers,
        status: error.status,
        statusText: error.statusText,
        url: error.url ?? undefined,
      });

      return throwError(() => enriched);
    })
  );
};
