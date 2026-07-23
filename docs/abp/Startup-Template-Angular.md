# https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular

## Startup Template Angular

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Startup-Template-Angular.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#introduction)

The easiest way of starting a new project using ABP with **ASP.NET Core** and **Angular** is to create a
template on the [download page](https://aspnetboilerplate.com/Templates). After creating and downloading your project, follow the
below steps to run your application.

### ASP.NET Core Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#asp-net-core-application)

- Open your solution in **Visual Studio 2017 v15.3.5+** and **build**
the solution.
- Select the ' **Web.Host**' project as the **startup project**.
- Check the **connection string** in the **appsettings.json** file of the Web.Host project, change it if you need to.
- Open the **Package Manager Console** and run an **Update-Database** command
to create your database (ensure that the Default project is selected as
**.EntityFrameworkCore** in the Package Manager Console window).
- Run the application. It will show **swagger-ui** if it is successful:

![Swagger UI](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/swagger-ui-module-zero-core-template.png)

In this template, **multi-tenancy is enabled by default**. If you don't
need it, you can disable it in the Core project's module class.

If you have problems with running the application, close and then re-open
Visual Studio again. It sometimes fails on the first package restore.

### Angular Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#angular-application)

#### Requirements [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#requirements)

The Angular application needs the following tools installed:

- [nodejs](https://nodejs.org/en/download/) 6.9+ with npm 3.10+
- Typescript 2.0+

We used the [angular-cli](https://cli.angular.io/) to develop the Angular
application.

#### Restore Packages [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#restore-packages)

Open a command prompt, navigate to the **angular** folder which contains
the \*.sln file and run the following command to **restore the npm packages**:

Copy

```
npm install
```

Note that the npm install may show some warning messages. This is not
related to our solution and generally it's not a problem. The solution
can also configured to work with [**yarn**](https://yarnpkg.com/) and we
recommend you use it if it is available on your computer.

#### Run The Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#run-the-application)

In your opened command prompt, run the following command:

Copy

```
npm start
```

Once the application has compiled, you can go to [http://localhost:4200](http://localhost:4200/) in
your browser. Be sure that the Web.Host application is running at the same
time. When you open the application, you will see the **login page**:

![Login Page](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/module-zero-core-template-ui-login.png)

The Angular client app also has **HMR** (Hot Module Replacement) enabled.
You can use the following command (instead of npm start) to enable HMR
at development time:

Copy

```
npm run hmr
```

#### Login [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#login)

You can now login to the application using the default credentials. The default username
is ' **admin**' and the password is ' **123qwe**'. If you want to
login as a tenant, you need to first switch to that tenant on the login page. By default, there
is a tenant named "Default". Once you login successfully, you will
see a dashboard:

![Dashboard](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/module-zero-core-template-ui-home.png)

This dashboard is just for demonstration purposes and is meant to be a base for your
actual dashboard.

#### Deployment of Angular Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#deployment-of-angular-application)

We used the **angular-cli** tooling to build an Angular solution. You can use the
`ng build --prod` command to publish your project. It publishes to the **dist**
folder by default. You can then host this folder on IIS or any web
server you like.

##### Merged Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#merged-project)

If you have merged Angular UI project into ASP.NET Core project then you only need to publish your .Host project. After publish .Host project, you should copy files that are in .Host/wwwroot/dist folder to publish\_folder/wwwroot. For example: Move files in .Host/wwwroot/dist to C:\\inetpub\\wwwroot\\my-website\\wwwroot

### Solution Details & Other Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#solution-details-other-features)

#### Token-Based Authentication [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#token-based-authentication)

If you want to consume APIs/application services from a mobile
application, you can use the token based authentication mechanism just like
we do for the Angular client. The startup template includes the JwtBearer token
authentication infrastructure.

We will use **Postman** (a chrome extension) to demonstrate
requests and responses.

##### Authentication [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#authentication)

Just send a **POST** request to
**http://localhost:21021/api/TokenAuth/Authenticate** with the
**Content-Type="application/json"** header as shown below:

![Swagger UI auth](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/swagger-ui-angular-auth.png)

We sent the values **usernameOrEmailAddress** and **password**. As seen
above, the result property of the returning JSON will contain the token and expiration
time (this is 24 hours by default and can be configured). We can save
it and use it for the next requests.

##### About Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#about-multi-tenancy)

The **API will work as host users by default**. You can send the **Abp.TenantId**
header value to work with a specified tenant. It's an integer value and by default is
1 for the default tenant.

##### Using The API [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#using-the-api)

After we authenticate and get the **token**, we can use it to call any
**authorized** action. All **application services** can be
used remotely. For example, we can use the **User service** to get a
**list of users**:

![Using API](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/swagger-ui-angular-api-v2.png)

We made a **GET** request to
**http://localhost:21021/api/services/app/user/getAll** with
**Content-Type="application/json"** and \*\*Authorization="Bearer
\*your-\*\*\* **_auth-token_** **"**. All the functionality available on the UI is
also available as the API.

#### Migrator Console Application [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#migrator-console-application)

The startup template includes a tool, Migrator.exe, to easily migrate your
databases. You can run this application to create/migrate the host and
tenant databases.

![Database Migrator](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/database-migrator.png)

This application gets the host connection string from its **own**
**appsettings.json file**. In the beginning, it will be the same as the appsettings.json
in the .Web.Host project. Be sure that the connection string
in the config file is the database you want. After getting the **host** **connection string**, it first creates the host database and then applies the
migrations if they don't already exist. It then gets the connection strings of
the tenant databases and runs the migrations for those databases. It skips a
tenant if it does not have a dedicated database or if the database has already
been migrated by another tenant (for databases shared between multiple tenants).

You can use this tool on the development or production environment to
migrate the databases on deployment instead of using EntityFramework's own
tooling (which requires some configuration and can work only for a single
database/tenant in one run).

#### Unit Testing [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#unit-testing)

The startup template includes the test infrastructure setup and a few tests
under the .Test project. You can check them and write similar tests
easily. Actually, they are integration tests rather than unit tests
since they test your code with all of ASP.NET Boilerplate's infrastructure
(including validation, authorization, unit of work...).

### Running on Docker [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#running-on-docker)

The startup template includes necesary files for building docker images and running those images in docker.

#### Building Docker images [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#building-docker-images)

In order to build docker image, open the command prompt, go to `aspnet-core/build` folder and run `build-with-ng.ps1` script. This script will build `abp/host` and `abp/ng` docker images.
The default `abp/host` image is designed to use your local SQL Server, so don't foget to set `ConnectionStrings__Default` in `aspnet-core/docker/ng/docker-compose.yml` before building the docker image. In order to connect your local SQL Server, you need to use your local IP address in the connection string.
A sample connection string is `ConnectionStrings__Default: "Server=192.168.1.42; Database=AbpProjectNameDb; User=sa; Password=123qwe;TrustServerCertificate=True;"`

#### Running the project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#running-the-project)

After creating the docker image, you can go to `aspnet-core/docker/ng` folder and run `up.ps1` script to run the docker image.

### Source Code [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular\#source-code)

This template is developed as an open source project and is available for free on GitHub:
[https://github.com/aspnetboilerplate/module-zero-core-template](https://github.com/aspnetboilerplate/module-zero-core-template)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe