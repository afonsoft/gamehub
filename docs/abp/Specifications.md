# https://aspnetboilerplate.com/Pages/Documents/Specifications

## Specifications

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Specifications.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#introduction)

The **_specification pattern_** _is a particular software design pattern,_
_whereby business rules can be recombined by chaining the business rules_
_together using boolean logic_
( [Wikipedia](https://en.wikipedia.org/wiki/Specification_pattern)).

In practical terms, it's used mostly **to define reusable filters** for
entities or other business objects.

#### Example [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#example)

In this section, we will see **the need for the specification pattern**.
This section is generic and not related to ABP's implementation.

Assume that you have a service method that calculates the total count of
your customers as shown below:

Copy

```
public class CustomerManager
{
    public int GetCustomerCount()
    {
        //TODO...
        return 0;
    }
}
```

You probably will want to get a customer count with a filter. For example,
you may have **premium customers** (which have a balance of more than
$100,000) or you may want to filter customers just by **registration**
**year**. Then you can create other methods like
_GetPremiumCustomerCount()_, _GetCustomerCountRegisteredInYear(int_
_year)_, _GetPremiumCustomerCountRegisteredInYear(int year)_ and more. As
you have more criteria, it's not possible to create a combination for
every possibility.

One solution to this problem is the **specification pattern**. We could
create a single method that gets **a parameter as the filter**:

Copy

```
public class CustomerManager
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerManager(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public int GetCustomerCount(ISpecification<Customer> spec)
    {
        var customers = _customerRepository.GetAllList();

        var customerCount = 0;

        foreach (var customer in customers)
        {
            if (spec.IsSatisfiedBy(customer))
            {
                customerCount++;
            }
        }

        return customerCount;
    }
}
```

This way, we can get any object as a parameter that implements the
**ISpecification<Customer>** interface which is defined as shown
below:

Copy

```
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T obj);
}
```

We can call **IsSatisfiedBy** with a customer to test if this
customer is intended. This way, we can use same GetCustomerCount with
different filters **without changing the method** itself.

While this solution is pretty fine in theory, it could be improved to
work better in C#. For instance, it's **not efficient** to get all
customers from a database to check if they satisfy the given
specification/condition. In the next section, we will see ABP's
implementation which overcomes this problem.

### Creating Specification Classes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#creating-specification-classes)

ABP defines the ISpecification interface as shown below:

Copy

```
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T obj);

    Expression<Func<T, bool>> ToExpression();
}
```

Includes a **ToExpression()** method which returns an expression and is used to
better integrate with **IQueryable** and **Expression trees**. This way, we
can easily pass a specification to a repository to apply a filter at the
database level.

We generally inherit from the **Specification<T> class** instead of
directly implementing the ISpecification<T> interface. A Specification
class automatically implements the IsSatisfiedBy method, so we only need to
define ToExpression. Let's create some specification classes:

Copy

```
//Customers with $100,000+ balance are assumed as PREMIUM customers.
public class PremiumCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return (customer) => (customer.Balance >= 100000);
    }
}

//A parametric specification example.
public class CustomerRegistrationYearSpecification : Specification<Customer>
{
    public int Year { get; }

    public CustomerRegistrationYearSpecification(int year)
    {
        Year = year;
    }

    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return (customer) => (customer.CreationYear == Year);
    }
}
```

As you can see, we just implemented simple **lambda expressions** to define the
specifications. Let's use these specifications to get the counts of the
customers:

Copy

```
count = customerManager.GetCustomerCount(new PremiumCustomerSpecification());
count = customerManager.GetCustomerCount(new CustomerRegistrationYearSpecification(2017));
```

### Using a Specification With a Repository [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#using-a-specification-with-a-repository)

We can now **optimize** CustomerManager to **apply the filter in the**
**database**:

Copy

```
public class CustomerManager
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerManager(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public int GetCustomerCount(ISpecification<Customer> spec)
    {
        return _customerRepository.Count(spec.ToExpression());
    }
}
```

It's that simple. We can pass any specification to the repositories since the
[repositories](https://aspnetboilerplate.com/Pages/Documents/Repositories) can work with the expressions as filters.
In this example, CustomerManager is unnecessary since we could directly
use the repository with the specification to query the database. But imagine that
we want to execute a business operation on some customers. In that case,
we could use the specifications with a domain service to specify the customers
to work on.

### Composing Specifications [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#composing-specifications)

One powerful feature of specifications is that they are **composable**
with **And, Or, Not** and **AndNot** extension methods. Example:

Copy

```
var count = customerManager.GetCustomerCount(new PremiumCustomerSpecification().And(new CustomerRegistrationYearSpecification(2017)));
```

We can even create a new specification class from existing
specifications:

Copy

```
public class NewPremiumCustomersSpecification : AndSpecification<Customer>
{
    public NewPremiumCustomersSpecification()
        : base(new PremiumCustomerSpecification(), new CustomerRegistrationYearSpecification(2017))
    {
    }
}
```

**AndSpecification** is a subclass of the **Specification** class which
satisfies only if both specifications are satisfied. We can then use
NewPremiumCustomersSpecification just like any other specification:

Copy

```
var count = customerManager.GetCustomerCount(new NewPremiumCustomersSpecification());
```

### Discussion [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#discussion)

While the specification pattern is older than C# lambda expressions, it's
generally compared to expressions. Some developers may think it's not
needed anymore and we can directly pass expressions to a repository or
to a domain service as shown below:

Copy

```
var count = _customerRepository.Count(c => c.Balance > 100000 && c.CreationYear == 2017);
```

Since ABP's [Repository](https://aspnetboilerplate.com/Pages/Documents/Repositories) supports expessions, this is a
completely valid use. You don't have to define or use any
specification in your application and you can go with expressions.

So, what's the point of a specification? Why and when should we consider
using them?

#### When To Use? [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#when-to-use-)

Some benefits of using specifications:

- **Reusabe**: Imagine that you need the PremiumCustomer filter in many
places in your code base. If you go with expressions and do not create
a specification, what happens if you later change the "Premium Customer"
definition? Say you want to change the minimum balance from $100,000 to
$250,000 and add another condition to be a customer older than 3.
If you used a specification, you just change a single class. If
you repeated (copy/pasted) the same expression, you need to change all of
them.
- **Composable**: You can combine multiple specifications to create new
specifications. This is another type of reusability.
- **Named**: PremiumCustomerSpecification better explains the intent
rather than a complex expression. So, if you have an expression that
is meaningful in your business, consider using specifications.
- **Testable**: A specification is a separately (and easily) testable
object.

#### When To Not Use? [Anchor](https://aspnetboilerplate.com/Pages/Documents/Specifications\#when-to-not-use-)

- **Non business expressions**: Do not use
specifications for non business-related expressions and operations.
- **Reporting**: If you are just creating a report do not create
specifications, but directly use IQueryable. You can even
use plain SQL, Views or another tool for reporting. DDD does not necessarily care about
reporting, so the way you query the underlying data store can be important
from a performance perspective.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe