import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AdBreakService } from './ad-break.service';

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
  private replyHandler?: (message: unknown) => void;

  constructor(
    private http: HttpClient,
    private adBreak: AdBreakService,
  ) {}

  setSession(sessionId: string, gameId: string): void {
    this.sessionId = sessionId;
    this.gameId = gameId;
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
    this.sendEvent(GameplayEventType.GameLoadingStarted);
  }

  gameLoadingFinished(): void {
    this.sendEvent(GameplayEventType.GameLoadingFinished);
  }

  gameplayStart(): void {
    this.sendEvent(GameplayEventType.GameplayStarted);
  }

  gameplayStop(): void {
    this.sendEvent(GameplayEventType.GameplayStopped);
  }

  async commercialBreakRequested(): Promise<void> {
    this.sendEvent(GameplayEventType.CommercialBreakRequested);
    if (this.gameId) {
      await this.adBreak.requestCommercial(this.gameId).toPromise();
    }
    this.commercialBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'commercialBreakCompleted' });
  }

  commercialBreakCompleted(): void {
    this.sendEvent(GameplayEventType.CommercialBreakCompleted);
  }

  async rewardedBreakRequested(): Promise<boolean> {
    this.sendEvent(GameplayEventType.RewardedBreakRequested);
    let completed = false;
    if (this.gameId) {
      const result = await this.adBreak.requestRewarded(this.gameId).toPromise();
      completed = result?.completed ?? false;
    }
    this.rewardedBreakCompleted();
    this.reply({ channel: 'gamehub-bridge', action: 'rewardedBreakCompleted', payload: { success: completed } });
    return completed;
  }

  rewardedBreakCompleted(): void {
    this.sendEvent(GameplayEventType.RewardedBreakCompleted);
  }

  gameErrorCaptured(error: Error | string): void {
    this.sendEvent(GameplayEventType.GameErrorCaptured, error.toString());
  }

  gameMeasuredEvent(category: string, what: string, action: string): void {
    this.sendEvent(GameplayEventType.GameMeasuredEvent, JSON.stringify({ category, what, action }));
  }

  handleMessage(event: MessageEvent<unknown>): void {
    const data = event.data as Record<string, unknown> | undefined;
    if (!data || typeof data !== 'object' || data['channel'] !== 'gamehub-bridge') {
      return;
    }
    const action = data['action'] as string;
    const payload = data['payload'] as Record<string, unknown> | undefined;

    switch (action) {
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
    }
  }

  private reply(message: unknown): void {
    if (this.replyHandler) {
      this.replyHandler(message);
    }
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

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
