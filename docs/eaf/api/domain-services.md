


# Domain Services in EAF

This document describes the domain services used in the Enterprise Application Framework (EAF).

## 1. Introduction

Domain services are responsible for implementing the core business logic of the application. They are typically used to encapsulate complex business rules and logic that do not naturally belong to an entity.

## 2. Key Concepts

*   **Domain Service**: A class that implements the core business logic of the application.
*   **Domain Logic**: The business rules and logic that govern the behavior of the application.
*   **Stateless**: Domain services should be stateless and should not maintain any internal state.
*   **Transaction**: A unit of work that is performed on the database.

## 3. Implementing Domain Services

To implement a domain service in EAF, you can create a class that implements the `IDomainService` interface and add methods to implement the business logic.

```csharp
// Example
using Abp.Domain.Services;
using Abp.Domain.Repositories;
using System.Threading.Tasks;

public interface IMyDomainService : IDomainService
{
    Task DoSomething(int id);
}

public class MyDomainService : DomainService, IMyDomainService
{
    private readonly IRepository<MyEntity> _myRepository;

    public MyDomainService(IRepository<MyEntity> myRepository)
    {
        _myRepository = myRepository;
    }

    public async Task DoSomething(int id)
    {
        var entity = await _myRepository.GetAsync(id);
        // Implement business logic here
    }
}
```

## 4. Best Practices

*   **Keep Domain Services Focused**: Keep domain services focused on implementing core business logic.
*   **Make Domain Services Stateless**: Make domain services stateless to improve scalability and testability.
*   **Use Transactions**: Use transactions to ensure data consistency.
*   **Test Domain Services Thoroughly**: Test domain services thoroughly to ensure that they implement the business logic correctly.




