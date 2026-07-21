import { Component, Injector, OnInit } from '@angular/core';

import { AppComponentBase } from '@shared/common/app-component-base';
import { AppNavigationService } from './app-navigation.service';
import { AppMenu } from './app-menu';

@Component({
  standalone: false,
  templateUrl: './adm-bar.component.html',
  selector: '[adm-bar]',
})
export class AdmBarComponent extends AppComponentBase implements OnInit {
  menu: AppMenu = null;
  showMenu = false;

  constructor(
    injector: Injector,
    private readonly _appNavigationService: AppNavigationService,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.menu = this._appNavigationService.getAdminMenu();
    this.showMenu = this.menu.items.some(element => this.showMenuItem(element));
  }

  showMenuItem(menuItem): boolean {
    return this._appNavigationService.showMenuItem(menuItem);
  }
}
