import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, retry } from 'rxjs/operators';

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
  controls?: string;
  suggestedDescription?: string;
  seoDescription?: string;
  ageRating: string;
  orientation: string;
  aspectRatio?: string;
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
  controls?: string;
  suggestedDescription?: string;
  seoDescription?: string;
  ageRating: string;
  orientation: string;
  aspectRatio?: string;
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
  gameId: string;
  gameSlug: string;
  version: string;
  buildNumber: number;
  status: string;
  sizeBytes: number;
  hashSha256: string;
  createdAt: string;
  publishedAt?: string;
}

export interface PreviewTokenResult {
  token: string;
  previewUrl: string;
  version: string;
  gameSlug: string;
}

export interface InspectorSessionResult {
  id: string;
  gameId: string;
  gameBuildId: string;
  devicePreset: string;
  resolution: string;
  startedAt: string;
}

export interface DeveloperReviewHistoryItem {
  id: string;
  gameId: string;
  gameBuildId?: string;
  status: string;
  decision?: string;
  notes: string;
  createdAt: string;
  completedAt?: string;
}

export interface DeveloperTeam {
  id?: string;
  name: string;
  primaryContactEmail: string;
  country: string;
  createdAt?: string;
  members?: DeveloperTeamMember[];
}

export interface DeveloperTeamMember {
  userId: number;
  userName: string;
  email: string;
  role: string;
}

export interface DeveloperDashboard {
  totalGames: number;
  publishedGames: number;
  pendingReviewGames: number;
  draftGames: number;
  rejectedGames: number;
  totalPlays: number;
  recentVersions: DeveloperGameVersion[];
  pendingActions: DeveloperDashboardAction[];
  playsOverTime: DashboardDailyPlays[];
}

export interface DashboardDailyPlays {
  date: string;
  plays: number;
}

export interface DeveloperGameVersion {
  id: string;
  gameId: string;
  gameTitle: string;
  version: string;
  status: string;
  createdAt: string;
  publishedAt?: string;
}

export interface DeveloperDashboardAction {
  gameId: string;
  title: string;
  slug: string;
  status: string;
  action: string;
}

export interface GameMetricsFilter {
  from?: string;
  to?: string;
  countryCode?: string;
  deviceType?: string;
  trafficSource?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
}

export interface GameMetricsDailyItem {
  date: string;
  plays: number;
  uniquePlayers: number;
  avgDurationSeconds: number;
  loadingFinishedCount: number;
  gameplayStartedCount: number;
  pageViewCount: number;
  conversionCount: number;
  errorCount: number;
  commercialBreakCount: number;
  rewardedBreakCount: number;
}

export interface GameMetricsResult {
  totalPlays: number;
  totalUniquePlayers: number;
  averageDurationSeconds: number;
  loadingFinishedCount: number;
  gameplayStartedCount: number;
  pageViewCount: number;
  conversionCount: number;
  errorCount: number;
  commercialBreakCount: number;
  rewardedBreakCount: number;
  daily: GameMetricsDailyItem[];
}

export interface DailyEarnings {
  date: string;
  commercialBreaks: number;
  rewardedBreaks: number;
  grossEstimatedRevenue: number;
  developerEstimatedRevenue: number;
}

export interface GameEarnings {
  gameId: string;
  gameTitle: string;
  totalPlays: number;
  commercialBreaks: number;
  rewardedBreaks: number;
  grossEstimatedRevenue: number;
  developerEstimatedRevenue: number;
  platformEstimatedRevenue: number;
  developerShare: number;
  contractType: number;
  daily: DailyEarnings[];
}

export interface DeveloperEarnings {
  from: string;
  to: string;
  totalGrossEstimatedRevenue: number;
  totalDeveloperEstimatedRevenue: number;
  totalPlatformEstimatedRevenue: number;
  totalCommercialBreaks: number;
  totalRewardedBreaks: number;
  games: GameEarnings[];
}

export interface DeveloperEarningsFilter {
  from?: string;
  to?: string;
  maxResultCount?: number;
}

export interface UploadResult {
  buildId: string;
  version: string;
  status: string;
  validationSummary?: { isValid: boolean; errors: string[]; warnings: string[]; packageSizeBytes: number; hashSha256: string; hasIndexHtml: boolean; indexHtmlPath: string };
}

export interface UploadImageResult {
  url: string;
  key: string;
  sizeBytes: number;
}

@Injectable({ providedIn: 'root' })
export class DeveloperService {
  private readonly profileUrl = '/api/services/app/DeveloperProfile';
  private readonly gameUrl = '/api/services/app/DeveloperGame';
  private readonly teamUrl = '/api/services/app/DeveloperTeam';
  private readonly uploadUrl = '/api/game-builds';
  private readonly assetUrl = '/api/game-assets';
  private readonly dashboardUrl = '/api/services/app/DeveloperDashboard';
  private readonly metricsUrl = '/api/services/app/GameMetrics';
  private readonly earningsUrl = '/api/services/app/DeveloperEarnings';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<DeveloperProfile | null> {
    return this.http
      .get<DeveloperProfile | { result?: DeveloperProfile }>(`${this.profileUrl}/GetMyProfile`)
      .pipe(map(response => this.unwrap<DeveloperProfile | null>(response)));
  }

  getTeam(): Observable<DeveloperTeam | null> {
    return this.http
      .get<DeveloperTeam | { result?: DeveloperTeam }>(`${this.teamUrl}/GetMyTeam`)
      .pipe(map(response => this.unwrap<DeveloperTeam | null>(response)));
  }

  getTeamGeneralSettings(): Observable<DeveloperTeam | null> {
    return this.http
      .get<DeveloperTeam | { result?: DeveloperTeam }>(`${this.teamUrl}/GetGeneralSettings`)
      .pipe(map(response => this.unwrap<DeveloperTeam | null>(response)));
  }

  updateTeamGeneralSettings(input: DeveloperTeam): Observable<DeveloperTeam> {
    return this.http
      .post<DeveloperTeam | { result?: DeveloperTeam }>(`${this.teamUrl}/UpdateGeneralSettings`, input)
      .pipe(map(response => this.unwrap<DeveloperTeam>(response)));
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
      .pipe(
        retry({ count: 2 }),
        map(response => this.unwrap<PagedGameSummary>(response)),
      );
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

  getGameVersions(gameId: string): Observable<BuildItem[]> {
    const params = new HttpParams().set('gameId', gameId);
    return this.http
      .get<BuildItem[] | { result?: BuildItem[] }>(`${this.gameUrl}/GetVersions`, { params })
      .pipe(map(response => this.unwrap<BuildItem[]>(response)));
  }

  getReviewHistory(gameId: string): Observable<DeveloperReviewHistoryItem[]> {
    const params = new HttpParams().set('gameId', gameId);
    return this.http
      .get<DeveloperReviewHistoryItem[] | { result?: DeveloperReviewHistoryItem[] }>(
        `${this.gameUrl}/GetReviewHistory`,
        { params },
      )
      .pipe(map(response => this.unwrap<DeveloperReviewHistoryItem[]>(response)));
  }

  createPreviewToken(gameId: string, version: string): Observable<PreviewTokenResult> {
    return this.http
      .post<PreviewTokenResult | { result?: PreviewTokenResult }>(`${this.gameUrl}/CreatePreviewTokenForBuild`, { gameId, version })
      .pipe(map(response => this.unwrap<PreviewTokenResult>(response)));
  }

  startInspectorSession(gameId: string, gameBuildId: string, devicePreset = 'desktop', resolution = '1024x768'): Observable<InspectorSessionResult> {
    return this.http
      .post<InspectorSessionResult | { result?: InspectorSessionResult }>(`${this.gameUrl}/StartInspectorSessionForBuild`, {
        gameId,
        gameBuildId,
        devicePreset,
        resolution,
      })
      .pipe(map(response => this.unwrap<InspectorSessionResult>(response)));
  }

  getDashboard(): Observable<DeveloperDashboard> {
    return this.http
      .get<DeveloperDashboard | { result?: DeveloperDashboard }>(`${this.dashboardUrl}/GetDashboard`)
      .pipe(map(response => this.unwrap<DeveloperDashboard>(response)));
  }

  getGameMetrics(gameId: string, filter: GameMetricsFilter = {}): Observable<GameMetricsResult> {
    const params = new HttpParams({ fromObject: this.toParams(filter) });
    return this.http
      .get<GameMetricsResult | { result?: GameMetricsResult }>(`${this.metricsUrl}/GetMetrics`, { params })
      .pipe(map(response => this.unwrap<GameMetricsResult>(response)));
  }

  getEarnings(filter: DeveloperEarningsFilter = {}): Observable<DeveloperEarnings> {
    const params = new HttpParams({ fromObject: this.toEarningsParams(filter) });
    return this.http
      .get<DeveloperEarnings | { result?: DeveloperEarnings }>(`${this.earningsUrl}/GetEarnings`, { params })
      .pipe(
        retry({ count: 2 }),
        map(response => this.unwrap<DeveloperEarnings>(response)),
      );
  }

  exportEarningsCsv(filter: DeveloperEarningsFilter = {}): Observable<Blob> {
    const params = new HttpParams({ fromObject: this.toEarningsParams(filter) });
    return this.http.get('/api/exports/earnings', { params, responseType: 'blob' });
  }

  exportGameMetricsCsv(gameId: string, filter: GameMetricsFilter = {}): Observable<Blob> {
    const params = new HttpParams({ fromObject: this.toParams(filter) });
    return this.http.get(`/api/exports/metrics/${encodeURIComponent(gameId)}`, { params, responseType: 'blob' });
  }

  private toEarningsParams(filter: DeveloperEarningsFilter): Record<string, string> {
    const params: Record<string, string> = { maxResultCount: (filter.maxResultCount ?? 50).toString() };
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    return params;
  }

  private toParams(filter: GameMetricsFilter): Record<string, string> {
    const params: Record<string, string> = {};
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    if (filter.countryCode) params['countryCode'] = filter.countryCode;
    if (filter.deviceType) params['deviceType'] = filter.deviceType;
    if (filter.trafficSource) params['trafficSource'] = filter.trafficSource;
    if (filter.utmSource) params['utmSource'] = filter.utmSource;
    if (filter.utmMedium) params['utmMedium'] = filter.utmMedium;
    if (filter.utmCampaign) params['utmCampaign'] = filter.utmCampaign;
    return params;
  }

  uploadBuild(gameId: string, file: File): Observable<UploadResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http
      .post<UploadResult | { result?: UploadResult }>(`${this.uploadUrl}/${gameId}/upload`, form)
      .pipe(map(response => this.unwrap<UploadResult>(response)));
  }

  uploadThumbnail(gameId: string, file: File): Observable<UploadImageResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http
      .post<UploadImageResult | { result?: UploadImageResult }>(`${this.assetUrl}/${gameId}/thumbnail`, form)
      .pipe(map(response => this.unwrap<UploadImageResult>(response)));
  }

  uploadAnimatedThumbnail(gameId: string, file: File): Observable<UploadImageResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http
      .post<UploadImageResult | { result?: UploadImageResult }>(`${this.assetUrl}/${gameId}/animated-thumbnail`, form)
      .pipe(map(response => this.unwrap<UploadImageResult>(response)));
  }

  uploadHero(gameId: string, file: File): Observable<UploadImageResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http
      .post<UploadImageResult | { result?: UploadImageResult }>(`${this.assetUrl}/${gameId}/hero`, form)
      .pipe(map(response => this.unwrap<UploadImageResult>(response)));
  }

  private unwrap<T>(response: T | { result?: T }): T {
    return response && typeof response === 'object' && 'result' in response
      ? (response as { result?: T }).result!
      : (response as T);
  }
}
