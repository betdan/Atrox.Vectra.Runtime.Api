namespace Atrox.Vectra.Runtime.Api.Tests;

using Atrox.Vectra.Runtime.Api.DataAccess.Connections;
using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
using Atrox.Vectra.Runtime.Api.Installers;
using CrossCutting.Crypto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ServiceRegistrationTests
{
    [Fact]
    public void RegisterServices_WithSqlServerEngine_RegistersSqlServerDatabase()
    {
        var services = CreateServiceCollection("SqlServer");

        var provider = services.BuildServiceProvider();
        var databaseConnection = provider.GetRequiredService<IDatabaseConnection>();

        Assert.IsType<SqlServerDatabase>(databaseConnection);
    }

    [Fact]
    public void RegisterServices_WithPostgreSqlEngine_RegistersPostgreSqlDatabase()
    {
        var services = CreateServiceCollection("PostgreSql");

        var provider = services.BuildServiceProvider();
        var databaseConnection = provider.GetRequiredService<IDatabaseConnection>();

        Assert.IsType<PostgreSqlDatabase>(databaseConnection);
    }

    [Fact]
    public void RegisterServices_WithInvalidEngine_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration("MongoDb");

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddSingleton<ICrypto, TestCrypto>();

        Assert.Throws<InvalidOperationException>(() => services.RegisterServices(configuration));
    }

    private static ServiceCollection CreateServiceCollection(string engine)
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(engine);

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddSingleton<ICrypto, TestCrypto>();
        services.RegisterServices(configuration);

        return services;
    }

    private static IConfiguration BuildConfiguration(string engine)
    {
        var config = new Dictionary<string, string>
        {
            ["Database:Engine"] = engine,
            ["ConnectionStrings:SqlServer:ConnectionStringFormat"] = "Server={server},{port};Database={database};User Id={userId};Password={decryptedPassword};",
            ["ConnectionStrings:SqlServer:Server"] = "localhost",
            ["ConnectionStrings:SqlServer:Port"] = "1433",
            ["ConnectionStrings:SqlServer:UserId"] = "sa",
            ["ConnectionStrings:SqlServer:Password"] = "",
            ["ConnectionStrings:SqlServer:Database"] = "master",
            ["ConnectionStrings:PostgreSql:ConnectionStringFormat"] = "Host={server};Port={port};Database={database};Username={userId};Password={decryptedPassword};",
            ["ConnectionStrings:PostgreSql:Server"] = "localhost",
            ["ConnectionStrings:PostgreSql:Port"] = "5432",
            ["ConnectionStrings:PostgreSql:UserId"] = "postgres",
            ["ConnectionStrings:PostgreSql:Password"] = "",
            ["ConnectionStrings:PostgreSql:Database"] = "postgres"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
    }

    private sealed class TestCrypto : ICrypto
    {
        public string Decrypt(string encryptedText) => encryptedText;
        public string Encrypt(string text) => text;
    }
}
