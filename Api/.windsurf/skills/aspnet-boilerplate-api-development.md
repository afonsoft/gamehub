# ASP.NET Boilerplate API Development Skill - EAF Project Name API

You are an expert in ASP.NET Boilerplate (ABP), .NET 10.0, and the EAF Project Name API template. You write functional, maintainable, performant, and scalable code following ABP and .NET best practices for API development.

## Project Context

This is the EAF Project Name API template located in `Templates/Api/src`. It's a complete ABP-based application with domain, application, and infrastructure layers.

### Current State
- **.NET Version**: 10.0
- **ABP Version**: 10.3.0
- **Database**: Entity Framework Core with SQL Server
- **Architecture**: N-Layer Architecture (Core, Application, EntityFrameworkCore, Web.Host)

### Project Structure

#### Core Layer (GameHub.Core)
- Domain entities and value objects
- Application interfaces
- Authorization and permissions
- Localization resources
- Feature definitions
- Settings

#### Application Layer (GameHub.Application)
- Application services
- DTOs (Data Transfer Objects)
- Business logic
- Exporting functionality
- Job definitions

#### EntityFrameworkCore Layer (GameHub.EntityFrameworkCore)
- DbContext implementation
- Entity configurations
- Repository implementations
- Database migrations
- Seed data

#### Web Host (GameHub.Web.Host)
- Startup configuration
- API controllers
- Authentication and authorization
- SignalR hubs
- Swagger UI

## ABP N-Layer Architecture

### Domain Layer (Core)

#### Entities
```csharp
// Entity in GameHub.Core/Airplanes/
public class Airplane : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Model { get; set; }
    public int Capacity { get; set; }
}
```

#### Permissions
```csharp
// GameHub.Core/Application/Authorization/GameHubPermissions.cs
public static class GameHubPermissions
{
    public const string Airplanes = "Pages.Airplanes";
    public const string Airplanes_Create = "Pages.Airplanes.Create";
    public const string Airplanes_Edit = "Pages.Airplanes.Edit";
    public const string Airplanes_Delete = "Pages.Airplanes.Delete";
}

// Permission provider
public class GameHubAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        var airplanes = context.GetPermissionOrNull(GameHubPermissions.Airplanes) 
            ?? context.CreatePermission(GameHubPermissions.Airplanes, L("Airplanes"));
        
        airplanes.CreateChildPermission(GameHubPermissions.Airplanes_Create, L("Create"));
        airplanes.CreateChildPermission(GameHubPermissions.Airplanes_Edit, L("Edit"));
        airplanes.CreateChildPermission(GameHubPermissions.Airplanes_Delete, L("Delete"));
    }
}
```

#### Localization
```xml
<!-- GameHub.Core/Application/Localization/GameHub/GameHub.xml -->
<?xml version="1.0" encoding="utf-8" ?>
<localizationDictionary culture="en">
  <texts>
    <text name="Airplanes" value="Airplanes" />
    <text name="CreateAirplane" value="Create Airplane" />
    <text name="EditAirplane" value="Edit Airplane" />
    <text name="DeleteAirplane" value="Delete Airplane" />
  </texts>
</localizationDictionary>
```

### Application Layer

#### Application Service Interface
```csharp
// GameHub.Application/Airplanes/IAirplaneAppService.cs
public interface IAirplaneAppService : IApplicationService
{
    Task<PagedResultDto<AirplaneDto>> GetAll(GetAllAirplanesInput input);
    Task<AirplaneDto> Get(EntityDto<Guid> input);
    Task<AirplaneDto> Create(CreateAirplaneDto input);
    Task<AirplaneDto> Update(UpdateAirplaneDto input);
    Task Delete(EntityDto<Guid> input);
    Task<FileDto> GetAirplanesToExcel(GetAllAirplanesForExcelInput input);
}
```

#### Application Service Implementation
```csharp
// GameHub.Application/Airplanes/AirplaneAppService.cs
public class AirplaneAppService : GameHubAppServiceBase, IAirplaneAppService
{
    private readonly IRepository<Airplane, Guid> _airplaneRepository;

    public AirplaneAppService(IRepository<Airplane, Guid> airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    [AbpAuthorize(GameHubPermissions.Airplanes)]
    public async Task<PagedResultDto<AirplaneDto>> GetAll(GetAllAirplanesInput input)
    {
        var query = _airplaneRepository.GetAll()
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), 
                e => e.Name.Contains(input.Filter) || e.Model.Contains(input.Filter));

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(input.Sorting ?? "name asc")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AirplaneDto>(
            totalCount,
            ObjectMapper.Map<List<AirplaneDto>>(items)
        );
    }

    [AbpAuthorize(GameHubPermissions.Airplanes_Create)]
    public async Task<AirplaneDto> Create(CreateAirplaneDto input)
    {
        var airplane = ObjectMapper.Map<Airplane>(input);
        await _airplaneRepository.InsertAsync(airplane);
        return ObjectMapper.Map<AirplaneDto>(airplane);
    }
}
```

#### DTOs
```csharp
// GameHub.Application/Airplanes/Dtos/AirplaneDto.cs
public class AirplaneDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Model { get; set; }
    public int Capacity { get; set; }
}

// GameHub.Application/Airplanes/Dtos/CreateAirplaneDto.cs
public class CreateAirplaneDto
{
    [Required]
    [StringLength(Airplane.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(Airplane.MaxModelLength)]
    public string Model { get; set; }

    [Range(Airplane.MinCapacity, Airplane.MaxCapacity)]
    public int Capacity { get; set; }
}

// GameHub.Application/Airplanes/Dtos/GetAllAirplanesInput.cs
public class GetAllAirplanesInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
```

#### AutoMapper Profile
```csharp
// GameHub.Application/Airplanes/AirplaneAutoMapperProfile.cs
public class AirplaneAutoMapperProfile : Profile
{
    public AirplaneAutoMapperProfile()
    {
        CreateMap<Airplane, AirplaneDto>();
        CreateMap<CreateAirplaneDto, Airplane>();
        CreateMap<Airplane, CreateAirplaneDto>();
        CreateMap<UpdateAirplaneDto, Airplane>();
    }
}
```

### EntityFrameworkCore Layer

#### DbContext
```csharp
// GameHub.EntityFrameworkCore/GameHubDbContext.cs
public class GameHubDbContext : AbpDbContext
{
    public DbSet<Airplane> Airplanes { get; set; }

    public GameHubDbContext(DbContextOptions<GameHubDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddCustomMappings();
    }
}
```

#### Entity Configuration
```csharp
// GameHub.EntityFrameworkCore/EntityConfigurations/AirplaneConfiguration.cs
public class AirplaneConfiguration : IEntityTypeConfiguration<Airplane>
{
    public void Configure(EntityTypeBuilder<Airplane> builder)
    {
        builder.ToTable("Airplanes");
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(Airplane.MaxNameLength);
            
        builder.Property(e => e.Model)
            .IsRequired()
            .HasMaxLength(Airplane.MaxModelLength);
            
        builder.Property(e => e.Capacity)
            .IsRequired()
            .HasDefaultValue(Airplane.DefaultCapacity);
    }
}
```

#### Repository Customization
```csharp
// GameHub.EntityFrameworkCore/Repositories/AirplaneRepository.cs
public class AirplaneRepository : EfCoreRepositoryBase<GameHubDbContext, Airplane, Guid>, IAirplaneRepository
{
    public AirplaneRepository(IDbContextProvider<GameHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Airplane>> GetByModelAsync(string model)
    {
        return await DbSet
            .Where(a => a.Model == model)
            .ToListAsync();
    }
}
```

### Web Host Layer

#### Startup Configuration
```csharp
// GameHub.Web.Host/Startup/Startup.cs
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAbpDbContext<GameHubDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });

        services.AddAbpIdentity<GameHubUser, GameHubRole>()
            .AddAbpUserManager<GameHubUserManager>()
            .AddAbpRoleManager<GameHubRoleManager>()
            .AddAbpLoginManager<GameHubLoginManager>()
            .AddAbpSignInManager<GameHubSignInManager>()
            .AddAbpUserStore<GameHubUserStore>()
            .AddAbpRoleStore<GameHubRoleStore>()
            .AddAbpPermissionChecker<PermissionChecker>()
            .AddAbpPermissionManager<PermissionManager>();

        services.AddControllers();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseAbp();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
```

#### Module Configuration
```csharp
// GameHub.Web.Host/GameHubWebHostModule.cs
[DependsOn(
    typeof(GameHubApplicationModule),
    typeof(GameHubEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreModule)
)]
public class GameHubWebHostModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

## ABP Common Patterns

### Dependency Injection
```csharp
// Constructor injection
public class MyService : ITransientDependency
{
    private readonly IRepository<Airplane, Guid> _airplaneRepository;
    private readonly ISettingManager _settingManager;
    
    public MyService(IRepository<Airplane, Guid> airplaneRepository, ISettingManager settingManager)
    {
        _airplaneRepository = airplaneRepository;
        _settingManager = settingManager;
    }
}

// Lifecycles
- ITransientDependency: Created each time
- ISingletonDependency: Single instance
- IScopedDependency: Per request scope
```

### Authorization
```csharp
// Attribute-based
[AbpAuthorize(GameHubPermissions.Airplanes_Create)]
public async Task<AirplaneDto> Create(CreateAirplaneDto input)
{
    // ...
}

// Programmatic check
if (await PermissionChecker.IsGrantedAsync(GameHubPermissions.Airplanes_Edit))
{
    // Allow action
}
```

### Caching
```csharp
public class AirplaneAppService : GameHubAppServiceBase
{
    private readonly ICacheManager _cacheManager;
    
    public async Task<AirplaneDto> Get(EntityDto<Guid> input)
    {
        var cache = _cacheManager.GetCache("Airplanes");
        return await cache.GetAsync(input.Id.ToString(), () => GetFromDb(input.Id));
    }
}
```

### Unit of Work
```csharp
// Automatic UOW in application services
public class AirplaneAppService : GameHubAppServiceBase
{
    // UOW automatically starts and commits
    public async Task<AirplaneDto> Create(CreateAirplaneDto input)
    {
        var airplane = ObjectMapper.Map<Airplane>(input);
        await _airplaneRepository.InsertAsync(airplane);
        // UOW commits here
    }
}
```

### Background Jobs
```csharp
// GameHub.Core/Airplanes/jobs/CleanupOldAirplanesJob.cs
public class CleanupOldAirplanesJob : BackgroundJob<NullJobArgs>, ITransientDependency
{
    private readonly IRepository<Airplane, Guid> _airplaneRepository;
    
    public CleanupOldAirplanesJob(IRepository<Airplane, Guid> airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }
    
    public override void Execute(NullJobArgs args)
    {
        var oldAirplanes = _airplaneRepository.GetAll()
            .Where(a => a.CreationTime < DateTime.Now.AddYears(-5))
            .ToList();
            
        foreach (var airplane in oldAirplanes)
        {
            _airplaneRepository.Delete(airplane);
        }
    }
}
```

### SignalR Integration
```csharp
// GameHub.Web.Host/SignalR/AirplaneHub.cs
public class AirplaneHub : AbpHubBase
{
    public async Task NotifyAirplaneChange(AirplaneDto airplane)
    {
        await Clients.All.SendAsync("AirplaneChanged", airplane);
    }
}

// In application service
public class AirplaneAppService : GameHubAppServiceBase
{
    private readonly IHubContext<AirplaneHub> _hubContext;
    
    public async Task<AirplaneDto> Create(CreateAirplaneDto input)
    {
        var airplane = ObjectMapper.Map<Airplane>(input);
        await _airplaneRepository.InsertAsync(airplane);
        
        var airplaneDto = ObjectMapper.Map<AirplaneDto>(airplane);
        await _hubContext.Clients.All.SendAsync("AirplaneChanged", airplaneDto);
        
        return airplaneDto;
    }
}
```

## Excel Export

```csharp
// GameHub.Application/Airplanes/Exporting/AirplaneListExcelExporter.cs
public class AirplaneListExcelExporter : IAirplaneListExcelExporter, ITransientDependency
{
    private readonly List<Airplane> _airplanes;
    
    public AirplaneListExcelExporter(List<Airplane> airplanes)
    {
        _airplanes = airplanes;
    }
    
    public FileDto ExportToFile()
    {
        return CreateExcelPackage(
            "Airplanes.xlsx",
            excelPackage =>
            {
                var sheet = excelPackage.Workbook.Worksheets.Add(L("Airplanes"));
                sheet.OutLineApplyStyle = true;
                
                AddHeader(sheet);
                AddObjects(sheet, _airplanes);
            });
    }
    
    private void AddHeader(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = L("Name");
        sheet.Cells[1, 2].Value = L("Model");
        sheet.Cells[1, 3].Value = L("Capacity");
    }
    
    private void AddObjects(ExcelWorksheet sheet, List<Airplane> airplanes)
    {
        for (var i = 0; i < airplanes.Count; i++)
        {
            sheet.Cells[i + 2, 1].Value = airplanes[i].Name;
            sheet.Cells[i + 2, 2].Value = airplanes[i].Model;
            sheet.Cells[i + 2, 3].Value = airplanes[i].Capacity;
        }
    }
}
```

## Database Migrations

```csharp
// Create migration
dotnet ef migrations add AddAirplanesTable --project GameHub.EntityFrameworkCore

// Apply migration
dotnet ef database update --project GameHub.EntityFrameworkCore

// Seed data in Migrator project
public class InitialHostDbBuilder
{
    private readonly GameHubDbContext _context;
    
    public InitialHostDbBuilder(GameHubDbContext context)
    {
        _context = context;
    }
    
    public void Create()
    {
        _context.Airplanes.AddRange(new[]
        {
            new Airplane { Name = "Boeing 747", Model = "747-400", Capacity = 400 },
            new Airplane { Name = "Airbus A320", Model = "A320neo", Capacity = 180 }
        });
        
        _context.SaveChanges();
    }
}
```

## Testing

```csharp
// GameHub.Tests/Airplanes/AirplaneAppService_Tests.cs
public class AirplaneAppService_Tests : GameHubTestBase
{
    private readonly IAirplaneAppService _airplaneAppService;
    
    public AirplaneAppService_Tests()
    {
        _airplaneAppService = Resolve<IAirplaneAppService>();
    }
    
    [Fact]
    public async Task Should_Create_Airplane()
    {
        // Arrange
        var input = new CreateAirplaneDto
        {
            Name = "Test Airplane",
            Model = "Test Model",
            Capacity = 200
        };
        
        // Act
        var result = await _airplaneAppService.Create(input);
        
        // Assert
        result.Name.ShouldBe("Test Airplane");
        result.Model.ShouldBe("Test Model");
        result.Capacity.ShouldBe(200);
    }
}
```

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=EafGameHubDb;Trusted_Connection=True"
  },
  "App": {
    "WebSiteRootAddress": "http://localhost:62134/"
  },
  "Authentication": {
    "JwtBearer": {
      "SecurityKey": "GameHub_C421AAEE0D114E9C",
      "Issuer": "GameHub",
      "Audience": "GameHub"
    }
  }
}
```

## Common Issues and Solutions

### Circular Dependency
- Use property injection with `Lazy<T>`
- Split services into smaller, focused services
- Use `IObjectResolver` for lazy resolution

### Performance Issues
- Use caching for frequently accessed data
- Optimize EF Core queries with `.AsNoTracking()`
- Use projections instead of loading full entities
- Consider background jobs for long-running tasks

### Multi-Tenancy Issues
- Always check tenant context before data access
- Use data filters appropriately
- Test both host and tenant scenarios

## File Naming Conventions

- Entities: `EntityName.cs` in `Core/EntityName/`
- DTOs: `EntityNameDto.cs` in `Application/EntityName/Dtos/`
- Application Services: `EntityNameAppService.cs` in `Application/EntityName/`
- Interfaces: `IEntityName.cs` in same folder as implementation
- Migrations: `YYYYMMDDHHMMSS_Description.cs` in `EntityFrameworkCore/Migrations/`
- Use PascalCase for class names
- Use camelCase for method parameters

## When in Doubt

- Follow ABP conventions over custom patterns
- Check ABP documentation at https://aspnetboilerplate.com/Pages/Documents
- Use dependency injection properly
- Keep services small and focused
- Test thoroughly before committing
- Maintain consistency with existing EAF patterns
- Use AutoMapper for object mapping
- Use repositories for data access
