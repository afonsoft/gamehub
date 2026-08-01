import { NgModule } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';

import { AuditLogsComponent } from './audit-logs/audit-logs.component';
import { EditionsComponent } from './editions/editions.component';
import { LanguageTextsComponent } from './languages/language-texts.component';
import { LanguagesComponent } from './languages/languages.component';
import { MaintenanceComponent } from './maintenance/maintenance.component';
import { MassNotificationsComponent } from './mass-notifications/mass-notifications.component';
import { OrganizationUnitsComponent } from './organization-units/organization-units.component';
import { PaymentsComponent } from './payments/payments.component';
import { RolesComponent } from './roles/roles.component';
import { UserDelegationsComponent } from './user-delegations/user-delegations.component';
import { SettingsComponent } from './settings/settings.component';
import { TenantsComponent } from './tenants/tenants.component';
import { UiCustomizationComponent } from './ui-customization/ui-customization.component';
import { UsersComponent } from './users/users.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: '',
        children: [
          { path: 'users', component: UsersComponent, data: { permission: 'Pages.Administration.Users' } },
          { path: 'roles', component: RolesComponent, data: { permission: 'Pages.Administration.Roles' } },
          { path: 'auditLogs', component: AuditLogsComponent, data: { permission: 'Pages.Administration.AuditLogs' } },
          { path: 'editions', component: EditionsComponent, data: { permission: 'Pages.Administration.Editions' } },
          { path: 'languages', component: LanguagesComponent, data: { permission: 'Pages.Administration.Languages' } },
          { path: 'organization-units', component: OrganizationUnitsComponent, data: { permission: 'Pages.Administration.OrganizationUnits' } },
          { path: 'mass-notifications', component: MassNotificationsComponent, data: { permission: 'Pages.Administration.MassNotifications' } },
          { path: 'user-delegations', component: UserDelegationsComponent, data: { permission: 'Pages.Administration.Users.Delegation' } },
          { path: 'payments', component: PaymentsComponent, data: { permission: 'Pages.Administration.Payments' } },
          {
            path: 'languages/:name/texts',
            component: LanguageTextsComponent,
            data: { permission: 'Pages.Administration.Languages.ChangeTexts' },
          },
          { path: 'tenants', component: TenantsComponent, data: { permission: 'Pages.Tenants' } },
          { path: 'settings', component: SettingsComponent, data: { permission: 'Pages.Administration.Settings' } },
          { path: 'maintenance', component: MaintenanceComponent, data: { permission: 'Pages.Administration.Maintenance' } },
          { path: 'ui-customization', component: UiCustomizationComponent, data: { permission: 'Pages.Administration.UiCustomization' } },
        ],
      },
    ]),
  ],
  exports: [RouterModule],
})
export class AdminRoutingModule {
  constructor(private readonly router: Router) {
    router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        window.scroll(0, 0);
      }
    });
  }
}
