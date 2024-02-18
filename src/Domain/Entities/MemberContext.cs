using System;

namespace Accounts.Domain.Entities;

public class MemberContext
{
    public string Email { get; set; }
    public Guid? CognitoId { get; set; }

    public bool IsGetByCognitoId { get; set; }

    public bool IsGetByMemberId { get; set; } = false;

    public bool PremiumStatus { get; set; }

    public int Status { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Mobile { get; set; }

    public string PostCode { get; set; }

    public bool? IsRisky { get; set; }

    public DateTime LastLogon { get; set; }

    public bool? ReceiveNewsLetter { get; set; }

    public bool? AppNotificationConsent { get; set; }

    public bool? SmsConcent { get; set; }

    public bool? InstallNotifier { get; set; }

    public int? CommsPromptShownCount { get; set; }

    public int MemberId { get; set; }
}