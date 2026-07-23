(function (global) {
  'use strict';

  const CHANNEL = 'gamehub-bridge';

  class GameHubSDK {
    constructor() {
      this._parentOrigin = null;
      this._pendingPromises = new Map();
      this._requestId = 0;
      window.addEventListener('message', (event) => this._handleMessage(event));
    }

    init(parentOrigin) {
      if (parentOrigin) {
        this._parentOrigin = parentOrigin;
      } else {
        try {
          this._parentOrigin = new URL(document.referrer).origin;
        } catch {
          this._parentOrigin = '*';
        }
      }

      return new Promise((resolve, reject) => {
        const requestId = this._nextRequestId();
        this._pendingPromises.set(requestId, { resolve, reject });
        this._post('init', {}, requestId);
        setTimeout(() => {
          if (this._pendingPromises.has(requestId)) {
            this._pendingPromises.delete(requestId);
            resolve();
          }
        }, 3000);
      });
    }

    _nextRequestId() {
      this._requestId += 1;
      return `req_${this._requestId}`;
    }

    _post(action, payload, requestId) {
      if (!window.parent || typeof window.parent.postMessage !== 'function') {
        return;
      }
      const message = { channel: CHANNEL, action, requestId };
      if (payload !== undefined) {
        message.payload = payload;
      }
      window.parent.postMessage(message, this._parentOrigin || '*');
    }

    _postPromise(action, payload) {
      return new Promise((resolve, reject) => {
        const requestId = this._nextRequestId();
        this._pendingPromises.set(requestId, { resolve, reject });
        this._post(action, payload, requestId);
        setTimeout(() => {
          if (this._pendingPromises.has(requestId)) {
            this._pendingPromises.delete(requestId);
            reject(new Error(`Timeout waiting for ${action}`));
          }
        }, 5000);
      });
    }

    _handleMessage(event) {
      if (this._parentOrigin && this._parentOrigin !== '*' && event.origin !== this._parentOrigin) {
        return;
      }
      const data = event.data;
      if (!data || data.channel !== CHANNEL) {
        return;
      }
      const payload = data.payload || {};
      const pending = this._pendingPromises.get(data.requestId);

      if (pending) {
        this._pendingPromises.delete(data.requestId);
        if (payload.error) {
          pending.reject(new Error(payload.error));
        } else {
          pending.resolve(payload.data);
        }
        return;
      }

      // Legacy direct action handling for commercial/rewarded breaks.
      if (data.action === 'commercialBreakCompleted') {
        const commercial = this._pendingPromises.get('commercial');
        if (commercial) {
          commercial.resolve();
          this._pendingPromises.delete('commercial');
        }
      }
      if (data.action === 'rewardedBreakCompleted') {
        const rewarded = this._pendingPromises.get('rewarded');
        if (rewarded) {
          rewarded.resolve(payload.success === true);
          this._pendingPromises.delete('rewarded');
        }
      }
    }

    gameLoadingStarted() {
      this._post('gameLoadingStarted');
    }

    gameLoadingFinished() {
      this._post('gameLoadingFinished');
    }

    gameplayStart() {
      this._post('gameplayStart');
    }

    gameplayStop() {
      this._post('gameplayStop');
    }

    commercialBreakRequested() {
      return new Promise((resolve) => {
        this._pendingPromises.set('commercial', { resolve });
        this._post('commercialBreakRequested');
      });
    }

    commercialBreakCompleted() {
      // Called by the parent; no-op on the SDK side.
    }

    rewardedBreakRequested() {
      return new Promise((resolve) => {
        this._pendingPromises.set('rewarded', { resolve });
        this._post('rewardedBreakRequested');
      });
    }

    rewardedBreakCompleted() {
      // Called by the parent; no-op on the SDK side.
    }

    getPlayerData(keys) {
      return this._postPromise('getPlayerData', { keys });
    }

    setPlayerData(data) {
      return this._postPromise('setPlayerData', { data });
    }

    login() {
      return this._postPromise('login');
    }

    getUser() {
      return this._postPromise('getUser');
    }

    getToken() {
      return this._postPromise('getToken');
    }

    captureError(error) {
      this._post('gameErrorCaptured', { error: error?.message || String(error) });
    }

    gameMeasuredEvent(category, what, action) {
      this._post('gameMeasuredEvent', { category, what, action });
    }
  }

  const sdk = new GameHubSDK();

  global.GameHubSDK = {
    init: (origin) => sdk.init(origin),
    gameLoadingStarted: () => sdk.gameLoadingStarted(),
    gameLoadingFinished: () => sdk.gameLoadingFinished(),
    gameplayStart: () => sdk.gameplayStart(),
    gameplayStop: () => sdk.gameplayStop(),
    commercialBreakRequested: () => sdk.commercialBreakRequested(),
    commercialBreakCompleted: () => sdk.commercialBreakCompleted(),
    rewardedBreakRequested: () => sdk.rewardedBreakRequested(),
    rewardedBreakCompleted: () => sdk.rewardedBreakCompleted(),
    getPlayerData: (keys) => sdk.getPlayerData(keys),
    setPlayerData: (data) => sdk.setPlayerData(data),
    login: () => sdk.login(),
    getUser: () => sdk.getUser(),
    getToken: () => sdk.getToken(),
    captureError: (error) => sdk.captureError(error),
    gameMeasuredEvent: (category, what, action) => sdk.gameMeasuredEvent(category, what, action),
  };
})(window);
