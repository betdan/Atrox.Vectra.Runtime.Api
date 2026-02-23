namespace CrossCutting.Config
{
    using CrossCutting.Models.HealthCheckModels;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;

    using System.Text.Json;

    public static class HealthCheckConfig
    {
        public static IApplicationBuilder AddRegistration(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new HealthCheckReponse
                    {
                        Status = report.Status.ToString(),
                        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        }),
                        HealthCheckDuration = report.TotalDuration.ToString()
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });

            return app;

        }
    }
}
