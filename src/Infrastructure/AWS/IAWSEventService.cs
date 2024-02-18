using Accounts.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.AWS
{
    public enum EventMessageAttributes
    {
        EventID,
        EventSource,
        EventType,
        Domain,
        CorrelationID,
        EventRaisedDateTimeUTC,
        EventPublishedDateTimeUTC,
        ContextualSequenceNumber
    }

    public interface IAWSEventService
    {
        string AWSResourceName { get; }

        IEnumerable<Type> EventTypes { get; }

        Task Publish(DomainEvent domainEvent);

        IAsyncEnumerable<SQSEvent> ReadEventStream(CancellationToken stoppingToken);

        Task<IEnumerable<SQSEvent>> ReadEvents();

        Task DeleteEvent(SQSEvent sqsEvent);
    }
}