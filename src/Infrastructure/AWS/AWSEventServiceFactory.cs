global using EventType = System.Type;

using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Accounts.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Accounts.Infrastructure.AWS
{
    internal record ReadServiceKey(AwsEventReadMode AWSReadMode, AwsResourceType AWSResourceType, string Domain);

    public class AWSEventServiceFactory : IAWSEventServiceFactory
    {
        private readonly IConfiguration configuration;
        private readonly IAmazonSimpleNotificationService snsClient;
        private readonly ConcurrentDictionary<ReadServiceKey, IAWSEventService> eventReaders;
        private readonly ConcurrentDictionary<EventType, List<IAWSEventService>> eventPublishers;
        private readonly IAmazonSQS sqsClient;

        private readonly ILoggerFactory loggerFactory;

        public AWSEventServiceFactory(IAmazonSQS sqsClient,
                                      IAmazonSimpleNotificationService snsClient,
                                      ILoggerFactory loggerFactory,
                                      IConfiguration configuration)
        {
            this.sqsClient = sqsClient;
            this.loggerFactory = loggerFactory;
            this.snsClient = snsClient;
            this.configuration = configuration;
            this.eventReaders = BuildEventReaders(configuration);
            this.eventPublishers = BuildEventPublishers(configuration);
        }

        private ConcurrentDictionary<EventType, List<IAWSEventService>> BuildEventPublishers(IConfiguration configuration)
        {
            var eventDestinations = GetAwsResources<AWSEventDestination>(configuration, "EventDestinations:AWSResources");

            var publishers = eventDestinations.GroupBy(x => x.EventType)
                                              .ToDictionary(g => g.Key, g => g.Select(x => BuildEventPublishingService(x)).ToList());

            return new ConcurrentDictionary<EventType, List<IAWSEventService>>(publishers);
        }

        private ConcurrentDictionary<ReadServiceKey, IAWSEventService> BuildEventReaders(IConfiguration configuration)
        {
            var eventSources = GetAwsResources<AWSEventSource>(configuration, "EventSources:AWSResources");

            var readers = eventSources.GroupBy(x => (x.AWSReadMode, x.AWSResourceType, x.Domain))
                                      .ToDictionary(g => new ReadServiceKey(g.Key.AWSReadMode, g.Key.AWSResourceType, g.Key.Domain),
                                                    g => BuildEventReadingService(g.Key.AWSResourceType, g.Key.Domain, g.ToList()));

            return new ConcurrentDictionary<ReadServiceKey, IAWSEventService>(readers);
        }

        private List<T> GetAwsResources<T>(IConfiguration configuration, string section) where T : AWSEventResource
        {
            return configuration.GetSection(section).Get<List<T>>()?.Select(x =>
            {
                x.EventType = GetEventType(x.EventTypeName);
                return x;
            }).Where(t => t.EventType != null).ToList() ?? new();
        }

        private IAWSEventService BuildEventReadingService(AwsResourceType awsResourceType, string domain, IEnumerable<AWSEventSource> awsEventResources)
        {
            return awsResourceType switch
            {
                AwsResourceType.SQS => new SQSEventService(configuration, domain, awsEventResources, sqsClient, loggerFactory.CreateLogger<SQSEventService>()),
                _ => default,
            };
        }

        private IAWSEventService BuildEventPublishingService(AWSEventDestination awsEventResource)
        {
            return awsEventResource.AWSResourceType switch
            {
                AwsResourceType.SNS => new SNSEventService(configuration, awsEventResource, snsClient, loggerFactory.CreateLogger<SNSEventService>()),
                _ => default,
            };
        }

        public List<IAWSEventService> GetAWSPublishersForEvent(DomainEvent domainEvent)
        {
            eventPublishers.TryGetValue(domainEvent.GetType(), out var publishers);
            return publishers ?? new();
        }

        public List<IAWSEventService> GetAWSEventReaders(AwsEventReadMode readMode)
        {
            return eventReaders.Where(x => x.Key.AWSReadMode == readMode).Select(v => v.Value).ToList();
        }

        private EventType GetEventType(string eventTypeName) => DomainAssembly.GetTypes()
                                                                              .FirstOrDefault(x => x.Name == eventTypeName &&
                                                                                                   x.IsSubclassOf(typeof(DomainEvent)));

        public virtual Assembly DomainAssembly => Assembly.Load("Accounts.Domain");
    }
}