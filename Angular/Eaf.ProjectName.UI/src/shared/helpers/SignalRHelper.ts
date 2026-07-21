import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';

export class SignalRHelper {
  private static _storageService: StorageService;
  static init(storageService: StorageService) {
    this._storageService = storageService;
  }
  static initSignalR(callback: () => void): void {
    const encryptedAuthToken = this._storageService.getCookieValue(AppConsts.authorization.encrptedAuthTokenName);

    eaf.signalr = {
      autoConnect: false, // _zone.runOutsideAngular in ChatSignalrService
      connect: undefined,
      hubs: undefined,
      qs: encryptedAuthToken ? AppConsts.authorization.encrptedAuthTokenName + '=' + encodeURIComponent(encryptedAuthToken) : '',
      remoteServiceBaseUrl: AppConsts.remoteServiceBaseUrl,
      startConnection: undefined,
      url: '/signalr',
      withUrlOptions: {},
    };

    const script = document.createElement('script');
    script.onload = () => {
      callback();
    };

    script.src = AppConsts.appBaseUrl + '/assets/eaf/eaf.signalr-client.js';
    document.head.appendChild(script);
  }
}
