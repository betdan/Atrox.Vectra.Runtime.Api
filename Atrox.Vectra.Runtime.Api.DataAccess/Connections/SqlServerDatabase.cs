namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
    using Microsoft.Data.SqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SqlServerDatabase(IConnectionStringBuilder _csb, IStoredProcedureParameterService _sps) : IDisposable, IDatabaseConnection
    {
        private readonly string _connectionString = _csb.BuildSqlServerConnectionString() ?? throw new InvalidOperationException("Could not build the SQL Server connection string.");
        private SqlConnection _connection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Security",
            "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "This command executes only stored procedures with controlled parameters.")]
        public async Task<(List<DataTable> resultSets, Dictionary<string, object> outputParameters, List<string> printMessages, int returnValue, Dictionary<int, string> rErrors)> ExecuteStoredProcedureAsync(string databaseName, string procedureName, Dictionary<string, object> inputParams = null, List<string> outputParams = null)
        {
            var resultSets = new List<DataTable>();
            var outputValues = new Dictionary<string, object>();
            var printMessages = new List<string>();
            var rErrors = new Dictionary<int, string>();
            int returnValue = 0;

            await using var connection = (SqlConnection)await GetOpenConnectionAsync().ConfigureAwait(false);

            connection.InfoMessage += (sender, e) =>
            {
                foreach (SqlError error in e.Errors)
                {
                    var message = new StringBuilder(error.Message);

                    if (!string.IsNullOrEmpty(error.Procedure))
                        message.Append($" (Procedure: {error.Procedure})");

                    if (error.LineNumber > 0)
                        message.Append($" (Line: {error.LineNumber})");

                    if (!message.ToString().Contains("Changed database"))
                    {
                        printMessages.Add(message.ToString());
                    }
                }
            };

            var (parameters, sqlEx) = await _sps.GetStoredProcedureParametersAsync(connection, databaseName, procedureName).ConfigureAwait(false);

            if (sqlEx != null)
            {
                foreach (SqlError sqlError in sqlEx.Errors)
                {
                    if (sqlError.Number > 0)
                    {
                        var messageError = new StringBuilder()
                            .Append($"({sqlError.Number}) {sqlError.Message}");

                        if (!string.IsNullOrEmpty(sqlError.Procedure))
                            messageError.Append($" (Procedure: {sqlError.Procedure})");

                        if (sqlError.LineNumber > 0)
                            messageError.Append($" (Line: {sqlError.LineNumber})");

                        rErrors[sqlError.Number] = messageError.ToString();
                    }
                }
                return (resultSets, outputValues, printMessages, returnValue, rErrors);
            }

            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            });

            if (parameters != null)
            {
                foreach (var dbParamOP in parameters)
                {
                    if (dbParamOP.IsOutput)
                    {
                        // Map output parameter SQL data type.
                        var sqlParameter = new SqlParameter(dbParamOP.ParameterName, SqlDbTypeMapper.Map(dbParamOP.DataTypes.FirstOrDefault()))
                        {
                            Direction = ParameterDirection.Output,
                            Size = dbParamOP.Length > 0 ? dbParamOP.Length : 0
                        };

                        // Assign default value if present.
                        if (!string.IsNullOrEmpty(dbParamOP.DefaultValue))
                        {
                            sqlParameter.Value = dbParamOP.DefaultValue;
                        }

                        // Add parameter to command.
                        command.Parameters.Add(sqlParameter);
                    }
                    else
                    {
                        // Handle input parameters.
                        if (inputParams != null)
                        {
                            var param = inputParams.FirstOrDefault(p => p.Key.Equals(dbParamOP.ParameterName, StringComparison.OrdinalIgnoreCase));
                            if (param.Key != null)
                            {
                                var mappedDbType = SqlDbTypeMapper.Map(dbParamOP.DataTypes.FirstOrDefault());
                                var convertedValue = SqlDbTypeConverter.ConvertToSqlDbType(param.Value, mappedDbType);
                                command.Parameters.AddWithValue(dbParamOP.ParameterName, convertedValue ?? DBNull.Value);
                            }
                        }
                    }
                }
            }

            try
            {
                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                do
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    resultSets.Add(dataTable);
                } while (!reader.IsClosed && reader.HasRows);
            }
            catch (SqlException ex)
            {
                foreach (SqlError error in ex.Errors)
                {
                    rErrors[error.Number] = error.Message;
                }
                return (resultSets, outputValues, printMessages, returnValue, rErrors);
            }

            if (parameters != null)
            {
                foreach (var param in parameters.Where(p => p.IsOutput))
                {
                    var paramValue = command.Parameters[param.ParameterName].Value;
                    var paramType = command.Parameters[param.ParameterName].SqlDbType.ToString();

                    outputValues[param.ParameterName] = new { Value = paramValue, Type = paramType };
                }
            }

            returnValue = (int)command.Parameters["@ReturnValue"].Value;

            if (!rErrors.Any())
            {
                rErrors[0] = "Successful";
            }

            return (resultSets, outputValues, printMessages, returnValue, rErrors);
        }


        public async Task<System.Data.Common.DbConnection> GetOpenConnectionAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync().ConfigureAwait(false);
            }
            return _connection;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }
}
