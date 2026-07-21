import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { NEVER } from 'rxjs';
import { DefaultThemeUiSettingsComponent } from './default-theme-ui-settings.component';
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
import { UiCustomizationSettingsServiceProxy, ThemeSettingsDto } from '@shared/service-proxies/service-proxies';
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
  setupEafGlobals,
  MockLocalizePipe,
} from '../../../test-helpers/mock-services';

class MockUiCustomizationSettingsServiceProxy {
  updateDefaultUiManagementSettings = jasmine.createSpy('updateDefaultUiManagementSettings').and.returnValue(NEVER);
  updateUiManagementSettings = jasmine.createSpy('updateUiManagementSettings').and.returnValue(NEVER);
  useSystemDefaultSettings = jasmine.createSpy('useSystemDefaultSettings').and.returnValue(NEVER);
}

describe('DefaultThemeUiSettingsComponent', () => {
  let component: DefaultThemeUiSettingsComponent;
  let fixture: ComponentFixture<DefaultThemeUiSettingsComponent>;
  let mockUiService: MockUiCustomizationSettingsServiceProxy;

  beforeEach(() => {
    setupEafGlobals();
    mockUiService = new MockUiCustomizationSettingsServiceProxy();

    TestBed.configureTestingModule({
      declarations: [DefaultThemeUiSettingsComponent, MockLocalizePipe],
      providers: [
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
        { provide: UiCustomizationSettingsServiceProxy, useValue: mockUiService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(DefaultThemeUiSettingsComponent);
    component = fixture.componentInstance;
    component.settings = {
      theme: '',
      menu: {
        allowAsideMinimizing: false,
        allowAsideHiding: false,
        defaultHiddenAside: false,
        defaultMinimizedAside: false,
      },
    } as any;
  });

  it('deve ser criado', () => {
    expect(component).toBeTruthy();
  });

  describe('getCustomizedSetting', () => {
    it('deve definir theme como "default"', () => {
      const settings = { theme: '', menu: {} } as any;
      const result = component.getCustomizedSetting(settings);
      expect(result.theme).toBe('default');
    });

    it('deve retornar o mesmo objeto settings', () => {
      const settings = { theme: 'other', menu: {} } as any;
      const result = component.getCustomizedSetting(settings);
      expect(result).toBe(settings);
    });
  });

  describe('allowAsideMinimizingChange', () => {
    it('deve resetar allowAsideHiding e defaultHiddenAside quando val é true', () => {
      component.settings.menu.allowAsideHiding = true;
      component.settings.menu.defaultHiddenAside = true;

      component.allowAsideMinimizingChange(true);

      expect(component.settings.menu.allowAsideHiding).toBe(false);
      expect(component.settings.menu.defaultHiddenAside).toBe(false);
    });

    it('deve resetar defaultMinimizedAside quando val é false', () => {
      component.settings.menu.defaultMinimizedAside = true;

      component.allowAsideMinimizingChange(false);

      expect(component.settings.menu.defaultMinimizedAside).toBe(false);
    });
  });

  describe('allowAsideHidingChange', () => {
    it('deve resetar defaultHiddenAside quando val é false', () => {
      component.settings.menu.defaultHiddenAside = true;

      component.allowAsideHidingChange(false);

      expect(component.settings.menu.defaultHiddenAside).toBe(false);
    });

    it('não deve alterar defaultHiddenAside quando val é true', () => {
      component.settings.menu.defaultHiddenAside = true;

      component.allowAsideHidingChange(true);

      expect(component.settings.menu.defaultHiddenAside).toBe(true);
    });
  });

  describe('updateDefaultUiManagementSettings', () => {
    it('deve chamar o serviço com settings customizados', () => {
      component.updateDefaultUiManagementSettings();

      expect(mockUiService.updateDefaultUiManagementSettings).toHaveBeenCalled();
      const args = mockUiService.updateDefaultUiManagementSettings.calls.first().args[0];
      expect(args.theme).toBe('default');
    });
  });

  describe('updateUiManagementSettings', () => {
    it('deve chamar o serviço com settings customizados', () => {
      component.updateUiManagementSettings();

      expect(mockUiService.updateUiManagementSettings).toHaveBeenCalled();
      const args = mockUiService.updateUiManagementSettings.calls.first().args[0];
      expect(args.theme).toBe('default');
    });
  });

  describe('useSystemDefaultSettings', () => {
    it('deve chamar o serviço', () => {
      component.useSystemDefaultSettings();

      expect(mockUiService.useSystemDefaultSettings).toHaveBeenCalled();
    });
  });
});
