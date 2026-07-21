import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { ChangeDetectionStrategy, Component, Injector, OnInit, AfterViewInit, ViewEncapsulation, ElementRef, ViewChild, Input, Inject, DestroyRef, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppMenu } from './app-menu';
import { AppNavigationService } from './app-navigation.service';
import * as objectPath from 'object-path';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MenuHorizontalDirective } from '@metronic/app/core/directives/menu-horizontal.directive';
import { MenuHorizontalOffcanvasDirective } from '@metronic/app/core/directives/menu-horizontal-offcanvas.directive';
import { DOCUMENT } from '@angular/common';

@Component({
  standalone: false,
  templateUrl: './top-bar-menu.component.html',
  selector: 'top-bar-menu',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopBarMenuComponent extends AppComponentBase implements OnInit, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  @Input() isTabMenuUsed?: boolean;

  menu: AppMenu = null;
  currentRouteUrl: any = '';
  menuDepth: 0;

  @ViewChild('m_header_menu', { static: true }) el: ElementRef;

  mMenuHorizontal: MenuHorizontalDirective;
  mMenuHorOffcanvas: MenuHorizontalOffcanvasDirective;

  constructor(
    injector: Injector,
    private readonly router: Router,
    public permission: PermissionCheckerService,
    private readonly _appNavigationService: AppNavigationService,
    @Inject(DOCUMENT) private readonly document: Document,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.menu = this._appNavigationService.getMenu();
    this.currentRouteUrl = this.router.url;

    this.router.events.pipe(filter(event => event instanceof NavigationEnd), takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      this.currentRouteUrl = this.router.url;
      this.ui.removeSelectItemClass(document);
      eaf.event.trigger('app.router.navigationEnd');
    });
  }

  ngAfterViewInit(): void {
    this.mMenuHorOffcanvas = new MenuHorizontalOffcanvasDirective(this.el);
    this.mMenuHorOffcanvas.ngAfterViewInit();

    this.mMenuHorizontal = new MenuHorizontalDirective(this.el);
    this.mMenuHorizontal.ngAfterViewInit();

    this.registerToEvents();
  }

  registerToEvents() {
    eaf.event.on('app.router.navigationEnd', () => {
      this.mMenuHorOffcanvas.menuOffcanvas.hide();
    });
  }

  showMenuItem(menuItem): boolean {
    return this._appNavigationService.showMenuItem(menuItem);
  }

  getItemCssClasses(item, parentItem, depth) {
    const isRootLevel = item && !parentItem;

    return [
      'm-menu__item',
      this.getSubmenuClass(item, isRootLevel),
      this.getIconOnlyClass(item),
      this.getActiveClass(item, isRootLevel),
      this.getTabClass(isRootLevel),
      this.getSubmenuTabClass(item, isRootLevel, depth),
    ].join(' ');
  }

  private getSubmenuClass(item, isRootLevel): string {
    return item.items?.length || this.isRootTabMenuItemWithoutChildren(item, isRootLevel) ? 'm-menu__item--submenu' : '';
  }

  private getIconOnlyClass(item): string {
    return objectPath.get(item, 'icon-only') ? 'm-menu__item--icon-only' : '';
  }

  private getActiveClass(item, isRootLevel): string {
    if (!this.isMenuItemIsActive(item)) {
      return '';
    }

    return this.isTabMenuUsed && isRootLevel ? 'm-menu__item--active m-menu__item--hover' : 'm-menu__item--active';
  }

  private getTabClass(isRootLevel): string {
    return this.isTabMenuUsed && isRootLevel ? 'm-menu__item--tabs' : '';
  }

  private getSubmenuTabClass(item, isRootLevel, depth): string {
    if (!item.items?.length) {
      return '';
    }

    if (this.isTabMenuUsed && !isRootLevel) {
      return depth === 1
        ? 'm-menu__item--submenu m-menu__item--rel m-menu__item--submenu-tabs m-menu__item--open-dropdown m-menu__item--hover'
        : 'm-menu__item--submenu m-menu__item--rel';
    }

    if (!this.isTabMenuUsed) {
      return depth >= 1 ? 'm-menu__item--submenu' : 'm-menu__item--rel';
    }

    return '';
  }

  getAnchorItemCssClasses(item, parentItem): string {
    const isRootLevel = item && !parentItem;
    let cssClasses = 'm-menu__link';

    if ((this.isTabMenuUsed && isRootLevel) || item.items.length) {
      cssClasses += ' m-menu__toggle';
    }

    return cssClasses;
  }

  getSubmenuCssClasses(item, parentItem, depth): string {
    let cssClasses = 'm-menu__submenu m-menu__submenu--classic';

    if (this.isTabMenuUsed) {
      if (depth === 0) {
        cssClasses += ' m-menu__submenu--tabs';
      }

      cssClasses += ' m-menu__submenu--' + (depth >= 2 ? 'right' : 'left');
    } else {
      cssClasses += ' m-menu__submenu--' + (depth >= 1 ? 'right' : 'left');
    }

    return cssClasses;
  }

  isRootTabMenuItemWithoutChildren(item: any, isRootLevel: boolean): boolean {
    return this.isTabMenuUsed && isRootLevel && !item.items.length;
  }

  isMenuItemIsActive(item): boolean {
    if (item.items.length) {
      return this.isMenuRootItemIsActive(item);
    }

    if (!item.route) {
      return false;
    }

    return item.route === this.currentRouteUrl;
  }

  isMenuRootItemIsActive(item): boolean {
    if (item.items) {
      for (const subItem of item.items) {
        if (this.isMenuItemIsActive(subItem)) {
          return true;
        }
      }
    }

    return false;
  }

  getItemAttrSubmenuToggle(menuItem, parentItem, depth) {
    const isRootLevel = menuItem && !parentItem;
    if (isRootLevel && this.isTabMenuUsed) {
      return 'tab';
    } else if (depth && depth >= 1) {
      return 'hover';
    } else {
      return 'click';
    }
  }

  getCssClass(): string {
    let menuCssClass = 'm-header-menu m-aside-header-menu-mobile m-aside-header-menu-mobile--offcanvas';
    menuCssClass += ' m-header--skin-' + this.currentTheme.baseSettings.header.headerSkin;
    menuCssClass += ' m-header-menu--skin-' + this.currentTheme.baseSettings.menu.asideSkin;
    menuCssClass += ' m-header-menu--submenu-skin-' + this.currentTheme.baseSettings.menu.asideSkin;
    menuCssClass += ' m-aside-header-menu-mobile--skin-' + this.currentTheme.baseSettings.menu.asideSkin;
    menuCssClass += ' m-aside-header-menu-mobile--submenu-skin-' + this.currentTheme.baseSettings.menu.asideSkin;

    if (this.currentTheme.baseSettings.layout.layoutType === 'boxed') {
      return menuCssClass + ' m-container--xxl';
    }

    return menuCssClass;
  }
}
