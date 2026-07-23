# https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping

## Object To Object Mapping

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Object-To-Object-Mapping.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#introduction)

It's common to map a similar object to another object. It's also
tedious and repetitive since generally both objects (classes) may have the
same/similar properties mapped to each other. Imagine a typical
[application service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) method below:

Copy

```
public class UserAppService : ApplicationService
{
    private readonly IRepository<User> _userRepository;

    public UserAppService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public void CreateUser(CreateUserInput input)
    {
        var user = new User
        {
            Name = input.Name,
            Surname = input.Surname,
            EmailAddress = input.EmailAddress,
            Password = input.Password
        };

        _userRepository.Insert(user);
    }
}
```

CreateUserInput is a simple [DTO](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects) class and User
is a simple [entity](https://aspnetboilerplate.com/Pages/Documents/Entities). We manually created a User
entity from the given input. The User entity will have more properties in a
real-world application and manually creating it will become tedious and
error-prone. We also have to change the mapping code when we want to add
new properties to User and CreateUserInput.

We can use a library to automatically handle our mappings.
[AutoMapper](http://automapper.org/) is one of the best libraries for
object to object mapping. ASP.NET Boilerplate defines an **IObjectMapper**
interface to abstract it and then implements this interface using AutoMapper
in the [Abp.AutoMapper](https://www.nuget.org/packages/Abp.AutoMapper)
package.

### IObjectMapper Interface [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#iobjectmapper-interface)

IObjectMapper is a simple abstraction that has Map methods to map an
object to another. We can replace the above code with this, instead:

Copy

```
public class UserAppService : ApplicationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IObjectMapper _objectMapper;

    public UserAppService(IRepository<User> userRepository, IObjectMapper objectMapper)
    {
        _userRepository = userRepository;
        _objectMapper = objectMapper;
    }

    public void CreateUser(CreateUserInput input)
    {
        var user = _objectMapper.Map<User>(input);
        _userRepository.Insert(user);
    }
}
```

Map is a simple method that gets the source object and creates a new
destination object with the type declared as the generic parameter (User
in this sample). The Map method has an overload to map an object to an
**existing** object. Assume that we already have a User entity and want
to update it's properties using an object:

Copy

```
public void UpdateUser(UpdateUserInput input)
{
    var user = _userRepository.Get(input.Id);
    _objectMapper.Map(input, user);
}
```

### AutoMapper Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#automapper-integration)

The **[Abp.AutoMapper](https://www.nuget.org/packages/Abp.AutoMapper)**
NuGet package ( [module](https://aspnetboilerplate.com/Pages/Documents/Module-System)) implements the IObjectMapper
and provides additional features.

#### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#installation)

First, install the **Abp.AutoMapper** NuGet package to your project:

Copy

```
Install-Package Abp.AutoMapper
```

Then add a dependency for **AbpAutoMapperModule** to your module
definition class:

Copy

```
[DependsOn(typeof(AbpAutoMapperModule))]
public class MyModule : AbpModule
{
    ...
}
```

You can then safely [inject](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and use
IObjectMapper in your code. You can also use
[AutoMapper](http://automapper.org/)'s own API when you need it.

#### Creating Mappings [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#creating-mappings)

Before using the mapping, AutoMapper requires you to define mappings between classes (by default).
You can see AutoMapper's own
[documentation](http://automapper.org/) for details on mapping. ASP.NET
Boilerplate makes it a bit easier and modular.

##### Auto Mapping Attributes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#auto-mapping-attributes)

Most of the time you may only want to directly (and conventionally) map classes.
In this case, you can use the **AutoMap**, **AutoMapFrom** and **AutoMapTo**
attributes. For instance, if we want to map the **CreateUserInput** class to
the **User** class in the sample above, we can use the **AutoMapTo** attribute
as shown below:

Copy

```
[AutoMapTo(typeof(User))]
public class CreateUserInput
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string EmailAddress { get; set; }

    public string Password { get; set; }
}
```

The AutoMap attribute maps two classes in both directions. But in this
sample, we only need to map from CreateUserInput to User, so we used
AutoMapTo.

##### Custom Mapping [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#custom-mapping)

Simple mapping may not be suitable in some cases. For instance, property
names of two classes may be a little different or you may want to ignore
some properties during the mappping. In such cases you should directly
use AutoMapper's API to define the mapping. The Abp.AutoMapper package
defines an API to make custom mapping more modular.

Assume that we want to ignore Password on mapping and the User has a slightly
different named Email property. We can define the mapping as shown below:

Copy

```
[DependsOn(typeof(AbpAutoMapperModule))]
public class MyModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Modules.AbpAutoMapper().Configurators.Add(config =>
        {
            config.CreateMap<CreateUserInput, User>()
                  .ForMember(u => u.Password, options => options.Ignore())
                  .ForMember(u => u.Email, options => options.MapFrom(input => input.EmailAddress));
        });
    }
}
```

AutoMapper has many more options and abilities for object to object
mapping. See it's [documentation](http://automapper.org/) for more info.

#### Pre Defined Mappings [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#pre-defined-mappings)

##### LocalizableString -> string [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#localizablestring-string)

The Abp.AutoMapper module defines a mapping to convert LocalizableString (or
ILocalizableString) objects to string objects. It makes the conversion
using [ILocalizationManager](https://aspnetboilerplate.com/Pages/Documents/Localization), so localizable
properties are automatically localized during the mapping process of any
class.

#### Injecting IMapper [Anchor](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping\#injecting-imapper)

You may need to directly use AutoMapper's IMapper object instead of
IObjectMapper abstraction. In that case, just inject IMapper in your
classes and use it. The Abp.AutoMapper package registers the IMapper with
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) as a singleton.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe