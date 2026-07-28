import { EafModule } from '@eaf/eaf.module';
import * as ngCommon from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClientJsonpModule } from '@angular/common/http';
import { CommonModule } from '@shared/common/common.module';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { UtilsModule } from '@shared/utils/utils.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { AccountRoutingModule } from './account-routing.module';
import { AccountComponent } from './account.component';
import { AccountRouteGuard } from './account-route-guard';
import { ConfirmEmailComponent } from './email-activation/confirm-email.component';
import { EmailActivationComponent } from './email-activation/email-activation.component';
import { LoginComponent } from './login/login.component';
import { LoginService } from './login/login.service';
import { SelectTenantComponent } from './login/select-tenant/select-tenant.component';
import { ForgotPasswordComponent } from './password/forgot-password.component';
import { ResetPasswordComponent } from './password/reset-password.component';
import { OAuthModule } from 'angular-oauth2-oidc';
import { SsoComponent } from './login/sso.component';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@NgModule({
  imports: [
    ngCommon.CommonModule,
    FormsModule,
    HttpClientModule,
    HttpClientJsonpModule,
    ModalModule.forRoot(),
    EafModule,
    CommonModule,
    UtilsModule,
    ServiceProxyModule,
    AccountRoutingModule,
    OAuthModule.forRoot(),
    BsDatepickerModule.forRoot(),
        BsDropdownModule.forRoot(),
  ],
  declarations: [
    AccountComponent,
    LoginComponent,
    SelectTenantComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    EmailActivationComponent,
    SsoComponent,
    ConfirmEmailComponent,
  ],
  providers: [LoginService, AccountRouteGuard],
})
export class AccountModule {}
