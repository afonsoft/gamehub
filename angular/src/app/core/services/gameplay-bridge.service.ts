import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class GameplayBridgeService {
  private readonly gameplayUrl = '/api/services/app/Gameplay';
  private readonly leaderboardUrl = '/api/services/app/Leaderboard';

  constructor(private http: HttpClient) {}

  startSession(gameId: string): Observable<{ sessionId: string }> {
    return this.http.post<{ sessionId: string }>(`${this.gameplayUrl}/StartSession`, { gameId, deviceType: 'Desktop' });
  }

  sendEvent(sessionId: string, eventType: string, payload: any = {}): Observable<any> {
    return this.http.post(`${this.gameplayUrl}/Event`, { sessionId, eventType, payload });
  }

  submitScore(gameId: string, score: number): Observable<any> {
    return this.http.post(`${this.leaderboardUrl}/SubmitScore`, { gameId, score });
  }
}
