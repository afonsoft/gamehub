using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace GameHub.EntityFrameworkCore
{
    /// <summary>
    /// Configures the database provider for the application DbContext.
    /// Supported providers: SqlServer (default), PostgreSQL, MySQL.
    /// </summary>
    public static class GameHubDbContextConfigurer
    {
        /// <summary>
        /// Resolves the database provider name from the explicit value, the environment variable
        /// <c>Database__Provider</c> (or <c>Database:Provider</c> at design-time), falling back to SQL Server.
        /// </summary>
        private static string ResolveDatabaseProvider(string databaseProvider)
        {
            if (!string.IsNullOrWhiteSpace(databaseProvider))
                return databaseProvider;

            return Environment.GetEnvironmentVariable("Database__Provider")
                ?? Environment.GetEnvironmentVariable("Database:Provider")
                ?? "SqlServer";
        }

        /// <summary>
        /// Configures the DbContext with the specified database provider using a connection string.
        /// </summary>
        /// <param name="builder">The DbContext options builder.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">
        /// The database provider name. Supported values:
        /// "SqlServer" or "MSSQL" (default),
        /// "PostgreSQL", "Postgres", or "Npgsql",
        /// "MySQL", "MariaDB", or "Pomelo".
        /// When not supplied, the value is read from the <c>Database__Provider</c> environment variable.
        /// </param>
        public static void Configure(
            DbContextOptionsBuilder<GameHubDbContext> builder,
            string connectionString,
            string databaseProvider = null)
        {
            databaseProvider = ResolveDatabaseProvider(databaseProvider);

            switch (databaseProvider?.ToUpperInvariant())
            {
                case "POSTGRESQL":
                case "POSTGRES":
                case "NPGSQL":
                    builder.UseNpgsql(connectionString);
                    break;

                case "MYSQL":
                case "MARIADB":
                case "POMELO":
                    builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    break;

                default:
                    builder.UseSqlServer(connectionString);
                    break;
            }
        }

        /// <summary>
        /// Configures the DbContext with the specified database provider using an existing connection.
        /// </summary>
        /// <param name="builder">The DbContext options builder.</param>
        /// <param name="connection">The existing database connection.</param>
        /// <param name="databaseProvider">The database provider name (see overload for supported values).</param>
        public static void Configure(
            DbContextOptionsBuilder<GameHubDbContext> builder,
            DbConnection connection,
            string databaseProvider = null)
        {
            databaseProvider = ResolveDatabaseProvider(databaseProvider);

            switch (databaseProvider?.ToUpperInvariant())
            {
                case "POSTGRESQL":
                case "POSTGRES":
                case "NPGSQL":
                    builder.UseNpgsql(connection);
                    break;

                case "MYSQL":
                case "MARIADB":
                case "POMELO":
                    builder.UseMySql(connection, ServerVersion.AutoDetect(connection.ConnectionString));
                    break;

                default:
                    builder.UseSqlServer(connection);
                    break;
            }
        }
    }
}