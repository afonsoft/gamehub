import { Injectable, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@microsoft/signalr';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';

@Injectable()
export class ChatSignalrService extends AppComponentBase {
  constructor(
    injector: Injector,
    public _zone: NgZone,
  ) {
    super(injector);
  }

  chatHub: HubConnection;
  isChatConnected = false;

  registerChatEvents(connection: HubConnection): void {
    connection.on('getChatMessage', message => {
      eaf.event.trigger('app.chat.messageReceived', message);
    });

    connection.on('getAllFriends', friends => {
      eaf.event.trigger('eaf.chat.friendListChanged', friends);
    });

    connection.on('getFriendshipRequest', (friendData, isOwnRequest) => {
      eaf.event.trigger('app.chat.friendshipRequestReceived', friendData, isOwnRequest);
    });

    connection.on('getUserConnectNotification', (friend, isConnected) => {
      eaf.event.trigger('app.chat.userConnectionStateChanged', {
        friend: friend,
        isConnected: isConnected,
      });
    });

    connection.on('getUserStateChange', (friend, state) => {
      eaf.event.trigger('app.chat.userStateChanged', {
        friend: friend,
        state: state,
      });
    });

    connection.on('getallUnreadMessagesOfUserRead', friend => {
      eaf.event.trigger('app.chat.allUnreadMessagesOfUserRead', {
        friend: friend,
      });
    });

    connection.on('getReadStateChange', friend => {
      eaf.event.trigger('app.chat.readStateChange', {
        friend: friend,
      });
    });
  }

  sendMessage(messageData, callback): void {
    if (!this.isChatConnected) {
      if (callback) {
        callback();
      }

      eaf.notify.warn(this.l('ChatIsNotConnectedWarning'));
      return;
    }

    this.chatHub
      .invoke('sendMessage', messageData)
      .then(result => {
        if (result) {
          eaf.notify.warn(result);
        }

        if (callback) {
          callback();
        }
      })
      .catch(error => {
        eaf.log.error(error);

        if (callback) {
          callback();
        }
      });
  }

  init(): void {
    this._zone.runOutsideAngular(async () => {
      this.chatHub = SignalRHelper.buildConnection('/signalr-chat');

      this.chatHub.onreconnecting(error => {
        this.isChatConnected = false;
        if (error) {
          eaf.log.debug('Chat reconnecting: ' + error);
        }
      });

      this.chatHub.onreconnected(connectionId => {
        this.isChatConnected = true;
        eaf.event.trigger('app.chat.connected');
        eaf.log.debug('Chat reconnected. ConnectionId: ' + connectionId);
      });

      this.chatHub.onclose(error => {
        this.isChatConnected = false;
        if (error) {
          eaf.log.debug('Chat connection closed with error: ' + error);
        } else {
          eaf.log.debug('Chat disconnected');
        }
      });

      this.registerChatEvents(this.chatHub);

      try {
        await this.chatHub.start();
        this.isChatConnected = true;
        eaf.event.trigger('app.chat.connected');
      } catch (error) {
        eaf.log.error('Chat connection failed: ' + error);
      }
    });
  }
}
