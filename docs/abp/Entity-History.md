# https://aspnetboilerplate.com/Pages/Documents/Entity-History

## Entity History

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Entity-History.md)

In this document

## Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#introduction)

ASP.NET Boilerplate provides an infrastructure to automatically log all
entity and property changes.

The saved fields for an entity change are: The related **tenant id**,
**entity change set id**, **entity id**,
**entity type name**, **change time** and the **change type**.

The saved fields for an entity property change are: The related **tenant id**,
**entity change id**, **property name**, **property type name**,
**new value** and the **original value**.

The entity changes are grouped in a change set for each `SaveChanges` call.

The saved fields for an entity change set are: The related **tenant id**,
changer **user id**, **creation time**, **reason**, the client's
**IP address**, the client's **computer name** and the **browser info** (if
entities are changed in a web request).

The Entity History tracking system uses
[`IAbpSession`](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) to
get the current `UserId` and `TenantId`.

No entities are automatically tracked by default. You should configure entities either by using the startup configuration or via attributes.

## About IEntityHistoryStore [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#about-ientityhistorystore)

The Entity History tracking system uses `IEntityHistoryStore` to
save change information. While you can implement it in your own way,
it's fully implemented in the **Module Zero** project.

## Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#configuration)

To configure Entity History, you can use the
`Configuration.EntityHistory` property
in your [module](https://aspnetboilerplate.com/Pages/Documents/Module-System)'s `PreInitialize` method.
Entity History is **enabled by default**.
You can disable it as shown below:

Copy

```csharp
    public class MyModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.EntityHistory.IsEnabled = false;
        }

        //...
    }
```

Here are the Entity History configuration properties:

- `IsEnabled`: Used to enable/disable the tracking system completely.
Default: `true`.
- `IsEnabledForAnonymousUsers`: If this is set to true, the change logs
are saved for users that are not logged in to the application.
Default: `false`.
- `Selectors`: Used to select entities to save change logs.
- `IgnoredTypes`: Used to skip entities when saving change logs.

`Selectors` is a list of predicates to select entities to save
change logs. A selector has a unique **name** and a **predicate**.
For example, a selector can be used to select **full audited entities**.
It's defined as shown below:

Copy

```csharp
    Configuration.EntityHistory.Selectors.Add(
        new NamedTypeSelector(
            "Abp.FullAuditedEntities",
            type => typeof (IFullAudited).IsAssignableFrom(type)
        )
    );
```

You can add your selectors in your module's `PreInitialize` method.

`IgnoredTypes` is a list of entity types to be ignored when saving
change logs.
For example, we configured all entities that implements `ISetting` to be tracked by the Entity History system.
In order to skip entity history for `SecretSetting` which implements `ISetting`.
It can be defined as shown below:

Copy

```csharp
    Configuration.EntityHistory.IgnoredTypes.AddIfNotContains(typeof(SecretSetting));
```

You can add your ignored types in your module's `PreInitialize` method.

#### Notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#notes)

- `IgnoredTypes` takes priority over the `Selectors` when saving change log for the entities.
- `EntityChangeSet`, `EntityChange`, `EntityPropertyChange` are added to `IgnoredTypes` by **default** in the Entity History system.
- `AuditLog` is added to `IgnoredTypes` by **default** in Module Zero project.

### Enable/Disable by attributes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#enable-disable-by-attributes)

While you can select tracked entities by configuration, you can use the
`Audited` and `DisableAuditing` attributes for a single
**entity** or an individual **property**. Example:

Copy

```csharp
    [Audited]
    public class MyEntity : Entity
    {
        public string MyProperty1 { get; set; }

        [DisableAuditing]
        public int MyProperty2 { get; set; }

        public long MyProperty3 { get; set; }
    }
```

All properties of `MyEntity` are tracked except `MyProperty2` since it's
explicitly disabled. The `Audited` attribute can be used to
save change logs for a desired property.

When using `Audited` attribute on an **entity** class, `EntityChange` will be created for an entity in `Added`/`Modified`/`Deleted` state, regardless of whether any `PropertyChange` is being created.
For example, if only `MyProperty2` is modified, `EntityChange` will be created even though `PropertyChange` will not be created.

When using `Audited` attribute on a **property**, `PropertyChange` will be created for the property if there is any difference between the **new and old values** of the property.

`DisableAuditing` can be used for an entity or a single **property of an**
**entity**. Thus, you can **hide sensitive data** in change logs, such as
passwords for example.

### Reason Property [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#reason-property)

The entity change set has a `Reason` property that can be used to understand why a
set of changes has occurred, i.e. the use case that resulted in these changes.

For example, Person A transfers money from Account A to Account B. Both account
balances change and "Money transfer" is recorded as the Reason for this change set.
Since a balance change can be due to other reasons, the Reason property explains
why these changes were made.

The `Abp.AspNetCore` package implements `HttpRequestEntityChangeSetReasonProvider`,
which returns the `HttpContext.Request`'s URL as the `Reason`.

### UseCase Attribute [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#usecase-attribute)

The preferred approach is using the `UseCase` attribute. Example:

Copy

```csharp
    [UseCase(Description = "Assign an issue to a user")]
    public virtual async Task AssignIssueAsync(AssignIssueInput input)
    {
        // ...

        await _unitOfWorkManager.Current.SaveChangesAsync();
    }
```

#### UseCase Attribute Restrictions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#usecase-attribute-restrictions)

You can use the `UseCase` attribute for:

- All `public` or `public virtual` methods for classes that are
used via its interface, e.g. an application service used via its interface.
- All `public virtual` methods for self-injected classes, e.g. **MVC**
**Controllers**.
- All `protected virtual` methods.

### IEntityChangeSetReasonProvider [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#ientitychangesetreasonprovider)

In some cases, you may need to change/override the `Reason` value for a limited scope.
You can use the `IEntityChangeSetReasonProvider.Use(...)` method as shown below:

Copy

```csharp
    public class MyService
    {
        private readonly IEntityChangeSetReasonProvider _reasonProvider;

        public MyService(IEntityChangeSetReasonProvider reasonProvider)
        {
            _reasonProvider = reasonProvider;
        }

        public virtual async Task AssignIssueAsync(AssignIssueInput input)
        {
            var reason = "Assign an issue to user: " + input.UserId.ToString();
            using (_reasonProvider.Use(reason))
            {
                ...

                await _unitOfWorkManager.Current.SaveChangesAsync();
            }
        }
    }
```

The `Use` method returns an `IDisposable` and it **must be disposed**. Once the return
value is disposed, the `Reason` is automatically restored to the previous value.

### IEntitySnapshotManager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#ientitysnapshotmanager)

You may need to get a snapshot of your entity on a given date. You can use the `IEntitySnapshotManager.GetSnapshotAsync(...)` method as shown below:

Copy

```csharp
    public class MyService
    {
        private readonly IEntitySnapshotManager _entitySnapshotManager;

        public MyService(IEntitySnapshotManager entitySnapshotManager)
        {
            _entitySnapshotManager = entitySnapshotManager;
        }

        public Task<EntityHistorySnapshot> GetMyEntitySnapshot(long id, DateTime time)
        {
            return _entitySnapshotManager.GetSnapshotAsync<MyEntity, long>(id, time);
        }

        public async Task<string> GetMyEntityMyPropertySnapshotValue(long id, DateTime time)
        {
           var snapshot = await GetMyEntitySnapshot(id, time);
           if (snapshot.IsPropertyChanged(nameof(MyEntity.MyProperty)))
           {
               // var stacktree = snapshot.PropertyChangesStackTree[nameof(MyEntity.MyProperty)];
               return snapshot[nameof(MyEntity.MyProperty)];
           }
           return null;
        }
    }
```

`IEntitySnapshotManager.GetSnapshotAsync(...)` returns an `EntityHistorySnapshot` that contains the snapshot value of all changed properties on the given date and a stack tree of the changes.

## ORM Integrations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#orm-integrations)

Entity History in **Module Zero** project is implemented for
[Entity Framework](https://aspnetboilerplate.com/Pages/Documents/EntityFramework-Integration) 6.x and
[Entity Framework Core](https://aspnetboilerplate.com/Pages/Documents/Entity-Framework-Core) with the following logics:

### Entity Changes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#entity-changes)

- An entity must not be one of the `IgnoredTypes`.
- An entity must be `public` in order to be saved in the change logs.
`private`, `protected`, `internal` entities are ignored.
- `DisableAuditing` takes priority over the `Audited` attribute when saving entity changes.
- An entity must satisfy one of the predicates from `Selectors`.

### Property Changes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#property-changes)

- Primary key properties are excluded from an entity's property changes.
- `DisableAuditing` takes priority over the `Audited` attribute when saving property changes.
- Property changes would only be saved if there are changes between original/new values.
`Audited` attribute will make property changes being saved even if there is no change between original/new values.

## Entity Framework Core [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#entity-framework-core)

- Entity changes only work for entities and owned entities.
- Property changes only work for scalar properties (e.g. string, int, bool...)
  - including shadow properties.

### Owned Entities [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#owned-entities)

Entity History tracks changes to owned entities as entity changes.
The primary key of the owner entity is saved as the **entity id**.

## Entity Framework 6.x [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#entity-framework-6-x)

- Entity changes only work for entities.
- Property changes only work for complex type properties and scalar properties (e.g. string, int, bool...)
  - including relationship changes.

### Complex Type Properties [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#complex-type-properties)

Entity History tracks changes to complex type properties as property changes (unlike owned entities for EF Core).
The original/new values of a complex type property will be a serialized JSON string of the complex type property.

### Relationship Changes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Entity-History\#relationship-changes)

Entity History tracks changes to relationships (navigation properties without foreign key) as property changes (like shadow properties for EF Core).

However, these use the navigation property name (e.g. `Blog`) as declared in the entity, unlike the shadow property name (e.g. `BlogId`) generated by EF Core.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe