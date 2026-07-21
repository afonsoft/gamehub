import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import {
  Injector,
  ElementRef,
  Component,
  OnInit,
  AfterViewInit,
  ViewEncapsulation,
  Inject,
  ChangeDetectionStrategy,
  DestroyRef,
  inject,
} from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppMenu } from './app-menu';
import { AppNavigationService } from './app-navigation.service';
import { DOCUMENT } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LayoutRefService } from '@metronic/app/core/services/layout-ref.service';

@Component({
  standalone: false,
  templateUrl: './side-bar-menu.component.html',
  selector: 'side-bar-menu',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SideBarMenuComponent extends AppComponentBase implements OnInit, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  menu: AppMenu = null;

  currentRouteUrl = '';
  insideTm: any;
  outsideTm: any;

  constructor(
    injector: Injector,
    private readonly el: ElementRef,
    private readonly router: Router,
    private readonly layoutRefService: LayoutRefService,
    public permission: PermissionCheckerService,
    private readonly _appNavigationService: AppNavigationService,
    @Inject(DOCUMENT) private readonly document: Document,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.menu = this._appNavigationService.getMenu();
    this.currentRouteUrl = this.router.url.split(/[?#]/)[0];

    this.router.events.pipe(filter(event => event instanceof NavigationEnd), takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      this.currentRouteUrl = this.router.url.split(/[?#]/)[0];
      this.ui.removeSelectItemClass(document);
      eaf.event.trigger('app.router.navigationEnd');
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.layoutRefService.addElement('asideLeft', this.el.nativeElement);
    });
  }

  showMenuItem(menuItem): boolean {
    return this._appNavigationService.showMenuItem(menuItem);
  }

  isMenuItemIsActive(item): boolean {
    if (item.items.length) {
      return this.isMenuRootItemIsActive(item);
    }

    if (!item.route) {
      return false;
    }

    // dashboard
    if (item.route !== '/' && this.currentRouteUrl.startsWith(item.route)) {
      return true;
    }

    return this.currentRouteUrl.replace(/\/$/, '') === item.route.replace(/\/$/, '');
  }

  isMenuRootItemIsActive(item): boolean {
    let result = false;

    for (const subItem of item.items) {
      result = this.isMenuItemIsActive(subItem);
      if (result) {
        return true;
      }
    }

    return false;
  }

  /**
   * Use for fixed left aside menu, to show menu on mouseenter event.
   * @param e Event
   */
  mouseEnter(e: Event) {
    if (!this.currentTheme.baseSettings.menu.allowAsideMinimizing) {
      return;
    }

    // check if the left aside menu is fixed
    if (this.document.body.classList.contains('m-aside-left--fixed')) {
      if (this.outsideTm) {
        clearTimeout(this.outsideTm);
        this.outsideTm = null;
      }

      this.insideTm = setTimeout(() => {
        // if the left aside menu is minimized
        if (this.document.body.classList.contains('m-aside-left--minimize') && mUtil.isInResponsiveRange('desktop')) {
          // show the left aside menu
          this.document.body.classList.remove('m-aside-left--minimize');
          this.document.body.classList.add('m-aside-left--minimize-hover');
        }
      }, 300);
    }
  }

  /**
   * Use for fixed left aside menu, to show menu on mouseenter event.
   * @param e Event
   */
  mouseLeave(e: Event) {
    if (!this.currentTheme.baseSettings.menu.allowAsideMinimizing) {
      return;
    }

    if (this.document.body.classList.contains('m-aside-left--fixed')) {
      if (this.insideTm) {
        clearTimeout(this.insideTm);
        this.insideTm = null;
      }

      this.outsideTm = setTimeout(() => {
        // if the left aside menu is expand
        if (this.document.body.classList.contains('m-aside-left--minimize-hover') && mUtil.isInResponsiveRange('desktop')) {
          // hide back the left aside menu
          this.document.body.classList.remove('m-aside-left--minimize-hover');
          this.document.body.classList.add('m-aside-left--minimize');
        }
      }, 500);
    }
  }
}
