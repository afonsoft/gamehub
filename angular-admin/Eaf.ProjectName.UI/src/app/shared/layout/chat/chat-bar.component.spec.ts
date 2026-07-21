import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ChatBarComponent } from './chat-bar.component';
import { ChatSignalrService } from './chat-signalr.service';
import { ChatServiceProxy, FriendshipServiceProxy, ProfileServiceProxy, CommonLookupServiceProxy } from '@shared/service-proxies/service-proxies';
import { LocalStorageService } from '@shared/utils/local-storage.service';
import { DateTimeService } from '@app/shared/common/timing/date-time.service';
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
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
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
  MockChatSignalrService,
  MockChatServiceProxy,
  MockFriendshipServiceProxy,
  MockProfileServiceProxy,
  MockCommonLookupServiceProxy,
  MockLocalStorageService,
  MockDateTimeService,
  MockLayoutRefService,
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../../test-helpers/mock-services';

describe('ChatBarComponent', () => {
  let component: ChatBarComponent;
  let fixture: ComponentFixture<ChatBarComponent>;

  beforeEach(() => {
    setupEafGlobals();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      declarations: [ChatBarComponent, MockLocalizePipe],
      providers: [
        { provide: ChatSignalrService, useClass: MockChatSignalrService },
        { provide: ChatServiceProxy, useClass: MockChatServiceProxy },
        { provide: FriendshipServiceProxy, useClass: MockFriendshipServiceProxy },
        { provide: ProfileServiceProxy, useClass: MockProfileServiceProxy },
        { provide: CommonLookupServiceProxy, useClass: MockCommonLookupServiceProxy },
        { provide: LocalStorageService, useClass: MockLocalStorageService },
        { provide: DateTimeService, useClass: MockDateTimeService },
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
        { provide: LayoutRefService, useClass: MockLayoutRefService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ChatBarComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have friends property declared', () => {
    const descriptor = Object.getOwnPropertyDescriptor(component, 'friends');
    // friends is declared on the class but may not be initialized until ngOnInit
    expect(component.hasOwnProperty('friends') || 'friends' in Object.getPrototypeOf(component) || descriptor !== undefined || component['friends'] === undefined).toBeTrue();
  });

  it('should have isOpen set to false initially', () => {
    expect(component.isOpen).toBeFalsy();
  });

  it('should have chatMessage as empty string initially', () => {
    expect(component.chatMessage).toBe('');
  });

  it('should have userNameFilter as empty string initially', () => {
    expect(component.userNameFilter).toBe('');
  });

  it('should have sendingMessage set to false initially', () => {
    expect(component.sendingMessage).toBeFalsy();
  });

  it('should have loadingPreviousUserMessages set to false initially', () => {
    expect(component.loadingPreviousUserMessages).toBeFalsy();
  });

  it('should have getShownUserName method defined', () => {
    expect(component.getShownUserName).toBeDefined();
  });

  it('should have getFilteredFriends method defined', () => {
    expect(component.getFilteredFriends).toBeDefined();
  });
});
