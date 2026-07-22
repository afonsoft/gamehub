import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppRouteGuard } from '@app/shared/common/auth/auth-route-guard';
import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: '',
        children: [
          { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Dashboard' } },
          {
            path: 'gamehub',
            loadChildren: () => import('./gamehub/gamehub.module').then(m => m.GameHubAdminModule),
            canLoad: [AppRouteGuard],
            data: { permission: 'Pages.Games' },
          },
        ],
      },
    ]),
  ],
  exports: [RouterModule],
})
export class MainRoutingModule {}
