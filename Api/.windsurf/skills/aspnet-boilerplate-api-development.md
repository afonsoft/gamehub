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

#### Core Layer (Eaf.ProjectName.Core)
- Domain entities and value objects
- Application interfaces
- Authorization and permissions
- Localization resources
- Feature definitions
- Settings

#### Application Layer (Eaf.ProjectName.Application)
- Application services
- DTOs (Data Transfer Objects)
- Business logic
- Exporting functionality
- Job definitions

#### EntityFrameworkCore Layer (Eaf.ProjectName.EntityFrameworkCore)
- DbContext implementation
- Entity configurations
- Repository implementations
- Database migrations
- Seed data

#### Web Host (Eaf.ProjectName.Web.Host)
- Startup configuration
- API controllers
- Authentication and authorization
- SignalR hubs
- Swagger UI

## ABP N-Layer Architecture

### Domain Layer (Core)

#### Entities
```csharp
// Entity in Eaf.ProjectName.Core/Airplanes/
public class Airplane : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Model { get; set; }
    public int Capacity { get; set; }
}
```

#### Permissions
```csharp
// Eaf.ProjectName.Core/Application/Authorization/ProjectNamePermissions.cs
public static class ProjectNamePermissions
{
    public const string Airplanes = "Pages.Airplanes";
    public const string Airplanes_Create = "Pages.Airplanes.Create";
    public const string Airplanes_Edit = "Pages.Airplanes.Edit";
    public const string Airplanes_Delete = "Pages.Airplanes.Delete";
}

// Permission provider
public class ProjectNameAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        var airplanes = context.GetPermissionOrNull(ProjectNamePermissions.Airplanes) 
            ?? context.CreatePermission(ProjectNamePermissions.Airplanes, L("Airplanes"));
        
        airplanes.CreateChildPermission(ProjectNamePermissions.Airplanes_Create, L("Create"));
        airplanes.CreateChildPermission(ProjectNamePermissions.Airplanes_Edit, L("Edit"));
        airplanes.CreateChildPermission(ProjectNamePermissions.Airplanes_Delete, L("Delete"));
    }
}
```

#### Localization
```xml
<!-- Eaf.ProjectName.Core/Application/Localization/ProjectName/ProjectName.xml -->
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
// Eaf.ProjectName.Application/Airplanes/IAirplaneAppService.cs
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
// Eaf.ProjectName.Application/Airplanes/AirplaneAppService.cs
public class AirplaneAppService : ProjectNameAppServiceBase, IAirplaneAppService
{
    private readonly IRepository<Airplane, Guid> _airplaneRepository;

    public AirplaneAppService(IRepository<Airplane, Guid> airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    [AbpAuthorize(ProjectNamePermissions.Airplanes)]
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

    [AbpAuthorize(ProjectNamePermissions.Airplanes_Create)]
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
// Eaf.ProjectName.Application/Airplanes/Dtos/AirplaneDto.cs
public class AirplaneDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Model { get; set; }
    public int Capacity { get; set; }
}

// Eaf.ProjectName.Application/Airplanes/Dtos/CreateAirplaneDto.cs
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

// Eaf.ProjectName.Application/Airplanes/Dtos/GetAllAirplanesInput.cs
public class GetAllAirplanesInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
```

#### AutoMapper Profile
```csharp
// Eaf.ProjectName.Application/Airplanes/AirplaneAutoMapperProfile.cs
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
// Eaf.ProjectName.EntityFrameworkCore/ProjectNameDbContext.cs
public class ProjectNameDbContext : AbpDbContext
{
    public DbSet<Airplane> Airplanes { get; set; }

    public ProjectNameDbContext(DbContextOptions<ProjectNameDbContext> options)
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
// Eaf.ProjectName.EntityFrameworkCore/EntityConfigurations/AirplaneConfiguration.cs
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
// Eaf.ProjectName.EntityFrameworkCore/Repositories/AirplaneRepository.cs
public class AirplaneRepository : EfCoreRepositoryBase<ProjectNameDbContext, Airplane, Guid>, IAirplaneRepository
{
    public AirplaneRepository(IDbContextProvider<ProjectNameDbContext> dbContextProvider)
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
// Eaf.ProjectName.Web.Host/Startup/Startup.cs
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAbpDbContext<ProjectNameDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });

        services.AddAbpIdentity<ProjectNameUser, ProjectNameRole>()
            .AddAbpUserManager<ProjectNameUserManager>()
            .AddAbpRoleManager<ProjectNameRoleManager>()
            .AddAbpLoginManager<ProjectNameLoginManager>()
            .AddAbpSignInManager<ProjectNameSignInManager>()
            .AddAbpUserStore<ProjectNameUserStore>()
            .AddAbpRoleStore<ProjectNameRoleStore>()
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
// Eaf.ProjectName.Web.Host/ProjectNameWebHostModule.cs
[DependsOn(
    typeof(ProjectNameApplicationModule),
    typeof(ProjectNameEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreModule)
)]
public class ProjectNameWebHostModule : AbpModule
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
[AbpAuthorize(ProjectNamePermissions.Airplanes_Create)]
public async Task<AirplaneDto> Create(CreateAirplaneDto input)
{
    // ...
}

// Programmatic check
if (await PermissionChecker.IsGrantedAsync(ProjectNamePermissions.Airplanes_Edit))
{
    // Allow action
}
```

### Caching
```csharp
public class AirplaneAppService : ProjectNameAppServiceBase
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
public class AirplaneAppService : ProjectNameAppServiceBase
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
// Eaf.ProjectName.Core/Airplanes/jobs/CleanupOldAirplanesJob.cs
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
// Eaf.ProjectName.Web.Host/SignalR/AirplaneHub.cs
public class AirplaneHub : AbpHubBase
{
    public async Task NotifyAirplaneChange(AirplaneDto airplane)
    {
        await Clients.All.SendAsync("AirplaneChanged", airplane);
    }
}

// In application service
public class AirplaneAppService : ProjectNameAppServiceBase
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
// Eaf.ProjectName.Application/Airplanes/Exporting/AirplaneListExcelExporter.cs
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
dotnet ef migrations add AddAirplanesTable --project Eaf.ProjectName.EntityFrameworkCore

// Apply migration
dotnet ef database update --project Eaf.ProjectName.EntityFrameworkCore

// Seed data in Migrator project
public class InitialHostDbBuilder
{
    private readonly ProjectNameDbContext _context;
    
    public InitialHostDbBuilder(ProjectNameDbContext context)
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
// Eaf.ProjectName.Tests/Airplanes/AirplaneAppService_Tests.cs
public class AirplaneAppService_Tests : ProjectNameTestBase
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
    "Default": "Server=localhost;Database=EafProjectNameDb;Trusted_Connection=True"
  },
  "App": {
    "WebSiteRootAddress": "http://localhost:62134/"
  },
  "Authentication": {
    "JwtBearer": {
      "SecurityKey": "ProjectName_C421AAEE0D114E9C",
      "Issuer": "ProjectName",
      "Audience": "ProjectName"
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
