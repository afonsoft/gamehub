# https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management

## Role Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Role-Management.md)

In this document

### Role Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management\#role-entity)

The Role entity represents a **role for the application**. It should be
derived from the **AbpRole** class as shown below:

Copy

```
public class Role : AbpRole<Tenant, User>
{
    //add your own role properties here
}
```

This class will be created when you download an ABP template with the option in the below image is selected.

![Login Page](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/include_module_zero_checkbox.png)

Roles are
stored in the **AbpRoles** table in the database. You can add your own custom
properties to the Role class (and create database migrations for the
changes).

AbpRole defines some properties. The most important are:

- **Name**: Unique name of the role in the tenant.
- **DisplayName**: Shown name of the role.
- **IsDefault**: Is this role assigned to new users by default?
- **IsStatic**: Is this role static? (setup during pre-build, and can not be
deleted).

Roles are used to **group permissions**. When a user has a role, then
he/she will have all the permissions of that role. A user can have
**multiple** roles. The Permissions of this user will be a merge of all the
permissions of all assigned roles.

#### Dynamic vs Static Roles [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management\#dynamic-vs-static-roles)

In Module Zero, roles can be dynamic or static:

- **Static role**: A static role has a known **name** (like 'admin')
which can not be changed (we can change the **display name**). It
exists on the system startup and can not be deleted. This way, we can
write code based on a static role's name.
- **Dynamic (non static) role**: We can create a dynamic role after
deployment. We can then grant permissions for that role, we can
assign the role to some users, and we can delete it. We do not know the
names of dynamic roles during development.

Use the **IsStatic** property to set it for a role. We must also
**register** static roles in
the [PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System) method of our module. Assume
that we have an "Admin" static role for tenants:

Copy

```
Configuration.Modules.Zero().RoleManagement.StaticRoles.Add(new StaticRoleDefinition("Admin", MultiTenancySides.Tenant));
```

This way, Module Zero will be aware of static roles.

#### Default Roles [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management\#default-roles)

One or more roles can be set as **default**. Default roles are assigned
to newly added/registered users by default. This is not a development time
property and can be set or changed after deployment. Use the **IsDefault**
property to set it.

### Role Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management\#role-manager)

**RoleManager** is a service to perform **domain logic** for roles:

Copy

```
public class RoleManager : AbpRoleManager<Tenant, Role, User>
{
    //...
}
```

You can [inject](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and use
the RoleManager to create, delete, update roles, grant permissions for roles
and much more. You can add your own methods here, too. You can also
**override** any method of the **AbpRoleManager** base class for your own
needs.

Like the UserManager, some methods of the RoleManager also return IdentityResult
as a result instead of throwing exceptions. See the [user\\
management](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management) document for more
information.

### Multi-Tenancy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management\#multi-tenancy)

Similar to user management, role management also works for a tenant
in a multi-tenant application. See the [user\\
management](https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management#multi-tenancy)
document for more information.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |