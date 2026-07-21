export class UrlHelper {
  /**
   * The URL requested, before initial routing.
   */
  static readonly initialUrl = encodeURI(location.href || '');

  static getQueryParameters(): any {
    return UrlHelper.getQueryParametersUsingParameters(document.location.search);
  }

  static getQueryParametersUsingParameters(search: string): any {
    return search
      .replace(/(^\?)/, '')
      .split('&')
      .map(
        function (n) {
          const parts = n.split('=');
          this[parts[0]] = parts[1];
          return this;
        }.bind({}),
      )[0];
  }

  static getQueryParametersUsingHash(): any {
    return document.location.hash
      .substring(1, document.location.hash.length)
      .replace(/(^\?)/, '')
      .split('&')
      .map(
        function (n) {
          const parts = n.split('=');
          this[parts[0]] = parts[1];
          return this;
        }.bind({}),
      )[0];
  }

  static getInitialUrlParameters(): any {
    const questionMarkIndex = UrlHelper.initialUrl.indexOf('?');
    if (questionMarkIndex >= 0) {
      return UrlHelper.initialUrl.substring(questionMarkIndex, UrlHelper.initialUrl.length);
    }

    return '';
  }

  static getReturnUrl(): string {
    const queryStringObj = UrlHelper.getQueryParametersUsingParameters(UrlHelper.getInitialUrlParameters());
    if (queryStringObj.returnUrl) {
      return decodeURIComponent(queryStringObj.returnUrl);
    }

    return '';
  }

  static getSingleSignIn(): boolean {
    const queryStringObj = UrlHelper.getQueryParametersUsingParameters(UrlHelper.getInitialUrlParameters());
    if (queryStringObj.ss) {
      return queryStringObj.ss;
    }

    return false;
  }
}
