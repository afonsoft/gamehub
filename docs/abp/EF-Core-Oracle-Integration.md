# https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration

## EF Core Oracle Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/EF-Core-Oracle-Integration.md)

In this document

### Download Starter Template [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#download-starter-template)

Download the starter template with **ASP.NET Core** and **Entity Framework Core** to integrate Oracle.
[Multi-page template with **ASP.NET Core** \+ **.NET Core Framework** \+ **Authentication**](https://aspnetboilerplate.com/Templates)
will be explained in this document.

### Install [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#install)

Install the [`Oracle.EntityFrameworkCore`](https://www.nuget.org/packages/Oracle.EntityFrameworkCore/) NuGet package to the \* **.EntityFrameworkCore** project.

Delete `Microsoft.EntityFrameworkCore.SqlServer` from \* **.EntityFrameworkCore** project because it will not be used anymore.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#configuration)

Some configuration and workarounds are needed to use Oracle with ASP.NET Core and Entity Framework Core.

#### Configure DbContext [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#configure-dbcontext)

Replace `YourProjectNameDbContextConfigurer.cs` with the following lines

Copy

```c
public static class PostgreSqlDemoDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<PostgreSqlDemoDbContext> builder, string connectionString)
    {
        builder.UseOracle(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<PostgreSqlDemoDbContext> builder, DbConnection connection)
    {
        builder.UseOracle(connection);
    }
 }
```

#### Configure connection string [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#configure-connection-string)

Change the connection string to your PostgreSQL connection in \* **.Web.Mvc/appsettings.json** and \* **.Migrator/appsettings.json** projects. For example:

Copy

```js
{
  "ConnectionStrings": {
    "Default": "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = XE))); User Id = system; Password = 123qwe;"
  },
  ...
}
```

### Configure IsolationLevel for UnitOfWork [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#configure-isolationlevel-for-unitofwork)

In order to use Oracle with EF Core, IsolationLevel of the UnitOfWork must be set to ReadCommitted as shown below. You can do this in the PreInitialize method of the **Core module** of your app.

Copy

```c
Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;
```

### Create Database [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-Oracle-Integration\#create-database)

Remove all migration classes(including `DbContextModelSnapshot`) under **\*.EntityFrameworkCore/Migrations** folder.
because `Oracle.EntityFrameworkCore` will add some of its own configuration to work with Entity Framework Core.

Now it's ready to build database.

- Open a terminal or command prompt and go to root directory of **\*.EntityFrameworkCore** project.
- Run `dotnet ef migrations add Initial_Migration` command.
- Place the code below to PreInitialize method of the Migrator project's Core module;

Copy

```c
Configuration.UnitOfWork.IsTransactional = false;
```

- Then, run the Migrator project to create and seed your database.

The Oracle integration is now complete. You can now run your project with Oracle.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe