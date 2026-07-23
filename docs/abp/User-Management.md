# https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management

## User Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/User-Management.md)

In this document

### User Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#user-entity)

The User entity represents a **user of the application**. It should be
derived from the **AbpUser** class as shown below:

Copy

```
public class User : AbpUser<Tenant, User>
{
    //add your own user properties here
}
```

This class will be created when you download an ABP template with the option in the below image is selected.

![Login Page](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/include_module_zero_checkbox.png)

Users are
stored in the **AbpUsers** table in the database. You can add custom
properties to the User class (and create database migrations for the
changes).

The AbpUser class defines some base properties. Some of the properties are:

- **UserName**: Login name of the user. Should be **unique** for a
[tenant](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management).
- **EmailAddress**: Email address of the user. Should be **unique**
for a [tenant](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management).
- **Password**: Hashed password of the user.
- **IsActive**: True, if this user can login to the application.
- **Name** and **Surname** of the user.

There are also some properties like **Roles**, **Permissions**,
**Tenant**, **Settings**, **IsEmailConfirmed**, and so on. Check the AbpUser
class for more information.

The AbpUser class is inherited from **FullAuditedEntity**. That means it has
creation, modification and deletion audit properties. It's also implements
**[Soft-Delete](https://aspnetboilerplate.com/Pages/Documents/Data-Filters#isoftdelete)** , so
when we delete a user, it's not deleted from database, just marked as
deleted.

The AbpUser class implements the
[IMayHaveTenant](https://aspnetboilerplate.com/Pages/Documents/Data-Filters#imayhavetenant) filter
to properly work in a multi-tenant application.

Finally, the **Id** of the User is defined as **long**.

### User Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#user-manager)

**UserManager** is a service to perform **domain logic** for users:

Copy

```
public class UserManager : AbpUserManager<Tenant, Role, User>
{
    //...
}
```

You can [inject](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and use
UserManager to create, delete, update users, grant permissions, change
roles for users and much more. You can add your own methods here. Also,
you can **override** any method of the **AbpUserManager** base class for
your own needs.

#### Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#multi-tenancy)

_If you're not creating a multi-tenant application, you can skip this_
_section. See the [multi-tenancy documentation](https://aspnetboilerplate.com/Pages/Multi-Tenancy) for_
_more information about multi-tenancy._

UserManager is designed to work for a **single tenant** at a time. It
works with the **current tenant** by default. Let's see some usages of
the UserManager:

Copy

```
public class MyTestAppService : ApplicationService
{
    private readonly UserManager _userManager;

    public MyTestAppService(UserManager userManager)
    {
        _userManager = userManager;
    }

    public void TestMethod_1()
    {
        //Find a user by email for current tenant
        var user = _userManager.FindByEmail("sampleuser@aspnetboilerplate.com");
    }

    public void TestMethod_2()
    {
        //Switch to tenant 42
        CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, 42);

        //Find a user by email for tenant 42
        var user = _userManager.FindByEmail("sampleuser@aspnetboilerplate.com");
    }

    public void TestMethod_3()
    {
        //Disabling MayHaveTenant filter, so we can reach all users
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            //Now, we can search for a username in all tenants
            var users = _userManager.Users.Where(u => u.UserName == "sampleuser").ToList();

            //Or we can add TenantId filter if we want to search for a specific tenant
            var user = _userManager.Users.FirstOrDefault(u => u.TenantId == 42 && u.UserName == "sampleuser");
        }
    }
}
```

#### User Login [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#user-login)

Module Zero defines LoginManager which has a **LoginAsync** method used
for logging into the application. It checks all logic for the login and returns a
login result. The LoginAsync method also **automatically saves all login**
**attempts** to the database (even if it's a failed attempt). You can use the
**UserLoginAttempt** entity to query it.

#### About IdentityResults [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#about-identityresults)

Some methods of UserManager return IdentityResult as a result instead of
throwing exceptions for some cases. This is the nature of ASP.NET Identity
Framework. Module Zero also follows it, so we should check this
returning result object to know if the operation succeeded.

Module Zero defines the **CheckErrors** extension method that automatically
checks errors and throws an exception (a localized
[UserFriendlyException](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions#userfriendlyexception))
if needed. Example usage:

Copy

```
(await UserManager.CreateAsync(user)).CheckErrors();
```

To get localized exceptions, we must provide a
[ILocalizationManager](https://aspnetboilerplate.com/Pages/Documents/Localization) instance:

Copy

```
(await UserManager.CreateAsync(user)).CheckErrors(LocalizationManager);
```

### External Authentication [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#external-authentication)

The Login method of Module Zero authenticates a user from the **AbpUsers**
table in the database. Some applications may require you to authenticate
users from some external sources (like active directory, from another
database's tables, or even from a remote service).

For such cases, UserManager defines an extension point named 'external
authentication source'. We can create a class derived from
**IExternalAuthenticationSource** and register it to the configuration.
There is a **DefaultExternalAuthenticationSource** class to simplify
the implementation of IExternalAuthenticationSource. Let's see an example:

Copy

```
public class MyExternalAuthSource : DefaultExternalAuthenticationSource<Tenant, User>,  ITransientDependency
{
    public override string Name
    {
        get { return "MyCustomSource"; }
    }

    public override Task<bool> TryAuthenticateAsync(string userNameOrEmailAddress, string plainPassword, Tenant tenant)
    {
        //TODO: authenticate user and return true or false
    }
}
```

In the TryAuthenticateAsync method, we can check the username and password from
some source and return true if a given user is authenticated by it.
We can also override the CreateUser and UpdateUser methods to
control user creation and updating for this source.

When a user is authenticated by an external source, Module Zero checks if
this user exists in the database (AbpUsers table). If not, it calls
CreateUser to create the user, otherwise it calls UpdateUser to allow the
authentication source to update existing user information.

We can define more than one external authentication source in an
application. The AbpUser entity has an AuthenticationSource property that
shows which source authenticated this user.

To register our authenciation source, we can use some code like this in the
[PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System) method of our module:

Copy

```
Configuration.Modules.Zero().UserManagement.ExternalAuthenticationSources.Add<MyExternalAuthSource>();
```

#### LDAP/Active Directory [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#ldap-active-directory)

LdapAuthenticationSource is an implementation of external authentication
to make users login with their LDAP (active directory) username and
password.

If we want to use LDAP authentication, we must first add the
[Abp.Zero.Ldap](https://www.nuget.org/packages/Abp.Zero.Ldap) NuGet
package to our project (generally to the Core (domain) project). We then
must extend the **LdapAuthenticationSource** for our application as shown
below:

Copy

```
public class MyLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
{
    public MyLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
        : base(settings, ldapModuleConfig)
    {
    }
}
```

Lastly, we must set a module dependency to **AbpZeroLdapModule** and
**enable** LDAP with the auth source created above:

Copy

```
[DependsOn(typeof(AbpZeroLdapModule))]
public class MyApplicationCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Modules.ZeroLdap().Enable(typeof (MyLdapAuthenticationSource));
    }

    ...
}
```

After these steps, the LDAP module will be enabled for your application, but
LDAP auth is not enabled by default. We can enable it using the settings.

##### Settings [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#settings)

The **LdapSettingNames** class defines constants for setting names. You can
use these constant names while changing settings (or getting settings).
LDAP settings are **per-tenant** (for multi-tenant applications), so
different tenants have different settings (see the setting definitions on
[github](https://github.com/aspnetboilerplate/module-zero/blob/master/src/Abp.Zero.Ldap/Ldap/Configuration/LdapSettingProvider.cs)).

As you can see in the MyLdapAuthenticationSource **constructor**,
LdapAuthenticationSource expects **ILdapSettings** as a constructor
argument. This interface is used to get the LDAP settings like domain, user
name and password to connect to Active Directory. The default implementation
( **LdapSettings** class) gets these settings from the [setting\\
manager](https://aspnetboilerplate.com/Pages/Documents/Setting-Management).

If you work with Setting manager, then there's no problem. You can change the LDAP
settings using the [setting manager\\
API](https://aspnetboilerplate.com/Pages/Documents/Setting-Management). If you want, you can add some
initial seed data to the database to enable LDAP auth by default.

Note: If you don't define a domain, username and password, LDAP
authentication works for the current domain if your application runs in a
domain with appropriate privileges.

##### Custom Settings [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#custom-settings)

If you want to define another setting source, you can implement a custom
ILdapSettings class as shown below:

Copy

```
public class MyLdapSettings : ILdapSettings
{
    public async Task<bool> GetIsEnabled(int? tenantId)
    {
        return true;
    }

    public async Task<ContextType> GetContextType(int? tenantId)
    {
        return ContextType.Domain;
    }

    public async Task<string> GetContainer(int? tenantId)
    {
        return null;
    }

    public async Task<string> GetDomain(int? tenantId)
    {
        return null;
    }

    public async Task<string> GetUserName(int? tenantId)
    {
        return null;
    }

    public async Task<string> GetPassword(int? tenantId)
    {
        return null;
    }
}
```

Then register it to IOC in PreInitialize method of your module:

Copy

```
[DependsOn(typeof(AbpZeroLdapModule))]
public class MyApplicationCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        IocManager.Register<ILdapSettings, MyLdapSettings>(); //change default setting source
        Configuration.Modules.ZeroLdap().Enable(typeof (MyLdapAuthenticationSource));
    }

    ...
}
```

Then you can get the LDAP settings from another source.

#### Social Logins [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management\#social-logins)

See the [social authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)
document for more info about social logins.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe