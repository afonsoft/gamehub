import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface DeveloperProfile {
  id?: string;
  displayName: string;
  legalName: string;
  websiteUrl: string;
  supportEmail: string;
  status?: string;
}

export interface CreateOrUpdateProfileInput {
  displayName: string;
  legalName: string;
  websiteUrl: string;
  supportEmail: string;
}

export interface GameSummary {
  id: string;
  title: string;
  slug: string;
  status: string;
  publishedBuildVersion?: string;
  latestBuildStatus?: string;
  latestBuildId?: string;
  lastUpdated: string;
}

export interface CreateGameDraftInput {
  title: string;
  shortDescription: string;
  description?: string;
  instructions?: string;
  ageRating: string;
  orientation: string;
  supportsDesktop?: boolean;
  supportsMobile?: boolean;
  supportsTablet?: boolean;
  categoryIds?: string[];
  tagIds?: string[];
}

export interface UpdateGameMetadataInput {
  gameId: string;
  title: string;
  shortDescription: string;
  description?: string;
  instructions?: string;
  ageRating: string;
  orientation: string;
  supportsDesktop?: boolean;
  supportsMobile?: boolean;
  supportsTablet?: boolean;
  categoryIds?: string[];
  tagIds?: string[];
}

export interface PagedGameSummary {
  totalCount: number;
  items: GameSummary[];
}

export interface BuildItem {
  id: string;
  version: string;
  buildNumber: number;
  status: string;
  sizeBytes: number;
  hashSha256: string;
  createdAt: string;
  publishedAt?: string;
}

export interface UploadResult {
  buildId: string;
  version: string;
  status: string;
  validationSummary?: { isValid: boolean; errors: string[]; warnings: string[]; packageSizeBytes: number; hashSha256: string };
}

@Injectable({ providedIn: 'root' })
export class DeveloperService {
  private readonly profileUrl = '/api/services/app/DeveloperProfile';
  private readonly gameUrl = '/api/services/app/DeveloperGame';
  private readonly uploadUrl = '/api/game-builds';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<DeveloperProfile | null> {
    return this.http
      .get<DeveloperProfile | { result?: DeveloperProfile }>(`${this.profileUrl}/GetMyProfile`)
      .pipe(map(response => this.unwrap<DeveloperProfile | null>(response)));
  }

  createOrUpdateProfile(input: CreateOrUpdateProfileInput): Observable<DeveloperProfile> {
    return this.http
      .post<DeveloperProfile | { result?: DeveloperProfile }>(`${this.profileUrl}/CreateOrUpdate`, input)
      .pipe(map(response => this.unwrap<DeveloperProfile>(response)));
  }

  getMyGames(skipCount = 0, maxResultCount = 50): Observable<PagedGameSummary> {
    const params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    return this.http
      .get<PagedGameSummary | { result?: PagedGameSummary }>(`${this.gameUrl}/GetMyGames`, { params })
      .pipe(map(response => this.unwrap<PagedGameSummary>(response)));
  }

  createDraft(input: CreateGameDraftInput): Observable<unknown> {
    return this.http.post(`${this.gameUrl}/CreateDraft`, input);
  }

  updateMetadata(input: UpdateGameMetadataInput): Observable<unknown> {
    return this.http.post(`${this.gameUrl}/UpdateMetadata`, input);
  }

  submitForReview(gameId: string, notes?: string): Observable<unknown> {
    return this.http.post(`${this.gameUrl}/SubmitForReview`, { gameId, notes });
  }

  approveBuild(gameBuildId: string): Observable<BuildItem> {
    return this.http
      .post<BuildItem | { result?: BuildItem }>(`${this.gameUrl}/ApproveBuild`, { gameBuildId })
      .pipe(map(response => this.unwrap<BuildItem>(response)));
  }

  rejectBuild(gameBuildId: string, reason: string): Observable<BuildItem> {
    return this.http
      .post<BuildItem | { result?: BuildItem }>(`${this.gameUrl}/RejectBuild`, { gameBuildId, reason })
      .pipe(map(response => this.unwrap<BuildItem>(response)));
  }

  getBuilds(gameId: string): Observable<BuildItem[]> {
    const params = new HttpParams().set('gameId', gameId);
    return this.http
      .get<BuildItem[] | { result?: BuildItem[] }>(`${this.gameUrl}/GetBuilds`, { params })
      .pipe(map(response => this.unwrap<BuildItem[]>(response)));
  }

  uploadBuild(gameId: string, file: File): Observable<UploadResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http
      .post<UploadResult | { result?: UploadResult }>(`${this.uploadUrl}/${gameId}/upload`, form)
      .pipe(map(response => this.unwrap<UploadResult>(response)));
  }

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
