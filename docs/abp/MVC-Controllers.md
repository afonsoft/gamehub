# https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers

## MVC Controllers

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/MVC-Controllers.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#introduction)

ASP.NET Boilerplate is integrated in to the **ASP.NET MVC Controllers** via
the **Abp.Web.Mvc** NuGet package. You can create regular MVC Controllers as
you always do. [Dependency\\
Injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) properly works for
regular MVC Controllers, but you should derive your controllers from
**AbpController**, which provides several benefits and provides for better integration
in to ASP.NET Boilerplate.

### AbpController Base Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#abpcontroller-base-class)

This is a simple controller derived from AbpController:

Copy

```
public class HomeController : AbpController
{
    public ActionResult Index()
    {
        return View();
    }
}
```

#### Localization [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#localization)

AbpController defines **L** method to make
[localization](https://aspnetboilerplate.com/Pages/Documents/Localization) easier. Example:

Copy

```
public class HomeController : AbpController
{
    public HomeController()
    {
        LocalizationSourceName = "MySourceName";
    }

    public ActionResult Index()
    {
        var helloWorldText = L("HelloWorld");

        return View();
    }
}
```

You should set **LocalizationSourceName** to make the **L** method work.
You can set it in your own base controller class, so you don't have to repeat it for each
controller.

#### Others [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#others)

You can also use the pre-injected
[AbpSession](https://aspnetboilerplate.com/Pages/Documents/Abp-Session),
[EventBus](https://aspnetboilerplate.com/Pages/Documents/EventBus-Domain-Events), [PermissionManager,\\
PermissionChecker](https://aspnetboilerplate.com/Pages/Documents/Authorization),
[SettingManager](https://aspnetboilerplate.com/Pages/Documents/Setting-Management), [FeatureManager,\\
FeatureChecker](https://aspnetboilerplate.com/Pages/Documents/Feature-Management),
[LocalizationManager](https://aspnetboilerplate.com/Pages/Documents/Localization),
[Logger](https://aspnetboilerplate.com/Pages/Documents/Logging), and
[CurrentUnitOfWork](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) base properties and
more.

### Filters [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#filters)

#### Exception Handling & Result Wrapping [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#exception-handling-result-wrapping)

All exceptions are automatically handled, logged, and a proper response
is returned to the client. See the [exception\\
handling](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions) documentation for more.

ASP.NET Boilerplate also **wraps** action results by default if the return
type is **JsonResult** (or Task<JsonResult> for async actions).

You can change exception handling and wrapping by using the **WrapResult**
and **DontWrapResult** attributes for controllers or actions or from the
[startup configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) (using
Configuration.Modules.AbpMvc()...) globally. See the [ajax\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/AJAX) for more info.

##### WrapResultFilters [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#wrapresultfilters)

You can also implement a custom `IWrapResultFilter` and decide result wrapping conditionally by the current request URL. A custom result wrapping filter must be added to `WrapResultFilters` as shown below;

Copy

```c
Configuration.Modules.AbpWebCommon().WrapResultFilters.Add(new MyWrapResultFilter());
```

This approach can be useful if you don't have access the source code of the Controllers and can't used result wrapping attributes on the Controllers.

#### Audit Logging [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#audit-logging)

The **AbpMvcAuditFilter** is used to integrate to the [audit logging\\
system](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging). It logs all requests to all actions by
default (if auditing is not disabled). You can control audit logging
using the **Audited** and **DisableAuditing** attributes for actions and
controllers.

#### Validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#validation)

The **AbpMvcValidationFilter** automatically checks **ModelState.IsValid**
and prevents execution of the action if it's not valid. It also implements
input DTO validation described in the [validation\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects).

#### Authorization [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#authorization)

You can use **AbpMvcAuthorize** attribute for your controllers or
actions to prevent unauthorized users from using your controllers and
actions. Example:

Copy

```
public class HomeController : AbpController
{
    [AbpMvcAuthorize("MyPermissionName")]
    public ActionResult Index()
    {
        return View();
    }
}
```

You can define the **AllowAnonymous** attribute for actions or controllers
to suppress authentication/authorization. The AbpController also defines an
**IsGranted** method as a shortcut to check permissions.

See the [authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization) documentation for
more info.

#### Unit Of Work [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#unit-of-work)

The **AbpMvcUowFilter** is used to integrate to the [Unit of\\
Work](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) system. It automatically begins a new unit of
work before an action execution, and if no exception is thrown, completes the unit of work
after the action's execution.

You can use the **UnitOfWork** attribute to control the behaviour of the UOW for an
action. You can also use the startup configuration to change the default unit of
work attribute for all actions.

#### Anti Forgery [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#anti-forgery)

The **AbpAntiForgeryMvcFilter** is used to auto-protect MVC actions for
POST, PUT and DELETE requests from CSRF/XSRF attacks. See the [CSRF\\
documentation](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection) for more.

### Model Binders [Anchor](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers\#model-binders)

The **AbpMvcDateTimeBinder** is used to normalize DateTime (and
Nullable<DateTime>) inputs using the **Clock.Normalize** method.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe