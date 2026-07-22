using Abp.Localization;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GameHub.Migrations.Seed.Host
{
    public class DefaultLanguagesCreator
    {
        public static List<ApplicationLanguage> InitialLanguages => GetInitialLanguages();
        private readonly GameHubDbContext _context;

        private static List<ApplicationLanguage> GetInitialLanguages()
        {
            var tenantId = GameHubConsts.MultiTenancyEnabled ? null : (int?)1;
            return new List<ApplicationLanguage>
            {
                new ApplicationLanguage(tenantId, "pt-BR", "Português (Brasil)", "famfamfam-flags br"),
                new ApplicationLanguage(tenantId, "en", "English", "famfamfam-flags us"),
                new ApplicationLanguage(tenantId, "es", "Español", "famfamfam-flags es")
            };
        }

        public DefaultLanguagesCreator(
            GameHubDbContext context
        )
        {
            _context = context;
        }

        public void Create()
        {
            foreach (var language in InitialLanguages)
            {
                var existingLanguage = _context.Languages.IgnoreQueryFilters()
                    .FirstOrDefault(l => l.TenantId == language.TenantId && l.Name == language.Name);

                if (existingLanguage != null)
                {
                    existingLanguage.DisplayName = language.DisplayName;
                    existingLanguage.Icon = language.Icon;
                }
                else
                {
                    _context.Languages.Add(language);
                }

                _context.SaveChanges();
            }
        }
    }
}