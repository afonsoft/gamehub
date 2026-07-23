# https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers

## Web API Controllers

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Web-API-Controllers.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#introduction)

ASP.NET Boilerplate is integrated into the **ASP.NET Web API Controllers** via
the **Abp.Web.Api** NuGet package. You can create regular ASP.NET Web API
Controllers like you always do. [Dependency\\
Injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) properly works for
regular ApiControllers, but you should derive your controllers from
**AbpApiController**, which provides several benefits and integrates better
into ASP.NET Boilerplate.

### AbpApiController Base Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#abpapicontroller-base-class)

This is a simple api controller derived from **AbpApiController**:

Copy

```
public class UsersController : AbpApiController
{

}
```

#### Localization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#localization)

AbpApiController defines an **L** method to make
[localization](https://aspnetboilerplate.com/Pages/Documents/Localization) easier. Example:

Copy

```
public class UsersController : AbpApiController
{
    public UsersController()
    {
        LocalizationSourceName = "MySourceName";
    }

    public UserDto Get(long id)
    {
        var helloWorldText = L("HelloWorld");

        //...
    }
}
```

You must set the **LocalizationSourceName** property in order to make the **L** method work.
You can set this in your own base api controller class so you don't have to repeat it
for each api controller.

#### Others [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#others)

You can also use the pre-injected
[AbpSession](https://aspnetboilerplate.com/Pages/Documents/Abp-Session),
[EventBus](https://aspnetboilerplate.com/Pages/Documents/EventBus-Domain-Events), [PermissionManager,\\
PermissionChecker](https://aspnetboilerplate.com/Pages/Documents/Authorization),
[SettingManager](https://aspnetboilerplate.com/Pages/Documents/Setting-Management), [FeatureManager,\\
FeatureChecker](https://aspnetboilerplate.com/Pages/Documents/Feature-Management),
[LocalizationManager](https://aspnetboilerplate.com/Pages/Documents/Localization),
[Logger](https://aspnetboilerplate.com/Pages/Documents/Logging), and
[CurrentUnitOfWork](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) base properties (and
more).

### Filters [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#filters)

ABP defines some **pre-built filters** for the AspNet Web API. All of them
are added to **all actions of all controllers** by default.

#### Audit Logging [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#audit-logging)

The **AbpApiAuditFilter** is used to integrate into the [audit logging\\
system](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging). It logs all requests to all actions by
default (if auditing is not disabled). You can control audit logging
using the **Audited** and **DisableAuditing** attributes for actions and
controllers.

#### Authorization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#authorization)

You can use the **AbpApiAuthorize** attribute for your api controllers or
actions to prevent unauthorized users from using your controllers and
actions. Example:

Copy

```
public class UsersController : AbpApiController
{
    [AbpApiAuthorize("MyPermissionName")]
    public UserDto Get(long id)
    {
        //...
    }
}
```

You can define the **AllowAnonymous** attribute for actions or controllers
to suppress authentication/authorization. AbpApiController also defines
the **IsGranted** method as a shortcut to check the permissions in the code.

See the [authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization) documentation for
more info.

#### Anti Forgery Filter [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#anti-forgery-filter)

**AbpAntiForgeryApiFilter** is used to automatically protect the ASP.NET Web API
actions from CSRF/XSRF attacks (including the [dynamic web api](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Web-API)) (for POST,
PUT and DELETE requests). See the [CSRF\\
documentation](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection) for more info.

#### Unit Of Work [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#unit-of-work)

**AbpApiUowFilter** is used to integrate into the [Unit of\\
Work](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) system. It automatically begins a new unit of
work before an action execution and if no exception is thrown, completes the unit of work after
the action execution.

You can use the **UnitOfWork** attribute to control the behavior of the UOW for an
action. You can also use the startup configuration to change the default unit of
work attribute for all actions.

#### Result Wrapping & Exception Handling [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#result-wrapping-exception-handling)

ASP.NET Boilerplate **does not wrap** Web API actions **by default** if
an action has successfully executed. It, however, **handles and wraps**
**exceptions**. You can add the WrapResult/DontWrapResult attributes to actions and
controllers for finer control. You can change this default behavior from the
[startup configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) (using
Configuration.Modules.AbpWebApi()...). See the [AJAX\\
document](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/AJAX) for more info about result wrapping.

#### Result Caching [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#result-caching)

ASP.NET Boilerplate adds the Cache-Control header (no-cache, no-store) to
the response of Web API requests. This way, it prevents browser caching of
responses even for GET requests. This behavior can be disabled through the
configuration.

#### Validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#validation)

**AbpApiValidationFilter** automatically checks **ModelState.IsValid**
and prevents execution of the action if it's not valid. It also implements
input DTO validation as described in the [validation\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects).

### Model Binders [Anchor](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers\#model-binders)

**AbpApiDateTimeBinder** is used to normalize DateTime (and
Nullable<DateTime>) inputs using the **Clock.Normalize** method.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe