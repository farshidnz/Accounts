using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FluentAssertions;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.AWS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accounts.Domain.Common;

namespace Infrastructure.UnitTests.AWS
{
    [TestFixture]
    public class SNSEventServiceTests
    {
        private Mock<IConfiguration> configuration;
        private Mock<IAmazonSimpleNotificationService> snsClient;
        private Mock<ILogger<SNSEventService>> logger;
        private AWSEventDestination awsEventResource;

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p[It.Is<string>(s => s == "Environment")]).Returns("test");

            snsClient = new Mock<IAmazonSimpleNotificationService>();
            snsClient.Setup(x => x.Config.RegionEndpoint).Returns(RegionEndpoint.APSoutheast2);
            snsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).ReturnsAsync(new PublishResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

            logger = new Mock<ILogger<SNSEventService>>();

            awsEventResource = new AWSEventDestination()
            {
                Type = "SNS",
                Domain = "Member"
            };
        }

        public SNSEventService SUT(AWSEventResource awsEventResource)
        {
            return new SNSEventService(configuration.Object, awsEventResource, snsClient.Object, logger.Object);
        }

        [Test]
        public async Task WhenPublishing_ShouldFindTopicARN_AndPublishEventToARN()
        {
            snsClient.SetupSequence(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                NextToken = "there is more token",
                Topics = new List<Topic>
                {
                    new Topic() { TopicArn = "region:account:team1-Member" },
                    new Topic() { TopicArn = "region:account:team2-Member" },
                    new Topic() { TopicArn = "region:account:team3-Member" }
                }
            }).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                NextToken = null,
                Topics = new List<Topic>
                {
                    new Topic() { TopicArn = "region:account:team4-Member" },
                    new Topic() { TopicArn = "region:account:team5-Member" },
                    new Topic() { TopicArn = "region:account:test-Member" }
                }
            });

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" })
            };

            var domainEvent = new TestEvent();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "Accounts";
            domainEvent.Metadata.CorrelationID = Guid.NewGuid();
            domainEvent.Metadata.ContextualSequenceNumber = 987654321;
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            await SUT(awsEventResource).Publish(domainEvent);

            snsClient.Verify(x => x.PublishAsync(It.Is<PublishRequest>(x =>
                                                            x.TopicArn == "region:account:test-Member" &&
                                                            x.Message == JsonConvert.SerializeObject(domainEvent, settings) &&
                                                            x.MessageAttributes["EventID"].StringValue == domainEvent.Metadata.EventID.ToString() &&
                                                            x.MessageAttributes["EventType"].StringValue == domainEvent.Metadata.EventType &&
                                                            x.MessageAttributes["EventSource"].StringValue == domainEvent.Metadata.EventSource &&
                                                            x.MessageAttributes["Domain"].StringValue == EventMetadata.Domain &&
                                                            x.MessageAttributes["CorrelationID"].StringValue == domainEvent.Metadata.CorrelationID.ToString() &&
                                                            x.MessageAttributes["ContextualSequenceNumber"].StringValue == "987654321" &&
                                                            x.MessageAttributes["EventRaisedDateTimeUTC"].StringValue == domainEvent.Metadata.RaisedDateTimeUTC.ToString("o") &&
                                                            x.MessageAttributes["EventPublishedDateTimeUTC"].StringValue == domainEvent.Metadata.PublishedDateTimeUTC.ToString("o")), default));
        }

        [Test]
        public void WhenPublishing_AndCantFindTopicARN_ShouldThowARNNotFoundException()
        {
            snsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic>()
            });

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventResource).Publish(new TestEvent()));
            ex.Message.Should().Contain($"Error finding ARN ending with name \":test-Member\" in region :{RegionEndpoint.APSoutheast2}");
        }

        [Test]
        public void WhenPublishing_AndErrorResponseToFindingTopicARN_ShouldThowARNNotFoundException()
        {
            snsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest
            });

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventResource).Publish(new TestEvent()));
            var expectedReason = new Exception($"Get SNS topics Http response code {System.Net.HttpStatusCode.BadRequest}");
            ex.Message.Should().Contain($"Error finding ARN ending with name \":test-Member\" in region :{RegionEndpoint.APSoutheast2}, " +
                $"error: {expectedReason}");
        }

        [Test]
        public void WhenExceptionInPublishing_ShouldThowPublishingErrorException()
        {
            snsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic>() { new Topic() { TopicArn = "region:account:test-Member" } }
            });

            var exception = new Exception("some publishing error");
            snsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).Throws(exception);

            var domainEvent = new TestEvent();
            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventResource).Publish(domainEvent));
            ex.Message.Should().StartWith($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SNS {"test-Member"}, error: {exception}");
        }

        [Test]
        public void WhenErrorResponseInPublishing_ShouldThowPublishingErrorException()
        {
            snsClient.Setup(x => x.ListTopicsAsync(It.IsAny<string>(), default)).ReturnsAsync(new ListTopicsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Topics = new List<Topic>() { new Topic() { TopicArn = "region:account:test-Member" } }
            });

            snsClient.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), default)).ReturnsAsync(new PublishResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var domainEvent = new TestEvent();
            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventResource).Publish(domainEvent));
            var expectedReason = new Exception($"Publish Http response code {System.Net.HttpStatusCode.InternalServerError}");
            ex.Message.Should().StartWith($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SNS {"test-Member"}, " +
                $"error: {expectedReason}");
        }
    }
}