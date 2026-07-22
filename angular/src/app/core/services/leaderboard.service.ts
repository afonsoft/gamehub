import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface LeaderboardEntry {
  rank: number;
  userId: number;
  displayName: string;
  score: number;
  updatedAt: string;
}

export interface LeaderboardList {
  items: LeaderboardEntry[];
}

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly apiUrl = '/api/services/app/Leaderboard';

  constructor(private http: HttpClient) {}

  getTop(gameId: string, take = 10): Observable<LeaderboardEntry[]> {
    const params = new HttpParams().set('gameId', gameId).set('take', take.toString());
    return this.http
      .get<LeaderboardEntry[] | { result?: LeaderboardEntry[] }>(`${this.apiUrl}/GetTop`, { params })
      .pipe(map(response => this.unwrap<LeaderboardEntry[]>(response)));
  }

  submitScore(gameId: string, score: number): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/SubmitScore`, { gameId, score });
  }

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
