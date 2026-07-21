import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { EafHttpInterceptor } from '../../assets/lib/eaf-ng2-module/src/eafHttpInterceptor';
import * as ApiServiceProxies from './service-proxies';

@NgModule({
  providers: [
    ApiServiceProxies.AirplanesServiceProxy,
    ApiServiceProxies.CachingServiceProxy,
    ApiServiceProxies.WebLogServiceProxy,
    ApiServiceProxies.AuditLogServiceProxy,
    ApiServiceProxies.CommonLookupServiceProxy,
    ApiServiceProxies.HostSettingsServiceProxy,
    ApiServiceProxies.LanguageServiceProxy,
    ApiServiceProxies.NotificationServiceProxy,
    ApiServiceProxies.PermissionServiceProxy,
    ApiServiceProxies.ProfileServiceProxy,
    ApiServiceProxies.RoleServiceProxy,
    ApiServiceProxies.SessionServiceProxy,
    ApiServiceProxies.TenantServiceProxy,
    ApiServiceProxies.TimingServiceProxy,
    ApiServiceProxies.UserServiceProxy,
    ApiServiceProxies.UserLoginServiceProxy,
    ApiServiceProxies.AccountServiceProxy,
    ApiServiceProxies.TokenAuthServiceProxy,
    ApiServiceProxies.UiCustomizationSettingsServiceProxy,
    ApiServiceProxies.FileServiceProxy,
    ApiServiceProxies.FriendshipServiceProxy,
    ApiServiceProxies.ChatServiceProxy,
    ApiServiceProxies.AboutServiceProxy,
    ApiServiceProxies.TenantAddressServiceProxy,
    ApiServiceProxies.WebhookSubscriptionServiceProxy,
    { provide: HTTP_INTERCEPTORS, useClass: EafHttpInterceptor, multi: true }
  ],
})
export class ServiceProxyModule {}
