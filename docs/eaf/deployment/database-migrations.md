



# Database Migrations in EAF

This document describes how to manage database migrations in the Enterprise Application Framework (EAF).

## 1. Introduction

Database migrations are used to update the database schema to match the current version of the application. EAF uses Entity Framework Core migrations to manage database schema changes.

## 2. Prerequisites

*   **.NET SDK**: Ensure you have the .NET SDK installed (version 9.0 is recommended).
*   **Entity Framework Core Tools**: Ensure that the Entity Framework Core tools are installed.

## 3. Adding a Migration

To add a migration, you can use the `dotnet ef migrations add` command.

```bash
dotnet ef migrations add MyMigration -p Eaf.EntityFrameworkCore -s Eaf.Web.Mvc
```

## 4. Applying Migrations

To apply migrations, you can use the `dotnet ef database update` command.

```bash
dotnet ef database update -p Eaf.EntityFrameworkCore -s Eaf.Web.Mvc
```

## 5. Reverting Migrations

To revert migrations, you can use the `dotnet ef database update` command with the name of the migration to revert to.

```bash
dotnet ef database update MyMigration -p Eaf.EntityFrameworkCore -s Eaf.Web.Mvc
```

## 6. Best Practices

*   **Use Migrations**: Use migrations to manage database schema changes.
*   **Test Migrations**: Test migrations thoroughly to ensure that they update the database schema correctly.
*   **Use a Migration Strategy**: Use a migration strategy to manage database schema changes in a production environment.





