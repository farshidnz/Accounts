using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Accounts.Domain.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Exceptions;
using Accounts.Infrastructure.Extensions;
using Accounts.Infrastructure.OutboxMessages;
using DotNetCore.CAP;
using MassTransit.Mediator;

namespace Accounts.Infrastructure.OutboxHandlers;

public class OutboxHandlers : ICapSubscribe
{
    private readonly Assembly _assembly;
    private readonly IMediator _mediator;
    private readonly IAWSEventServiceFactory _eventServiceFactory;
    public OutboxHandlers(Assembly assembly, IMediator mediator, IAWSEventServiceFactory eventServiceFactory)
    {
        _assembly = assembly;
        _mediator = mediator;
        _eventServiceFactory = eventServiceFactory;
    }
    [CapSubscribe(nameof(OutboxHandlers))]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Handle(OutboxMessage outboxMessage)
    {
        var messageType = _assembly.GetType(outboxMessage.MessageType);
        if (messageType == null)
        {
            throw new InvalidOperationException(
                $"{outboxMessage.Metadata?.EventType} not found in assembly {_assembly.FullName}");
        }
        
        try
        {
            var domainEvent = Newtonsoft.Json.JsonConvert.DeserializeObject(outboxMessage.Message, messageType);
            await Task.WhenAll(PublishToExternalEventDestinations((DomainEvent)domainEvent),PublishToInternalEventHandlers((DomainEvent)domainEvent));
        }
        catch (InvalidOperationException e)
        {
            throw new OutboxMessageHandlerNotFoundException(e);
        }
    }
    public async Task PublishToInternalEventHandlers(DomainEvent domainEvent)
    {
        await _mediator.PublishEvent(domainEvent);
    }

    private async Task PublishToExternalEventDestinations(DomainEvent domainEvent)
    {
        var externalPublishers = _eventServiceFactory.GetAWSPublishersForEvent(domainEvent);
        var hasEventPublishers = externalPublishers?.Any() ?? false;
        if (hasEventPublishers)
        {
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;
            await Task.WhenAll(externalPublishers.Select(x => x.Publish(domainEvent)));
        }
    }

}