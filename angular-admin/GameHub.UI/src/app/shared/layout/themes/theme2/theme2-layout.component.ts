import { Injector, ElementRef, Component, AfterViewInit, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';

import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  standalone: false,
  templateUrl: './theme2-layout.component.html',
  selector: 'theme2-layout',
  animations: [appModuleAnimation()],
})
export class Theme2LayoutComponent extends AppComponentBase implements AfterViewInit {
  @ViewChild('mHeader', { static: true }) mHeader: ElementRef;

  constructor(
    injector: Injector,
    private readonly layoutRefService: LayoutRefService,
  ) {
    super(injector);
  }

  ngAfterViewInit(): void {
    this.layoutRefService.addElement('header', this.mHeader.nativeElement);
    this.initStickyHeader();
  }

  initStickyHeader(): any {
    const headerEl = mUtil.get('m_header');
    const options = {
      offset: {
        desktop: null,
        mobile: null,
      },
      minimize: {
        mobile: null,
        desktop: null,
      },
      classic: {
        mobile: true,
        desktop: true,
      },
    };

    if (mUtil.attr(headerEl, 'm-minimize-mobile') === 'minimize') {
      options.minimize.mobile = {};
      options.minimize.mobile.on = 'm-header--minimize-on';
      options.minimize.mobile.off = 'm-header--minimize-off';
    } else {
      options.minimize.mobile = false;
    }

    if (mUtil.attr(headerEl, 'm-minimize') === 'minimize') {
      options.minimize.desktop = {};
      options.minimize.desktop.on = 'm-header--minimize-on';
      options.minimize.desktop.off = 'm-header--minimize-off';
    } else {
      options.minimize.desktop = false;
    }

    if (mUtil.attr(headerEl, 'm-minimize-offset')) {
      options.offset.desktop = mUtil.attr(headerEl, 'm-minimize-offset');
    }

    if (mUtil.attr(headerEl, 'm-minimize-mobile-offset')) {
      options.offset.mobile = mUtil.attr(headerEl, 'm-minimize-mobile-offset');
    }

    return new (mHeader as any)('m_header', options);
  }
}
