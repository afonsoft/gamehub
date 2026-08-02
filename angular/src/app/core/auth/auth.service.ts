import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { TokenService } from './token.service';
import { HubAuthService } from './hub-auth.service';

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

export type TenantSelectionMode = 'PlayerDefault' | 'CreateNew' | 'JoinExisting';

export interface RegisterModel {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
  isDeveloper?: boolean;
  tenantSelectionMode?: TenantSelectionMode;
  newTenantName?: string;
  existingTenantId?: number | null;
  joinRequestMessage?: string;
}

export interface RegisterResultModel {
  userId: number;
  userName: string;
  tenantId?: number | null;
  canLogin: boolean;
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
  private readonly hubAuth = inject(HubAuthService);
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

  register(model: RegisterModel): Observable<{ success: boolean; canLogin?: boolean; userName?: string; tenantId?: number | null; error?: string }> {
    return this.http.post<any>(this.registerUrl, model).pipe(
      map(response => {
        if (response?.success === false) {
          return { success: false, error: this.extractApiError(response) } as const;
        }
        const result = this.unwrap<RegisterResultModel>(response);
        if (!result) {
          return { success: false, error: 'Registration failed. Please try again.' } as const;
        }
        return { success: true, canLogin: result.canLogin, userName: result.userName, tenantId: result.tenantId } as const;
      }),
      switchMap(response => {
        if ('success' in response && response.success === false) {
          return of(response);
        }
        if (!response.canLogin) {
          return of({ success: true, canLogin: false, userName: response.userName, tenantId: response.tenantId });
        }
        return this.loginAfterRegister(model.userName, model.password, response.tenantId).pipe(
          map(success => ({ success, canLogin: true, userName: response.userName, tenantId: response.tenantId }))
        );
      }),
      catchError(err => {
        const message = this.extractErrorMessage(err);
        return of({ success: false, error: message });
      })
    );
  }

  private loginAfterRegister(userName: string, password: string, tenantId?: number | null): Observable<boolean> {
    if (tenantId) {
      return this.hubAuth.selectTenant({ userNameOrEmailAddress: userName, password, tenantId }).pipe(
        tap(result => {
          if (typeof result === 'object' && result?.accessToken) {
            this.tokenService.setToken(result.accessToken);
          }
        }),
        map(result => typeof result === 'object' && !!result?.accessToken),
        catchError(() => of(false))
      );
    }

    return this.hubAuth.getAvailableTenants({ userNameOrEmailAddress: userName, password }).pipe(
      switchMap(tenants => {
        if (!tenants?.length) {
          return of(false);
        }
        const target = tenants.find(t => t.isDefault) ?? tenants[0];
        return this.hubAuth.selectTenant({ userNameOrEmailAddress: userName, password, tenantId: target.tenantId });
      }),
      tap(result => {
        if (typeof result === 'object' && result?.accessToken) {
          this.tokenService.setToken(result.accessToken);
        }
      }),
      map(result => typeof result === 'object' && !!result?.accessToken),
      catchError(() => of(false))
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
