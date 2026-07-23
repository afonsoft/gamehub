# https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration

## Dapper Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Dapper-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration\#introduction)

[Dapper](https://github.com/StackExchange/Dapper) is an
object-relational mapper (ORM) for .NET.
The [Abp.Dapper](https://www.nuget.org/packages/Abp.Dapper) package simply
integrates Dapper to ASP.NET Boilerplate. It works as a secondary ORM
provider along with EF 6.x, EF Core or NHibernate.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration\#installation)

Before you start, you need to install
[Abp.Dapper](https://www.nuget.org/packages/Abp.Dapper) and either EF
Core, EF 6.x or the NHibernate ORM NuGet packages in to the project you want
to use.

#### Module Registration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration\#module-registration)

First you need to add the **DependsOn** attribute for the **AbpDapperModule** on
your module where you register it:

Copy

```
[DependsOn(\
     typeof(AbpEntityFrameworkCoreModule),\
     typeof(AbpDapperModule)\
)]
public class MyModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(SampleApplicationModule).GetAssembly());
    }
}
```

**Note** that the AbpDapperModule dependency should be added later than the EF
Core dependency.

#### Entity to Table Mapping [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration\#entity-to-table-mapping)

You can configure mappings. For example, the **Person** class maps to the
**Persons** table in the following example:

Copy

```
public class PersonMapper : ClassMapper<Person>
{
    public PersonMapper()
    {
        Table("Persons");
        Map(x => x.Roles).Ignore();
        AutoMap();
    }
}
```

You should set the assemblies that contain mapper classes. Example:

Copy

```
[DependsOn(\
     typeof(AbpEntityFrameworkModule),\
     typeof(AbpDapperModule)\
)]
public class MyModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(SampleApplicationModule).GetAssembly());
        DapperExtensions.SetMappingAssemblies(new List<Assembly> { typeof(MyModule).GetAssembly() });
    }
}
```

### Usage [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dapper-Integration\#usage)

After registering **AbpDapperModule**, you can use the Generic
IDapperRepository interface (instead of standard IRepository) to inject
dapper repositories.

Copy

```
public class SomeApplicationService : ITransientDependency
{
    private readonly IDapperRepository<Person> _personDapperRepository;
    private readonly IRepository<Person> _personRepository;

    public SomeApplicationService(
        IRepository<Person> personRepository,
        IDapperRepository<Person> personDapperRepository)
    {
        _personRepository = personRepository;
        _personDapperRepository = personDapperRepository;
    }

    public void DoSomeStuff()
    {
        var people = _personDapperRepository.Query("select * from Persons");
    }
}
```

You can use both EF and Dapper repositories at the same
time and in the same transaction!

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe