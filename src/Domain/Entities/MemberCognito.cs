using Accounts.Domain.Common;
using Accounts.Domain.Enums;
using System;


namespace Accounts.Domain.Entities;

public class MemberCognito : DomainEntity, IHasIdentity<int>
{
    public int ID => CognitoPoolId;
    public int CognitoPoolId { get; set; }
    public string CognitoPoolName { get; set; }
#nullable enable
    public Guid? CognitoId { get; set; }
    public string? Email { get; set; }

    public string? PIIData { get; set; } = "";

    public string? PIIIEncryptAlgorithm { get; set; }
#nullable disable
}
