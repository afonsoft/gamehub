# Getting Started - EAF API Template

## Overview

The EAF (Enterprise Application Framework) API Template is a complete ASP.NET Core Web API application built with .NET 10.0, C# 14, and ASP.NET Boilerplate. It provides a solid foundation for building enterprise-grade REST APIs with features like multi-tenancy, authentication, authorization, background jobs, and more.

## Prerequisites

Before running the EAF API template, ensure you have the following installed:

- **.NET 10.0 SDK**: The latest .NET SDK
- **SQL Server**: SQL Server 2016 or later (or SQL Server Express)
- **Visual Studio 2022** or **VS Code**: For development
- **Git**: For version control

## Installation

### 1. Clone or Copy the Template

Copy the `Templates/Api` folder to your desired location.

### 2. Restore NuGet Packages

Navigate to the solution directory and restore packages:

```bash
cd Templates/Api
dotnet restore
```

This will restore all required NuGet packages for all projects in the solution.

### 3. Configure Connection Strings

Edit the connection strings in `appsettings.json` in the `GameHub.Web.Host` project:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=EafProjectName;Trusted_Connection=True;MultipleActiveResultSets=true",
    "Hangfire": "Server=localhost;Database=EafProjectNameHangfire;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Update App Configuration

Edit `appsettings.json` in `GameHub.Web.Host` to configure application settings:

```json
{
  "App": {
    "WebSiteRootAddress": "http://localhost:21021/"
  }
}
```

## Database Setup

### 1. Create Database

The template uses Entity Framework Core migrations to create and update the database schema.

### 2. Run Migrations

Navigate to the `GameHub.Web.Host` project and run migrations:

```bash
cd src/GameHub.Web.Host
dotnet ef database update
```

This will create the database schema and seed initial data.

### 3. Seed Initial Data

The template includes a seed data system that creates:

- Default admin user (admin/123qwe)
- Default tenant
- Default roles
- Default permissions
- Default languages

## Running the Application

### Development Mode

Using Visual Studio:

1. Open `Eaf.ApiWithSrc.sln` or `GameHub.sln`
2. Set `GameHub.Web.Host` as the startup project
3. Press F5 or click "Start" to run the application

Using Command Line:

```bash
cd src/GameHub.Web.Host
dotnet run
```

The API will be available at `http://localhost:21021`

### Production Mode

Build the application for production:

```bash
dotnet build --configuration Release
```

Or publish the application:

```bash
dotnet publish --configuration Release --output ./publish
```

Run the published application:

```bash
cd publish
dotnet GameHub.Web.Host.dll
```

## Docker Deployment

O Dockerfile do template está em `Templates/Api/Dockerfile` e deve ser buildado a partir da raiz do repositório, pois o template referencia os projetos-fonte do EAF em `src/`.

### Build Docker Image

```bash
cd /path/to/EAF
docker build -t eaf-api -f Templates/Api/Dockerfile .
```

### Run Docker Container

```bash
docker run -d -p 8001:8001 \
  -e ConnectionStrings__Default="Server=sqlserver;Database=EafProjectName;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Encrypt=false" \
  -e Database__Provider=SqlServer \
  -e Hangfire__IsEnabled=false \
  -e SqlServerCache__IsEnabled=false \
  -e RedisCache__IsEnabled=false \
  -e App__CorsOrigins=* \
  eaf-api
```

### Docker Compose

Use o `docker-compose.yml` em `Templates/Api`:

```bash
cd Templates/Api
ConnectionStrings__Default="..." docker-compose up --build
```

## Project Structure

```
src/
├── GameHub.Application/    # Application layer (Application Services, DTOs)
├── GameHub.Core/            # Domain layer (Entities, Value Objects, Interfaces)
├── GameHub.EntityFrameworkCore/  # Data access layer (DbContext, Repositories)
├── GameHub.Migrator/        # Database migration tool
└── GameHub.Web.Host/        # Web API host (Controllers, Startup)
```

## API Endpoints

The API exposes the following main endpoints:

- **Swagger UI**: `http://localhost:21021/swagger` - Interactive API documentation
- **Health Check**: `http://localhost:21021/health` - Application health status
- **API Endpoints**: All API endpoints are prefixed with `/api/services/app`

### Authentication

The API uses token-based authentication:

1. Login endpoint: `POST /api/TokenAuth/Authenticate`
2. Returns an access token
3. Include token in Authorization header: `Bearer {token}`

### Example API Call

```bash
# Login to get token
curl -X POST http://localhost:21021/api/TokenAuth/Authenticate \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"admin","password":"123qwe"}'

# Use token to call API
curl -X GET http://localhost:21021/api/services/app/User/GetAll \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Configuration

### Environment Variables

You can override configuration using environment variables:

```bash
export ConnectionStrings__Default="Server=localhost;Database=EafProjectName;Trusted_Connection=True"
export App__WebSiteRootAddress="http://localhost:21021/"
```

### appsettings.json

The main configuration file includes:

- Connection strings
- Application settings
- Authentication settings
- Email settings
- External login provider settings
- Azure Active Directory settings
- LDAP settings

## Development Scripts

Available dotnet CLI commands:

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/GameHub.Web.Host

# Add migration
dotnet ef migrations add AddNewTable --project src/GameHub.EntityFrameworkCore

# Update database
dotnet ef database update --project src/GameHub.EntityFrameworkCore
```

## Troubleshooting

### Common Issues

**1. Database Connection Error**

- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure SQL Server allows remote connections
- Verify SQL Server authentication mode (Windows vs SQL Server)

**2. Migration Errors**

- Ensure you're in the correct project directory
- Check that Entity Framework Core tools are installed:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Verify the connection string is correct

**3. Port Already in Use**

- Change the port in `launchSettings.json` in `GameHub.Web.Host`
- Or kill the process using the port:
  ```bash
  netstat -ano | findstr :21021
  taskkill /PID <PID> /F
  ```

**4. Authentication Token Issues**

- Verify the token expiration time in appsettings.json
- Check that the token is being sent in the Authorization header
- Ensure the user account is active

**5. CORS Issues**

- Configure CORS in `Startup.cs`:
  ```csharp
  app.UseCors(options =>
  {
      options.WithOrigins("http://localhost:4200")
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
  });
  ```

## Default Credentials

After running the application for the first time, you can log in with:

- **Username**: admin
- **Password**: 123qwe

**Important**: Change the default password immediately after first login.

## Next Steps

After successfully running the application:

1. Read the [Modules Documentation](MODULES.md) for detailed module structure
2. Review the [Implementations Documentation](IMPLEMENTATIONS.md) for implementation patterns
3. Explore the Swagger UI at `http://localhost:21021/swagger`
4. Review the code in `GameHub.Application` to understand the application layer
5. Review the code in `GameHub.Core` to understand the domain layer
6. Review the code in `GameHub.EntityFrameworkCore` to understand the data access layer

## Additional Resources

- [ASP.NET Boilerplate Documentation](https://aspnetboilerplate.com/Pages/Documents)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [Swagger/OpenAPI Documentation](https://swagger.io/docs/)
