# https://aspnetboilerplate.com/Pages/Documents/Audit-Logging

## Audit Logging

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Audit-Logging.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging\#introduction)

Wikipedia: " _An audit trail (also called audit log) is a_
_security-relevant chronological record, set of records, and/or_
_destination and source of records that provide documentary evidence of_
_the sequence of activities that have affected at any time a specific_
_operation, procedure, or event_".

ASP.NET Boilerplate provides the infrastructure to automatically log all
interactions within the application. It can record intended method calls
with caller info and arguments.

Basically, the saved fields are: Related **tenant id**, caller **user id**,
called **service name** (the class of the called method), called
**method name**, execution **parameters** (serialized into JSON),
**execution time**, execution **duration** (in milliseconds), the client's
**IP address**, the client's **computer name** and the **exception** (if
the method throws an exception).

With this information, we not just know who did the operation, but we can also
measure the **performance** of the application and observe the
**exceptions** thrown. Furthermore, you can get **statistics** about the usage
of your application.

The auditing system uses [**IAbpSession**](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) to
get the current UserId and TenantId.

The Application Service, MVC Controller, Web API and ASP.NET Core methods
are automatically audited by default.

#### About IAuditingStore [Anchor](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging\#about-iauditingstore)

The auditing system uses **IAuditingStore** to
save audit information. While you can implement it in your own way,
it's fully implemented in the **Module Zero** project. If you don't
implement it, SimpleLogAuditingStore is used and it writes audit
information to the [log](https://aspnetboilerplate.com/Pages/Documents/Logging).

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging\#configuration)

To configure auditing, you can use the **Configuration.Auditing** property
in your [module](https://aspnetboilerplate.com/Pages/Documents/Module-System)'s PreInitialize method.
Auditing is **enabled by default**. You can disable it as shown below:

Copy

```
public class MyModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Auditing.IsEnabled = false;
    }

    //...
}
```

Here are the auditing configuration properties:

- **IsEnabled**: Used to enable/disable the auditing system completely.
Default: **true**.
- **IsEnabledForAnonymousUsers**: If this is set to true, audit logs
are saved for users that are not logged in to the system.
Default: **false**.
- **Selectors**: Used to select other classes to save audit logs.
- **SaveReturnValues**: Used to enable/disable to save return values. Default: **false**.
- **IgnoredTypes**: Used to ignore defined types.

**Selectors** is a list of predicates to select other types of classes that save
audit logs. A selector has a unique **name** and a **predicate**. The
only **default** selector in this list is used to select **application**
**service classes**. It's defined as shown below:

Copy

```
Configuration.Auditing.Selectors.Add(
    new NamedTypeSelector(
        "Abp.ApplicationServices",
        type => typeof (IApplicationService).IsAssignableFrom(type)
    )
);
```

You can add your selectors in your module's PreInitialize method.
You can also remove the selector above by name if you don't want to save
audit logs for application services. This is why it has a unique name
(Use simple LINQ to find the selector in Selectors and remove it if you
want).

Note: In addition to the standard audit configuration, MVC and ASP.NET Core
modules define configurations to enable/disable audit logging for
actions.

### Enable/Disable by attributes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging\#enable-disable-by-attributes)

While you can select auditing classes by configuration, you can use the
**Audited** and **DisableAuditing** attributes for a single **class** or an
individual **method**. Example:

Copy

```
[Audited]
public class MyClass
{
    public void MyMethod1(int a)
    {
        //...
    }

    [DisableAuditing]
    public void MyMethod2(string b)
    {
        //...
    }

    public void MyMethod3(int a, int b)
    {
        //...
    }
}
```

All methods of MyClass are audited except MyMethod2 since it's
explicitly disabled. The Audited attribute can be used to
save audits for the desired method.

**DisableAuditing** can also be used for a single **property of a**
**DTO**. Thus, you can **hide sensitive data** in audit logs, such as
passwords for example.

### Notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging\#notes)

- A method must be **public** in order to be saved in audit logs. Private
and protected methods are ignored.
- A method must be **virtual** if it's called over class reference.
This is not needed if it's injected using its interface (like
injecting the IPersonService interface to use the PersonService class). This
is needed since ASP.NET Boilerplate uses dynamic proxying and
interception. This is not true for **MVC** Controller actions. They
may not be virtual.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe