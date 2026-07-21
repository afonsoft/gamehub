using Abp.MultiTenancy;
using Eaf.Middleware.MultiTenancy;
using Eaf.ProjectName.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Eaf.ProjectName.Migrations.Seed.Tenants
{
    public class DefaultTenantBuilder
    {
        private readonly ProjectNameDbContext _context;

        public DefaultTenantBuilder(
            ProjectNameDbContext context
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