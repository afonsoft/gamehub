


# Entities in EAF

This document describes the entities used in the Enterprise Application Framework (EAF).

## 1. Introduction

Entities are the core building blocks of the domain model. They represent real-world objects or concepts and have a unique identity that persists over time.

## 2. Key Concepts

*   **Entity**: A class that represents a table in the database and has a unique identity.
*   **Properties**: Attributes of an entity that store data.
*   **Identity**: A unique identifier for an entity.
*   **Domain Logic**: The business rules and logic that govern the behavior of the entity.

## 3. Implementing Entities

To implement an entity in EAF, you can create a class that inherits from the `Entity` class and add properties to represent the data.

```csharp
// Example
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class MyEntity : Entity<int>
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }
}
```

## 4. Best Practices

*   **Keep Entities Simple**: Keep entities simple and focused on representing data.
*   **Use Meaningful Names**: Use meaningful names for entities and properties.
*   **Enforce Business Rules**: Enforce business rules within the entity to ensure data integrity.
*   **Use Value Objects**: Use value objects to represent complex data types.





