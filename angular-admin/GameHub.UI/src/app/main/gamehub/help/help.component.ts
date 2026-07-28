import { Component } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';

type Lang = 'pt' | 'en';

@Component({
  standalone: false,
  selector: 'gamehub-help',
  templateUrl: './help.component.html',
  animations: [appModuleAnimation()],
})
export class HelpComponent {
  lang: Lang = 'pt';

  setLang(lang: Lang): void {
    this.lang = lang;
  }
}
