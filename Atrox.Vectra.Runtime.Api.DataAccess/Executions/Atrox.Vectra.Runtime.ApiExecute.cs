namespace Atrox.Vectra.Runtime.Api.DataAccess.Executions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Executions;
    using System.Data;

    public class AtroxVectraRuntimeApiExecute(IDatabaseConnection bd, ILogger<AtroxVectraRuntimeApiExecute> logger, IConfiguration conf) : IAtroxVectraRuntimeApiExecute
    {
        private readonly IDatabaseConnection _bd = bd ?? throw new ArgumentNullException(nameof(bd));
        private readonly ILogger<AtroxVectraRuntimeApiExecute> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IConfiguration _conf = conf ?? throw new ArgumentNullException(nameof(conf));

        public async Task<AtroxVectraRuntimeApiResponse> ExecuteAtroxVectraRuntimeApiClientAsync(ApplicationDto req)
        {
            if (req == null || req.Data == null)
            {
                throw new ArgumentNullException(nameof(req), "The 'req' parameter or 'req.Data' is null.");
            }

            AtroxVectraRuntimeApiResponse result = new AtroxVectraRuntimeApiResponse
            {
                returns = new int(),
                prints = new List<String>(),
                outputParameters = new Dictionary<string, ParameterValue>(),
                resultSets = new List<ResultSets>()
            };

            var inputParams = new Dictionary<string, object>();

            foreach (var param in req.Data.inputParameters)
            {
                if (!string.IsNullOrEmpty(param.paramName) && param.value != null)
                {
                    inputParams[param.paramName] = param.value;
                }
            }


            try
            {
                var (resultSets, outputValues, printMessages, returnValue, errors) = await _bd.ExecuteStoredProcedureAsync(req.Data.databaseName, req.Data.procedureName, inputParams);

                result.returns = returnValue;
                result.prints = printMessages;

                var raisErrors = new List<RaisError>();

                foreach (var error in errors)
                {
                    raisErrors.Add(new RaisError
                    {
                        code = error.Key,
                        message = error.Value
                    });
                }

                result.raisError = raisErrors;

                outputValues ??= new Dictionary<string, object>();

                foreach (var outputParam in outputValues)
                {
                    var paramDetails = outputParam.Value as dynamic;

                    result.outputParameters[outputParam.Key] = new ParameterValue
                    {
                        Value = paramDetails?.Value?.ToString() ?? string.Empty,
                        Type = paramDetails?.Type ?? string.Empty
                    };
                }

                foreach (var table in resultSets ?? new List<DataTable>())
                {
                    var resultSet = new ResultSets
                    {
                        ResultSet = new List<Dictionary<string, object>>()
                    };

                    foreach (DataRow row in table.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();

                        foreach (DataColumn column in table.Columns)
                        {
                            rowDict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                        }

                        resultSet.ResultSet.Add(rowDict);
                    }

                    if (result.resultSets == null)
                    {
                        result.resultSets = new List<ResultSets>();
                    }

                    result.resultSets.Add(resultSet);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing stored procedure.");
                throw;
            }

            return result;
        }

    }
}




