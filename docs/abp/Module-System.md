# https://aspnetboilerplate.com/Pages/Documents/Module-System

## Module System

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Module-System.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#introduction)

ASP.NET Boilerplate provides the infrastructure to build modules and
compose them to create an application. A module can depend on another
module. Generally, an assembly is considered a module. If you create
an application with more than one assembly, it's recommended that you create a
module definition for each one.

The module system is currently focused on the server-side rather than client-side.

### Module Definition [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#module-definition)

A module is defined with a class that is derived from **AbpModule** that is in the [ABP package](https://www.nuget.org/packages/Abp). Say
that we're developing a Blog module that can be used in different
applications. The simplest module definition can be as shown below:

Copy

```
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

The Module definition class is responsible for registering its classes via
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection), if needed (it can be done
conventionally as shown above). It can also configure the application
and other modules, add new features to the application, and so on...

### Lifecycle Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#lifecycle-methods)

ASP.NET Boilerplate calls some specific methods of modules on
application startup and shutdown. You can override these methods to
perform some specific tasks.

ASP.NET Boilerplate calls these methods **ordered by dependencies**. If
module A depends on module B, module B is initialized before module A.

The exact order of startup methods: PreInitialize-B, PreInitialize-A,
Initialize-B, Initialize-A, PostInitialize-B and PostInitialize-A. This
is true for all dependency graphs. The **shutdown** method is also similar,
but in **reverse order**.

#### PreInitialize [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#preinitialize)

This method is called first, when the application starts. It's the go-to method
to [configure](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) the framework and other
modules before they initialize.

You can also write some specific code here to run before the dependency
injection registrations. For example, if you create a [conventional\\
registration](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) class, you should register it
here using the IocManager.AddConventionalRegisterer method.

#### Initialize [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#initialize)

This is the place where [dependency\\
injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) registration should be
done. It's generally done using the IocManager.RegisterAssemblyByConvention
method. If you want to define custom dependency registration, see the
[dependency injection documentation](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection).

#### PostInitialize [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#postinitialize)

This method is called last in the startup process. It's safe to resolve a
dependency here.

#### Shutdown [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#shutdown)

This method is called when the application shuts down.

### Module Dependencies [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#module-dependencies)

A module can be dependent on another. You need to **explicitly**
declare the dependencies using the **DependsOn** attribute, like below:

Copy

```
[DependsOn(typeof(MyBlogCoreModule))]
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

Here, we declare to ASP.NET Boilerplate that MyBlogApplicationModule
depends on the MyBlogCoreModule and the MyBlogCoreModule should be
initialized before the MyBlogApplicationModule.

ABP can resolve dependencies recursively beginning from the **startup**
**module** and initialize them accordingly. The startup module initializes as
the last module.

### PlugIn Modules [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#plugin-modules)

While modules are investigated beginning from the startup module and go
through the dependencies, ABP can also load modules **dynamically**.
The **AbpBootstrapper** class defines the **PlugInSources** property which can
be used to add sources to dynamically loaded [plugin modules](https://aspnetboilerplate.com/Pages/Documents/Plugin). A plugin
source can be any class implementing the **IPlugInSource** interface.
The **PlugInFolderSource** class implements it to get the plugin modules from
assemblies located in a folder.

#### ASP.NET Core [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#asp-net-core)

The ABP ASP.NET Core module defines options in the **AddAbp** extension method
to add plugin sources in the **Startup** class:

Copy

```
services.AddAbp<MyStartupModule>(options =>
{
    options.PlugInSources.Add(new FolderPlugInSource(@"C:\MyPlugIns"));
});
```

We could use the **AddFolder** extension method for a simpler syntax:

Copy

```
services.AddAbp<MyStartupModule>(options =>
{
    options.PlugInSources.AddFolder(@"C:\MyPlugIns");
});
```

See the [ASP.NET Core document](https://aspnetboilerplate.com/Pages/Documents/AspNet-Core) for more info on the Startup class.

#### ASP.NET MVC, Web API [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#asp-net-mvc-web-api)

For classic ASP.NET MVC applications, we can add plugin folders by
overriding the **Application\_Start** in the **global.asax** as shown below:

Copy

```
public class MvcApplication : AbpWebApplication<MyStartupModule>
{
    protected override void Application_Start(object sender, EventArgs e)
    {
        AbpBootstrapper.PlugInSources.AddFolder(@"C:\MyPlugIns");
        //...
        base.Application_Start(sender, e);
    }
}
```

##### Controllers in PlugIns [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#controllers-in-plugins)

If your modules include MVC or Web API Controllers,
ASP.NET cannot investigate your controllers. To overcome this issue,
you can change the global.asax file like below:

Copy

```
using System.Web;
using Abp.PlugIns;
using Abp.Web;
using MyDemoApp.Web;

[assembly: PreApplicationStartMethod(typeof(PreStarter), "Start")]

namespace MyDemoApp.Web
{
    public class MvcApplication : AbpWebApplication<MyStartupModule>
    {
    }

    public static class PreStarter
    {
        public static void Start()
        {
            //...
            MvcApplication.AbpBootstrapper.PlugInSources.AddFolder(@"C:\MyPlugIns\");
            MvcApplication.AbpBootstrapper.PlugInSources.AddToBuildManager();
        }
    }
}
```

### Additional Assemblies [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#additional-assemblies)

The default implementations for IAssemblyFinder and ITypeFinder (which is
used by ABP to investigate specific classes in the application) only
finds module assemblies and types in those assemblies. We can override the
**GetAdditionalAssemblies** method in our module to include additional
assemblies.

### Custom Module Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#custom-module-methods)

Your modules can also have custom methods that can be used by other
modules that depend on this module. Assume that MyModule2 depends on
MyModule1 and wants to call a method of MyModule1 in the PreInitialize method.

Copy

```
public class MyModule1 : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }

    public void MyModuleMethod1()
    {
        //this is a custom method of this module
    }
}

[DependsOn(typeof(MyModule1))]
public class MyModule2 : AbpModule
{
    private readonly MyModule1 _myModule1;

    public MyModule2(MyModule1 myModule1)
    {
        _myModule1 = myModule1;
    }

    public override void PreInitialize()
    {
        _myModule1.MyModuleMethod1(); //Call MyModule1's method
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

Here we constructor-injected MyModule1 to MyModule2, so MyModule2 can
call MyModule1's custom method. This is only possible if Module2 depends
on Module1.

### Module Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#module-configuration)

While custom module methods can be used to configure modules, we suggest
you use the [startup configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) system to
define and set the configuration for modules.

### Module Lifetime [Anchor](https://aspnetboilerplate.com/Pages/Documents/Module-System\#module-lifetime)

Module classes are automatically registered as a **singleton**.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe