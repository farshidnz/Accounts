namespace Accounts.Domain.Enums;

public enum KycStatus
{
    No = 1,
    Emailed = 2,
    DocumentsReceived = 3,
    RefusedToComply = 4,
    Yes = 5
}