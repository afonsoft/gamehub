import { HashLocationStrategy } from '@angular/common';
import { Injectable } from '@angular/core';

import { environment } from '../../environments/environment';

@Injectable()
export class CustomLocationStrategy extends HashLocationStrategy {
  prepareExternalUrl(internal: string): string {
    const url = this.getBaseHref() + '/#' + internal;
    return environment ? url : '/#' + internal;
  }
}
