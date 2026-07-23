

# Controllers in EAF

This document describes the controllers used in the Enterprise Application Framework (EAF).

## 1. Introduction

Controllers are responsible for handling incoming HTTP requests and returning responses. In EAF, controllers are typically implemented as ASP.NET Core MVC controllers.

## 2. Key Concepts

*   **ASP.NET Core MVC**: A framework for building web applications and APIs.
*   **Controller**: A class that handles incoming HTTP requests.
*   **Action**: A method on a controller that handles a specific HTTP request.
*   **Route**: A pattern that maps HTTP requests to actions.
*   **Model Binding**: The process of mapping data from HTTP requests to action parameters.
*   **Action Results**: The result of an action, such as a view, a JSON response, or a redirect.

## 3. Implementing Controllers

To implement a controller in EAF, you can create a class that inherits from the `Controller` class and add action methods to handle specific HTTP requests.

```csharp
// Example
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class MyController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Handle the GET request
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MyModel model)
    {
        // Handle the POST request
        return Created();
    }
}
```

## 4. Routing

Routing is used to map HTTP requests to actions. You can use attribute routing to define routes for your controllers and actions.

```csharp
// Example
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
public class MyController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Handle the GET request
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MyModel model)
    {
        // Handle the POST request
        return Created();
    }
}
```

## 5. Model Binding

Model binding is used to map data from HTTP requests to action parameters. You can use the `[FromBody]`, `[FromQuery]`, and `[FromRoute]` attributes to specify the source of the data.

## 6. Action Results

Action results are used to return responses to HTTP requests. You can use the `Ok()`, `Created()`, `BadRequest()`, and `NotFound()` methods to return different types of responses.

## 7. Best Practices

*   **Keep Controllers Thin**: Keep controllers thin and focused on handling HTTP requests and returning responses.
*   **Use Application Services**: Use application services to handle business logic.
*   **Validate Input**: Validate input data to prevent errors and security vulnerabilities.
*   **Handle Exceptions**: Handle exceptions gracefully and provide informative error messages.


