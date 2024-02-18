using Amazon.SQS.Model;
using MassTransit.Mediator;
using Accounts.Application.Common.Models;
using Accounts.Domain.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Extensions;
using Accounts.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.BackgroundHostedService
{
    public class EventPolledReadingService : BackgroundService
    {
        private readonly ILogger<EventPolledReadingService> logger;

        private readonly IAWSEventServiceFactory awsEventServiceFactory;
        private readonly IMediator mediator;
        public readonly string ServiceName = "EventPolledReadingService";

        public EventPolledReadingService(ILogger<EventPolledReadingService> logger,
                                         IAWSEventServiceFactory awsEventServiceFactory,
                                         IMediator mediator)
        {
            this.logger = logger;
            this.awsEventServiceFactory = awsEventServiceFactory;
            this.mediator = mediator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => logger.LogInformation($"{ServiceName} background task is stopping due to CancellationToken."));

            await PolledReadEvents(stoppingToken);

            logger.LogInformation($"{ServiceName} background task is stopping.");
        }

        public async Task PolledReadEvents(CancellationToken stoppingToken)
        {
            var readers = awsEventServiceFactory.GetAWSEventReaders(AwsEventReadMode.PolledRead);

            await Task.WhenAll(readers.Select(x => ReadEvents(x, stoppingToken)));
        }

        private async Task ReadEvents(IAWSEventService eventService, CancellationToken stoppingToken)
        {
            if (eventService != null)
            {
                logger.LogInformation($"{ServiceName} is starting to read and process events from SQS : {eventService.AWSResourceName}.");

                await foreach (var sqsEvent in eventService.ReadEventStream(stoppingToken))
                {
                    await ProcessEvent(eventService, sqsEvent);
                }
            }
        }

        private async Task ProcessEvent(IAWSEventService eventService, SQSEvent sqsEvent)
        {
            if (IsValidDomainEvent(sqsEvent))
            {
                if (await PublishToInternalEventHandlers(sqsEvent))
                {
                    await eventService.DeleteEvent(sqsEvent);
                }
            }
            else
            {
                await eventService.DeleteEvent(sqsEvent);
            }
        }

        private bool IsValidDomainEvent(SQSEvent sqsEvent) => sqsEvent.DomainEvent != default;
        
        public async Task<bool> PublishToInternalEventHandlers(SQSEvent sqsEvent)
        {
            try
            {
                await mediator.PublishEvent(sqsEvent.DomainEvent, sqsEvent.Message);
                return true;
            }
            catch (Exception e)
            {
                var errorMessage = $"Error handling domain event: {sqsEvent.DomainEvent.ToJson()}";
                logger.LogError(e, errorMessage);
                return false;
            }
        }
    }
}