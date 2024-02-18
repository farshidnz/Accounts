using Accounts.Domain.Common;
using Newtonsoft.Json;
using System;

namespace Accounts.Domain.Events
{
    public class AccountsEventBase : DomainEvent
    {
        public Guid CognitoId { get; set; }

#nullable enable
        public string? PIIIEncryptAlgorithm { get; set; }
        public string? PIIData { get; set; }
    }

    public class PIIData
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PostCode { get; set; }
        public string? Gender { get; set; }

        [JsonIgnore]
        public bool HasPIIData => (!string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(LastName) || !string.IsNullOrEmpty(Email) ||
                !string.IsNullOrEmpty(Phone) || DateOfBirth != null || !string.IsNullOrEmpty(PostCode) || Gender != null);
    }

#nullable disable
}