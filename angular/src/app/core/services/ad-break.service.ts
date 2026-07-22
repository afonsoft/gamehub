import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface CommercialBreakResult {
  completed: boolean;
  durationSeconds: number;
}

export interface RewardedBreakResult {
  completed: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdBreakService {
  private readonly apiUrl = '/api/services/app/AdBreak';

  constructor(private http: HttpClient) {}

  requestCommercial(gameId: string): Observable<CommercialBreakResult> {
    return this.http
      .post<CommercialBreakResult | { result?: CommercialBreakResult }>(`${this.apiUrl}/RequestCommercialBreak`, { gameId })
      .pipe(map(response => this.unwrap<CommercialBreakResult>(response)));
  }

  requestRewarded(gameId: string): Observable<RewardedBreakResult> {
    return this.http
      .post<RewardedBreakResult | { result?: RewardedBreakResult }>(`${this.apiUrl}/RequestRewardedBreak`, { gameId })
      .pipe(map(response => this.unwrap<RewardedBreakResult>(response)));
  }

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
