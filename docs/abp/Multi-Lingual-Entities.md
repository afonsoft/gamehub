# https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities

## Multi Lingual Entities

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Multi-Lingual-Entities.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#introduction)

ASP.NET Boilerplate defines two basic interfaces for Multi-Lingual entity definitions to provide a standard model for translating entities.

#### IMultiLingualEntity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#imultilingualentity)

`IMultiLingualEntity<TTranslation>` interface is used to mark multi lingual entities. The entities marked with `IMultiLingualEntity<TTranslation>` interface must define language-neutral information. The entities marked with `IMultiLingualEntity<TTranslation>` contains a collection of Translations which contains language-dependent information.

A sample multi lingual entity would be;

Copy

```
public class Product : Entity, IMultiLingualEntity<ProductTranslation>
{
    public decimal Price { get; set; }

    public ICollection<ProductTranslation> Translations { get; set; }
}
```

#### IEntityTranslation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#ientitytranslation)

IEntityTranslation interface is used to mark translation of a Multi-Lingual entity. The entities marked with IEntityTranslation interface must define language dependent information. The entities marked with IEntityTranslation contains Language field which contains a language code for the translation and a reference to Multi-Lingual entity.

A sample multi lingual entity would be;

Copy

```
public class ProductTranslation : Entity, IEntityTranslation<Product>
{
    public string Name { get; set; }

    public Product Core { get; set; }

    public int CoreId { get; set; }

    public string Language { get; set; }
}
```

#### CreateMultiLingualMap [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#createmultilingualmap)

When listing Multi-Lingual entities on a user interface, most of the time, only one translation of a Multi-Lingual entity which is in user's current language will be displayed to user.

For this purpose, ABP defines CreateMultiLingualMap extension method to map a Multi-Lingual entity and one of it's Translation to an appropriate Dto class using **AutoMapper**.

By using CreateMultiLingualMap extension method, only one record from Translations collection of a Multi-Lingual entity will be mapped to target Dto class. This extension method finds the translation with selected UI language first. If there is no translation with selected UI language, then extension method searches for the default language setting (see [Setting-Management](https://aspnetboilerplate.com/Pages/Documents/Setting-Management#setting-scope)) and uses the translation in default language. If extension method couldn't find any translation in current UI language or default language, it uses one of the existing translations.

A sample Dto class for sample Product entity above would be;

Copy

```
public class ProductListDto
{
    // Mapped from Product.Price
    public decimal Price { get; set; }

    // Mapped from ProductTranslation.Name
    public string Name { get; set; }
}
```

And it's mapping configuration is;

Copy

```
Configuration.Modules.AbpAutoMapper().Configurators.Add(configuration =>
{
    CustomDtoMapper.CreateMappings(configuration, new MultiLingualMapContext(
        IocManager.Resolve<ISettingManager>()
    ));
});

internal static class CustomDtoMapper
{
    public static void CreateMappings(IMapperConfigurationExpression configuration, MultiLingualMapContext context)
    {
        configuration.CreateMultiLingualMap<Product, ProductTranslation, ProductListDto>(context);
    }
}
```

SettingManager is required to find default language setting when mapping a multi lingual entity to a Dto class.

In some cases like editing a multi lingual entity on the UI, all translations may be needed in the Dto class. In such cases, the Dto classes can be defined like below and [Object-To-Object-Mapping](https://aspnetboilerplate.com/Pages/Documents/Object-To-Object-Mapping) can be used.

Copy

```
[AutoMap(typeof(Product))]
public class ProductDto
{
    public decimal Price { get; set; }

    public List<ProductTranslationDto> Translations {get; set;}
}

[AutoMap(typeof(ProductTranslation))]
public class ProductTranslationDto
{
    public string Name { get; set; }

    public string Language { get; set; }
}
```

CreateMultiLingualMap extension method returns an object of type CreateMultiLingualMapResult which contains **EntityMap** and **TranslationMap** fields. These fields can be used to customize multi lingual mapping. A sample usage would be;

Copy

```c
configuration.CreateMultiLingualMap<Order, OrderTranslation, OrderListDto>(context)
    .EntityMap.ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
```

CreateMultiLingualMap also allows you to define type of the id field of main and translation entities as well;

Copy

```c
configuration.CreateMultiLingualMap<Order, int, OrderTranslation, long, OrderListDto>(context, true);
```

### Crud Operations [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#crud-operations)

#### Creating a MultiLingual Entity with Translation(s) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#creating-a-multilingual-entity-with-translation-s-)

A Dto class like the below one can be used for creating a Multi-Lingual entity with it's translations.

Copy

```
[AutoMap(typeof(Product))]
public class ProductDto
{
    public decimal Price { get; set; }

    public ICollection<ProductTranslationDto> Translations { get; set; }
}
```

After defining such a Dto class, we can use it in our application service to create a Multi-Lingual entity.

Copy

```
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product> _productRepository;

    public ProductAppService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task CreateProduct(ProductDto input)
    {
        var product = ObjectMapper.Map<Product>(input);
        await _productRepository.InsertAsync(product);
    }
}
```

#### Updating a Multi-Lingual Entity with Translation(s) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#updating-a-multi-lingual-entity-with-translation-s-)

We can use similar Dto class for updating our Multi-Lingual entity. A sample application service method for update operation can be defined like below;

Copy

```
public async Task UpdateProduct(ProductDto input)
{
    var product = await _productRepository.GetAllIncluding(p => p.Translations)
        .FirstOrDefaultAsync(p => p.Id == input.Id);

    product.Translations.Clear();

    ObjectMapper.Map(input, product);
}
```

##### Note for EntityFramework 6.x [Anchor](https://aspnetboilerplate.com/Pages/Documents/Multi-Lingual-Entities\#note-for-entityframework-6-x)

For EntityFramework 6.x, all the translations must be deleted from database manually because Entity Framework 6.x doesn't delete related data. Instead, EntityFramework 6.x tries to set CoreId of each Translation entity to null which fails. So, a sample code like the below one might be used to delete translations of a Multi-Lingual entity for EntityFramework 6.x.

Copy

```
foreach (var translation in product.Translations.ToList())
{
    await _productTranslationRepository.DeleteAsync(translation);
    product.Translations.Remove(translation);
}
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe