# https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template

## Startup Template

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Startup-Template.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#introduction)

The easiest way of starting a new project using ABP with **ASP.NET MVC 5.x (and optionally AngularJS frontend)** is to create a template on the [download page](https://aspnetboilerplate.com/Templates).

After creating and downloading your project:

- Open your solution in Visual Studio 2017 v15.3.5+.
- Select the ' **Web**' project as the startup project.
- Open the Package Manager Console, select the ' **EntityFramework**' project
as the **Default project** and run EntityFramework's
' **Update-Database**' command. This will create the database. You
can then change the **connection string** in the web.config.
- Run the application. The default username is ' **admin**' and the password is
' **123qwe**'.

Be sure you have installed Typescript 2.0+ in Visual Studio
because the Abp.Web.Resources NuGet package comes with d.ts and it requires
Typescript 2.0+.

In this template, **multi-tenancy is enabled by default**. You can
disable it in the Core project's module class if you don't need it.

### Token-Based Authentication [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#token-based-authentication)

The startup template uses cookie-based authentication for browsers. However,
if you want to consume Web APIs or application services (those that are
exposed via the [dynamic web api](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Web-API)) from a
mobile application, you probably want a token-based authentication
mechanism. The startup template includes the infrastructure for bearer-token
authentication. The **AccountController** in the **.WebApi** project contains
an **Authenticate** action to get the token. We can then use the token for the
next requests.

Here **Postman** (a chrome extension) is used to demonstrate
requests and responses.

#### Authentication [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#authentication)

Just send a **POST** request to
**http://localhost:6634/api/Account/Authenticate** with the
**Context-Type="application/json"** header as shown below:

![Request for token](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/startup-mvc5-postman-authanticate.png)

We sent a **JSON request body** which includes a **userNameOrEmailAddress** and
**password**. **tenancyName** should also be sent for **tenant** users.
As seen above, the **result** property of the returning JSON contains the token.
We can save it and use it for the next requests.

#### Use API [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#use-api)

After we authenticate and get the **token**, we can use it to call
**authorized** actions. All **application services** can be
used remotely. For example, we can use the **user service** to get a
**list of roles**:

![Authorization via token](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/startup-mvc5-postman-getroles.png)

We just made a **POST** request to
**http://localhost:6634/api/services/app/user/GetRoles** with
**Content-Type="application/json"** and **Authorization="Bearer**
**_your-_** **_auth-token_** **"** headers. The request body was empty **{}**.
For the most part, request and response bodies will be different for each API.

Almost all operations available on the UI are also available as a Web API,
since the UI uses the same Web API, and can be easily consumed.

### Migrator Console Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#migrator-console-application)

The startup template includes a tool, Migrator.exe, to easily migrate your
databases. You can run this application to create/migrate the host and
tenant databases.

![Database Migrator](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/database-migrator.png)

This application gets the host connection string from it's **own**
**appsettings.json file**. In the beginning, it will be the
same in the appsettings.json in the .Web.Host project.
Be sure that the connection string
in the config file is the database you want. After getting the **host** **connection string**, it first creates the host database and applies
migrations if they don't already exist. It then gets the connection strings of the
tenant databases and runs migrations against those databases. It skips a
tenant if it does not have a dedicated database or its database has already
been migrated by another tenant (for shared databases between multiple
tenants).

You can use this tool on the development or on the production environment to
migrate databases on deployment instead of EntityFramework's own
tooling (which requires some configuration and can only work for a single
database/tenant in one run).

### Unit Testing [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#unit-testing)

The startup template includes the test infrastructure setup and a few tests
under the .Test project. You can check them and write similar tests
easily. They are actually integration tests rather than unit tests,
since they test your code with all the ASP.NET Boilerplate infrastructure
(including validation, authorization, unit of work...).

### Source Code [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template\#source-code)

This template is developed as an open source project and is available for free on GitHub:
[https://github.com/aspnetboilerplate/module-zero-template](https://github.com/aspnetboilerplate/module-zero-template)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe