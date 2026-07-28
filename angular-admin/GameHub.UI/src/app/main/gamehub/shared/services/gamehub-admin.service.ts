import { Injectable, Inject, Optional } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface BuildListItem {
  id: string;
  gameId: string;
  gameTitle: string;
  developerName: string;
  version: string;
  buildNumber: number;
  status: string;
  sizeBytes: number;
  createdAt: string;
  publishedAt?: string;
}

export interface PagedBuildList {
  totalCount: number;
  items: BuildListItem[];
}

export interface BuildFile {
  name: string;
  key: string;
  sizeBytes: number;
  contentType: string;
  url: string;
  lastModified?: string;
  isIndexHtml: boolean;
}

export interface BuildFileList {
  items: BuildFile[];
}

export interface ValidationReport {
  id: string;
  gameId: string;
  gameBuildId: string;
  gameTitle: string;
  version: string;
  isValid: boolean;
  hasExternalRequests: boolean;
  warningsCount: number;
  errorsCount: number;
  warnings: string[];
  createdAt: string;
}

export interface InspectorSession {
  id: string;
  gameId: string;
  gameSlug?: string;
  gameBuildId?: string;
  startedAt: string;
  devicePreset?: string;
  resolution?: string;
  status: string;
}

export interface InspectorSessionDetail extends InspectorSession {
  events: InspectorSdkEvent[];
  warnings: InspectorWarning[];
  checklistAnswers: InspectorChecklistAnswer[];
}

export interface InspectorChecklistAnswer {
  id: string;
  sessionId: string;
  questionId: string;
  answer: string;
  updatedAt: string;
}

export interface InspectorChecklistCompletion {
  totalQuestions: number;
  answeredQuestions: number;
  completionPercentage: number;
}

export interface InspectorSdkEvent {
  id: string;
  sessionId: string;
  eventType: string;
  payload?: string;
  sequenceNumber: number;
  timestamp: string;
}

export interface InspectorWarning {
  id: string;
  sessionId: string;
  category: string;
  message: string;
  severity: string;
}

export interface PlaytestRecording {
  id: string;
  playtestSessionId: string;
  url: string;
  durationSeconds: number;
  deviceType: string;
  countryCode: string;
  consoleOutput: string;
  notes: string;
  createdAt: string;
}

export interface PagedPlaytestRecordings {
  totalCount: number;
  items: PlaytestRecording[];
}

@Injectable({
  providedIn: 'root',
})
export class GameHubAdminService {
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string,
  ) {
    this.baseUrl = baseUrl || '';
  }

  getDashboardSummary(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetSummary`).pipe(map(this.unwrapResult));
  }

  getPlaysOverTime(days: number): Observable<any> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetPlaysOverTime`, { params }).pipe(map(this.unwrapResult));
  }

  getRecentUploads(count: number): Observable<any> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetRecentUploads`, { params }).pipe(map(this.unwrapResult));
  }

  getRecentGames(count: number): Observable<any> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetRecentGames`, { params }).pipe(map(this.unwrapResult));
  }

  getTopGames(count: number): Observable<any> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetTopGames`, { params }).pipe(map(this.unwrapResult));
  }

  getFeatureFlags(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/services/app/FeatureFlag/GetAll`).pipe(map(this.unwrapResult));
  }

  toggleFeatureFlag(id: string, isEnabled: boolean): Observable<any> {
    const params = new HttpParams().set('id', id).set('isEnabled', isEnabled.toString());
    return this.http.put(`${this.baseUrl}/api/services/app/FeatureFlag/Toggle`, null, { params }).pipe(map(this.unwrapResult));
  }

  getAuditLogs(skipCount: number, maxResultCount: number): Observable<any> {
    const params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AuditLog/GetAll`, { params }).pipe(map(this.unwrapResult));
  }

  getUsers(skipCount: number, maxResultCount: number): Observable<any> {
    const params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    return this.http.get(`${this.baseUrl}/api/services/app/AdminUser/GetAll`, { params }).pipe(map(this.unwrapResult));
  }

  getGames(skipCount: number, maxResultCount: number, status?: string): Observable<any> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    if (status) {
      params = params.set('Status', status);
    }
    return this.http.get(`${this.baseUrl}/api/services/app/AdminGame/GetAll`, { params }).pipe(map(this.unwrapResult));
  }

  getGameDetail(id: string): Observable<any> {
    const params = new HttpParams().set('gameId', id);
    return this.http.get(`${this.baseUrl}/api/services/app/AdminGame/GetDetail`, { params }).pipe(map(this.unwrapResult));
  }

  getBuilds(skipCount: number, maxResultCount: number, status?: string, gameId?: string, searchText?: string): Observable<PagedBuildList> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    if (status) {
      params = params.set('Status', status);
    }
    if (gameId) {
      params = params.set('GameId', gameId);
    }
    if (searchText) {
      params = params.set('SearchText', searchText);
    }
    return this.http.get<PagedBuildList>(`${this.baseUrl}/api/services/app/AdminBuild/GetAllBuilds`, { params }).pipe(map(this.unwrapResult));
  }

  getBuildFiles(buildId: string): Observable<BuildFileList> {
    const params = new HttpParams().set('buildId', buildId);
    return this.http.get<BuildFileList>(`${this.baseUrl}/api/services/app/AdminBuild/GetBuildFiles`, { params }).pipe(map(this.unwrapResult));
  }

  suspendGame(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/AdminGame/Suspend`, { gameId: id, reason }).pipe(map(this.unwrapResult));
  }

  approveThumbnail(gameId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/AdminGame/ApproveThumbnail`, { gameId }).pipe(map(this.unwrapResult));
  }

  rejectThumbnail(gameId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/AdminGame/RejectThumbnail`, { gameId }).pipe(map(this.unwrapResult));
  }

  getPendingReviews(count?: number): Observable<any> {
    if (count != null) {
      const params = new HttpParams().set('count', count.toString());
      return this.http.get(`${this.baseUrl}/api/services/app/AdminDashboard/GetPendingReviews`, { params }).pipe(map(this.unwrapResult));
    }
    return this.http.get(`${this.baseUrl}/api/services/app/Moderation/GetPendingReviews`).pipe(map(this.unwrapResult));
  }

  getReviewDetail(id: string): Observable<any> {
    const params = new HttpParams().set('reviewId', id);
    return this.http.get(`${this.baseUrl}/api/services/app/Moderation/GetDetail`, { params }).pipe(map(this.unwrapResult));
  }

  completeReview(reviewId: string, decision: string, notes: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/Moderation/CompleteReview`, { reviewId, decision, notes }).pipe(map(this.unwrapResult));
  }

  getCategories(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/services/app/Category/GetAll`).pipe(map(this.unwrapResult));
  }

  getCategoryById(id: string): Observable<any> {
    const params = new HttpParams().set('id', id);
    return this.http.get(`${this.baseUrl}/api/services/app/Category/Get`, { params }).pipe(map(this.unwrapResult));
  }

  createOrUpdateCategory(category: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/Category/CreateOrUpdate`, category).pipe(map(this.unwrapResult));
  }

  deleteCategory(id: string): Observable<any> {
    const params = new HttpParams().set('id', id);
    return this.http.delete(`${this.baseUrl}/api/services/app/Category/Delete`, { params }).pipe(map(this.unwrapResult));
  }

  getTags(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/services/app/Tag/GetAll`).pipe(map(this.unwrapResult));
  }

  getTagById(id: string): Observable<any> {
    const params = new HttpParams().set('id', id);
    return this.http.get(`${this.baseUrl}/api/services/app/Tag/Get`, { params }).pipe(map(this.unwrapResult));
  }

  createOrUpdateTag(tag: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/Tag/CreateOrUpdate`, tag).pipe(map(this.unwrapResult));
  }

  deleteTag(id: string): Observable<any> {
    const params = new HttpParams().set('id', id);
    return this.http.delete(`${this.baseUrl}/api/services/app/Tag/Delete`, { params }).pipe(map(this.unwrapResult));
  }

  getReports(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/services/app/UserReport/GetAll`).pipe(map(this.unwrapResult));
  }

  updateReportStatus(reportId: string, status: string): Observable<any> {
    const params = new HttpParams().set('reportId', reportId).set('status', status);
    return this.http.put(`${this.baseUrl}/api/services/app/AdminReport/UpdateStatus`, null, { params });
  }

  getValidationReports(maxResultCount: number = 50): Observable<ValidationReport[]> {
    const params = new HttpParams().set('maxResultCount', maxResultCount.toString());
    return this.http.get<ValidationReport[]>(`${this.baseUrl}/api/services/app/BuildValidation/GetReports`, { params }).pipe(map(this.unwrapResult));
  }

  getInspectorSessions(gameId: string, maxResultCount: number = 20): Observable<InspectorSession[]> {
    const params = new HttpParams()
      .set('gameId', gameId)
      .set('maxResultCount', maxResultCount.toString());
    return this.http.get<InspectorSession[]>(`${this.baseUrl}/api/services/app/Inspector/GetSessions`, { params }).pipe(map(this.unwrapResult));
  }

  getInspectorSession(sessionId: string): Observable<InspectorSessionDetail> {
    const params = new HttpParams().set('sessionId', sessionId);
    return this.http.get<InspectorSessionDetail>(`${this.baseUrl}/api/services/app/Inspector/GetSession`, { params }).pipe(map(this.unwrapResult));
  }

  startInspectorSession(input: Partial<InspectorSession>): Observable<InspectorSession> {
    return this.http.post<InspectorSession>(`${this.baseUrl}/api/services/app/Inspector/StartSession`, input).pipe(map(this.unwrapResult));
  }

  validateInspectorSession(sessionId: string): Observable<InspectorWarning[]> {
    const params = new HttpParams().set('sessionId', sessionId);
    return this.http.get<InspectorWarning[]>(`${this.baseUrl}/api/services/app/Inspector/ValidateSession`, { params }).pipe(map(this.unwrapResult));
  }

  saveInspectorChecklistAnswer(sessionId: string, questionId: string, answer: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/Inspector/SaveChecklistAnswer`, { sessionId, questionId, answer }).pipe(map(this.unwrapResult));
  }

  getInspectorChecklistCompletion(sessionId: string): Observable<InspectorChecklistCompletion> {
    const params = new HttpParams().set('sessionId', sessionId);
    return this.http.get<InspectorChecklistCompletion>(`${this.baseUrl}/api/services/app/Inspector/GetChecklistCompletion`, { params }).pipe(map(this.unwrapResult));
  }

  getPlaytestRecordings(skipCount: number, maxResultCount: number, gameId?: string, deviceType?: string): Observable<PagedPlaytestRecordings> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());
    if (gameId) params = params.set('GameId', gameId);
    if (deviceType) params = params.set('DeviceType', deviceType);
    return this.http.get<PagedPlaytestRecordings>(`${this.baseUrl}/api/services/app/Playtest/GetAllRecordings`, { params }).pipe(map(this.unwrapResult));
  }

  getPlaytestRecording(id: string): Observable<PlaytestRecording> {
    const params = new HttpParams().set('recordingId', id);
    return this.http.get<PlaytestRecording>(`${this.baseUrl}/api/services/app/Playtest/GetRecording`, { params }).pipe(map(this.unwrapResult));
  }

  addPlaytestRecordingNotes(id: string, notes: string): Observable<PlaytestRecording> {
    return this.http.post<PlaytestRecording>(`${this.baseUrl}/api/services/app/Playtest/AddNotes`, { recordingId: id, notes }).pipe(map(this.unwrapResult));
  }

  createPreviewToken(gameId: string, version: string): Observable<{ token: string; previewUrl: string; version: string; gameSlug: string }> {
    return this.http
      .post<{ token: string; previewUrl: string; version: string; gameSlug: string } | { result?: { token: string; previewUrl: string; version: string; gameSlug: string } }>(
        `${this.baseUrl}/api/services/app/GamePreview/CreatePreviewToken`,
        { gameId, version },
      )
      .pipe(map(this.unwrapResult));
  }

  requestPlaytest(gameId: string, notes = ''): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/Playtest/RequestPlaytest`, { gameId, notes }).pipe(map(this.unwrapResult));
  }

  private unwrapResult = (response: any): any => {
    return response && typeof response === 'object' && 'result' in response ? response.result : response;
  };
}
