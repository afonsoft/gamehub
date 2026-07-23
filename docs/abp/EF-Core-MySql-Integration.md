# https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration

## EF Core MySql Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/EF-Core-MySql-Integration.md)

In this document

### Download Starter Template [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#download-starter-template)

Download the starter template with **ASP.NET Core** and **Entity Framework Core** to integrate MySQL.
[Multi-page template with **ASP.NET Core 2.x** \+ **.NET Core Framework** \+ **Authentication**](https://aspnetboilerplate.com/Templates)
will be explained in this document.

### Getting Started [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#getting-started)

There are two Entity Framework Core providers for MySQL that are mentioned in the Micrososft Docs. One of them is the
[Official MySQL EF Core Database Provider](https://docs.microsoft.com/en-us/ef/core/providers/mysql/) and the
other is [Pomelo EF Core Database Provider for MySQL](https://docs.microsoft.com/en-us/ef/core/providers/pomelo/).

> **NOTE:** The official provider [`MySql.Data.EntityFrameworkCore`](https://www.nuget.org/packages/MySql.Data.EntityFrameworkCore) [supports EF Core 2.0 after version 6.10.5](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html), but you have to make a lot of changes when using it. So, the Pomelo EF Core Database Provider will be used in this example instead.
>
> Related issue: https://github.com/aspnetboilerplate/aspnetboilerplate/issues/4007
>
> Related guide: https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html

### Install [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#install)

Install the [`Pomelo.EntityFrameworkCore.MySql`](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/) NuGet package to the \* **.EntityFrameworkCore** project.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#configuration)

#### Configure DbContext [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#configure-dbcontext)

Replace `YourProjectNameDbContextConfigurer.cs` with the following lines

Copy

```c
public static class MySqlDemoDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<MySqlDemoDbContext> builder, string connectionString)
    {
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        builder.UseMySql(connectionString, serverVersion);
    }

    public static void Configure(DbContextOptionsBuilder<MySqlDemoDbContext> builder, DbConnection connection)
    {
        var serverVersion = ServerVersion.AutoDetect(connection.ConnectionString);
        builder.UseMySql(connection, serverVersion);
    }
 }
```

Some configuration and workarounds are needed to use MySQL with ASP.NET Core and Entity Framework Core.

#### Configure connection string [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#configure-connection-string)

Change the connection string to your MySQL connection in \* **.Web.Mvc/appsettings.json**. Example:

Copy

```js
{
  "ConnectionStrings": {
    "Default": "server=127.0.0.1;uid=root;pwd=1234;database=mysqldemodb"
  },
  ...
}
```

#### A workaround [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#a-workaround)

To prevent EF Core from calling `Program.BuildWebHost()` rename `BuildWebHost`. For example, change it to `InitWebHost`.
To understand why it needs to be renamed, check the following issues:

> **Reason** : [EF Core 2.0: design-time DbContext discovery changes](https://github.com/aspnet/EntityFrameworkCore/issues/9033)
>
> **Workaround** : [Design: Allow IDesignTimeDbContextFactory to short-circuit service provider creation](https://github.com/aspnet/EntityFrameworkCore/issues/9076#issuecomment-313278753)
>
> **NOTE :** If you don't rename BuildWebHost, you'll get an error running BuildWebHost method.

### Create Database [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-Core-MySql-Integration\#create-database)

Remove all migration classes (including DbContextModelSnapshot) under **\*.EntityFrameworkCore/Migrations** folder; generally, you can remove all files in that folder.
Because `Pomelo.EntityFrameworkCore.MySql` will add some of its own configurations to work with Entity Framework Core.

Now it's ready to build the database.

- Select **\*.Web.Mvc** as the startup project.
- Open **Package Manager Console** and select the **\*.EntityFrameworkCore** project.
- Run the `add-migration Initial_Migration` command
- Run the `update-database` command

The MySQL integration is now complete. You can now run your project with MySQL.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe