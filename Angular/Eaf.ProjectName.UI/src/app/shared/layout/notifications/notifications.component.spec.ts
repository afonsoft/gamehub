import { Router } from '@angular/router';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA, ChangeDetectorRef } from '@angular/core';
import { NotificationsComponent } from './notifications.component';
import { NotificationServiceProxy } from '@shared/service-proxies/service-proxies';
import { UserNotificationHelper } from './UserNotificationHelper';
import { LocalizationService } from '@eaf/localization/localization.service';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { MessageService } from '@eaf/message/message.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import {
  MockLocalizationService,
  MockPermissionCheckerService,
  MockFeatureCheckerService,
  MockMessageService,
  MockNotifyService,
  MockSettingService,
  MockEafMultiTenancyService,
  MockAppSessionService,
  MockAppUiCustomizationService,
  MockAppUrlService,
  MockNotificationServiceProxy,
  MockUserNotificationHelper,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../../test-helpers/mock-services';

describe('NotificationsComponent', () => {
  let component: NotificationsComponent;
  let fixture: ComponentFixture<NotificationsComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      declarations: [NotificationsComponent, MockLocalizePipe],
      providers: [
        { provide: Router, useValue: { navigate: () => {}, events: { subscribe: () => {} }, url: '/' } },
        { provide: NotificationServiceProxy, useClass: MockNotificationServiceProxy },
        { provide: UserNotificationHelper, useClass: MockUserNotificationHelper },
        { provide: ChangeDetectorRef, useValue: { detectChanges: () => {} } },
        { provide: LocalizationService, useClass: MockLocalizationService },
        { provide: PermissionCheckerService, useClass: MockPermissionCheckerService },
        { provide: FeatureCheckerService, useClass: MockFeatureCheckerService },
        { provide: MessageService, useClass: MockMessageService },
        { provide: NotifyService, useClass: MockNotifyService },
        { provide: SettingService, useClass: MockSettingService },
        { provide: EafMultiTenancyService, useClass: MockEafMultiTenancyService },
        { provide: AppSessionService, useClass: MockAppSessionService },
        { provide: AppUiCustomizationService, useClass: MockAppUiCustomizationService },
        { provide: AppUrlService, useClass: MockAppUrlService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have readStateFilter initialized to ALL', () => {
    expect(component.readStateFilter).toBe('ALL');
  });

  it('should have loading set to false initially', () => {
    expect(component.loading).toBeFalsy();
  });

  it('should check if record is read', () => {
    const readRecord = { formattedNotification: { state: 'READ' } };
    const unreadRecord = { formattedNotification: { state: 'UNREAD' } };
    expect(component.isRead(readRecord)).toBeTruthy();
    expect(component.isRead(unreadRecord)).toBeFalsy();
  });

  it('should format notifications', () => {
    const records = [
      { id: '1' },
      { id: '2' },
    ];
    const formatted = component.formatNotifications(records);
    expect(formatted).toHaveSize(2);
    expect(formatted[0].formattedNotification).toBeDefined();
  });

  it('should get row class for read notification', () => {
    const readRecord = { state: 'READ' } as any;
    expect(component.getRowClass(readRecord)).toBe('notification-read');
  });

  it('should get empty row class for unread notification', () => {
    const unreadRecord = { state: 'UNREAD' } as any;
    expect(component.getRowClass(unreadRecord)).toBe('');
  });
});
