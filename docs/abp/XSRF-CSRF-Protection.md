# https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection

## XSRF CSRF Protection

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/XSRF-CSRF-Protection.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#introduction)

" _**Cross-Site Request Forgery** (CSRF) is a type of attack that occurs_
_when a malicious web site, email, blog, instant message, or program_
_causes a user’s web browser to perform an unwanted action on a trusted_
_site for which the user is currently authenticated_"
( [OWASP](https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF)_Prevention_Cheat_Sheet)).

It's also briefly described
[here](https://www.asp.net/web-api/overview/security/preventing-cross-site-request-forgery-csrf-attacks)
where it explains how to implement it into ASP.NET Web API.

ABP framework **simplifies** and **automates** CSRF protection as much
as possible. The [startup templates](https://aspnetboilerplate.com/Templates) come with this
**pre-configured** and it works out-of-the-box. In this document, we will
explain how it's integrated into the ASP.NET platforms and how it works.

#### Http Verbs [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#http-verbs)

You don't normally need to protect the the **GET**, **HEAD**,
**OPTIONS** and **TRACE** action HTTP verbs since they normally are side-effect
free (they don't change the database). While ABP assumes this and
implements Anti Forgery protection for only the **POST**, **PUT, PATCH** and
**DELETE** verbs, you can change this behavior using the attributes
defined in this document.

#### Non-Browser Clients [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#non-browser-clients)

CSRF is a type of attack that is a problem for browsers because a
browser sends all cookies (including auth cookies) in all requests,
including cross-domain requests. This is not a problem for non-browser
clients, like mobile applications. The ABP framework understands the
difference and automatically **skips anti-forgery validation for non-**
**browser clients**.

### ASP.NET MVC [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#asp-net-mvc)

#### Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#features)

ASP.NET MVC has its own built-in AntiForgery system, but there are a few weaknesses:

- It requires you to add the **ValidateAntiForgeryToken** attribute to all
actions that need to be protected. You could potentially **forget** to add
it for all the needed actions!
- The ValidateAntiForgeryToken attribute only checks the
**\_\_RequestVerificationToken** in the HTML **form fields**. This
makes it very hard or impossible to use it for **AJAX** requests,
especially if you are sending " **application/json**" as the
content-type. In AJAX requests, it's common to set the token in the
**request header**.
- It's **hard to access** the verification token in **JavaScript**
(especially if you don't write your own JavaScript in .cshtml
files). We need to access it to use it in our **AJAX** requests.
- Even if we can access to the token in JavaScript, we must **manually**
**add** it to the header for every request.

ABP does following things to overcome these problems:

- You do not need to add the **ValidateAntiForgeryToken** attribute for **POST**,
**PUT, PATCH** and **DELETE** actions anymore, because they are
**automatically protected** (using **AbpAntiForgeryMvcFilter**).
Automatic protection will be enough for most cases. But you can
disable it for an action or controller using the
**DisableAbpAntiForgeryTokenValidation** attribute and you can
enable it for any action/controller using the
**ValidateAbpAntiForgeryToken** attribute.
- In addition to the HTML **form field**, **AbpAntiForgeryMvcFilter** also checks the token in the **header**.
This way, we can easily use anti-forgery token protections for AJAX requests.
- ABP provides the **abp.security.antiForgery.getToken()** function to get the
token in JavaScript, even if you don't need it often.
- ABP **Automatically** adds an anti-forgery token to the **header** for all
AJAX requests.

In this way, CSRF protection works almost seamlessly.

#### Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#integration)

The startup templates already integrate the CSRF protections out-of-the-box.
If you need to manually add it to your project (maybe you have a legacy project), follow this guide.

##### Layout View [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#layout-view)

We need to add the following code in our **Layout** view:

Copy

```
@{
    SetAntiForgeryCookie();
}
```

All pages that use this layout will include it. This method is defined
in the base ABP view class. It creates and sets the appropriate token cookies
and makes JavaScript do the side-work. If you have more than one layout, add
this to all of them.

That's all we have to do for ASP.NET MVC applications. All AJAX requests
will be protected automatically, but we should still use
the **@Html.AntiForgeryToken()** HTML helper for our **HTML forms** which
are **not posted via AJAX**. There is **no need** to add the
ValidateAbpAntiForgeryToken attribute for the corresponding action.

#### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#configuration)

XSRF protection is **enabled by default**. You can disable or configure
it in your [module](https://aspnetboilerplate.com/Pages/Documents/Module-System)'s PreInitialize method. Example:

Copy

```
Configuration.Modules.AbpWeb().AntiForgery.IsEnabled = false;
```

You can also configure token and cookie names using
_Configuration.Modules.AbpWebCommon().AntiForgery_ object.

### ASP.NET Web API [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#asp-net-web-api)

#### Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#features-1)

The ASP.NET Web API **does not** include an anti-forgery mechanism. However, ASP.NET
Boilerplate provides the infrastructure to add automated CSRF protection for ASP.NET
Web API Controllers.

#### Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#integration-1)

##### With ASP.NET MVC Clients [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#with-asp-net-mvc-clients)

If you are using the Web API inside an MVC project, **no additional**
**configuration is needed**. Even if you are self-hosting your Web API layer
in another process, no configuration is needed as long as you are making
AJAX requests from a configured MVC application.

##### With Other Clients [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#with-other-clients)

If your clients are different kinds of applications (say, an independent
Angular application which can not use the SetAntiForgeryCookie() method as
described above), then you should provide a way of setting the anti-
forgery token cookie. One possible way of doing this is to create an api
controller like the following:

Copy

```
using System.Net.Http;
using Abp.Web.Security.AntiForgery;
using Abp.WebApi.Controllers;

namespace AngularForgeryDemo.Controllers
{
    public class AntiForgeryController : AbpApiController
    {
        private readonly IAbpAntiForgeryManager _antiForgeryManager;

        public AntiForgeryController(IAbpAntiForgeryManager antiForgeryManager)
        {
            _antiForgeryManager = antiForgeryManager;
        }

        public HttpResponseMessage GetTokenCookie()
        {
            var response = new HttpResponseMessage();

            _antiForgeryManager.SetCookie(response.Headers);

            return response;
        }
    }
}
```

You can then call this action from the client to set the cookie.

### ASP.NET Core [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#asp-net-core)

#### Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#features-2)

**ASP.NET Core** MVC has a better [Anti\\
Forgery](https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-2.0)
mechanism compared to previous versions (ASP.NET MVC 5.x):

- It has the **AutoValidateAntiforgeryTokenAttribute** class that
automates anti-forgery validation for all **POST**, **PUT, PATCH**
and **DELETE** actions.
- It has the **ValidateAntiForgeryToken** and **IgnoreAntiforgeryToken**
attributes to control token validation.
- It automatically adds an anti-forgery security token to HTML forms if you
don't explicitly disable it. So there's no need to call
@Html.AntiForgeryToken() in most cases.
- It can read the request token from the HTTP **header** and the **form**
**field**.

ABP adds the following features:

- ABP **automatically** adds an anti-forgery token to the **header** for all
AJAX requests.
- It also provides an **abp.security.antiForgery.getToken()** function to get the
token in the JavaScript, even you will not need it much.

#### Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#integration-2)

The startup templates are already integrated to use CSRF protections out-of-the-box.
If you need to manually add it to your project (maybe you created your
project before we added it), follow this guide.

##### Startup Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#startup-class)

First, we must add the AutoValidateAntiforgeryTokenAttribute to the global
filters while adding MVC in the ConfigureServices method of the Startup class:

Copy

```
services.AddMvc(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
```

This way, all MVC actions (except GET, HEAD, OPTIONS and TRACE as declared
before) will be automatically validated for an anti-forgery token.

##### Layout View [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#layout-view-1)

We must add the following code in our **Layout** view:

Copy

```
@using Abp.Web.Security.AntiForgery
@inject IAbpAntiForgeryManager AbpAntiForgeryManager
@{
    AbpAntiForgeryManager.SetCookie(Context);
}
```

All the pages that use this layout will include it. It creates and sets
the appropriate token cookies and makes JavaScript do all the work. If you have
more than one layout, add this to all of them.

That's all we must do for ASP.NET Core MVC applications. All AJAX
requests will work automatically. For non-ajax form submits, ASP.NET
Core automatically adds an anti-forgery token field if you use one of
asp-\* tags in your form. So there's normally no need to use @Html.AntiForgeryToken().

* * *

Note: If you host your application in multiple instances and use redis, you should also use redis for antiforgery token.

- Add `Microsoft.AspNetCore.DataProtection.StackExchangeRedis` nuget package to your `Web` project.
- Add following code to `Startup.cs`

Copy

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(
            ConnectionMultiplexer.Connect(_appConfiguration["Abp:RedisCache:ConnectionString"])
            , "DataProtection-Keys"
        );
    ...
}
```

* * *

### Client Libraries [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#client-libraries)

The anti-forgery token must be provided in the request header for all AJAX
requests, as we declared above. We will see how it's done here.

#### jQuery [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#jquery)

The abp.jquery.js script defines an AJAX interceptor which adds the anti-forgery
token to the request header for every request. It gets the token from the
**abp.security.antiForgery.getToken()** JavaScript function.

#### AngularJS [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#angularjs)

AngularJS automatically adds the anti-forgery token to all AJAX requests.
See the _Cross Site Request Forgery (XSRF) Protection_ section in the AngularJS
[$http document](https://docs.angularjs.org/api/ng/service/$http). ABP
uses the same cookie and header names by default. So, Angular
integration works out of the box.

#### Other Libraries [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#other-libraries)

If you are using any other library for AJAX requests, you have three
options:

##### Intercept XMLHttpRequest [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#intercept-xmlhttprequest)

Since all libraries use JavaScript's native AJAX object,
XMLHttpRequest, you can define a simple interceptor to add the token to
the header:

Copy

```
(function (send) {
    XMLHttpRequest.prototype.send = function (data) {
        this.setRequestHeader(abp.security.antiForgery.tokenHeaderName, abp.security.antiForgery.getToken());
        return send.call(this, data);
    };
})(XMLHttpRequest.prototype.send);
```

##### Using the Library Interceptor [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#using-the-library-interceptor)

A good library provides interception points (like jQuery and AngularJS),
so follow your vendor's documentation to learn how to intercept
requests and manipulate headers.

##### Add the Header Manually [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#add-the-header-manually)

As a final option, you can use the abp.security.antiForgery.getToken() method to get
the token and add it to the request header manually for every request.
You probably do not need this and can solve this problem by using the methods described above.

### Internals [Anchor](https://aspnetboilerplate.com/Pages/Documents/XSRF-CSRF-Protection\#internals)

You may wonder "How does ABP handle this?". Actually, we use the same
mechanism described in the AngularJS documentation mentioned before. ABP
stores the token into a cookie (as described above) and sets the request
headers using that cookie. For validating it, it also integrates well into the
ASP.NET MVC, Web API and Core frameworks.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe