using Dapper;
using System;
using System.Data;

namespace Accounts.Infrastructure.Persistence.Handlers;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid guid)
    {
        parameter.Value = guid.ToString();
    }

    public override Guid Parse(object value)
    {
        if (value != null && value?.ToString().Length > 34)
            return new Guid(value.ToString());
        else return Guid.Empty;
    }
}