import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export interface SdkError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
  retryAfter?: number;
}

@Injectable({
  providedIn: 'root',
})
export class ErrorMapperService {
  map(err: unknown): SdkError {
    if (err instanceof HttpErrorResponse) {
      return this.fromHttpErrorResponse(err);
    }

    if (err instanceof Error) {
      return {
        code: 'temporarily_unavailable',
        message: err.message || 'An unexpected error occurred.',
        retryable: true,
      };
    }

    return {
      code: 'temporarily_unavailable',
      message: 'An unexpected error occurred. Please try again later.',
      retryable: true,
    };
  }

  private fromHttpErrorResponse(response: HttpErrorResponse): SdkError {
    const body = response.error;
    const code = body?.error?.code ?? body?.code ?? this.defaultCode(response.status);
    const message = body?.error?.message ?? body?.message ?? this.defaultMessage(response.status, response.message);
    const retryable = (body?.error?.retryable ?? body?.retryable) || (response.status >= 500 || response.status === 429);
    const correlationId = body?.error?.correlationId ?? body?.correlationId;
    const retryAfterHeader = response.headers?.get('Retry-After');
    const retryAfter = retryAfterHeader ? parseInt(retryAfterHeader, 10) : undefined;

    return {
      code,
      message,
      retryable,
      correlationId,
      retryAfter,
    };
  }

  private defaultCode(status: number): string {
    if (status === 401) return 'not_authenticated';
    if (status === 403) return 'not_authorized';
    if (status === 409) return 'validation_failed';
    if (status === 429) return 'rate_limited';
    if (status === 503) return 'temporarily_unavailable';
    if (status >= 400 && status < 500) return 'validation_failed';
    return 'temporarily_unavailable';
  }

  private defaultMessage(status: number, fallback: string): string {
    if (status === 401) return 'Authentication required.';
    if (status === 403) return 'You do not have permission to perform this action.';
    if (status === 429) return 'Too many requests. Please try again later.';
    if (status === 503) return 'Service temporarily unavailable. Please try again later.';
    if (status >= 500) return 'An unexpected error occurred. Please try again later.';
    if (fallback?.startsWith('Http failure response')) return 'An unexpected error occurred. Please try again later.';
    return fallback || 'An unexpected error occurred. Please try again later.';
  }
}
