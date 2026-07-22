# Modules - EAF API Template

## Overview

The EAF API Template follows the N-Layer Architecture pattern with clear separation of concerns across different layers. Each layer has a specific responsibility and communicates with other layers through well-defined interfaces.

## Project Structure

```
src/
├── GameHub.Application/    # Application Layer
├── GameHub.Core/            # Domain Layer
├── GameHub.EntityFrameworkCore/  # Data Access Layer
├── GameHub.Migrator/        # Database Migration Tool
└── GameHub.Web.Host/        # Presentation Layer (Web API)
```

## Domain Layer (GameHub.Core)

**Location**: `src/GameHub.Core/`

The Domain Layer contains the core business logic and entities. It has no dependencies on other layers.

### Components

#### Entities

**Location**: `Entities/`

Domain entities that represent business objects:

- `AbpUser`: Extended user entity from ABP
- `AbpRole`: Extended role entity from ABP
- `AbpTenant`: Extended tenant entity from ABP
- Custom entities specific to your application

#### Value Objects

**Location**: `ValueObjects/`

Immutable value objects that represent domain concepts:

- `EmailAddress`: Email address validation
- `PhoneNumber`: Phone number validation
- Custom value objects for your domain

#### Interfaces

**Location**: `Interfaces/`

Repository interfaces for data access:

- `IUserRepository`: User repository interface
- `IRoleRepository`: Role repository interface
- `ITenantRepository`: Tenant repository interface
- Custom repository interfaces

#### Services

**Location**: `DomainServices/`

Domain services that contain business logic not naturally fitting in entities:

- `UserManager`: User management logic
- `RoleManager`: Role management logic
- `TenantManager`: Tenant management logic
- Custom domain services

#### Specifications

**Location**: `Specifications/`

Specification pattern for complex queries:

- `ActiveUserSpecification`: Filter for active users
- Custom specifications for your domain

## Application Layer (GameHub.Application)

**Location**: `src/GameHub.Application/`

The Application Layer contains application services, DTOs, and application logic. It coordinates between the domain layer and the presentation layer.

### Components

#### Application Services

**Location**: `App/`

Application services that handle application use cases:

- `UserAppService`: User CRUD operations
- `RoleAppService`: Role CRUD operations
- `TenantAppService`: Tenant CRUD operations
- `ConfigurationAppService`: Application configuration
- Custom application services

#### DTOs

**Location**: `Dto/`

Data Transfer Objects for data exchange:

- `UserDto`: User data representation
- `CreateUserDto`: User creation data
- `UpdateUserDto`: User update data
- `PagedResultDto<T>`: Paged result wrapper
- Custom DTOs for your application

#### Validators

**Location**: `Validations/`

FluentValidation validators for DTOs:

- `CreateUserDtoValidator`: User creation validation
- `UpdateUserDtoValidator`: User update validation
- Custom validators for your DTOs

#### Profiles

**Location**: `Profiles/`

AutoMapper profiles for object mapping:

- `UserProfile`: User entity to DTO mapping
- `RoleProfile`: Role entity to DTO mapping
- Custom profiles for your mappings

#### Interfaces

**Location**: `Interfaces/`

Application service interfaces:

- `IUserAppService`: User application service interface
- `IRoleAppService`: Role application service interface
- Custom application service interfaces

## Data Access Layer (GameHub.EntityFrameworkCore)

**Location**: `src/GameHub.EntityFrameworkCore/`

The Data Access Layer provides implementation of repositories and DbContext using Entity Framework Core.

### Components

#### DbContext

**Location**: `EafGameHubDbContext.cs`

Main DbContext that configures Entity Framework Core:

- Entity configurations
- Relationship mappings
- Database model builder
- Query filters for multi-tenancy

#### Repositories

**Location**: `Repositories/`

Repository implementations:

- `EfCoreUserRepository`: User repository implementation
- `EfCoreRoleRepository`: Role repository implementation
- `EfCoreTenantRepository`: Tenant repository implementation
- Custom repository implementations

#### Entity Configurations

**Location**: `EntityConfigurations/`

Fluent API entity configurations:

- `UserConfiguration`: User entity configuration
- `RoleConfiguration`: Role entity configuration
- `TenantConfiguration`: Tenant entity configuration
- Custom entity configurations

#### Migrations

**Location**: `Migrations/`

Database migrations created by Entity Framework Core:

- Initial migration with base schema
- Custom migrations for schema changes

## Web Host Layer (GameHub.Web.Host)

**Location**: `src/GameHub.Web.Host/`

The Web Host Layer contains the ASP.NET Core Web API host and controllers.

### Components

#### Startup

**Location**: `Startup.cs`

Application startup configuration:

- Service registration and dependency injection
- Middleware pipeline configuration
- Authentication and authorization setup
- Swagger/OpenAPI configuration
- CORS configuration
- SignalR configuration

#### Controllers

**Location**: `Controllers/`

API controllers for HTTP endpoints:

- `TokenAuthController`: Authentication endpoint
- `AccountController`: Account operations
- Custom controllers for your application

#### Filters

**Location`: `Filters/`

Action filters for cross-cutting concerns:

- `AbpAuthorizationFilter`: Authorization filter
- `AbpAuditLogFilter`: Audit logging filter
- `AbpValidationFilter`: Request validation filter
- Custom filters for your application

#### Middleware

**Location**: `Middleware/`

Custom middleware for request/response processing:

- `AbpExceptionHandlerMiddleware`: Exception handling
- Custom middleware for your application

#### Services

**Location**: `Services/`

Application-specific services:

- `SignalRHub`: Real-time communication hub
- Custom services for your application

## Migrator Project (GameHub.Migrator)

**Location**: `src/GameHub.Migrator/`

Console application for database migrations and seeding.

### Components

#### Migrator Service

**Location**: `MigratorService.cs`

Service that handles database migration:

- Run migrations
- Seed initial data
- Handle tenant database creation

## Module Dependencies

### Dependency Rules

The EAF template follows strict dependency rules:

- **Core Layer**: No dependencies on other layers
- **Application Layer**: Depends only on Core Layer
- **EntityFrameworkCore Layer**: Depends on Core Layer
- **Web.Host Layer**: Depends on Application and EntityFrameworkCore layers
- **Migrator Layer**: Depends on EntityFrameworkCore Layer

### Dependency Injection

The template uses Castle Windsor for dependency injection:

- Services are registered in modules
- Scoped, transient, and singleton lifetimes
- Interceptor registration for cross-cutting concerns

## Creating New Modules

### Step 1: Define Entity in Core Layer

```csharp
// src/GameHub.Core/Entities/MyEntity.cs
public class MyEntity : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### Step 2: Create Repository Interface in Core Layer

```csharp
// src/GameHub.Core/Interfaces/IMyEntityRepository.cs
public interface IMyEntityRepository : IRepository<MyEntity, Guid>
{
    // Custom methods
}
```

### Step 3: Implement Repository in EntityFrameworkCore Layer

```csharp
// src/GameHub.EntityFrameworkCore/Repositories/MyEntityRepository.cs
public class MyEntityRepository : EfCoreRepositoryBase<EafGameHubDbContext, MyEntity, Guid>, IMyEntityRepository
{
    public MyEntityRepository(IDbContextProvider<EafGameHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

### Step 4: Create DTOs in Application Layer

```csharp
// src/GameHub.Application/Dto/MyEntityDto.cs
public class MyEntityDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// src/GameHub.Application/Dto/CreateMyEntityDto.cs
public class CreateMyEntityDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
```

### Step 5: Create Application Service

```csharp
// src/GameHub.Application/App/MyEntityAppService.cs
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IRepository<MyEntity, Guid> _myEntityRepository;

    public MyEntityAppService(IRepository<MyEntity, Guid> myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

### Step 6: Configure AutoMapper Profile

```csharp
// src/GameHub.Application/Profiles/MyEntityProfile.cs
public class MyEntityProfile : Profile
{
    public MyEntityProfile()
    {
        CreateMap<MyEntity, MyEntityDto>();
        CreateMap<CreateMyEntityDto, MyEntity>();
    }
}
```

### Step 7: Register Application Service in Module

```csharp
// src/GameHub.Application/EafGameHubApplicationModule.cs
public class EafGameHubApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }

    public override void PreInitialize()
    {
        // Register application service
        IocManager.Register<IMyEntityAppService, MyEntityAppService>(DependencyLifeStyle.Transient);
    }
}
```

### Step 8: Create Migration

```bash
dotnet ef migrations add AddMyEntity --project src/GameHub.EntityFrameworkCore
```

### Step 9: Update Database

```bash
dotnet ef database update --project src/GameHub.EntityFrameworkCore
```

## Module Best Practices

1. **Separation of Concerns**: Keep each layer focused on its responsibility
2. **Dependency Direction**: Dependencies should only point downward (Web -> Application -> Core)
3. **Interface Segregation**: Define specific interfaces for repositories and services
4. **DTOs**: Use DTOs for data transfer between layers
5. **Validation**: Validate DTOs in the Application Layer
6. **Mapping**: Use AutoMapper for entity-DTO mapping
7. **Async**: Use async/await for all database operations
8. **Unit of Work**: Let ABP handle Unit of Work automatically
9. **Multi-tenancy**: Use AbpSession for tenant context
10. **Authorization**: Use [AbpAuthorize] attribute for permission checking
