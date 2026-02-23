namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using System.Data;

    public static class SqlDbTypeMapper
    {
        private static readonly Dictionary<string, SqlDbType> _typeMap = new()
        {
            ["int"] = SqlDbType.Int,
            ["tinyint"] = SqlDbType.TinyInt,
            ["smallint"] = SqlDbType.SmallInt,
            ["bigint"] = SqlDbType.BigInt,
            ["decimal"] = SqlDbType.Decimal,
            ["numeric"] = SqlDbType.Decimal,  // 'numeric' es mapeado a Decimal en SQL Server
            ["float"] = SqlDbType.Float,
            ["real"] = SqlDbType.Real,
            ["money"] = SqlDbType.Money,
            ["smallmoney"] = SqlDbType.SmallMoney,
            ["datetime"] = SqlDbType.DateTime,
            ["smalldatetime"] = SqlDbType.SmallDateTime,
            ["date"] = SqlDbType.Date,
            ["time"] = SqlDbType.Time,
            ["char"] = SqlDbType.Char,
            ["varchar"] = SqlDbType.VarChar,
            ["nchar"] = SqlDbType.NChar,
            ["nvarchar"] = SqlDbType.NVarChar,
            ["text"] = SqlDbType.Text,
            ["ntext"] = SqlDbType.NText,
            ["binary"] = SqlDbType.Binary,
            ["varbinary"] = SqlDbType.VarBinary,
            ["image"] = SqlDbType.Image,
            ["bit"] = SqlDbType.Bit
        };

        public static SqlDbType Map(string dataType) =>
            _typeMap.TryGetValue(dataType, out var sqlType) ? sqlType : SqlDbType.VarChar;
    }
}
