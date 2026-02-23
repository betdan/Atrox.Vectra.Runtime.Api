namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Atrox.Vectra.Runtime.Api.Business.Models.DataAccess;
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Connections;

    public class StoredProcedureParameterService(IMemoryCache _memoryCache) : IStoredProcedureParameterService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Seguridad",
            "CA2100: Revisar las consultas SQL por posibles vulnerabilidades de seguridad",
            Justification = "Este solo ejecuta una consulta ya estructurada, no recibe c�digo SQL.")]
        public async Task<(List<Parameter>, SqlException)> GetStoredProcedureParametersAsync(SqlConnection connection, string databaseName, string procedureName)
        {
            var cacheName = $"{databaseName}..{procedureName}";
            SqlException sqlEx = null;

            using (var changeDbCommand = new SqlCommand($@"USE {databaseName}", connection))
            {
                changeDbCommand.ExecuteNonQuery();
            }

            if (_memoryCache.TryGetValue(cacheName, out List<Parameter> cachedParameters))
            {
                return (cachedParameters, sqlEx);
            }

            var sql = @"
                SELECT 
                    p.name AS ParameterName, 
                    t.name AS DataType, 
                    p.max_length AS Length, 
                    p.is_output AS IsOutput,
                    '' AS DefaultValue
                FROM sys.parameters p
                JOIN sys.types t ON p.user_type_id = t.user_type_id
                WHERE p.object_id = OBJECT_ID(@ProcedureName)
                ORDER BY p.parameter_id";

            var parameters = new List<Parameter>();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@ProcedureName", procedureName));
            command.CommandTimeout = 30;

            try
            {
                await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    parameters.Add(new Parameter
                    {
                        ParameterName = reader["ParameterName"].ToString(),
                        DataTypes = new List<string> { reader["DataType"].ToString() },
                        Length = Convert.ToInt32(reader["Length"]),
                        IsOutput = Convert.ToBoolean(reader["IsOutput"]),
                        DefaultValue = reader["DefaultValue"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                sqlEx = ex;
            }
            finally
            {
                if (parameters.Count > 0)
                {
                    _memoryCache.Set(cacheName, parameters, TimeSpan.FromMinutes(10));
                }
            }

            return (parameters, sqlEx);
        }
    }
}
