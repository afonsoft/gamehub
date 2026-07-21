import { Component, Injector, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AccountServiceProxy, ActivateEmailInput, ResolveTenantIdInput } from '@shared/service-proxies/service-proxies';

@Component({
  standalone: false,
  template: `<p>{{ waitMessage }}</p>`,
})
export class ConfirmEmailComponent extends AppComponentBase implements OnInit {
  waitMessage: string;

  model: ActivateEmailInput = new ActivateEmailInput();

  constructor(
    injector: Injector,
    private readonly _accountService: AccountServiceProxy,
    private readonly _router: Router,
    private readonly _activatedRoute: ActivatedRoute,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.waitMessage = this.l('PleaseWaitToConfirmYourEmailMessage');

    this.model.c = this._activatedRoute.snapshot.queryParams['c'];

    this._accountService.resolveTenantId(new ResolveTenantIdInput({ c: this.model.c })).subscribe(tenantId => {
      this.appSession.changeTenantIfNeeded(tenantId);

      this._accountService.activateEmail(this.model).subscribe(() => {
        this.notify.success(this.l('YourEmailIsConfirmedMessage'), '', {
          onClose: () => {
            this._router.navigate(['account/login']);
          },
        });
      });
    });
  }

  parseTenantId(tenantIdAsStr?: string): number {
    return !tenantIdAsStr ? undefined : Number.parseInt(tenantIdAsStr, 10);
  }
}