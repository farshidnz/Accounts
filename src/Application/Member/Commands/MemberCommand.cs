using System;

namespace Accounts.Application.Member.Commands;

public class MemberCommand
{
    public bool? PremiumStatus { get; set; }

    public Guid? CognitoId { get; set; }

    public int? Status { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Mobile { get; set; }

    public string PostCode { get; set; }

    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the receive news letter. subscribeNewsletters
    /// </summary>
    /// <value>
    /// The receive news letter.
    /// </value>
    public bool? SubscribeNewsletters { get; set; }

    /// <summary>
    /// Gets or sets the SMS consent. subscribeSMS
    /// </summary>
    /// <value>
    /// The SMS consent.
    /// </value>
    public bool? SubscribeSMS { get; set; }

    /// <summary>
    /// Gets or sets the application notification consent.subscribeAppNotifications
    /// </summary>
    /// <value>
    /// The application notification consent.
    /// </value>
    public bool? SubscribeAppNotifications { get; set; }

    public string CommsPromptAction { get; set; }
    public bool? ClickWindowActive { get; set; }
    public bool? PopUpActive { get; set; }
    public bool? IsValidated { get; set; }

    public bool? RequiredLogin { get; set; }
    public bool? IsAvailable { get; set; }
    public string MailChimpListEmailId { get; set; }
    public DateTime DateReceivedNewsLetter { get; set; }
    public string CommunicationEmail { get; set; }
    public string PayPalEmail { get; set; }

    public string TwoFactorAuthId { get; set; }

    public bool? TwoFactorAuthenticatedEnable { get; set; }

    public DateTime TwoFactorAuthenticatedActivatedBy { get; set; }

    public string TwoFactorAuthenticatedMobile { get; set; }

    public string TwoFactorAuthenticatedCountryCode { get; set; }

    public string RiskDescription { get; set; }
    public bool? IsRisky { get; set; }
    public DateTime? LastLogon { get; set; }

    public DateTime? DateJoined { get; set; }

    public string Comment { get; set; }

    public bool? IsWithdrawalCapped { get; set; }

    public int? KycStatusId { get; set; }

    public int? SignUpVerificationEmailSentStatus { get; set; }

    public int? TrustScore { get; set; }

    public string OriginationSource { get; set; }

    public bool? Active { get; set; }

    public bool? InstallNotifier { get; set; }
}