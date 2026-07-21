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
  ],
  providers: [],
})
export class GameHubAdminModule {}
