var eaf = eaf || {}; // NOSONAR
(function () {

    if (!toastr) {
        return;
    }

    /* DEFAULTS *************************************************/

    toastr.options.positionClass = 'toast-bottom-right';

    /* NOTIFICATION *********************************************/

    var showNotification = function (type, message, title, options) { // NOSONAR
        toastr[type](message, title, options);
    };

    eaf.notify.success = function (message, title, options) {
        showNotification('success', message, title, options);
    };

    eaf.notify.info = function (message, title, options) {
        showNotification('info', message, title, options);
    };

    eaf.notify.warn = function (message, title, options) {
        showNotification('warning', message, title, options);
    };

    eaf.notify.error = function (message, title, options) {
        showNotification('error', message, title, options);
    };

})();