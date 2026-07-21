var eaf = eaf || {}; // NOSONAR
(function ($) {

    //Check if SignalR is defined
    if (!$ || !$.connection) { // NOSONAR
        return;
    }

    //Create namespaces
    eaf.signalr = eaf.signalr || {};
    eaf.signalr.hubs = eaf.signalr.hubs || {};

    //Get the common hub
    eaf.signalr.hubs.common = $.connection.eafCommonHub;

    var commonHub = eaf.signalr.hubs.common; // NOSONAR
    if (!commonHub) {
        return;
    }

    //Register to get notifications
    commonHub.client.getNotification = function (notification) {
        eaf.event.trigger('eaf.notifications.received', notification);
    };

    //Connect to the server
    eaf.signalr.connect = function() {
        $.connection.hub.start().done(function () {
            eaf.log.debug('Connected to SignalR server!');
            eaf.event.trigger('eaf.signalr.connected');
            commonHub.server.register().done(function () {
                eaf.log.debug('Registered to the SignalR server!');
            });
        });
    };

    if (eaf.signalr.autoConnect === undefined) {
        eaf.signalr.autoConnect = true;
    }

    if (eaf.signalr.autoConnect) {
        eaf.signalr.connect();
    }

    //reconnect if hub disconnects
    $.connection.hub.disconnected(function () {
        if (!eaf.signalr.autoConnect) {
            return;
        }

        setTimeout(function () {
            if ($.connection.hub.state === $.signalR.connectionState.disconnected) {
                $.connection.hub.start();
            }
        }, 5000);
    });

})(jQuery);