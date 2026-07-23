(function (global) {
  'use strict';

  const CHANNEL = 'gamehub-bridge';

  class GameHubSDK {
    constructor() {
      this._parentOrigin = null;
      this._pendingPromises = new Map();
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
    }

    _post(action, payload) {
      if (!window.parent || typeof window.parent.postMessage !== 'function') {
        return;
      }
      const message = { channel: CHANNEL, action };
      if (payload !== undefined) {
        message.payload = payload;
      }
      window.parent.postMessage(message, this._parentOrigin || '*');
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
      if (data.action === 'commercialBreakCompleted') {
        const resolve = this._pendingPromises.get('commercial');
        if (resolve) {
          resolve();
          this._pendingPromises.delete('commercial');
        }
      }
      if (data.action === 'rewardedBreakCompleted') {
        const resolve = this._pendingPromises.get('rewarded');
        if (resolve) {
          resolve(payload.success === true);
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
        this._pendingPromises.set('commercial', resolve);
        this._post('commercialBreakRequested');
      });
    }

    commercialBreakCompleted() {
      // Called by the parent; no-op on the SDK side.
    }

    rewardedBreakRequested() {
      return new Promise((resolve) => {
        this._pendingPromises.set('rewarded', resolve);
        this._post('rewardedBreakRequested');
      });
    }

    rewardedBreakCompleted() {
      // Called by the parent; no-op on the SDK side.
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
    captureError: (error) => sdk.captureError(error),
    gameMeasuredEvent: (category, what, action) => sdk.gameMeasuredEvent(category, what, action),
  };
})(window);
