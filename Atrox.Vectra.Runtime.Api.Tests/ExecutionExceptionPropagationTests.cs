namespace Atrox.Vectra.Runtime.Api.Tests;

using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
using Atrox.Vectra.Runtime.Api.DataAccess.Executions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using System.Data.Common;
using Xunit;

public class ExecutionExceptionPropagationTests
{
    [Fact]
    public async Task ExecuteAtroxVectraRuntimeApiClientAsync_WhenDatabaseFails_Throws()
    {
        var sut = new AtroxVectraRuntimeApiExecute(
            new ThrowingDatabaseConnection(),
            NullLogger<AtroxVectraRuntimeApiExecute>.Instance,
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build());

        var request = new ApplicationDto
        {
            Data = new AtroxVectraRuntimeApiRequest
            {
                databaseName = "db",
                procedureName = "sp_test",
                inputParameters = new List<InputParamsRequest>()
            }
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAtroxVectraRuntimeApiClientAsync(request));
        Assert.Equal("boom", exception.Message);
    }

    private sealed class ThrowingDatabaseConnection : IDatabaseConnection
    {
        public void Dispose()
        {
        }

        public Task<(List<DataTable> resultSets, Dictionary<string, object> outputParameters, List<string> printMessages, int returnValue, Dictionary<int, string> rErrors)> ExecuteStoredProcedureAsync(string databaseName, string procedureName, Dictionary<string, object> inputParams = null, List<string> outputParams = null)
        {
            throw new InvalidOperationException("boom");
        }

        public Task<DbConnection> GetOpenConnectionAsync()
        {
            throw new NotSupportedException();
        }
    }
}
