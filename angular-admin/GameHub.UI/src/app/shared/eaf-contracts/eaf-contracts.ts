export interface EafError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
}

export interface ContextualChatMessage {
  id?: number;
  message?: string;
  userId?: number;
  conversationId?: string;
  gameId?: string;
  matchId?: string;
  contextType?: string;
  clientMessageId?: string;
  creationTime?: string;
}

export interface ChatContext {
  conversationId?: string;
  gameId?: string;
  matchId?: string;
  contextType?: string;
}

export interface EafNotificationMetadata {
  version: number;
  gameId?: string;
  matchId?: string;
  inviteId?: string;
  severity?: string;
  expiresAtUtc?: string;
}

export interface RateLimitDecision {
  allowed: boolean;
  retryAfterSeconds: number;
  resetAt?: string;
}

export interface RateLimitContract {
  allowed: boolean;
  count: number;
  limit: number;
  resetAt: string;
  retryAfterSeconds: number;
}

export interface ModerationAuditContract {
  id: string;
  action: string;
  moderatorUserId?: number;
  tenantId?: number;
  resourceId?: string;
  resourceType?: string;
  reason?: string;
  createdAt: string;
}

const transientStatusCodes = new Set([408, 425, 429, 500, 502, 503, 504]);

export function normalizeEafError(payload: unknown, status: number, correlationId?: string): EafError {
  if (isEafError(payload)) {
    return {
      ...payload,
      correlationId: payload.correlationId ?? correlationId,
    };
  }

  return {
    code: status === 401 ? 'not_authenticated' : status === 403 ? 'not_authorized' : 'temporarily_unavailable',
    message: 'Não foi possível concluir a operação.',
    retryable: transientStatusCodes.has(status),
    correlationId,
  };
}

function isEafError(payload: unknown): payload is EafError {
  if (typeof payload !== 'object' || payload === null) {
    return false;
  }

  const value = payload as Partial<EafError>;
  return typeof value.code === 'string' && typeof value.message === 'string' && typeof value.retryable === 'boolean';
}
