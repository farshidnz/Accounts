using Accounts.Application.Common.Interfaces;
using MassTransit;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Commands.UpdateMember.v1;

using Accounts.Application.Common.Exceptions;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using Domain.Entities;
using Mapster;
using Newtonsoft.Json;
using System;

public class UpdateMemberCommandConsumer : IConsumer<UpdateMemberCommand>
{
    public const int MaxCommsPromptShownCount = 2;

    private readonly IAccountsPersistanceContext<int, Member> _accountsContext;

    private readonly IApplicationPersistenceContext<int, Member> _applicationContext;

    private readonly IEncryptionService _encryptionService;
    private const string EncryptAlgorithm = "SYMMETRIC_DEFAULT";
    public UpdateMemberCommandConsumer(IAccountsPersistanceContext<int, Member> accountsContext
        , IApplicationPersistenceContext<int, Member> applicationContext
        , IEncryptionService encryptionService)
    {
        _accountsContext = accountsContext;
        _applicationContext = applicationContext;
        _encryptionService = encryptionService;
    }

    public async Task Consume(ConsumeContext<UpdateMemberCommand> context)
    {
        Member member = await _accountsContext.GetMember(context.Message.Adapt<MemberContext>());

        if (member == null)
        {
            throw new NotFoundException($"Member not found with id : {context.Message.Email}{context.Message.CognitoId}");
        }
        SetCommsPromptShownCount(context.Message, member);
        var memberToAdapt = (context.Message, member).Adapt<Member>();

        MemberDetailChanged memberDetailChangedEvent = memberToAdapt.Adapt<MemberDetailChanged>();
        memberDetailChangedEvent.PIIDataObject = memberToAdapt.Adapt<PIIData>();

        memberDetailChangedEvent.PIIData = JsonConvert.SerializeObject(memberDetailChangedEvent.PIIDataObject,                            
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

        if(memberDetailChangedEvent.PIIDataObject.HasPIIData)
        {
            memberDetailChangedEvent.PIIData = _encryptionService.Encrypt(memberDetailChangedEvent.PIIData, EncryptionDataType.Base64, EncryptAlgorithm).Result.ToString();
            memberDetailChangedEvent.PIIIEncryptAlgorithm = EncryptAlgorithm;
        }
        memberToAdapt.MessageType = typeof(MemberDetailChanged).ToString();
        memberToAdapt.RaiseEvent(memberDetailChangedEvent);
        await _applicationContext.Save(memberToAdapt);

        await context.RespondAsync((member, context.Message).Adapt<MemberInfo>());
    }

    private void SetCommsPromptShownCount(UpdateMemberCommand command, Member member)
    {
        if (Enum.TryParse(typeof(CommsPromptDismissalAction), command.CommsPromptAction, out var parsedAction))
        {
            var action = (parsedAction as CommsPromptDismissalAction?).Value;

            switch (action)
            {
                case CommsPromptDismissalAction.Close:
                    command.CommsPromptAction = (member.CommsPromptShownCount += 1).ToString();
                    break;

                case CommsPromptDismissalAction.Review:
                    command.CommsPromptAction = MaxCommsPromptShownCount.ToString();
                    break;
            }
        }
    }
}