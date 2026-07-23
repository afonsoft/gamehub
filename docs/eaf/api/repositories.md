


# Repositories in EAF

This document describes the repositories used in the Enterprise Application Framework (EAF).

## 1. Introduction

Repositories are responsible for abstracting the data access logic from the rest of the application. They provide a consistent interface for accessing and manipulating data.

## 2. Key Concepts

*   **Repository**: An interface that defines the methods for accessing and manipulating data.
*   **Entity**: A class that represents a table in the database.
*   **Data Access Logic**: The code that is used to access and manipulate data in the database.
*   **Abstraction**: The process of hiding the implementation details of the data access logic.

## 3. Implementing Repositories

To implement a repository in EAF, you can create an interface that inherits from the `IRepository` interface and add methods to access and manipulate data.

```csharp
// Example
using Abp.Domain.Repositories;
using System.Threading.Tasks;

public interface IMyRepository : IRepository<MyEntity>
{
    Task<MyEntity> GetByName(string name);
}

public class MyRepository : EfCoreRepositoryBase<MyDbContext, MyEntity>, IMyRepository
{
    public MyRepository(IDbContextProvider<MyDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<MyEntity> GetByName(string name)
    {
        return await FirstOrDefaultAsync(e => e.Name == name);
    }
}
```

## 4. Best Practices

*   **Use Repositories**: Use repositories to abstract data access logic.
*   **Keep Repositories Simple**: Keep repositories simple and focused on data access.
*   **Test Repositories Thoroughly**: Test repositories thoroughly to ensure that they access and manipulate data correctly.
*   **Use Asynchronous Operations**: Use asynchronous operations to avoid blocking the main thread.




