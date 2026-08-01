import { Component, Injector, OnInit, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DashboardServiceProxy, IDashboardOutput, IDashboardTileDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './dashboard.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class DashboardComponent extends AppComponentBase implements OnInit {
  loading = false;
  dashboard: IDashboardOutput;

  constructor(
    injector: Injector,
    private readonly _dashboardService: DashboardServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    const request = this.appSession.tenantId
      ? this._dashboardService.getTenantDashboard()
      : this._dashboardService.getHostDashboard();

    request.pipe(finalize(() => (this.loading = false))).subscribe(result => {
      this.dashboard = result;
    });
  }

  get tiles(): IDashboardTileDto[] {
    return this.dashboard?.tiles ?? [];
  }
}
