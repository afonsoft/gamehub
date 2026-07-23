# https://aspnetboilerplate.com/Pages/Documents/Repositories

## Repositories

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Repositories.md)

In this document

The repository pattern " _Mediates between the domain and data mapping layers using a_
_collection-like interface for accessing domain objects_" (Martin
Fowler).

Repositories, in practice, are used to perform database operations for
domain objects ( [Entity](https://aspnetboilerplate.com/Pages/Documents/Entities) and Value types).
Generally, a separate repository is used for each Entity (or Aggregate
Root).

### Default Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#default-repositories)

In ASP.NET Boilerplate, repository classes implement the
**IRepository<TEntity, TPrimaryKey>** interface. ABP can
automatically create default repositories for each entity type. You can
directly [inject](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) **IRepository<TEntity>** (or IRepository<TEntity,
TPrimaryKey>). An example [application\\
service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) uses a repository to
insert an entity into a database:

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
uses the **Insert** method.

### Custom Repositories [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#custom-repositories)

You only create a repository class for an entity when you need to create
custom repository methods for that entity.

#### Custom Repository Interface [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#custom-repository-interface)

A repository definition for a Person entity is shown below:

Copy

```
public interface IPersonRepository : IRepository<Person>
{

}
```

IPersonRepository extends **IRepository<TEntity>**. It's used to
define entities which have a primary key type of int (Int32). If your
entity's primary key is not an int, you can extend the
**IRepository<TEntity, TPrimaryKey>** interface as shown below:

Copy

```
public interface IPersonRepository : IRepository<Person, long>
{

}
```

#### Custom Repository Implementation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#custom-repository-implementation)

ASP.NET Boilerplate is designed to be independent from a particular ORM
(Object/Relational Mapping) framework or another technique to access a
database. Repositories are implemented in **NHibernate** and
**EntityFramework**, out-of-the-box. See the following documents to implement
repositories in ASP.NET Boilerplate on these frameworks:

- [NHibernate integration](https://aspnetboilerplate.com/Pages/Documents/NHibernate-Integration)
- [EntityFramework\\
integration](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration)

### Base Repository Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#base-repository-methods)

Every repository has some common methods coming from the
IRepository<TEntity> interface. We will investigate most of them
here.

#### Querying [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#querying)

##### Getting single entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#getting-single-entity)

Copy

```
TEntity Get(TPrimaryKey id);
Task<TEntity> GetAsync(TPrimaryKey id);
TEntity Single(Expression<Func<TEntity, bool>> predicate);
Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);
TEntity FirstOrDefault(TPrimaryKey id);
Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id);
TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
TEntity Load(TPrimaryKey id);
```

The **Get** method is used to get an Entity with a given primary key (Id). It
throws an exception if there is no entity in the database with the given Id.
The **Single** method is similar to Get but takes an expression rather than
an Id. This way, you can write a lambda expression to get an Entity. Example
usages:

Copy

```
var person = _personRepository.Get(42);
var person = _personRepository.Single(p => p.Name == "John");
```

Note that the **Single** method throws an exception if there is no entity
with the given conditions or where there is more than one entity.

Instead of throwing an exception, **FirstOrDefault** is similar but returns
**null** if there is no entity with a given Id or expression. It returns
the first found entity if there are more than one entity for the given
conditions.

**Load** does not retrieve the entity from the database but creates a proxy
object for lazy-loading. If you only use the Id property, the Entity is not
actually retrieved. It's retrieved from the database only if you access
other properties of the entity. This can be used instead of Get, for
performance reasons. It's implemented in **NHibernate**. If the ORM provider
does not implements it, the Load method works identically to the Get method.

##### Getting a list of entities [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#getting-a-list-of-entities)

Copy

```
List<TEntity> GetAllList();
Task<List<TEntity>> GetAllListAsync();
List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);
Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);
IQueryable<TEntity> GetAll();
IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
```

The **GetAllList** method is used to retrieve all entities from the database. An overload
of it can be used to filter entities. Examples:

Copy

```
var allPeople = _personRepository.GetAllList();
var somePeople = _personRepository.GetAllList(person => person.IsActive && person.Age > 42);
```

The **GetAll** method returns an IQueryable<T>. This way, you can add Linq methods
after it. Examples:

Copy

```
//Example 1
var query = from person in _personRepository.GetAll()
            where person.IsActive
            orderby person.Name
            select person;
var people = query.ToList();

//Example 2:
List<Person> personList2 = _personRepository.GetAll().Where(p => p.Name.Contains("H")).OrderBy(p => p.Name).Skip(40).Take(20).ToList();
```

When using GetAll, almost all queries can be written in Linq. It
can even be used in a join expression!

The **GetAllIncluding** allows to specify related data to be included in query results. In the following example, people will be retrieved with their Addresses property populated.

Copy

```
var allPeople = await _personRepository.GetAllIncluding(p => p.Addresses).ToListAsync();
```

#### About IQueryable<T> [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#about-iqueryable-t-)

When you call GetAll() out of a repository method, there must be an open
database connection. This is because of the deferred execution of
IQueryable<T>. It does not perform a database query unless you call the
ToList() method or use the IQueryable<T> in a foreach loop (or
somehow access the queried items). So when you call the ToList() method,
the database connection must be alive. For a web application, you don't need to
worry about that in most cases since the MVC controller methods are units of work
by default and the database connection is available for the entire request. See
the **[UnitOfWork](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work)** documentation to
understand it better.

##### Custom return value [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#custom-return-value)

There is also an additional method to provide the power of the IQueryable that
can be usable out of a unit of work.

Copy

```
T Query<T>(Func<IQueryable<TEntity>, T> queryMethod);
```

The Query method accepts a lambda (or method) that receives
IQueryable<T> and returns any type of object. Example:

Copy

```
var people = _personRepository.Query(q => q.Where(p => p.Name.Contains("H")).OrderBy(p => p.Name).ToList());
```

Since the given lamda (or method) is executed inside the repository method,
it's executed when the database connection is available. You can return a
list of entities, a single entity, or a projection or something else that
executes the query.

#### Insert [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#insert)

The IRepository interface defines methods to insert an entity to database:

Copy

```
TEntity Insert(TEntity entity);
Task<TEntity> InsertAsync(TEntity entity);
TPrimaryKey InsertAndGetId(TEntity entity);
Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity);
TEntity InsertOrUpdate(TEntity entity);
Task<TEntity> InsertOrUpdateAsync(TEntity entity);
TPrimaryKey InsertOrUpdateAndGetId(TEntity entity);
Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity);
```

The **Insert** method simply inserts new a entity in to a database and returns the
same inserted entity. The **InsertAndGetId** method returns the Id of a newly
inserted entity. This is useful if the Id is auto-increment and you need the Id
of the newly inserted entity. The **InsertOrUpdate** method inserts or updates a given
entity by checking its Id's value. Lastly, the **InsertOrUpdateAndGetId** method
returns the Id of the entity after inserting or updating it.

#### Update [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#update)

The IRepository interface defines methods to update an existing entity in the
database. It takes the entity to be updated and returns the same entity
object.

Copy

```
TEntity Update(TEntity entity);
Task<TEntity> UpdateAsync(TEntity entity);
```

Most of the time you don't need to explicitly call the Update methods since the
unit of work system automatically saves all changes when the unit of work
completes. See the [unit of work](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) documentation for more info.

#### Delete [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#delete)

The IRepository interface defines methods to delete an existing entity from the
database

Copy

```
void Delete(TEntity entity);
Task DeleteAsync(TEntity entity);
void Delete(TPrimaryKey id);
Task DeleteAsync(TPrimaryKey id);
void Delete(Expression<Func<TEntity, bool>> predicate);
Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
```

The first method accepts an existing entity, the second one accepts an Id of the
entity to delete. The last one accepts a condition to delete all
entities that fit a given condition. Note that all entities matching a given
predicate may be retrieved from the database and then deleted (based on
repository implementation). So use it carefully! It may cause
performance problems if there are too many entities with a given
condition.

#### Others [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#others)

The IRepository also provides methods to get the count of entities in a table.

Copy

```
int Count();
Task<int> CountAsync();
int Count(Expression<Func<TEntity, bool>> predicate);
Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
long LongCount();
Task<long> LongCountAsync();
long LongCount(Expression<Func<TEntity, bool>> predicate);
Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);
```

#### About Async Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#about-async-methods)

ASP.NET Boilerplate supports an async programming model. The repository
methods have async versions. Here's a sample [application\\
service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) method that uses the async
model:

Copy

```
public class PersonAppService : AbpWpfDemoAppServiceBase, IPersonAppService
{
    private readonly IRepository<Person> _personRepository;

    public PersonAppService(IRepository<Person> personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<GetPeopleOutput> GetAllPeople()
    {
        var people = await _personRepository.GetAllListAsync();

        return new GetPeopleOutput
        {
            People = Mapper.Map<List<PersonDto>>(people)
        };
    }
}
```

The GetAllPeople method is async and uses GetAllListAsync with the await
keyword.

Async may not be supported by all ORM frameworks. It's supported by
EntityFramework. If it's not supported, the Async repository methods work
synchronously. For example, InsertAsync works the same as Insert in
EntityFramework since EF does not write new entities to the database until the
unit of work completes (a.k.a. DbContext.SaveChanges).

### Managing Database Connections [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#managing-database-connections)

A database connection is not opened or closed in a repository method.
Connection management is made automatically by ASP.NET Boilerplate.

A database connection is **opened** and a **transaction** automatically begins while
entering a repository method. When the method ends and
returns, all changes are **saved**, the transaction is **committed** and
the database connection is **closed** by ASP.NET Boilerplate.
If your repository method throws any type of Exception, the transaction
is automatically **rolled back** and the database connection is closed. This
is true for all public methods of classes that implement the IRepository
interface.

If a repository method calls another repository method (even a method
of a different repository) they share the same connection and transaction.
The connection is managed (opened/closed) by the first method that enters a
repository. For more information on database connection management, see the
[UnitOfWork](https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work) documentation.

### Lifetime of a Repository [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#lifetime-of-a-repository)

All repository instances are **Transient**. This means they are
instantiated per-usage. See the [Dependency\\
Injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) documentation for more
information.

### Repository Best Practices [Anchor](https://aspnetboilerplate.com/Pages/Documents/Repositories\#repository-best-practices)

- For an entity of T, use IRepository<T> wherever it's possible.
Don't create custom repositories unless it's really needed.
The pre-defined repository methods will be enough for most cases.
- If you are creating a custom repository (by extending
IRepository<TEntity>);
  - Repository classes should be stateless. That means you must
    not define repository-level state objects and a repository
    method call should not effect another call.
  - Custom repository methods should not contain business logic or
    application logic. It should just perform data-related or
    orm-specific tasks.
  - While repositories can use dependency injection, define fewer or
    no dependencies to other services.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe