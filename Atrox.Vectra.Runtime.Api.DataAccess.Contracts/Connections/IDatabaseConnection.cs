namespace Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections
{
    using System.Data;
    using System.Data.Common;

    public interface IDatabaseConnection : IDisposable
    {
        Task<(List<DataTable> resultSets, Dictionary<string, object> outputParameters, List<string> printMessages, int returnValue, Dictionary<int, string> rErrors)> ExecuteStoredProcedureAsync(string databaseName, string procedureName, Dictionary<string, object> inputParams = null, List<string> outputParams = null);
        Task<DbConnection> GetOpenConnectionAsync();
    }
}
