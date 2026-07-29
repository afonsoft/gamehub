---
name: aspnet-core-api
license: UNLICENSED
description: Use when designing and implementing RESTful APIs with ASP.NET Core, or when applying security, error handling, documentation, testing, or performance optimization to API endpoints
---

# ASP.NET Core API Skill

## When to Use This Skill

Use this skill when you need to:
- Design and implement RESTful APIs with ASP.NET Core
- Apply security best practices with JWT authentication
- Implement proper error handling and validation
- Create comprehensive API documentation with OpenAPI/Swagger
- Set up testing strategies for API endpoints
- Optimize API performance and caching

## Prerequisites

- ASP.NET Core 6+ project
- Understanding of RESTful API principles
- Knowledge of C# and Entity Framework Core
- Familiarity with authentication and authorization concepts

## Step-by-Step Workflows

### 1. API Project Setup

**Input**: API requirements and project specifications  
**Output**: Complete ASP.NET Core API project structure

```
Create an ASP.NET Core API project with these requirements:

1. Set up project structure with proper folders
2. Configure controllers, services, and repositories
3. Implement dependency injection
4. Add Entity Framework Core with database context
5. Configure authentication and authorization
6. Add logging and error handling
7. Set up OpenAPI/Swagger documentation
```

### 2. Controller Implementation

**Input**: API endpoints specifications  
**Output: Complete controllers with proper HTTP methods

```
Implement API controllers following these patterns:

1. Use proper HTTP verbs (GET, POST, PUT, DELETE)
2. Implement async/await patterns
3. Add proper validation and error handling
4. Return appropriate HTTP status codes
5. Use DTOs for data transfer
6. Apply authorization attributes
7. Add OpenAPI documentation
```

### 3. Security Implementation

**Input**: Security requirements and user roles  
**Output**: Complete JWT authentication and authorization

```
Implement security for the API with these requirements:

1. Configure JWT authentication
2. Set up authorization policies
3. Implement role-based access control
4. Add API key authentication if needed
5. Secure sensitive data
6. Implement rate limiting
7. Add CORS policies
```

## Core API Patterns

### Project Structure

```csharp
// Project structure
MyApi/
├── Controllers/
│   ├── UsersController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IUsersService.cs
│   │   └── IProductsService.cs
│   └── Implementations/
│       ├── UsersService.cs
│       └── ProductsService.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IUsersRepository.cs
│   │   └── IProductsRepository.cs
│   └── Implementations/
│       ├── UsersRepository.cs
│       └── ProductsRepository.cs
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   └── Product.cs
│   ├── DTOs/
│   │   ├── CreateUserDto.cs
│   │   └── UpdateUserDto.cs
│   └── Requests/
│       ├── CreateUserRequest.cs
│       └── UpdateUserRequest.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Configuration/
│   └── DependencyInjection.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs
```

### Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUsersService usersService,
        ILogger<UsersController> logger)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        try
        {
            var users = await _usersService.GetUsersAsync(page, pageSize, search);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving users");
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _usersService.GetUserAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving the user");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _usersService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while creating the user");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateUser(
        int id, 
        [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _usersService.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while updating the user");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _usersService.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while deleting the user");
        }
    }
}
```

### DTOs and Requests

```csharp
// DTOs for data transfer
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }
}
```

## Security Implementation

### JWT Configuration

```csharp
// Program.cs - JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});
```

### JWT Service

```csharp
public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    }

    public string GenerateToken(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"])),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }
}
```

## Error Handling

### Custom Exceptions

```csharp
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public ApiException(int statusCode, string message, string? errorCode = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class ValidationException : ApiException
{
    public ValidationException(string message) 
        : base(StatusCodes.Status400BadRequest, message, "VALIDATION_ERROR")
    {
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) 
        : base(StatusCodes.Status404NotFound, message, "NOT_FOUND")
    {
    }
}
```

### Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ApiException apiEx => new
            {
                StatusCode = apiEx.StatusCode,
                Message = apiEx.Message,
                ErrorCode = apiEx.ErrorCode
            },
            ValidationException validationEx => new
            {
                StatusCode = validationEx.StatusCode,
                Message = validationEx.Message,
                ErrorCode = validationEx.ErrorCode
            },
            _ => new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An internal server error occurred",
                ErrorCode = "INTERNAL_SERVER_ERROR"
            }
        };

        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## OpenAPI/Swagger Configuration

```csharp
// Program.cs - Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "ASP.NET Core API with best practices",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

## Performance Optimization

### Caching Implementation

```csharp
public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan expiration);
    void Remove(string key);
}

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}

// Usage in controller
[HttpGet("cached/{id}")]
[ResponseCache(Duration = 300)] // 5 minutes
public async Task<ActionResult<UserDto>> GetUserCached(int id)
{
    var cacheKey = $"user_{id}";
    var cachedUser = _cacheService.Get<UserDto>(cacheKey);
    
    if (cachedUser != null)
    {
        return Ok(cachedUser);
    }

    var user = await _usersService.GetUserAsync(id);
    if (user == null)
        return NotFound();

    _cacheService.Set(cacheKey, user, TimeSpan.FromMinutes(5));
    return Ok(user);
}
```

### Rate Limiting

```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var cacheKey = $"rate_limit_{clientId}";
        
        var requestCount = _cache.Get<int>(cacheKey);
        if (requestCount >= 100) // 100 requests per minute
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
```

## Testing Strategies

### Unit Testing Controllers

```csharp
public class UsersControllerTests
{
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _usersServiceMock = new Mock<IUsersService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_usersServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkResult()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        };

        _usersServiceMock.Setup(s => s.GetUsersAsync(1, 10, null))
                      .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count());
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        var createdUser = new UserDto
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _usersServiceMock.Setup(s => s.CreateUserAsync(request))
                      .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedUser = Assert.IsType<UserDto>(createdAtResult.Value);
        Assert.Equal(createdUser.Id, returnedUser.Id);
    }
}
```

### Integration Testing

```csharp
public class UsersApiTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersApiTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
    }
}
```

## Deployment Configuration

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApi.csproj", "."]
RUN dotnet restore "./MyApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MyApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApiDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Issuer": "MyApi",
    "Audience": "MyApiUsers",
    "Key": "ThisIsASecretKeyForJWTTokenGeneration123456789",
    "ExpirationMinutes": 60
  },
  "AllowedHosts": "*"
}
```

## Best Practices Summary

### Do's
- ✅ Use async/await consistently
- ✅ Implement proper error handling
- ✅ Use DTOs for data transfer
- ✅ Add comprehensive validation
- ✅ Include OpenAPI documentation
- ✅ Implement authentication and authorization
- ✅ Use proper HTTP status codes
- ✅ Add comprehensive logging
- ✅ Implement caching strategies
- ✅ Write comprehensive tests

### Don'ts
- ❌ Return sensitive data in responses
- ❌ Use synchronous operations for I/O
- ❌ Ignore error handling
- ❌ Expose database entities directly
- ❌ Use magic strings or numbers
- ❌ Skip authentication for sensitive endpoints
- ❌ Ignore performance optimization
- ❌ Forget to document APIs
- ❌ Use blocking calls in async methods

## References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [RESTful API Design Guidelines](https://restfulapi.net/)
- [JWT Authentication in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- [OpenAPI/Swagger Documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swaggerswagger)

---

**This skill provides comprehensive guidance for building production-ready ASP.NET Core APIs. Use it to create secure, performant, and well-documented web APIs.**
