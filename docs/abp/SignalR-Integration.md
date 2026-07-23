# https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration

## SignalR Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/SignalR-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#introduction)

This document is for .NET Framework 4.6.1. If you're interested in ASP.NET
Core, see the [SignalR AspNetCore Integration](https://aspnetboilerplate.com/Pages/Documents/SignalR-AspNetCore-Integration) documentation instead.

The [Abp.Web.SignalR](http://www.nuget.org/packages/Abp.Web.SignalR) NuGet
package makes it easy to use **SignalR** in ASP.NET Boilerplate-based
applications. See the [SignalR documentation](https://www.asp.net/signalr)
for more detailed information on SignalR.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#installation)

#### Server-Side [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#server-side)

Install the
[**Abp.Web.SignalR**](http://www.nuget.org/packages/Abp.Web.SignalR)
NuGet package to your project (generally to your Web layer) and add a
**dependency** to your module:

Copy

```
[DependsOn(typeof(AbpWebSignalRModule))]
public class YourProjectWebModule : AbpModule
{
    //...
}
```

Then use the **MapSignalR** method in your OWIN startup class as you always
do:

Copy

```
[assembly: OwinStartup(typeof(Startup))]
namespace MyProject.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //...

            app.MapSignalR();
        }
    }
}
```

**Note:** Abp.Web.SignalR only depends on the Microsoft.AspNet.SignalR.Core
package, so you will also need to **install** the
**[Microsoft.AspNet.SignalR](https://www.nuget.org/packages/Microsoft.AspNet.SignalR)**
package to your Web project, if you haven't installed it before (See the [SignalR\\
documents](https://www.asp.net/signalr) for more info).

#### Client-Side [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#client-side)

The **abp.signalr.js** script should be included on the page. It's located
in the
**[Abp.Web.Resources](https://www.nuget.org/packages/Abp.Web.Resources)**
package (It's already installed in the [startup templates](https://aspnetboilerplate.com/Templates)). We
should include it after signalr hubs:

Copy

```
<script src="~/signalr/hubs"></script>
<script src="~/Abp/Framework/scripts/libs/abp.signalr.js"></script>
```

That's all you have to do! SignalR is properly configured and integrated into your
project.

### Connection Establishment [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#connection-establishment)

ASP.NET Boilerplate **automatically connects** to the server (from the
client) when **abp.signalr.js** is included on your page. This is
generally fine, but there may be cases where you might not want to. You can add
these lines just before including **abp.signalr.js** to disable auto
connecting:

Copy

```javascript
<script>
    abp.signalr = abp.signalr || {};
    abp.signalr.autoConnect = false; //connects server automatically after page load (default true)
    abp.signalr.autoReconnect = true; //tries to reconnect when the client disconnects (default true)
    abp.signalr.reconnectTime = 5000; //time to wait before reconnect to the server (default is 5000 milisecond)
    abp.signalr.maxTries = 8; //max reconnect try count (default is 8)
    abp.signalr.increaseReconnectTime = function (time) { //anytime reconnection request gets fail abp will increase the time to wait before next request with using that function.
        return time * 2; //(default is twice of previous time)
    };
</script>
```

In this case, you can call the **abp.signalr.connect()** function manually
whenever you need to connect to the server.

ASP.NET Boilerplate also **automatically reconnects** to the server
(from the client) when the client disconnects, if
\*\*abp.signalr.autoReconnect \*\* is true.

The **"abp.signalr.connected"** global event is triggered when the client
connects to the server. You can register to this event to take actions
when the connection is successfully established. See the JavaScript [event\\
bus documentation](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/Event-Bus) for more
information about client-side events.

### Built-In Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#built-in-features)

You can use the full power of SignalR in your applications. Additionally,
the **Abp.Web.SignalR** package implements some built-in features.

#### Notification [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#notification)

The **Abp.Web.SignalR** package implements the **IRealTimeNotifier** to send
real-time notifications to clients (see the [notification\\
system](https://aspnetboilerplate.com/Pages/Documents/Notification-System)). This way, your users can get
real-time push notifications.

#### Online Clients [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#online-clients)

ASP.NET Boilerplate provides the **IOnlineClientManager** to get information
about online users (inject IOnlineClientManager and use the
GetByUserIdOrNull, GetAllClients, and IsOnline methods, for example).
The IOnlineClientManager needs a communication infrastructure to properly
work. The **Abp.Web.SignalR** package provides that infrastructure, so you
can inject and use IOnlineClientManager in any layer of your application,
if SignalR is installed.

#### PascalCase vs. camelCase [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#pascalcase-vs-camelcase)

The Abp.Web.SignalR package overrides SignalR's default **ContractResolver**
to use the **CamelCasePropertyNamesContractResolver** on serialization.
This way, we can have classes with **PascalCase** properties on the server
and use them as **camelCase** on the client for sending/receiving
objects (because camelCase is preferred notation in JavaScript). If you
want to ignore this for your classes in some assemblies, then you can
add those assemblies to the AbpSignalRContractResolver. **IgnoredAssemblies**
list.

### Your SignalR Code [Anchor](https://aspnetboilerplate.com/Pages/Documents/SignalR-Integration\#your-signalr-code)

The **Abp.Web.SignalR** package also simplifies your SignalR code. Imagine
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

    public void SendMessage(string message)
    {
        Clients.All.getMessage(string.Format("User {0}: {1}", AbpSession.UserId, message));
    }

    public async override Task OnConnected()
    {
        await base.OnConnected();
        Logger.Debug("A client connected to MyChatHub: " + Context.ConnectionId);
    }

    public async override Task OnDisconnected(bool stopCalled)
    {
        await base.OnDisconnected(stopCalled);
        Logger.Debug("A client disconnected from MyChatHub: " + Context.ConnectionId);
    }
}
```

We implemented the **ITransientDependency** to simply register our hub via the
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) system
(you can make it singleton based on your needs). We
[property-injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection#property-injection-pattern)
the [session](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) and
[logger](https://aspnetboilerplate.com/Pages/Documents/Logging).

**SendMessage** is a method of our hub that can be used by clients. We
call the **getMessage** function of **all** clients in this method. We can
use [AbpSession](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) to get the current user id
(if user logged in) as done above. We also made an override of **OnConnected** and
**OnDisconnected**, but for demonstration purposes only.

Here's the **client-side** JavaScript code to send/receive messages using
our hub.

Copy

```
var chatHub = $.connection.myChatHub; // Get a reference to the hub

chatHub.client.getMessage = function (message) { // Register for incoming messages
    console.log('received message: ' + message);
};

abp.event.on('abp.signalr.connected', function() { // Register to connect event
    chatHub.server.sendMessage("Hi everybody, I'm connected to the chat!"); // Send a message to the server
});
```

We can then use the **chatHub** anytime we need to send a message to the
server. See the [SignalR documentation](https://www.asp.net/signalr) for
more information.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe