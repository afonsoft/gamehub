import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { GameCard } from './game-catalog.service';

const FAVORITES_KEY = 'gamehub-favorites';
const RECENT_KEY = 'gamehub-recent';

export interface PlayerFavorite {
  gameId: string;
  game: GameCard;
  createdAt: string;
}

export interface PlayerRecentGame {
  gameId: string;
  game: GameCard;
  lastPlayedAt: string;
  totalSessions: number;
}

export interface PlayerData {
  favoriteIds: string[];
  recentIds: string[];
}

@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly apiUrl = '/api/services/app/PlayerAccount';

  constructor(private http: HttpClient) {}

  getFavorites(): Observable<PlayerFavorite[]> {
    return this.http
      .get<PlayerFavorite[]>(`${this.apiUrl}/GetFavorites`)
      .pipe(map(response => this.unwrap(response) ?? []));
  }

  toggleFavorite(gameId: string, isAuthenticated: boolean): Observable<boolean> {
    if (!isAuthenticated) {
      const favorites = this.getLocalFavorites();
      const index = favorites.indexOf(gameId);
      if (index >= 0) {
        favorites.splice(index, 1);
      } else {
        favorites.unshift(gameId);
      }
      this.saveLocalFavorites(favorites);
      return of(index < 0);
    }

    return this.http
      .post<boolean>(`${this.apiUrl}/ToggleFavorite`, { gameId })
      .pipe(map(response => this.unwrap(response) ?? false));
  }

  getRecent(max = 20): Observable<PlayerRecentGame[]> {
    const params = new HttpParams().set('Max', max.toString());
    return this.http
      .get<PlayerRecentGame[]>(`${this.apiUrl}/GetRecent`, { params })
      .pipe(map(response => this.unwrap(response) ?? []));
  }

  trackPlay(gameId: string, isAuthenticated: boolean): Observable<void> {
    if (!isAuthenticated) {
      this.addLocalRecent(gameId);
      return of(undefined);
    }

    return this.http.post<void>(`${this.apiUrl}/TrackPlay`, { gameId });
  }

  mergeLocalData(): Observable<void> {
    const data = this.getLocalData();
    if (!data.favoriteIds.length && !data.recentIds.length) {
      return of(undefined);
    }
    return this.http.post<void>(`${this.apiUrl}/MergeLocalData`, data);
  }

  getLocalFavorites(): string[] {
    return this.readJson<string[]>(FAVORITES_KEY) ?? [];
  }

  getLocalRecent(): string[] {
    return this.readJson<string[]>(RECENT_KEY) ?? [];
  }

  getLocalData(): PlayerData {
    return { favoriteIds: this.getLocalFavorites(), recentIds: this.getLocalRecent() };
  }

  clearLocalData(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(FAVORITES_KEY);
      localStorage.removeItem(RECENT_KEY);
    }
  }

  private addLocalRecent(gameId: string): void {
    const recent = this.getLocalRecent().filter(id => id !== gameId);
    recent.unshift(gameId);
    this.saveLocalRecent(recent.slice(0, 50));
  }

  private saveLocalFavorites(favorites: string[]): void {
    this.writeJson(FAVORITES_KEY, favorites);
  }

  private saveLocalRecent(recent: string[]): void {
    this.writeJson(RECENT_KEY, recent);
  }

  private readJson<T>(key: string): T | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem(key);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as T;
    } catch {
      return null;
    }
  }

  private writeJson(key: string, value: unknown): void {
    if (typeof localStorage === 'undefined') return;
    localStorage.setItem(key, JSON.stringify(value));
  }

  private unwrap<T>(response: any): T | null {
    return response && typeof response === 'object' && 'result' in response ? response.result : response;
  }
}
