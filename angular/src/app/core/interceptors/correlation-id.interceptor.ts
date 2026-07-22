import { HttpInterceptorFn } from '@angular/common/http';

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.headers.has('X-Correlation-ID')) {
    const correlationId = typeof crypto !== 'undefined' && crypto.randomUUID
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(36).slice(2)}`;
    req = req.clone({
      setHeaders: { 'X-Correlation-ID': correlationId },
    });
  }

  return next(req);
};
