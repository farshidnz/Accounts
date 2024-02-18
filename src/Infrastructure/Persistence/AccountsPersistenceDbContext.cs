using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Common;
using Accounts.Domain.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public class AccountsPersistenceDbContext<Key, Entity> : IAccountsPersistanceContext<Key, Entity>
    where Entity : IHasIdentity<Key>
{
    private readonly IRepository _repository;

    public AccountsPersistenceDbContext(IRepository repository)
    {
        _repository = repository;
    }

    public async Task AddOrUpdate(Entity context)
    {
        string dataType = typeof(Entity).Name;
        switch (dataType)
        {
            case nameof(Member):
                var member = context as Member;
                if (!member.IsNewMember)
                    await UpdateMemberDetails(member);
                else
                    await CreateMember(member);
                break;

            case nameof(MemberCognito):
                await UpdateMemberCognito(context as MemberCognito);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Gets the member by email or CognitoID.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task<Member> GetMember(MemberContext context)
    {
        string query = $"select id, memberid, cognitoid, premiumstatus, installnotifier ,status, membernewid, dateofbirth, firstname, lastname, postcode, mobile, email, ssousername, ssoprovider, accesscode, gender, receivenewsletter, smsconsent, appnotificationconsent, commspromptshowncount, clickwindowactive, popupactive, isvalidated, requiredlogin, isavailable, activeby, mailchimplistemailid, datereceivednewsletter, communicationemail, paypalemail, campaignid, twofactorauthid, istwofactorauthenticatedenable, twofactorauthenticatedactivatedby, twofactorauthenticatedmobile, twofactorauthenticatedcountrycode, riskdescription, isrisky, lastlogon, datedeletedbymember, datejoined, datereceivenewsletter, comment, iswithdrawalcapped, kycstatusid, signupverificationemailsentstatus, cognitopoolid, trustscore, originationsource, active " +
            $"from public.member ";

        MemberRequest queryArgs = new() { email = context.Email, memberId = context.MemberId, cognitoId = context.CognitoId };
        if (!string.IsNullOrEmpty(context.Email) && !context.IsGetByCognitoId)
        {
            query = string.Concat(query, "where email = @email;");
        }
        else
            if (context.MemberId > 0 && context.IsGetByMemberId)
        {
            query = string.Concat(query, $"where memberid = @memberId;");
        }
        else
                if (context.IsGetByCognitoId)
            query = string.Concat(query, $"where cognitoid = uuid(@cognitoId);");

        return (await _repository.QueryAsync<Member>(query, queryArgs)).FirstOrDefault();
    }

    /// <summary>
    /// Gets the CognitoPoolId by CognitoPoolId.
    /// </summary>
    /// <param name="cognitoPoolName">The Cognito Pool Name.</param>
    /// <returns> The CognitoPoolId </returns>
    public async Task<MemberCognito> GetCognitoPoolId(string cognitoPoolName)
    {
        string query = $"select cognitopoolid, cognitopoolname from public.cognitopool where cognitopoolname = @cognitoPoolName";

        var queryArgs = new { cognitoPoolName = cognitoPoolName };

        return (await _repository.QueryAsync<MemberCognito>(query, queryArgs)).FirstOrDefault();
    }

    /// <summary>
    /// Updates the member details
    /// </summary>
    /// <param name="member">The member.</param>
    private async Task UpdateMemberDetails(Member member)
    {
        StringBuilder query = new(@"UPDATE public.member ");
        query.Append($"SET ");
        CreateUpdateScript(query);
        query.Append($" WHERE cognitoid = uuid(@CognitoId)");

        await _repository.ExecuteAsync(query.ToString(), member);//,new { FirstName = member.FirstName, LastName = member.LastName,PostCode = member.PostCode, Email = member.Email, ReceiveNewsLetter = member.ReceiveNewsLetter , CognitoId = member.CognitoId.ToString()});
    }

    public async Task CreateMember(Member member)
    {
        StringBuilder query = new(@"INSERT INTO public.member");
        query.Append(@"( cognitoid, premiumstatus, status, membernewid, dateofbirth, firstname, lastname, postcode, mobile, email, ssousername, ssoprovider, accesscode, gender, receivenewsletter, smsconsent, appnotificationconsent, commspromptshowncount, clickwindowactive, popupactive, isvalidated, requiredlogin, isavailable, activeby, mailchimplistemailid, datereceivednewsletter, communicationemail, paypalemail, campaignid, installnotifier, twofactorauthid, istwofactorauthenticatedenable, twofactorauthenticatedactivatedby, twofactorauthenticatedmobile, twofactorauthenticatedcountrycode, riskdescription, isrisky, lastlogon, datedeletedbymember, datejoined, datereceivenewsletter, comment, iswithdrawalcapped, kycstatusid, signupverificationemailsentstatus, cognitopoolid, trustscore, originationsource, active)
	VALUES ( @CognitoId, @PremiumStatus, @Status, @MemberNewId, @DateOfBirth, @FirstName, @LastName, @PostCode, @Mobile, @Email, @SSOUserName, @SSOProvider, @AccessCode, @Gender, @ReceiveNewsLetter, @SmsConsent, @AppNotificationConsent, @CommsPromptShownCount, @ClickWindowActive, @PopupActive, @IsValidated, @RequiredLogin, @IsAvailable, @ActiveBy, @MailChimpListEmailId, @DateReceivedNewsLetter, @CommunicationEmail, @PaypalEmail, @CampaignId, @InstallNotifier, @TwoFactorAuthId, @IsTwoFactorAuthenticatedEnable, @TwoFactorAuthenticatedActivatedBy, @TwoFactorAuthenticatedMobile,@TwoFactorAuthenticatedCountryCode, @RiskDescription, @IsRisky, @LastLogon, @DateDeletedByMember, @DateJoined, @DateReceiveNewsLetter, @Comment, @IsWithdrawalCapped, @KycStatusId,@SignupVerificationEmailSentStatus, @CognitoPoolId, @TrustScore, @OriginationSource, @Active);");

        await _repository.ExecuteAsync(query.ToString(), member);
    }

    /// <summary>
    /// Updates the cognito of member
    /// </summary>
    /// <param name="member">The member cognito.</param>
    private async Task UpdateMemberCognito(MemberCognito member)
    {
        StringBuilder query = new(@"UPDATE public.member ");
        query.Append($"SET cognitoid = @CognitoId, cognitopoolid = @CognitoPoolId");
        query.Append($" WHERE email = @Email");
        await _repository.ExecuteAsync(query.ToString(), member);
    }

    /// <summary>
    /// Creates the update script based on the object
    /// , the property has to be in the Member context and has to have a value.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>string with the query</returns>
    private static StringBuilder CreateUpdateScript(StringBuilder query)
    {
        query.Append(@"firstname = @FirstName, lastname = @LastName, postcode = @PostCode, email= @Email,receivenewsletter = @ReceiveNewsLetter,")
            .Append(@"smsconsent = @SmsConsent, mobile = @Mobile, appnotificationconsent = @AppNotificationConsent, isvalidated = @IsValidated, comment = @Comment,")
            .Append(@"riskdescription = @RiskDescription, kycstatusid = @KycStatusId, trustscore = @TrustScore, isrisky = @IsRisky,")
            .Append(@"originationsource = @OriginationSource, installnotifier = @InstallNotifier, commspromptshowncount = @CommsPromptShownCount, lastlogon = @LastLogon");

        return query;
    }

    private sealed class MemberRequest
    {
        public int memberId { get; set; }
        public Guid? cognitoId { get; set; }
        public string email { get; set; }
    }
}