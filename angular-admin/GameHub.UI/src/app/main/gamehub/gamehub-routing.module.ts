import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppRouteGuard } from '@app/shared/common/auth/auth-route-guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { GameListComponent } from './games/game-list.component';
import { GameDetailComponent } from './games/game-detail.component';
import { ReviewQueueComponent } from './moderation/review-queue.component';
import { ReviewDetailComponent } from './moderation/review-detail.component';
import { CategoryListComponent } from './categories/category-list.component';
import { CategoryEditComponent } from './categories/category-edit.component';
import { TagListComponent } from './tags/tag-list.component';
import { TagEditComponent } from './tags/tag-edit.component';
import { FeatureFlagsComponent } from './dashboard/feature-flags.component';
import { AuditLogComponent } from './dashboard/audit-log.component';
import { UserListComponent } from './users/user-list.component';
import { BuildListComponent } from './uploads/build-list.component';
import { BuildFilesComponent } from './uploads/build-files.component';
import { ReportListComponent } from './reports/report-list.component';
import { InspectorComponent } from './inspector/inspector.component';
import { InspectorSessionComponent } from './inspector/inspector-session.component';
import { PlaytestRecordingListComponent } from './playtest/playtest-recording-list.component';
import { gameDetailResolver } from './resolvers/game-detail.resolver';
import { moderationDetailResolver } from './resolvers/moderation-detail.resolver';
import { categoryEditResolver } from './resolvers/category-edit.resolver';
import { tagEditResolver } from './resolvers/tag-edit.resolver';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.GameHubDashboard.View' } },
  { path: 'games', component: GameListComponent, data: { permission: 'Pages.Games.View' } },
  { path: 'games/:id', component: GameDetailComponent, resolve: { game: gameDetailResolver }, data: { permission: 'Pages.Games.View' } },
  { path: 'games/:id/edit', component: GameDetailComponent, resolve: { game: gameDetailResolver }, data: { permission: 'Pages.Games.Edit' } },
  { path: 'moderation', component: ReviewQueueComponent, data: { permission: 'Pages.Moderation.View' } },
  { path: 'moderation/:id', component: ReviewDetailComponent, resolve: { review: moderationDetailResolver }, data: { permission: 'Pages.Moderation.View' } },
  { path: 'categories', component: CategoryListComponent, data: { permission: 'Pages.Categories.Manage' } },
  { path: 'categories/create', component: CategoryEditComponent, resolve: { category: categoryEditResolver }, data: { permission: 'Pages.Categories.Manage' } },
  { path: 'categories/:id/edit', component: CategoryEditComponent, resolve: { category: categoryEditResolver }, data: { permission: 'Pages.Categories.Manage' } },
  { path: 'tags', component: TagListComponent, data: { permission: 'Pages.Tags.Manage' } },
  { path: 'tags/create', component: TagEditComponent, resolve: { tag: tagEditResolver }, data: { permission: 'Pages.Tags.Manage' } },
  { path: 'tags/:id/edit', component: TagEditComponent, resolve: { tag: tagEditResolver }, data: { permission: 'Pages.Tags.Manage' } },
  { path: 'dashboard/flags', component: FeatureFlagsComponent, data: { permission: 'Pages.GameHubDashboard.FeatureFlags' } },
  { path: 'dashboard/audit', component: AuditLogComponent, data: { permission: 'Pages.GameHubDashboard.AuditLog' } },
  { path: 'users', component: UserListComponent, data: { permission: 'Pages.Users.Manage' } },
  { path: 'uploads', component: BuildListComponent, data: { permission: 'Pages.Builds.View' } },
  { path: 'uploads/:id', component: BuildFilesComponent, data: { permission: 'Pages.Builds.View' } },
  { path: 'inspector', component: InspectorComponent, data: { permission: 'Pages.Builds.View' } },
  { path: 'inspector/session/:id', component: InspectorSessionComponent, data: { permission: 'Pages.Builds.View' } },
  { path: 'playtests', component: PlaytestRecordingListComponent, data: { permission: 'Pages.Moderation.View' } },
  { path: 'reports', component: ReportListComponent, data: { permission: 'Pages.Reports.Manage' } },
  { path: '**', redirectTo: 'dashboard', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class GameHubAdminRoutingModule {}
