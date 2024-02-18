using Accounts.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Accounts.Domain.Common
{
    public interface IHasIdentity<out T>
    {
        public T ID { get; }
    }

    public abstract class DomainEntity : IHasDomainEvent
    {
        // keep track of contextual seq numbers, the event context is defined by EventContextProvider
        // property attributes on each event.
        private readonly Dictionary<EventContext, ulong> ContextualEventSequenceNumbers = new();

        private bool HasEvents { get; set; }
        public Queue<DomainEvent> DomainEvents { get; } = new();

        public bool HasDomainEvents
        {
            get { return (DomainEvents.Count != 0 || HasEvents); }

            set { HasEvents = value; }
        }

        // set your domain name here to populate event metadata
        public string DomainName { get; set; }

        // set the MessageType here for CAP outbox
        public string MessageType { get; set; }

        public void RaiseEvent(DomainEvent domainEvent)
        {
            var eventContext = domainEvent.GetEventContext();
            ContextualEventSequenceNumbers.TryGetValue(eventContext, out ulong prevSeqNum);

            var nextSeqNum = prevSeqNum + 1;
            domainEvent.Metadata.ContextualSequenceNumber = nextSeqNum;

            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "Accounts";

            ContextualEventSequenceNumbers[eventContext] = nextSeqNum;

            DomainEvents.Enqueue(domainEvent);
        }

        public void AssignEventsCorrelationID(Guid? correlationID)
        {
            correlationID = correlationID ?? Guid.NewGuid();
            foreach (var domainEvent in DomainEvents)
            {
                domainEvent.Metadata.CorrelationID = correlationID.Value;
            }
        }
    }
}