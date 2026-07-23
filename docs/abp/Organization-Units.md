# https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units

## Organization Units

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Organization-Units.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#introduction)

Organization units (OU) can be used to **hierarchically group users and**
**entities**.

### OrganizationUnit Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#organizationunit-entity)

An OU is represented by the **OrganizationUnit** entity. The fundamental
properties of this entity are:

- **TenantId**: Tenant's Id of this OU. Can be null for host OUs.
- **ParentId**: Parent OU's Id. Can be null if this is a root OU.
- **Code**: A hierarchical string code that is unique for a tenant.
- **DisplayName**: Shown name of the OU.

The OrganizationUnit entity's primary key (Id) is a **long** type and it derives
from the [**FullAuditedEntity**](https://aspnetboilerplate.com/Pages/Documents/Entities#auditing) class
which provides audit information and implements the
[**ISoftDelete**](https://aspnetboilerplate.com/Pages/Documents/Data-Filters#isoftdelete) interface
(OUs are not deleted from the database, they are just marked as deleted).

#### Organization Tree [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#organization-tree)

Since an OU can have a parent, all OUs of a tenant are in a **tree**
structure. There are some rules for this tree;

- There can be more than one root (where the ParentId is null).
- Maximum depth of the tree is defined as a constant as
OrganizationUnit. **MaxDepth**. The value is **16**.
- There is a limit for the first-level children count of an OU (because of the
fixed OU Code unit length explained below).

#### OU Code [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#ou-code)

OU code is automatically generated and maintained by the OrganizationUnit
Manager. It's a string that looks something like this:

" **00001.00042.00005**"

This code can be used to easily query the database for all the children
of an OU (recursively). There are some rules for this code:

- It must be **unique** for a [tenant](https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy).
- All the children of the same OU have codes that **start with the parent OU's code**.
- It's **fixed length** and based on the level of the OU in the tree, as shown in
the sample.
- While the OU code is unique, it can be **changeable** if you move an OU.
- We must reference an OU by Id, not Code.

### OrganizationUnit Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#organizationunit-manager)

The **OrganizationUnitManager** class can be
[injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and used to manage
OUs. Common use cases are:

- Create, Update or Delete an OU
- Move an OU in the OU tree.
- Getting information about the OU tree and its items.

#### Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#multi-tenancy)

The OrganizationUnitManager is designed to work for a **single tenant** at a
time. It works for the **current tenant** by default.

### Common Use Cases [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#common-use-cases)

Here, we will see some common use cases for OUs. You can find the source code of
the samples
[here](https://github.com/aspnetboilerplate/aspnetboilerplate-samples/tree/master/OrganizationUnitsDemo).

#### Creating An Entity That Belongs To An Organization Unit [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#creating-an-entity-that-belongs-to-an-organization-unit)

The most obvious usage of OUs is to assign an entity to an OU. Let's see a
sample entity:

Copy

```csharp
public class Product : Entity, IMustHaveTenant, IMustHaveOrganizationUnit
{
    public virtual int TenantId { get; set; }

    public virtual long OrganizationUnitId { get; set; }

    public virtual string Name { get; set; }

    public virtual float Price { get; set; }
}
```

We simply created the **OrganizationUnitId** property to assign this entity
to an OU. The **IMustHaveOrganizationUnit** defines the OrganizationUnitId
property. We don't have to implement it, but it's recommended because it provides
standardization. There is also the IMayHaveOrganizationId interface which has a
**nullable** OrganizationUnitId property.

We can now relate a Product to an OU and query the products of a specific
OU.

**Please note**; The product entity must have a **TenantId** (which is a property
of IMustHaveTenant) to distinguish it from products of different tenants in a
multi-tenant application (see the [Multi-Tenancy\\
document](https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy#data-filters) for more info). If your
application is not multi-tenant, you don't need this interface and
property.

#### Getting Entities In An Organization Unit [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#getting-entities-in-an-organization-unit)

Getting the Products of an OU is simple. Let's see this sample [domain\\
service](https://aspnetboilerplate.com/Pages/Documents/Domain-Services):

Copy

```csharp
public class ProductManager : IDomainService
{
    private readonly IRepository<Product> _productRepository;

    public ProductManager(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public List<Product> GetProductsInOu(long organizationUnitId)
    {
        return _productRepository.GetAllList(p => p.OrganizationUnitId == organizationUnitId);
    }

}
```

As shown above, we can simply write a predicate against Product.OrganizationUnitId.

#### Get Entities In An Organization Unit Including It's Child Organization Units [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#get-entities-in-an-organization-unit-including-it-s-child-organization-units)

We may want to get the Products of an organization unit including child
organization units. In this case, the OU **Code** can help us:

Copy

```csharp
public class ProductManager : IDomainService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;

    public ProductManager(
        IRepository<Product> productRepository,
        IRepository<OrganizationUnit, long> organizationUnitRepository)
    {
        _productRepository = productRepository;
        _organizationUnitRepository = organizationUnitRepository;
    }

    [UnitOfWork]
    public virtual List<Product> GetProductsInOuIncludingChildren(long organizationUnitId)
    {
        var code = _organizationUnitRepository.Get(organizationUnitId).Code;

        var query =
            from product in _productRepository.GetAll()
            join organizationUnit in _organizationUnitRepository.GetAll() on product.OrganizationUnitId equals organizationUnit.Id
            where organizationUnit.Code.StartsWith(code)
            select product;

        return query.ToList();
    }
}
```

First, we got the **code** of the the given OU. Then we created a LINQ expression with a
**join** and a **StartsWith(code)** condition (StartsWith creates a
**LIKE** query in SQL). This way we can hierarchically get the products of an
OU.

#### Filter Entities For A User [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#filter-entities-for-a-user)

We may want to get all products that are in the OUs of a specific user.
Example code:

Copy

```csharp
public class ProductManager : IDomainService
{
    private readonly IRepository<Product> _productRepository;
    private readonly UserManager _userManager;

    public ProductManager(
        IRepository<Product> productRepository,
        UserManager userManager)
    {
        _productRepository = productRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userManager = userManager;
    }

    public async Task<List<Product>> GetProductsForUserAsync(long userId)
    {
		var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
		var organizationUnits = await _userManager.GetOrganizationUnitsAsync(user);
		var organizationUnitCodes = organizationUnits.Select(ou => ou.Code).ToList();

		var predicate = PredicateBuilder.New<OrganizationUnit>();
		foreach (var code in organizationUnitCodes)
		{
			predicate = predicate.Or(ou => ou.Code.StartsWith(code));
		}

		var organizationUnitIds = await _organizationUnitRepository.GetAll().Where(predicate).Select(ou => ou.Id).ToListAsync();
        return await _productRepository.GetAllListAsync(p => organizationUnitIds.Contains(p.OrganizationUnitId));
    }
}
```

We simply found the Ids of the OUs of the user. We then used a **Contains** condition
while getting the products. We could also create a LINQ query with join
to get the same list, instead.

We may want to get products in the user's OUs including their child OUs:

Copy

```csharp
public class ProductManager : IDomainService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
    private readonly UserManager _userManager;

    public ProductManager(
        IRepository<Product> productRepository,
        IRepository<OrganizationUnit, long> organizationUnitRepository,
        UserManager userManager)
    {
        _productRepository = productRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userManager = userManager;
    }

    [UnitOfWork]
    public virtual async Task<List<Product>> GetProductsForUserIncludingChildOusAsync(long userId)
    {
        var user = await _userManager.GetUserByIdAsync(userId);
        var organizationUnits = await _userManager.GetOrganizationUnitsAsync(user);
        var organizationUnitIds = organizationUnits.Select(ou => ou.Id).ToList();

        var newQuery = _productRepository.GetAll()
        .Where(product => organizationUnitIds.Contains(product.OrganizationUnitId));

        return newQuery.ToList();
    }
}
```

We combined **Any** with the **StartsWith** condition into a LINQ join
statement.

There will most likely be more complex requirements, but they all can be done
with LINQ or SQL.

### Settings [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units\#settings)

You can inject and use the **IOrganizationUnitSettings** interface to get
the Organization Unit's setting values. There currently is just a single
setting that can be changed for your application needs:

- **MaxUserMembershipCount**: Maximum allowed membership count for a
user.


Default value is **int.MaxValue** which allows a user to be a member
of unlimited OUs at the same time.


The Setting name is a constant defined in
_AbpZeroSettingNames.OrganizationUnits.MaxUserMembershipCount_.

You can change the setting values using the [setting\\
manager](https://aspnetboilerplate.com/Pages/Documents/Setting-Management).

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe