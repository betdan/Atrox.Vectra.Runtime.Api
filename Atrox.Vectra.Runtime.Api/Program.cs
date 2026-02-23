using CrossCutting.Config;
using CrossCutting.Crypto;
using CrossCutting.Middlewares;
using Atrox.Vectra.Runtime.Api.Installers;
using Atrox.Vectra.Runtime.Api.Transports.Grpc;
using Atrox.Vectra.Runtime.Api.Transports.WebSocket;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;
var grpcOptions = configuration.GetSection("Transports:Grpc").Get<GrpcTransportOptions>() ?? new GrpcTransportOptions();
var webSocketOptions = configuration.GetSection("Transports:WebSocket").Get<WebSocketTransportOptions>() ?? new WebSocketTransportOptions();

var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

builder.Host.UseSerilog(logger);
builder.Services.Configure<WebSocketTransportOptions>(configuration.GetSection("Transports:WebSocket"));

if (grpcOptions.Enabled)
{
    builder.Services.AddGrpc();
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(grpcOptions.Port, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });
}

builder.Services.AddSingleton<ICrypto, Crypto>();
builder.Services.AddControllers();

Log.Information("Application Starting Up.");
builder.Services.Configure<AppConfiguration>(configuration.GetSection("ServicesConfig"));
builder.Services.RegisterServices(configuration);
builder.Services.RegisterMassTransit(configuration);
builder.Services.RegisterHealthChecks(configuration);

ServiceExtensions.RegisterSwagger(builder.Services, configuration);

var app = builder.Build();
app.UseHttpsRedirection();
HealthCheckConfig.AddRegistration(app);
SwaggerConfig.AddRegistration(app);
app.UseMetricServer();
app.UseHttpMetrics();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestHeaderValidationMiddleware>();

if (webSocketOptions.Enabled)
{
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(webSocketOptions.KeepAliveSeconds)
    });
    app.UseMiddleware<RuntimeWebSocketMiddleware>();
}

app.UseRouting();

app.MapHealthChecks("/health");
app.MapControllers();

if (grpcOptions.Enabled)
{
    app.MapGrpcService<RuntimeExecutionGrpcService>();
}

await app.RunAsync();




