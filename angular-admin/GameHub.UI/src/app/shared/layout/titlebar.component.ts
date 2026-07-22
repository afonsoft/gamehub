import { Component, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';



@Component({
  standalone: false,
  selector: 'titlebar',
  template: `<h3 class="m_header_tilebar header-{{ currentTheme.baseSettings.header.headerSkin }}">GameHub</h3>`,
})
export class TitleBarComponent extends AppComponentBase {
  constructor(injector: Injector) {
    super(injector);
  }
}
