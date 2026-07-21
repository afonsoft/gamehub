(function (window) {
    window['env'] = window['env'] || {};
    window['env']['ASPNETCORE_ENVIRONMENT'] = '${ASPNETCORE_ENVIRONMENT}';
    window['env']['remoteServiceBaseUrl'] = '${REMOTE_SERVICE_BASE_URL}';
    window['env']['appBaseUrl'] = '${APP_BASE_URL}';
  })(this);
