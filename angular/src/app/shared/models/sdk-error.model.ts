export interface SdkError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
}

const transientStatusCodes = new Set([408, 425, 429, 500, 502, 503, 504]);
const safeMethods = new Set(['GET', 'HEAD', 'OPTIONS']);

export function normalizeSdkError(payload: unknown, status: number, correlationId?: string): SdkError {
  if (
    payload &&
    typeof payload === 'object' &&
    'code' in payload &&
    typeof (payload as SdkError).code === 'string' &&
    'message' in payload &&
    typeof (payload as SdkError).message === 'string'
  ) {
    return {
      code: (payload as SdkError).code,
      message: (payload as SdkError).message,
      retryable: (payload as SdkError).retryable ?? transientStatusCodes.has(status),
      correlationId: (payload as SdkError).correlationId ?? correlationId,
    };
  }

  const code = status === 401
    ? 'not_authenticated'
    : status === 403
      ? 'not_authorized'
      : status === 429
        ? 'rate_limited'
        : status >= 500
          ? 'temporarily_unavailable'
          : 'validation_failed';

  return {
    code,
    message: 'An unexpected error occurred. Please try again later.',
    retryable: transientStatusCodes.has(status),
    correlationId,
  };
}

export function isRetryableRequest(method: string): boolean {
  return safeMethods.has(method.toUpperCase());
}

export function parseRetryAfter(header: string | null): number | null {
  if (!header) return null;
  const seconds = parseInt(header, 10);
  return Number.isNaN(seconds) ? null : seconds;
}
