using System;

namespace Accounts.Domain.Events
{
    // PIIData should include encryped email and phone
    public class MemberJoined : AccountsEventBase
    {
        public ulong MemberId { get; set; }
        public int ClientId { get; set; }
        public string CognitoPoolId { get; set; }
        public int OriginationSource { get; set; } // 1 - website, 2- mobileapp, 3 - moneyMe App
        public DateTime DateJoined { get; set; }  // It's UTC format
        public string MemberNewId { get; set; }
        public bool IsValidated { get; set; }
        public int Status { get; set; }
        public PIIData PIIDataObject { get; set; }
    }
}