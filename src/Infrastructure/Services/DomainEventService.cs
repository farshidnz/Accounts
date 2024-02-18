using MassTransit.Mediator;
using Accounts.Domain.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Extensions;
using Accounts.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Services
{
    public class DomainEventService : IDomainEventService
    {
        private readonly IEventOutboxPersistenceContext eventOutboxPersistenceContext;
        private readonly ILogger<DomainEventService> logger;
        private readonly IMediator mediator;
        private readonly IAWSEventServiceFactory eventServiceFactory;

        public DomainEventService(IEventOutboxPersistenceContext eventOutboxPersistenceContext,
                                  ILogger<DomainEventService> logger,
                                  IMediator mediator,
                                  IAWSEventServiceFactory eventServiceFactory)
        {
            this.eventOutboxPersistenceContext = eventOutboxPersistenceContext;
            this.logger = logger;
            this.mediator = mediator;
            this.eventServiceFactory = eventServiceFactory;
        }

        private async Task Publish(DomainEvent domainEvent)
        {
            logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.ToJson());

            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            await Task.WhenAll(PublishToExternalEventDestinations(domainEvent),
                               PublishToInternalEventHandlers(domainEvent));
        }

        public async Task PublishToInternalEventHandlers(DomainEvent domainEvent)
        {
            await mediator.PublishEvent(domainEvent);
        }

        private async Task PublishToExternalEventDestinations(DomainEvent domainEvent)
        {
            var externalPublishers = eventServiceFactory.GetAWSPublishersForEvent(domainEvent);
            var hasEventPublishers = externalPublishers?.Any() ?? false;
            if (hasEventPublishers)
            {
                await Task.WhenAll(externalPublishers.Select(x => x.Publish(domainEvent)));
            }
        }

        public async Task PublishEventOutbox()
        {
            try
            {
                var eventOutbox = await eventOutboxPersistenceContext.Get();
                while (eventOutbox?.Count > 0)
                {
                    await Publish(eventOutbox.Dequeue());
                    await eventOutboxPersistenceContext.Save(eventOutbox);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception in publishing event outbox, Error: {e.Message}");
            }
        }
    }
}