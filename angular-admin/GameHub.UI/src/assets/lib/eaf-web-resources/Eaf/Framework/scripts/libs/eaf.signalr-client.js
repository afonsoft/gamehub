var eaf = eaf || {}; // NOSONAR
(function () {

    // Check if SignalR is defined
    if (!signalR) {
        return;
    }

    // Create namespaces
    eaf.signalr = eaf.signalr || {};
    eaf.signalr.hubs = eaf.signalr.hubs || {};
    eaf.signalr.reconnectTime = eaf.signalr.reconnectTime || 5000;
    eaf.signalr.maxTries = eaf.signalr.maxTries || 8;
    eaf.signalr.increaseReconnectTime = eaf.signalr.increaseReconnectTime || function (time) {
        return time * 2;
    };

    eaf.signalr.withUrlOptions = eaf.signalr.withUrlOptions || {};

    // Configure the connection for eaf.signalr.hubs.common
    function configureConnection(connection) {
        // Set the common hub
        eaf.signalr.hubs.common = connection;

        let tries = 1;
        let reconnectTime = eaf.signalr.reconnectTime;

        // Reconnect loop
        function tryReconnect() {
            if (tries <= eaf.signalr.maxTries) {
                connection.start()
                    .then(function () {
                        reconnectTime = eaf.signalr.reconnectTime;
                        tries = 1;
                        eaf.event.trigger('eaf.signalr.reconnected');
                    }).catch(function () {
                    tries += 1;
                    reconnectTime = eaf.signalr.increaseReconnectTime(reconnectTime);
                    setTimeout(function () { // NOSONAR
                            tryReconnect();
                        },
                        reconnectTime
                    );
                });
            }
        }

        // Reconnect if hub disconnects
        connection.onclose(function (e) {
            if (e) {
                eaf.log.debug('Connection closed with error: ' + e);
            } else {
                eaf.log.debug('Disconnected');
            }

            if (!eaf.signalr.autoReconnect) {
                return;
            }

            eaf.event.trigger('eaf.signalr.disconnected');
            tryReconnect();
        });

        // Register to get notifications
        connection.on('getNotification', function (notification) {
            eaf.event.trigger('eaf.notifications.received', notification);
        });
    }

    // Connect to the server for eaf.signalr.hubs.common
    function connect() {
        let url = eaf.signalr.url || (eaf.appPath + 'signalr');

        // Start the connection
        startConnection(url, configureConnection)
            .then(function (connection) {
                eaf.event.trigger('eaf.signalr.connected');
                // Call the Register method on the hub
                connection.invoke('register').then(function () {
                });
            })
            .catch(function (error) {
                eaf.log.debug(error.message);
            });
    }

    // Starts a connection with transport fallback - if the connection cannot be started using
    // the webSockets transport the function will fallback to the serverSentEvents transport and
    // if this does not work it will try longPolling. If the connection cannot be started using
    // any of the available transports the function will return a rejected Promise.
    function startConnection(url, configureConnection) {
        if (eaf.signalr.remoteServiceBaseUrl) {
            url = eaf.signalr.remoteServiceBaseUrl + url;
        }

        // Add query string: https://github.com/aspnet/SignalR/issues/680
        if (eaf.signalr.qs) {
            url += (url.indexOf('?') == -1 ? '?' : '&') + eaf.signalr.qs; // NOSONAR
        }

        return function start(transport) {
            eaf.log.debug('Starting connection using ' + signalR.HttpTransportType[transport] + ' transport');
            let connection = new signalR.HubConnectionBuilder()
                .withUrl(url, transport)
                .build();
            connection.serverTimeoutInMilliseconds = 120000;
            connection.keepAliveIntervalInMilliseconds = 15000;

            if (configureConnection && typeof configureConnection === 'function') {
                configureConnection(connection);
            }

            return connection.start()
                .then(function () {
                    return connection;
                })
                .catch(function (error) {
                    eaf.log.debug('Cannot start the connection using ' + signalR.HttpTransportType[transport] + ' transport. ' + error.message);
                    if (transport !== signalR.HttpTransportType.LongPolling) {
                        return start(transport + 1);
                    }

                    return Promise.reject(error); // NOSONAR
                });
        }(signalR.HttpTransportType.WebSockets);
    }

    eaf.signalr.autoConnect = eaf.signalr.autoConnect === undefined ? true : eaf.signalr.autoConnect;
    eaf.signalr.autoReconnect = eaf.signalr.autoReconnect === undefined ? true : eaf.signalr.autoReconnect;
    eaf.signalr.connect = eaf.signalr.connect || connect;
    eaf.signalr.startConnection = eaf.signalr.startConnection || startConnection;

    if (eaf.signalr.autoConnect && !eaf.signalr.hubs.common) {
        eaf.signalr.connect();
    }
})();
