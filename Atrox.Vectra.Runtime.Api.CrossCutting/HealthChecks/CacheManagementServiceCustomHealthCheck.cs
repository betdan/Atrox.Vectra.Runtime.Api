namespace CrossCutting.HealthChecks
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class CacheManagementServiceCustomHealthCheck(IConfiguration configuration) : IHealthCheck
    {
        static readonly HttpClient client = new HttpClient();
        private readonly IConfiguration _configuration = configuration;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                bool healthCheckEnabled = _configuration.GetValue<bool>("HealthCheck:status");

                if (!healthCheckEnabled)
                {
                    return HealthCheckResult.Healthy();
                }
                string healthCheckUrl = _configuration.GetValue<string>("HealthCheck:url_CacheManagementeService");


                var response = await client.GetAsync(healthCheckUrl, CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy();
                }
                else
                {
                    return HealthCheckResult.Unhealthy();
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
