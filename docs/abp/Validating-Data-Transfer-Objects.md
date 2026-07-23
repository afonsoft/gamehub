# https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects

## Validating Data Transfer Objects

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Validating-Data-Transfer-Objects.md)

In this document

### Introduction to validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#introduction-to-validation)

In an application, the inputs should be validated first. The input can be
sent by a user or another application. In a web application, validation is
usually implemented twice: on the client and server sides. Client-side
validation is implemented mostly for user experience. It's better to
check a form first in the client and show invalid fields to the user.
However, server-side validation is unavoidable and more critical.

Server-side validation is generally implemented in [application\\
services](https://aspnetboilerplate.com/Pages/Documents/Application-Services) or controllers (in
general, all services get data from the presentation layer). An application
service method should first check (validate) the input and then use it.
ASP.NET Boilerplate provides the infrastructure to automatically
validate inputs of an application for:

- All [application service](https://aspnetboilerplate.com/Pages/Documents/Application-Services) methods
- All [ASP.NET Core](https://aspnetboilerplate.com/Pages/Documents/AspNet-Core) MVC controller actions
- All ASP.NET [MVC](https://aspnetboilerplate.com/Pages/Documents/MVC-Controllers) and [Web\\
API](https://aspnetboilerplate.com/Pages/Documents/Web-API-Controllers) controller actions.

See the Disabling Validation section to disable validation if needed.

### Using data annotations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#using-data-annotations)

ASP.NET Boilerplate supports data annotation attributes. Assume that
we're developing a Task application service that is used to create a
task by when it gets an input as shown below:

Copy

```
public class CreateTaskInput
{
    public int? AssignedPersonId { get; set; }

    [Required]
    public string Description { get; set; }
}
```

Here, the **Description** property is marked as **Required**.
AssignedPersonId is optional. There are also many attributes (like
MaxLength, MinLength, RegularExpression...) in the
**System.ComponentModel.DataAnnotations** namespace. See the Task
[application service](https://aspnetboilerplate.com/Pages/Documents/Application-Services)
implementation:

Copy

```
public class TaskAppService : ITaskAppService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPersonRepository _personRepository;

    public TaskAppService(ITaskRepository taskRepository, IPersonRepository personRepository)
    {
        _taskRepository = taskRepository;
        _personRepository = personRepository;
    }

    public void CreateTask(CreateTaskInput input)
    {
        var task = new Task { Description = input.Description };

        if (input.AssignedPersonId.HasValue)
        {
            task.AssignedPerson = _personRepository.Load(input.AssignedPersonId.Value);
        }

        _taskRepository.Insert(task);
    }
}
```

As you can see, there is no validation code written since ASP.NET Boilerplate does
it automatically. ASP.NET Boilerplate also checks if input is **null**
and throws an **AbpValidationException** if it is, so you don't have to write
**null-check** code (guard clauses). It also throws an
AbpValidationException if any of the input properties are invalid.

This mechanism is similar to ASP.NET MVC's validation but note that an
application service class is not derived from a Controller, it's a plain
class and can work even outside of a web application.

### Custom Validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#custom-validation)

If data annotations are not sufficient for your case, you can implement
the **ICustomValidate** interface as shown below:

Copy

```
public class CreateTaskInput : ICustomValidate
{
    public int? AssignedPersonId { get; set; }

    public bool SendEmailToAssignedPerson { get; set; }

    [Required]
    public string Description { get; set; }

    public void AddValidationErrors(CustomValidationContext context)
    {
        if (SendEmailToAssignedPerson && (!AssignedPersonId.HasValue || AssignedPersonId.Value <= 0))
        {
            context.Results.Add(new ValidationResult("AssignedPersonId must be set if SendEmailToAssignedPerson is true!"));
        }
    }
}
```

The ICustomValidate interface declares the **AddValidationErrors** method to be
implemented. We must add the **ValidationResult** objects to the
**context.Results** list if there are validation errors. You can also
use the context.IocResolver to [resolve\\
dependencies](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) if needed in the validation
process.

In addition to ICustomValidate, ABP also supports .NET's standard
IValidatableObject interface. You can also implement it to perform
additional custom validations. If you implement both interfaces, both of
them will be called.

### Fluent Validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#fluent-validation)

In order to use [FluentValidation](https://github.com/JeremySkinner/FluentValidation), you need to install [Abp.FluentValidation](https://www.nuget.org/packages/Abp.FluentValidation) package first.

Copy

```
Install-Package Abp.FluentValidation
```

Then, You should set a dependency to AbpFluentValidationModule from your module. Example:

Copy

```
[DependsOn(typeof(AbpFluentValidationModule))]
public class MyProjectAppModule : AbpModule
{

}
```

After all, you can define your [FluentValidation](https://github.com/JeremySkinner/FluentValidation) validators to validate matching input classes.

As an example, if you have an input class and a Controller which uses this class as it's input parameter;

Copy

```
public class MyCustomArgument1
{
	public int Value { get; set; }
}

public class MyTestController : AbpController {

	public JsonResult GetJsonValue([FromQuery] MyCustomArgument1 arg1)
	{
		return Json(new MyCustomArgument1
		{
			Value = arg1.Value
		});
	}
}
```

If you want to limit the value of MyCustomArgument1's Value field between 1 and 99, you can define a validator like the one below;

Copy

```
public class MyCustomArgument1Validator : AbstractValidator<MyCustomArgument1>
{
	public MyCustomArgument1Validator()
	{
		RuleFor(x => x.Value).InclusiveBetween(1, 99);
	}
}
```

ABP will run MyCustomArgument1Validator to validate MyCustomArgument1 class automatically.

### Disabling Validation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#disabling-validation)

For automatically validated classes (see Introduction section), you can
use these attributes to control validation:

- **DisableValidation** attribute can be used for classes, methods or
properties of DTOs to disable validation.
- **EnableValidation** attribute can only be used to enable validation
for a method, if it's disabled for the containing class.

### Normalization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects\#normalization)

We may need to perform an extra operations to prepare DTO parameters
after validation. ASP.NET Boilerplate defines an **IShouldNormalize**
interface that has a **Normalize** method. If you implement this
interface, the Normalize method is called just after validation (and just
before the method call). Assume that our DTO gets a Sorting direction. If
it's not supplied, we want to set a default sorting:

Copy

```
public class GetTasksInput : IShouldNormalize
{
    public string Sorting { get; set; }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Sorting))
        {
            Sorting = "Name ASC";
        }
    }
}
```

**Note:** By default, Normalization only works when an AppService or a Controller is called directly. For example, if you call an AppService method from a Controller, Normalization will not work. In such cases, you can either enable conventional interceptors by setting `removeConventionalInterceptors=false` when calling `services.AddAbp` in the Startup.cs file (this is not suggested for performance reasons) or you can call Normalize method of the related class manually.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe