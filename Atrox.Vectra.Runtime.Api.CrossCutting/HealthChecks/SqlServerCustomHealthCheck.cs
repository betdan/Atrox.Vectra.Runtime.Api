namespace CrossCutting.HealthChecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Configuration;

    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;

    public class SqlServerCustomHealthCheck : IHealthCheck
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly string _databaseEngine;

        public SqlServerCustomHealthCheck(IDatabaseConnection databaseConnection, IConfiguration configuration)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            _databaseEngine = configuration["Database:Engine"] ?? "SqlServer";
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = await _databaseConnection.GetOpenConnectionAsync();

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    return HealthCheckResult.Healthy($"{_databaseEngine} database connection is healthy.");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Unable to open the database connection.");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
            }
        }
    }
}


