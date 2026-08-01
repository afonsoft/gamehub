using Abp.MultiTenancy;
using Eaf.Middleware.MultiTenancy;
using GameHub.EntityFrameworkCore;
using GameHub.Migrations.Seed.Editions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GameHub.Migrations.Seed.Tenants
{
    public class PlayerTenantBuilder
    {
        private readonly GameHubDbContext _context;

        public PlayerTenantBuilder(
            GameHubDbContext context
        )
        {
            _context = context;
        }

        public void Create()
        {
            var playerTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == GameHubConsts.PlayerTenantName);
            if (playerTenant == null)
            {
                var edition = new DefaultEditionBuilder(_context).Create();
                playerTenant = new Tenant(GameHubConsts.PlayerTenantName, GameHubConsts.PlayerTenantName)
                {
                    EditionId = edition.Id,
                };
                _context.Tenants.Add(playerTenant);
                _context.SaveChanges();
            }
        }
    }
}
