# https://aspnetboilerplate.com/Pages/Documents/Webhook-System

## Webhook System

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Webhook-System.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#introduction)

Webhooks are used to **inform** tenants of specific events. ASP.NET Boilerplate provides a **pub/sub** (publish/subscribe) based webhook system.

### Webhook Definitions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#webhook-definitions)

First of all, you have to define webhooks that your system allows.

You must create a class inherited from `WebhookDefinitionProvider` and define your **WebhookDefinition** s as seen below.

Copy

```csharp
public class AppWebhookDefinitionProvider : WebhookDefinitionProvider
{
    public override void SetWebhooks(IWebhookDefinitionContext context)
    {
        context.Manager.Add(new WebhookDefinition(
            name: AppWebHookNames.NewUserRegistered,
            displayName: L("NewUserRegisteredWebhookDefinition")
        ));

        context.Manager.Add(new WebhookDefinition(
            name: AppWebHookNames.TenantDeleted,
            displayName: L("TenantDeletedWebhookDefinition"),
            description: L("DescriptionTenantDeletedWebhookDefinition"),
            featureDependency: new SimpleFeatureDependency(AppFeatures.TestCheckFeature)
        ));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, AbpZeroTemplateConsts.LocalizationSourceName);
    }
}
```

_WebhookDefinition.cs:_

| Parameter | Summary |
| --- | --- |
| Name\* (string) | Unique name of the webhook. Must be unique. <br>(You can use `IWebhookDefinitionManager.Contains` method to check whether it is already exists) |
| DisplayName (ILocalizableString) | Display name of the webhook. |
| Description (ILocalizableString) | Description for the webhook. |
| FeatureDependency (IFeatureDependency) | A [Feature Dependency](https://aspnetboilerplate.com/Pages/Documents/Feature-Management). A defined webhook will be available to a tenant if this feature is enabled on the tenant.<br>(All webhooks are available for the host) |

After defining such a webhook provider, you must register it in the [PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System#preinitialize) method of our module, as shown below:

Copy

```csharp
public class AbpZeroTemplateCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Webhooks.Providers.Add<AppWebhookDefinitionProvider>();
    }

    //...
}
```

### Subscribe to Webhook(s) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#subscribe-to-webhook-s-)

The **IWebhookSubscriptionManager** provides an API to **subscribe** to webhook.

Examples:

Copy

```csharp
 private readonly IWebhookSubscriptionManager _webHookSubscriptionManager;

 public WebhookAppService(IWebhookSubscriptionManager webHookSubscriptionManager)
 {
     _webHookSubscriptionManager = webHookSubscriptionManager;
 }

 public async Task<string> AddTestSubscription()
 {
     var webhookSubscription = new WebhookSubscription()
     {
         TenantId = AbpSession.TenantId,
         WebhookUri = "http://localhost:21021/MyWebHookEndpoint",
         Webhooks = new List<string>()
         {
             AppWebHookNames.NewUserRegistered,
             AppWebHookNames.TenantDeleted
         },
         Headers = new Dictionary<string, string>()
         {
             { "MyTestHeaderKey", "MyTestHeaderValue" },
             { "MyTestHeaderKey2", "MyTestHeaderValue2" }
         }
     };
     await _webHookSubscriptionManager.AddOrUpdateSubscriptionAsync(webhookSubscription);
     return webhookSubscription.Secret;
 }
```

_WebhookSubscription.cs:_

| Parameter | Summary |
| --- | --- |
| TenantId\* (int?) | Subscriber tenant's unique identifier |
| WebhookUri\* (string) | Your webhook endpoint |
| Webhooks (list of string) | List of subscribed webhook names ``(`WebhookDefinition.Name`)``. You can receive a webhook event if you subscribe it. |
| Headers (Dictionary) | Additional headers. You can add an additional header to the subscription. Your webhook will contain that header(s) additionally. |
| Secret (string) | Your private webhook secret. You can verify the received webhook by using that key. Do not share it publicly.<br>(This value is automatically generated when you create a secret. Modification not recommended) |

##### **Check Signature** [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#check-signature)

Abp currently uses the **SHA256** hash algorithm to create a signature. It creates a signature by hashing the HTTP request's body. Your other application which subscribed to a webhook should hash received body and check whether it is equal with received one.

Copy

```csharp
[HttpPost]
public async Task WebHookTest()
{
    using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
    {
        var body = await reader.ReadToEndAsync();

        if (!IsSignatureCompatible("whs_YOURWEBHOOKSECRET", body))//read webhooksecret from user secret
        {
            throw new Exception("Unexpected Signature");
        }
        //It is certain that Webhook has not been modified.
    }
}

private bool IsSignatureCompatible(string secret, string body)
{
    if (!HttpContext.Request.Headers.ContainsKey("abp-webhook-signature"))
    {
        return false;
    }

    var receivedSignature = HttpContext.Request.Headers["abp-webhook-signature"].ToString().Split("=");//will be something like "sha256=whs_XXXXXXXXXXXXXX"
    //It starts with hash method name (currently "sha256") then continue with signature. You can also check if your hash method is true.

    string computedSignature;
    switch (receivedSignature[0])
    {
        case "sha256":
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            using (var hasher = new HMACSHA256(secretBytes))
            {
                var data = Encoding.UTF8.GetBytes(body);
                computedSignature = BitConverter.ToString(hasher.ComputeHash(data));
            }
            break;
        default:
            throw new NotImplementedException();
    }
    return computedSignature == receivedSignature[1];
}
```

### Publish Webhooks [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#publish-webhooks)

**IWebhookPublisher** is used to publish webhooks. Examples:

Copy

```csharp
public class AppWebhookPublisher : DomainService, IAppWebhookPublisher
{
    private readonly IWebhookPublisher _webHookPublisher;

    public AppWebhookPublisher(IWebhookPublisher webHookPublisher)
    {
        _webHookPublisher = webHookPublisher;
    }

    public async Task NewUserRegisteredAsync(User user)
    {
        await _webHookPublisher.PublishAsync(AppWebHookNames.NewUserRegistered,
            new
            {
                UserName = user.UserName,
                EmailAddress = user.EmailAddress
            }
        );
    }

    public async Task OnMyDataChanged(MyDataChangedInput myDataChangedInput)
    {
        await _webHookPublisher.PublishAsync(AppWebHookNames.OnDataChanged, myDataChangedInput);
    }
}
```

It will send webhooks to all subscriptions of the current tenant. If there is no subscription it does not do anything.

If you want to send webhook(s) to a specific tenant you can set tenant id as seen below

Copy

```csharp
 await _webHookPublisher.PublishAsync(AppWebHookNames.OnDataChanged, myDataChangedInput,5);//sends webhook(s) to subscriptions of the tenant whose id is 5

 await _webHookPublisher.PublishAsync(AppWebHookNames.TenantDeleted, tenantDeletedInput,null);//sends webhook(s) to subscriptions of host
```

As we mentioned before, users can define headers for their webhook subscription. But sometimes you may want to send webhooks with same headers or you may need to replace user's header or add a couple of headers etc. To do that you can use header parameter in Publish methods.

Copy

```csharp
await _webhookPublisher.PublishAsync(AppWebhookDefinitionNames.Users.Created, data,
  headers: new WebhookHeader()
  {
      UseOnlyGivenHeaders = false,
      Headers = new Dictionary<string, string>()
      {
          {"Key1", "Value1"},
          {"Key3", "Value3"}
      }
  };
);
```

Note:

- If subscription already has given header, publisher uses the one you give.
- If the **UseOnlyGivenHeaders** parameter is **true**, webhook will only contain given headers. If it is **false** given headers will be added to predefined headers in subscription (default is **false**).

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#configuration)

Webhook configurations:

| Parameter | Summary |
| --- | --- |
| TimeoutDuration(TimeSpan) | HttpClient timeout. WebhookSender will wait `TimeoutDuration` second before throw timeout exception. Then the webhook will be considered unsuccessful. |
| MaxSendAttemptCount(int) | Max send attempt count that IWebhookPublisher will try to resend webhook until it gets `HttpStatusCode.OK` |
| JsonSerializerSettings [(JsonSerializerSettings)](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm) | Json serializer settings for converting webhook data to json, If this is null default settings will be used.(JsonExtensions.ToJsonString(object,bool,bool))\* |
| Providers(type list of WebhookDefinitionProvider) | Webhook providers. |
| IsAutomaticSubscriptionDeactivationEnabled(bool) | If you enable that, subscriptions will be automatically disabled if they fails `MaxConsecutiveFailCountBeforeDeactivateSubscription` times consecutively.<br>Tenants should activate it back manually. |
| MaxConsecutiveFailCountBeforeDeactivateSubscription(int) | Max consecutive fail count to deactivate subscription if `IsAutomaticSubscriptionDeactivationEnabled` is true |

### Auto Subscription Deactivation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#auto-subscription-deactivation)

Webhook has built-in auto subscription deactivation features. Sometimes webhook endpoints can be unreachable for a long time, not to send webhooks to that endpoints again and again, you can enable auto subscription deactivation. After webhook attempts of subscription fail `MaxConsecutiveFailCountBeforeDeactivateSubscription` times, the subscription becomes inactive till tenant go and activate it back.

Copy

```csharp
public class AbpZeroTemplateCoreModule : AbpModule
{
    public override void PreInitialize()
    {
    	Configuration.Webhooks.IsAutomaticSubscriptionDeactivationEnabled = true;//default false
	Configuration.Webhooks.MaxConsecutiveFailCountBeforeDeactivateSubscription = 15;

        //if an endpoint fails 15 times consecutively, the subscription will be inactive.
        //default value of MaxConsecutiveFailCountBeforeDeactivateSubscription is MaxSendAttemptCount x 3
    }
    //...
}
```

### Tables [Anchor](https://aspnetboilerplate.com/Pages/Documents/Webhook-System\#tables)

**WebHookSubscriptionInfo:** Stores webhook subscriptions

**WebhookEvent:** Stores created webhook's data. For example when a new user created then webhook published. One **WebhookEvent** will be created then webhooks will be sent to all subscribed tenants with using that data.

**WebhookSendAttempt:** Table for store webhook send attempts. Each row stores an attempt to send a webhook of **WebhookEvent** to subscriptions of tenants with the webhook result.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe