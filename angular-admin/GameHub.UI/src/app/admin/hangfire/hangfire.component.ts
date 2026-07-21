import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { Location } from '@angular/common';

@Component({
  standalone: false,
  selector: 'hangfire',
  templateUrl: './hangfire.component.html',
})
export class HangfireComponent extends AppComponentBase implements OnInit {
  ngOnInit(): void {
    window.open(AppConsts.remoteServiceBaseUrl + '/hangfire/?AuthToken=' + eaf.auth.getToken());
    this._location.back();
  }

  constructor(
    private readonly _location: Location,
    injector: Injector,
  ) {
    super(injector);
  }
}
