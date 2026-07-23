# https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions

## Handling Exceptions

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Handling-Exceptions.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#introduction)

This document is for the ASP.NET MVC and Web API. If you're interested in
ASP.NET Core, see the [ASP.NET Core](https://aspnetboilerplate.com/Pages/Documents/AspNet-Core) documentation.

In a web application, exceptions are usually handled in MVC Controller
and Web API Controller actions. When an exception occurs,
the application user is informed about the error and with an optional reason.

If an error occurs in a regular HTTP request, an error page is shown.
If an error occurs in an AJAX request, the server sends the error information
to the client and the client then handles and shows it to the user.

Handling exceptions in all web requests is tedious, and hard to keep DRY.
ASP.NET Boilerplate **automates** this. You almost never need to
explicitly handle an exception. ASP.NET Boilerplate handles all
exceptions, logs them, and returns an appropriate and formatted response to
the client. It also handles these responses in the client and shows error
messages to the user.

### Enabling Error Handling [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#enabling-error-handling)

To enable error handling for ASP.NET MVC Controllers, **customErrors**
mode must be enabled for ASP.NET MVC applications.

Copy

```
<customErrors mode="On" />
```

It can also be ' **RemoteOnly**' if you do not want to handle errors on a
local computer, for instance. Note that this is only required for ASP.NET MVC
Controllers, and not for Web API Controllers.

If you are already handling exceptions in a global filter, it may
hide exceptions. Thus, ABP's exception handling may not work as you
expected. So if you do this, do it carefully!

### Non-Ajax Requests [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#non-ajax-requests)

If a request is not AJAX, an error page is shown.

#### Showing Exceptions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#showing-exceptions)

Imagine that there is an MVC controller action which throws an arbitrary
exception:

Copy

```
public ActionResult Index()
{
    throw new Exception("A sample exception message...");
}
```

Most likely, this exception would be thrown by another method that is called
from this action. ASP.NET Boilerplate handles this exception, logs it
and shows the ' **Error.cshtml**' view. You can **customize** this view to
show the error. Here's an **example** error view (the default Error view in the ASP.NET
Boilerplate templates):

![Default Error view](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/error-page-default.png)

ASP.NET Boilerplate hides the details of the exception from users and shows
a standard (and localizable) error message, unless you explicitly throw
a **UserFriendlyException**.

#### UserFriendlyException [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#userfriendlyexception)

The UserFriendlyException is a special type of exception that is directly
shown to the user. See the sample code below:

Copy

```
public ActionResult Index()
{
    throw new UserFriendlyException("Ooppps! There is a problem!", "You are trying to see a product that is deleted...");
}
```

ASP.NET Boilerplate logs it and does not hide the exception:

![User friendly exception](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/error-page-user-friendly.png)

If you want to show a special error message to users, just throw a
UserFriendlyException (or an exception derived from it).

#### Error Model [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#error-model)

ASP.NET Boilerplate passes an **ErrorViewModel** object as a model to the
Error view:

Copy

```
public class ErrorViewModel
{
    public AbpErrorInfo ErrorInfo { get; set; }

    public Exception Exception { get; set; }
}
```

**ErrorInfo** contains detailed information about the error that can be
shown to the user. The **Exception** object is the thrown exception. You can
check it and show additional information if you want. For example, we
can show validation errors if it's an **AbpValidationException**:

![Validation errors](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/error-page-validation.png)

### AJAX Requests [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#ajax-requests)

If the return type of an MVC action is a JsonResult (or Task<JsonResult for
async actions), ASP.NET Boilerplate returns a JSON object to the client
when exceptions occur. Sample return object for an error:

Copy

```
{
  "targetUrl": null,
  "result": null,
  "success": false,
  "error": {
    "message": "An internal error occurred during your request!",
    "details": "..."
  },
  "unAuthorizedRequest": false
}
```

**success: false** indicates that there is an error. The **error** object
provides the **message** and **details**.

When you use ASP.NET Boilerplate's infrastructure to make an AJAX request
on the client side, it automatically handles this JSON object and shows an
error message to the user using the [message API](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/Message).
See the [AJAX API](https://aspnetboilerplate.com/Pages/Documents/Javascript-API/AJAX) documentation for more
information.

### Exception Event [Anchor](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions\#exception-event)

When ASP.NET Boilerplare handles an exception, it triggers an
**AbpHandledExceptionData** event.
(See [eventbus documentation](https://aspnetboilerplate.com/Pages/Documents/EventBus-Domain-Events)
for more information about Event Bus). Example:

Copy

```
public class MyExceptionHandler : IEventHandler<AbpHandledExceptionData>, ITransientDependency
{
    public void HandleEvent(AbpHandledExceptionData eventData)
    {
        //TODO: Check eventData.Exception!
    }
}
```

If you put this example class into your application (generally into your
Web project), the **HandleEvent** method will be called for all exceptions
handled by ASP.NET Boilerplate. From there, you can investigate the Exception
object in detail.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe