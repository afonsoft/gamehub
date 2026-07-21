using Eaf.ProjectName.EntityFrameworkCore;

namespace Eaf.ProjectName.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly ProjectNameDbContext _context;

        public InitialHostDbBuilder(
            ProjectNameDbContext context
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