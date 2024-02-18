using Accounts.Application.Common.Mappings;
using Accounts.Domain.Events;
using Mapster;

namespace Accounts.Application.Member.Commands.CreateMember.v1;

using Domain.Entities;
using Domain.Enums;

public class CreateMemberCommand : MemberCommand, IMapTo<MemberContext>, IMapTo<Member>
{
    public long MemberId { get; set; }

    public int ClientId { get; set; }

    public string CognitoPoolId { get; set; }
    public string MemberNewId { get; set; }

    public void Mapping(TypeAdapterConfig config)
    {
        config.ForType<CreateMemberCommand, MemberContext>()
            .Map(dest => dest.IsGetByCognitoId, _ => false);

        config.ForType<CreateMemberCommand, Member>()
           .Map(dest => dest.IsNewMember, _ => true)
           .Map(dest => dest.MessageType, _ => typeof(MemberJoined).ToString())
           .Map(dest => dest.KycStatusId, _ => (int)KycStatus.No)
           .Map(dest => dest.CognitoPoolId, _ => (int)CognitoPool.MemberUserPool);
    }
}