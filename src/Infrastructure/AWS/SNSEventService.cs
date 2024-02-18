using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
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
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.AWS
{
    public class SNSEventService : IAWSEventService
    {
        private readonly IConfiguration configuration;
        private readonly IAmazonSimpleNotificationService snsClient;
        private readonly ILogger<SNSEventService> logger;

        public SNSEventService(IConfiguration configuration, AWSEventResource awsEventResource, IAmazonSimpleNotificationService snsClient, ILogger<SNSEventService> logger)
        {
            this.configuration = configuration;
            this.AWSEventResource = awsEventResource;
            this.EventTypes = new List<Type> { awsEventResource.EventType };
            this.snsClient = snsClient;
            this.logger = logger;
            ARN = new AsyncLazy<string>(async () =>
            {
                return await GetARN();
            });
        }

        public string AWSResourceName
        {
            get
            {
                return configuration["Environment"] + "-" + AWSEventResource.Domain;
            }
        }

        public AWSEventResource AWSEventResource { get; }

        public IEnumerable<Type> EventTypes { get; }

        private AsyncLazy<string> ARN { get; }

        private IContractResolver EventPublishingResolver { get; } = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" });

        private async Task<PublishRequest> CreatePublishEventRequest(DomainEvent domainEvent) => new PublishRequest()
        {
            TopicArn = await ARN,
            Message = domainEvent.ToJson(EventPublishingResolver),
            MessageAttributes = new()
            {
                { EventMessageAttributes.EventID.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.EventID.ToString(), DataType = "String" } },
                { EventMessageAttributes.EventType.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.EventType, DataType = "String" } },
                { EventMessageAttributes.EventSource.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.EventSource, DataType = "String" } },
                { EventMessageAttributes.Domain.ToString(), new MessageAttributeValue() { StringValue = EventMetadata.Domain, DataType = "String" } },
                { EventMessageAttributes.CorrelationID.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.CorrelationID.ToString(), DataType = "String" } },
                { EventMessageAttributes.ContextualSequenceNumber.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.ContextualSequenceNumber.ToString(), DataType = "String" } },
                { EventMessageAttributes.EventRaisedDateTimeUTC.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.RaisedDateTimeUTC.ToString("o"), DataType = "String" } },
                { EventMessageAttributes.EventPublishedDateTimeUTC.ToString(), new MessageAttributeValue() { StringValue = domainEvent.Metadata.PublishedDateTimeUTC.ToString("o"), DataType = "String" } }
            }
        };

        public async Task Publish(DomainEvent domainEvent)
        {
            try
            {
                logger.LogInformation($"Publishing event with metadata {JsonConvert.SerializeObject(domainEvent?.Metadata)}");
                var request = await CreatePublishEventRequest(domainEvent);
                var response = await snsClient.PublishAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Publish Http response code {response.HttpStatusCode}");
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Error publishing domain event: {domainEvent.ToJson()} to SNS {AWSResourceName}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        private async Task<string> GetARN()
        {
            string topicName = string.Empty;
            try
            {
                string nextToken = string.Empty;
                do
                {
                    var response = await snsClient.ListTopicsAsync(nextToken);
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Get SNS topics Http response code {response.HttpStatusCode}");
                    }

                    var matchingTopic = response.Topics.FirstOrDefault(x => x.TopicArn.EndsWith($":{AWSResourceName}", StringComparison.OrdinalIgnoreCase));
                    if (matchingTopic != null)
                    {
                        return matchingTopic.TopicArn;
                    }

                    nextToken = response.NextToken;
                } while (!string.IsNullOrEmpty(nextToken));

                throw new Exception("Could not find the topic in the topic list");
            }
            catch (Exception e)
            {
                var errorMessage = $"Error finding ARN ending with name \":{AWSResourceName}\" in region :{snsClient.Config.RegionEndpoint}";
                logger.LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        public IAsyncEnumerable<SQSEvent> ReadEventStream(CancellationToken stoppingToken) => throw new NotSupportedException();

        public Task<IEnumerable<SQSEvent>> ReadEvents() => throw new NotSupportedException();

        public Task DeleteEvent(SQSEvent sqsEvent) => throw new NotSupportedException();
    }
}