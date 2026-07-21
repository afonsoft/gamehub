var eaf = eaf || {}; // NOSONAR
(function () {
    var showMessage = function (type, message, title, isHtml, opts) { // NOSONAR

        eaf.ui.clearBusy();

        if (!title) {
            title = message;
            message = undefined;
        }

        opts = opts || {};
        opts.title = title;
        opts.type = type;
        opts.confirmButtonText = opts.confirmButtonText || eaf.localization.eaf('Ok');

        if (isHtml) {
            opts.html = message;
        } else {
            opts.text = message;
        }

        return swal(opts);
    };

    eaf.message.info = function (message, title, isHtml, opts) {
        return showMessage('info', message, title, isHtml, opts);
    };

    eaf.message.success = function (message, title, isHtml, opts) {
        return showMessage('success', message, title, isHtml, opts);
    };

    eaf.message.warn = function (message, title, isHtml, opts) {
        return showMessage('warning', message, title, isHtml, opts);
    };

    eaf.message.error = function (message, title, isHtml, opts) {
        return showMessage('error', message, title, isHtml, opts); 
    };
    
    eaf.message.question = function (message, title, isHtml, opts) {
        return showMessage('question', message, title, isHtml, opts); 
    };

    eaf.message.confirm = function (message, titleOrCallback, callback, isHtml, opts) {

        var title = undefined; // NOSONAR

        if (typeof titleOrCallback === "function") {
            callback = titleOrCallback;
        }
        else if (titleOrCallback) {
            title = titleOrCallback;
        }; // NOSONAR


        opts = opts || {};
        opts.title = title ? title : eaf.localization.eaf('AreYouSure'); // NOSONAR
        opts.type = 'warning';

        opts.confirmButtonText = opts.confirmButtonText || eaf.localization.eaf('Yes');
        opts.cancelButtonText = opts.cancelButtonText || eaf.localization.eaf('Cancel');
        opts.showCancelButton = true;
        opts.reverseButtons = true;

        if (isHtml) {
            opts.html = message;
        } else {
            opts.text = message;
        }

        return swal(opts).then(function(result) {
            callback && callback(result.value); // NOSONAR
        });
    };
})();
