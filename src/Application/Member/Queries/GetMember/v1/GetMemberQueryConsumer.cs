using Accounts.Application.Common.Exceptions;
using Accounts.Application.Common.Interfaces;
using Mapster;
using MassTransit;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Queries.GetMember.v1;

using Domain.Entities;

public class GetMemberQueryConsumer : IConsumer<GetMemberQuery>
{
    private readonly IAccountsPersistanceContext<int, Domain.Entities.Member> accountsContext;

    public GetMemberQueryConsumer(IAccountsPersistanceContext<int, Domain.Entities.Member> _accountsContext)
    {
        accountsContext = _accountsContext;
    }

    public async Task Consume(ConsumeContext<GetMemberQuery> context)
    {
        Member member = await accountsContext.GetMember(new MemberContext()
        {
            CognitoId = context.Message.CognitoId,
            Email = context.Message.Email,
            IsGetByCognitoId = !string.IsNullOrEmpty(context.Message.CognitoId?.ToString())
        });

        if (member == null)
        {
            throw new NotFoundException($"Member not found with id : {context.Message.Email}{context.Message.CognitoId}");
        }

        var memberInfo = member.Adapt<MemberInfo>();

        await context.RespondAsync(memberInfo);
    }
}