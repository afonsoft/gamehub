using GameHub.EntityFrameworkCore;

namespace GameHub.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly GameHubDbContext _context;

        public InitialHostDbBuilder(
            GameHubDbContext context
        )
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}