using Amazon.SQS;
using Amazon.SQS.Model;
using Accounts.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.AWS
{
    public record SQSEvent(DomainEvent DomainEvent, Message Message);

    public class SQSEventService : IAWSEventService
    {
        private readonly IConfiguration configuration;
        private readonly IAmazonSQS sqsClient;
        private readonly ILogger<SQSEventService> logger;
        private readonly string domain;

        public SQSEventService(IConfiguration configuration, string domain, IEnumerable<AWSEventResource> awsEventResources, IAmazonSQS sqsClient, ILogger<SQSEventService> logger)
        {
            this.configuration = configuration;
            this.domain = domain;
            this.EventTypes = awsEventResources.Select(x => x.EventType);
            this.AWSEventResources = awsEventResources;
            this.sqsClient = sqsClient;
            this.logger = logger;
            URL = new AsyncLazy<string>(async () =>
            {
                return await GetURL();
            });
        }

        public string AWSResourceName 
        {
            get
            {
                return configuration["Environment"] + "-" + configuration["ServiceName"] + "-" + this.domain;
            }
        }

        public IEnumerable<AWSEventResource> AWSEventResources { get; }

        public IEnumerable<Type> EventTypes { get; }

        public AsyncLazy<string> URL { get; }

        // take 1 second break if nothing to read
        public virtual Task TakeBreakBeforePollingForEvents(CancellationToken stoppingToken) => Task.Delay(1000, stoppingToken);

        public virtual bool CancellationRequested(CancellationToken stoppingToken) => stoppingToken.IsCancellationRequested;

        public async Task Publish(DomainEvent domainEvent)
        {
            try
            {
                var request = await CreatePublishEventRequest(domainEvent);
                var response = await sqsClient.SendMessageAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Publish Http response code {response.HttpStatusCode}");
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Error publishing domain event: {domainEvent.ToJson()} to SQS {AWSResourceName}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        public virtual async IAsyncEnumerable<SQSEvent> ReadEventStream([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            while (!CancellationRequested(stoppingToken))
            {
                var events = await PollForEvents();
                if (!events.Any())
                {
                    await TakeBreakBeforePollingForEvents(stoppingToken);
                    continue;
                }

                foreach (var @event in events)
                {
                    yield return @event;
                }
            }

            async Task<IEnumerable<SQSEvent>> PollForEvents()
            {
                try
                {
                    return await ReadEvents();
                }
                catch
                {
                    return new List<SQSEvent>();
                }
            }
        }

        public virtual async Task<IEnumerable<SQSEvent>> ReadEvents()
        {
            try
            {
                var request = await CreateReadEventsRequest();
                logger.LogInformation($"Reading events from {request.QueueUrl}");
                var response = await sqsClient.ReceiveMessageAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Read SQS Http response code {response.HttpStatusCode}");
                }

                return response.Messages.Select(x => new SQSEvent(ConvertToDomainEvent(x), x));
            }
            catch (Exception e)
            {
                var errorMessage = $"Error reading from SQS {AWSResourceName}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        private DomainEvent ConvertToDomainEvent(Message message)
        {
            try
            {
                if (message.MessageAttributes.TryGetValue(EventMessageAttributes.EventType.ToString(), out var messageEventType))
                {
                    var matchingEventType = EventTypes.FirstOrDefault(x => x.Name == messageEventType.StringValue);
                    if (matchingEventType != null)
                    {
                        return JsonConvert.DeserializeObject(message.Body, matchingEventType) as DomainEvent;
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to deserialise message : {message.Body}, into event types {string.Join(",", EventTypes.Select(x => x.Name))}.";
                logger.LogError(e, errorMessage);
            }

            return default;
        }

        public virtual async Task DeleteEvent(SQSEvent sqsEvent)
        {
            try
            {
                var response = await sqsClient.DeleteMessageAsync(await URL, sqsEvent.Message.ReceiptHandle);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Delete SQS Http response code {response.HttpStatusCode}");
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Error deleting event \"{sqsEvent.DomainEvent.ToJson()}\" from SQS {AWSResourceName}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        private async Task<string> GetURL()
        {
            try
            {
                var response = await sqsClient.GetQueueUrlAsync(AWSResourceName);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Get queue URL Http response code {response.HttpStatusCode}");
                }

                return response.QueueUrl;
            }
            catch (Exception e)
            {
                var errorMessage = $"Error finding URL for SQS with name \":{AWSResourceName}\" in region :{sqsClient.Config.RegionEndpoint}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        private IContractResolver EventPublishingResolver { get; } = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" });

        private async Task<SendMessageRequest> CreatePublishEventRequest(DomainEvent domainEvent) => new SendMessageRequest()
        {
            QueueUrl = await URL,
            MessageBody = domainEvent.ToJson(EventPublishingResolver),
            MessageAttributes = new()
            {
                { EventMessageAttributes.EventID.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.EventID.ToString(), DataType = "String" } },
                { EventMessageAttributes.EventType.ToString(), new MessageAttributeValue() { StringValue = domainEvent.GetType().Name, DataType = "String" } },
                { EventMessageAttributes.EventSource.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.EventSource, DataType = "String" } },
                { EventMessageAttributes.Domain.ToString(), new MessageAttributeValue() { StringValue = EventMetadata.Domain, DataType = "String" } },
                { EventMessageAttributes.CorrelationID.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.CorrelationID.ToString(), DataType = "String" } },
                { EventMessageAttributes.ContextualSequenceNumber.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.ContextualSequenceNumber.ToString(), DataType = "String" } },
                { EventMessageAttributes.EventRaisedDateTimeUTC.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.RaisedDateTimeUTC.ToString("o"), DataType = "String" } },
                { EventMessageAttributes.EventPublishedDateTimeUTC.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.PublishedDateTimeUTC.ToString("o"), DataType = "String" } }
            }
        };

        private async Task<ReceiveMessageRequest> CreateReadEventsRequest() => new ReceiveMessageRequest
        {
            QueueUrl = await URL,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 20,
            MessageAttributeNames = Enum.GetNames(typeof(EventMessageAttributes)).ToList()
        };

    }
}