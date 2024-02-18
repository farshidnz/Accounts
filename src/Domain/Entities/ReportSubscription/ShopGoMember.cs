using Accounts.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Entities.ReportSubscription
{
#nullable enable
    public class ShopGoMember
    {
        public int MemberId { get; set; }
        public int? ClientId { get; set; }
        public int? Status { get; set; }
        // public string DateOfBirth { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PostCode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? UserPassword { get; set; }
        public string? SaltKey { get; set; }
        // public string CookieIpAddress { get; set; }
        // public string FacebookUsername { get; set; }
        // public string AccessCode { get; set; }
        // public int? Gender { get; set; }
        public bool? ReceiveNewsLetter { get; set; }
        // public bool? SmsConsent { get; set; }
        //  public bool? AppNotificationConsent { get; set; }
        //  public int? CommsPromptShownCount { get; set; }
        public bool? ClickWindowActive { get; set; }
        public bool? PopUpActive { get; set; }
        public bool? IsValidated { get; set; }
        public bool? IsResetPassword { get; set; }
        public bool? RequiredLogin { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime? ActivateBy { get; set; }
        // public DateTime? DateDeletedByMember { get; set; }
        public DateTime? DateJoined { get; set; }
        public string? HashedEmail { get; set; }
        public string? HashedMobile { get; set; }
        //  public string MailChimpListEmailID { get; set; }
        // public DateTime? DateReceiveNewsLetter { get; set; }
        public string? CommunicationsEmail { get; set; }
        public MemberNewId? MemberNewId { get; set; }
        //  public string HashedMemberNewId { get; set; }
        public bool? AutoCreated { get; set; }
        //  public string PaypalEmail { get; set; }
        //  public int? CampaignId { get; set; }
        //   public string TwoFactorAuthyId { get; set; }
        //   public bool? IsTwoFactorAuthEnabled { get; set; }
        //   public string TwoFactorAuthActivationToken { get; set; }
        //  public DateTime? TwoFactorAuthActivateBy { get; set; }
        //  public string TwoFactorAuthActivationMobile { get; set; }
        //  public string TwoFactorAuthActivationCountryCode { get; set; }
        //   public string RiskDescription { get; set; }
        //   public bool? IsRisky { get; set; }
        //   public DateTime? LastLogon { get; set; }
        //  public int? Source { get; set; }
        //  public DateTime? DateDeletedByMemberUtc { get; set; }
        //   public DateTime? DateJoinedUtc { get; set; }
        //   public DateTime? DateReceiveNewsLetterUtc { get; set; }
        //   public DateTime? TwoFactorAuthActivateByUtc { get; set; }
        //   public DateTime? LastLogonUtc { get; set; }
        //   public string Comment { get; set; }
        //   public string MobileSHA256 { get; set; }
        //   public bool? InstallNotifier { get; set; }
        //   public int? KycStatusId { get; set; }
        //   public int? PersonId { get; set; }
        //   public bool? IsWithdrawalCapped { get; set; }
        //   public int? SignupVerificationEmailSentStatus { get; set; }
    }

    public class MemberNewId
    {
        public string? Guid { get; set; }
    }
#nullable disable
}
