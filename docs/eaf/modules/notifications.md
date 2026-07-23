# Notifications in EAF (Enterprise Application Foundation)

## 1. Introduction to Notifications in EAF

Notifications are a crucial part of modern applications, providing real-time or asynchronous updates to users about events, alerts, or important information.

The EAF (Enterprise Application Foundation) is an open source platform that leverages the notification system provided by the ASP.NET Boilerplate (ABP) framework. This system allows for sending notifications to users in a consistent and extensible way.

## 2. Key Concepts

* **Notification**: Represents a message to be delivered to a user or a set of users.
* **UserNotification**: Represents the status of a notification for a specific user (e.g., unread, read).
* **RealTimeNotifier**: An abstraction for sending real-time notifications to clients (e.g., using SignalR).
* **INotificationStore**: An abstraction for persisting notifications.
* **INotificationPublisher**: An abstraction for publishing notifications.
* **INotificationConfiguration**: Configuration options for the notification system.

## 3. Sending Notifications

To send a notification, you can use the `INotificationPublisher` interface.

```csharp
// Example
using Abp.Notifications;
using Abp.Runtime.Session;
using System;
using System.Threading.Tasks;

public class MyService
{
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IAbpSession _abpSession;

    public MyService(INotificationPublisher notificationPublisher, IAbpSession abpSession)
    {
        _notificationPublisher = notificationPublisher;
        _abpSession = abpSession;
    }

    public async Task DoSomething()
    {
        await _notificationPublisher.PublishAsync(
            "MyEvent",
            new NotificationData { { "message", "Hello, world!" } },
            severity: NotificationSeverity.Info,
            userIds: new[] { _abpSession.GetUserId() }
        );
    }
}
```

## 4. Real-Time Notifications

EAF uses SignalR to push notifications to clients in real-time. The `IRealTimeNotifier` interface is used to send notifications to connected clients.

## 5. UI Integration

The EAF provides UI components for displaying notifications to users. These components typically display a list of unread notifications and allow users to mark them as read.

## 6. Customization

The EAF allows for customization of the notification system. You can create custom notification data, custom real-time notifiers, and custom UI components.

## 7. Troubleshooting

* **Notifications are not being received**: Check the SignalR configuration and make sure the client is connected to the SignalR hub.
* **Notifications are not being persisted**: Check the database configuration and make sure the notification store is configured correctly.

