import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
import { UtilsModule } from '@shared/utils/utils.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TableModule } from 'primeng/table';
import { PaginatorModule } from 'primeng/paginator';
import { GameHubAdminRoutingModule } from './gamehub-routing.module';

import { DashboardComponent } from './dashboard/dashboard.component';
import { GameListComponent } from './games/game-list.component';
import { ReviewQueueComponent } from './moderation/review-queue.component';
import { CategoryListComponent } from './categories/category-list.component';
import { CategoryEditComponent } from './categories/category-edit.component';
import { TagListComponent } from './tags/tag-list.component';
import { TagEditComponent } from './tags/tag-edit.component';
import { FeatureFlagsComponent } from './dashboard/feature-flags.component';
import { AuditLogComponent } from './dashboard/audit-log.component';
import { GameDetailComponent } from './games/game-detail.component';
import { ReviewDetailComponent } from './moderation/review-detail.component';
import { UserListComponent } from './users/user-list.component';
import { BuildListComponent } from './uploads/build-list.component';
import { BuildFilesComponent } from './uploads/build-files.component';
import { ReportListComponent } from './reports/report-list.component';
import { InspectorComponent } from './inspector/inspector.component';
import { InspectorSessionComponent } from './inspector/inspector-session.component';
import { PlaytestRecordingListComponent } from './playtest/playtest-recording-list.component';
import { TestSessionComponent } from './playtest/test-session.component';
import { DocsComponent } from './docs/docs.component';
import { ApiSandboxComponent } from './api-sandbox/api-sandbox.component';
import { CompanyListComponent } from './companies/company-list.component';
import { CompanyEditComponent } from './companies/company-edit.component';
import { CompanyEmployeesComponent } from './companies/company-employees.component';
import { CompanyService } from './companies/company.service';
import { HelpComponent } from './help/help.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AppCommonModule,
    UtilsModule,
    GameHubAdminRoutingModule,
    ModalModule.forRoot(),
    TableModule,
    PaginatorModule,
  ],
  declarations: [
    DashboardComponent,
    GameListComponent,
    GameDetailComponent,
    ReviewQueueComponent,
    ReviewDetailComponent,
    CategoryListComponent,
    CategoryEditComponent,
    TagListComponent,
    TagEditComponent,
    FeatureFlagsComponent,
    AuditLogComponent,
    UserListComponent,
    BuildListComponent,
    BuildFilesComponent,
    ReportListComponent,
    InspectorComponent,
    InspectorSessionComponent,
    PlaytestRecordingListComponent,
    TestSessionComponent,
    DocsComponent,
    ApiSandboxComponent,
    CompanyListComponent,
    CompanyEditComponent,
    CompanyEmployeesComponent,
    HelpComponent,
  ],
  providers: [CompanyService],
})
export class GameHubAdminModule {}
