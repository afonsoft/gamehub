import { Component, Injector, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';



declare let d3, Datamap: any;

@Component({
  standalone: false,
  templateUrl: './dashboard.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class DashboardComponent extends AppComponentBase {
  constructor(injector: Injector) {
    super(injector);
  }
}
