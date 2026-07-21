import { Injector, ElementRef, Component, AfterViewInit, ViewChild, HostBinding } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';

import { AppConsts } from '@shared/AppConsts';
import { MenuAsideOffcanvasDirective } from '@metronic/app/core/directives/menu-aside-offcanvas.directive';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  standalone: false,
  templateUrl: './theme3-layout.component.html',
  selector: 'theme3-layout',
  animations: [appModuleAnimation()],
})
export class Theme3LayoutComponent extends AppComponentBase implements AfterViewInit {
  @ViewChild('mHeader', { static: true }) mHeader: ElementRef;
  @ViewChild('mAsideLeft', { static: true }) mAsideLeft: ElementRef;
  @HostBinding('attr.mMenuAsideOffcanvas') mMenuAsideOffcanvas: MenuAsideOffcanvasDirective;

  remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;
  defaultLogo = AppConsts.appBaseUrl + '/assets/common/images/eaf/eaf-' + this.currentTheme.baseSettings.menu.asideSkin + '.png';

  constructor(
    injector: Injector,
    private readonly layoutRefService: LayoutRefService,
  ) {
    super(injector);
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.mMenuAsideOffcanvas = new MenuAsideOffcanvasDirective(this.mAsideLeft);
      this.mMenuAsideOffcanvas.ngAfterViewInit();
    });

    this.layoutRefService.addElement('header', this.mHeader.nativeElement);
  }
}
