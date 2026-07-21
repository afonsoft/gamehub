///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EafMultiTenancyService {
  get isEnabled(): boolean {
    return eaf.multiTenancy.isEnabled;
  }
}
