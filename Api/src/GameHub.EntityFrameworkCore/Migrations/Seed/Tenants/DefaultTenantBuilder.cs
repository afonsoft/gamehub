using Abp.MultiTenancy;
using Eaf.Middleware.MultiTenancy;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GameHub.Migrations.Seed.Tenants
{
    public class DefaultTenantBuilder
    {
        private readonly GameHubDbContext _context;

        public DefaultTenantBuilder(
            GameHubDbContext context
        )
        {
            _context = context;
        }

        public void Create()
        {
            var defaultTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant(AbpTenantBase.DefaultTenantName, AbpTenantBase.DefaultTenantName);
                _context.Tenants.Add(defaultTenant);
                _context.SaveChanges();
            }
        }
    }
}