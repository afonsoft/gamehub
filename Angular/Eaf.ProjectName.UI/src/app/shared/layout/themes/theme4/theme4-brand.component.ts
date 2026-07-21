import { Injector, Component, ViewEncapsulation, Inject } from '@angular/core';

import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';

import { DOCUMENT } from '@angular/common';

@Component({
  standalone: false,
  templateUrl: './theme4-brand.component.html',
  selector: 'theme4-brand',
  encapsulation: ViewEncapsulation.None,
})
export class Theme4BrandComponent extends AppComponentBase {
  remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;
  defaultLogo = AppConsts.appBaseUrl + '/assets/common/images/eaf/eaf-' + this.currentTheme.baseSettings.header.headerSkin + '.png';

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
