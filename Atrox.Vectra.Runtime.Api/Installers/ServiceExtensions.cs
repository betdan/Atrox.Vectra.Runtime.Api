namespace Atrox.Vectra.Runtime.Api.Installers
{
    using CrossCutting.HealthChecks;
    using CrossCutting.Metrics;
    using global::MassTransit;
    using global::Atrox.Vectra.Runtime.Api.DataAccess.Connections;
    using global::Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
    using global::Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Executions;
    using System.Text.Json;
    using Prometheus.SystemMetrics;
    using global::Atrox.Vectra.Runtime.Api.Transports.Amqp;
    using global::Atrox.Vectra.Runtime.Api.DataAccess.Executions;
    using CrossCutting.Xml;
    using Microsoft.OpenApi.Models;
    using global::Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using global::Atrox.Vectra.Runtime.Api.Application.Services;

    public static class ServiceExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseEngine = configuration["Database:Engine"];

            if (!string.Equals(databaseEngine, "SqlServer", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(databaseEngine, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid Database:Engine value. Allowed values: SqlServer, PostgreSql.");
            }

            services.AddMemoryCache();
            services.AddScoped<IAtroxVectraRuntimeApiExecute, AtroxVectraRuntimeApiExecute>();
            services.AddScoped<IConnectionStringBuilder, ConnectionStringBuilder>();
            services.AddTransient<MetricCollector>();
            services.AddSystemMetrics();
            services.AddScoped<IAtroxVectraRuntimeApiServices, AtroxVectraRuntimeApiServices>();
            services.AddScoped<IExecutionService, AtroxVectraRuntimeApiServices>();

            if (string.Equals(databaseEngine, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IStoredProcedureParameterService, StoredProcedureParameterService>();
                services.AddScoped<IDatabaseConnection, SqlServerDatabase>();
            }
            else
            {
                services.AddScoped<IDatabaseConnection, PostgreSqlDatabase>();
            }
        }

        public static void RegisterHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
              .AddCheck<SqlServerCustomHealthCheck>("Database");
        }

        public static void RegisterMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<AtroxVectraRuntimeApiConsumer>()
                    .Endpoint(e => e.Name = configuration["RabbitMqQueueName:Atrox.Vectra.Runtime.Api"]);

                x.UsingRabbitMq((context, rabbitMqConfiguration) =>
                {
                    rabbitMqConfiguration.ConfigureJsonSerializerOptions(_ => new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    rabbitMqConfiguration.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                    rabbitMqConfiguration.Host(new Uri(configuration["RabbitMq:Hostname"]), h =>
                    {
                        h.Username(configuration["RabbitMq:UserName"]);
                        h.Password(configuration["RabbitMq:Password"]);
                    });

                    rabbitMqConfiguration.ConfigureEndpoints(context);
                });
            });
        }

        public static void RegisterSwagger(IServiceCollection services, IConfiguration configuration)
        {
            var xmlFile = "Atrox.Vectra.Runtime.Api.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            CreateXml.Xml(xmlPath);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Atrox.Vectra.Runtime.Api",
                    Description = "API Rest - Atrox.Vectra.Runtime.Api",
                    TermsOfService = new Uri(configuration["ExternalLinks:TermsOfService"]),
                    Contact = new OpenApiContact
                    {
                        Name = "Integration support contact",
                        Email = "danbet.tech@gmail.com",
                        Url = new Uri(configuration["ExternalLinks:IntegrationSupport"]),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under license",
                        Url = new Uri(configuration["ExternalLinks:License"]),
                    }
                });
                c.IncludeXmlComments(xmlPath);

            });
        }
    }
}




