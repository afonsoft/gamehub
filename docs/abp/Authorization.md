# https://aspnetboilerplate.com/Pages/Documents/Authorization

## Authorization

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Authorization.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#introduction)

Almost all enterprise applications use authorization at some level.
Authorization is used to check if a user is allowed to perform some
specific operation in the application. ASP.NET Boilerplate defines a
**permission based** infrastructure to implement authorization.

#### About IPermissionChecker [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#about-ipermissionchecker)

The Authorization system uses **IPermissionChecker** to check permissions.
While you can implement it in your own way, it's fully implemented in the
**Module Zero** project. If it's not implemented, NullPermissionChecker
is used which grants all permissions to everyone.

### Defining Permissions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#defining-permissions)

A unique **permission** is defined for each operation that needs to be
authorized. We need to define a permission before it is used. ASP.NET
Boilerplate is designed to be [modular](https://aspnetboilerplate.com/Pages/Documents/Module-System),
so different modules can have different permissions. A module should
create a class derived from **AuthorizationProvider** in order to define
it's permissions. An example authorization provider is shown below:

Copy

```
public class MyAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        var administration = context.CreatePermission("Administration");

        var userManagement = administration.CreateChildPermission("Administration.UserManagement");
        userManagement.CreateChildPermission("Administration.UserManagement.CreateUser");

        var roleManagement = administration.CreateChildPermission("Administration.RoleManagement");
    }
}
```

**IPermissionDefinitionContext** has methods to get and create
permissions.

A permission is defined with these properties:

- **Name**: a system-wide **unique** name. It's a good idea to define a
const string for a permission name instead of a magic string. We
prefer to use . (dot) notation for hierarchical names but it's not
required. You can set any name you like. The only rule is that it must
be unique.
- **Display name**: A localizable string that can be used to show the
permission later in UI.
- **Description**: A localizable string that can be used to show the
definition of the permission later in UI.
- **MultiTenancySides**: For the multi-tenant application, a permission
can be used by tenants or the host. This is a **Flags** enumeration
and thus a permission can be used on both sides.
- **featureDependency**: Can be used to declare a dependency to
[features](https://aspnetboilerplate.com/Pages/Documents/Feature-Management). Thus, this
permission can be granted only if the feature dependency is satisfied.
It waits for an object implementing IFeatureDependency. The default
implementation is the SimpleFeatureDependency class. Example usage:
`new SimpleFeatureDependency("MyFeatureName")`

Permissions can have parent and child permissions. While this does
not affect permission checking, it helps to group the permissions in the UI.

After creating an authorization provider, we should register it in the
PreInitialize method of our module:

Copy

```
Configuration.Authorization.Providers.Add<MyAuthorizationProvider>();
```

Authorization providers are registered to [dependency\\
injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) automatically. An
authorization provider can inject any dependency (like a repository) to
build permission definitions using some other sources.

### Checking Permissions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#checking-permissions)

#### Using AbpAuthorize Attribute [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#using-abpauthorize-attribute)

The **AbpAuthorize** ( **AbpMvcAuthorize** for MVC Controllers and
**AbpApiAuthorize** for Web API Controllers) attribute is the easiest
and most common way of checking permissions. Consider the [application\\
service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) method shown below:

Copy

```
[AbpAuthorize("Administration.UserManagement.CreateUser")]
public void CreateUser(CreateUserInput input)
{
    //A user can not execute this method if he is not granted the "Administration.UserManagement.CreateUser" permission.
}
```

The CreateUser method can not be called by a user who is not granted the
permission " _Administration.UserManagement.CreateUser_".

The AbpAuthorize attribute also checks if the current user is logged in (using
[IAbpSession.UserId](https://aspnetboilerplate.com/Pages/Documents/Abp-Session)). If we declare
an AbpAuthorize for a method, it only checks for the login:

Copy

```
[AbpAuthorize]
public void SomeMethod(SomeMethodInput input)
{
    //A user can not execute this method if he did not login.
}
```

##### AbpAuthorize attribute notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#abpauthorize-attribute-notes)

ASP.NET Boilerplate uses the power of dynamic method interception for
authorization. There are some restrictions for the methods using the
AbpAuthorize attribute.

- It cannot be used for private methods.
- It cannot be used for static methods.
- You can not use it for methods of a non-injected class (We must use
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection)).

Also:

- You can use it for any **public** method if the method is called over an
**interface** (like Application Services used over interface).
- A method should be **virtual** if it's called directly from a class
reference (like ASP.NET MVC or Web API Controllers).
- A method should be **virtual** if it's **protected**.

**Note**: There are four types of authorize attributes:

- In an application service (application layer), we use the
**Abp.Authorization.AbpAuthorize** attribute.
- In an MVC controller (web layer), we use the
**Abp.Web.Mvc.Authorization.AbpMvcAuthorize** attribute.
- In ASP.NET Web API, we use the
**Abp.WebApi.Authorization.AbpApiAuthorize** attribute.
- In ASP.NET Core, we use the
**Abp.AspNetCore.Mvc.Authorization.AbpMvcAuthorize** attribute.

This difference comes from inheritance. In the application layer it's
completely ASP.NET Boilerplate's implementation and it does not extend any
class. For MVC and Web API, it inherits from the Authorize attributes
of those frameworks.

##### Suppress Authorization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#suppress-authorization)

You can disable authorization for a method/class by adding
**AbpAllowAnonymous** attribute to application services. Use the
**AllowAnonymous** attribute for MVC, Web API and ASP.NET Core Controllers, which
is a native attribute of these frameworks.

#### Using IPermissionChecker [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#using-ipermissionchecker)

While the AbpAuthorize attribute is good enough for most cases, there are
situations where we may want to check for a permission in a method's body. We can
inject and use **IPermissionChecker** for that as shown in the example
below:

Copy

```
public void CreateUser(CreateOrUpdateUserInput input)
{
    if (!PermissionChecker.IsGranted("Administration.UserManagement.CreateUser"))
    {
        throw new AbpAuthorizationException("You are not authorized to create user!");
    }

    //A user can not reach this point if he is not granted for "Administration.UserManagement.CreateUser" permission.
}
```

You can code any logic since **IsGranted** simply returns true
or false (It has an Async version, too). If you simply check a permission
and throw an exception as shown above, you can use the **Authorize**
method:

Copy

```
public void CreateUser(CreateOrUpdateUserInput input)
{
    PermissionChecker.Authorize("Administration.UserManagement.CreateUser");

    //A user can not reach this point if he is not granted for "Administration.UserManagement.CreateUser" permission.
}
```

Since authorization is widely used, **ApplicationService** and some
common base classes inject and define the PermissionChecker property. Thus,
permission checker can be used without injecting application service
classes.

#### In Razor Views [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#in-razor-views)

The base view class defines the IsGranted method to check if the current user has
permission. Thus, we can conditionally render the view. Example:

Copy

```
@if (IsGranted("Administration.UserManagement.CreateUser"))
{
    <button id="CreateNewUserButton" class="btn btn-primary"><i class="fa fa-plus"></i> @L("CreateNewUser")</button>
}
```

#### Client Side (JavaScript) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#client-side-javascript-)

In the client side, we can use the API defined in the **abp.auth** namespace. In
most cases, we need to check if the current user has a specific permission
(with permission name). Example:

Copy

```
abp.auth.isGranted('Administration.UserManagement.CreateUser');
```

You can also use **abp.auth.grantedPermissions** to get all granted
permissions or **abp.auth.allPermissions** to get all available
permission names in the application. Check **abp.auth** namespace on
runtime for others.

### Permission Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Authorization\#permission-manager)

We may need the definitions of permissions. **IPermissionManager** can be
[injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and used in this case.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |