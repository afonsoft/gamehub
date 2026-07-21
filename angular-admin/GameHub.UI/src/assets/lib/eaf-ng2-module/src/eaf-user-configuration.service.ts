import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

declare let jQuery: any;
declare let eaf: any;

@Injectable({
  providedIn: 'root',
})
export class EafUserConfigurationService {
  constructor(private readonly _http: HttpClient) {}

  initialize(): void {
    this._http.get('/AbpUserConfiguration/GetAll').subscribe(result => {
      jQuery.extend(true, eaf, JSON.parse(JSON.stringify(result)));
    });
  }
}
