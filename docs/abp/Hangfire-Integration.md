# https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration

## Hangfire Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Hangfire-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration\#introduction)

[Hangfire](http://hangfire.io/) is a compherensive background job
manager. You can **integrate** ASP.NET Boilerplate with Hangfire to use
it instead of the [default background job\\
manager](https://aspnetboilerplate.com/Pages/Documents/Background-Jobs-And-Workers). You can use the
**same background job API** for Hangfire. As such, your code will be
**independent** of Hangfire. If you like, you can directly use
**Hangfire's API**, too.

Hangfire Integration depends on the frameworks you are using.

### ASP.NET Core Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration\#asp-net-core-integration)

The [**Abp.HangFire.AspNetCore**](https://www.nuget.org/packages/Abp.HangFire.AspNetCore)
package is used to integrate to ASP.NET Core based applications. It
depends on
[Hangfire.AspNetCore](https://www.nuget.org/packages/Hangfire.AspNetCore/).
[This\\
document](https://www.hangfire.io/blog/2016/07/16/hangfire-1.6.0.html)
describes how to install hangfire to an ASP.NET Core project. It's similar
for ABP based projects too. First install the
[Abp.HangFire.AspNetCore](https://www.nuget.org/packages/Abp.HangFire.AspNetCore)
package to your web project:

Copy

```
Install-Package Abp.HangFire.AspNetCore
```

You can then install any storage for Hangfire. The most common one is SQL
Server (see the
[**Hangfire.SqlServer**](https://www.nuget.org/packages/Hangfire.SqlServer)
NuGet package). After you have installed these NuGet packages, you need to
**configure** your project to use Hangfire.

First, we change the Startup class to add Hangfire to dependency
injection, and then configure the storage and connection string in the
**ConfigureServices** method:

Copy

```
services.AddHangfire(config =>
{
    config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
});
```

We then add the UseHangfireServer call in the **Configure** method:

Copy

```
app.UseHangfireServer();
```

If you want to use hangfire's dashboard, you can add it, too:

Copy

```
app.UseHangfireDashboard();
```

If you want to [authorize](https://aspnetboilerplate.com/Pages/Documents/Authorization) the dashboard, you can
use AbpHangfireAuthorizationFilter as shown below:

Copy

```
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AbpHangfireAuthorizationFilter("MyHangFireDashboardPermissionName") }
});
```

The configuration above is the standard way to integrate hangfire to an
ASP.NET Core application. For ABP based projects, we should also
configure our web module to replace Hangfire for ABP's default
background job manager:

Copy

```
[DependsOn(typeof (AbpHangfireAspNetCoreModule))]
public class MyProjectWebModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.BackgroundJobs.UseHangfire();
    }

    //...
}
```

We added **AbpHangfireAspNetCoreModule** as a dependency and used the
Configuration.BackgroundJobs. **UseHangfire** method to replace Hangfire
for ABP's default background job manager.

Hangfire requires the schema creation permission in your database since it
creates its own schema and tables on first run. See the [Hangfire\\
documentation](http://docs.hangfire.io/en/latest/) for more information.

### ASP.NET MVC 5.x Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration\#asp-net-mvc-5-x-integration)

The [**Abp.HangFire**](https://www.nuget.org/packages/Abp.HangFire) NuGet
package is used for ASP.NET MVC 5.x projects:

Copy

```
Install-Package Abp.HangFire
```

You can then install any storage for Hangfire. The most common one is SQL
Server (see the
[**Hangfire.SqlServer**](https://www.nuget.org/packages/Hangfire.SqlServer)
NuGet package). After you have installed these NuGet packages, you can
**configure** your project to use Hangfire as shown below:

Copy

```
[DependsOn(typeof (AbpHangfireModule))]
public class MyProjectWebModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.BackgroundJobs.UseHangfire(configuration =>
        {
            configuration.GlobalConfiguration.UseSqlServerStorage("Default");
        });

    }

    //...
}
```

We added **AbpHangfireModule** as a dependency and used the
Configuration.BackgroundJobs. **UseHangfire** method to enable and
configure Hangfire ("Default" is the connection string in web.config).

Hangfire requires the schema creation permission in your database since it
creates its own schema and tables on first run. See the [Hangfire\\
documentation](http://docs.hangfire.io/en/latest/) for more information.

#### Dashboard Authorization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration\#dashboard-authorization)

Hangfire can show a **dashboard page** so you can see the status of all background
jobs in real time. You can configure it as described in its
[documentation](http://docs.hangfire.io/en/latest/configuration/using-dashboard.html).
By default, this dashboard page is available for all users, and is not
authorized. You can integrate it in to ABP's [authorization\\
system](https://aspnetboilerplate.com/Pages/Documents/Authorization) using the **AbpHangfireAuthorizationFilter**
class defined in the Abp.HangFire package. Example configuration:

Copy

```
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AbpHangfireAuthorizationFilter() }
});
```

This checks if the current user has logged in to the application. If you
want to require an additional permission, you can pass into its
constructor:

Copy

```
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AbpHangfireAuthorizationFilter("MyHangFireDashboardPermissionName") }
});
```

**Note**: UseHangfireDashboard should be called after the authentication
middleware in your Startup class (probably as the last line). Otherwise,
authorization will always fail.

#### Limitations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration\#limitations)

More than one background jobs in a single transaction isn't supported by Hangfire. Because, Hangfire does not participate the current transaction. It does not use the ambient transaction (TransactionScope).

It works with default background job manager since it simply performs a db command and it belongs to the current transaction as expected.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe