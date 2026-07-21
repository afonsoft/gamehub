import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { AppTimezoneScope } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  SettingScopes,
  HostSettingsEditDto,
  HostSettingsServiceProxy,
  SendTestEmailInput,
  JsonClaimMapDto,
} from '@shared/service-proxies/service-proxies';
import { AppConsts } from '@shared/AppConsts';
import { KeyValueListManagerComponent } from '@app/shared/common/key-value-list-manager/key-value-list-manager.component';

@Component({
  standalone: false,
  templateUrl: './settings.component.html',
  animations: [appModuleAnimation()],
})
export class SettingsComponent extends AppComponentBase implements OnInit {
  @ViewChild('openIdConnectClaimsMappingManager', { static: true }) openIdConnectClaimsMappingManager: KeyValueListManagerComponent;

  loading = false;
  hostSettings: HostSettingsEditDto;
  testEmailAddress: string = undefined;
  showTimezoneSelection = eaf.clock.provider.supportsMultipleTimezone;
  defaultTimezoneScope: SettingScopes = AppTimezoneScope.Application;

  usingDefaultTimeZone = false;
  openIdConnectClaimMappings: { key: string; value: string }[];
  enabledSocialLoginSettings: string[];
  initialTimeZone: string = undefined;

  constructor(
    injector: Injector,
    private readonly _hostSettingService: HostSettingsServiceProxy,
  ) {
    super(injector);
  }

  loadHostSettings(): void {

    this._hostSettingService.getAllSettings().subscribe(setting => {
      this.hostSettings = setting;
      this.initialTimeZone = setting.general.timezone;
      this.usingDefaultTimeZone = setting.general.timezoneForComparison === this.setting.get('Eaf.Timing.TimeZone');
      this.openIdConnectClaimMappings = this.hostSettings.externalLoginProviderSettings.openIdConnectClaimsMapping.map(item => {
        return {
          key: item.key,
          value: item.claim,
        };
      });
    });
  }

  init(): void {

    this.testEmailAddress = this.appSession.user.emailAddress;
    this.showTimezoneSelection = eaf.clock.provider.supportsMultipleTimezone;
    this.loadHostSettings();
    this.loadSocialLoginSettings();
  }

  ngOnInit(): void {

    this.init();
  }

  sendTestEmail(): void {

    const input = new SendTestEmailInput();
    input.emailAddress = this.testEmailAddress;
    this._hostSettingService.sendTestEmail(input).subscribe(result => {
      this.notify.success(this.l('TestEmailSentSuccessfully'));
    });
  }

  saveAll(): void {

    this.loading = true;
    this._hostSettingService.updateAllSettings(this.hostSettings).subscribe(
      result => {
        this.loading = false;
        this.notify.success(this.l('SavedSuccessfully'));

        AppConsts.appActiveDirectoryEnabled =
          this.hostSettings.azureActiveDirectory.isModuleEnabled && this.hostSettings.azureActiveDirectory.isEnabled;

        AppConsts.appLdapEnabled = this.hostSettings.ldap.isModuleEnabled && this.hostSettings.ldap.isEnabled;

        if (
          eaf.clock.provider.supportsMultipleTimezone &&
          this.usingDefaultTimeZone &&
          this.initialTimeZone !== this.hostSettings.general.timezone
        ) {
          this.message.info(this.l('TimeZoneSettingChangedRefreshPageNotification')).then(() => {
            window.location.reload();
          });
        }
      },
      err => {
        this.loading = false;
        (window as any).eaf.log.error(err);
      },
    );
  }

  loadSocialLoginSettings(): void {

    this.enabledSocialLoginSettings = ['Google', 'Microsoft', 'OpenId', 'Auth0'];
  }

  clearAdSettings(): void {


    if (this.hostSettings.azureActiveDirectory.isEnabled) {
      this.message.confirm(this.l('UserActiveDirectoryDeleteAllWarningMessage'), this.l('AreYouSure'), isConfirmed => {
        if (isConfirmed) {
          this.hostSettings.azureActiveDirectory.clientId = '';
          this.hostSettings.azureActiveDirectory.clientSecret = '';
          this.hostSettings.azureActiveDirectory.tenant = '';

          this.hostSettings.userManagement.isRegisterRequiredForLogin = false;
          this.hostSettings.userManagement.storeExternalTokenInformation = true;
        } else {
          this.hostSettings.azureActiveDirectory.isEnabled = true;
        }
      });
    }
  }

  clearLdapSettings(): void {


    if (this.hostSettings.ldap.isEnabled) {
      this.message.confirm(this.l('UserLdapDeleteAllWarningMessage'), this.l('AreYouSure'), isConfirmed => {
        if (isConfirmed) {
          this.hostSettings.ldap.domain = '';
          this.hostSettings.ldap.userName = '';
          this.hostSettings.ldap.password = '';

          this.hostSettings.userManagement.isRegisterRequiredForLogin = false;
          this.hostSettings.userManagement.storeExternalTokenInformation = true;
        } else {
          this.hostSettings.ldap.isEnabled = true;
        }
      });
    }
  }

  mapClaims(): void {
    if (this.openIdConnectClaimsMappingManager) {
      this.hostSettings.externalLoginProviderSettings.openIdConnectClaimsMapping = this.openIdConnectClaimsMappingManager.getItems().map(
        item =>
          new JsonClaimMapDto({
            key: item.key,
            claim: item.value,
          }),
      );
    }
  }
}
