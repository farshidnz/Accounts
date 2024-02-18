using System.Collections.Generic;

namespace Accounts.Domain.Entities.ReportSubscription
{
    // This is only used for the sync data from ShopGo Database to Postgres via ReportSubscriptionMessageEvent
    public class ChangeDetail<T> : IPrimaryKeyObject
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public T Before { get; set; }
        public T After { get; set; }
        public List<ModifiedField> ModifiedFields { get; set; }

        public long PrimaryKey
        {
            get { return Id; }
        }
    }

    public class ModifiedField
    {
        public string FieldName { get; set; }
    }
}
