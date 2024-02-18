using Newtonsoft.Json;

namespace Accounts.Domain.Events
{
    public class MemberDetailChanged : AccountsEventBase
    {
        public ulong MemberId { get; set; }
        public int ClientId { get; set; }
        public string MemberNewId { get; set; }
        public bool? IsValidated { get; set; }
        public int? Status { get; set; }

        [JsonIgnore]
        public PIIData PIIDataObject { get; set; }

#nullable enable
        public string? AccessCode { get; set; }
        public bool? ReceiveNewsletter { get; set; }
        public bool? SMSConsent { get; set; }
        public bool? AppNotificationConsent { get; set; }
        public bool? IsRisky { get; set; }
        public string? Comment { get; set; }
        public bool? Active { get; set; }
        public bool? PremiumStatus { get; set; }
        public string? SSOUsername { get; set; }
        public string? SSOProvider { get; set; }
        public int? TrustScore { get; set; }

        public bool? InstallNotifier { get; set; }
        public string? OriginationSource { get; set; }

#nullable disable
    }
}