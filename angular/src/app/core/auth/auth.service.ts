import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { TokenService } from './token.service';

export interface AuthenticateModel {
  userNameOrEmailAddress: string;
  password: string;
  rememberClient?: boolean;
}

export interface AuthenticateResultModel {
  accessToken: string;
  encryptedAccessToken?: string;
  expireInSeconds?: number;
  userId?: number;
}

export interface RegisterModel {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  private readonly authUrl = '/api/TokenAuth/Authenticate';
  private readonly registerUrl = '/api/services/app/Account/Register';

  login(model: AuthenticateModel): Observable<boolean> {
    return this.http.post<AuthenticateResultModel | { result?: AuthenticateResultModel }>(this.authUrl, model).pipe(
      map(response => this.unwrap(response)),
      tap(result => {
        if (result?.accessToken) {
          this.tokenService.setToken(result.accessToken);
        }
      }),
      map(result => !!result?.accessToken),
      catchError(() => of(false))
    );
  }

  register(model: RegisterModel): Observable<boolean> {
    return this.http.post<unknown | { result?: unknown }>(this.registerUrl, model).pipe(
      map(response => this.unwrap(response) !== undefined),
      catchError(() => of(false))
    );
  }

  logout(returnUrl?: string): void {
    this.tokenService.clearToken();
    void this.router.navigate(['/login'], returnUrl ? { queryParams: { returnUrl } } : undefined);
  }

  isLoggedIn(): boolean {
    return this.tokenService.isValid();
  }

  isDeveloper(): boolean {
    return this.tokenService.isInRole('Developer') || this.tokenService.isInRole('Admin');
  }

  private unwrap<T>(response: T | { result?: T }): T | null {
    if (response && typeof response === 'object' && 'result' in response) {
      return (response as { result?: T }).result ?? null;
    }
    return response as T;
  }
}
