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

export interface PlayerProfile {
  username: string;
  avatarUrl: string | null;
}

export interface PrivacyPolicy {
  gameSlug: string;
  url: string;
  text: string;
  requiresConsent: boolean;
}

export interface PrivacyConsent {
  consented: boolean;
  policyVersion: string;
  consentedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class GameplayBridgeService implements GameplayBridge {
  private readonly gameplayUrl = '/api/services/app/Gameplay';

  private sessionId: string | null = null;
  private gameId: string | null = null;
  private gameSlug: string | null = null;
  private gameOrigin = environment.gameOrigin;
  private replyHandler?: (message: unknown) => void;
  private onSaveError?: () => void;
  private onMovePill?: (topPercent?: number, topPx?: number) => void;
  private onRewardedBreak?: (resolve: (rewarded: boolean) => void) => void;
  private readonly pillPositionKey = 'gamehub_pill_position';

  private isLoadingStarted = false;
  private isLoadingFinished = false;
  private isPlaying = false;
  private isAdRunning = false;
  private pendingLoadingFinished = false;
  private isInspectorMode = false;
  private inspectorSessionId: string | null = null;
  private readonly localSavePrefix = 'gamehub_save_';
  private readonly ignorePrefix = 'gamehub_ignore_';
  private readonly languageKey = 'gamehub_language';
  private readonly privacyConsentKey = 'gamehub_privacy_consent';
  private readonly cloudSaveUrl = '/api/services/app/CloudSave';
  private readonly playerAccountUrl = '/api/services/app/PlayerAccount';
  private readonly privacyUrl = '/api/services/app/Privacy/GetForGame';
  private readonly consentUrl = '/api/services/app/Privacy/GetConsent';

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

  setGame(slug: string): void {
    this.gameSlug = slug;
  }

  setGameOrigin(origin: string): void {
    this.gameOrigin = origin;
  }

  setReplyHandler(handler?: (message: unknown) => void): void {
    this.replyHandler = handler;
  }

  setInspectorMode(enabled: boolean, sessionId?: string): void {
    this.isInspectorMode = enabled;
    this.inspectorSessionId = sessionId ?? null;
  }

  setOnSaveError(handler?: () => void): void {
    this.onSaveError = handler;
  }

  setOnMovePill(handler?: (topPercent?: number, topPx?: number) => void): void {
    this.onMovePill = handler;
    const stored = this.getStoredPillPosition();
    if (stored && this.onMovePill) {
      this.onMovePill(stored.topPercent, stored.topPx);
    }
  }

  setOnRewardedBreak(handler?: (resolve: (rewarded: boolean) => void) => void): void {
    this.onRewardedBreak = handler;
  }

  async requestRewardedAd(): Promise<{ rewarded: boolean; adBlocked?: boolean }> {
    if (!this.gameId) {
      return { rewarded: false };
    }

    const result = await this.adBreak.requestRewarded(this.gameId, this.sessionId ?? undefined).toPromise();
    const adBlocked = Boolean(result?.adBlocked);
    const rewarded = Boolean(result?.completed && result?.rewardGranted && !adBlocked);
    return { rewarded, adBlocked };
  }

  private movePill(topPercent?: number, topPx?: number): void {
    this.storePillPosition({ topPercent, topPx });
    if (this.onMovePill) {
      this.onMovePill(topPercent, topPx);
    }
    this.reply({ channel: 'gamehub-bridge', action: 'pillMoved', payload: { topPercent, topPx } });
  }

  private getStoredPillPosition(): { topPercent?: number; topPx?: number } | null {
    const raw = this.safeLocalStorageGet(this.pillPositionKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as { topPercent?: number; topPx?: number };
    } catch {
      return null;
    }
  }

  private storePillPosition(position: { topPercent?: number; topPx?: number }): void {
    this.safeLocalStorageSet(this.pillPositionKey, JSON.stringify(position));
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
    this.reply({ channel: 'gamehub-bridge', action: 'adBreakMute' });

    if (this.gameId) {
      const result = await this.adBreak.requestCommercial(this.gameId, this.sessionId ?? undefined).toPromise();
      if (result?.adBlocked) {
        this.reply({ channel: 'gamehub-bridge', action: 'adBreakUnmute' });
      }
    }

    this.isAdRunning = false;
    this.commercialBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'adBreakUnmute' });
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
    this.reply({ channel: 'gamehub-bridge', action: 'adBreakMute' });

    let rewardGranted = false;
    if (this.onRewardedBreak) {
      rewardGranted = await new Promise<boolean>(resolve => this.onRewardedBreak?.(resolve));
    } else if (this.gameId) {
      const result = await this.adBreak.requestRewarded(this.gameId, this.sessionId ?? undefined).toPromise();
      rewardGranted = Boolean(result?.completed && result?.rewardGranted && !result?.adBlocked);
      if (result?.adBlocked) {
        this.reply({ channel: 'gamehub-bridge', action: 'adBreakUnmute' });
      }
    }

    this.isAdRunning = false;
    this.rewardedBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'adBreakUnmute' });
    this.reply({ channel: 'gamehub-bridge', action: 'rewardedBreakCompleted', payload: { success: rewardGranted } });
    return rewardGranted;
  }

  rewardedBreakCompleted(): void {
    this.sendEvent(GameplayEventType.RewardedBreakCompleted);
  }

  gameErrorCaptured(error: Error | string): void {
    if (this.isAdRunning) return;
    this.sendEvent(GameplayEventType.GameErrorCaptured, error.toString());
    this.reportError(error);
  }

  private reportError(error: Error | string): void {
    if (!this.sessionId || !this.gameId) return;
    const message = typeof error === 'string' ? error : error.message ?? error.toString();
    const stack = typeof error === 'string' ? '' : error.stack ?? '';
    this.http
      .post(`${this.gameplayUrl}/CaptureError`, {
        sessionId: this.sessionId,
        gameId: this.gameId,
        message,
        stackTrace: stack,
        source: 'game',
        severity: 'Error',
      })
      .subscribe({ error: () => {} });
  }

  gameMeasuredEvent(category: string, what: string, action: string): void {
    if (this.isAdRunning) return;
    this.sendEvent(GameplayEventType.GameMeasuredEvent, JSON.stringify({ category, what, action }));
  }

  measureFps(average: number, min: number): void {
    if (!this.sessionId) return;
    this.http.post(`${this.gameplayUrl}/UpdateFps`, { sessionId: this.sessionId, average, min }).subscribe();
  }

  async getPlayerData(requestId: string, keys?: string[]): Promise<void> {
    if (!this.gameId) {
      this.replyResponse(requestId, undefined, 'No game session');
      return;
    }

    try {
      const data = this.auth.isLoggedIn()
        ? { ...(await this.getCloudSave()), ...this.getLocalSave() }
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
      this.setLocalSave(merged);

      if (this.auth.isLoggedIn()) {
        const cloudData = this.filterIgnoreKeys(merged);
        const result = await this.saveCloudSave(cloudData);
        if (result && !result.saved) {
          this.onSaveError?.();
          this.replyResponse(requestId, { saved: false, message: result.message ?? 'Progresso local apenas' });
          return;
        }
      }

      this.replyResponse(requestId, { saved: true });
    } catch (err) {
      this.replyResponse(requestId, { saved: false, message: err instanceof Error ? err.message : 'Storage error' });
    }
  }

  async save(data: Record<string, unknown>): Promise<void> {
    await this.setPlayerData('', data);
  }

  async load(): Promise<Record<string, unknown>> {
    if (!this.gameId) {
      return {};
    }

    try {
      const data = this.auth.isLoggedIn()
        ? { ...(await this.getCloudSave()), ...this.getLocalSave() }
        : this.getLocalSave();
      return data ?? {};
    } catch {
      return this.getLocalSave();
    }
  }

  login(): void {
    if (typeof window !== 'undefined') {
      const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
      window.location.href = `/login?returnUrl=${returnUrl}`;
    }
  }

  async getUser(requestId: string): Promise<void> {
    if (!this.auth.isLoggedIn()) {
      this.replyResponse(requestId, undefined, 'No user');
      return;
    }

    try {
      const profile = await firstValueFrom(
        this.http.post<{ result?: PlayerProfile }>(`${this.playerAccountUrl}/GetPlayerProfile`, {})
          .pipe(map(response => this.unwrap<{ result?: PlayerProfile }>(response)?.result ?? this.unwrap<PlayerProfile>(response)))
      );
      this.replyResponse(requestId, { username: profile?.username ?? '', avatarUrl: profile?.avatarUrl ?? null });
    } catch {
      this.replyResponse(requestId, undefined, 'No user');
    }
  }

  async getToken(requestId: string): Promise<void> {
    if (!this.gameId) {
      this.replyResponse(requestId, undefined, 'No game session');
      return;
    }

    if (!this.auth.isLoggedIn()) {
      this.replyResponse(requestId, undefined, 'No token');
      return;
    }

    try {
      const result = await firstValueFrom(
        this.http.post<{ result?: { token: string } }>(`${this.playerAccountUrl}/GetToken`, { gameId: this.gameId })
          .pipe(map(response => this.unwrap<{ result?: { token: string } }>(response)?.result ?? this.unwrap<{ token: string }>(response)))
      );
      if (result?.token) {
        this.replyResponse(requestId, { token: result.token });
      } else {
        this.replyResponse(requestId, undefined, 'No token');
      }
    } catch {
      this.replyResponse(requestId, undefined, 'No token');
    }
  }

  async getPrivacyPolicy(requestId: string): Promise<void> {
    if (!this.gameSlug) {
      this.replyResponse(requestId, undefined, 'No game session');
      return;
    }

    try {
      const result = await firstValueFrom(
        this.http.get<{ result?: PrivacyPolicy }>(`${this.privacyUrl}?gameSlug=${this.gameSlug}`)
          .pipe(map(response => this.unwrap<{ result?: PrivacyPolicy }>(response)?.result ?? this.unwrap<PrivacyPolicy>(response)))
      );

      this.replyResponse(requestId, { url: result?.url ?? '', text: result?.text ?? '', requiresConsent: result?.requiresConsent ?? false });
    } catch {
      this.replyResponse(requestId, undefined, 'Privacy policy unavailable');
    }
  }

  async getLanguage(requestId: string): Promise<void> {
    const fallback = this.safeLocalStorageGet(this.languageKey) ?? 'en-US';

    if (!this.auth.isLoggedIn()) {
      this.replyResponse(requestId, { language: fallback });
      return;
    }

    try {
      const result = await firstValueFrom(
        this.http.post<{ result?: string }>(`${this.playerAccountUrl}/GetLanguage`, {})
          .pipe(map(response => this.unwrap<{ result?: string }>(response)?.result ?? this.unwrap<string>(response)))
      );
      this.replyResponse(requestId, { language: result || fallback });
    } catch {
      this.replyResponse(requestId, { language: fallback });
    }
  }

  async setLanguage(requestId: string, language: string): Promise<void> {
    this.safeLocalStorageSet(this.languageKey, language);

    if (this.auth.isLoggedIn()) {
      try {
        await firstValueFrom(this.http.post(`${this.playerAccountUrl}/SetLanguage`, { language }));
      } catch {
        // ignore backend failures; localStorage is the source of truth for anonymous users
      }
    }

    this.replyResponse(requestId, { language });
    this.reply({ channel: 'gamehub-bridge', action: 'languageChanged', payload: { language } });
  }

  getStoredLanguage(): string {
    return this.safeLocalStorageGet(this.languageKey) ?? 'en-US';
  }

  async getPrivacyConsent(requestId: string): Promise<void> {
    const fallback = this.getStoredPrivacyConsent();

    if (!this.gameId) {
      this.replyResponse(requestId, fallback);
      return;
    }

    if (this.auth.isLoggedIn()) {
      try {
        const result = await firstValueFrom(
          this.http.post<{ result?: PrivacyConsent }>(this.consentUrl, { gameId: this.gameId })
            .pipe(map(response => this.unwrap<{ result?: PrivacyConsent }>(response)?.result ?? this.unwrap<PrivacyConsent>(response)))
        );
        this.replyResponse(requestId, result ?? fallback);
        return;
      } catch {
        // Fallback to localStorage on API errors.
      }
    }

    this.replyResponse(requestId, fallback);
  }

  async setPrivacyConsent(requestId: string, consented: boolean, policyVersion: string): Promise<void> {
    this.storePrivacyConsent({ consented, policyVersion, consentedAt: new Date().toISOString() });

    if (this.auth.isLoggedIn() && this.gameId) {
      try {
        await firstValueFrom(this.http.post(`${this.privacyUrl.replace('/GetForGame', '')}/SaveConsent`, {
          gameId: this.gameId,
          policyVersion
        }));
      } catch {
        // ignore backend failures; localStorage is the source of truth for anonymous users
      }
    }

    this.replyResponse(requestId, { consented, policyVersion });
  }

  getStoredPrivacyConsent(): PrivacyConsent {
    const raw = this.safeLocalStorageGet(this.getPrivacyConsentKey());
    if (!raw) {
      return { consented: false, policyVersion: '' };
    }
    try {
      return JSON.parse(raw) as PrivacyConsent;
    } catch {
      return { consented: false, policyVersion: '' };
    }
  }

  private storePrivacyConsent(consent: PrivacyConsent): void {
    this.safeLocalStorageSet(this.getPrivacyConsentKey(), JSON.stringify(consent));
  }

  private getPrivacyConsentKey(): string {
    return this.gameSlug ? `${this.privacyConsentKey}_${this.gameSlug}` : this.privacyConsentKey;
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
      case 'measureFps':
        this.measureFps(
          (payload?.['average'] as number) ?? 0,
          (payload?.['min'] as number) ?? 0,
        );
        break;
      case 'getPlayerData':
      case 'load':
        void this.getPlayerData(requestId ?? '', payload?.['keys'] as string[] | undefined);
        break;
      case 'setPlayerData':
      case 'save':
        void this.setPlayerData(requestId ?? '', (payload?.['data'] as Record<string, unknown>) ?? {});
        break;
      case 'login':
        this.login();
        break;
      case 'getUser':
        void this.getUser(requestId ?? '');
        break;
      case 'getToken':
        void this.getToken(requestId ?? '');
        break;
      case 'getPrivacyPolicy':
        void this.getPrivacyPolicy(requestId ?? '');
        break;
      case 'getLanguage':
        void this.getLanguage(requestId ?? '');
        break;
      case 'setLanguage':
        void this.setLanguage(requestId ?? '', (payload?.['language'] as string) ?? 'en-US');
        break;
      case 'getPrivacyConsent':
        void this.getPrivacyConsent(requestId ?? '');
        break;
      case 'setPrivacyConsent':
        void this.setPrivacyConsent(
          requestId ?? '',
          (payload?.['consented'] as boolean) ?? true,
          (payload?.['policyVersion'] as string) ?? ''
        );
        break;
      case 'movePill':
        this.movePill(
          payload?.['topPercent'] as number | undefined,
          payload?.['topPx'] as number | undefined
        );
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
      if (typeof localStorage === 'undefined') return null;
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private safeLocalStorageSet(key: string, value: string): void {
    try {
      if (typeof localStorage === 'undefined') return;
      localStorage.setItem(key, value);
    } catch {
      // Ignore in private/incognito mode.
    }
  }

  private filterIgnoreKeys(data: Record<string, unknown>): Record<string, unknown> {
    const filtered: Record<string, unknown> = {};
    for (const key of Object.keys(data)) {
      if (!key.startsWith(this.ignorePrefix)) {
        filtered[key] = data[key];
      }
    }
    return filtered;
  }

  private getDeviceId(): string {
    try {
      if (typeof localStorage === 'undefined') return '';
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
          const result = this.unwrap<{ data?: string; Data?: string }>(response);
          const payload = result?.data ?? result?.Data;
          if (payload) {
            try {
              return JSON.parse(payload) as Record<string, unknown>;
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

  private saveCloudSave(data: Record<string, unknown>): Promise<{ saved: boolean; message?: string } | null> {
    const request$ = this.http
      .post<{ saved?: boolean; message?: string; result?: { saved?: boolean; message?: string } }>(`${this.cloudSaveUrl}/Save`, {
        gameId: this.gameId,
        deviceId: this.getDeviceId(),
        data: JSON.stringify(data),
      })
      .pipe(
        map(response => {
          const result = this.unwrap<{ saved?: boolean; message?: string }>(response);
          return { saved: result?.saved ?? true, message: result?.message };
        }),
        catchError(() => of({ saved: false, message: 'Progresso local apenas' })),
      );
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

    if (this.isInspectorMode && this.inspectorSessionId) {
      this.http.post('/api/services/app/Inspector/LogSdkEvent', {
        sessionId: this.inspectorSessionId,
        eventType: GameplayEventType[eventType],
        payload,
        sequenceNumber: Date.now(),
      }).subscribe();
    }
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
