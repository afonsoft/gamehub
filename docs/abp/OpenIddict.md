# https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict

## OpenIddict

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/OpenIddict.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#introduction)

[OpenIddict](https://github.com/openiddict/openiddict-core) aims at providing a versatile solution to implement OpenID Connect client, server and token validation support in any ASP.NET Core 2.1 (and higher) application. ASP.NET 4.6.1 (and higher) applications are also fully supported thanks to a native Microsoft.Owin 4.2 integration.

#### Startup Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#startup-project)

This document assumes that you have already created an ASP.NET Core based
project (including Module Zero) from the [startup templates](https://aspnetboilerplate.com/Templates) and
have set it up to work. We created an [ASP.NET Core MVC startup\\
project](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core) for this demonstration.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#installation)

There are 3 NuGet packages:

- [**Abp.ZeroCore.OpenIddict**](https://www.nuget.org/packages/Abp.ZeroCore.OpenIddict)
is the main integration package.
- [**Abp.ZeroCore.OpenIddict.EntityFrameworkCore**](https://www.nuget.org/packages/Abp.ZeroCore.OpenIddict.EntityFrameworkCore)
is the storage provider for EF Core.
- [**Abp.AspNetCore.OpenIddict**](https://www.nuget.org/packages/Abp.AspNetCore.OpenIddict)
is the package for ASP.NET Core.

Install **Abp.ZeroCore.OpenIddict** package to your Core project and add a module dependency to `AbpZeroCoreOpenIddictModule`.

Install **Abp.AspNetCore.OpenIddict** package to your Web project and add a module dependency to `AbpZeroCoreOpenIddictEntityFrameworkCoreModule`.

Install **Abp.ZeroCore.OpenIddict.EntityFrameworkCore** package to your EntityFrameworkCore project and add a module dependency to `AbpAspNetCoreOpenIddictModule`.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#configuration)

Configuring and using OpenIddict with Abp.ZeroCore is similar to
independently using OpenIddict. You should read its [own\\
documentation](https://documentation.openiddict.com/index.html) to better understand how

#### EntityFrameworkCore Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#entityframeworkcore-project)

You need to implement `IOpenIddictDbContext` in your DbContext class. This will add Entities required by OpenIddict to your DbContext.

After this, call `modelBuilder.ConfigureOpenIddict();` in the `OnModelCreating` method of your DbContext.

Then, create repositories inherited from below classes;

- `EfCoreOpenIddictApplicationRepository`
- `EfCoreOpenIddictAuthorizationRepository`
- `EfCoreOpenIddictScopeRepository`
- `EfCoreOpenIddictTokenRepository`

#### Web Project [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/OpenIddict\#web-project)

In your web project, create Controllers inherited from generic controllers defined in ASP.NET Boilerplate;

- `AuthorizeController`
- `TokenController`
- `UserInfoController`

Create a static class to Configure `OpenIddict` in your Web project and call it in Startup.cs;

Copy

```csharp
public static class OpenIddictRegistrar
    {
        public static void Register(
            IServiceCollection services,
            IConfigurationRoot configuration,
            Action<OpenIddictCoreOptions> setupOptions)
        {
            services.Configure<AbpOpenIddictClaimsPrincipalOptions>(options =>
            {
                options.ClaimsPrincipalHandlers.Add<AbpDefaultOpenIddictClaimsPrincipalHandler>();
            });

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(builder =>
                {
                    builder
                        .SetDefaultApplicationEntity<OpenIddictApplicationModel>()
                        .SetDefaultAuthorizationEntity<OpenIddictAuthorizationModel>()
                        .SetDefaultScopeEntity<OpenIddictScopeModel>()
                        .SetDefaultTokenEntity<OpenIddictTokenModel>();

                    builder
                        .AddApplicationStore<AbpOpenIddictApplicationStore>()
                        .AddAuthorizationStore<AbpOpenIddictAuthorizationStore>()
                        .AddScopeStore<AbpOpenIddictScopeStore>()
                        .AddTokenStore<AbpOpenIddictTokenStore>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the token endpoint.
                    options.SetAuthorizationEndpointUris("connect/authorize", "connect/authorize/callback")
                        .SetTokenEndpointUris("connect/token")
                        .SetUserinfoEndpointUris("connect/userinfo");

                    // Enable the client credentials flow.
                    options.AllowClientCredentialsFlow();
                    options.AllowPasswordFlow();
                    options.AllowAuthorizationCodeFlow();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableVerificationEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();

                    options.DisableAccessTokenEncryption();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });
        }
    }
```

You also need to fill data into OpenIddict tables by following its own [documentation](https://documentation.openiddict.com/)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe