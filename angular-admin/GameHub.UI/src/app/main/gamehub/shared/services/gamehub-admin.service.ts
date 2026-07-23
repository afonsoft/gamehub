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

  private unwrapResult = (response: any): any => {
    return response && typeof response === 'object' && 'result' in response ? response.result : response;
  };
}
