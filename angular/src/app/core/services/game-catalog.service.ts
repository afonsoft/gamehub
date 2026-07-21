import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GameCard {
  id: string;
  title: string;
  slug: string;
  thumbnailUrl: string;
  shortDescription: string;
  supportsMobile: boolean;
  supportsDesktop: boolean;
  totalPlays: number;
}

export interface HomeResponse {
  highlights: GameCard[];
  newGames: GameCard[];
  mostPlayed: GameCard[];
  trending: GameCard[];
  categories: { id: string; name: string; slug: string }[];
}

@Injectable({ providedIn: 'root' })
export class GameCatalogService {
  private readonly apiUrl = '/api/services/app/GameCatalog';

  constructor(private http: HttpClient) {}

  getHome(): Observable<HomeResponse> {
    return this.http.get<HomeResponse>(`${this.apiUrl}/GetHome`);
  }

  getGames(): Observable<{ items: GameCard[] }> {
    return this.http.get<{ items: GameCard[] }>(`${this.apiUrl}/GetGames`);
  }

  getBySlug(slug: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetBySlug`, { params: { slug } });
  }
}
