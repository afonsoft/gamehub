# https://aspnetboilerplate.com/Pages/Documents/Navigation

## Navigation

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Navigation.md)

In this document

Every web application has some kind of menu to navigate between pages/screens.
ASP.NET Boilerplate provides a common infrastructure to create and show
a menu to users.

### Creating Menus [Anchor](https://aspnetboilerplate.com/Pages/Documents/Navigation\#creating-menus)

An application may consist of different
[modules](https://aspnetboilerplate.com/Pages/Documents/Module-System) and each module may have it's
own menu items. To define menu items, we need to create a class derived
from the **NavigationProvider**.

Imagine that we have a main menu like the one shown below:

- Tasks
- Reports
- Administration
  - User management
  - Role management

Here, the Administration menu item has two **sub menu items**. Here's a
navigation provider class to create such a menu:

Copy

```
public class SimpleTaskSystemNavigationProvider : NavigationProvider
{
    public override void SetNavigation(INavigationProviderContext context)
    {
        context.Manager.MainMenu
            .AddItem(
                new MenuItemDefinition(
                    "Tasks",
                    new LocalizableString("Tasks", "SimpleTaskSystem"),
                    url: "/Tasks",
                    icon: "fa fa-tasks"
                    )
            ).AddItem(
                new MenuItemDefinition(
                    "Reports",
                    new LocalizableString("Reports", "SimpleTaskSystem"),
                    url: "/Reports",
                    icon: "fa fa-bar-chart"
                    )
            ).AddItem(
                new MenuItemDefinition(
                    "Administration",
                    new LocalizableString("Administration", "SimpleTaskSystem"),
                    icon: "fa fa-cogs"
                    ).AddItem(
                        new MenuItemDefinition(
                            "UserManagement",
                            new LocalizableString("UserManagement", "SimpleTaskSystem"),
                            url: "/Administration/Users",
                            icon: "fa fa-users",
                            requiredPermissionName: "SimpleTaskSystem.Permissions.UserManagement"
                            )
                    ).AddItem(
                        new MenuItemDefinition(
                            "RoleManagement",
                            new LocalizableString("RoleManagement", "SimpleTaskSystem"),
                            url: "/Administration/Roles",
                            icon: "fa fa-star",
                            requiredPermissionName: "SimpleTaskSystem.Permissions.RoleManagement"
                            )
                    )
            );
    }
}
```

A MenuItemDefinition can basically have a unique **name**, a localizable
**display name**, an **url** and an **icon**. Also:

- A menu item may require a permission to show this menu to a
particular user (See the [authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization)
document). The **requiredPermissionName** property can be used in this
case.
- A menu item can be dependent on a
[feature](https://aspnetboilerplate.com/Pages/Documents/Feature-Management).
The **featureDependency** property can be used in this case.
- A menu item can define **customData** and the **order** in which it appears.

The **INavigationProviderContext** has methods to get existing menu items,
add menus, and edit menu items. This way, different modules can add their own items
to the menu.

There may be one or more menus in an application.
The **context.Manager.MainMenu** references the default main menu. We can
create and add more menus using the **context.Manager.Menus** property.

#### Registering Navigation Provider [Anchor](https://aspnetboilerplate.com/Pages/Documents/Navigation\#registering-navigation-provider)

After creating the navigation provider, we need to register it to ASP.NET
Boilerplate's configuration on the **PreInitialize** method of our
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System):

Copy

```
Configuration.Navigation.Providers.Add<SimpleTaskSystemNavigationProvider>();
```

### Showing Menu [Anchor](https://aspnetboilerplate.com/Pages/Documents/Navigation\#showing-menu)

The **IUserNavigationManager** can be
[injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and used to get menu
items and show them to the user. This way, we can create a menu on the server-side.

ASP.NET Boilerplate automatically generates a **JavaScript API** to get the
menu and items on the client-side. Methods and objects under the **abp.nav**
namespace can be used for this purpose. For instance,
**abp.nav.menus.MainMenu** can be used to get the main menu of the
application. This way, we can create a menu on the client-side.

The ASP.NET Boilerplate [templates](https://aspnetboilerplate.com/Templates) use this system to create
and show a menu to the user. Create a template and see the source code
for more info.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe