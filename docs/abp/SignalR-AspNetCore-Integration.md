# https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration

## SignalR AspNetCore Integration

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/SignalR-AspNetCore-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#introduction)

The [Abp.AspNetCore.SignalR](http://www.nuget.org/packages/Abp.AspNetCore.SignalR) NuGet
package makes it easier to use **ASP.NET Core SignalR** in ASP.NET Boilerplate-based
applications.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#installation)

#### Server-Side [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#server-side)

Install the
[**Abp.AspNetCore.SignalR**](http://www.nuget.org/packages/Abp.AspNetCore.SignalR)
NuGet package to your project (generally to your Web layer) and add a
**dependency** to your module:

Copy

```
[DependsOn(typeof(AbpAspNetCoreSignalRModule))]
public class YourProjectWebModule : AbpModule
{
    //...
}
```

Then use the **AddSignalR** and **UseSignalR** methods in your Startup class:

Copy

```
using Abp.AspNetCore.SignalR.Hubs;

namespace MyProject.Web.Startup
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
            });
        }
    }
}
```

#### Client-Side (Angular) [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#client-side-angular-)

The **@aspnet/signalr** package should be added in package.json, and the signalr.min.js included under **scripts** in angular.json.

The **abp.signalr-client.js** script should be included under **assets** in angular.json.

SignalR cannot send authorization headers, so encryptedAuthToken is sent in the query string. The startup template includes SignalRAspNetCoreHelper. We should call it in ngOnInit in app.component.ts:

Copy

```
SignalRAspNetCoreHelper.initSignalR();
```

That's all you have to do. SignalR is properly configured and integrated into your project.

#### Client-Side (jQuery) [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#client-side-jquery-)

The **abp.signalr-client.js** script should be included on the page. It's located
in the
**[Abp.Web.Resources](https://www.nuget.org/packages/Abp.Web.Resources)**
package (and already installed in the [startup templates](https://aspnetboilerplate.com/Templates)). We
should include it after the signalr.min.js:

Copy

```
<script src="~/lib/signalr-client/signalr.min.js"></script>
<script src="~/lib/abp-web-resources/Abp/Framework/scripts/libs/abp.signalr-client.js"></script>
```

That's all you have to do. SignalR is properly configured and integrated into your project.

### Connection Establishment [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#connection-establishment)

ASP.NET Boilerplate **automatically connects** to the server (from the
client) when **abp.signalr-client.js** is included on your page. This is
generally fine. But there may be some cases where you may not want it. You can add
these lines just before including **abp.signalr-client.js** to disable auto
connecting:

Copy

```
<script>
    abp.signalr = abp.signalr || {};
    abp.signalr.autoConnect = false;
    abp.signalr.reconnectTime = 5000;
    abp.signalr.maxTries = 8;
    abp.signalr.withUrlOptions = {};
    abp.signalr.increaseReconnectTime = function (time) { //anytime reconnection request gets fail abp will increase the time to wait before next request with using that function.
        return time * 2; //(default is twice of previous time)
    };
</script>
```

This is mostly useful when you need to register your custom events to SignalR becasue if you register your events after `abp.signalr.connect()` is called, your events will not be triggered.

Note: See [Official SignalR documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-6.0&tabs=javascript#configure-additional-options) for withUrlOptions values.

In this case, you can call the **abp.signalr.connect()** function manually
whenever you need to connect to the server.

ASP.NET Boilerplate also **automatically reconnects** to the server
(from the client) when the client disconnects, if
**abp.signalr.autoConnect** is true. At most **abp.signalr.maxTries** times it tries to connect to the server. It starts to wait for **abp.signalr.reconnectTime** ms, then each time connection fails, it waits 2 times longer. (for example: 5000ms - 10000ms - 20000ms...). (You can override **abp.signalr.increaseReconnectTime** to change increasing method)

The **"abp.signalr.connected"** global event is triggered when the client
connects to the server. You can register to this event to take actions
when the connection is successfully established. See the JavaScript [event\\
bus documentation](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/Event-Bus) for more information
about client-side events.

### Built-In Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#built-in-features)

You can use all the power of SignalR in your applications. Additionally, the
**Abp.AspNetCore.SignalR** package implements some built-in features.

#### Notification [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#notification)

The **Abp.AspNetCore.SignalR** package implements the **IRealTimeNotifier** to send
real-time notifications to clients (see the [notification\\
system](https://aspnetboilerplate.com/Pages/Documents/Notification-System)). This way, your users can get
real-time push notifications!

#### Online Clients [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#online-clients)

ASP.NET Boilerplate provides the **IOnlineClientManager** to get information
about online users (inject IOnlineClientManager and use
GetByUserIdOrNull, GetAllClients, IsOnline methods for example).
The IOnlineClientManager needs a communication infrastructure to properly
work. The **Abp.AspNetCore.SignalR** package provides this infrastructure, so you
can inject and use IOnlineClientManager in any layer of your application
if SignalR is installed.

### Your SignalR Code [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration\#your-signalr-code)

The **Abp.AspNetCore.SignalR** package also simplifies your SignalR code. Consider
that we want to add a Hub to our application:

Copy

```
public class MyChatHub : Hub, ITransientDependency
{
    public IAbpSession AbpSession { get; set; }

    public ILogger Logger { get; set; }

    public MyChatHub()
    {
        AbpSession = NullAbpSession.Instance;
        Logger = NullLogger.Instance;
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("getMessage", string.Format("User {0}: {1}", AbpSession.UserId, message));
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Logger.Debug("A client connected to MyChatHub: " + Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
        Logger.Debug("A client disconnected from MyChatHub: " + Context.ConnectionId);
    }
}
```

Copy

```
endpoints.MapHub<MyChatHub>("/signalr-myChatHub"); // Prefix with '/signalr'
```

We implemented the **ITransientDependency** interface to simply register our hub to the
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) system
(you can make it a singleton based on your needs). We
[property-injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection#property-injection-pattern)
the [session](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) and
[logger](https://aspnetboilerplate.com/Pages/Documents/Logging).
Alternatively, we can inherit AbpHubBase.

**SendMessage** is a method of our hub that can be used by clients. We
call the **getMessage** function of **all** clients in this method. We can
use the [AbpSession](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) to get the current user id
(if user logged in) as done above. We also overrode **OnConnectedAsync** and
**OnDisconnectedAsync**, which is just for demonstration purposes and not needed here.

Here is the **client-side** JavaScript code to send/receive messages using
our hub.

Copy

```
var chatHub = null;

abp.signalr.startConnection(abp.appPath + 'signalr-myChatHub', function (connection) {
    chatHub = connection; // Save a reference to the hub

    connection.on('getMessage', function (message) { // Register for incoming messages
        console.log('received message: ' + message);
    });
}).then(function (connection) {
    abp.log.debug('Connected to myChatHub server!');
    abp.event.trigger('myChatHub.connected');
});

abp.event.on('myChatHub.connected', function() { // Register for connect event
    chatHub.invoke('sendMessage', "Hi everybody, I'm connected to the chat!"); // Send a message to the server
});
```

We can then use the **chatHub** anytime we need to send messages to the
server.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe