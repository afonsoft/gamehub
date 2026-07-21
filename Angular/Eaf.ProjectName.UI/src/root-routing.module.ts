import { NgModule } from '@angular/core';
import { NavigationEnd, Router, RouterModule, Routes } from '@angular/router';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';

const routes: Routes = [
  { path: '', redirectTo: '/app/main/dashboard', pathMatch: 'full' },
  {
    path: 'account',
    loadChildren: () => import('account/account.module').then(m => m.AccountModule), //Lazy load account module
    data: { preload: true },
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { enableTracing: false })],
  exports: [RouterModule],
  providers: [],
})
export class RootRoutingModule {
  constructor(
    private readonly router: Router,
    private readonly _uiCustomizationService: AppUiCustomizationService,
  ) {
    router.events.subscribe((event: NavigationEnd) => {
      setTimeout(() => {
        this.toggleBodyCssClass(event.url);
      }, 0);
    });
  }

  toggleBodyCssClass(url: string): void {
    if (url) {
      if (url === '/') {
        if (eaf.session.userId > 0) {
          this.setAppModuleBodyClassInternal();
        } else {
          this.setAccountModuleBodyClassInternal();
        }
      }

      if (url.includes('/account/')) {
        this.setAccountModuleBodyClassInternal();
      } else {
        this.setAppModuleBodyClassInternal();
      }
    }
  }

  setAppModuleBodyClassInternal(): void {
    const currentBodyClass = document.body.className;
    let classesToRemember = '';

    if (currentBodyClass.includes('m-brand--minimize')) {
      classesToRemember += ' m-brand--minimize ';
    }

    if (currentBodyClass.includes('m-aside-left--minimize')) {
      classesToRemember += ' m-aside-left--minimize';
    }

    if (currentBodyClass.includes('m-brand--hide')) {
      classesToRemember += ' m-brand--hide';
    }

    if (currentBodyClass.includes('m-aside-left--hide')) {
      classesToRemember += ' m-aside-left--hide';
    }

    if (currentBodyClass.includes('swal2-toast-shown')) {
      classesToRemember += ' swal2-toast-shown';
    }

    document.body.className = this._uiCustomizationService.getAppModuleBodyClass() + ' ' + classesToRemember;
  }

  setAccountModuleBodyClassInternal(): void {
    const currentBodyClass = document.body.className;
    let classesToRemember = '';

    if (currentBodyClass.includes('swal2-toast-shown')) {
      classesToRemember += ' swal2-toast-shown';
    }

    document.body.className = this._uiCustomizationService.getAccountModuleBodyClass() + ' ' + classesToRemember;
  }

  getSetting(key: string): string {
    return eaf.setting.get(key);
  }
}
