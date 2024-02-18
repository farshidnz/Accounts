using Accounts.Application.Common.Interfaces;
using MassTransit;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Commands.CreateMember.v1;

using Accounts.Application.Common.Exceptions;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using Domain.Entities;
using Mapster;
using Newtonsoft.Json;

public class CreateMemberCommandConsumer : IConsumer<CreateMemberCommand>
{
    private readonly IAccountsPersistanceContext<int, Member> _accountsContext;

    private readonly IApplicationPersistenceContext<int, Member> _applicationContext;

    private readonly IEncryptionService _encryptionService;
    private const string EncryptAlgorithm = "SYMMETRIC_DEFAULT";

    public CreateMemberCommandConsumer(IAccountsPersistanceContext<int, Member> accountsContext
        , IApplicationPersistenceContext<int, Member> applicationContext
        , IEncryptionService encryptionService)
    {
        _accountsContext = accountsContext;
        _applicationContext = applicationContext;
        _encryptionService = encryptionService;
    }

    public async Task Consume(ConsumeContext<CreateMemberCommand> context)
    {
        Member member = await _accountsContext.GetMember(context.Message.Adapt<MemberContext>());

        if (member != null)
        {
            throw new ValidationException($"Member already exists: {context.Message.MemberId}{context.Message.CognitoId}");
        }
        member = context.Message.Adapt<Member>();
        MemberJoined memberJoinedEvent = context.Message.Adapt<MemberJoined>();
        memberJoinedEvent.PIIDataObject = context.Message.Adapt<PIIData>();

        memberJoinedEvent.PIIData = JsonConvert.SerializeObject(memberJoinedEvent.PIIDataObject,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

        if (memberJoinedEvent.PIIDataObject.HasPIIData)
        {
            memberJoinedEvent.PIIData = await _encryptionService.Encrypt(memberJoinedEvent.PIIData, EncryptionDataType.Base64, EncryptAlgorithm);
            memberJoinedEvent.PIIIEncryptAlgorithm = EncryptAlgorithm;
        }

        member.RaiseEvent(memberJoinedEvent);

        await _applicationContext.Save(member);

        await context.RespondAsync((member).Adapt<MemberInfo>());
    }
}