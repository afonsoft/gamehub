var eaf = eaf || {}; // NOSONAR
(function ($) {

    if (!$) {
        return;
    }

    /* JQUERY ENHANCEMENTS ***************************************************/

    // eaf.ajax -> uses $.ajax ------------------------------------------------

    eaf.ajax = function (userOptions) {
        userOptions = userOptions || {};

        var options = $.extend(true, {}, eaf.ajax.defaultOpts, userOptions); // NOSONAR
        var oldBeforeSendOption = options.beforeSend; // NOSONAR
        options.beforeSend = function(xhr) {
            if (oldBeforeSendOption) {
                 oldBeforeSendOption(xhr);
            }

            xhr.setRequestHeader("Pragma", "no-cache");
            xhr.setRequestHeader("Cache-Control", "no-cache");
            xhr.setRequestHeader("Expires", "Sat, 01 Jan 2000 00:00:00 GMT");
        };

        options.success = undefined;
        options.error = undefined;

        return $.Deferred(function ($dfd) {
            $.ajax(options)
                .done(function (data, textStatus, jqXHR) {
                    if (data.__abp) {
                        eaf.ajax.handleResponse(data, userOptions, $dfd, jqXHR);
                    } else {
                        $dfd.resolve(data);
                        userOptions.success && userOptions.success(data); // NOSONAR
                    }
                }).fail(function (jqXHR) {
                    if (jqXHR.responseJSON && jqXHR.responseJSON.__abp) { // NOSONAR
                        eaf.ajax.handleResponse(jqXHR.responseJSON, userOptions, $dfd, jqXHR);
                    } else {
                        eaf.ajax.handleNonEafErrorResponse(jqXHR, userOptions, $dfd);
                    }
                });
        });
    };

    $.extend(eaf.ajax, {
        defaultOpts: {
            dataType: 'json',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        },

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
            details: 'The resource requested could not found on the server.'
        },

        logError: function (error) {
            eaf.log.error(error);
        },

        showError: function (error) {
            if (error.details) {
                return eaf.message.error(error.details, error.message);
            } else {
                return eaf.message.error(error.message || eaf.ajax.defaultError.message);
            }
        },

        handleTargetUrl: function (targetUrl) {
            if (!targetUrl) {
                location.href = eaf.appPath;
            } else {
                location.href = targetUrl;
            }
        },

        handleNonEafErrorResponse: function (jqXHR, userOptions, $dfd) {
            if (userOptions.eafHandleError !== false) {
                switch (jqXHR.status) {
                    case 401:
                        eaf.ajax.handleUnAuthorizedRequest(
                            eaf.ajax.showError(eaf.ajax.defaultError401),
                            eaf.appPath
                        );
                        break;
                    case 403:
                        eaf.ajax.showError(eaf.ajax.defaultError403);
                        break;
                    case 404:
                        eaf.ajax.showError(eaf.ajax.defaultError404);
                        break;
                    default:
                        eaf.ajax.showError(eaf.ajax.defaultError);
                        break;
                }
            }

            $dfd.reject.apply(this, arguments);
            userOptions.error && userOptions.error.apply(this, arguments); // NOSONAR
        },

        handleUnAuthorizedRequest: function (messagePromise, targetUrl) {
            if (messagePromise) {
                messagePromise.done(function () {
                    eaf.ajax.handleTargetUrl(targetUrl);
                });
            } else {
                eaf.ajax.handleTargetUrl(targetUrl);
            }
        },

        handleResponse: function (data, userOptions, $dfd, jqXHR) { // NOSONAR
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                eaf.log.info('[EAF jQuery handleResponse] Received data:', data);
                eaf.log.info('[EAF jQuery handleResponse] data.success:', data ? data.success : 'no data');
            }
            if (data) {
                if (data.success === true) {
                    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                        eaf.log.info('[EAF jQuery handleResponse] Success - resolving with result:', data.result);
                    }
                    $dfd && $dfd.resolve(data.result, data, jqXHR); // NOSONAR
                    userOptions.success && userOptions.success(data.result, data, jqXHR); // NOSONAR

                    if (data.targetUrl) {
                        eaf.ajax.handleTargetUrl(data.targetUrl);
                    }
                } else if (data.success === false) {
                    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                        eaf.log.info('[EAF jQuery handleResponse] Error - success is false');
                    }
                    var messagePromise = null; // NOSONAR

                    if (data.error) {
                        if (userOptions.eafHandleError !== false) {
                            messagePromise = eaf.ajax.showError(data.error);
                        }
                    } else {
                        data.error = eaf.ajax.defaultError;
                    }

                    eaf.ajax.logError(data.error);

                    $dfd && $dfd.reject(data.error, jqXHR); // NOSONAR
                    userOptions.error && userOptions.error(data.error, jqXHR); // NOSONAR

                    if (jqXHR.status === 401 && userOptions.eafHandleError !== false) {
                        eaf.ajax.handleUnAuthorizedRequest(messagePromise, data.targetUrl);
                    }
                } else { //not wrapped result
                    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                        eaf.log.info('[EAF jQuery handleResponse] Not wrapped result - resolving with data:', data);
                    }
                    $dfd && $dfd.resolve(data, null, jqXHR); // NOSONAR
                    userOptions.success && userOptions.success(data, null, jqXHR); // NOSONAR
                }
            } else { //no data sent to back
                if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                    eaf.log.info('[EAF jQuery handleResponse] No data - resolving with jqXHR');
                }
                $dfd && $dfd.resolve(jqXHR); // NOSONAR
                userOptions.success && userOptions.success(jqXHR); // NOSONAR
            }
        },

        blockUI: function (options) {
            if (options.blockUI) {
                if (options.blockUI === true) { //block whole page
                    eaf.ui.setBusy();
                } else { //block an element
                    eaf.ui.setBusy(options.blockUI);
                }
            }
        },

        unblockUI: function (options) {
            if (options.blockUI) {
                if (options.blockUI === true) { //unblock whole page
                    eaf.ui.clearBusy();
                } else { //unblock an element
                    eaf.ui.clearBusy(options.blockUI);
                }
            }
        },

        ajaxSendHandler: function (event, request, settings) {
            var token = eaf.security.antiForgery.getToken(); // NOSONAR
            if (!token) {
                return;
            }

            if (!eaf.security.antiForgery.shouldSendToken(settings)) {
                return;
            }

            if (!settings.headers || settings.headers[eaf.security.antiForgery.tokenHeaderName] === undefined) { // NOSONAR
                request.setRequestHeader(eaf.security.antiForgery.tokenHeaderName, token);
            }
        }
    });

    $(document).ajaxSend(function (event, request, settings) {
        return eaf.ajax.ajaxSendHandler(event, request, settings);
    });

    /* JQUERY PLUGIN ENHANCEMENTS ********************************************/

    /* jQuery Form Plugin
     * http://www.malsup.com/jquery/form/
     */

    // eafAjaxForm -> uses ajaxForm ------------------------------------------

    if ($.fn.ajaxForm) {
        $.fn.eafAjaxForm = function (userOptions) {
            userOptions = userOptions || {};

            var options = $.extend({}, $.fn.eafAjaxForm.defaults, userOptions); // NOSONAR

            options.beforeSubmit = function () {
                eaf.ajax.blockUI(options);
                userOptions.beforeSubmit && userOptions.beforeSubmit.apply(this, arguments); // NOSONAR
            };

            options.success = function (data) {
                eaf.ajax.handleResponse(data, userOptions);
            };

            options.complete = function () {
                eaf.ajax.unblockUI(options);
                userOptions.complete && userOptions.complete.apply(this, arguments); // NOSONAR
            };

            return this.ajaxForm(options);
        };

        $.fn.eafAjaxForm.defaults = {
            method: 'POST'
        };
    }

    eaf.event.on('eaf.dynamicScriptsInitialized', function () {
        eaf.ajax.defaultError.message = eaf.localization.eaf('DefaultError');
        eaf.ajax.defaultError.details = eaf.localization.eaf('DefaultErrorDetail');
        eaf.ajax.defaultError401.message = eaf.localization.eaf('DefaultError401');
        eaf.ajax.defaultError401.details = eaf.localization.eaf('DefaultErrorDetail401');
        eaf.ajax.defaultError403.message = eaf.localization.eaf('DefaultError403');
        eaf.ajax.defaultError403.details = eaf.localization.eaf('DefaultErrorDetail403');
        eaf.ajax.defaultError404.message = eaf.localization.eaf('DefaultError404');
        eaf.ajax.defaultError404.details = eaf.localization.eaf('DefaultErrorDetail404');
    });

})(jQuery);
