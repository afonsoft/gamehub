import { Injectable } from '@angular/core';
import { UiCustomizationSettingsDto } from '@shared/service-proxies/service-proxies';

@Injectable()
export class AppUiCustomizationService {
  private _theme: UiCustomizationSettingsDto;

  init(theme: UiCustomizationSettingsDto): void {
    this._theme = theme;
  }

  getContainerClass() {
    if (!this._theme?.baseSettings?.layout) {
      return 'm-container--responsive m-container--xxl';
    }

    if (this._theme.baseSettings.layout.layoutType === 'fluid') {
      return 'm-container--fluid';
    }

    return 'm-container--responsive m-container--xxl';
  }

  private isLeftMenuUsed(): boolean {
    return this._theme?.baseSettings?.menu?.position === 'left';
  }

  getAppModuleBodyClass(): string {
    if (!this._theme?.baseSettings) {
      return 'm-page--fluid';
    }

    const topMenuUsed = this._theme.baseSettings.menu.position === 'top';

    return (
      'm-page--' +
      this._theme.baseSettings.layout.layoutType +
      ' ' +
      (this._theme.baseSettings.layout.contentSkin !== '' ? 'm-content--skin-' + this._theme.baseSettings.layout.contentSkin : '') +
      ' ' +
      'm-header--' +
      (this._theme.baseSettings.header.desktopFixedHeader ? 'fixed' : 'static') +
      ' ' +
      (this._theme.baseSettings.header.mobileFixedHeader ? 'm-header--fixed-mobile' : '') +
      ' ' +
      (this._theme.baseSettings.menu.fixedAside && !topMenuUsed ? 'm-aside-left--fixed' : '') +
      ' ' +
      (this._theme.baseSettings.menu.defaultMinimizedAside ? 'm-aside-left--minimize m-brand--minimize' : '') +
      ' ' +
      (this._theme.baseSettings.menu.defaultHiddenAside || this._theme.baseSettings.menu.position === 'top' ? 'm-aside-left--hide' : '') +
      ' ' +
      (this.isLeftMenuUsed() ? 'm-aside-left--enabled' : '') +
      ' ' +
      'm-aside-left--skin-' +
      this._theme.baseSettings.menu.asideSkin +
      ' ' +
      'm-aside-left--offcanvas'
    );
  }

  getAccountModuleBodyClass() {
    if (!this._theme?.baseSettings?.menu) {
      return 'm--skin- m-header--fixed m-header--fixed-mobile m-aside-left--offcanvas m-footer--push m-aside--offcanvas-default';
    }

    return (
      'm--skin- m-header--fixed m-header--fixed-mobile ' +
      (this.isLeftMenuUsed() ? ' m-aside-left--enabled' : '') +
      ' m-aside-left--skin-' +
      this._theme.baseSettings.menu.asideSkin +
      ' m-aside-left--offcanvas m-footer--push m-aside--offcanvas-default'
    );
  }

  getSideBarMenuClass(): string {
    if (!this._theme?.baseSettings?.menu) {
      return 'm-aside-menu m-aside-menu--skin-light';
    }

    let menuCssClass = 'm-aside-menu m-aside-menu--skin-' + this._theme.baseSettings.menu.asideSkin;

    menuCssClass += ' m-aside-menu--submenu-skin-';
    menuCssClass += this._theme.baseSettings.menu.asideSkin;

    if (!this._theme.baseSettings.menu.fixedAside && this._theme.baseSettings.menu.submenuToggle === 'Dropdown') {
      menuCssClass += ' m-aside-menu--dropdown';
    }

    return menuCssClass;
  }

  getMenuListClass(): string {
    return 'm-menu__nav--dropdown-submenu-arrow';
  }

  getTopBarMenuContainerClass(): string {
    if (!this._theme?.baseSettings?.menu || !this._theme?.baseSettings?.layout) {
      return 'm-header__bottom m-header-menu--skin-light m-container m-container--full-height m-container--responsive';
    }

    const menuCssClass =
      'm-header__bottom m-header-menu--skin-' +
      this._theme.baseSettings.menu.asideSkin +
      ' m-container m-container--full-height m-container--responsive';
    if (this._theme.baseSettings.layout.layoutType === 'boxed') {
      return menuCssClass + ' m-container--xxl';
    }

    return menuCssClass;
  }

  getIsMenuScrollable(): boolean {
    if (!this._theme?.baseSettings?.menu) {
      return false;
    }

    return this._theme.allowMenuScroll && this._theme.baseSettings.menu.fixedAside;
  }

  getSideBarMenuItemClass(item, isMenuActive) {
    let menuCssClass = 'm-menu__item';

    if (item.items.length) {
      menuCssClass += ' m-menu__item--submenu';
    }

    if (isMenuActive) {
      menuCssClass += ' m-menu__item--open m-menu__item--expanded m-menu__item--active';
    }

    return menuCssClass;
  }

  removeSelectItemClass(document: Document): void {
    const itens = ['m-menu__item--active', 'm-menu__item--open', 'm-menu__item--expanded', 'm-menu__item--hover'];
    itens.forEach(className => {
      const total = document.getElementsByClassName(className).length;
      for (let index = 0; index < total; index++) {
        try {
          document.getElementsByClassName(className)[index].classList.remove(className);
        } catch {}
      }
    });
  }
}
