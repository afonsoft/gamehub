/**
 * Shared mock services for unit tests.
 * These mocks replicate the pattern used in app.component.spec.ts.
 */

import { Pipe, PipeTransform } from '@angular/core';

// ===== EAF Framework Mocks =====

export class MockLocalizationService {
  localize(key: string, sourceName?: string): string {
    return key;
  }
}

export class MockPermissionCheckerService {
  isGranted(permissionName: string): boolean {
    return true;
  }
}

export class MockFeatureCheckerService {
  get(featureName: string): boolean {
    return true;
  }
  isEnabled(featureName: string): boolean {
    return true;
  }
}

export class MockMessageService {
  info(message: string): Promise<any> {
    return Promise.resolve();
  }
  success(message: string): void {
    // no-op
  }
  warn(message: string): void {
    // no-op
  }
  error(message: string): void {
    // no-op
  }
  confirm(message: string, title?: string, callback?: (result: boolean) => void): void {
    if (callback) {
      callback(true);
    }
  }
}

export class MockNotifyService {
  info(message: string): void {
    // no-op
  }
  success(message: string): void {
    // no-op
  }
  warn(message: string): void {
    // no-op
  }
  error(message: string): void {
    // no-op
  }
}

export class MockSettingService {
  get(key: string): any {
    return null;
  }
  getBoolean(key: string): boolean {
    return false;
  }
}

export class MockEafMultiTenancyService {
  isEnabled = false;
  getTenantId(): number {
    return 1;
  }
}

export class MockEafSessionService {
  tenantId: number = null;
  impersonatorUserId: number = null;
}

// ===== Application Service Mocks =====

export class MockAppSessionService {
  user = {
    name: 'Test User',
    surname: 'User',
    userName: 'testuser',
    emailAddress: 'test@test.com',
    profilePictureId: null,
    authenticationSource: undefined,
  };
  tenant = {
    id: 1,
    tenancyName: 'TestTenant',
  };
  tenancyName = 'TestTenant';
  theme = {
    baseSettings: {
      menu: { asideSkin: 'light' },
      header: {},
      layout: {},
      footer: {},
    },
  };
  init(): void {
    // no-op
  }
  getShownLoginName(): string {
    return 'testuser';
  }
}

export class MockAppUiCustomizationService {
  init(): void {
    // no-op
  }
  getUiCustomizationSettings(): any {
    return {};
  }
}

export class MockAppUrlService {
  appRootUrl = 'http://localhost';
  getRootUrl(): string {
    return 'http://localhost';
  }
}

export class MockChatSignalrService {
  configureConnection(connection: any): void {
    // no-op
  }
  isChatConnected = false;
  sendMessage(data: any, callback: () => void): void {
    if (callback) callback();
  }
  init(): void {
    // no-op
  }
}

export class MockUserNotificationHelper {
  info(message: string): void {
    // no-op
  }
  success(message: string): void {
    // no-op
  }
  warn(message: string): void {
    // no-op
  }
  error(message: string): void {
    // no-op
  }
  format(record: any, truncate: boolean): any {
    return { text: '', state: 'READ' };
  }
  setAsRead(id: string, callback: () => void): void {
    if (callback) callback();
  }
  setAllAsRead(callback: () => void): void {
    if (callback) callback();
  }
  openSettingsModal(): void {
    // no-op
  }
}

export class MockAppAuthenticationService {
  init(): Promise<boolean> {
    return Promise.resolve(true);
  }
}

export class MockAppAuthService {
  logout(): void {
    // no-op
  }
}

export class MockCookieService {
  get(key: string): string {
    return '';
  }
}

export class MockGoogleTagManagerService {
  addGtmToDom(): void {
    // no-op
  }
  pushTag(item: any): void {
    // no-op
  }
}

export class MockAppLocalizationService {
  l(key: string): string {
    return key;
  }
}

// ===== Service Proxy Mocks =====

import { of, Observable } from 'rxjs';

export class MockTokenAuthServiceProxy {
  authenticate(model: any): Observable<any> {
    return of({});
  }
  getExternalAuthenticationProviders(): Observable<any[]> {
    return of([]);
  }
  externalAuthenticate(model: any): Observable<any> {
    return of({});
  }
}

export class MockUserServiceProxy {
  getUsers(filter?: string, sorting?: string, maxResultCount?: number, skipCount?: number): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  deleteUser(id: number): Observable<void> {
    return of(undefined);
  }
  unlockUser(input: any): Observable<void> {
    return of(undefined);
  }
  closeSessionUser(id: number): Observable<void> {
    return of(undefined);
  }
  getUsersToExcel(): Observable<any> {
    return of({});
  }
}

export class MockRoleServiceProxy {
  getRoles(permission?: string, sorting?: string): Observable<any> {
    return of({ items: [] });
  }
  deleteRole(id: number): Observable<void> {
    return of(undefined);
  }
}

export class MockEditionServiceProxy {
  getEditions(filter?: string, sorting?: string, skipCount?: number, maxResultCount?: number): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getEditionForEdit(id: number): Observable<any> {
    return of({});
  }
  createEdition(input: any): Observable<void> {
    return of(undefined);
  }
  updateEdition(input: any): Observable<void> {
    return of(undefined);
  }
  deleteEdition(id: number): Observable<void> {
    return of(undefined);
  }
}

export class MockMassNotificationServiceProxy {
  getAll(...args: any[]): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  create(input: any): Observable<any> {
    return of({});
  }
  cancel(input: any): Observable<any> {
    return of({});
  }
}

export class MockUserDelegationServiceProxy {
  getMyDelegations(input: any): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getDelegatedUsers(input: any): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  create(input: any): Observable<any> {
    return of({});
  }
  cancel(id: number): Observable<any> {
    return of({});
  }
}

export class MockPaymentServiceProxy {
  getAll(...args: any[]): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  createPayment(input: any): Observable<any> {
    return of({});
  }
  processPayment(id: number, input: any): Observable<any> {
    return of({});
  }
  getGatewayList(): Observable<any[]> {
    return of([
      { name: 'Null', displayName: 'Null', isConfigured: true, isDefault: true },
      { name: 'Stripe', displayName: 'Stripe', isConfigured: true, isDefault: false },
      { name: 'PayPal', displayName: 'PayPal', isConfigured: true, isDefault: false },
      { name: 'MercadoPago', displayName: 'Mercado Pago', isConfigured: true, isDefault: false },
      { name: 'PagSeguro', displayName: 'PagSeguro', isConfigured: true, isDefault: false },
    ]);
  }
  getGatewaySettings(): Observable<any> {
    return of({});
  }
  updateGatewaySettings(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockOrganizationUnitServiceProxy {
  getOrganizationUnits(): Observable<any> {
    return of([]);
  }
  create(input: any): Observable<any> {
    return of({});
  }
  update(input: any): Observable<any> {
    return of({});
  }
  move(input: any): Observable<void> {
    return of(undefined);
  }
  delete(id: number): Observable<void> {
    return of(undefined);
  }
  getOrganizationUnitUsers(input: any): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getOrganizationUnitRoles(input: any): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  addUserToOrganizationUnit(input: any): Observable<void> {
    return of(undefined);
  }
  removeUserFromOrganizationUnit(input: any): Observable<void> {
    return of(undefined);
  }
  addRoleToOrganizationUnit(input: any): Observable<void> {
    return of(undefined);
  }
  removeRoleFromOrganizationUnit(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockDashboardServiceProxy {
  getHostDashboard(): Observable<any> {
    return of({ tiles: [], isHostDashboard: true });
  }
  getTenantDashboard(): Observable<any> {
    return of({ tiles: [], isHostDashboard: false });
  }
}

export class MockTenantServiceProxy {
  getTenants(filter?: string, sorting?: string, maxResultCount?: number, skipCount?: number): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  deleteTenant(id: number): Observable<void> {
    return of(undefined);
  }
  unlockTenantAdmin(input: any): Observable<void> {
    return of(undefined);
  }
  createTenant(input: any): Observable<void> {
    return of(undefined);
  }
  getTenantForEdit(id: number): Observable<any> {
    return of({});
  }
}

export class MockAuditLogServiceProxy {
  getAuditLogs(...args: any[]): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getAuditLogsToExcel(...args: any[]): Observable<any> {
    return of({});
  }
  getEntityChanges(...args: any[]): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getEntityChangesToExcel(...args: any[]): Observable<any> {
    return of({});
  }
  getEntityHistoryObjectTypes(): Observable<any[]> {
    return of([]);
  }
}

export class MockLanguageServiceProxy {
  getLanguages(filter?: string, sorting?: string): Observable<any> {
    return of({ defaultLanguageName: 'en', items: [] });
  }
  deleteLanguage(id: number): Observable<void> {
    return of(undefined);
  }
  setDefaultLanguage(input: any): Observable<void> {
    return of(undefined);
  }
  getAllLanguages(): Observable<any[]> {
    return of([]);
  }
}

export class MockHostSettingsServiceProxy {
  getAllSettings(): Observable<any> {
    return of({
      general: { timezone: 'UTC', timezoneForComparison: 'UTC' },
      userManagement: {},
      azureActiveDirectory: { isModuleEnabled: false, isEnabled: false },
      ldap: { isModuleEnabled: false, isEnabled: false },
      externalLoginProviderSettings: { openIdConnectClaimsMapping: [] },
    });
  }
  updateAllSettings(settings: any): Observable<void> {
    return of(undefined);
  }
  sendTestEmail(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockProfileServiceProxy {
  getProfilePicture(): Observable<any> {
    return of({ profilePicture: null });
  }
  changeLanguage(input: any): Observable<void> {
    return of(undefined);
  }
  changePassword(input: any): Observable<void> {
    return of(undefined);
  }
  updateCurrentUserProfile(input: any): Observable<void> {
    return of(undefined);
  }
  updateProfilePicture(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockNotificationServiceProxy {
  getUserNotifications(state?: any, maxResultCount?: number, skipCount?: number): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  deleteNotification(id: string): Observable<void> {
    return of(undefined);
  }
  getNotificationSettings(): Observable<any> {
    return of({ receiveNotifications: true, notifications: [] });
  }
  updateNotificationSettings(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockChatServiceProxy {
  getUserChatFriendsWithSettings(): Observable<any> {
    return of({ friends: [], serverTime: new Date() });
  }
  getUserChatMessages(minMessageId?: number, tenantId?: number, userId?: number, groupId?: string): Observable<any> {
    return of({ items: [] });
  }
  markAllUnreadMessagesOfUserAsRead(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockFriendshipServiceProxy {
  createFriendshipRequest(input: any): Observable<void> {
    return of(undefined);
  }
  createFriendshipRequestByUserName(input: any): Observable<void> {
    return of(undefined);
  }
  blockUser(input: any): Observable<void> {
    return of(undefined);
  }
  unblockUser(input: any): Observable<void> {
    return of(undefined);
  }
}

export class MockCommonLookupServiceProxy {
  findUsers(input: any): Observable<any> {
    return of({ items: [] });
  }
}

export class MockAccountServiceProxy {
  isTenantAvailable(input: any): Observable<any> {
    return of({ state: 1, tenantId: 1 });
  }
}

export class MockAirplanesServiceProxy {
  getAll(...args: any[]): Observable<any> {
    return of({ totalCount: 0, items: [] });
  }
  getAirplaneForEdit(id: number): Observable<any> {
    return of({});
  }
  createOrEdit(input: any): Observable<void> {
    return of(undefined);
  }
  delete(id: number): Observable<void> {
    return of(undefined);
  }
}

// ===== Utility Mocks =====

export class MockFileDownloadService {
  downloadTempFile(file: any): void {
    // no-op
  }
}

export class MockLocalStorageService {
  getItem(key: string, callback: (err: any, value: any) => void): void {
    callback(null, null);
  }
  setItem(key: string, value: any): void {
    // no-op
  }
}

export class MockImpersonationService {
  impersonate(userId: number, tenantId?: number): void {
    // no-op
  }
  backToImpersonator(): void {
    // no-op
  }
}

export class MockDateTimeService {
  getDate(): Date {
    return new Date();
  }
  fromISODateString(dateString: string): Date {
    return new Date(dateString);
  }
  plusSeconds(date: Date, seconds: number): Date {
    return new Date(date.getTime() + seconds * 1000);
  }
  getDiffInSeconds(date1: Date, date2: Date): number {
    return 0;
  }
}

export class MockLayoutRefService {
  layoutRef: any = {};
}

export class MockActivatedRoute {
  snapshot = {
    queryParams: {},
    params: {},
  };
}

export class MockSessionServiceProxy {
  getCurrentLoginInformations(): Observable<any> {
    return of({ user: {}, tenant: {} });
  }
}

// ===== Mock Pipes =====

@Pipe({ standalone: false, name: 'localize' })
export class MockLocalizePipe implements PipeTransform {
  transform(key: string, ...args: any[]): string {
    return key;
  }
}

// ===== Global `eaf` Mock Setup =====

export function setupEafGlobals(): void {
  (window as any).eaf = {
    session: {
      tenantId: null,
      userId: 1,
    },
    auth: {
      tokenCookieName: 'eaf.auth.token',
    },
    appPath: '/',
    setting: {
      get: (key: string) => null,
    },
    localization: {
      languages: [{ name: 'en', displayName: 'English', isDisabled: false }],
      currentLanguage: { name: 'en', displayName: 'English' },
    },
    event: {
      on: (eventName: string, callback: (...args: any[]) => void) => {},
      trigger: (eventName: string, ...args: any[]) => {},
    },
    notify: {
      info: (message: string) => {},
      success: (message: string) => {},
      warn: (message: string) => {},
      error: (message: string) => {},
    },
    message: {
      info: (message: string) => Promise.resolve(),
      success: (message: string) => {},
      warn: (message: string) => {},
      error: (message: string, title?: string) => {},
      confirm: (message: string, title?: string, callback?: (result: boolean) => void) => {
        if (callback) callback(true);
      },
    },
    ui: {
      setBusy: (element?: any) => {},
      clearBusy: (element?: any) => {},
    },
    utils: {
      formatString: (...args: any[]) => args[0] || '',
      truncateStringWithPostfix: (text: string, length: number) => (text ? text.substring(0, length) : ''),
    },
    log: {
      error: (message: string) => {},
      warn: (message: string) => {},
      info: (message: string) => {},
      debug: (message: string) => {},
    },
    clock: {
      provider: {
        supportsMultipleTimezone: false,
      },
    },
    notifications: {
      severity: {
        INFO: 0,
        SUCCESS: 1,
        WARN: 2,
        ERROR: 3,
        FATAL: 4,
      },
    },
    custom: {
      EntityHistory: {
        isEnabled: false,
        enabledEntities: [],
      },
    },
  };
}

// Initialize eaf globals when this module is imported
setupEafGlobals();
