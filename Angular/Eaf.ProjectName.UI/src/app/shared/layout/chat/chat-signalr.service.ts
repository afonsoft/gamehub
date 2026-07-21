import { Injectable, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@microsoft/signalr';

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

  configureConnection(connection): void {
    // Set the common hub
    this.chatHub = connection;

    // Reconnect loop
    let reconnectTime = 5000;
    let tries = 1;
    const maxTries = 8;
    const start = async () => {
      while (tries <= maxTries) {
        try {
          await connection.start();
          reconnectTime = 5000;
          tries = 1;
          return;
        } catch {
          await new Promise(resolve => setTimeout(resolve, reconnectTime));
          reconnectTime *= 2;
          tries += 1;
        }
      }

      throw new Error('Maximum reconnect attempts exceeded');
    };

    // Reconnect if hub disconnects
    connection.onclose(e => {
      this.isChatConnected = false;

      if (e) {
        eaf.log.debug('Chat connection closed with error: ' + e);
      } else {
        eaf.log.debug('Chat disconnected');
      }

      start().then(() => {
        this.isChatConnected = true;
      });
    });

    // Register to get notifications
    this.registerChatEvents(connection);
  }

  registerChatEvents(connection): void {
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
    this._zone.runOutsideAngular(() => {
      eaf.signalr.connect();
      eaf.signalr
        .startConnection(eaf.appPath + 'signalr-chat', connection => {
          this.configureConnection(connection);
        })
        .then(() => {
          eaf.event.trigger('app.chat.connected');
          this.isChatConnected = true;
        });
    });
  }
}
