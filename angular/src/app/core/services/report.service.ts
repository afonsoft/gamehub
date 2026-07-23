import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SubmitReportInput {
  gameId: string;
  reason: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiUrl = '/api/services/app/UserReport';

  constructor(private http: HttpClient) {}

  submit(input: SubmitReportInput): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Submit`, input);
  }
}
