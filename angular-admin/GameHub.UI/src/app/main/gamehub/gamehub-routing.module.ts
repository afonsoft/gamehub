import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppRouteGuard } from '@app/shared/common/auth/auth-route-guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { GameListComponent } from './games/game-list.component';
import { ReviewQueueComponent } from './moderation/review-queue.component';
import { CategoryListComponent } from './categories/category-list.component';
import { TagListComponent } from './tags/tag-list.component';
import { FeatureFlagsComponent } from './dashboard/feature-flags.component';
import { AuditLogComponent } from './dashboard/audit-log.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.GameHubDashboard.View' } },
  { path: 'games', component: GameListComponent, data: { permission: 'Pages.Games.View' } },
  { path: 'moderation', component: ReviewQueueComponent, data: { permission: 'Pages.Moderation.View' } },
  { path: 'categories', component: CategoryListComponent, data: { permission: 'Pages.Categories.Manage' } },
  { path: 'tags', component: TagListComponent, data: { permission: 'Pages.Tags.Manage' } },
  { path: 'dashboard/flags', component: FeatureFlagsComponent, data: { permission: 'Pages.GameHubDashboard.FeatureFlags' } },
  { path: 'dashboard/audit', component: AuditLogComponent, data: { permission: 'Pages.GameHubDashboard.AuditLog' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class GameHubAdminRoutingModule {}
