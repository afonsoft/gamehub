# https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration

## EF MySql Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/EF-MySql-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#introduction)

While our default templates are designed to work with SQL Server, you can
easily modify them to work with MySql. In order to do that, you need to
follow these steps.

#### Download Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#download-project)

Go to [http://aspnetboilerplate.com/Templates](https://aspnetboilerplate.com/Templates) and download a new
project. Select ASP.NET MVC 5.x tab and don't forget to select Entity
Framework.

#### Install MySql.Data.Entity/MySql.Data.EntityFramework [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#install-mysql-data-entity-mysql-data-entityframework)

Into your **.EntityFramework** and **.Web** projects you need to install
[MySql.Data.EntityFramework](https://www.nuget.org/packages/MySql.Data.EntityFramework/)
NuGet package, if you use a MySql Connector/NET 8.0.x or higher;
[MySql.Data.Entity](https://www.nuget.org/packages/MySql.Data.Entity/)
NuGet package, if you use an older MySql Connector/NET.

#### Make changes in app.config/web.config file(s) [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#make-changes-in-app-config-web-config-file-s-)

Note: installing the NuGet package into your **.Web** project should make the
necessary changes in your web.config file.
By the way, check it has been modified in this way:

- the `<entityFramework>` tag to:
  - For older MsSql connector: `<entityFramework codeConfigurationType="MySql.Data.Entity.MySqlEFConfiguration, MySql.Data.Entity">`
  - For MsSql connector/NET 8.0+: `<entityFramework codeConfigurationType="MySql.Data.EntityFramework.MySqlEFConfiguration, MySql.Data.EntityFramework">`
- the `<provider>` tag to:
  - For older MsSql connector: `<provider invariantName="MySql.Data.MySqlClient" type="MySql.Data.MySqlClient.MySqlProviderServices, MySql.Data.Entity" />`
  - For MsSql connector/NET 8.0+: `<provider invariantName="MySql.Data.MySqlClient" type="MySql.Data.MySqlClient.MySqlProviderServices, MySql.Data.EntityFramework" />`
- You need to change your connection string in the web.config file in
order to work with your MySql database. An example connection string
would be:


#### Configure the EF migration [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#configure-the-ef-migration)

Open your DbContext's configuration class (Configuration.cs) and place
the following code in it's constructor

Copy

```
SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
```

#### Re-generate the migrations [Anchor](https://aspnetboilerplate.com/Pages/Documents/EF-MySql-Integration\#re-generate-the-migrations)

If you choose "Include login, register, user, role and tenant management pages" while downloading your startup
template, there will be some migration files included in your project.
These files are generated for Sql Server. Delete all the migration files
in your **.EntityFramework** project under the Migrations folder. Migration
files start with a timestamp. A migration file name should look like this
"201506210746108\_AbpZero\_Initial"

After deleting all the migration files, select your **.Web** project as the
startup project, open Visual Studio's Package Manager Console and select
the **.EntityFramework** project as the default project in the Package Manager
Console. Then run the following command to add a migration for MySql.

Copy

```
Add-Migration "AbpZero_Initial"
```

Now you can create your database using the following command

Copy

```
Update-Database
```

That's it! You can now run your project with MySql.

Additional information are available in [this post](https://davidsekar.com/asp-net/mysql-error-the-provider-did-not-return-a-providermanifesttoken).

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe