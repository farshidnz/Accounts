using Accounts.Domain.Common;
using Accounts.Domain.Enums;
using System;

namespace Accounts.Domain.Entities;

public class Member : DomainEntity, IHasIdentity<int>
{
    public int ID => MemberId;
    public int MemberId { get; set; }
    public Guid? CognitoId { get; set; }

    public bool? PremiumStatus { get; set; }

    public Gender? Gender { get; set; }

    public int Status { get; set; } = 1;

    public Guid? MemberNewId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime? DateReceiveNewsletter { get; set; }
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Mobile { get; set; }

    public string PostCode { get; set; }

    public string Email { get; set; }

    public string SSOUsername { get; set; }

    public string SSOProvider { get; set; }

    public string AccessCode { get; set; }

    public bool ReceiveNewsLetter { get; set; }

    public bool SmsConsent { get; set; }
    public bool AppNotificationConsent { get; set; }
    public int CommsPromptShownCount { get; set; }
    public bool ClickWindowActive { get; set; }
    public bool PopUpActive { get; set; }
    public bool IsValidated { get; set; }

    public bool RequiredLogin { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime ActiveBy { get; set; }
    public string MailChimpListEmailId { get; set; }
    public DateTime DateReceivedNewsLetter { get; set; }
    public string CommunicationEmail { get; set; }
    public string PayPalEmail { get; set; }
    public int CampaignId { get; set; }

    public string TwoFactorAuthId { get; set; }

    public bool TwoFactorAuthenticatedEnable { get; set; }

    public DateTime TwoFactorAuthenticatedActivatedBy { get; set; }

    public string TwoFactorAuthenticatedMobile { get; set; }

    public string TwoFactorAuthenticatedCountryCode { get; set; }

    public string RiskDescription { get; set; }
    public bool IsRisky { get; set; }
    public DateTime LastLogon { get; set; }
    public DateTime DateDeletedByMember { get; set; }

    public DateTime DateJoined { get; set; }

    public string Comment { get; set; }

    public bool IsWithdrawalCapped { get; set; }

    public int KycStatusId { get; set; }

    public bool InstallNotifier { get; set; }
    public int SignUpVerificationEmailSentStatus { get; set; }

    public int CognitoPoolId { get; set; }

    public int TrustScore { get; set; }

    public string OriginationSource { get; set; }

    public bool Active { get; set; }

    public bool IsNewMember { get; set; }

    public bool IsTwoFactorAuthenticatedEnable { get; set; }
}