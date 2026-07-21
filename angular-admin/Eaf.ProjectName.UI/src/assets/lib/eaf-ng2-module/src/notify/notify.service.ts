///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NotifyService {
  info(message: string, title?: string, options?: any): void {
    eaf.notify.info(message, title, options);
  }

  success(message: string, title?: string, options?: any): void {
    eaf.notify.success(message, title, options);
  }

  warn(message: string, title?: string, options?: any): void {
    eaf.notify.warn(message, title, options);
  }

  error(message: string, title?: string, options?: any): void {
    eaf.notify.error(message, title, options);
  }
}
