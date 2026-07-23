# https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System

## Dynamic Parameter System

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Dynamic-Parameter-System.md)

In this document

## Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#introduction)

**Dynamic Property System** is a system that allows you to add and manage new properties on entity objects at runtime without any code changes. With this system, you can define dynamic properties on entity objects and perform operations on these objects easily. For example, it can be used for cities, counties, gender, status codes etc.

### Dynamic Property Definition [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#dynamic-property-definition)

First of all you need to define which entities can have that feature and allowed _input types_ \\* can a dynamic property has.

Copy

```csharp
public class MyDynamicEntityPropertyDefinitionProvider : DynamicEntityPropertyDefinitionProvider
{
     public override void SetDynamicEntityProperties(IDynamicEntityPropertyDefinitionContext context)
     {
         //input types dynamic properties can have
         context.Manager.AddAllowedInputType<SingleLineStringInputType>();
         context.Manager.AddAllowedInputType<CheckboxInputType>();
         context.Manager.AddAllowedInputType<ComboboxInputType>();

         //entities that can have dynamic property
         context.Manager.AddEntity<Country>();
         context.Manager.AddEntity<Blog, long>();
     }
}
```

_InputType: A UI input type for the feature. This can be defined, and then used while creating an automatic feature screen. For example: [SingleLineStringInputType](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/UI/Inputs/SingleLineStringInputType.cs), [ComboboxInputType](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/UI/Inputs/ComboboxInputType.cs), [CheckboxInputType](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/UI/Inputs/CheckboxInputType.cs)_

Copy

```csharp
public class DynamicEntityPropertyTestModule : AbpModule
{
    public override void PreInitialize()
    {        Configuration.DynamicEntityProperties.Providers.Add<MyDynamicEntityPropertyDefinitionProvider>();
    }
}
```

### Dynamic Property [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#dynamic-property)

It is a property that you can use in other entities.

Creating a dynamic property.

Copy

```csharp
var cityProperty = new DynamicProperty
{
    PropertyName = "City",//Property name.
    InputType = InputTypeBase.GetName<ComboboxInputType>(),
    Permission = "App.Permission.DynamicProperty.City",
    TenantId = AbpSession.TenantId
};
var dynamicPropertyManager = Resolve<IDynamicPropertyManager>();
dynamicPropertyManager.Add(cityProperty);
```

**_DynamicProperty.cs_**

| Property | Summary |
| --- | --- |
| PropertyName\* (string) | Unique name of the dynamic property |
| Input Type\* (string) | Input type name of the dynamic property.(Input type should be provided in definition.) |
| Permission (string) | Required permission to manage anything about that property <br>(`DynamicPropertyValue`, `DynamicEntityProperty`, `DynamicEntityPropertyValue`) |
| TenantId (int?) | Tenant's unique identifier <br>(For example you can use (IAbpSession).TenantId to get current tenant's id) |

You can use [**IDynamicPropertyManager**](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/IDynamicPropertyManager.cs) to manage dynamic property. (It uses cache)

Copy

```csharp
public interface IDynamicPropertyManager
{
    DynamicProperty Get(int id);

    Task<DynamicProperty> GetAsync(int id);

    DynamicProperty Get(string propertyName);

    Task<DynamicProperty> GetAsync(string propertyName);

    void Add(DynamicProperty dynamicProperty);

    Task AddAsync(DynamicProperty dynamicProperty);

    void Update(DynamicProperty dynamicProperty);

    Task UpdateAsync(DynamicProperty dynamicProperty);

    void Delete(int id);

    Task DeleteAsync(int id);
}
```

### Dynamic Property Value [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#dynamic-property-value)

If your dynamic property's input types need values to select (for example `ComboboxInputType`), you can store the values that your dynamic properties can have here.

Adding a value to your dynamic property.

Copy

```csharp
var istanbul = new new DynamicPropertyValue(cityProperty, "Istanbul", AbpSession.TenantId);
var london = new new DynamicPropertyValue(cityProperty, "London", AbpSession.TenantId);

var _dynamicPropertyValueManager = Resolve<IDynamicPropertyValueManager>();
_dynamicPropertyValueManager.Add(istanbul);
_dynamicPropertyValueManager.Add(london);
```

**_DynamicPropertyValue.cs_**

| Property | Summary |
| --- | --- |
| DynamicPropertyId\* (int) | Unique indentifier of DynamicProperty |
| Value\* (string) | Value |
| TenantId (int?) | Tenant's unique identifier <br>(For example you can use (IAbpSession).TenantId to get current tenant's id) |

You can use [**IDynamicPropertyValueManager**](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/IDynamicPropertyValueManager.cs) to manage dynamic property values. (It checks permissions)

Copy

```csharp
public interface IDynamicPropertyValueManager
{
    DynamicPropertyValue Get(int id);

    Task<DynamicPropertyValue> GetAsync(int id);

    List<DynamicPropertyValue> GetAllValuesOfDynamicProperty(int dynamicPropertyId);

    Task<List<DynamicPropertyValue>> GetAllValuesOfDynamicPropertyAsync(int dynamicPropertyId);

    void Add(DynamicPropertyValue dynamicPropertyValue);

    Task AddAsync(DynamicPropertyValue dynamicPropertyValue);

    void Update(DynamicPropertyValue dynamicPropertyValue);

    Task UpdateAsync(DynamicPropertyValue dynamicPropertyValue);

    void Delete(int id);

    Task DeleteAsync(int id);

    void CleanValues(int dynamicPropertyId);

    Task CleanValuesAsync(int dynamicPropertyId);
}
```

### Dynamic Entity Properties [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#dynamic-entity-properties)

Dynamic property definitions of an entity. It stores which dynamic properties an entity have.

Adding New Property to Entity

Copy

```csharp
var _dynamicEntityPropertyManager = Resolve<IDynamicEntityPropertyManager>();
var cityDynamicPropertyOfCountry = _dynamicEntityPropertyManager.Add<Country>(cityProperty, tenantId: 1);//add cityProperty to Country entity
```

**_DynamicEntityProperty.cs_**

| Property | Summary |
| --- | --- |
| DynamicPropertyId\* (int) | Unique indentifier of DynamicProperty |
| EntityFullName\* (string) | Full name of type of entity <br>(For example: "typeof(MyEntity).FullName") |
| TenantId (int?) | Tenant's unique identifier <br>(For example you can use (IAbpSession).TenantId to get current tenant's id) |

You can use [**IDynamicEntityPropertyManager**](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/IDynamicEntityPropertyManager.cs) to manage entities dynamic properties. (It uses cache and checks required permissions.) See also: [DynamicEntityPropertyManagerExtensions](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/Extensions/DynamicEntityPropertyManagerExtensions.cs)

Copy

```csharp
public interface IDynamicEntityPropertyManager
{
    DynamicEntityProperty Get(int id);

    Task<DynamicEntityProperty> GetAsync(int id);

    List<DynamicEntityProperty> GetAll(string entityFullName);

    Task<List<DynamicEntityProperty>> GetAllAsync(string entityFullName);

    List<DynamicEntityProperty> GetAll();

    Task<List<DynamicEntityProperty>> GetAllAsync();

    void Add(DynamicEntityProperty dynamicEntityProperty);

    Task AddAsync(DynamicEntityProperty dynamicEntityProperty);

    void Update(DynamicEntityProperty dynamicEntityProperty);

    Task UpdateAsync(DynamicEntityProperty dynamicEntityProperty);

    void Delete(int id);

    Task DeleteAsync(int id);
}
```

### Entity Dynamic Property Values [Anchor](https://aspnetboilerplate.com/Pages/Documents/Dynamic-Parameter-System\#entity-dynamic-property-values)

The values your that your entity rows have.

Adding value to entities dynamic property

Copy

```csharp
var valueOfRow1 = new DynamicEntityPropertyValue(cityDynamicPropertyOfCountry, EntityId: "1", value: "Istanbul", tenantId: AbpSession.TenantId);
var valueOfRow12 = new DynamicEntityPropertyValue(cityDynamicPropertyOfCountry, EntityId: "1", value: "London", tenantId: AbpSession.TenantId);//can have multiple values
var valueOfRow2 = new DynamicEntityPropertyValue(cityDynamicPropertyOfCountry, EntityId: "2", value: "London", tenantId: AbpSession.TenantId);

var _dynamicEntityPropertyValueManager = Resolve<IDynamicEntityPropertyValueManager>();
_dynamicEntityPropertyValueManager.Add(valueOfRow1);
_dynamicEntityPropertyValueManager.Add(valueOfRow12);
_dynamicEntityPropertyValueManager.Add(valueOfRow2);
```

Get values of entity property

Copy

```csharp
var _dynamicEntityPropertyValueManager = Resolve<IDynamicEntityPropertyValueManager>();
var allValues = _dynamicEntityPropertyValueManager.GetValues<Country>(EntityId: "1");
var cityValues = _dynamicEntityPropertyValueManager.GetValues<Country>(EntityId: "1", cityProperty);
```

**_DynamicEntityPropertyValue.cs_**

| Property | Summary |
| --- | --- |
| DynamicEntityPropertyId\* (int) | Unique indentifier of DynamicEntityPropertyId |
| EntityId\* (string) | Unique idenfier of entity row |
| Value\* (string) | Value |
| TenantId (int?) | Tenant's unique identifier <br>(For example you can use (IAbpSession).TenantId to get current tenant's id) |

You can use [**IDynamicEntityPropertyValueManager**](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/IDynamicEntityPropertyValueManager.cs) to manage entities dynamic properties. (It uses cache and checks required permissions.) See also: [DynamicEntityPropertyValueManagerExtensions](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/DynamicEntityProperties/Extensions/DynamicEntityPropertyValueManagerExtensions.cs)

Copy

```csharp
public interface IDynamicEntityPropertyValueManager
{
    DynamicEntityPropertyValue Get(int id);

    Task<DynamicEntityPropertyValue> GetAsync(int id);

    void Add(DynamicEntityPropertyValue dynamicEntityPropertyValue);

    Task AddAsync(DynamicEntityPropertyValue dynamicEntityPropertyValue);

    void Update(DynamicEntityPropertyValue dynamicEntityPropertyValue);

    Task UpdateAsync(DynamicEntityPropertyValue dynamicEntityPropertyValue);

    void Delete(int id);

    Task DeleteAsync(int id);

    List<DynamicEntityPropertyValue> GetValues(int dynamicEntityPropertyId, string EntityId);

    Task<List<DynamicEntityPropertyValue>> GetValuesAsync(int dynamicEntityPropertyId, string EntityId);

    List<DynamicEntityPropertyValue> GetValues(string entityFullName, string EntityId);

    Task<List<DynamicEntityPropertyValue>> GetValuesAsync(string entityFullName, string EntityId);

    List<DynamicEntityPropertyValue> GetValues(string entityFullName, string EntityId, int dynamicPropertyId);

    Task<List<DynamicEntityPropertyValue>> GetValuesAsync(string entityFullName, string EntityId, int dynamicPropertyId);

    List<DynamicEntityPropertyValue> GetValues(string entityFullName, string EntityId, string propertyName);

    Task<List<DynamicEntityPropertyValue>> GetValuesAsync(string entityFullName, string EntityId, string propertyName);

    void CleanValues(int dynamicEntityPropertyId, string EntityId);

    Task CleanValuesAsync(int dynamicEntityPropertyId, string EntityId);
}
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe