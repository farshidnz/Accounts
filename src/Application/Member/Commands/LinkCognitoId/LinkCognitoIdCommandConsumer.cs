using Accounts.Application.Common.Interfaces;
using Mapster;
using MassTransit;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Commands.LinkCognitoId;
using Accounts.Application.Common.Exceptions;
using Accounts.Domain.Events;
using Domain.Entities;

public class LinkCognitoIdCommandConsumer : IConsumer<LinkCognitoIdCommand>
{
    private readonly IAccountsPersistanceContext<int, MemberCognito> accountsContext;

    public LinkCognitoIdCommandConsumer(IAccountsPersistanceContext<int, MemberCognito> accountsContext)
    {
        this.accountsContext = accountsContext;
    }

    public async Task Consume(ConsumeContext<LinkCognitoIdCommand> context)
    {
        MemberCognito cognitoPool = await accountsContext.GetCognitoPoolId(context.Message.CognitoPoolId);
        if (cognitoPool == null)
        {
            throw new NotFoundException($"CognitoPoolId - {context.Message.CognitoPoolId} not found.");
        }
        MemberCognito mc = (context.Message, cognitoPool).Adapt<MemberCognito>();
        CognitoLinked cognitoLinkedEvent = context.Message.Adapt<CognitoLinked>();
        mc.MessageType = typeof(CognitoLinked).ToString();
        mc.RaiseEvent(cognitoLinkedEvent);
        await accountsContext.AddOrUpdate(mc);
    }
}
