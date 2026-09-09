using System.Data;
using Dapper;

namespace InventoryPOS.Data;

public class EnumStringTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString().ToLowerInvariant();
    }

    public override T Parse(object value)
    {
        var raw = value.ToString() ?? throw new InvalidCastException("Cannot parse null enum value.");
        return (T)Enum.Parse(typeof(T), raw, ignoreCase: true);
    }
}
