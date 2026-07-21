import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AirplanesComponent } from './airplanes/airplanes.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: '',
        children: [
          { path: 'airplanes', component: AirplanesComponent, data: { permission: 'Pages.Airplanes' } },
          { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Dashboard' } },
        ],
      },
    ]),
  ],
  exports: [RouterModule],
})
export class MainRoutingModule {}
