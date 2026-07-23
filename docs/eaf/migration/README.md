# Migration Documentation

## Overview

This section provides documentation for migrating the EAF system between different versions, frameworks, and platforms. Migration guides help you upgrade your EAF applications while minimizing breaking changes and ensuring compatibility.

## Available Migrations

### Angular UI Migrations

The Angular UI template has undergone several major version upgrades. Detailed migration guides are available in the template's documentation:

- **[Angular 12 to 15 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_12_TO_15.md)**: Guide for upgrading from Angular 12 to Angular 15
- **[Angular 15 to 17 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_15_TO_17.md)**: Guide for upgrading from Angular 15 to Angular 17
- **[Angular 17 to 18 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_17_TO_18.md)**: Guide for upgrading from Angular 17 to Angular 18
- **[Angular 17 to 19 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_17_TO_19.md)**: Guide for upgrading from Angular 17 to Angular 19
- **[Angular 18 to 19 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_18_TO_19.md)**: Guide for upgrading from Angular 18 to Angular 19
- **[Angular 19 to 20 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_19_TO_20.md)**: Guide for upgrading from Angular 19 to Angular 20
- **[Angular 20 to 21 Migration](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_ANGULAR_20_TO_21.md)**: Guide for upgrading from Angular 20 to Angular 21

### Migration Summaries

Quick reference summaries for major version changes:

- **[Angular 15 Migration Summary](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_SUMMARY_15.md)**: Summary of changes in Angular 15
- **[Angular 17 Migration Summary](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_SUMMARY_17.md)**: Summary of changes in Angular 17
- **[Angular 18 Migration Summary](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_SUMMARY_18.md)**: Summary of changes in Angular 18
- **[Angular 19 Migration Summary](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_SUMMARY_19.md)**: Summary of changes in Angular 19
- **[Angular 20 Migration Summary](../../Templates/Angular/Eaf.ProjectName.UI/docs/MIGRATION_SUMMARY_20.md)**: Summary of changes in Angular 20

## .NET Framework Migrations

### ASP.NET Core Upgrades

The EAF API and Worker templates use ASP.NET Core. When upgrading .NET versions, consider:

- **Breaking Changes**: Check ASP.NET Core release notes for breaking changes
- **Package Updates**: Update all NuGet packages to compatible versions
- **API Changes**: Update deprecated API calls
- **Configuration**: Review configuration changes between versions

### Entity Framework Core Migrations

When upgrading Entity Framework Core:

- **Review Migration Guide**: Check EF Core migration documentation
- **Update DbContext**: Update DbContext for new features
- **Test Migrations**: Test all database migrations
- **Performance**: Review performance improvements and changes

## Database Migrations

### Running Migrations

The EAF templates use Entity Framework Core for database migrations:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/Eaf.ProjectName.EntityFrameworkCore

# Update database
dotnet ef database update --project src/Eaf.ProjectName.EntityFrameworkCore
```

### Rollback Migrations

To rollback to a previous migration:

```bash
dotnet ef database update PreviousMigration --project src/Eaf.ProjectName.EntityFrameworkCore
```

### Seed Data

The EAF templates include seed data for:

- Default admin user
- Default tenant
- Default roles and permissions
- Default languages

Seed data is applied automatically on first database creation.

## Multi-Tenancy Migrations

### Tenant Database Migration

For multi-tenant applications with separate databases per tenant:

1. Migrate host database first
2. Migrate each tenant database individually
3. Use the migrator tool for automated tenant migration

### Tenant Data Migration

When migrating tenant data:

- Backup tenant databases before migration
- Test migration on a staging tenant
- Verify data integrity after migration
- Monitor performance after migration

## Best Practices

### Before Migration

1. **Backup**: Always backup your database and code before migration
2. **Test**: Test migration on a staging environment first
3. **Read Documentation**: Read all migration guides thoroughly
4. **Plan**: Plan for potential rollback scenarios
5. **Communicate**: Communicate migration schedule to stakeholders

### During Migration

1. **Monitor**: Monitor migration progress and logs
2. **Test**: Test critical functionality after each step
3. **Validate**: Validate data integrity
4. **Document**: Document any custom changes made during migration

### After Migration

1. **Test**: Thoroughly test all functionality
2. **Monitor**: Monitor application performance and logs
3. **Cleanup**: Remove deprecated code and dependencies
4. **Document**: Document the migration process for future reference
5. **Train**: Train team members on new features and changes

## Troubleshooting

### Common Migration Issues

**Migration Fails**

- Check database connection string
- Verify database permissions
- Review migration logs for specific errors
- Ensure all dependencies are installed

**Breaking Changes After Upgrade**

- Review breaking changes in release notes
- Update deprecated API calls
- Update configuration settings
- Test affected functionality

**Performance Issues After Migration**

- Review performance improvements in new version
- Update queries for new EF Core features
- Review caching strategy
- Monitor database performance

## Support

For migration issues:

- Check the specific migration guide for your upgrade path
- Review the [Angular UI documentation](../../Templates/Angular/Eaf.ProjectName.UI/docs/)
- Review the [API documentation](../../Templates/Api/docs/)
- Check the [Worker documentation](../../Templates/Worker/docs/)
- Consult the main EAF documentation

## Additional Resources

- [ASP.NET Core Migration Guide](https://docs.microsoft.com/aspnet/core/migration/)
- [Entity Framework Core Migration Guide](https://docs.microsoft.com/ef/core/what-is-new/ef-core-5.0-to-6.0-migration)
- [Angular Update Guide](https://update.angular.io/)
- [.NET Upgrade Assistant](https://docs.microsoft.com/dotnet/core/upgrade-assistant/overview)
