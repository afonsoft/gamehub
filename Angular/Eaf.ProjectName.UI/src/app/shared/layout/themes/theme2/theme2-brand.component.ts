import { DOCUMENT } from '@angular/common';
import { Component, Inject, Injector, ViewEncapsulation } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  standalone: false,
  templateUrl: './theme2-brand.component.html',
  selector: 'theme2-brand',
  encapsulation: ViewEncapsulation.None,
})
export class Theme2BrandComponent extends AppComponentBase {
  remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;
  defaultLogo = AppConsts.appBaseUrl + '/assets/common/images/eaf/eaf-on-' + this.currentTheme.baseSettings.header.headerSkin + '.png';

  constructor(
    injector: Injector,
    @Inject(DOCUMENT) private readonly document: Document,
  ) {
    super(injector);
  }

  clickTopbarToggle(): void {
    this.document.body.classList.toggle('m-topbar--on');
  }
}
