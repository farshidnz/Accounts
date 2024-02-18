using Accounts.Application.Common.Models;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.AWS;
using FluentAssertions;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.AWS
{
    [TestFixture]
    public class LambdaEventTriggerHandlerTests
    {
        private Mock<IAWSEventService> eventService;
        private Mock<IAWSEventServiceFactory> eventServiceFactory;
        private Mock<ILogger<LambdaEventTriggerHandler>> logger;
        private Mock<IMediator> mediator;
        private Mock<IServiceCollection> serviceCollection;

        [SetUp]
        public void Setup()
        {
            eventService = new Mock<IAWSEventService>();
            eventService.Setup(x => x.EventTypes).Returns(new List<Type> { typeof(TestEvent) });

            eventServiceFactory = new Mock<IAWSEventServiceFactory>();
            eventServiceFactory.Setup(x => x.GetAWSEventReaders(AwsEventReadMode.LambdaTrigger)).Returns(new List<IAWSEventService> { eventService.Object });

            logger = new Mock<ILogger<LambdaEventTriggerHandler>>();
            mediator = new Mock<IMediator>();

            serviceCollection = new Mock<IServiceCollection>();
        }

        public LambdaEventTriggerHandler SUT()
        {
            var sut = new Mock<LambdaEventTriggerHandler>(serviceCollection.Object)
            {
                CallBase = true
            };
            sut.Setup(x => x.BuildServiceProvider()).Returns(null as ServiceProvider);
            sut.Setup(x => x.GetEventServiceFactory(It.IsAny<ServiceProvider>())).Returns(eventServiceFactory.Object);
            sut.Setup(x => x.GetMediator(It.IsAny<ServiceProvider>())).Returns(mediator.Object);
            sut.Setup(x => x.GetLogger(It.IsAny<ServiceProvider>())).Returns(logger.Object);

            return sut.Object;
        }

        [Test]
        public void WhenTriggeringWithoutEventTypeAttribute_ShouldThrowInvalidOperationException()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await SUT().ReadEvents(GetTestSQSEvent(eventType: null), null));
            ex.Message.Should().Be("Invalid event message, missing required Message Attribute \"EventType\"");
        }

        [Test]
        public void WhenTriggeringWithUnknownEventType_ShouldThrowUnsupportedException()
        {
            var ex = Assert.ThrowsAsync<NotSupportedException>(async () => await SUT().ReadEvents(GetTestSQSEvent("UnknownEventType"), null));
            ex.Message.Should().Be("Unsupported event type: UnknownEventType");
        }

        [Test]
        public async Task WhenTriggeringWithKnownEventType_ShouldPublishEventToInternalConsumers()
        {
            var testEvent = new TestEvent() { state = "abc" };

            await SUT().ReadEvents(GetTestSQSEvent("TestEvent", testEvent.ToJson()), null);

            mediator.Verify(x => x.Publish(It.Is<object>(x => isDomainEventNotifaction(x, testEvent)), default));
        }

        [Test]
        public void WhenTriggeringWithKnownEventType_AndErroInPublishingToInternalHandlers_ShouldThrowException()
        {
            var exception = new Exception("error publishing");
            mediator.Setup(x => x.Publish(It.IsAny<object>(), default)).Throws(exception);

            var testEvent = new TestEvent() { state = "abc" };
            testEvent.Metadata.EventType = "TestEvent";
            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT().ReadEvents(GetTestSQSEvent("TestEvent", testEvent.ToJson()), null));
        }

        private bool isDomainEventNotifaction(object notification, TestEvent expected)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return domainEvent.state == expected.state;
        }

        private Amazon.Lambda.SQSEvents.SQSEvent GetTestSQSEvent(string eventType, string eventJson = null)
        {
            var attributes = new Dictionary<string, Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute>()
            {
            };
            if (eventType != null)
            {
                attributes.Add("EventType", new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute()
                {
                    DataType = "StringValue",
                    StringValue = eventType
                });
            }

            return new Amazon.Lambda.SQSEvents.SQSEvent()
            {
                Records = new List<Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage>
                {
                    new Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage
                    {
                        Body = eventJson,
                        MessageAttributes = attributes
                    }
                }
            };
        }
    }
}