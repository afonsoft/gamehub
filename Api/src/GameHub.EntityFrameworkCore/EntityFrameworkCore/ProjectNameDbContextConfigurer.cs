using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace GameHub.EntityFrameworkCore
{
    /// <summary>
    /// Configures the database provider for the application DbContext.
    /// Supported providers: SqlServer (default), PostgreSQL, MySQL.
    /// </summary>
    public static class ProjectNameDbContextConfigurer
    {
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
        /// </param>
        public static void Configure(
            DbContextOptionsBuilder<ProjectNameDbContext> builder,
            string connectionString,
            string databaseProvider = "SqlServer")
        {
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
            DbContextOptionsBuilder<ProjectNameDbContext> builder,
            DbConnection connection,
            string databaseProvider = "SqlServer")
        {
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