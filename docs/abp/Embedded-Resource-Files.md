# https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files

## Embedded Resource Files

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Embedded-Resource-Files.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#introduction)

ASP.NET Boilerplate provides an easy way of using embedded **Razor**
**views** (.cshtml files) and **other resources** (css, js, img... files)
in your web application. You can use this feature to create
[plugins/modules](https://aspnetboilerplate.com/Pages/Documents/Module-System) that contain UI functionality.

### Create the Embedded Files [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#create-the-embedded-files)

First, we create a file and mark it as an **embedded resource**. Any
assembly can contain embedded resource files. The process changes based
on your project format.

#### xproj/project.json Format [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#xproj-project-json-format)

Assume that we have a project named EmbeddedPlugIn, as shown below:

![Embedded resource sample project](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/embedded-resource-project-xproj.png)

To make **all files** under the **Views** folder embedded resources, we
add the following configuration to **project.json**:

Copy

```
  "buildOptions": {
    "embed": {
      "include": [\
        "Views/**/*.*"\
      ]
    }
  }
```

#### csproj Format [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#csproj-format)

Assume that we have a project named EmbeddedPlugIn, as shown below:

![Embedded resource project structure](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/embedded-resource-project-csproj.png)

Select the **Index.cshtml** file, go to the properties window (shorcut is F4)
and change it's **Build Action** to **Embedded Resource**.

![Embedding a file into a c# project](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/embedded-resource-sample-file-csproj.png)

You should change the build action to **embedded resource** for **all** the
files you want to use in a web application.

### Add To Embedded Resource Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#add-to-embedded-resource-manager)

Once we embed our files into the assembly, we can use the [startup\\
configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) to add them to the embedded
resource manager. You can add a line like this to the PreInitialize method of
your [module](https://aspnetboilerplate.com/Pages/Documents/Module-System):

Copy

```
Configuration.EmbeddedResources.Sources.Add(
    new EmbeddedResourceSet(
        "/Views/",
        Assembly.GetExecutingAssembly(),
        "EmbeddedPlugIn.Views"
    )
);
```

Let's explain the parameters:

- The first parameter defines the **root folder** for the files (like
http://yourdomain.com\*\*/Views/\*\*, here). It matches to the root
namespace.
- The second parameter defines the **Assembly** containing the files. This code
should be located in the assembly containing the embedded files.
Otherwise, you should change this parameter accordingly.
- The last parameter defines the **root namespace** of the files in the
assembly. This is the default namespace (generally, the assembly
name) plus the 'folders in the assembly' joined by a dot.

### Consume Embedded Views [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#consume-embedded-views)

For **.cshtml** files, it's straightforward to return them from a
Controller Action. BlogController in the EmbeddedPlugIn assembly is
shown below:

Copy

```
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EmbeddedPlugIn.Controllers
{
    public class BlogController : AbpController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
```

As you can see, it's the same as regular controllers and works just as expected.

### Consume Embedded Resources [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#consume-embedded-resources)

To consume embedded resources (js, css, img...), we can use them in
our views like we normally do:

Copy

```
@section Styles {
    <link href="~/Views/Blog/Index.css" rel="stylesheet" />
}

@section Scripts
{
    <script src="~/Views/Blog/Index.js"></script>
}

<h2 id="BlogTitle">Blog plugin!</h2>
```

It assumes that the main application has the Styles and Scripts sections. We
can also use other files, like images, like we normally do.

#### ASP.NET Core Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#asp-net-core-configuration)

ASP.NET MVC 5.x projects will automatically integrate to the embedded
resource manager through Owin (if your startup file contains
app.UseAbp() as expected). For ASP.NET Core projects, we have to manually
add **app.UseEmbeddedFiles()** to the Startup class, just after
app.UseStaticFiles(), as shown below:

Copy

```
app.UseStaticFiles();
app.UseEmbeddedFiles(); //Allows to expose embedded files to the web!
```

#### Ignored Files [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#ignored-files)

Normally, **all files** in the embedded resource manager can be directly
consumed by clients as if they were static files. You can ignore some
file extensions for security and other purposes. **.cshtml** and
**.config** files are ignored by default (for direct requests from
clients). You can add more extensions in the PreInitialize method of your
module as shown below:

Copy

```
Configuration.Modules.AbpWebCommon().EmbeddedResources.IgnoredFileExtensions.Add("exe");
```

### Override Embedded Files [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#override-embedded-files)

One important feature of embedded resource files is that **they can be**
**overridden** by higher modules. This means you can create a file with the
same name in the same folder in your web application to override an
embedded file (your file in the web application does not require it to be
an embedded resource, because static files have priority over embedded
files). Thus, you can override the css, js or view files of your
modules/plugins in the application. If module A depends on module
B and module A defines an embedded resource with the same path, it will
override the embedded resource file of module B.

Note: For ASP.NET Core projects, you should put the overriding files
in the wwwroot folder as the root path.

### Notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Embedded-Resource-Files\#notes)

Since the path is encoded using dots in embedded resources, the following paths and files will be the same:

Copy

```bash
/First/Second/Third/File.js
/First/Second/Third.File.js
/First/Second.Third.File.js
/First.Second.Third.File.js
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe