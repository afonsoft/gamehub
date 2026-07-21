import { Component, Input, OnInit } from '@angular/core';
import { ChatMessageDto, ChatServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppConsts } from 'shared/AppConsts';

import { StorageService } from '@eaf/utils/storage.service';

@Component({
  standalone: false,
  selector: 'chat-message',
  templateUrl: './chat-message.component.html',
})
export class ChatMessageComponent implements OnInit {
  @Input()
  message: ChatMessageDto;

  chatMessage: string;
  chatMessageType: string;
  fileName: string;
  fileContentType: string;

  constructor(
    private readonly _chatService: ChatServiceProxy,
    private readonly _storageService: StorageService,
  ) {}

  ngOnInit(): void {
    this.setChatMessageType();
  }

  private setChatMessageType(): void {

    const encryptedAuthToken = this._storageService.getCookieValue(AppConsts.authorization.encrptedAuthTokenName);

    if (this.message.message.startsWith('[image]')) {
      this.chatMessageType = 'image';

      const image = JSON.parse(this.message.message.substring('[image]'.length));
      this.chatMessage =
        AppConsts.remoteServiceBaseUrl +
        '/api/services/app/Chat/GetUploadedObject?fileId=' +
        image.id +
        '&fileName=' +
        image.name +
        '&contentType=' +
        image.contentType +
        '&' +
        AppConsts.authorization.encrptedAuthTokenName +
        '=' +
        encodeURIComponent(encryptedAuthToken);
    } else if (this.message.message.startsWith('[file]')) {
      this.chatMessageType = 'file';

      const file = JSON.parse(this.message.message.substring('[file]'.length));
      this.chatMessage =
        AppConsts.remoteServiceBaseUrl +
        '/api/services/app/Chat/GetUploadedObject?fileId=' +
        file.id +
        '&fileName=' +
        file.name +
        '&contentType=' +
        file.contentType +
        '&' +
        AppConsts.authorization.encrptedAuthTokenName +
        '=' +
        encodeURIComponent(encryptedAuthToken);

      this.fileName = file.name;
    } else if (this.message.message.startsWith('[link]')) {
      this.chatMessageType = 'link';
      const linkMessage = JSON.parse(this.message.message.substring('[link]'.length));
      this.chatMessage = linkMessage.message ?? '';
    } else {
      this.chatMessageType = 'text';
      this.chatMessage = this.message.message;
    }
  }
}
