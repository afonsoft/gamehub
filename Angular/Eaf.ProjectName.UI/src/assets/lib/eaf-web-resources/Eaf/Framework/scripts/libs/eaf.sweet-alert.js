var eaf = eaf || {}; // NOSONAR
(function ($) {
    if (!sweetAlert || !$) {
        return;
    }

    /* DEFAULTS *************************************************/

    eaf.libs = eaf.libs || {};
    eaf.libs.sweetAlert = {
        config: {
            'default': {

            },
            info: {
                icon: 'info'
            },
            success: {
                icon: 'success'
            },
            warn: {
                icon: 'warning'
            },
            error: {
                icon: 'error'
            },
            question: {
                icon: 'question'
            },
            confirm: {
                icon: 'question',
                title: 'Are you sure?',
                buttons: ['Cancel', 'Yes']
            }
        }
    };

    /* MESSAGE **************************************************/

    var showMessage = function (type, message, title, isHtml) { // NOSONAR

        if (!title) {
            title = message;
            message = undefined;
        }

        var messageContent = { // NOSONAR
            title: title
        };

        if (isHtml) {
            var el = document.createElement('div'); // NOSONAR
            el.innerHTML = message;

            messageContent.content = el;
        } else {
            messageContent.text = message;
        }

        var opts = $.extend( // NOSONAR
            {},
            eaf.libs.sweetAlert.config['default'],
            eaf.libs.sweetAlert.config[type],
            messageContent
        );

        return $.Deferred(function ($dfd) {
            sweetAlert(opts).then(function () {
                $dfd.resolve();
            });
        });
    };

    eaf.message.info = function (message, title, isHtml) {
        return showMessage('info', message, title, isHtml);
    };

    eaf.message.success = function (message, title, isHtml) {
        return showMessage('success', message, title, isHtml);
    };

    eaf.message.warn = function (message, title, isHtml) {
        return showMessage('warn', message, title, isHtml);
    };

    eaf.message.error = function (message, title, isHtml) {
        return showMessage('error', message, title, isHtml);
    };

    eaf.message.question = function (message, title, isHtml) {
        return showMessage('question', message, title, isHtml);
    };

    eaf.message.confirm = function (message, titleOrCallback, callback, isHtml) {
        var messageContent; // NOSONAR

        if (isHtml) {
            var el = document.createElement('div'); // NOSONAR
            el.innerHTML = message;

            messageContent = {
                content: el
            }
        } else {
            messageContent = {
                text: message
            }
        }

        if ($.isFunction(titleOrCallback)) {
            callback = titleOrCallback;
        } else if (titleOrCallback) {
            messageContent.title = titleOrCallback;
        }; // NOSONAR

        var opts = $.extend( // NOSONAR
            {},
            eaf.libs.sweetAlert.config['default'],
            eaf.libs.sweetAlert.config.confirm,
            messageContent
        );

        return $.Deferred(function ($dfd) {
            sweetAlert(opts).then(function (isConfirmed) {
                callback && callback(isConfirmed); // NOSONAR
                $dfd.resolve(isConfirmed);
            });
        });
    };

    eaf.event.on('eaf.dynamicScriptsInitialized', function () {
        eaf.libs.sweetAlert.config.confirm.title = eaf.localization.eaf('AreYouSure');
        eaf.libs.sweetAlert.config.confirm.buttons = [eaf.localization.eaf('Cancel'), eaf.localization.eaf('Yes')];
    });

})(jQuery);
