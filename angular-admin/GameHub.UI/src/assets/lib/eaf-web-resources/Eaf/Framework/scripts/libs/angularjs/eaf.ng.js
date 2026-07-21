(function (eaf, angular) {

    if (!angular) {
        return;
    }

    eaf.ng = eaf.ng || {};

    eaf.ng.http = {
        defaultError: {
            message: 'An error has occurred!',
            details: 'Error detail not sent by server.'
        },

        defaultError401: {
            message: 'You are not authenticated!',
            details: 'You should be authenticated (sign in) in order to perform this operation.'
        },

        defaultError403: {
            message: 'You are not authorized!',
            details: 'You are not allowed to perform this operation.'
        },

        defaultError404: {
            message: 'Resource not found!',
            details: 'The resource requested could not be found on the server.'
        },

        logError: function (error) {
            eaf.log.error(error);
        },

        showError: function (error) {
            if (error.details) {
                return eaf.message.error(error.details, error.message || eaf.ng.http.defaultError.message);
            } else {
                return eaf.message.error(error.message || eaf.ng.http.defaultError.message);
            }
        },

        handleTargetUrl: function (targetUrl) {
            if (!targetUrl) {
                location.href = eaf.appPath;
            } else {
                location.href = targetUrl;
            }
        },

        handleNonEafErrorResponse: function (response, defer) {
            if (response.config.eafHandleError !== false) {
                switch (response.status) {
                    case 401:
                        eaf.ng.http.handleUnAuthorizedRequest(
                            eaf.ng.http.showError(eaf.ng.http.defaultError401),
                            eaf.appPath
                        );
                        break;
                    case 403:
                        eaf.ng.http.showError(eaf.ajax.defaultError403);
                        break;
                    case 404:
                        eaf.ng.http.showError(eaf.ajax.defaultError404);
                        break;
                    default:
                        eaf.ng.http.showError(eaf.ng.http.defaultError);
                        break;
                }
            }

            defer.reject(response);
        },

        handleUnAuthorizedRequest: function (messagePromise, targetUrl) {
            if (messagePromise) {
                messagePromise.done(function () {
                    eaf.ng.http.handleTargetUrl(targetUrl || eaf.appPath);
                });
            } else {
                eaf.ng.http.handleTargetUrl(targetUrl || eaf.appPath);
            }
        },

        handleResponse: function (response, defer) {
            var originalData = response.data; // NOSONAR

            if (originalData.success === true) {
                response.data = originalData.result;
                defer.resolve(response);

                if (originalData.targetUrl) {
                    eaf.ng.http.handleTargetUrl(originalData.targetUrl);
                }
            } else if (originalData.success === false) {
                var messagePromise = null; // NOSONAR

                if (originalData.error) {
                    if (response.config.eafHandleError !== false) {
                        messagePromise = eaf.ng.http.showError(originalData.error);
                    }
                } else {
                    originalData.error = defaultError;
                }

                eaf.ng.http.logError(originalData.error);

                response.data = originalData.error;
                defer.reject(response);

                if (response.status == 401 && response.config.eafHandleError !== false) {
                    eaf.ng.http.handleUnAuthorizedRequest(messagePromise, originalData.targetUrl);
                }
            } else { //not wrapped result
                defer.resolve(response);
            }
        }
    }

    var eafModule = angular.module('eaf', []); // NOSONAR

    eafModule.config([
        '$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push(['$q', function ($q) {

                return {

                    'request': function (config) {
                        if (config.url.indexOf('.cshtml') !== -1) { // NOSONAR
                            config.url = eaf.appPath + 'EafAppView/Load?viewUrl=' + config.url + '&_t=' + eaf.pageLoadTime.getTime();
                        }

                        return config;
                    },

                    'response': function (response) {
                        if (!response.data || !response.data.__abp) { // NOSONAR
                            //Non EAF related return value
                            return response;
                        }

                        var defer = $q.defer(); // NOSONAR
                        eaf.ng.http.handleResponse(response, defer);
                        return defer.promise;
                    },

                    'responseError': function (ngError) {
                        var defer = $q.defer(); // NOSONAR

                        if (!ngError.data || !ngError.data.__abp) { // NOSONAR
                            eaf.ng.http.handleNonEafErrorResponse(ngError, defer);
                        } else {
                            eaf.ng.http.handleResponse(ngError, defer);
                        }

                        return defer.promise;
                    }

                };
            }]);
        }
    ]);

    eaf.event.on('eaf.dynamicScriptsInitialized', function () {
        eaf.ng.http.defaultError.message = eaf.localization.eaf('DefaultError');
        eaf.ng.http.defaultError.details = eaf.localization.eaf('DefaultErrorDetail');
        eaf.ng.http.defaultError401.message = eaf.localization.eaf('DefaultError401');
        eaf.ng.http.defaultError401.details = eaf.localization.eaf('DefaultErrorDetail401');
        eaf.ng.http.defaultError403.message = eaf.localization.eaf('DefaultError403');
        eaf.ng.http.defaultError403.details = eaf.localization.eaf('DefaultErrorDetail403');
        eaf.ng.http.defaultError404.message = eaf.localization.eaf('DefaultError404');
        eaf.ng.http.defaultError404.details = eaf.localization.eaf('DefaultErrorDetail404');
    });

})((eaf || (eaf = {})), (angular || undefined)); // NOSONAR
