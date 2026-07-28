import { Component, Injector, OnInit, ViewEncapsulation } from '@angular/core';
import { ImpersonationService } from '@app/admin/users/impersonation.service';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { EafSessionService } from '@eaf/session/eaf-session.service';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ChangeUserLanguageDto, ProfileServiceProxy } from '@shared/service-proxies/service-proxies';


@Component({
  standalone: false,
  templateUrl: './topbar.component.html',
  selector: 'topbar',
  encapsulation: ViewEncapsulation.None,
})
export class TopBarComponent extends AppComponentBase implements OnInit {
  languages: eaf.localization.ILanguageInfo[];
  currentLanguage: eaf.localization.ILanguageInfo;
  isImpersonatedLogin = false;
  isMultiTenancyEnabled = false;
  shownLoginName = '';
  shownFullName = '';
  tenancyName = '';
  userName = '';
  profilePicture = AppConsts.appBaseUrl + '/assets/common/images/nopicture.png';
  defaultLogo = AppConsts.appBaseUrl + '/assets/common/images/eaf/eaf-' + this.currentTheme.baseSettings.menu.asideSkin + '.png';
  remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;
  unreadChatMessageCount = 0;
  chatConnected = false;
  isSystemUser = true;
  showChatMenu = false;

  constructor(
    injector: Injector,
    private readonly _eafSessionService: EafSessionService,
    private readonly _eafMultiTenancyService: EafMultiTenancyService,
    private readonly _profileServiceProxy: ProfileServiceProxy,
    private readonly _authService: AppAuthService,
    private readonly _impersonationService: ImpersonationService,
    private readonly _storageService: StorageService,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.isMultiTenancyEnabled = this._eafMultiTenancyService.isEnabled;
    this.languages = this.localization.languages?.filter(l => !l.isDisabled) || [];

    this.currentLanguage = this.localization.currentLanguage;
    this.isImpersonatedLogin = this._eafSessionService.impersonatorUserId > 0;

    this.showChatMenu = this.feature.isEnabled('App.ChatFeature');

    this.setCurrentLoginInformations();
    this.getProfilePicture();

    this.registerToEvents();
  }

  registerToEvents() {
    eaf.event.on('app.profilePictureChanged', () => {
      this.getProfilePicture();
    });

    eaf.event.on('app.onMySettingsModalSaved', () => {
      this.onMySettingsModalSaved();
    });

    eaf.event.on('app.languagesChanged', () => {
      this.reloadLanguages();
    });

    eaf.event.on('app.chat.unreadMessageCountChanged', messageCount => {
      this.unreadChatMessageCount = messageCount;
    });

    eaf.event.on('app.chat.connected', () => {
      this.chatConnected = true;
    });
  }

  showChat(id: string): void {
    const side = document.getElementById(id);
    side.classList.add('mr-0');
  }

  changeLanguage(languageName: string): void {
    const input = new ChangeUserLanguageDto();
    input.languageName = languageName;

    this._storageService.setCookieValue(
      'Abp.Localization.CultureName',
      languageName,
      new Date(Date.now() + 5 * 365 * 86400000), //5 year
      eaf.appPath,
    );
    this._profileServiceProxy.changeLanguage(input).subscribe(() => {
      window.location.reload();
    });
  }

  reloadLanguages(): void {
    this.languages = this.localization.languages?.filter(l => !(<any>l).isDisabled) || [];
  }

  setCurrentLoginInformations(): void {
    const user = this.appSession.user;
    this.shownLoginName = user ? this.appSession.getShownLoginName() : '';
    this.shownFullName = user ? `${user.name} ${user.surname || ''}`.trim() : '';
    this.tenancyName = this.appSession.tenancyName || '';
    this.userName = user ? user.userName : '';
    this.isSystemUser = user ? user.authenticationSource == undefined : true;
  }

  getProfilePicture(): void {
    this._profileServiceProxy.getProfilePicture().subscribe(result => {
      if (result?.profilePicture) {
        this.profilePicture = 'data:image/jpeg;base64,' + result.profilePicture;
      }
    });
  }

  showLoginAttempts(): void {
    eaf.event.trigger('app.show.loginAttemptsModal');
  }

  changePassword(): void {
    eaf.event.trigger('app.show.changePasswordModal');
  }

  changeProfilePicture(): void {
    eaf.event.trigger('app.show.changeProfilePictureModal');
  }

  changeMySettings(): void {
    eaf.event.trigger('app.show.mySettingsModal');
  }

  logout(): void {
    this._authService.logout();
  }

  onMySettingsModalSaved(): void {
    this.shownLoginName = this.appSession.getShownLoginName();
  }

  backToMyAccount(): void {
    this._impersonationService.backToImpersonator();
  }
}
