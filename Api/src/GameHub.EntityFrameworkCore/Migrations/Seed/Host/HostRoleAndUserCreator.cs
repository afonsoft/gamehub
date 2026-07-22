using Abp.Authorization.Users;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using GameHub.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace GameHub.Migrations.Seed.Host
{
    public class HostRoleAndUserCreator
    {
        private readonly GameHubDbContext _context;

        public HostRoleAndUserCreator(GameHubDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateHostRoleAndUsers();
        }

        private void CreateHostRoleAndUsers()
        {
            //Admin role for host
            var adminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
            if (adminRoleForHost == null)
            {
                adminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Admin, StaticRoleNames.Host.Admin) { IsStatic = true, IsDefault = true }).Entity;
                _context.SaveChanges();
            }

            //admin user for host
            var adminUserForHost = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == AbpUserBase.AdminUserName);
            if (adminUserForHost == null)
            {
                var user = new User
                {
                    TenantId = null,
                    UserName = AbpUserBase.AdminUserName,
                    Name = "admin",
                    Surname = "gamehub",
                    EmailAddress = "gamehub@afonsoft.com.br",
                    IsEmailConfirmed = true,
                    ShouldChangePasswordOnNextLogin = true,
                    IsActive = true,
                };

                user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(user, "123qwe");
                user.SetNormalizedNames();

                adminUserForHost = _context.Users.Add(user).Entity;
                _context.SaveChanges();

                //Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(null, adminUserForHost.Id, adminRoleForHost.Id));
                _context.SaveChanges();

                //User account of admin user
                _context.UserAccounts.Add(new UserAccount
                {
                    TenantId = null,
                    UserId = adminUserForHost.Id,
                    UserName = AbpUserBase.AdminUserName,
                    EmailAddress = adminUserForHost.EmailAddress
                });

                _context.SaveChanges();
            }
        }
    }
}