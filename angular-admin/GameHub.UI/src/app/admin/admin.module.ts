import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
import { UtilsModule } from '@shared/utils/utils.module';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { NgxBootstrapDatePickerConfigService } from 'assets/lib/ngx-bootstrap/ngx-bootstrap-datepicker-config.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PopoverModule } from 'ngx-bootstrap/popover';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { BsDatepickerConfig, BsDatepickerModule, BsDaterangepickerConfig, BsLocaleService } from 'ngx-bootstrap/datepicker';
import { TreeDragDropService } from 'primeng/api';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ContextMenuModule } from 'primeng/contextmenu';
import { DragDropModule } from 'primeng/dragdrop';
import { EditorModule } from 'primeng/editor';
import { NgxFileDropModule } from 'ngx-file-drop';
import { FileUploadModule as PrimeNgFileUploadModule } from 'primeng/fileupload';
import { InputMaskModule } from 'primeng/inputmask';
import { PaginatorModule } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { TreeModule } from 'primeng/tree';

import { AdminRoutingModule } from './admin-routing.module';
import { AuditLogDetailModalComponent } from './audit-logs/audit-log-detail-modal.component';
import { AuditLogsComponent } from './audit-logs/audit-logs.component';
import { CreateOrEditLanguageModalComponent } from './languages/create-or-edit-language-modal.component';
import { EditTextModalComponent } from './languages/edit-text-modal.component';
import { LanguageTextsComponent } from './languages/language-texts.component';
import { LanguagesComponent } from './languages/languages.component';
import { MaintenanceComponent } from './maintenance/maintenance.component';
import { CreateOrEditRoleModalComponent } from './roles/create-or-edit-role-modal.component';
import { RolesComponent } from './roles/roles.component';
import { SettingsComponent } from './settings/settings.component';
import { FeatureTreeComponent } from './shared/feature-tree.component';
import { PermissionComboComponent } from './shared/permission-combo.component';
import { PermissionTreeComponent } from './shared/permission-tree.component';
import { RoleComboComponent } from './shared/role-combo.component';
import { CreateTenantModalComponent } from './tenants/create-tenant-modal.component';
import { EditTenantModalComponent } from './tenants/edit-tenant-modal.component';
import { TenantFeaturesModalComponent } from './tenants/tenant-features-modal.component';
import { TenantsComponent } from './tenants/tenants.component';
import { DefaultThemeUiSettingsComponent } from './ui-customization/default-theme-ui-settings.component';
import { Theme2ThemeUiSettingsComponent } from './ui-customization/theme2-theme-ui-settings.component';
import { Theme3ThemeUiSettingsComponent } from './ui-customization/theme3-theme-ui-settings.component';
import { Theme4ThemeUiSettingsComponent } from './ui-customization/theme4-theme-ui-settings.component';
import { UiCustomizationComponent } from './ui-customization/ui-customization.component';
import { CreateOrEditUserModalComponent } from './users/create-or-edit-user-modal.component';
import { EditUserPermissionsModalComponent } from './users/edit-user-permissions-modal.component';
import { ImpersonationService } from './users/impersonation.service';
import { UsersComponent } from './users/users.component';

@NgModule({
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    NgxFileDropModule,
    PrimeNgFileUploadModule,
    ModalModule.forRoot(),
    TabsModule.forRoot(),
    TooltipModule.forRoot(),
    PopoverModule.forRoot(),
    BsDropdownModule.forRoot(),
    BsDatepickerModule.forRoot(),
    AdminRoutingModule,
    UtilsModule,
    AppCommonModule,
    TableModule,
    TreeModule,
    DragDropModule,
    ContextMenuModule,
    PaginatorModule,
    PrimeNgFileUploadModule,
    AutoCompleteModule,
    EditorModule,
    InputMaskModule,
    NgxChartsModule,
  ],
  declarations: [
    UsersComponent,
    PermissionComboComponent,
    RoleComboComponent,
    CreateOrEditUserModalComponent,
    EditUserPermissionsModalComponent,
    PermissionTreeComponent,
    FeatureTreeComponent,
    RolesComponent,
    CreateOrEditRoleModalComponent,
    AuditLogsComponent,
    AuditLogDetailModalComponent,
    SettingsComponent,
    LanguagesComponent,
    LanguageTextsComponent,
    CreateOrEditLanguageModalComponent,
    TenantsComponent,
    CreateTenantModalComponent,
    EditTenantModalComponent,
    TenantFeaturesModalComponent,
    CreateOrEditLanguageModalComponent,
    EditTextModalComponent,
    UiCustomizationComponent,
    MaintenanceComponent,
    DefaultThemeUiSettingsComponent,
    Theme4ThemeUiSettingsComponent,
    Theme2ThemeUiSettingsComponent,
    Theme3ThemeUiSettingsComponent,
  ],
  exports: [],
  providers: [
    ImpersonationService,
    TreeDragDropService,
    { provide: BsDatepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerConfig },
    { provide: BsDaterangepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDaterangepickerConfig },
    { provide: BsLocaleService, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerLocale },
  ],
})
export class AdminModule {}
