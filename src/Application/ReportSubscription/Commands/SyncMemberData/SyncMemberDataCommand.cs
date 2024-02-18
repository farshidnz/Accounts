using System;
using Accounts.Domain.Entities.ReportSubscription;

namespace Accounts.Application.ReportSubscription.Commands.SyncMemberData
{
    public class SyncMemberDataCommand<T>
    {
        public string TableName { get;  set; }
        public ChangeDetail<T> Changes { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
