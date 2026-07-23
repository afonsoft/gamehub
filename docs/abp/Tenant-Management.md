# https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management

## Tenant Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Tenant-Management.md)

In this document

### About Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management\#about-multi-tenancy)

We strongly recommend that you read the [multi-tenancy\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy) before this one.

### Enabling Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management\#enabling-multi-tenancy)

ASP.NET Boilerplate and Module Zero can run in **multi-tenant** or
**single-tenant** modes. Multi-tenancy is disabled by default. We can
enable it in the PreInitialize method of our
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System) as shown below:

Copy

```
[DependsOn(typeof(AbpZeroCoreModule))]
public class MyCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.MultiTenancy.IsEnabled = true;
    }

    ...
}
```

Note: Even if our application is not multi-tenant, we must define a
default tenant (see Default Tenant section of this document).

When we create a project [template](https://aspnetboilerplate.com/Templates) based on ASP.NET
Boilerplate and Module Zero, we have the **Tenant** entity and the
**TenantManager** domain service.

### Tenant Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management\#tenant-entity)

The Tenant entity represents a Tenant of the application.

Copy

```
public class Tenant : AbpTenant<Tenant, User>
{

}
```

It's derived from a generic **AbpTenant** class. Tenant entities are
stored in the **AbpTenants** table in the database. You can add your own custom
properties to the Tenant class.

The AbpTenant class defines some base properties, the most important are:

- **TenancyName**: This is the **unique** name of a tenant in the
application. It should not normally be changed. It can be used to
allocate subdomains to tenants like ' **mytenant**.mydomain.com'.
As such, it cannot contain spaces.
Tenant. **TenancyNameRegex** constant defines the naming rule.
- **Name**: An arbitrary, human-readable, long name of the tenant.
- **IsActive**: True, if this tenant can use the application. If it's
false, no user of this tenant can login to the application.

The AbpTenant class inherits **FullAuditedEntity**. This means it
has creation, modification and deletion **audit properties**. It is also
**[Soft-Delete](https://aspnetboilerplate.com/Pages/Documents/Data-Filters#isoftdelete)**, so
when we delete a tenant, it's not deleted from the database, just marked as
deleted.

Finally, the **Id** of AbpTenant is defined as an **int**.

### Tenant Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management\#tenant-manager)

The Tenant Manager is a service to perform the **domain logic** for tenants:

Copy

```
public class TenantManager : AbpTenantManager<Tenant, Role, User>
{
    public TenantManager(IRepository<Tenant> tenantRepository)
        : base(tenantRepository)
    {

    }
}
```

The TenantManager is also used to manage the tenant
[features](https://aspnetboilerplate.com/Pages/Documents/Feature-Management). You can add your own
methods here. You can also override any method of the AbpTenantManager base
class for your own needs.

### Default Tenant [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management\#default-tenant)

ASP.NET Boilerplate and Module Zero assume that there is a pre-defined
tenant where the TenancyName is ' **Default**' and the Id is **1**. In a
single-tenant application, this is used as the only tenant. In a
multi-tenant application, you can delete it or make it passive.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe