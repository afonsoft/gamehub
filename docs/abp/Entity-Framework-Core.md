# https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core

## Entity Framework Core

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Entity-Framework-Core.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#introduction)

The [Abp.EntityFrameworkCore](https://www.nuget.org/packages/Abp.EntityFrameworkCore)
NuGet package is used to integrate the Entity Framework (EF) Core ORM
framework. After installing this package, you need to also add a
[DependsOn](https://aspnetboilerplate.com/Pages/Documents/Module-System) attribute for
**AbpEntityFrameworkCoreModule**.

### DbContext [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#dbcontext)

EF Core requires you to define a class derived from DbContext. In ABP, we
need to derive from the **AbpDbContext** as shown below:

Copy

```
public class MyDbContext : AbpDbContext
{
    public DbSet<Product> Products { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
}
```

The constructor should get a **DbContextOptions<T>** as shown above.
The parameter name must be **options**. It's not possible to change it
because ABP provides it as an anonymous object parameter.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#configuration)

#### The Startup Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#the-startup-class)

Use the **AddAbpDbContext** method on the service collection in
the **ConfigureServices** method as shown below:

Copy

```
services.AddAbpDbContext<MyDbContext>(options =>
{
    options.DbContextOptions.UseSqlServer(options.ConnectionString);
});
```

For non web projects, we will not have a Startup class. In this case, we
can use the **Configuration.Modules.AbpEfCore().AddDbContext** method in our
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System) class to configure the DbContext, shown
below:

Copy

```
Configuration.Modules.AbpEfCore().AddDbContext<MyDbContext>(options =>
{
    options.DbContextOptions.UseSqlServer(options.ConnectionString);
});
```

We used the given connection string and Sql Server as the database
provider. Normally, **options.ConnectionString** is the **default connection**
**string** (see next section). However, ABP can use
IConnectionStringResolver to determine it. This behaviour can be
changed and the connection string can be determined dynamically. The action
passed to AddDbContext is called whenever a DbContext instance will be
created. You also have a chance to return different connection
strings, conditionally.

Where do we set the default connection string?

#### In the module PreInitialize method [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#in-the-module-preinitialize-method)

You can do it in the PreInitialize method of your module as shown below:

Copy

```
public class MyEfCoreAppModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = GetConnectionString("Default");
        ...
    }
}
```

You can define the GetConnectionString method, which simply returns the
connection string from a configuration file. This is generally in the
appsettings.json file.

### Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#repositories)

Repositories are used to abstract data access from higher layers. See the
[repository documentation](https://aspnetboilerplate.com/Pages/Documents/Repositories) for more info.

#### Default Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#default-repositories)

[Abp.EntityFrameworkCore](http://www.nuget.org/packages/Abp.EntityFrameworkCore)
implements default repositories for all the entities defined in your
DbContext. You don't have to create repository classes to use predefined
repository methods. Example:

Copy

```
public class PersonAppService : IPersonAppService
{
    private readonly IRepository<Person> _personRepository;

    public PersonAppService(IRepository<Person> personRepository)
    {
        _personRepository = personRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };

        _personRepository.Insert(person);
    }
}
```

The PersonAppService contructor-injects **IRepository<Person>** and
uses the **Insert** method. In this way, you can easily inject
**IRepository<TEntity>** (or IRepository<TEntity,
TPrimaryKey>) and use the predefined methods.

#### Custom Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#custom-repositories)

If standard repository methods are not sufficient, you can create custom
repository classes for your entities.

##### Application Specific Base Repository Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#application-specific-base-repository-class)

ASP.NET Boilerplate provides a base class **EfCoreRepositoryBase** to
implement repositories easily. To implement the **IRepository** interface,
you can simply derive your repository from this class. It's better to
create your own base class that extends EfRepositoryBase. This way, you can
easily add shared and common methods to your repositories. Here's an example base
class for all the repositories of a _Support_ application:

Copy

```
public interface ISupportRepository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    //A new custom method
    List<TEntity> GetActiveList();
}

public interface ISupportRepository<TEntity> : ISupportRepository<TEntity, int>, IRepository<TEntity>
    where TEntity : class, IEntity<int>
{

}

public class SupportRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<SupportDbContext, TEntity, TPrimaryKey>, ISupportRepository<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    public SupportRepositoryBase(IDbContextProvider<SupportDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }

    //A new custom method
    public List<TEntity> GetActiveList()
    {
        if (typeof(IPassivable).GetTypeInfo().IsAssignableFrom(typeof(TEntity)))
        {
            return GetAll()
                .Where(e => ((IPassivable)e).IsActive)
                .Cast<TEntity>()
                .ToList();
        }

        return GetAllList();
    }

    //An override of a default method
    public override int Count()
    {
        throw new Exception("can not get count!");
    }
}

public class SupportRepositoryBase<TEntity> : SupportRepositoryBase<TEntity, int>, ISupportRepository<TEntity>
    where TEntity : class, IEntity<int>
{
    public SupportRepositoryBase(IDbContextProvider<SupportDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }
}
```

Note that we're inheriting from
EfCoreRepositoryBase< **SupportDbContext**, TEntity,
TPrimaryKey>. This sets ASP.NET Boilerplate to use the
SupportDbContext in our repositories.

By default, all repositories for your given DbContext
(SupportDbContext in this example) are implemented using
EfCoreRepositoryBase. You can replace it with your own repository base
class by adding the **AutoRepositoryTypes** attribute to your
DbContext as shown below:

Copy

```
[AutoRepositoryTypes(\
    typeof(ISupportRepository<>),\
    typeof(ISupportRepository<,>),\
    typeof(SupportRepositoryBase<>),\
    typeof(SupportRepositoryBase<,>),\
    WithDefaultRepositoryInterfaces = true\
)]
public class SupportDbContext : AbpDbContext
{
    ...
}
```

##### Custom Repository Example [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#custom-repository-example)

To implement a custom repository, just derive it from your application
specific base repository class like the one we created above.

Assume that we have a Task entity that can be assigned to a Person
(entity). We also have a Task which has a State (new, assigned, completed... and so on).
We may need to write a custom method to get the list of Tasks with some
conditions and include the AssisgnedPerson property, pre-fetched (included), in a
single database query. See the following code:

Copy

```
public interface ITaskRepository : ISupportRepository<Task, long>
{
    List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state);
}

public class TaskRepository : SupportRepositoryBase<Task, long>, ITaskRepository
{
    public TaskRepository(IDbContextProvider<SupportDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state)
    {
        var query = GetAll();

        if (assignedPersonId.HasValue)
        {
            query = query.Where(task => task.AssignedPerson.Id == assignedPersonId.Value);
        }

        if (state.HasValue)
        {
            query = query.Where(task => task.State == state);
        }

        return query
            .OrderByDescending(task => task.CreationTime)
            .Include(task => task.AssignedPerson)
            .ToList();
    }
}
```

**We first defined** ITaskRepository and then implemented it.
The **GetAll()** method returns **IQueryable<Task>**. We then add some
**Where** filters using the given parameters. Finally, we call
**ToList()** to get the list of Tasks.

You can also use the **Context** object in the repository methods to reach
your DbContext and directly use Entity Framework APIs.

**Note**: Define the custom repository **interface** in the
**domain/core** layer, **implement** it in the **EntityFrameworkCore**
project for layered applications. That way, you can inject the interface
from any project without referencing EF Core.

##### Replacing the Default Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#replacing-the-default-repositories)

Even if you created a TaskRepository as shown above, any class can
still [inject](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) IRepository<Task, long>
and use it. That's not a problem in most cases. But what if you did an
**override** on a base method in your custom repository? Say that you have
overidden Delete method in your custom repository to add a custom
behaviour on delete. If a class injects IRepository<Task, long>
and usea the default repository to Delete a task, your custom behaviour
will not work. To overcome this issue, you can replace your custom
repository implementation with the default one like shown below:

Copy

```
Configuration.ReplaceService<IRepository<Task, Guid>>(() =>
{
    IocManager.IocContainer.Register(
        Component.For<IRepository<Task, Guid>, ITaskRepository, TaskRepository>()
            .ImplementedBy<TaskRepository>()
            .LifestyleTransient()
    );
});
```

We registered the TaskRepository for IRepository<Task, Guid>,
ITaskRepository and TaskRepository. This way, any one of these can be injected
to use the TaskRepository.

#### Batch Operations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#batch-operations)

ASP.NET Boilerplate is integrated with [Entity Framework Plus](https://github.com/zzzprojects/EntityFramework-Plus) to provide batch delete and update operations. In order to use those methods on your repositories, you need to add [Abp.EntityFrameworkCore.EFPlus](https://www.nuget.org/packages/Abp.EntityFrameworkCore.EFPlus) NuGet package to your project. Note that `BatchDeleteAsync` method deletes entities permanently even if the entity is [**soft-delete**](https://aspnetboilerplate.com/Pages/Documents/Entities#soft-delete).

#### Repository Best Practices [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#repository-best-practices)

- **Use the default repositories** wherever it's possible. You can use a
default repository even if you have a custom repository for an entity
(if you are using the standard repository methods).
- Always create the **repository base class** for your application for
custom repositories, as defined above.
- Define **interfaces** for your custom repositories in the **domain**
**layer** (.Core project in the startup template). Then define the custom repository
**classes** in the **.EntityFrameworkCore** project if you want to
abstract EF Core from your domain/application.

### Notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#notes)

- When using Lazy Loading, it is suggested to use a protected constructor for Entities instead of a private constructor.

### Other Database Integrations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core\#other-database-integrations)

This document and the examples are based on using MS SQL Server.
The following documents can be followed for different database integrations.

- [MySQL Integration](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration)
- [PostgreSQL Integration](https://aspnetboilerplate.com/Pages/Documents/EF-Core-PostgreSql-Integration)
- [SQLite Integration](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration)
- [Oracle Integration](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration)

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |