# https://aspnetboilerplate.com/Pages/Documents/Abp-Session

## Abp Session

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Abp-Session.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#introduction)

ASP.NET Boilerplate provides an **IAbpSession** interface to obtain the current
user and tenant **without** using ASP.NET's Session. IAbpSession is also
fully integrated and used by other structures in ASP.NET Boilerplate such as the
[setting](https://aspnetboilerplate.com/Pages/Documents/Setting-Management) and
[authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization) systems.

### Injecting Session [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#injecting-session)

IAbpSession is generally **[property\**\
**injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection#property-injection-pattern)**
to needed classes unless it's not possible to work without session
information. If we use property injection, we can use
**NullAbpSession.Instance** as a default value, as shown below:

Copy

```
public class MyClass : ITransientDependency
{
    public IAbpSession AbpSession { get; set; }

    public MyClass()
    {
        AbpSession = NullAbpSession.Instance;
    }

    public void MyMethod()
    {
        var currentUserId = AbpSession.UserId;
        //...
    }
}
```

Since authentication/authorization is an application layer task, it's
advisable to **use the IAbpSession in the application layer and upper layers**.
This is not generally done in the domain layer. **ApplicationService**,
**AbpController,** **AbpApiController** and some other base classes have
**AbpSession** already injected, so you can, for instance, directly use the
AbpSession property in an application service method.

### Session Properties [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#session-properties)

AbpSession defines a few key properties:

- **UserId**: Id of the current user or null if there is no current
user. It can not be null if the calling code is authorized.
- **TenantId**: Id of the current tenant or null if there is no
current tenant (in case of user has not logged in or he is a host
user).
- **ImpersonatorUserId**: Id of the impersonator user, if the current
session is impersonated by another user. It's null if this is not an
impersonated login.
- **ImpersonatorTenantId**: Id of the impersonator user's tenant, if
the current session is impersonated by another user. It's null if this
is not an impersonated login.
- **MultiTenancySide**: It may be Host or Tenant.

UserId and TenantId are **nullable**. There are also the non-nullable
**GetUserId()** and **GetTenantId()** methods. If you're sure there is a
current user, you can call GetUserId(). If the current user is null, this
method throws an exception. GetTenantId() also works in this way.

Impersonator properties are not as common as other properties and are
generally used for [audit logging](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging) purposes.

**ClaimsAbpSession**

ClaimsAbpSession is the **default implementation** of the IAbpSession
interface. It gets session properties (except MultiTenancySide, it's
calculated) from the claims of the current user's principal. For a cookie-based
form authentication, it gets the values from cookies. Thus, it's fully integrated
in to ASP.NET's authentication mechanism.

### Overriding Current Session Values [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#overriding-current-session-values)

In some specific cases, you may need to change/override session values
for a limited scope. In such cases, you can use the IAbpSession.Use method
as shown below:

Copy

```
public class MyService
{
    private readonly IAbpSession _session;

    public MyService(IAbpSession session)
    {
        _session = session;
    }

    public void Test()
    {
        using (_session.Use(42, null))
        {
            var tenantId = _session.TenantId; //42
            var userId = _session.UserId; //null
        }
    }
}
```

The Use method returns an IDisposable and it **must be disposed**. Once the
return value is disposed, Session values are **automatically restored**
the to previous values.

If you are using this method in a UniOfWork scope, you may need to call `SaveChanges` method of the current unit of work in the using statement which wraps `_session.Use`, otherwise IDisposable object might be disposed before the current unit of work completes and you may get a different value for the UserId and TenantId you intend to use.

Copy

```
public class MyService
{
    private readonly IAbpSession _session;

    public MyService(IAbpSession session)
    {
        _session = session;
    }

    [UnitOfWork()]
    public void Test()
    {
        using (_session.Use(42, null))
        {
            var tenantId = _session.TenantId; //42
            var userId = _session.UserId; //null
            CurrentUnitOfWork.SaveChanges();
        }
    }
}
```

#### Warning! [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#warning-)

Always use the Use method in a using block as shown above. Otherwise, you may
get unexpected session values. You can have nested Use blocks and they will
work as you expect.

### User Identifier [Anchor](https://aspnetboilerplate.com/Pages/Documents/Abp-Session\#user-identifier)

You can use **.ToUserIdentifier()** extension method to create a
UserIdentifier object from IAbpSession. Since UserIdentifier is used in
a lot of APIs, this will simplify the creation of a UserIdentifier object for the
current user.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe