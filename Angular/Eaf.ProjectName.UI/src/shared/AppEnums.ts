import { UserNotificationState } from '@shared/service-proxies/service-proxies';

export class AppTimezoneScope {
  static readonly Application = 1;
  static readonly Tenant = 2;
  static readonly User = 4;
  static readonly All = 7;
}

export class AppUserNotificationState {
  static readonly Unread: number = UserNotificationState.Unread;
  static readonly Read: number = UserNotificationState.Read;
}
