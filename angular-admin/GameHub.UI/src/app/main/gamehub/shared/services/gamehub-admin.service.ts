import { Injectable, Inject, Optional } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

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

  suspendGame(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/services/app/AdminGame/Suspend`, { gameId: id, reason }).pipe(map(this.unwrapResult));
  }

  getPendingReviews(): Observable<any> {
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

  private unwrapResult = (response: any): any => {
    return response && typeof response === 'object' && 'result' in response ? response.result : response;
  };
}
