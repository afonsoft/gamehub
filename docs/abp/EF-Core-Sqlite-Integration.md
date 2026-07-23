# https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration

## EF Core Sqlite Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/EF-Core-Sqlite-Integration.md)

In this document

### Download Starter Template [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#download-starter-template)

Download a starter template with **ASP.NET Core** and **Entity Framework Core** to integrate SQLite.
[Multi-page template with **ASP.NET Core 2.x** \+ **.NET Core Framework** \+ **Authentication**](https://aspnetboilerplate.com/Templates)
will be explained in this document.

### Install [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#install)

Install the [`Microsoft.EntityFrameworkCore.Sqlite`](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/) NuGet package to the **\*.EntityFrameworkCore** project.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#configuration)

Some configuration and workarounds are needed to use SQLite with ASP.NET Core and Entity Framework Core.

#### Configure DbContext [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#configure-dbcontext)

Since SQLite doesn't support multithreading, transactions should be disabled in the `SQLiteDemoEntityFrameworkModule.PreInitialize()` method.

Copy

```c
[DependsOn(\
    typeof(SQLiteDemoCoreModule),\
    typeof(AbpZeroCoreEntityFrameworkCoreModule))]
public class SQLiteDemoEntityFrameworkModule : AbpModule
{
    public override void PreInitialize()
    {
        ...
        // add this line to disable transactions
        Configuration.UnitOfWork.IsTransactional = false;
        ...
    }
}
```

Replace `YourProjectNameDbContextConfigurer.cs` with the following lines

Copy

```c
public static class SqliteDemoDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<SqliteDemoDbContext> builder, string connectionString)
    {
        builder.UseSqlite(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<SqliteDemoDbContext> builder, DbConnection connection)
    {
        builder.UseSqlite(connection);
    }
 }
```

#### Configure connection string [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#configure-connection-string)

Change the connection string to your SQLite connection in \* **.Web.Mvc/appsettings.json**. Example:

Copy

```js
{
  "ConnectionStrings": {
    "Default": "Data Source=SqliteDemoDb.db;Cache=Shared"
  },
  ...
}
```

### Create Database [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Sqlite-Integration\#create-database)

Remove all migration classes(include `DbContextModelSnapshot`) under **\*.EntityFrameworkCore/Migrations** folder before creating database.
`Microsoft.EntityFrameworkCore.Sqlite` will add some of its own configuration to work with Entity Framework Core.

Now it's ready to build the database.

- Select **\*.Web.Mvc** as the startup project.
- Open **Package Manager Console** and select the **\*.EntityFrameworkCore** project.
- Run the `add-migration Initial_Migration` command
- Run the `update-database` command

The SQLite integration is now complete. You can now run your project with SQLite.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe