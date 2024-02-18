namespace Accounts.Domain.Enums;
using System.ComponentModel;
using System.Runtime.Serialization;
public enum CommsPromptDismissalAction
{
    [Description("Close")]
    [EnumMember]
    Close,

    [Description("Review")]
    [EnumMember]
    Review
}