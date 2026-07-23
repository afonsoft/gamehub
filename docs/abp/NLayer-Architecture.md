# https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture

## NLayer Architecture

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/NLayer-Architecture.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#introduction)

The layering of an application's codebase is a widely accepted technique to
help reduce complexity and to improve code reusability. To achieve a layered
architecture, ASP.NET Boilerplate follows the principles of the **Traditional "N-Layer" Architecture** with **Domain**
**Driven Design**.

### Traditional "N-Layer" Architecture Layers [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#traditional-n-layer-architecture-layers)

There are four fundamental layers in the N-Layer Architecture:

- **Presentation Layer**: Provides an interface to the user. Uses the
Application Layer to achieve user interactions.
- **Application Layer**: Mediates between the Presentation and Domain
Layers. Orchestrates business objects to perform specific
application tasks.
- **Domain Layer**: Includes business objects and their rules. This is the
heart of the application and this is where DDD is applied.
- **Infrastructure Layer**: Provides generic technical capabilities
that support higher layers mostly using 3rd-party libraries.

### ASP.NET Boilerplate Application Architecture Model [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#asp-net-boilerplate-application-architecture-model)

In addition to DDD, there are also other logical and physical layers in
a modern architected application. The model below is suggested and
implemented for ASP.NET Boilerplate applications. ASP.NET Boilerplate
not only makes implementing this model easier by providing base classes
and services, but also provides [startup templates](https://aspnetboilerplate.com/Templates) to
directly start with this model.

![ASP.NET Boilerplate NLayer Architecture](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/abp-nlayer-architecture.png)

#### Client Applications [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#client-applications)

These are remote clients that use the application as a service via HTTP APIs
(API Controllers, [OData](https://aspnetboilerplate.com/Pages/Documents/OData-Integration) Controllers, maybe even a
GraphQL endpoint). A remote client can be a SPA (Single Page App), a mobile application, or
a 3rd-party consumer. [Localization](https://aspnetboilerplate.com/Pages/Documents/Localization) and
[Navigation](https://aspnetboilerplate.com/Pages/Documents/Navigation) can be done inside this applications.

#### Presentation Layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#presentation-layer)

ASP.NET \[Core\] MVC (Model-View-Controller) can be considered to be the
presentation layer. It can be a physical layer (uses application via
HTTP APIs) or a logical layer (directly injects and uses [application\\
services](https://aspnetboilerplate.com/Pages/Documents/Application-Services)). In either case it can include
[Localization](https://aspnetboilerplate.com/Pages/Documents/Localization), [Navigation](https://aspnetboilerplate.com/Pages/Documents/Navigation),
[Object Mapping](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping),
[Caching](https://aspnetboilerplate.com/Pages/Documents/Caching), [Configuration\\
Management](https://aspnetboilerplate.com/Pages/Documents/Setting-Management), [Audit\\
Logging](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging) and so on. It also deals with
[Authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization), [Session](https://aspnetboilerplate.com/Pages/Documents/Abp-Session),
[Features](https://aspnetboilerplate.com/Pages/Documents/Feature-Management) (for
[multi-tenant](https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy) applications) and [Exception\\
Handling](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions).

#### Distributed Service Layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#distributed-service-layer)

This layer is used to serve application/domain functionality via remote
APIs like REST, OData, GraphQL... They don't contain business logic but
only translate HTTP requests to domain interactions, or can use
application services to delegate the operation. This layer generally
includes [Authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization), [Caching](https://aspnetboilerplate.com/Pages/Documents/Caching),
[Audit Logging](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging), [Object\\
Mapping](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping), [Exception\\
Handling](https://aspnetboilerplate.com/Pages/Documents/Handling-Exceptions), [Session](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) and so
on...

#### Application Layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#application-layer)

The application layer mainly includes [Application\\
Services](https://aspnetboilerplate.com/Pages/Documents/Application-Services) that use domain layer and domain
objects ( [Domain Services](https://aspnetboilerplate.com/Pages/Documents/Domain-Services),
[Entities](https://aspnetboilerplate.com/Pages/Documents/Entities)...) to perform requested application
functionalities. It uses [Data Transfer\\
Objects](https://aspnetboilerplate.com/Pages/Documents/Data-Transfer-Objects) to get data from and return data
to the presentation or distributed service layer. It can also deal with
[Authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization), [Caching](https://aspnetboilerplate.com/Pages/Documents/Caching), [Audit\\
Logging](https://aspnetboilerplate.com/Pages/Documents/Audit-Logging), [Object\\
Mapping](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping), the [Session](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) and
so on...

#### Domain Layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#domain-layer)

This is the main layer that implements our domain logic. It includes
[Entities](https://aspnetboilerplate.com/Pages/Documents/Entities), [Value Objects](https://aspnetboilerplate.com/Pages/Documents/Value-Objects), and [Domain\\
Services](https://aspnetboilerplate.com/Pages/Documents/Domain-Services) to perform business/domain logic. It can
also include [Specifications](https://aspnetboilerplate.com/Pages/Documents/Specifications) and trigger [Domain\\
Events](https://aspnetboilerplate.com/Pages/Documents/EventBus-Domain-Events). It defines Repository Interfaces
to read and persist entities from the data source (generally a DBMS).

#### Infrastructure Layer [Anchor](https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture\#infrastructure-layer)

The infrastructure layer makes other layers work: It implements
the repository interfaces (using [Entity Framework\\
Core](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core) for example) to actually work with a
real database. It may also include an integration to a vendor to [send\\
emails](https://aspnetboilerplate.com/Pages/Documents/Email-Sending) and so on. This is not a strict layer below
all layers, but actually supports other layers by implementing the abstract
concepts of them.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe