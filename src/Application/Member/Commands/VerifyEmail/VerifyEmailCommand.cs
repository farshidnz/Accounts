using Accounts.Application.Common.Mappings;
using Mapster;

namespace Accounts.Application.Member.Commands.VerifyEmail;

using Domain.Entities;

public class VerifyEmailCommand : IMapTo<MemberContext> , IMapTo<Member>
{
    public string Code { get; set; }

    public void Mapping(TypeAdapterConfig config)
    {
        config.ForType<VerifyEmailCommand, MemberContext>()
            .Map(dest => dest.IsGetByMemberId, _ => true);

        config.ForType<(VerifyEmailCommand, Member), Member>()
             .Map(dest => dest, src => src.Item2)
             .Map(dest => dest.IsValidated, _ => true)
             .Map(dest => dest.DomainName, _ => "Member")
             .Map(dest => dest.HasDomainEvents, _ => true);
    }
}