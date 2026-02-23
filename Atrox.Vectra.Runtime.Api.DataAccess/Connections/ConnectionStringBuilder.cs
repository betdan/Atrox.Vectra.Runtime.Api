namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using CrossCutting.Crypto;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;

    public class ConnectionStringBuilder(IConfiguration _configuration, ILogger<ConnectionStringBuilder> _log, ICrypto _crypto) : IConnectionStringBuilder
    {
        public string BuildSqlServerConnectionString()
        {
            return BuildConnectionStringFromSection("ConnectionStrings:SqlServer", "SQL Server");
        }

        public string BuildPostgreSqlConnectionString()
        {
            return BuildConnectionStringFromSection("ConnectionStrings:PostgreSql", "PostgreSQL");
        }

        public string BuildConnectionString(string engine)
        {
            if (string.Equals(engine, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                return BuildSqlServerConnectionString();
            }

            if (string.Equals(engine, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                return BuildPostgreSqlConnectionString();
            }

            throw new InvalidOperationException($"Unsupported database engine '{engine}'. Allowed values: SqlServer, PostgreSql.");
        }

        private string BuildConnectionStringFromSection(string sectionPath, string engineDisplayName)
        {
            var settings = _configuration.GetSection(sectionPath);

            if (!settings.Exists())
            {
                throw new InvalidOperationException($"{sectionPath} section was not found in appsettings.json.");
            }

            string connectionStringFormat = settings["ConnectionStringFormat"];
            string server = settings["Server"];
            string port = settings["Port"];
            string userId = settings["UserId"];
            string encryptedPassword = settings["Password"];
            string database = settings["Database"];

            if (string.IsNullOrEmpty(connectionStringFormat))
            {
                throw new InvalidOperationException($"{engineDisplayName} ConnectionStringFormat is missing in the configuration.");
            }

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
            {
                throw new InvalidOperationException($"One or more required {engineDisplayName} connection values are missing.");
            }

            string connectionString;

            if (string.IsNullOrEmpty(encryptedPassword))
            {
                connectionString = connectionStringFormat
                    .Replace("{server}", server)
                    .Replace("{port}", port)
                    .Replace("{database}", database);
            }
            else
            {
                string decryptedPassword = _crypto.Decrypt(encryptedPassword);
                connectionString = connectionStringFormat
                    .Replace("{server}", server)
                    .Replace("{port}", port)
                    .Replace("{userId}", userId)
                    .Replace("{decryptedPassword}", decryptedPassword)
                    .Replace("{database}", database);
            }

            _log.LogDebug("{engine} connection string built successfully.", engineDisplayName);

            return connectionString;
        }
    }
}



