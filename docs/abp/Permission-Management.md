# https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management

## Permission Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Permission-Management.md)

In this document

#### About Authorization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management\#about-authorization)

We strongly recommend that you read the [authorization\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Authorization) before this one.

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management\#introduction)

Module Zero implements the **IPermissionChecker** interface of ASP.NET
Boilerplate's authorization system. To define and check permissions,
see the [authorization document](https://aspnetboilerplate.com/Pages/Documents/Authorization). In
this document, we will show you how to grant permissions for roles and users.

### Role Permissions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management\#role-permissions)

If we **grant** a permission to a role, all the users that have this role are
authorized for the permission (unless explicitly prohibited for a
specific user).

We use the **RoleManager** to change the permissions of a Role. For example,
**SetGrantedPermissionsAsync** can be used to change all the permissions of
a role in one method call:

Copy

```
public class RoleAppService : IRoleAppService
{
    private readonly RoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;

    public RoleAppService(RoleManager roleManager, IPermissionManager permissionManager)
    {
        _roleManager = roleManager;
        _permissionManager = permissionManager;
    }

    public async Task UpdateRolePermissions(UpdateRolePermissionsInput input)
    {
        var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
        var grantedPermissions = _permissionManager
            .GetAllPermissions()
            .Where(p => input.GrantedPermissionNames.Contains(p.Name))
            .ToList();

        await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
    }
}
```

In this example, we get a **RoleId** and a list of granted permission
names (input.GrantedPermissionNames is a List<string>) as an input.
We then use the **IPermissionManager** to find all the **Permission** objects by
name. After that, we call the **SetGrantedPermissionsAsync** method to update
the permissions of the role.

There are also other methods like GrantPermissionAsync and
ProhibitPermissionAsync to control the permissions one-by-one.

### User Permissions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management\#user-permissions)

While the role-based permission management can be enough for most
applications, we may need to control the permissions per user. When we
define a permission setting for a user, it overrides the permission setting
defined for the roles of the user.

As an example, imagine that we have an application service method for prohibiting a
permission for a user:

Copy

```
public class UserAppService : IUserAppService
{
    private readonly UserManager _userManager;
    private readonly IPermissionManager _permissionManager;

    public UserAppService(UserManager userManager, IPermissionManager permissionManager)
    {
        _userManager = userManager;
        _permissionManager = permissionManager;
    }

    public async Task ProhibitPermission(ProhibitPermissionInput input)
    {
        var user = await _userManager.GetUserByIdAsync(input.UserId);
        var permission = _permissionManager.GetPermission(input.PermissionName);

        await _userManager.ProhibitPermissionAsync(user, permission);
    }
}
```

The UserManager has many methods to control the permissions of users. In this
example, we're getting a UserId and PermissionName and using the
UserManager's **ProhibitPermissionAsync** method to prohibit a
permission for a user.

When we **prohibit** a permission for a user, he/she **cannot** be
authorized for this permission even his/her roles are **granted** for
the permission. We can use the same principle for granting permissions. When we
**grant** a permission specifically for a user, this user **is granted**
the permission even if the roles of the user are not granted the
permission. We can use the **ResetAllPermissionsAsync** method to delete
all user-specific permission settings for a user.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe