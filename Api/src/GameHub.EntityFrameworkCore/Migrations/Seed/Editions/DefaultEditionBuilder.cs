using System.Linq;
using Abp.Application.Editions;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Migrations.Seed.Editions
{
    public class DefaultEditionBuilder
    {
        public const string FreeEditionName = "Free";

        private readonly GameHubDbContext _context;

        public DefaultEditionBuilder(GameHubDbContext context)
        {
            _context = context;
        }

        public Edition Create()
        {
            var edition = _context.Editions.IgnoreQueryFilters().FirstOrDefault(e => e.Name == FreeEditionName);
            if (edition == null)
            {
                edition = new Edition
                {
                    Name = FreeEditionName,
                    DisplayName = FreeEditionName,
                };
                _context.Editions.Add(edition);
                _context.SaveChanges();
            }

            return edition;
        }
    }
}
