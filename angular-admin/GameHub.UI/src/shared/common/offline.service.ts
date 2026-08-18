import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional, PLATFORM_ID } from '@angular/core';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';
import * as localForage from 'localforage';
import { BehaviorSubject, lastValueFrom } from 'rxjs';

export interface OfflineAction {
  id: string;
  url: string;
  method: string;
  body?: any;
  tenantId?: number | null;
  timestamp: number;
}

/**
 * Serviço de fila offline para ações do usuário.
 * Persiste ações em localForage e sincroniza quando a conexão volta.
 */
@Injectable({ providedIn: 'root' })
export class OfflineService {
  private static idCounter = 0;
  private readonly queueKey = 'eaf-offline-queue';
  private readonly online = new BehaviorSubject<boolean>(true);
  private readonly pending = new BehaviorSubject<number>(0);
  private readonly syncActive = new BehaviorSubject<boolean>(false);

  online$ = this.online.asObservable();
  pending$ = this.pending.asObservable();
  syncActive$ = this.syncActive.asObservable();

  constructor(
    private readonly http: HttpClient,
    @Inject(PLATFORM_ID) private readonly platformId: Object,
    @Optional() @Inject(API_BASE_URL) private readonly baseUrl: string | undefined
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this.online.next(navigator.onLine);
    }
  }

  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    window.addEventListener('online', () => {
      this.online.next(true);
      this.syncQueue();
    });

    window.addEventListener('offline', () => this.online.next(false));

    this.refreshPendingCount();
    if (navigator.onLine) {
      this.syncQueue();
    }
  }

  async queueAction(action: Omit<OfflineAction, 'id' | 'timestamp'>): Promise<void> {
    const queue = await this.getQueue();
    queue.push({
      ...action,
      id: this.generateId(),
      timestamp: Date.now(),
    });
    await localForage.setItem(this.queueKey, queue);
    await this.refreshPendingCount();
  }

  async syncQueue(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || !navigator.onLine || this.syncActive.value) {
      return;
    }

    const queue = await this.getQueue();
    if (queue.length === 0) {
      return;
    }

    this.syncActive.next(true);
    const failed: OfflineAction[] = [];

    try {
      for (const action of queue) {
        try {
          await this.send(action);
        } catch (err) {
          failed.push(action);
          break;
        }
      }
    } finally {
      await localForage.setItem(this.queueKey, failed);
      await this.refreshPendingCount();
      this.syncActive.next(false);
    }
  }

  async getQueue(): Promise<OfflineAction[]> {
    return (await localForage.getItem<OfflineAction[]>(this.queueKey)) ?? [];
  }

  async clearQueue(): Promise<void> {
    await localForage.removeItem(this.queueKey);
    await this.refreshPendingCount();
  }

  private async send(action: OfflineAction): Promise<any> {
    const url = this.resolveUrl(action.url);
    const headers: Record<string, string> = {
      Accept: 'application/json',
    };

    if (action.body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }

    const request = this.http.request(action.method, url, {
      body: action.body,
      headers,
    });

    return await lastValueFrom(request);
  }

  private resolveUrl(url: string): string {
    if (/^https?:\/\//i.test(url)) {
      return url;
    }

    const base = this.baseUrl || (isPlatformBrowser(this.platformId) ? window.location.origin : '');
    return `${base.replace(/\/$/, '')}/${url.replace(/^\//, '')}`;
  }

  private async refreshPendingCount(): Promise<void> {
    const queue = await this.getQueue();
    this.pending.next(queue.length);
  }

  private generateId(): string {
    if (
      isPlatformBrowser(this.platformId) &&
      typeof window !== 'undefined' &&
      window.crypto?.getRandomValues
    ) {
      const bytes = new Uint8Array(8);
      window.crypto.getRandomValues(bytes);
      const random = Array.from(bytes, (b: number) => b.toString(16).padStart(2, '0')).join('');
      return `${Date.now().toString(36)}-${random}`;
    }

    OfflineService.idCounter += 1;
    return `${Date.now().toString(36)}-${OfflineService.idCounter.toString(36)}`;
  }
}
