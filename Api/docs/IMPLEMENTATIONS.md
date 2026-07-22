# Implementations - EAF API Template

## Overview

This document describes the key implementation patterns and practices used in the EAF API Template.

## Application Service Implementation

### Base Application Service

All application services extend `ApplicationService` from ABP, which provides:

- **Localization**: `L()` method for localized strings
- **Object Mapper**: `ObjectMapper` for entity-DTO mapping
- **Permission Checker**: `PermissionChecker` for authorization
- **Session**: `AbpSession` for current user and tenant information
- **Logger**: `Logger` for logging

### Standard CRUD Application Service

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IRepository<MyEntity, Guid> _myEntityRepository;

    public MyEntityAppService(IRepository<MyEntity, Guid> myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public async Task<PagedResultDto<MyEntityDto>> GetAllAsync(GetAllMyEntityInput input)
    {
        var query = await _myEntityRepository.GetAllAsync();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.Name.Contains(input.Filter));
        }

        // Apply sorting
        query = query.OrderBy(input.Sorting ?? "Name ASC");

        // Apply pagination
        var totalCount = await AsyncQueryableExecuter.CountAsync(query);
        var items = await AsyncQueryableExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        return new PagedResultDto<MyEntityDto>(
            totalCount,
            ObjectMapper.Map<List<MyEntityDto>>(items)
        );
    }

    public async Task<MyEntityDto> GetAsync(EntityDto<Guid> input)
    {
        var entity = await _myEntityRepository.GetAsync(input.Id);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }

    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }

    public async Task<MyEntityDto> UpdateAsync(UpdateMyEntityDto input)
    {
        var entity = await _myEntityRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, entity);
        await _myEntityRepository.UpdateAsync(entity);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }

    public async Task DeleteAsync(EntityDto<Guid> input)
    {
        await _myEntityRepository.DeleteAsync(input.Id);
    }
}
```

### Authorization in Application Services

Use the `[AbpAuthorize]` attribute to protect application services:

```csharp
[AbpAuthorize("Pages.Users")]
public class UserAppService : ApplicationService, IUserAppService
{
    // Methods require "Pages.Users" permission
}

[AbpAuthorize("Pages.Users.Create")]
public async Task CreateUserAsync(CreateUserDto input)
{
    // This method requires "Pages.Users.Create" permission
}
```

Check permissions programmatically:

```csharp
public async Task MyMethod()
{
    if (await PermissionChecker.IsGrantedAsync("Pages.Users.Create"))
    {
        // User has permission
    }
}
```

## Repository Implementation

### Custom Repository Implementation

```csharp
public class MyEntityRepository : EfCoreRepositoryBase<EafGameHubDbContext, MyEntity, Guid>, IMyEntityRepository
{
    public MyEntityRepository(IDbContextProvider<EafGameHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<MyEntity>> GetActiveEntitiesAsync()
    {
        return await (await GetAllAsync())
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<List<MyEntity>> GetEntitiesByTenantAsync(int tenantId)
    {
        return await (await GetAllAsync())
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();
    }
}
```

### Using Repository in Application Service

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IMyEntityRepository _myEntityRepository;

    public MyEntityAppService(IMyEntityRepository myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public async Task<List<MyEntityDto>> GetActiveEntitiesAsync()
    {
        var entities = await _myEntityRepository.GetActiveEntitiesAsync();
        return ObjectMapper.Map<List<MyEntityDto>>(entities);
    }
}
```

## Entity Configuration Implementation

### Fluent API Entity Configuration

```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.Name);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Multi-tenancy filter
        builder.ConfigureByConvention();
    }
}
```

### Register Entity Configuration in DbContext

```csharp
public class EafGameHubDbContext : AbpDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register entity configurations
        modelBuilder.ApplyConfiguration(new MyEntityConfiguration());
    }
}
```

## DTO Validation Implementation

### FluentValidation Validator

```csharp
public class CreateMyEntityDtoValidator : AbstractValidator<CreateMyEntityDto>
{
    public CreateMyEntityDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be valid");
    }
}
```

### Register Validator in Module

```csharp
public class EafGameHubApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        // Register validators
        Configuration.Validation.Validators.Add<CreateMyEntityDtoValidator>();
    }
}
```

## AutoMapper Profile Implementation

### AutoMapper Profile

```csharp
public class MyEntityProfile : Profile
{
    public MyEntityProfile()
    {
        CreateMap<MyEntity, MyEntityDto>();
        CreateMap<CreateMyEntityDto, MyEntity>();
        CreateMap<UpdateMyEntityDto, MyEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
```

### Register Profile in Module

```csharp
public class EafGameHubApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configure AutoMapper
        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            cfg => cfg.AddProfile<MyEntityProfile>()
        );
    }
}
```

## Multi-Tenancy Implementation

### Tenant-Aware Queries

Repositories automatically filter by tenant when using `IRepository`:

```csharp
public async Task<List<MyEntityDto>> GetAllAsync()
{
    // This automatically filters by current tenant
    var entities = await _myEntityRepository.GetAllListAsync();
    return ObjectMapper.Map<List<MyEntityDto>>(entities);
}
```

### Disable Tenant Filter

To query across all tenants (host level):

```csharp
using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
{
    var allEntities = await _myEntityRepository.GetAllListAsync();
}
```

### Switch Tenant

```csharp
using (CurrentUnitOfWork.SetTenantId(tenantId))
{
    // Operations in this scope use the specified tenant
    var entities = await _myEntityRepository.GetAllListAsync();
}
```

## Background Job Implementation

### Hangfire Background Job

```csharp
public class MyBackgroundJob : BackgroundJob<MyBackgroundJobArgs>
{
    private readonly IRepository<MyEntity, Guid> _myEntityRepository;

    public MyBackgroundJob(IRepository<MyEntity, Guid> myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public override Task ExecuteAsync(MyBackgroundJobArgs args)
    {
        // Background job logic
        var entities = _myEntityRepository.GetAllList();
        // Process entities
        return Task.CompletedTask;
    }
}
```

### Enqueue Background Job

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public MyEntityAppService(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task EnqueueJobAsync()
    {
        await _backgroundJobManager.EnqueueAsync<MyBackgroundJob, MyBackgroundJobArgs>(
            new MyBackgroundJobArgs { EntityId = Guid.NewGuid() }
        );
    }
}
```

## Event Bus Implementation

### Define Event

```csharp
public class MyEntityCreatedEvent : EventSource
{
    public Guid EntityId { get; set; }
    public string Name { get; set; }
}
```

### Publish Event

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IEventBus _eventBus;

    public MyEntityAppService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);

        // Publish event
        await _eventBus.PublishAsync(new MyEntityCreatedEvent
        {
            EntityId = entity.Id,
            Name = entity.Name
        });

        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

### Handle Event

```csharp
public class MyEntityCreatedEventHandler : IEventHandler<MyEntityCreatedEvent>, ITransientDependency
{
    public void HandleEvent(MyEntityCreatedEvent eventData)
    {
        // Handle event
        Logger.Info($"Entity created: {eventData.Name}");
    }
}
```

## SignalR Implementation

### SignalR Hub

```csharp
[Hub]
public class MyHub : AbpHub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}
```

### Register Hub in Startup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSignalR();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHub<MyHub>("/signalr");
    });
}
```

### Use SignalR in Application Service

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IHubContext<MyHub> _hubContext;

    public MyEntityAppService(IHubContext<MyHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);

        // Notify clients via SignalR
        await _hubContext.Clients.All.SendAsync("EntityCreated", entity.Name);

        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

## File Upload Implementation

### File Upload Controller

```csharp
public class FileController : EafControllerBase
{
    private readonly IBinaryObjectManager _binaryObjectManager;

    public FileController(IBinaryObjectManager binaryObjectManager)
    {
        _binaryObjectManager = binaryObjectManager;
    }

    [HttpPost]
    public async Task<UploadFileOutput> Upload()
    {
        var file = Request.Form.Files[0];
        var bytes = new byte[file.Length];
        using (var stream = file.OpenReadStream())
        {
            await stream.ReadAsync(bytes, 0, (int)file.Length);
        }

        var fileObject = new BinaryObject
        {
            Bytes = bytes
        };

        await _binaryObjectManager.SaveAsync(fileObject);

        return new UploadFileOutput
        {
            Id = fileObject.Id,
            Name = file.FileName
        };
    }
}
```

## Email Sending Implementation

### Email Sender

```csharp
public class MyEmailSender : IMyEmailSender, ITransientDependency
{
    private readonly IEmailSender _emailSender;

    public MyEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendWelcomeEmailAsync(User user)
    {
        await _emailSender.SendAsync(
            user.EmailAddress,
            "Welcome to EAF",
            $"Welcome {user.Name}!"
        );
    }
}
```

## Caching Implementation

### Cache Application Service

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly ICacheManager _cacheManager;

    public MyEntityAppService(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<MyEntityDto> GetAsync(EntityDto<Guid> input)
    {
        var cacheKey = $"MyEntity_{input.Id}";
        var cachedEntity = await _cacheManager.GetCache("MyEntityCache")
            .GetAsync(cacheKey, async () =>
            {
                var entity = await _myEntityRepository.GetAsync(input.Id);
                return ObjectMapper.Map<MyEntityDto>(entity);
            });

        return cachedEntity;
    }
}
```

## Unit of Work Implementation

### Automatic Unit of Work

ABP automatically manages Unit of Work for application service methods:

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    // Unit of Work is automatically started and committed
    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);
        // Unit of Work is committed here
        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

### Manual Unit of Work

```csharp
public class MyService
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<MyEntity, Guid> _myEntityRepository;

    public MyService(
        IUnitOfWorkManager unitOfWorkManager,
        IRepository<MyEntity, Guid> myEntityRepository)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _myEntityRepository = myEntityRepository;
    }

    public async Task CreateEntityAsync()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            var entity = new MyEntity { Name = "Test" };
            await _myEntityRepository.InsertAsync(entity);
            await uow.CompleteAsync();
        }
    }
}
```

## Best Practices

### Application Services

1. **Extend ApplicationService**: Always extend `ApplicationService` for base functionality
2. **Use Async**: Use async/await for all database operations
3. **Authorization**: Use `[AbpAuthorize]` attribute for permission checking
4. **Validation**: Use FluentValidation for DTO validation
5. **Mapping**: Use AutoMapper for entity-DTO mapping
6. **Logging**: Use `Logger` for logging
7. **Localization**: Use `L()` for localized strings

### Repositories

1. **Use IRepository**: Use the generic repository for simple operations
2. **Custom Repositories**: Create custom repositories for complex queries
3. **Async**: Use async methods for all database operations
4. **Unit of Work**: Let ABP manage Unit of Work automatically
5. **Multi-tenancy**: Repositories automatically filter by tenant

### Entities

1. **Inherit from ABP Base Classes**: Use `FullAuditedAggregateRoot` for entities
2. **Guid Primary Keys**: Use Guid for primary keys
3. **Soft Delete**: Use soft delete for entities
4. **Auditing**: Enable auditing for entities
5. **Multi-tenancy**: Enable multi-tenancy for entities

### DTOs

1. **Separate DTOs**: Create separate DTOs for create, update, and output
2. **Validation**: Use FluentValidation for DTO validation
3. **Data Annotations**: Use data annotations for basic validation
4. **Naming**: Use consistent naming (EntityDto, CreateEntityDto, UpdateEntityDto)

### Configuration

1. **Use appsettings.json**: Store configuration in appsettings.json
2. **Environment Variables**: Use environment variables for production
3. **Options Pattern**: Use the options pattern for configuration
4. **IOptions**: Inject `IOptions<T>` for configuration access
