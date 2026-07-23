# https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects

## Data Transfer Objects

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Data-Transfer-Objects.md)

In this document

**Data Transfer Objects** are used to transfer data between the
**Application Layer** and the **Presentation Layer**.

The Presentation Layer calls an [Application\\
Service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) method with a Data
Transfer Object ( **DTO**). The application service then uses these domain objects
to perform some specific business logic, and then finally returns a DTO back to the
presentation layer. Thus, the Presentation layer is completely isolated from the
Domain layer. In an ideally layered application, the presentation layer never
works with domain objects,
( [Repositories](https://aspnetboilerplate.com/Pages/Documents/Repositories), or
[Entities](https://aspnetboilerplate.com/Pages/Documents/Entities)...).

### The need for DTOs [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#the-need-for-dtos)

At first, creating a DTO class for each Application Service method can be seen as
tedious and time-consuming work. However, they can save your application if you
correctly use them. Why?

#### Abstraction of the domain layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#abstraction-of-the-domain-layer)

DTOs provide an efficient way of abstracting domain objects from the
presentation layer. In effect, your layers are correctly separated. If
you want to change the presentation layer completely, you can continue with
the existing application and domain layers. Alternatively, you can re-write
your domain layer, completely change the database schema, entities and O/RM
framework, all without changing the presentation layer. This, of course, is
as long as the contracts (method signatures and DTOs) of your application
services remain unchanged.

#### Data hiding [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#data-hiding)

Say you have a User entity with the properties Id, Name, EmailAddress
and Password. If the GetAllUsers() method of UserAppService returns a
List<User>, anyone can see the passwords of all your users, even if you do not
show it on the screen. It's not just about security, it's about data
hiding. Application services should return to the presentation layer what it
needs. Not more, not less.

#### Serialization & lazy load problems [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#serialization-lazy-load-problems)

When you return data (an object) to the presentation layer, it's most likely
serialized. For example, in an MVC method that returns
JSON, your object can be serialized to JSON and sent to the client.
Returning an Entity to the presentation layer can be problematic in that
regard. How?

In a real-world application, your entities will have references to each other.
The User entity can have a reference to it's Roles. If you want to
serialize User, its Roles are also serialized. The Role class may
have a List<Permission> and the Permission class can have a reference
to a PermissionGroup class and so on... Imagine all of these objects being
serialized at once. You could easily and accidentally serialize your whole database!
Also, if your objects have circular references, they can **not** be serialized.

What's the solution? Marking properties as NonSerialized? No, you can
not know when it should be serialized and when it shouldn't be. It may
be needed in one application service method, and not needed in other.
Returning safe, serializable, and specially designed DTOs is a good
choice in this situation.

Almost all O/RM frameworks support lazy-loading. It's a feature that loads
entities from the database when they're needed. Say a User class has a reference
to a Role class. When you get a User from the database, the Role property is not
filled. When you first read the Role property, it's loaded from the database.
So, if you return such an Entity to the presentation layer, it
will cause it to retrieve additional entities from the database. If a
serialization tool reads the entity, it reads all properties recursively
and again your whole database can be retrieved (if there are suitable,
related properties between entities).

More problems can arise if you use Entities in the presentation
layer. It's best not to reference the domain/business layer assembly in the
presentation layer.

### DTO conventions & validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#dto-conventions-validation)

ASP.NET Boilerplate strongly supports DTOs. It provides
conventional classes, interfaces, and standardizes DTO naming and usage
conventions. When you write your code as described here in the documentation,
ASP.NET Boilerplate will easily automate some common tasks.

#### Example [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#example)

Here's a prime example. Say that we want to develop an application
service method that is used to **search people** by name and then return a
**list of people**. In that case, we may have a **Person** [entity](https://aspnetboilerplate.com/Pages/Documents/Entities) as shown below:

Copy

```
public class Person : Entity
{
    public virtual string Name { get; set; }
    public virtual string EmailAddress { get; set; }
    public virtual string Password { get; set; }
}
```

We then define an interface for our [application\\
service](https://aspnetboilerplate.com/Pages/Documents/Application-Services):

Copy

```
public interface IPersonAppService : IApplicationService
{
    SearchPeopleOutput SearchPeople(SearchPeopleInput input);
}
```

ASP.NET Boilerplate suggests naming input/output parameters as
MethodName **Input** and MethodName **Output** and defining a separated
input and output DTO for every application service method. Even if your
method only takes and returns **one** parameter, it's better to create a DTO
class. In turn, your code will be more extensible. You can add more
properties later without changing the signature of your method and without
breaking existing client applications.

Your method can return **void** if there is no return value. It
will not break existing applications if you add a return value later. If
your method does not use any arguments, you do not have to define an
input DTO. Keep in mind that it maybe better to write an input DTO class if you
think that you'll add parameters in the future. This is up to you.

Here's how the input and output DTO classes are defined in this example:

Copy

```
public class SearchPeopleInput
{
    [StringLength(40, MinimumLength = 1)]
    public string SearchedName { get; set; }
}

public class SearchPeopleOutput
{
    public List<PersonDto> People { get; set; }
}

public class PersonDto : EntityDto
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
}
```

ASP.NET Boilerplate **automatically validates** the input before the execution
of a method. It's similar to ASP.NET MVC's model validation, but note that the
application service is not a Controller, it's a plain old C# class. ASP.NET
Boilerplate makes an interception and checks the input automatically. See the [DTO\\
validation](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects) document for more info.

**EntityDto** is a simple class that declares an **Id property**, since they
are common for most entities. It has a generic version if your entity's primary
key is not int. You don't have to use it, but it can be better to define an
Id property.

As you can see, **PersonDto** does not include a Password property since it's
not needed for the presentation layer. It can be dangerous to send
peoples' passwords to the presentation layer! If a JavaScript client
requested it, anyone can easily grab the passwords from the result.

Let's implement **IPersonAppService** before go further:

Copy

```
public class PersonAppService : IPersonAppService
{
    private readonly IPersonRepository _personRepository;

    public PersonAppService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public SearchPeopleOutput SearchPeople(SearchPeopleInput input)
    {
        //Get entities
        var peopleEntityList = _personRepository.GetAllList(person => person.Name.Contains(input.SearchedName));

        //Convert to DTOs
        var peopleDtoList = peopleEntityList
            .Select(person => new PersonDto
                                {
                                    Id = person.Id,
                                    Name = person.Name,
                                    EmailAddress = person.EmailAddress
                                }).ToList();

        return new SearchPeopleOutput { People = peopleDtoList };
    }
}
```

Here we get entities from the database, **convert** them to DTOs and then
return an output. Notice that we didn't validate the input? ASP.NET Boilerplate
**validate** s it. It even checks **if input parameter is null** and if so, an
exception is thrown. This saves us from writing guard clauses in every
method!

You're probably thinking, "Won't I have to write a whole bunch of code to
move data between the Person entity and the PersonDto object?"
It's really a tedious work! The Person entity could have
many more properties.

### Auto mapping between DTOs and entities [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#auto-mapping-between-dtos-and-entities)

Fortunately, there are some tools that make this very easy!
**[AutoMapper](http://automapper.org/)** is one of them. See the [AutoMapper\\
Integration](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping) document to learn how to use
it in ASP.NET Boilerplate.

### Helper interfaces and classes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects\#helper-interfaces-and-classes)

ASP.NET Boilerplate provides some helper interfaces that can be implemented to
standardize common DTO property names.

**ILimitedResultRequest** defines a **MaxResultCount** property. This way,
you can implement it in your input DTOs to standardize the limiting result set.

**IPagedResultRequest** extends **ILimitedResultRequest** by adding
**SkipCount**. Let's implement this interface in SearchPeopleInput
for paging:

Copy

```
public class SearchPeopleInput : IPagedResultRequest
{
    [StringLength(40, MinimumLength = 1)]
    public string SearchedName { get; set; }

    public int MaxResultCount { get; set; }
    public int SkipCount { get; set; }
}
```

As a result of a paged request, you can return an output DTO that
implements **IHasTotalCount**. Naming standardization helps us to create
re-usable code and conventions. Check out the interfaces and classes under the
**Abp.Application.Services.Dto** namespace for more info.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |