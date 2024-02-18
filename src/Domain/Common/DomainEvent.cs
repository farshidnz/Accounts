using Accounts.Domain.Attributes;
using Accounts.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Accounts.Domain.Common
{
    public interface IHasDomainEvent
    {
        public Queue<DomainEvent> DomainEvents { get; }

        public void RaiseEvent(DomainEvent domainEvent) => DomainEvents.Enqueue(domainEvent);
    }

    public abstract class DomainEvent
    {
        public EventMetadata Metadata { get; set; } = new EventMetadata();

        public string ToJson(IContractResolver contractResolver = default) => JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            ContractResolver = contractResolver
        });

        public EventContext GetEventContext()
        {
            var eventType = this.GetType();
            var EventContextName = eventType.GetCustomAttribute<EventContextName>(true)?.ContextName ?? eventType.Name;
            var eventContextPropertyValues = new List<object> { EventContextName };
            eventContextPropertyValues.AddRange(eventType.GetProperties()
                                                         .Where(p => p.IsDefined(typeof(EventContextProvider)))
                                                         .Select(x => x.GetValue(this)));

            return new EventContext(eventContextPropertyValues);
        }
    }

    public class EventMetadata
    {
        public Guid EventID { get; set; } = Guid.NewGuid();

        public string EventSource { get; set; }

        public string EventType { get; set; }

        public static string Domain => "Member";

        public Guid CorrelationID { get; set; }

        public DateTimeOffset RaisedDateTimeUTC { get; set; } = DateTime.UtcNow;

        public DateTimeOffset PublishedDateTimeUTC { get; set; } = DateTime.UtcNow;

        // the context is per domain entity/event type
        public ulong ContextualSequenceNumber { get; set; }
    }
}