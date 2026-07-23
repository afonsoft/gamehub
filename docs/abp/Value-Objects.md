# https://aspnetboilerplate.com/Pages/Documents/Value-Objects

## Value Objects

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Value-Objects.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Value-Objects\#introduction)

" _An object that represents a descriptive aspect of the domain with no_
_conceptual identity is called a VALUE OBJECT._" (Eric Evans).

[Entities](https://aspnetboilerplate.com/Pages/Documents/Entities) have identities
(Id), Value Objects do not. If the identities of two
Entities are different, they are considered as different
objects/entities even if all the properties of those entities are the
same. Imagine two different people that have the same Name, Surname and Age but
are different people (their identity numbers are different). For an Address class (which
is a classic Value Object), if the two addresses have the same Country, City, and Street number, etc,
they are considered to be the same address.

In Domain Driven Design (DDD), the Value Object is another type of domain
object which can include business logic and is an essential part of the
domain.

### Value Object Base Classes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Value-Objects\#value-object-base-classes)

ABP has a class for value objects: **ValueObject** . For example, all of these tests pass:

Copy

```csharp
var address1 = new Address(new Guid("21C67A65-ED5A-4512-AA29-66308FAAB5AF"), "Baris Manco Street", 42);
var address2 = new Address(new Guid("21C67A65-ED5A-4512-AA29-66308FAAB5AF"), "Baris Manco Street", 42);

Assert.True(address1.ValueEquals(address2));
Assert.True(address2.ValueEquals(address1));
```

Even if they are different objects in memory, they are identical for our domain.

#### ValueObject [Anchor](https://aspnetboilerplate.com/Pages/Documents/Value-Objects\#valueobject)

Here's an example **Address** that inherits from the **ValueObject** class:

Copy

```csharp
public class Address : ValueObject
{
    public Guid CityId { get; }

    public string Street { get; }

    public int Number { get; }

    public Address(
        Guid cityId,
        string street,
        int number)
    {
        CityId = cityId;
        Street = street;
        Number = number;
    }

    //Requires to implement this method to return properties.
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Street;
        yield return CityId;
        yield return Number;
    }
}
```

### Best Practices [Anchor](https://aspnetboilerplate.com/Pages/Documents/Value-Objects\#best-practices)

Here are some best practices when using Value Objects:

- Design a value object as **immutable** (like the Address above)
if there is not a good reason for designing it as mutable.
- The properties that make up a Value Object should form a conceptual
whole. For example, CityId, Street and Number shouldn't be separate
properties of a Person entity. This also makes the Person entity
simpler.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |