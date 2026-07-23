# https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work

## Unit Of Work

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Unit-Of-Work.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#introduction)

Connection and transaction management is one of the most important
concepts in an application that uses a database. You need to know when to open a
connection, when to start a transaction, and how to dispose the connection,
and so on... ASP.NET Boilerplate manages database connections and
transactions by using its **unit of work** system.

### Connection & Transaction Management in ASP.NET Boilerplate [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#connection-transaction-management-in-asp-net-boilerplate)

ASP.NET Boilerplate **opens** a database connection (it may not be
opened immediately, but opened during the first database usage, based on the ORM
provider implementation) and begins a **transaction** when **entering**
a **unit of work method**. You can use the connection safely in this
method. At the end of the method, the transaction is **committed** and
the connection is **disposed**. If the method throws an **exception**, the
transaction is **rolled back**, and the connection is disposed. In this
way, a unit of work method is **atomic** (a **unit of work**). ASP.NET
Boilerplate does all of this automatically.

If a unit of work method calls another unit of work method, both use the
same connection & transaction. The first entered method manages the
connection & transaction and then the others reuse it.

The default **IsolationLevel** for a unit of work is **ReadUncommitted** if it is not configured. It can be easily configured using unit of work [options](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#options).

#### Conventional Unit Of Work Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#conventional-unit-of-work-methods)

Some methods are unit of work methods by default:

- All [MVC](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers), [Web API](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers)
and [ASP.NET Core MVC](https://aspnetboilerplate.com/Pages/Documents/AspNet-Core) Controller actions.
- All [Application Service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) methods.
- All [Repository](https://aspnetboilerplate.com/Pages/Documents/Repositories) methods.

Assume that we have an [application\\
service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) method like the one below:

Copy

```
public class PersonAppService : IPersonAppService
{
    private readonly IPersonRepository _personRepository;
    private readonly IStatisticsRepository _statisticsRepository;

    public PersonAppService(IPersonRepository personRepository, IStatisticsRepository statisticsRepository)
    {
        _personRepository = personRepository;
        _statisticsRepository = statisticsRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
        _personRepository.Insert(person);
        _statisticsRepository.IncrementPeopleCount();
    }
}
```

In the CreatePerson method, we're inserting a person using the person
repository and incrementing the total people count using the statistics
repository. Both of these repositories **share** the same connection and
transaction, since the application service method is a unit of
work by default. ASP.NET Boilerplate opens a database connection and
starts a transaction when entering the CreatePerson method, and if no exception is thrown, it
commits the transaction at the end of it. If an exception
is thrown, it rolls everything back. In this way, all the database operations in
the CreatePerson method become **atomic** (a **unit of work**).

In addition to the default, conventional unit of work classes, you can add
your own convention in the PreInitialize method of your
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System) like below:

Copy

```
Configuration.UnitOfWork.ConventionalUowSelectors.Add(type => ...);
```

You should check the type and return true if the type must be a
conventional unit of work class.

#### Controlling the Unit Of Work [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#controlling-the-unit-of-work)

The unit of work **implicitly** works for the methods defined above. In most
cases you don't have to control the unit of work manually for web
applications. You can **explicitly** use it if you want to control the unit
of work somewhere else. There are two approaches for it.

##### UnitOfWork Attribute [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#unitofwork-attribute)

The first and preferred approach is to use the **UnitOfWork** attribute. Example:

Copy

```
[UnitOfWork]
public void CreatePerson(CreatePersonInput input)
{
    var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };
    _personRepository.Insert(person);
    _statisticsRepository.IncrementPeopleCount();
}
```

This way, the CreatePerson method becomes a unit of work and manages the database
connection and transaction. Both repositories use the same unit of work.
Note that you do not need the UnitOfWork attribute if this is an application
service method. See the ' [unit of work method\\
restrictions](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#unitofwork-attribute-restrictions)' section.

There are options for the UnitOfWork attribute. See the 'unit of work in
detail' section. The UnitOfWork attribute can also be used on classes to
configure all the methods of them. The method attribute overrides the class
attribute if it exists.

##### IUnitOfWorkManager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#iunitofworkmanager)

The second approach is to use the **IUnitOfWorkManager.Begin(...)** method
as shown below:

Copy

```
public class MyService
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IPersonRepository _personRepository;
    private readonly IStatisticsRepository _statisticsRepository;

    public MyService(IUnitOfWorkManager unitOfWorkManager, IPersonRepository personRepository, IStatisticsRepository statisticsRepository)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _personRepository = personRepository;
        _statisticsRepository = statisticsRepository;
    }

    public void CreatePerson(CreatePersonInput input)
    {
        var person = new Person { Name = input.Name, EmailAddress = input.EmailAddress };

        using (var unitOfWork = _unitOfWorkManager.Begin())
        {
            _personRepository.Insert(person);
            _statisticsRepository.IncrementPeopleCount();

            unitOfWork.Complete();
        }
    }
}
```

You can inject and use IUnitOfWorkManager as shown here (Some base
classes already have **UnitOfWorkManager** injected by default: MVC
Controllers, [application services](https://aspnetboilerplate.com/Pages/Documents/Application-Services), [domain\\
services](https://aspnetboilerplate.com/Pages/Documents/Domain-Services)...). This way, you can create a **limited**
**scope** unit of work. Using this approach, you must call the **Complete**
method manually. If you don't call it, the transaction is rolled back and
the changes are not saved.

The **Begin** method has overloads that set the **unit of work options**. It's
simpler (and recommended) to use the **UnitOfWork** attribute if you don't otherwise
have a good reason.

### Unit Of Work in Detail [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#unit-of-work-in-detail)

#### Disabling Unit Of Work [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#disabling-unit-of-work)

You may want to disable the unit of work **for the conventional unit of work**
**methods** . To do that, use the UnitOfWorkAttribute's **IsDisabled**
property. Example usage:

Copy

```
[UnitOfWork(IsDisabled = true)]
public virtual void RemoveFriendship(RemoveFriendshipInput input)
{
    _friendshipRepository.Delete(input.Id);
}
```

Normally, you don't want to do this, but in some situations you may want
to disable the unit of work:

- You may want to use the unit of work in a limited scope with
the UnitOfWorkScope class as described above.

Note that if a unit of work method calls this RemoveFriendship method,
disabling this method is ignored, and it will use the same unit of work with
the caller method. So, disable carefully! The code above
works well since the repository methods are a unit of work by default.

#### Non-Transactional Unit Of Work [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#non-transactional-unit-of-work)

By its nature, a unit of work is transactional. ASP.NET Boilerplate starts,
commits or rolls back an explicit database-level transaction.
In some special cases, the transaction may cause problems since
it may lock some rows or tables in the database. In these situations, you
may want to disable the database-level transaction. The UnitOfWork attribute can
get a boolean value in its constructor to work as non-transactional.
Example usage:

Copy

```
[UnitOfWork(isTransactional: false)]
public GetTasksOutput GetTasks(GetTasksInput input)
{
    var tasks = _taskRepository.GetAllWithPeople(input.AssignedPersonId, input.State);
    return new GetTasksOutput
            {
                Tasks = Mapper.Map<List<TaskDto>>(tasks)
            };
}
```

We recommend you use this attribute as **\[UnitOfWork(isTransactional:**\
**false)\]**. It's more readable and explicit, but you could also use
\[UnitOfWork(false)\].

Note that ORM frameworks like NHibernate and EntityFramework
internally save changes in a single command. Assume that you updated a
few Entities in a non-transactional UOW. Even in this situation all the
updates are performed at end of the unit of work with a single database
command. If you execute an SQL query directly, it's performed
immediately and not rolled back if your UOW is not transactional.

There is a restriction for non-transactional UOWs. If you're already in
a transactional unit of work scope, setting isTransactional to false is
ignored (use the Transaction Scope Option to create a non-transactional unit
of work in a transactional unit of work).

Use a non-transactional unit of work carefully since most of the time things
should be transactional to ensure data integrity. If your method just reads
data and does not change it, it can be safely non-transactional.

#### A Unit Of Work Method Calls Another [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#a-unit-of-work-method-calls-another)

The unit of work is ambient. If a unit of work method calls another unit of
work method, they share the same connection and transaction. The first method
manages the connection and then the other methods reuse it.

#### Unit Of Work Scope [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#unit-of-work-scope)

You can create a different and isolated transaction in another
transaction or you can create a non-transactional scope in a transaction.
.NET defines
[TransactionScopeOption](https://learn.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption?view=net-8.0)
for that. You can set the Scope option of the unit of work to control it.

#### Automatically Saving Changes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#automatically-saving-changes)

If a method is a unit of work, ASP.NET Boilerplate automatically saves all the changes at
the end of the method. Assume that we need a method to
update the name of a person:

Copy

```
[UnitOfWork]
public void UpdateName(UpdateNameInput input)
{
    var person = _personRepository.Get(input.PersonId);
    person.Name = input.NewName;
}
```

That's all you have to do! The name was updated. We didn't even have to call
the \_personRepository.Update method. The ORM framework keeps track of all
the changes of entities in a unit of work and reflects these changes to the
database.

Note that we do not need to declare the UnitOfWork attribute for conventional unit of work
methods.

#### IRepository.GetAll() Method [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#irepository-getall-method)

When you call the GetAll() method of a repository, there must be an open
database connection since it returns IQueryable. This is needed because
of the deferred execution of IQueryable. It does not perform the database query
unless you call the ToList() method or use IQueryable in a foreach loop,
or somehow access the queried items. So when you call the ToList() method,
the database connection has to be alive.

Consider the example below:

Copy

```
[UnitOfWork]
public SearchPeopleOutput SearchPeople(SearchPeopleInput input)
{
    // get IQueryable<Person>
    var query = _personRepository.GetAll();

    // add some filters if selected
    if (!string.IsNullOrEmpty(input.SearchedName))
    {
        query = query.Where(person => person.Name.StartsWith(input.SearchedName));
    }

    if (input.IsActive.HasValue)
    {
        query = query.Where(person => person.IsActive == input.IsActive.Value);
    }

    // get paged result list
    var people = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

    return new SearchPeopleOutput { People = Mapper.Map<List<PersonDto>>(people) };
}
```

Here the SearchPeople method is a unit of work since the ToList() method of
IQueryable is called in the method body. The database connection must also be
open when IQueryable.ToList() is executed.

In most cases you will use the GetAll method safely in a web application,
since all the controller actions are a unit of work by default. This way, the
database connection is available during the entire request.

#### UnitOfWork Attribute Restrictions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#unitofwork-attribute-restrictions)

You can use the UnitOfWork attribute for:

- All **public** or **public virtual** methods for classes that are
used over an interface (Like an application service used over a service
interface).
- All **public virtual** methods for self-injected classes (Like **MVC**
**Controllers** and **Web API Controllers**).
- All **protected virtual** methods.

We recommended you always make the methods **virtual**. You can **not use**
**the attribute for private methods** because ASP.NET Boilerplate uses dynamic
proxying for that, and because private methods can not be seen from derived
classes. The UnitOfWork attribute (and any proxying) does not work if you
don't use [dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection)
and instantiate the class yourself.

### Options [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#options)

There are some options that can be used to change the behavior of a unit of
work.

First, we can change the default values of all the unit of works in the [startup\\
configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration). This is
generally done in the PreInitialize method of our
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System).

Copy

```
public class SimpleTaskSystemCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;
        Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
    }

    //...other module methods
}
```

As a second option, we can override the defaults for a particular unit of work. The **UnitOfWork** attribute
constructor and IUnitOfWorkManager. **Begin** method have overloads to set these options.

As a final option, you can use the startup configuration to configure the default unit
of work attributes for the ASP.NET MVC, Web API and ASP.NET Core MVC
Controllers (see their documentation for more info).

### Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#methods)

The UnitOfWork system works seamlessly and invisibly, but in some special
cases you may need to call its methods.

You can access the current unit of work in one of two ways:

- You can directly use the **CurrentUnitOfWork** property if your class is
derived from some specific base classes (ApplicationService,
DomainService, AbpController, AbpApiController... etc.)
- You can inject IUnitOfWorkManager into any class and use the
**IUnitOfWorkManager.Current** property.

#### SaveChanges [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#savechanges)

ASP.NET Boilerplate saves all changes at the end of a unit of work. You
don't have to do anything, but sometimes you may want to save changes
to the database in the middle of a unit of work operation. For example,
after saving some changes, we may want to get the Id of a newly inserted
[Entity](https://aspnetboilerplate.com/Pages/Documents/Entities) using
[EntityFramework](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration).

You can use the **SaveChanges** or **SaveChangesAsync** method of the current
unit of work.

Note that if the current unit of work is transactional, all changes in the
transaction are rolled back if an exception occurs. Even the saved changes!

### Events [Anchor](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work\#events)

A unit of work has the **Completed**, **Failed** and **Disposed** events.
You can register to these events and perform any operations you need. For
example, you may want to run some code when the current unit of work
successfully completes. Example:

Copy

```
public void CreateTask(CreateTaskInput input)
{
    var task = new Task { Description = input.Description };

    if (input.AssignedPersonId.HasValue)
    {
        task.AssignedPersonId = input.AssignedPersonId.Value;
        _unitOfWorkManager.Current.Completed += (sender, args) => { /* TODO: Send email to assigned person */ };
    }

    _taskRepository.Insert(task);
}
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe