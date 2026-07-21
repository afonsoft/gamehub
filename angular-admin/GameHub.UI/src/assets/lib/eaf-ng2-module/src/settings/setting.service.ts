///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SettingService {
  get(name: string): string {
    return eaf.setting.get(name);
  }

  getBoolean(name: string): boolean {
    return eaf.setting.getBoolean(name);
  }

  getInt(name: string): number {
    return eaf.setting.getInt(name);
  }
}
