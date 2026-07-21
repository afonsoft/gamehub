import { BehaviorSubject } from 'rxjs';
import { Injectable } from '@angular/core';
import { filter } from 'rxjs/operators';
import { Router, NavigationEnd } from '@angular/router';

@Injectable()
export class LayoutConfigService {
  public onLayoutConfigUpdated$: BehaviorSubject<any>;

  constructor(private readonly router: Router) {
    // register on config changed event and set default config
    this.onLayoutConfigUpdated$ = new BehaviorSubject(null);

    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      this.onLayoutConfigUpdated$.next(null);
    });
  }
}
