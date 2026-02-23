namespace Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections
{
    using Atrox.Vectra.Runtime.Api.Business.Models.DataAccess;
    using Microsoft.Data.SqlClient;

    public interface IStoredProcedureParameterService
    {
        Task<(List<Parameter>, SqlException)> GetStoredProcedureParametersAsync(SqlConnection connection, string databaseName, string procedureName);
    }
}



