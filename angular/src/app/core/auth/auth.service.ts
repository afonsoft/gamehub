import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
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
  isDeveloper?: boolean;
}

export interface RegisterResultModel {
  userId: number;
  userName: string;
}

export interface RegisterErrorModel {
  error: {
    message: string;
    details?: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  private readonly authUrl = '/api/TokenAuth/Authenticate';
  private readonly registerUrl = '/api/services/app/Registration/Register';

  /**
   * Authenticates using the legacy token endpoint.
   * Prefer the HubAuth flow for multi-tenant selection.
   */
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

  register(model: RegisterModel): Observable<{ success: boolean; error?: string }> {
    return this.http.post<any>(this.registerUrl, model).pipe(
      map(response => {
        if (response?.success === false) {
          return { success: false, error: this.extractApiError(response) } as const;
        }
        const result = this.unwrap<RegisterResultModel>(response);
        if (!result) {
          return { success: false, error: 'Registration failed. Please try again.' } as const;
        }
        return result;
      }),
      switchMap(response => {
        if ('success' in response && response.success === false) {
          return of(response);
        }
        return this.login({
          userNameOrEmailAddress: model.userName,
          password: model.password,
          rememberClient: true,
        }).pipe(map(success => ({ success })));
      }),
      catchError(err => {
        const message = this.extractErrorMessage(err);
        return of({ success: false, error: message });
      })
    );
  }

  finalizeLogin(accessToken: string, returnUrl?: string): void {
    this.tokenService.setToken(accessToken);
    void this.router.navigateByUrl(returnUrl || '/');
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

  private extractErrorMessage(err: unknown): string {
    if (!err) {
      return 'Unknown error';
    }
    const error = (err as any)?.error;
    if (error?.details) {
      return error.details;
    }
    if (error?.message) {
      return error.message;
    }
    if ((err as any)?.message) {
      return (err as any).message;
    }
    return 'Registration failed. Please try again.';
  }

  private extractApiError(response: any): string {
    const error = response?.error;
    if (error?.details) {
      return error.details;
    }
    return error?.message || 'Registration failed. Please try again.';
  }
}
