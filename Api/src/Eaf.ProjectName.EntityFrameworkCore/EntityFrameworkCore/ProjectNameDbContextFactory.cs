using Eaf.Middleware.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Eaf.ProjectName.EntityFrameworkCore
{
    /* THIS CLASS IS NEEDED TO RUN "DOTNET EF ..." COMMANDS FROM COMMAND LINE ON DEVELOPMENT. NOT USED ANYWHERE ELSE */

    public class ProjectNameDbContextFactory : IDesignTimeDbContextFactory<ProjectNameDbContext>
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> _configurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();

        public ProjectNameDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ProjectNameDbContext>();
            var configuration = GetConfigurationRoot();
            var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";
            ProjectNameDbContextConfigurer.Configure(
                builder,
                configuration.GetConnectionString(ProjectNameConsts.ConnectionStringName),
                databaseProvider);

            return new ProjectNameDbContext(builder.Options);
        }

        public static IConfigurationRoot GetConfigurationRoot()
        {
            var path = CalculateContentRootFolder();
            return _configurationCache.GetOrAdd(path, p => buildConfiguration(p));
        }

        private static IConfigurationRoot buildConfiguration(string path)
        {
            return AppConfigurations.Get(path);
        }

        private static string CalculateContentRootFolder()
        {
            var coreAssemblyDirectoryPath = Path.GetDirectoryName(typeof(ProjectNameDbContextFactory).Assembly.Location);
            if (coreAssemblyDirectoryPath == null)
            {
                throw new InvalidOperationException($"Could not find location of Suite.Docs.Core assembly!");
            }

            var directoryInfo = new DirectoryInfo(coreAssemblyDirectoryPath);

            while (!DirectoryContains(directoryInfo.FullName, "appsettings.json"))
            {
                if (directoryInfo.Parent == null)
                    throw new InvalidOperationException($"Could not find content root folder!");

                directoryInfo = directoryInfo.Parent;
            }

            return directoryInfo.FullName;
        }

        private static bool DirectoryContains(string directory, string fileName)
        {
            return Directory.GetFiles(directory).Any(filePath => string.Equals(Path.GetFileName(filePath), fileName));
        }
    }
}