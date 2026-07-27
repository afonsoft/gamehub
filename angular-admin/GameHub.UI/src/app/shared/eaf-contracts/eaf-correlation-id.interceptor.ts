import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { retry } from 'rxjs/operators';
import { normalizeEafError } from './eaf-contracts';

@Injectable()
export class EafCorrelationIdInterceptor implements HttpInterceptor {
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const correlationId = request.headers.get('X-Correlation-ID') ?? crypto.randomUUID();
    const requestWithCorrelationId = request.clone({
      setHeaders: { 'X-Correlation-ID': correlationId },
    });

    const response = next.handle(requestWithCorrelationId);
    if (!['GET', 'HEAD', 'OPTIONS'].includes(request.method.toUpperCase())) {
      return response;
    }

    return response.pipe(
      retry({
        count: 1,
        delay: (error: unknown) => {
          if (!(error instanceof HttpErrorResponse) || !normalizeEafError(error.error, error.status, correlationId).retryable) {
            return throwError(() => error);
          }

          return new Observable<void>(subscriber => {
            const timeout = setTimeout(() => {
              subscriber.next();
              subscriber.complete();
            }, 250);

            return () => clearTimeout(timeout);
          });
        },
      }),
    );
  }
}
