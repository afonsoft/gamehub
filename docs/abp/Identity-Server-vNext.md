# https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext

## Identity Server vNext

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Identity-Server-vNext.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#introduction)

**Identity Server** changed its license model, so we suggest using [OpenIddict](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict) for new projects.

[Identity Server](http://identityserver.io/) is an open source **OpenID**
**Connect** and **OAuth 2.0** framework. It can be used to make your
application an **authentication / single sign on server**. It can also
issue **access tokens** for 3rd party clients. This document describes
how you can integrate IdentityServer4 (version **2.0+**) to your
project.

#### Startup Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#startup-project)

This document assumes that you have already created an ASP.NET Core based
project (including Module Zero) from the [startup templates](https://aspnetboilerplate.com/Templates) and
have set it up to work. We created an [ASP.NET Core MVC startup\\
project](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core) for this demonstration.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#installation)

There are two NuGet packages:

- [**Abp.ZeroCore.IdentityServer4.vNext**](https://www.nuget.org/packages/Abp.ZeroCore.IdentityServer4.vNext)
is the main integration package.
- [**Abp.ZeroCore.IdentityServer4.vNext.EntityFrameworkCore**](https://www.nuget.org/packages/Abp.ZeroCore.IdentityServer4.vNext.EntityFrameworkCore)
is the storage provider for EF Core.

Since the EF Core package already depends on the first one, you only have to
install the
[**Abp.ZeroCore.IdentityServer4.vNext.EntityFrameworkCore**](https://www.nuget.org/packages/Abp.ZeroCore.IdentityServer4.vNext.EntityFrameworkCore)
package to your project. Install it to the project that contains your
DbContext (.EntityFrameworkCore project for default templates):

Copy

```
Install-Package Abp.ZeroCore.IdentityServer4.vNext.EntityFrameworkCore
```

Then you can add a dependency to your [module](https://aspnetboilerplate.com/Pages/Module-System)
(generally, to your EntityFrameworkCore project):

Copy

```
[DependsOn(typeof(AbpZeroCoreIdentityServervNextEntityFrameworkCoreModule))]
public class MyModule : AbpModule
{
    //...
}
```

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#configuration)

Configuring and using IdentityServer4 with Abp.ZeroCore is similar to
independently using IdentityServer4. You should read its [own\\
documentation](https://identityserver4.readthedocs.io/) to better understand how
it works. In this document, we only show the additional configuration needed
to integrate it into Abp.ZeroCore.

#### Startup Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#startup-class)

In the ASP.NET Core **Startup class**, we must add IdentityServer to the
**service collection** and to the ASP.NET Core **middleware pipeline**.
Highlighted, here are the **differences** from the standard IdentityServer4 usage:

Copy

```
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //...

        services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
            .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
            .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
            .AddInMemoryClients(IdentityServerConfig.GetClients())
            .AddAbpPersistedGrants<IAbpPersistedGrantDbContext>()
            .AddAbpIdentityServer<User>();

        //...
    }

    public void Configure(IApplicationBuilder app)
    {
        //...

            app.UseJwtTokenMiddleware("IdentityBearer");
            app.UseIdentityServer();

        //...
    }
}
```

We added **services.AddIdentityServer()** just after
**IdentityRegistrar.Register(services)** and added
**app.UseJwtTokenMiddleware("IdentityBearer")** just after
**app.UseAuthentication()** in the startup project.

#### IdentityServerConfig Class [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#identityserverconfig-class)

We have used the IdentityServerConfig class to get identity resources, api
resources and clients. You can find more information about this class in
its own
[documentation](https://identityserver4.readthedocs.io/en/latest/quickstarts/1_client_credentials.html).
For the simplest case, it can be a static class like below:

Copy

```
public static class IdentityServerConfig
{
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone()
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("default-api-scope")
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new ApiResource("default-api-resource", "Default (all) API")
            {
                Scopes = { "default-api-scope" }
            }
        };
    }

    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials.Union(GrantTypes.ResourceOwnerPassword).ToList(),
                AllowedScopes = {"default-api-scope"},
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                }
            }
        };
    }
}
```

#### DbContext Changes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#dbcontext-changes)

The **AddAbpPersistedGrants()** method is used to save consent responses to
the persistent data store. In order to use it, **YourDbContext** must
implement the **IAbpPersistedGrantDbContext** interface as shown below:

Copy

```
public class YourDbContext : AbpZeroDbContext<Tenant, Role, User, YourDbContext>, IAbpPersistedGrantDbContext
{
    public DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

    public YourDbContext(DbContextOptions<YourDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigurePersistedGrantEntity();
    }
}
```

The IAbpPersistedGrantDbContext interface defines the **PersistedGrants** DbSet. We also
must call the modelBuilder. **ConfigurePersistedGrantEntity()** extension
method as shown above in order to configure EntityFramework for the
**PersistedGrantEntity**.

Note that this change in YourDbContext causes a new database migration.
So remember to use the "Add-Migration" and "Update-Database" commands to
update your database.

IdentityServer4 will continue to work even if you don't call the
AddAbpPersistedGrants<YourDbContext>() extension method, but user
consent responses will be stored in an in-memory data store in that case
(which is cleared when you restart your application!).

#### JWT Authentication Middleware [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#jwt-authentication-middleware)

If we want to authorize clients against the same application we can use the
[IdentityServer authentication\\
middleware](https://identityserver4.readthedocs.io/en/latest/topics/apis.html?highlight=UseIdentityServerAuthentication#the-identityserver-authentication-middleware)
for that.

First, install the IdentityServer4.AccessTokenValidation package from NuGet
to your project:

Copy

```
Install-Package IdentityServer4.AccessTokenValidation
```

We can then add the middleware to the Startup class as shown below:

Copy

```
services.AddAuthentication().AddIdentityServerAuthentication("IdentityBearer", options =>
{
    options.Authority = "https://localhost:44388/";
    options.RequireHttpsMetadata = false;
});
```

We added this just after the **services.AddIdentityServer()** line in the startup
project.

### Testing [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server-vNext\#testing)

Our identity server is now ready to get requests from clients. We can
create a console application to make requests and get responses.

- Create a new **Console Application** inside your solution.
- Add the **IdentityModel** NuGet package to the console application. This
package is used to create clients for OAuth endpoints.

While the **IdentityModel** NuGet package is enough to create a client and
consume your API, we need to use the API in a more type safe way: We
will convert incoming data to DTOs which are returned by the application services.

- Add a reference to the **Application** layer from the console application.
This will allow us to use the same DTO classes returned by the
application layer on the client-side.
- Add the **Abp.Web.Common** NuGet package. This will allow us to use
the AjaxResponse class defined in ASP.NET Boilerplate. Otherwise,
we will have to deal with raw JSON strings to handle the server response.

Change Program.cs as shown below:

Copy

```
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Web.Models;
using IdentityModel.Client;
using Newtonsoft.Json;

namespace IdentityServerIntegrationDemo.ConsoleApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunDemoAsync().Wait();
            Console.ReadLine();
        }

        public static async Task RunDemoAsync()
        {
            var accessToken = await GetAccessTokenViaOwnerPasswordAsync();
            await GetUsersListAsync(accessToken);
        }

        private static async Task<string> GetAccessTokenViaOwnerPasswordAsync()
        {
            var disco =  await new HttpClient().GetDiscoveryDocumentAsync("https://localhost:44388");

            var tokenClient = new HttpClient();
            tokenClient.DefaultRequestHeaders.Add("Abp.TenantId", "1");
            var tokenResponse = await tokenClient.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                UserName = "admin",
                Password = "123qwe"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Error: ");
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);

            return tokenResponse.AccessToken;
        }

        private static async Task GetUsersListAsync(string accessToken)
        {
            var client = new HttpClient();
            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("https://localhost:44388/api/services/app/user/GetAll");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var ajaxResponse = JsonConvert.DeserializeObject<AjaxResponse<PagedResultDto<UserListDto>>>(content);
            if (!ajaxResponse.Success)
            {
                throw new Exception(ajaxResponse.Error?.Message ?? "Remote service throws exception!");
            }

            Console.WriteLine();
            Console.WriteLine("Total user count: " + ajaxResponse.Result.TotalCount);
            Console.WriteLine();
            foreach (var user in ajaxResponse.Result.Items)
            {
                Console.WriteLine($"### UserId: {user.Id}, UserName: {user.UserName}");
                Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
            }
        }
    }

    internal class UserListDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }
}
```

Before running this application, ensure that your web project set up and
running, because this console application will make a request to the web
application. Also, ensure that the requesting port (44388) is the same
as your web application.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe