---
name: testing-xunit
description: Comprehensive unit testing with xUnit, mocking, test patterns, and best practices for .NET applications
---

# xUnit Testing Skill

## When to Use This Skill

Use this skill when you need to:
- Write comprehensive unit tests with xUnit framework
- Implement proper mocking and test isolation
- Apply test patterns and best practices
- Set up test infrastructure and fixtures
- Test async operations and dependencies
- Measure code coverage and test quality

## Prerequisites

- .NET 6+ project with xUnit test projects
- Understanding of unit testing concepts
- Familiarity with mocking frameworks (Moq, NSubstitute)
- Knowledge of test patterns (AAA, Test Data Builders)

## Step-by-Step Workflows

### 1. Test Project Setup

**Input**: Production project structure  
**Output**: Complete xUnit test project with proper configuration

```
Create an xUnit test project with these requirements:

1. Set up test project with proper dependencies
2. Configure test runners and coverage tools
3. Create test fixtures and test data builders
4. Set up mocking frameworks
5. Configure test categories and traits
6. Add test organization and naming conventions
7. Set up CI/CD integration for testing
```

### 2. Unit Test Implementation

**Input**: Class or method to test  
**Output: Complete unit tests with proper structure

```
Create unit tests following these patterns:

1. Use Arrange-Act-Assert (AAA) pattern
2. Implement proper test isolation
3. Add meaningful test names and descriptions
4. Use test data builders and factories
5. Mock dependencies appropriately
6. Test edge cases and error conditions
7. Add assertions for all expected behaviors
```

### 3. Integration Testing

**Input**: Multiple components or external dependencies  
**Output: Integration tests with real dependencies

```
Create integration tests with these requirements:

1. Set up test database or in-memory providers
2. Configure test web hosts or containers
3. Test end-to-end workflows
4. Validate external service integrations
5. Test database operations and migrations
6. Validate API endpoints and contracts
7. Clean up test data and resources
```

## Core Testing Patterns

### Test Project Structure

```csharp
// Test project structure
MyProject.Tests/
├── Unit/
│   ├── Services/
│   │   ├── UserServiceTests.cs
│   │   └── ProductServiceTests.cs
│   ├── Controllers/
│   │   ├── UsersControllerTests.cs
│   │   └── ProductsControllerTests.cs
│   └── Repositories/
│       ├── UserRepositoryTests.cs
│       └── ProductRepositoryTests.cs
├── Integration/
│   ├── Api/
│   │   ├── UsersApiTests.cs
│   │   └── ProductsApiTests.cs
│   ├── Database/
│   │   ├── UserRepositoryIntegrationTests.cs
│   │   └── ProductRepositoryIntegrationTests.cs
│   └── External/
│       └── EmailServiceIntegrationTests.cs
├── Fixtures/
│   ├── DatabaseFixture.cs
│   ├── WebApplicationFixture.cs
│   └── TestDataFixture.cs
├── Builders/
│   ├── UserBuilder.cs
│   ├── ProductBuilder.cs
│   └── OrderBuilder.cs
├── Helpers/
│   ├── TestHelper.cs
│   └── AssertionHelper.cs
└── TestData/
    ├── Users.json
    └── Products.json
```

### Basic Unit Test Structure

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        
        _userService = new UserService(
            _userRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateUser_ValidUser_ReturnsUserWithId()
    {
        // Arrange
        var createUserRequest = new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        var expectedUser = new User
        {
            Id = 1,
            Name = createUserRequest.Name,
            Email = createUserRequest.Email,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                      .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.CreateUserAsync(createUserRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(createUserRequest.Name);
        result.Email.Should().Be(createUserRequest.Email);
        
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendWelcomeEmail(It.IsAny<User>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateUser_InvalidName_ThrowsArgumentException(string name)
    {
        // Arrange
        var createUserRequest = new CreateUserRequest
        {
            Name = name,
            Email = "john@example.com",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync(createUserRequest));
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUser_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync((User)null!);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }
}
```

### Test Data Builders

```csharp
public class UserBuilder
{
    private readonly User _user;

    public UserBuilder()
    {
        _user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public UserBuilder WithId(int id)
    {
        _user.Id = id;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder CreatedAt(DateTime createdAt)
    {
        _user.CreatedAt = createdAt;
        return this;
    }

    public UserBuilder AsInactive()
    {
        _user.IsActive = false;
        return this;
    }

    public User Build() => _user;

    public static implicit operator User(UserBuilder builder) => builder.Build();
}

// Usage in tests
[Fact]
public async Task UpdateUser_ValidData_UpdatesUser()
{
    // Arrange
    var existingUser = new UserBuilder()
        .WithId(1)
        .WithName("John Doe")
        .WithEmail("john@example.com")
        .Build();

    var updateRequest = new UpdateUserRequest
    {
        Name = "Jane Smith",
        Email = "jane@example.com"
    };

    _userRepositoryMock.Setup(r => r.GetByIdAsync(existingUser.Id))
                      .ReturnsAsync(existingUser);

    // Act
    var result = await _userService.UpdateUserAsync(existingUser.Id, updateRequest);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Jane Smith");
    result.Email.Should().Be("jane@example.com");
}
```

### Test Fixtures

```csharp
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }
    public IDbConnection Connection { get; private set; }

    public DatabaseFixture()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        Connection = Context.Database.GetDbConnection();
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        };

        Context.Users.AddRange(users);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Connection?.Dispose();
        Context?.Dispose();
    }
}

public class UserServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly UserService _userService;

    public UserServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _userService = new UserService(
            new UserRepository(_fixture.Context),
            Mock.Of<IEmailService>(),
            Mock.Of<ILogger<UserService>>());
    }

    [Fact]
    public async Task GetUser_ExistingInDatabase_ReturnsUser()
    {
        // Act
        var result = await _userService.GetUserAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
    }
}
```

### Async Testing

```csharp
public class AsyncServiceTests
{
    [Fact]
    public async Task ProcessDataAsync_ValidData_ProcessesSuccessfully()
    {
        // Arrange
        var service = new AsyncService();
        var data = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = await service.ProcessDataAsync(data);

        // Assert
        result.Should().NotBeNull();
        result.ProcessedCount.Should().Be(5);
        result.Sum.Should().Be(15);
    }

    [Fact]
    public async Task ProcessDataAsync_WithCancellation_CancelsOperation()
    {
        // Arrange
        var service = new AsyncService();
        var data = Enumerable.Range(1, 1000).ToList();
        var cts = new CancellationTokenSource();
        
        // Cancel immediately
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ProcessDataAsync(data, cts.Token));
    }

    [Fact]
    public async Task ProcessDataAsync_ThrowsException_ThrowsSameException()
    {
        // Arrange
        var service = new AsyncService();
        var data = new List<int> { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessDataAsync(data));
    }
}
```

## Mocking Strategies

### Mock Repository

```csharp
public class UserRepositoryTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _repository = new UserRepository(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "John Doe" };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync(expectedUser);

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ValidUser_ReturnsUserWithId()
    {
        // Arrange
        var user = new User { Name = "John Doe", Email = "john@example.com" };
        var expectedUser = user with { Id = 1 };

        _repositoryMock.Setup(r => r.AddAsync(user))
                      .ReturnsAsync(expectedUser);

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        
        _repositoryMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Doe" },
            new User { Id = 2, Name = "Jane Smith" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(users);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
```

### Mock External Services

```csharp
public class EmailServiceTests
{
    [Fact]
    public async Task SendWelcomeEmail_ValidUser_SendsEmail()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
        var emailServiceMock = new Mock<IEmailService>();
        
        emailServiceMock.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await emailServiceMock.Object.SendWelcomeEmailAsync(user);

        // Assert
        result.Should().BeTrue();
        
        emailServiceMock.Verify(e => e.SendEmailAsync(
            user.Email,
            "Welcome to Our Service",
            It.Contains(user.Name)), Times.Once);
    }

    [Fact]
    public async Task SendEmail_Failure_ThrowsException()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        
        emailServiceMock.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new SmtpException("SMTP server unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<SmtpException>(
            () => emailServiceMock.Object.SendEmailAsync(
                "test@example.com",
                "Test Subject",
                "Test Body"));
    }
}
```

## Integration Testing

### Web Application Factory

```csharp
public class UsersApiTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public UsersApiTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
        content.Should().NotBeEmpty();
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        createdUser.Name.Should().Be("John Doe");
        createdUser.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task CreateUser_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Name = "", // Invalid
            Email = "invalid-email", // Invalid
            Password = "123" // Too short
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace database with in-memory version
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Create test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            dbContext.Database.EnsureCreated();
            
            // Seed test data
            dbContext.Users.AddRange(new List<User>
            {
                new() { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new() { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
            });
            dbContext.SaveChanges();
        });
    }
}
```

## Test Categories and Traits

```csharp
// Test categories
[Trait("Category", "Unit")]
public class UserServiceUnitTests
{
    [Fact]
    [Trait("Priority", "High")]
    public void CreateUser_ValidData_CreatesUser()
    {
        // Test implementation
    }

    [Fact]
    [Trait("Priority", "Low")]
    public void CreateUser_InvalidData_ThrowsException()
    {
        // Test implementation
    }
}

[Trait("Category", "Integration")]
[Trait("Database", "Required")]
public class UserRepositoryIntegrationTests
{
    [Fact]
    [Trait("Slow", "True")]
    public async Task GetAllUsers_WithDatabase_ReturnsUsers()
    {
        // Test implementation
    }
}

// Custom traits
public class TestTraits
{
    public const string Unit = "Unit";
    public const string Integration = "Integration";
    public const string Slow = "Slow";
    public const string Database = "Database";
    public const string External = "External";
    public const string HighPriority = "High";
    public const string LowPriority = "Low";
}
```

## Code Coverage

### Configuration

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./coverage.xml</CoverletOutput>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>
</Project>
```

### Running Coverage

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

### coverlet.runsettings

```json
{
  "$schema": "https://json.schemastore.org/coverlet.runsettings",
  "enable": true,
  "exclude": [
    "[*]Program.cs",
    "[*]*.g.cs",
    "[*]*.g.i.cs"
  ],
  "excludeByAttribute": [
    "Obsolete",
    "GeneratedCodeAttribute",
    "CompilerGeneratedAttribute"
  ],
  "includeDirectories": [
    "src/"
  ],
  "threshold": 80,
  "thresholdType": "line",
  "thresholdStat": "minimum",
  "reportFormats": [
    "json",
    "xml",
    "html"
  ]
}
```

## Best Practices

### Test Naming

```csharp
// Good test names
[Fact]
public async Task CreateUser_ValidUser_ReturnsUserWithId() { }

[Fact]
public async Task GetUser_WithInvalidId_ThrowsNotFoundException() { }

[Theory]
[InlineData(1, "John Doe", "john@example.com")]
[InlineData(2, "Jane Smith", "jane@example.com")]
public void User_Constructor_WithValidData_CreatesUser(int id, string name, string email) { }

// Bad test names
[Fact]
public void Test1() { }
[Fact]
public void UserTest() { }
[Fact]
public void CheckUser() { }
```

### Test Organization

```csharp
public class UserServiceTests
{
    // Constructor tests
    public class Constructor
    {
        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException() { }
    }

    // Create user tests
    public class CreateUser
    {
        [Fact]
        public async Task CreateUser_ValidUser_ReturnsUserWithId() { }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateUser_InvalidName_ThrowsArgumentException(string name) { }
    }

    // Get user tests
    public class GetUser
    {
        [Fact]
        public async Task GetUser_ExistingUser_ReturnsUser() { }
        
        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNull() { }
    }
}
```

### Assertion Guidelines

```csharp
// Good assertions
result.Should().NotBeNull();
result.Id.Should().Be(expectedId);
result.Name.Should().Be(expectedName);
result.Email.Should().Be(expectedEmail);

// Bad assertions
Assert.NotNull(result);
Assert.Equal(expectedId, result.Id);
Assert.Equal(expectedName, result.Name);
Assert.Equal(expectedEmail, result.Email);

// Use specific assertions for collections
users.Should().HaveCount(3);
users.Should().Contain(u => u.Email == "john@example.com");
users.Should().OnlyContain(u => u.IsActive);

// Use collection assertions for order
orderedUsers.Should().BeInAscendingOrder(u => u.Name);
```

## References

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/testing/)

---

**This skill provides comprehensive guidance for unit testing with xUnit. Use it to create maintainable, reliable, and comprehensive test suites for your .NET applications.**
