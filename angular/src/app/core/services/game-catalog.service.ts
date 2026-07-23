import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Category {
  id: string;
  name: string;
  slug: string;
  sortOrder?: number;
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
}

export interface GameCard {
  id: string;
  title: string;
  slug: string;
  thumbnailUrl: string;
  shortDescription: string;
  supportsMobile: boolean;
  supportsDesktop: boolean;
  totalPlays: number;
  totalLikes: number;
  totalDislikes: number;
  averageRating: number;
  totalVotes: number;
  categories?: Category[];
}

export interface GameVoteResult {
  gameId: string;
  totalLikes: number;
  totalDislikes: number;
  userVote?: 'Like' | 'Dislike' | null;
}

export interface HomeResponse {
  highlights: GameCard[];
  newGames: GameCard[];
  mostPlayed: GameCard[];
  trending: GameCard[];
  categories: Category[];
}

export interface GameDetail {
  id: string;
  title: string;
  slug: string;
  status?: string;
  thumbnailUrl: string;
  heroImageUrl: string;
  shortDescription: string;
  description: string;
  instructions: string;
  controls?: string;
  ageRating: string;
  orientation: string;
  developerName: string;
  publishedBuildUrl: string;
  totalPlays: number;
  totalLikes: number;
  totalDislikes: number;
  averageRating: number;
  totalVotes: number;
  supportsDesktop: boolean;
  supportsMobile: boolean;
  supportsTablet: boolean;
  categories: { id: string; name: string; slug: string }[];
  tags: { id: string; name: string; slug: string }[];
  relatedGames: GameCard[];
}

export interface PagedGames {
  totalCount: number;
  items: GameCard[];
}

@Injectable({ providedIn: 'root' })
export class GameCatalogService {
  private readonly apiUrl = '/api/services/app/GameCatalog';
  private readonly tagUrl = '/api/services/app/Tag';

  constructor(private http: HttpClient) {}

  getHome(): Observable<HomeResponse> {
    return this.http.get<HomeResponse>(`${this.apiUrl}/GetHome`).pipe(
      map(response => this.unwrap<HomeResponse>(response)),
    );
  }

  getTags(): Observable<Tag[]> {
    return this.http.get<Tag[]>(`${this.tagUrl}/GetAll`).pipe(
      map(response => this.unwrap<Tag[]>(response)),
    );
  }

  getGames(
    skipCount = 0,
    maxResultCount = 24,
    sorting = 'MostPlayed',
    categorySlug?: string,
    tagSlug?: string,
    device?: string,
    orientation?: string,
  ): Observable<PagedGames> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString())
      .set('Sorting', sorting);
    if (categorySlug) params = params.set('CategorySlug', categorySlug);
    if (tagSlug) params = params.set('TagSlug', tagSlug);
    if (device) params = params.set('Device', device);
    if (orientation) params = params.set('Orientation', orientation);
    return this.http.get<PagedGames>(`${this.apiUrl}/GetGames`, { params }).pipe(
      map(response => this.unwrap<PagedGames>(response)),
    );
  }

  getBySlug(slug: string): Observable<GameDetail | null> {
    return this.http.get<GameDetail>(`${this.apiUrl}/GetBySlug`, { params: { slug } }).pipe(
      map(response => this.unwrap<GameDetail | null>(response)),
    );
  }

  getVote(gameId: string, deviceId?: string): Observable<GameVoteResult> {
    let params = new HttpParams().set('gameId', gameId);
    if (deviceId) params = params.set('deviceId', deviceId);
    return this.http.get<GameVoteResult>(`${this.apiUrl}/GetVote`, { params }).pipe(
      map(response => this.unwrap<GameVoteResult>(response)),
    );
  }

  vote(gameId: string, voteType: 'Like' | 'Dislike', deviceId?: string): Observable<GameVoteResult> {
    return this.http.post<GameVoteResult>(`${this.apiUrl}/Vote`, { gameId, voteType, deviceId }).pipe(
      map(response => this.unwrap<GameVoteResult>(response)),
    );
  }

  search(
    query: string,
    categories: string[] = [],
    tags: string[] = [],
    skipCount = 0,
    maxResultCount = 24,
  ): Observable<PagedGames> {
    let params = new HttpParams()
      .set('Query', query)
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    categories.forEach(c => (params = params.append('Categories', c)));
    tags.forEach(t => (params = params.append('Tags', t)));
    return this.http.get<PagedGames>(`${this.apiUrl}/Search`, { params }).pipe(
      map(response => this.unwrap<PagedGames>(response)),
    );
  }

  private unwrap<T>(response: any): T {
    return response && typeof response === 'object' && 'result' in response ? response.result : response;
  }
}
