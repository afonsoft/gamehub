import { NgModule } from '@angular/core';
import { NavigationEnd, RouteConfigLoadEnd, RouteConfigLoadStart, Router, RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { AppRouteGuard } from './shared/common/auth/auth-route-guard';
import { NotificationsComponent } from './shared/layout/notifications/notifications.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: 'app',
        component: AppComponent,
        canActivate: [AppRouteGuard],
        canActivateChild: [AppRouteGuard],
        children: [
          {
            path: '',
            children: [
              { path: 'notifications', component: NotificationsComponent },
              { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
            ],
          },
          {
            path: 'main',
            loadChildren: () => import('app/main/main.module').then(m => m.MainModule), //Lazy load main module
            data: { preload: true },
          },
          {
            path: 'admin',
            loadChildren: () => import('app/admin/admin.module').then(m => m.AdminModule), //Lazy load admin module
            canLoad: [AppRouteGuard],
          },
          {
            path: '**',
            redirectTo: 'notifications',
            pathMatch: 'prefix',
          },
        ],
      },
    ]),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {
  constructor(private readonly router: Router) {
    router.events.subscribe(event => {
      if (event instanceof RouteConfigLoadStart) {
        eaf.ui.setBusy();
      }

      if (event instanceof RouteConfigLoadEnd) {
        eaf.ui.clearBusy();
      }

      if (event instanceof NavigationEnd) {
        document.querySelector(String.raw`meta[property=og\:url]`).setAttribute('content', window.location.href);
      }
    });
  }
}
