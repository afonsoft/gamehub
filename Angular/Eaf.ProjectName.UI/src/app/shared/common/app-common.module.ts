import { EafModule } from '@eaf/eaf.module';
import * as ngCommon from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLocalizationService } from '@app/shared/common/localization/app-localization.service';
import { AppNavigationService } from '@app/shared/layout/nav/app-navigation.service';
import { CommonModule } from '@shared/common/common.module';
import { UtilsModule } from '@shared/utils/utils.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginatorModule } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { AppAuthService } from './auth/app-auth.service';
import { AppRouteGuard } from './auth/auth-route-guard';
import { CommonLookupModalComponent } from './lookup/common-lookup-modal.component';
import { EntityTypeHistoryModalComponent } from './entityHistory/entity-type-history-modal.component';
import { EntityChangeDetailModalComponent } from './entityHistory/entity-change-detail-modal.component';
import { DateRangePickerInitialValueSetterDirective } from './timing/date-range-picker-initial-value.directive';
import { DatePickerInitialValueSetterDirective } from './timing/date-picker-initial-value.directive';
import { DateTimeService } from './timing/date-time.service';
import { TimeZoneComboComponent } from './timing/timezone-combo.component';
import { KeyValueListManagerComponent } from './key-value-list-manager/key-value-list-manager.component';

@NgModule({
  imports: [
    ngCommon.CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ModalModule.forRoot(),
    UtilsModule,
    EafModule,
    CommonModule,
    TableModule,
    PaginatorModule,
  ],
  declarations: [
    TimeZoneComboComponent,
    CommonLookupModalComponent,
    EntityTypeHistoryModalComponent,
    EntityChangeDetailModalComponent,
    DateRangePickerInitialValueSetterDirective,
    DatePickerInitialValueSetterDirective,
    KeyValueListManagerComponent,
  ],
  exports: [
    TimeZoneComboComponent,
    CommonLookupModalComponent,
    EntityTypeHistoryModalComponent,
    EntityChangeDetailModalComponent,
    DateRangePickerInitialValueSetterDirective,
    DatePickerInitialValueSetterDirective,
    KeyValueListManagerComponent,
  ],
  providers: [DateTimeService, AppLocalizationService, AppNavigationService],
})
export class AppCommonModule {
  static forRoot(): ModuleWithProviders<AppCommonModule> {
    return {
      ngModule: AppCommonModule,
      providers: [AppAuthService, AppRouteGuard],
    };
  }
}
