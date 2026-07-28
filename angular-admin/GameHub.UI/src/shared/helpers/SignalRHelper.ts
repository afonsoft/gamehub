import * as signalR from '@microsoft/signalr';
import { TokenService } from '@eaf/auth/token.service';
import { AppConsts } from '@shared/AppConsts';

export class SignalRHelper {
  private static _tokenService: TokenService;

  static init(tokenService: TokenService): void {
    this._tokenService = tokenService;
  }

  static buildConnection(hubUrl: string = '/signalr'): signalR.HubConnection {
    const base = (AppConsts.remoteServiceBaseUrl || '').replace(/\/$/, '');
    const fullUrl = base + hubUrl;

    return new signalR.HubConnectionBuilder()
      .withUrl(fullUrl, {
        accessTokenFactory: () => this._tokenService?.getToken() ?? '',
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.ServerSentEvents |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }
}
