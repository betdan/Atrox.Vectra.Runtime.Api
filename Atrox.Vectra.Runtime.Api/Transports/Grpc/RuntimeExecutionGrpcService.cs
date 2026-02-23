namespace Atrox.Vectra.Runtime.Api.Transports.Grpc
{
    using global::Grpc.Core;
    using Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using System.Globalization;

    public class RuntimeExecutionGrpcService(IExecutionService executionService, ILogger<RuntimeExecutionGrpcService> logger) : RuntimeExecution.RuntimeExecutionBase
    {
        private readonly IExecutionService _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
        private readonly ILogger<RuntimeExecutionGrpcService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public override async Task<ExecuteResponse> Execute(ExecuteRequest request, ServerCallContext context)
        {
            var internalRequest = new ApplicationDto
            {
                Data = new AtroxVectraRuntimeApiRequest
                {
                    databaseName = request.DatabaseName,
                    procedureName = request.ProcedureName,
                    inputParameters = request.InputParameters
                        .Select(parameter => new InputParamsRequest
                        {
                            paramName = parameter.Name,
                            value = parameter.Value
                        })
                        .ToList()
                }
            };

            _logger.LogInformation("gRPC request received for procedure: {procedureName}", request.ProcedureName);
            var executionResult = await _executionService.ExecuteServiceAsync(internalRequest);

            var response = new ExecuteResponse
            {
                Success = executionResult?.Error == null || executionResult.Error.All(error => error.Code == "0"),
                Error = string.Join(" | ", executionResult?.Error?.Where(error => error.Code != "0").Select(error => error.Message) ?? Enumerable.Empty<string>())
            };

            if (executionResult?.Data?.prints != null)
            {
                response.Prints.AddRange(executionResult.Data.prints);
            }

            if (executionResult?.Data?.outputParameters != null)
            {
                foreach (var outputParameter in executionResult.Data.outputParameters)
                {
                    response.OutputParameters.Add(new OutputParameter
                    {
                        Key = outputParameter.Key,
                        Value = outputParameter.Value?.Value ?? string.Empty,
                        Type = outputParameter.Value?.Type ?? string.Empty
                    });
                }
            }

            if (executionResult?.Data?.resultSets != null)
            {
                foreach (var resultSet in executionResult.Data.resultSets)
                {
                    var grpcResultSet = new ResultSet();
                    var rows = resultSet?.ResultSet ?? new List<Dictionary<string, object>>();

                    foreach (var row in rows)
                    {
                        var grpcRow = new Row();
                        foreach (var cell in row)
                        {
                            grpcRow.Cells.Add(new Cell
                            {
                                Key = cell.Key,
                                Value = Convert.ToString(cell.Value, CultureInfo.InvariantCulture) ?? string.Empty
                            });
                        }
                        grpcResultSet.Rows.Add(grpcRow);
                    }

                    response.ResultSets.Add(grpcResultSet);
                }
            }

            return response;
        }
    }
}
