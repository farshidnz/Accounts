using Accounts.Application.Common.Mappings;
using Mapster;

namespace Accounts.Application.Member;

public class MemberInfo : Domain.Entities.Member, IMapFrom<Domain.Entities.Member>
{
    public void Mapping(TypeAdapterConfig config)
    {
        config.ForType<Domain.Entities.Member, MemberInfo>();
    }
}