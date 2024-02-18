using Accounts.Application.Common.Interfaces;
using Mapster;
using MassTransit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Application.Member.Commands.VerifyEmail;

using Accounts.Application.Common.Exceptions;
using Accounts.Application.Member.Model;
using Accounts.Domain.Events;
using Domain.Entities;

public class VerifyEmailCommandConsumer : IConsumer<VerifyEmailCommand>
{
    private readonly IAccountsPersistanceContext<int, Member> accountsContext;
    private readonly IApplicationPersistenceContext<int, Member> _applicationContext;

    public VerifyEmailCommandConsumer(IAccountsPersistanceContext<int, Member> accountsContext, IApplicationPersistenceContext<int, Member> applicationContext)
    {
        this.accountsContext = accountsContext;
        _applicationContext = applicationContext;
    }

    public async Task Consume(ConsumeContext<VerifyEmailCommand> context)
    {
        GetMemberAndEmailFromCode(context.Message.Code, out var hashedEmail, out var memberId);

        if (string.IsNullOrEmpty(hashedEmail) || string.IsNullOrEmpty(memberId))
            throw new ValidationException("Code in the wrong format");

        Member member = await accountsContext.GetMember(new MemberContext()
        {
            MemberId = Convert.ToInt32(memberId),
            IsGetByMemberId = true
        });
        if (member == null)
        {
            throw new NotFoundException($"Member not found with Memberid : {memberId}");
        }

        MemberDetailChanged memberDetailChangedEvent = new MemberDetailChanged()
        {
            IsValidated = true,
            CognitoId = (Guid)member.CognitoId
        };
        Member memberToAdapt = (context.Message, member).Adapt<Member>();
        memberToAdapt.MessageType = typeof(MemberDetailChanged).ToString();
        memberToAdapt.RaiseEvent(memberDetailChangedEvent);

        await _applicationContext.Save(memberToAdapt);

        await context.RespondAsync(new VerifyEmailDto() { Email = member.Email });
    }

    /// <summary>
    /// gets memberid and email from code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="hashedEmail"></param>
    /// <param name="memberId"></param>
    private static void GetMemberAndEmailFromCode(string code, out string hashedEmail, out string memberId)
    {
        string[] decodeDataArr;

        decodeDataArr = code.Split("$");

        memberId = decodeDataArr[0];
        hashedEmail = string.Join("$", decodeDataArr.Skip(1).ToArray());
    }
}