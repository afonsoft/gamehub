import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { Component, EventEmitter, Injector, Input, Output } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { ChatFriendDto } from './ChatFriendDto';

@Component({
  standalone: false,
  templateUrl: './chat-friend-list-item.component.html',
  selector: 'chat-friend-list-item',
})
export class ChatFriendListItemComponent {
  remoteServiceUrl: string = AppConsts.remoteServiceBaseUrl;
  appPath: string = AppConsts.appBaseUrl;

  @Input() friend: ChatFriendDto;
  @Output() selectChatFriend: EventEmitter<string> = new EventEmitter<string>();

  multiTenancy: EafMultiTenancyService;

  constructor(injector: Injector) {
    this.multiTenancy = injector.get(EafMultiTenancyService);
  }

  getShownUserName(tenanycName: string, userName: string): string {
    if (!this.multiTenancy.isEnabled) {
      return userName;
    }
    return (tenanycName || '.') + '\\' + userName;
  }

  getTitle() {
    return this.friend.name + ' ' + this.friend.surname + ' <' + this.friend.email + '>';
  }

  getRemoteImageUrl(profilePictureId: string, userId: number, tenantId?: number): string {
    if (userId > 0)
      return (
        this.remoteServiceUrl +
        '/api/services/app/Profile/GetFriendProfilePictureById?id=' +
        profilePictureId +
        '&userId=' +
        userId +
        '&tenantId=' +
        tenantId
      );
    return this.appPath + '/assets/common/images/nopicture.png';
  }
}
