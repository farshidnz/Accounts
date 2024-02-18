using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accounts.Domain.Common;

namespace Infrastructure.UnitTests.AWS
{
    [TestFixture]
    public class SQSEventServiceTests
    {
        private Mock<IConfiguration> configuration;
        private string queueUrl;
        private Mock<IAmazonSQS> sqsClient;
        private Mock<ILogger<SQSEventService>> logger;

        private AWSEventDestination awsEventDestination = new()
        {
            Type = "SQS",
            Domain = "Member",
            EventType = typeof(TestEvent)
        };

        private AWSEventDestination awsEventSource = new()
        {
            Type = "SQS",
            Domain = "Member",
            EventType = typeof(TestEvent)
        };

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p[It.Is<string>(s => s == "Environment")]).Returns("test");
            configuration.SetupGet(p => p[It.Is<string>(s => s == "ServiceName")]).Returns("accounts");

            queueUrl = "test-accounts-Member";

            sqsClient = new Mock<IAmazonSQS>();
            sqsClient.Setup(x => x.Config.RegionEndpoint).Returns(RegionEndpoint.APSoutheast2);
            sqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).ReturnsAsync(new SendMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });
            sqsClient.Setup(x => x.GetQueueUrlAsync(queueUrl, default)).ReturnsAsync(new GetQueueUrlResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                QueueUrl = "sqsURL"
            });
            sqsClient.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(new DeleteMessageResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

            logger = new Mock<ILogger<SQSEventService>>();
        }

        public SQSEventService SUT(AWSEventResource awsEventResource)
        {
            return SQSServicePartialMock(awsEventResource).Object;
        }

        private Mock<SQSEventService> SQSServicePartialMock(AWSEventResource awsEventResource)
        {
            var sut = new Mock<SQSEventService>(configuration.Object, awsEventDestination.Domain, new List<AWSEventResource> { awsEventResource }, sqsClient.Object, logger.Object)
            {
                CallBase = true
            };
            sut.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                               .Returns(false)
                               .Returns(true);
            sut.Setup(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            return sut;
        }

        [Test]
        public async Task WhenPublishing_ShouldFindSQSUrl_AndPublishEventToURL()
        {
            var domainEvent = new TestEvent() { state = "state info" };
            domainEvent.Metadata.EventID = Guid.NewGuid();
            domainEvent.Metadata.EventType = domainEvent.GetType().Name;
            domainEvent.Metadata.EventSource = "Accounts";
            domainEvent.Metadata.CorrelationID = Guid.NewGuid();
            domainEvent.Metadata.ContextualSequenceNumber = 987654321;
            domainEvent.Metadata.RaisedDateTimeUTC = DateTime.UtcNow;
            domainEvent.Metadata.PublishedDateTimeUTC = DateTime.UtcNow;

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new EventPublishingJsonContractResolver(propNamesToIgnore: new[] { "Metadata" })
            };

            await SUT(awsEventDestination).Publish(domainEvent);

            sqsClient.Verify(x => x.SendMessageAsync(It.Is<SendMessageRequest>(x =>
                                                            x.QueueUrl == "sqsURL" &&
                                                            x.MessageBody == JsonConvert.SerializeObject(domainEvent, settings) &&
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
        public void WhenPublishing_AndCantFindSQSUrl_ShouldThowURLNotFoundException()
        {
            sqsClient.Setup(x => x.GetQueueUrlAsync(queueUrl, default)).ReturnsAsync(new GetQueueUrlResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.NotFound
            });

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventDestination).Publish(new TestEvent()));
            ex.Message.Should().Contain($"Error finding URL for SQS with name \":{queueUrl}\" in region :{RegionEndpoint.APSoutheast2}");
        }

        [Test]
        public void WhenExceptionInPublishing_ShouldThowPublishingErrorException()
        {
            var exception = new Exception("some publishing error");
            sqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).Throws(exception);

            var domainEvent = new TestEvent();
            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventDestination).Publish(domainEvent));
            ex.Message.Should().StartWith($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SQS {queueUrl}, error: {exception}");
        }

        [Test]
        public void WhenErrorResponseInPublishing_ShouldThowPublishingErrorException()
        {
            sqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default)).ReturnsAsync(new SendMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var domainEvent = new TestEvent();
            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventDestination).Publish(domainEvent));
            var expectedReason = new Exception($"Publish Http response code {System.Net.HttpStatusCode.InternalServerError}");
            ex.Message.Should().StartWith($"Error publishing domain event: {JsonConvert.SerializeObject(domainEvent)} to SQS {queueUrl}, " +
                $"error: { expectedReason }");
        }

        [Test]
        public async Task WhenReading_ShouldReturnEventsFromSqsQueue()
        {
            var testEvent = new TestEvent() { state = "test" };

            sqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ReceiveMessageResponse()
                     {
                         HttpStatusCode = System.Net.HttpStatusCode.OK,
                         Messages = new List<Message> {
                             new Message {
                                 Body = JsonConvert.SerializeObject(testEvent),
                                 MessageAttributes = new Dictionary<string, MessageAttributeValue>
                                 {
                                     { "EventType", new MessageAttributeValue() { StringValue = "TestEvent", DataType = "String" }}
                                 }
                             }
                         }
                     });

            var readEvents = await SUT(awsEventSource).ReadEvents();

            readEvents.Count().Should().Be(1);

            var readEvent = readEvents.First().DomainEvent;
            readEvent.GetType().Should().Be(typeof(TestEvent));
            (readEvent as TestEvent).state.Should().Be(testEvent.state);
        }

        [Test]
        public async Task WhenReadingUnknownEventType_ShouldReturnInvalidNullDomainEvent()
        {
            var testEvent = new TestEvent() { state = "test" };

            sqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ReceiveMessageResponse()
                     {
                         HttpStatusCode = System.Net.HttpStatusCode.OK,
                         Messages = new List<Message> {
                             new Message {
                                 Body = JsonConvert.SerializeObject(testEvent),
                                 MessageAttributes = new Dictionary<string, MessageAttributeValue>
                                 {
                                     { "EventType", new MessageAttributeValue() { StringValue = "Unknown", DataType = "String" }}
                                 }
                             }
                         }
                     });

            var readEvents = await SUT(awsEventSource).ReadEvents();

            readEvents.Count().Should().Be(1);

            var readEvent = readEvents.First().DomainEvent;
            readEvent.Should().BeNull();
        }

        [Test]
        public void WhenErrorReadingEvent_ShouldThowReadingErrorException()
        {
            sqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default)).ReturnsAsync(new ReceiveMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
            });

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventSource).ReadEvents());
            var expectedReason = new Exception($"Read SQS Http response code {System.Net.HttpStatusCode.InternalServerError}");
            ex.Message.Should().StartWith($"Error reading from SQS {queueUrl}, " +
                                          $"error: { expectedReason }");
        }

        [Test]
        public async Task WhenDeleting_ShouldDeleteEventFromSqsQueue()
        {
            var testEvent = new TestEvent() { state = "test" };
            var sqsEvent = new SQSEvent(testEvent, new Message() { ReceiptHandle = "eventHandle" });

            await SUT(awsEventSource).DeleteEvent(sqsEvent);

            sqsClient.Verify(x => x.DeleteMessageAsync("sqsURL", "eventHandle", default));
        }

        [Test]
        public void WhenErrorDeletingEvent_ShouldThowDeletingErrorException()
        {
            var testEvent = new TestEvent() { state = "test" };
            var sqsEvent = new SQSEvent(testEvent, new Message() { ReceiptHandle = "eventHandle" });
            sqsClient.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                     .ReturnsAsync(new DeleteMessageResponse()
                     {
                         HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
                     });

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT(awsEventSource).DeleteEvent(sqsEvent));
            var expectedReason = new Exception($"Delete SQS Http response code {System.Net.HttpStatusCode.InternalServerError}");
            ex.Message.Should().StartWith($"Error deleting event \"{sqsEvent.DomainEvent.ToJson()}\" from SQS {queueUrl}, " +
                                          $"error: { expectedReason }");
        }

        [Test]
        public async Task WhenReadingEventStream_ShouldReturnEventsFromSqsQueue()
        {
            sqsClient.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(new ReceiveMessageResponse()
                     {
                         HttpStatusCode = System.Net.HttpStatusCode.OK,
                         Messages = new List<Message> {
                             new Message {
                                 Body = (new TestEvent() { state = "1" }).ToJson(),
                                 MessageAttributes = new Dictionary<string, MessageAttributeValue>
                                 {
                                     { "EventType", new MessageAttributeValue() { StringValue = "TestEvent", DataType = "String" }}
                                 } },
                             new Message {
                                 Body = (new TestEvent() { state = "2" }).ToJson(),
                                 MessageAttributes = new Dictionary<string, MessageAttributeValue>
                                 {
                                     { "EventType", new MessageAttributeValue() { StringValue = "TestEvent", DataType = "String" }}
                                 } }
                         }
                     }));

            var msgSeq = 1;
            await foreach (var @event in SUT(awsEventSource).ReadEventStream(default))
            {
                (@event.DomainEvent as TestEvent).state.Should().Be(msgSeq.ToString());
                msgSeq++;
            }
        }

        [Test]
        public async Task WhenNoEventsInQueue_ShouldTakeBreakBeforeContinuingToResumeReadingQueue()
        {
            var sqsService = SQSServicePartialMock(awsEventSource);
            await foreach (var message in sqsService.Object.ReadEventStream(default)) ;

            sqsService.Verify(x => x.TakeBreakBeforePollingForEvents(It.IsAny<CancellationToken>()), Moq.Times.Once);
        }
    }
}