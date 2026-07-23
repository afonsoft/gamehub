# https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration

## EntityFramework Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/EntityFramework-Integration.md)

In this document

ASP.NET Boilerplate can work with any O/RM framework. It has built-in
integration with **EntityFramework**. This document will explain how to
use EntityFramework with ASP.NET Boilerplate. It's assumed that you're
already familar with EntityFramework at a basic level.

### NuGet Package [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#nuget-package)

The NuGet package to use EntityFramework as an O/RM in ASP.NET Boilerplate is
[Abp.EntityFramework](http://www.nuget.org/packages/Abp.EntityFramework).
You should add it to your application. It's better to implement
EntityFramework in a separated assembly (dll) in your application and
depend on that package from this assembly.

### DbContext [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#dbcontext)

As you know, to work with EntityFramework, you should define a
**DbContext** class for your application. An example DbContext is shown
below:

Copy

```
public class SimpleTaskSystemDbContext : AbpDbContext
{
    public virtual IDbSet<Person> People { get; set; }
    public virtual IDbSet<Task> Tasks { get; set; }

    public SimpleTaskSystemDbContext()
        : base("Default")
    {

    }

    public SimpleTaskSystemDbContext(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {

    }

    public SimpleTaskSystemDbContext(DbConnection existingConnection)
        : base(existingConnection, false)
    {

    }

    public SimpleTaskSystemDbContext(DbConnection existingConnection, bool contextOwnsConnection)
        : base(existingConnection, contextOwnsConnection)
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().ToTable("StsPeople");
        modelBuilder.Entity<Task>().ToTable("StsTasks").HasOptional(t => t.AssignedPerson);
    }
}
```

It's a regular DbContext class except with the following rules:

- It's derived from **AbpDbContext** instead of DbContext.
- It should have the constructors like the sample above (constructor
parameter names should also be the same). Explanation:
  - The **Default** constructor passes "Default" to the base class as the
    connection string. It expects a "Default" named connection
    string in the web.config/app.config file. This constructor is
    not used by ABP, but used by the EF command-line migration tool
    commands (like "update-database").
  - The constructor gets the **nameOrConnectionString** which is used by ABP
    to pass the connection name or string on runtime.
  - The constructor get the **existingConnection** which can be used for unit
    tests, and is not directly used by ABP.
  - The constructor gets the **existingConnection** and the
    **contextOwnsConnection** is used by ABP on single database/
    multiple dbcontext scenarios to share the same connection &
    transaction () when **DbContextEfTransactionStrategy** is used
    (see Transaction Management section below).

EntityFramework can map classes to database tables in a conventional
way. You don't even need to make a configuration unless you make some
custom stuff. In this example, we mapped entities to different tables.
By default, the Task entity maps to the **Tasks** table. We changed it to be
**StsTasks** table. Instead of configuring it with data annotation
attributes, it is recommended that you use fluent configuration. You can choose
what you like.

### Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#repositories)

Repositories are used to abstract data access from higher layers. See the
[repository documentation](https://aspnetboilerplate.com/Pages/Documents/Repositories) for more info.

#### Default Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#default-repositories)

The [Abp.EntityFramework](http://www.nuget.org/packages/Abp.EntityFramework)
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
uses the **Insert** method. This way, you can easily inject
**IRepository<TEntity>** (or IRepository<TEntity,
TPrimaryKey>) and use the predefined methods.

#### Custom Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#custom-repositories)

If standard repository methods are not sufficient, you can create custom
repository classes for your entities.

##### Application Specific Base Repository Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#application-specific-base-repository-class)

ASP.NET Boilerplate provides a base class, **EfRepositoryBase**, to
implement repositories easily. To implement the **IRepository** interface,
you can simply derive your repository from this class. However, it's better to
create your own base class that extends the EfRepositoryBase. Thus, you can
easily add shared/common methods to your repositories or override existing
methods. An example base class for all the repositories of a
_SimpleTaskSystem_ application:

Copy

```
//Base class for all repositories in my application
public class SimpleTaskSystemRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<SimpleTaskSystemDbContext, TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    public SimpleTaskSystemRepositoryBase(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    //add common methods for all repositories
}

//A shortcut for entities those have integer Id
public class SimpleTaskSystemRepositoryBase<TEntity> : SimpleTaskSystemRepositoryBase<TEntity, int>
    where TEntity : class, IEntity<int>
{
    public SimpleTaskSystemRepositoryBase(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    //do not add methods here, add them to the class above (because this class inherits it)
}
```

Notice that we're inheriting from
EfRepositoryBase< **SimpleTaskSystemDbContext**, TEntity, and
TPrimaryKey>? This sets ASP.NET Boilerplate to use the
SimpleTaskSystemDbContext in our repositories.

By default, all the repositories for your given DbContext
(SimpleTaskSystemDbContext in this example) are implemented using
EfRepositoryBase. You can replace it to your own base
repository class by adding the **AutoRepositoryTypes** attribute to your
DbContext as shown below:

Copy

```
[AutoRepositoryTypes(\
    typeof(IRepository<>),\
    typeof(IRepository<,>),\
    typeof(SimpleTaskSystemEfRepositoryBase<>),\
    typeof(SimpleTaskSystemEfRepositoryBase<,>)\
)]
public class SimpleTaskSystemDbContext : AbpDbContext
{
    ...
}
```

##### Custom Repository Example [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#custom-repository-example)

To implement a custom repository, simply derive it from your application
specific base repository class like the one we created above.

Assume that we have a Task entity that can be assigned to a Person
(entity) and a Task State (new, assigned, completed... and so on).
We may need to write a custom method to get the list of Tasks, with some
conditions, and with a pre-fetched (included) AssisgnedPerson property; All in a
single database query. See the example code:

Copy

```
public interface ITaskRepository : IRepository<Task, long>
{
    List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state);
}

public class TaskRepository : SimpleTaskSystemRepositoryBase<Task, long>, ITaskRepository
{
    public TaskRepository(IDbContextProvider<SimpleTaskSystemDbContext> dbContextProvider)
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
The **GetAll()** method returns an **IQueryable<Task>**, then we can added
some **Where** filters using the given parameters. Finally, we called
**ToList()** to get the list of Tasks.

You can also use the **Context** object in repository methods to reach
your DbContext, so that you can directly use the Entity Framework APIs.

**Note**: Define the custom repository **interface** in the
**domain/core** layer, **implement** it in the **EntityFramework**
project for layered applications. This way, you can inject the interface
from any project without actually referencing EF.

#### Repository Best Practices [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#repository-best-practices)

- **Use default repositories** wherever it's possible. You can use
the default repository even if you have a custom repository for an entity
(if you use standard repository methods).
- Always create a **repository base class** for your application for
custom repositories, as defined above.
- Define **interfaces** for your custom repositories in the **domain**
**layer** (.Core project in startup template), and custom repository
**classes** in the **.EntityFramework** project, if you want to abstract
EF from your domain/application.

### Transaction Management [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#transaction-management)

ASP.NET Boilerplate has a built-in [unit of work](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work)
system to manage database connection and transactions. Entity framework
has different [transaction management\\
approaches](https://learn.microsoft.com/en-us/ef/ef6/saving/transactions?redirectedfrom=MSDN).
ASP.NET Boilerplate uses the ambient TransactionScope approach by default,
but it also has a built-in implementation for the DbContext transaction API. If
you want to switch to the DbContext transaction API, you can configure it in the
PreInitialize method of your [module](https://aspnetboilerplate.com/Pages/Documents/Module-System) like this:

Copy

```
Configuration.ReplaceService<IEfTransactionStrategy, DbContextEfTransactionStrategy>(DependencyLifeStyle.Transient);
```

Remember to add " _using Abp.Configuration.Startup;_" to your code file
to be able to use the ReplaceService generic extension method.

In addition, your DbContext **should have constructors** as described in the
DbContext section of this document.

### Data Stores [Anchor](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration\#data-stores)

Since ASP.NET Boilerplate has built-in integration with EntityFramework,
it can work with the data stores EntityFramework supports. Our free startup
templates are designed to work with Sql Server but you can modify them
to work with a different data store.

For example, if you want to work with MySql, please refer to [this\\
document](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe