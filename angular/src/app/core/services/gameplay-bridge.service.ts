import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom, Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AdBreakService } from './ad-break.service';
import { AuthService } from '../auth/auth.service';
import { TokenService } from '../auth/token.service';
import { environment } from '../../../environments/environment';

export enum GameplayEventType {
  GameLoadingStarted = 0,
  GameLoadingFinished = 1,
  GameplayStarted = 2,
  GameplayStopped = 3,
  CommercialBreakRequested = 4,
  CommercialBreakCompleted = 5,
  RewardedBreakRequested = 6,
  RewardedBreakCompleted = 7,
  GameErrorCaptured = 8,
  GameMeasuredEvent = 9,
}

export interface GameplayBridge {
  gameLoadingStarted(): void;
  gameLoadingFinished(): void;
  gameplayStart(): void;
  gameplayStop(): void;
  commercialBreakRequested(): Promise<void>;
  commercialBreakCompleted(): void;
  rewardedBreakRequested(): Promise<boolean>;
  rewardedBreakCompleted(): void;
  gameErrorCaptured(error: Error | string): void;
  gameMeasuredEvent(category: string, what: string, action: string): void;
}

export interface StartPlaySessionInput {
  gameId: string;
  deviceType: string;
  browser: string;
  referrer?: string;
}

export interface PlaySession {
  sessionId: string;
  gameId: string;
  startedAt: string;
}

@Injectable({ providedIn: 'root' })
export class GameplayBridgeService implements GameplayBridge {
  private readonly gameplayUrl = '/api/services/app/Gameplay';

  private sessionId: string | null = null;
  private gameId: string | null = null;
  private gameOrigin = environment.gameOrigin;
  private replyHandler?: (message: unknown) => void;

  private isLoadingStarted = false;
  private isLoadingFinished = false;
  private isPlaying = false;
  private isAdRunning = false;
  private pendingLoadingFinished = false;
  private readonly localSavePrefix = 'gamehub_save_';
  private readonly ignorePrefix = 'gamehub_ignore_';
  private readonly cloudSaveUrl = '/api/services/app/CloudSave';

  constructor(
    private http: HttpClient,
    private adBreak: AdBreakService,
    private auth: AuthService,
    private token: TokenService,
  ) {}

  setSession(sessionId: string, gameId: string): void {
    this.sessionId = sessionId;
    this.gameId = gameId;

    if (this.pendingLoadingFinished) {
      this.pendingLoadingFinished = false;
      this.sendLoadingFinished();
    }
  }

  setGameOrigin(origin: string): void {
    this.gameOrigin = origin;
  }

  setReplyHandler(handler?: (message: unknown) => void): void {
    this.replyHandler = handler;
  }

  startSession(input: StartPlaySessionInput): Observable<PlaySession> {
    return this.http
      .post<PlaySession | { result?: PlaySession }>(`${this.gameplayUrl}/StartSession`, input)
      .pipe(map(response => this.unwrap<PlaySession>(response)));
  }

  stopSession(sessionId: string): Observable<unknown> {
    return this.http.post(`${this.gameplayUrl}/StopSession`, { sessionId });
  }

  gameLoadingStarted(): void {
    if (this.isLoadingStarted) return;
    this.isLoadingStarted = true;
    this.sendEvent(GameplayEventType.GameLoadingStarted);
  }

  gameLoadingFinished(): void {
    if (this.isLoadingFinished) return;
    this.isLoadingFinished = true;

    if (this.sessionId && this.gameId) {
      this.sendLoadingFinished();
    } else {
      this.pendingLoadingFinished = true;
    }
  }

  gameplayStart(): void {
    if (this.isAdRunning || this.isPlaying) return;
    this.isPlaying = true;
    this.sendEvent(GameplayEventType.GameplayStarted);
  }

  gameplayStop(): void {
    if (this.isAdRunning || !this.isPlaying) return;
    this.isPlaying = false;
    this.sendEvent(GameplayEventType.GameplayStopped);
  }

  async commercialBreakRequested(): Promise<void> {
    if (this.isAdRunning) return;
    this.isAdRunning = true;
    this.isPlaying = false;
    this.sendEvent(GameplayEventType.CommercialBreakRequested);

    if (this.gameId) {
      await this.adBreak.requestCommercial(this.gameId).toPromise();
    }

    this.isAdRunning = false;
    this.commercialBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'commercialBreakCompleted' });
  }

  commercialBreakCompleted(): void {
    this.sendEvent(GameplayEventType.CommercialBreakCompleted);
  }

  async rewardedBreakRequested(): Promise<boolean> {
    if (this.isAdRunning) return false;
    this.isAdRunning = true;
    this.isPlaying = false;
    this.sendEvent(GameplayEventType.RewardedBreakRequested);

    let completed = false;
    if (this.gameId) {
      const result = await this.adBreak.requestRewarded(this.gameId).toPromise();
      completed = result?.completed ?? false;
    }

    this.isAdRunning = false;
    this.rewardedBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'rewardedBreakCompleted', payload: { success: completed } });
    return completed;
  }

  rewardedBreakCompleted(): void {
    this.sendEvent(GameplayEventType.RewardedBreakCompleted);
  }

  gameErrorCaptured(error: Error | string): void {
    if (this.isAdRunning) return;
    this.sendEvent(GameplayEventType.GameErrorCaptured, error.toString());
  }

  gameMeasuredEvent(category: string, what: string, action: string): void {
    if (this.isAdRunning) return;
    this.sendEvent(GameplayEventType.GameMeasuredEvent, JSON.stringify({ category, what, action }));
  }

  async getPlayerData(requestId: string, keys?: string[]): Promise<void> {
    if (!this.gameId) {
      this.replyResponse(requestId, undefined, 'No game session');
      return;
    }

    try {
      const data = this.auth.isLoggedIn()
        ? await this.getCloudSave()
        : this.getLocalSave();

      if (keys && keys.length > 0 && data) {
        const filtered: Record<string, unknown> = {};
        for (const key of keys) {
          if (Object.prototype.hasOwnProperty.call(data, key)) {
            filtered[key] = data[key];
          }
        }
        this.replyResponse(requestId, filtered);
      } else {
        this.replyResponse(requestId, data ?? {});
      }
    } catch (err) {
      this.replyResponse(requestId, {}, err instanceof Error ? err.message : 'Storage error');
    }
  }

  async setPlayerData(requestId: string, data: Record<string, unknown>): Promise<void> {
    if (!this.gameId) {
      this.replyResponse(requestId, undefined, 'No game session');
      return;
    }

    try {
      const merged = { ...this.getLocalSave(), ...data };
      if (this.auth.isLoggedIn()) {
        await this.saveCloudSave(merged);
      }
      this.setLocalSave(merged);
      this.replyResponse(requestId, { success: true });
    } catch (err) {
      this.replyResponse(requestId, undefined, err instanceof Error ? err.message : 'Storage error');
    }
  }

  login(requestId: string): void {
    const token = this.token.getToken();
    const username = this.token.getUserName();
    if (token && username) {
      this.replyResponse(requestId, { token, username });
      return;
    }
    this.replyResponse(requestId, undefined, 'Login required');
  }

  getUser(requestId: string): void {
    const username = this.token.getUserName();
    if (username) {
      this.replyResponse(requestId, { username, avatarUrl: null });
    } else {
      this.replyResponse(requestId, undefined, 'No user');
    }
  }

  getToken(requestId: string): void {
    const token = this.token.getToken();
    if (token) {
      this.replyResponse(requestId, { token });
    } else {
      this.replyResponse(requestId, undefined, 'No token');
    }
  }

  handleMessage(event: MessageEvent<unknown>): void {
    if (event.origin !== this.gameOrigin) {
      return;
    }

    const data = event.data as Record<string, unknown> | undefined;
    if (!data || typeof data !== 'object' || data['channel'] !== 'gamehub-bridge') {
      return;
    }
    const action = data['action'] as string;
    const payload = data['payload'] as Record<string, unknown> | undefined;
    const requestId = data['requestId'] as string | undefined;

    switch (action) {
      case 'init':
        this.reply({ channel: 'gamehub-bridge', action: 'initAck', requestId });
        break;
      case 'gameLoadingStarted':
        this.gameLoadingStarted();
        break;
      case 'gameLoadingFinished':
        this.gameLoadingFinished();
        break;
      case 'gameplayStart':
        this.gameplayStart();
        break;
      case 'gameplayStop':
        this.gameplayStop();
        break;
      case 'commercialBreakRequested':
        void this.commercialBreakRequested();
        break;
      case 'rewardedBreakRequested':
        void this.rewardedBreakRequested();
        break;
      case 'gameErrorCaptured':
        this.gameErrorCaptured((payload?.['error'] as string | Error) ?? 'unknown');
        break;
      case 'gameMeasuredEvent':
        this.gameMeasuredEvent(
          (payload?.['category'] as string) ?? '',
          (payload?.['what'] as string) ?? '',
          (payload?.['action'] as string) ?? '',
        );
        break;
      case 'getPlayerData':
        void this.getPlayerData(requestId ?? '', payload?.['keys'] as string[] | undefined);
        break;
      case 'setPlayerData':
        void this.setPlayerData(requestId ?? '', (payload?.['data'] as Record<string, unknown>) ?? {});
        break;
      case 'login':
        this.login(requestId ?? '');
        break;
      case 'getUser':
        this.getUser(requestId ?? '');
        break;
      case 'getToken':
        this.getToken(requestId ?? '');
        break;
    }
  }

  private reply(message: unknown): void {
    if (this.replyHandler) {
      this.replyHandler(message);
    }
  }

  private replyResponse(requestId: string, data?: unknown, error?: string): void {
    const payload: { data?: unknown; error?: string } = {};
    if (error) {
      payload.error = error;
    } else {
      payload.data = data;
    }
    this.reply({ channel: 'gamehub-bridge', action: 'response', requestId, payload });
  }

  private getSaveKey(): string {
    return `${this.localSavePrefix}${this.gameId ?? 'unknown'}`;
  }

  private getLocalSave(): Record<string, unknown> {
    const raw = this.safeLocalStorageGet(this.getSaveKey());
    if (!raw) return {};
    try {
      return JSON.parse(raw) as Record<string, unknown>;
    } catch {
      return {};
    }
  }

  private setLocalSave(data: Record<string, unknown>): void {
    this.safeLocalStorageSet(this.getSaveKey(), JSON.stringify(data));
  }

  private safeLocalStorageGet(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private safeLocalStorageSet(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      // Ignore in private/incognito mode.
    }
  }

  private getDeviceId(): string {
    try {
      let id = localStorage.getItem('gamehub-device-id');
      if (!id) {
        id = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
        localStorage.setItem('gamehub-device-id', id);
      }
      return id;
    } catch {
      return '';
    }
  }

  private getCloudSave(): Promise<Record<string, unknown>> {
    const request$ = this.http
      .post<Record<string, unknown>>(`${this.cloudSaveUrl}/Get`, {
        gameId: this.gameId,
        deviceId: this.getDeviceId(),
      })
      .pipe(
        map(response => {
          const result = this.unwrap<{ Data?: string }>(response);
          if (result?.Data) {
            try {
              return JSON.parse(result.Data) as Record<string, unknown>;
            } catch {
              return {};
            }
          }
          return {};
        }),
        catchError(() => of(this.getLocalSave())),
      );
    return firstValueFrom(request$);
  }

  private saveCloudSave(data: Record<string, unknown>): Promise<unknown> {
    const request$ = this.http
      .post(`${this.cloudSaveUrl}/Save`, {
        gameId: this.gameId,
        deviceId: this.getDeviceId(),
        data: JSON.stringify(data),
      })
      .pipe(catchError(() => of(null)));
    return firstValueFrom(request$);
  }

  private sendEvent(eventType: GameplayEventType, payload?: string): void {
    if (!this.sessionId || !this.gameId) {
      return;
    }
    const body = {
      sessionId: this.sessionId,
      gameId: this.gameId,
      eventType,
      eventName: GameplayEventType[eventType],
      payloadJson: payload,
    };
    this.http.post(`${this.gameplayUrl}/Event`, body).subscribe();
  }

  private sendLoadingFinished(): void {
    this.sendEvent(GameplayEventType.GameLoadingFinished);
  }

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
