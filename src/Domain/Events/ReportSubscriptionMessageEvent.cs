using Accounts.Domain.Common;
using Newtonsoft.Json;
using System;

namespace Accounts.Domain.Events
{
    public class ReportSubscriptionMessageEvent : DomainEvent
    {
        [JsonProperty("displayName")]
        public static string DisplayName => "Cashrewards\\PlainQueue\\CallQueuedHandler";

        [JsonProperty("job")]
        public static string Job => "Cashrewards\\PlainQueue\\CallQueuedHandler@call";

        [JsonProperty("maxTries")]
        public int? MaxTries { get; set; }

        [JsonProperty("timeout")]
        public int? Timeout { get; set; }

        [JsonProperty("data")]
        public ReportSubscriptionMessageEventData Data { get; set; }
    }

    public class ReportSubscriptionMessageEventData
    {
        public Guid UniqueId { get; set; }
        public string TableName { get; set; }
        public string Changes { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
