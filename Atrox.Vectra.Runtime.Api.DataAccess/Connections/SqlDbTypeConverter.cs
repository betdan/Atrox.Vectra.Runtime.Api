namespace Atrox.Vectra.Runtime.Api.DataAccess.Connections
{
    using System;
    using System.Data;

    public static class SqlDbTypeConverter
    {
        public static object ConvertToSqlDbType(object value, SqlDbType sqlDbType)
        {
            if (value == null || value == DBNull.Value || (value is string strValue && string.Equals(strValue, "null", StringComparison.OrdinalIgnoreCase)))
            {
                return DBNull.Value;
            }

            try
            {
                return sqlDbType switch
                {
                    SqlDbType.Int => Convert.ToInt32(value),
                    SqlDbType.SmallInt => Convert.ToInt16(value),
                    SqlDbType.TinyInt => Convert.ToByte(value),
                    SqlDbType.BigInt => Convert.ToInt64(value),
                    SqlDbType.Float => Convert.ToSingle(value),
                    SqlDbType.Real => Convert.ToDouble(value),
                    SqlDbType.Decimal => Convert.ToDecimal(value),
                    SqlDbType.Money => Convert.ToDecimal(value),
                    SqlDbType.SmallMoney => Convert.ToDecimal(value),
                    SqlDbType.Bit => Convert.ToBoolean(value),
                    SqlDbType.Char or SqlDbType.NChar or SqlDbType.VarChar or SqlDbType.NVarChar => value.ToString(),
                    SqlDbType.DateTime or SqlDbType.SmallDateTime or SqlDbType.Date => Convert.ToDateTime(value),
                    SqlDbType.Time => Convert.ToDateTime(value).TimeOfDay,
                    SqlDbType.Binary or SqlDbType.VarBinary => (byte[])value,
                    SqlDbType.Image => (byte[])value,
                    SqlDbType.Text or SqlDbType.NText => value.ToString(),
                    _ => value
                };
            }
            catch
            {
                throw new InvalidCastException($"Could not convert value '{value}' to SQL type {sqlDbType}");
            }
        }
    }
}
