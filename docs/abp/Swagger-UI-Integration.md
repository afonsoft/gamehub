# https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration

## Swagger UI Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Swagger-UI-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#introduction)

From it's web site: "....with a Swagger-enabled API, you get
**interactive documentation**, client SDK generation and
discoverability."

### ASP.NET Core [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#asp-net-core)

#### Install NuGet Package [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#install-nuget-package)

Install the
**[Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore/)**
NuGet package to your **Web** project.

#### Configure [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#configure)

Add the following configuration code for Swagger into the **ConfigureServices** method of
your **Startup.cs**

Copy

```
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    //your other code...

      services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AbpZeroTemplate API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
            });

    //your other code...
}
```

Then, to use swagger, add this code into the **Configure** method of **Startup.cs**

Copy

```
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    //your other code...

     app.UseSwagger();
            //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AbpZeroTemplate API V1");
            }); //URL: /swagger

    //your other code...
}
```

#### Test [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#test)

That's it! You can now browse the swagger ui under " **/swagger**".

### ASP.NET 5.x [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#asp-net-5-x)

#### Install NuGet Package [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#install-nuget-package-1)

Install the
**[Swashbuckle.Core](https://www.nuget.org/packages/Swashbuckle.Core/)**
NuGet package to your **WebApi** project (or Web project).

#### Configure [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#configure-1)

Add the configuration code for Swagger into the
[Initialize](https://aspnetboilerplate.com/Pages/Documents/Module-System) method of your module.
Example:

Copy

```
public class SwaggerIntegrationDemoWebApiModule : AbpModule
{
    public override void Initialize()
    {
        //your other code...

        ConfigureSwaggerUi();
    }

    private void ConfigureSwaggerUi()
    {
        Configuration.Modules.AbpWebApi().HttpConfiguration
            .EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "SwaggerIntegrationDemo.WebApi");
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            })
            .EnableSwaggerUi(c =>
            {
                c.InjectJavaScript(Assembly.GetAssembly(typeof(AbpProjectNameWebApiModule)), "AbpCompanyName.AbpProjectName.Api.Scripts.Swagger-Custom.js");
            });
    }
}
```

Note that we inject a JavaScript file named " **Swagger-Custom.js**"
while configuring the swagger ui. This script file is used to add a CSRF token
to requests while testing api services in the ui. You may also need to
add this file to your WebApi project as an **embedded resource** and use
it's Logical Name in the InjectJavaScript method while injecting it.

**IMPORTANT**: The code above will be slightly different for your
project (Namespace will not be AbpCompanyName.AbpProjectName... and
AbpProjectNameWebApiModule will be _YourProjectName_ WebApiModule).

Here's the content of the **Swagger-Custom.js**:

Copy

```
var getCookieValue = function(key) {
    var equalities = document.cookie.split('; ');
    for (var i = 0; i < equalities.length; i++) {
        if (!equalities[i]) {
            continue;
        }

        var splitted = equalities[i].split('=');
        if (splitted.length !== 2) {
            continue;
        }

        if (decodeURIComponent(splitted[0]) === key) {
            return decodeURIComponent(splitted[1] || '');
        }
    }

    return null;
};

var csrfCookie = getCookieValue("XSRF-TOKEN");
var csrfCookieAuth = new SwaggerClient.ApiKeyAuthorization("X-XSRF-TOKEN", csrfCookie, "header");
swaggerUi.api.clientAuthorizations.add("X-XSRF-TOKEN", csrfCookieAuth);
```

See the Swashbuckle
[documentation](https://github.com/domaindrivendev/Swashbuckle) for more
configuration options.

#### Test [Anchor](https://aspnetboilerplate.com/Pages/Documents/Swagger-UI-Integration\#test-1)

That's it! Let's browse to **/swagger/ui/index**:

![Swagger UI](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/swagger-ui.png)

You can see all the Web API Controllers (and also the [dynamic web\\
api](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Web-API) controllers) and test them.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe