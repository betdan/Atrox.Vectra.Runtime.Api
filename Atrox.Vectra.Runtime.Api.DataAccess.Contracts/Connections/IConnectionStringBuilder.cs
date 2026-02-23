namespace Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections
{
    public interface IConnectionStringBuilder
    {
        string BuildSqlServerConnectionString();
        string BuildPostgreSqlConnectionString();
        string BuildConnectionString(string engine);
    }
}


