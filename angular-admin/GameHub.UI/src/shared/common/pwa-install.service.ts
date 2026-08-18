import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Serviço que captura o evento beforeinstallprompt e permite disparar a instalação da PWA.
 */
@Injectable({ providedIn: 'root' })
export class PwaInstallService {
  private readonly installPrompt = new BehaviorSubject<any | null>(null);

  installPrompt$ = this.installPrompt.asObservable();

  constructor(@Inject(PLATFORM_ID) private readonly platformId: Object) {}

  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    window.addEventListener('beforeinstallprompt', (event: Event) => {
      event.preventDefault();
      this.installPrompt.next(event);
    });

    window.addEventListener('appinstalled', () => {
      this.installPrompt.next(null);
    });
  }

  async promptInstall(): Promise<void> {
    const event = this.installPrompt.value;
    if (!event) {
      return;
    }

    event.prompt();
    await event.userChoice;
    this.installPrompt.next(null);
  }

  isInstallable(): Observable<boolean> {
    return new BehaviorSubject<boolean>(this.installPrompt.value !== null).asObservable();
  }
}
