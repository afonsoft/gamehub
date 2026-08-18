import { MockLocalizePipe, setupEafGlobals } from '../test-helpers/mock-services';
import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { of, Subject } from 'rxjs';

import { AppComponent } from './app.component';
import { ChatSignalrService } from '@app/shared/layout/chat/chat-signalr.service';
import { UserNotificationHelper } from '@app/shared/layout/notifications/UserNotificationHelper';
import { AppAuthenticationService } from '@shared/common/auth/app-authentication-service';
import { CookieService } from 'ngx-cookie-service';
// GoogleTagManagerService is mocked below due to Angular version incompatibility
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import { OfflineService } from '@shared/common/offline.service';
import { PwaInstallService } from '@shared/common/pwa-install.service';

// Mock do ChatSignalrService
class MockChatSignalrService {
  configureConnection(connection) {}
  isChatConnected = false;
}

// Mock do UserNotificationHelper
class MockUserNotificationHelper {
  info(message: string) {}
  success(message: string) {}
  warn(message: string) {}
  error(message: string) {}
}

// Mock do AppAuthenticationService
class MockAppAuthenticationService {
  init(): Promise<boolean> {
    return Promise.resolve(true);
  }
}

// Mock do CookieService
class MockCookieService {
  get(key: string): string {
    return '';
  }
}


// Mock do AppSessionService
class MockAppSessionService {
  init(): void {}
}

// Mock do AppUiCustomizationService
class MockAppUiCustomizationService {
  init(): void {}
}

// Mock do LocalizationService
class MockLocalizationService {
  localize(key: string, sourceName?: string): string {
    return key;
  }
}

// Mock do PermissionCheckerService
class MockPermissionCheckerService {
  isGranted(permissionName: string): boolean {
    return true;
  }
}

// Mock do FeatureCheckerService
class MockFeatureCheckerService {
  get(featureName: string): boolean {
    return true;
  }
}

// Mock do MessageService
class MockMessageService {
  info(message: string): void {}
  success(message: string): void {}
  warn(message: string): void {}
  error(message: string): void {}
}

// Mock do NotifyService
class MockNotifyService {
  info(message: string): void {}
  success(message: string): void {}
  warn(message: string): void {}
  error(message: string): void {}
}

// Mock do SettingService
class MockSettingService {
  get(key: string): any {
    return null;
  }
}

// Mock do EafMultiTenancyService
class MockEafMultiTenancyService {
  getTenantId(): number {
    return 1;
  }
}

// Mock do AppUrlService
class MockAppUrlService {
  getRootUrl(): string {
    return 'http://localhost';
  }
}

class MockSwUpdate {
  isEnabled = true;
  versionUpdates = new Subject<any>();
  activateUpdate(): Promise<void> {
    return Promise.resolve();
  }
}

class MockOfflineService {
  online$ = of(true);
  pending$ = of(0);
  syncActive$ = of(false);
  initialize(): void {}
  queueAction(): Promise<void> { return Promise.resolve(); }
  syncQueue(): Promise<void> { return Promise.resolve(); }
  getQueue(): Promise<any[]> { return Promise.resolve([]); }
  clearQueue(): Promise<void> { return Promise.resolve(); }
}

class MockPwaInstallService {
  installPrompt$ = of(null);
  initialize(): void {}
  promptInstall(): Promise<void> { return Promise.resolve(); }
}

describe('AppComponent', () => {
  beforeEach(async () => {
    setupEafGlobals();
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      declarations: [AppComponent, MockLocalizePipe],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: ChatSignalrService, useClass: MockChatSignalrService },
        { provide: UserNotificationHelper, useClass: MockUserNotificationHelper },
        { provide: AppAuthenticationService, useClass: MockAppAuthenticationService },
        { provide: CookieService, useClass: MockCookieService },
                { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppUrlService, useClass: MockAppUrlService },
        { provide: SwUpdate, useClass: MockSwUpdate },
        { provide: OfflineService, useClass: MockOfflineService },
        { provide: PwaInstallService, useClass: MockPwaInstallService },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should set isOnline based on navigator.onLine', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    app.setUpPwa();
    expect(app.isOnline).toBe(navigator.onLine);
  });

  it('should show update banner when version update is ready', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    app.setUpPwa();
    const swUpdate = TestBed.inject(SwUpdate) as unknown as MockSwUpdate;
    swUpdate.versionUpdates.next({ type: 'VERSION_READY', latestVersion: { hash: 'abc123' }, currentVersion: { hash: 'old' } });
    expect(app.updateAvailable).toBeTrue();
    expect(app.updateVersion).toBe('abc123');
  });
});
