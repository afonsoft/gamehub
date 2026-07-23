# https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration

## NHibernate Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/NHibernate-Integration.md)

In this document

ASP.NET Boilerplate can work with any O/RM framework. It has built-in
integration with **NHibernate**. This document will explain how to use
NHibernate with ASP.NET Boilerplate. It's assumed that you're already
familar with NHibernate at some basic level.

### NuGet package [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#nuget-package)

The NuGet package to use NHibernate as an O/RM in ASP.NET Boilerplate is
[Abp.NHibernate](http://www.nuget.org/packages/Abp.NHibernate). You
need to add it to your application. It's better to implement NHibernate
in a separated assembly (dll) in your application and depend on that
package from this assembly.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#configuration)

To start using NHibernate, you must configure it in the
[PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System) method of your module.

Copy

```
[DependsOn(typeof(AbpNHibernateModule))]
public class SimpleTaskSystemDataModule : AbpModule
{
    public override void PreInitialize()
    {
        var connStr = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        Configuration.Modules.AbpNHibernate().FluentConfiguration
            .Database(MsSqlConfiguration.MsSql2008.ConnectionString(connStr))
            .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()));
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

The **AbpNHibernateModule** module provides basic functionality and adapters
to make NHibernate work with ASP.NET Boilerplate.

#### Entity mapping [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#entity-mapping)

In this sample configuration above, we have fluently mapped using all the
mapping classes in the current assembly. An example mapping class can be as
follows:

Copy

```
public class TaskMap : EntityMap<Task>
{
    public TaskMap()
        : base("TeTasks")
    {
        References(x => x.AssignedUser).Column("AssignedUserId").LazyLoad();

        Map(x => x.Title).Not.Nullable();
        Map(x => x.Description).Nullable();
        Map(x => x.Priority).CustomType<TaskPriority>().Not.Nullable();
        Map(x => x.Privacy).CustomType<TaskPrivacy>().Not.Nullable();
        Map(x => x.State).CustomType<TaskState>().Not.Nullable();
    }
}
```

**EntityMap** is a class of ASP.NET Boilerplate that extends
**ClassMap<T>**, automatically maps the **Id** property and gets the
**table name** in the constructor. We're deriving from it and mapping
other properties using
[FluentNHibernate](http://www.fluentnhibernate.org/).  If you want, you can
derive directly from ClassMap. You can use the full API of
**FluentNHibernate** and you can use the other mapping techniques,
like mapping XML files.

### Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#repositories)

Repositories are used to abstract data access from higher layers. See the
[repository documentation](https://aspnetboilerplate.com/Pages/Documents/Repositories) for more info.

#### Default Implementation [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#default-implementation)

The [Abp.NHibernate](http://www.nuget.org/packages/Abp.NHibernate) NuGet package
implements default repositories for entities in your application. You
don't have to create repository classes for entities, just use the
predefined repository methods. Example:

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
TPrimaryKey>) and use the pre-defined methods. See the [repository\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Repositories) for a list of all the pre-defined
methods.

#### Custom Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#custom-repositories)

If you want to add a custom method, as a best practice, you must first
add it to a repository interface, then implement it in a
repository class. ASP.NET Boilerplate provides a base class
**NhRepositoryBase** to implement repositories easily. To implement the
IRepository interface, you can just derive your repository from this
class.

Assume that we have a Task entity that can be assigned to a Person
(entity) and the Task has a State (new, assigned, completed... and so on).
We may need to write a custom method to get the list of Tasks with some
conditions and with AssisgnedPerson property pre-fetched in a single
database query. See the example code:

Copy

```
public interface ITaskRepository : IRepository<Task, long>
{
    List<Task> GetAllWithPeople(int? assignedPersonId, TaskState? state);
}

public class TaskRepository : NhRepositoryBase<Task, long>, ITaskRepository
{
    public TaskRepository(ISessionProvider sessionProvider)
        : base(sessionProvider)
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
            .Fetch(task => task.AssignedPerson)
            .ToList();
    }
}
```

**GetAll()** returns **IQueryable<Task>**, then we can add some
**Where** filters using given parameters. Finally we can call
**ToList()** to get the list of Tasks.

You can also use the **Session** object in the repository methods to use the full
API of NHibernate.

**Note**: Define the custom repository **interface** in the
**domain/core** layer, **implement** it in the **NHibernate** project
for layered applications. This way, you can inject the interface from any
project without referencing NH.

##### Application Specific Base Repository Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration\#application-specific-base-repository-class)

Although you can derive your repositories from the NhRepositoryBase in
ASP.NET Boilerplate, it's better practice to create your own base
class that **extends** the NhRepositoryBase. This way, you can add shared/common
methods to your repositories easily. Example:

Copy

```
//Base class for all repositories in my application
public abstract class MyRepositoryBase<TEntity, TPrimaryKey> : NhRepositoryBase<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    protected MyRepositoryBase(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //add common methods for all repositories
}

//A shortcut for entities that have an integer Id.
public abstract class MyRepositoryBase<TEntity> : MyRepositoryBase<TEntity, int>
    where TEntity : class, IEntity<int>
{
    protected MyRepositoryBase(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //do not add any methods here, add the class above (since this inherits it)
}

public class TaskRepository : MyRepositoryBase<Task>, ITaskRepository
{
    public TaskRepository(ISessionProvider sessionProvider)
        : base(sessionProvider)
    {
    }

    //Specific methods for task repository
}
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe