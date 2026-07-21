import { Injector, ElementRef, Component, AfterViewInit, ViewChild, HostBinding } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';
import { MenuAsideOffcanvasDirective } from '@metronic/app/core/directives/menu-aside-offcanvas.directive';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  standalone: false,
  templateUrl: './default-layout.component.html',
  selector: 'default-layout',
  animations: [appModuleAnimation()],
})
export class DefaultLayoutComponent extends AppComponentBase implements AfterViewInit {
  @ViewChild('mHeader', { static: true }) mHeader: ElementRef;
  @ViewChild('mAsideLeft', { static: true }) mAsideLeft: ElementRef;
  @HostBinding('attr.mMenuAsideOffcanvas') mMenuAsideOffcanvas: MenuAsideOffcanvasDirective;

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

    this.registerToEvents();
  }

  registerToEvents() {
    eaf.event.on('app.router.navigationEnd', () => {
      this.mMenuAsideOffcanvas.menuOffcanvas.hide();
    });
  }
}
