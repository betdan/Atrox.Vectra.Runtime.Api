namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;
    using Npgsql;
    using System.Data;
    using System.Data.Common;

    public class PostgreSqlDatabase(IConnectionStringBuilder connectionStringBuilder) : IDisposable, IDatabaseConnection
    {
        private readonly string _connectionString = connectionStringBuilder.BuildPostgreSqlConnectionString() ?? throw new InvalidOperationException("Could not build the PostgreSQL connection string.");
        private NpgsqlConnection _connection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Security",
            "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "This command executes only stored procedures with controlled parameters.")]
        public async Task<(List<DataTable> resultSets, Dictionary<string, object> outputParameters, List<string> printMessages, int returnValue, Dictionary<int, string> rErrors)> ExecuteStoredProcedureAsync(
            string databaseName,
            string procedureName,
            Dictionary<string, object> inputParams = null,
            List<string> outputParams = null)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentException("The stored procedure name is required.", nameof(procedureName));
            }

            var resultSets = new List<DataTable>();
            var outputValues = new Dictionary<string, object>();
            var printMessages = new List<string>();
            var rErrors = new Dictionary<int, string>();
            var returnValue = 0;

            await using var connection = (NpgsqlConnection)await GetOpenConnectionAsync().ConfigureAwait(false);
            connection.Notice += (_, notice) => printMessages.Add(notice.Notice.MessageText);

            if (!string.IsNullOrWhiteSpace(databaseName) &&
                !string.Equals(connection.Database, databaseName, StringComparison.OrdinalIgnoreCase))
            {
                printMessages.Add($"Ignoring requested database '{databaseName}'. Active PostgreSQL database is '{connection.Database}'.");
            }

            await using var command = new NpgsqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (inputParams != null)
            {
                foreach (var inputParam in inputParams)
                {
                    command.Parameters.AddWithValue(inputParam.Key, inputParam.Value ?? DBNull.Value);
                }
            }

            if (outputParams != null)
            {
                foreach (var outputParam in outputParams.Where(name => !string.IsNullOrWhiteSpace(name)))
                {
                    command.Parameters.Add(new NpgsqlParameter(outputParam, DbType.String)
                    {
                        Direction = ParameterDirection.Output
                    });
                }
            }

            try
            {
                await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                do
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    resultSets.Add(dataTable);
                } while (await reader.NextResultAsync().ConfigureAwait(false));
            }
            catch (PostgresException ex)
            {
                rErrors[1] = $"({ex.SqlState}) {ex.MessageText}";
                returnValue = -1;
                return (resultSets, outputValues, printMessages, returnValue, rErrors);
            }
            catch (Exception ex)
            {
                rErrors[1] = ex.Message;
                returnValue = -1;
                return (resultSets, outputValues, printMessages, returnValue, rErrors);
            }

            if (outputParams != null)
            {
                foreach (var outputParam in outputParams.Where(name => !string.IsNullOrWhiteSpace(name)))
                {
                    outputValues[outputParam] = new
                    {
                        Value = command.Parameters[outputParam]?.Value ?? DBNull.Value,
                        Type = command.Parameters[outputParam]?.DbType.ToString() ?? string.Empty
                    };
                }
            }

            if (!rErrors.Any())
            {
                rErrors[0] = "Successful";
                returnValue = 0;
            }

            return (resultSets, outputValues, printMessages, returnValue, rErrors);
        }

        public async Task<DbConnection> GetOpenConnectionAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new NpgsqlConnection(_connectionString);
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
