namespace Accounts.Domain.Entities.ReportSubscription
{
    // This is only used for the sync data from ShopGo Database to Postgres via ReportSubscriptionMessageEvent
    public interface IPrimaryKeyObject
    {
        long PrimaryKey { get; }
    }
}
