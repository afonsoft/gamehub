import './polyfills';
import 'moment-timezone';
import 'moment/min/locales.min';

import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { environment } from './environments/environment';
import { RootModule } from './root.module';

if (environment.production) {
  enableProdMode();
}

const bootstrap = () => {
  return platformBrowserDynamic().bootstrapModule(RootModule);
};

bootstrap(); //Regular bootstrap
