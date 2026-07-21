import { Injectable } from '@angular/core';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import {
  ApplicationInfoDto,
  GetCurrentLoginInformationsOutput,
  SessionServiceProxy,
  TenantLoginInfoDto,
  UiCustomizationSettingsDto,
  UserLoginInfoDto,
} from '@shared/service-proxies/service-proxies';

@Injectable()
export class AppSessionService {
  private _user: UserLoginInfoDto;
  private _tenant: TenantLoginInfoDto;
  private _application: ApplicationInfoDto;
  private _theme: UiCustomizationSettingsDto;

  constructor(
    private readonly _sessionService: SessionServiceProxy,
    private readonly _eafMultiTenancyService: EafMultiTenancyService,
  ) {}

  get application(): ApplicationInfoDto {
    return this._application;
  }

  set application(val: ApplicationInfoDto) {
    this._application = val;
  }

  get user(): UserLoginInfoDto {
    return this._user;
  }

  get userId(): number {
    return this.user ? this.user.id : null;
  }

  get tenant(): TenantLoginInfoDto {
    return this._tenant;
  }

  get tenancyName(): string {
    return this._tenant ? this.tenant.tenancyName : '';
  }

  get tenantId(): number {
    return this.tenant ? this.tenant.id : null;
  }

  getShownLoginName(): string {
    const userName = this._user.userName;
    if (!this._eafMultiTenancyService.isEnabled) {
      return userName;
    }

    return (this._tenant ? this._tenant.tenancyName : '.') + '\\' + userName;
  }

  getShownFullName(): string {
    return this._user.name + ' ' + this._user.surname;
  }

  get theme(): UiCustomizationSettingsDto {
    return this._theme;
  }

  set theme(val: UiCustomizationSettingsDto) {
    this._theme = val;
  }

  init(): Promise<UiCustomizationSettingsDto> {
    return new Promise<UiCustomizationSettingsDto>((resolve, reject) => {
      this._sessionService
        .getCurrentLoginInformations()
        .toPromise()
        .then(
          (result: GetCurrentLoginInformationsOutput) => {
            this._application = result.application;
            this._user = result.user;
            this._tenant = result.tenant;
            this._theme = result.theme;
            resolve(result.theme);
          },
          err => {
            reject(err);
          },
        );
    });
  }

  changeTenantIfNeeded(tenantId?: number): boolean {
    if (this.isCurrentTenant(tenantId)) {
      return false;
    }

    eaf.multiTenancy.setTenantIdCookie(tenantId);
    location.reload();
    return true;
  }

  private isCurrentTenant(tenantId?: number) {
    const isTenant = tenantId > 0;

    if (!isTenant && !this.tenant) {
      // this is host
      return true;
    }

    if ((!tenantId && this.tenant) || (tenantId && this.tenant?.id !== tenantId)) {
      return false;
    }

    return true;
  }
}
