# https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration

## Startup Configuration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Startup-Configuration.md)

In this document

ASP.NET Boilerplate provides the infrastructure and a model to configure
its [modules](https://aspnetboilerplate.com/Pages/Documents/Module-System) on startup.

### Configuring ASP.NET Boilerplate [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#configuring-asp-net-boilerplate)

Configuring ASP.NET Boilerplate is made on the **PreInitialize** method of
your module. Example configuration:

Copy

```
public class SimpleTaskSystemModule : AbpModule
{
    public override void PreInitialize()
    {
        //Add languages for your application
        Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flag-england", true));
        Configuration.Localization.Languages.Add(new LanguageInfo("tr", "Türkçe", "famfamfam-flag-tr"));

        //Add a localization source
        Configuration.Localization.Sources.Add(
            new XmlLocalizationSource(
                "SimpleTaskSystem",
                HttpContext.Current.Server.MapPath("~/Localization/SimpleTaskSystem")
                )
            );

        //Configure navigation/menu
        Configuration.Navigation.Providers.Add<SimpleTaskSystemNavigationProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

ASP.NET Boilerplate is designed with **[modularity](https://aspnetboilerplate.com/Pages/Documents/Module-System)** in
mind. Different modules can configure ASP.NET Boilerplate. For example,
different modules can add navigation providers to add their own menu
items to the main menu. (See the
[localization](https://aspnetboilerplate.com/Pages/Documents/Localization) and
[navigation](https://aspnetboilerplate.com/Pages/Documents/Navigation) documents for details on
configuring them).

#### Replacing Built-In Services [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#replacing-built-in-services)

The **Configuration.ReplaceService** method can be used to **override** a
built-in service. For example, you can replace IAbpSession service with
your custom implementation as shown below:

Copy

```
Configuration.ReplaceService<IAbpSession, MySession>(DependencyLifeStyle.Transient);
```

The ReplaceService method has an overload to pass an **action** to make a
replacement in a custom way (you can directly use Castle Windsor with its
advanced registration API).

The same service can be replaced multiple times, especially in different
modules. The last one replaced will be the one that is valid. The module PreInitialize
methods are executed by the [dependency order](https://aspnetboilerplate.com/Pages/Documents/Module-System).

### Configuring Modules [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#configuring-modules)

Besides the framework's own startup configuration, a module can extend
the **IAbpModuleConfigurations** interface to provide configuration points
for the module. Example:

Copy

```
...
using Abp.Web.Configuration;
...
public override void PreInitialize()
{
    Configuration.Modules.AbpWebCommon().SendAllExceptionsToClients = true;
}
...
```

In this example, we configured the AbpWebCommon module to send all
exceptions to clients.

Not every module should define this type of configuration. It's
generally needed when a module will be re-usable in different
applications and needs to be configured on startup.

### Creating Configuration For a Module [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#creating-configuration-for-a-module)

Assume that we have a module named MyModule and it has some
configuration properties. First, we create a class for these configurable
properties:

Copy

```
public class MyModuleConfig
{
    public bool SampleConfig1 { get; set; }

    public string SampleConfig2 { get; set; }
}
```

We then register this class via [Dependency\\
Injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) on the **PreInitialize** method of
MyModule (Thus, it will be injectable):

Copy

```
IocManager.Register<MyModuleConfig>();
```

It should be registered as a **Singleton** like in this example. We can now
use the following code to configure MyModule in our module's
PreInitialize method:

Copy

```
Configuration.Get<MyModuleConfig>().SampleConfig1 = false;
```

#### Configuring using extension method [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#configuring-using-extension-method)

While we can use the IAbpStartupConfiguration.Get method as shown below, we
can create an extension method to the IModuleConfigurations like this:

Copy

```
public static class MyModuleConfigurationExtensions
{
    public static MyModuleConfig MyModule(this IModuleConfigurations moduleConfigurations)
    {
        return moduleConfigurations.AbpConfiguration.Get<MyModuleConfig>();
    }
}
```

Now other modules can configure this module using the extension method:

Copy

```
Configuration.Modules.MyModule().SampleConfig1 = false;
Configuration.Modules.MyModule().SampleConfig2 = "test";
```

This makes it easy to investigate module configurations and collect them in
a single place (Configuration.Modules...). ABP itself defines extension
methods for its own module configurations.

#### Configuring using injection [Anchor](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration\#configuring-using-injection)

At some point, MyModule needs this configuration. You can inject
MyModuleConfig and use the configured values. Example:

Copy

```
public class MyService : ITransientDependency
{
    private readonly MyModuleConfig _configuration;

    public MyService(MyModuleConfig configuration)
    {
        _configuration = configuration;
    }

    public void DoIt()
    {
        if (_configuration.SampleConfig2 == "test")
        {
            //...
        }
    }
}
```

This way, modules can create central configuration points in the ASP.NET
Boilerplate system.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe