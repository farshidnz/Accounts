using Amazon.SQS.Model;
using MassTransit.Mediator;
using Accounts.Application.Common.Models;
using Accounts.Domain.Common;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.BackgroundHostedService;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class EventPolledReadingServiceTests
    {
        private Mock<ILogger<EventPolledReadingService>> logger;
        private Mock<IMediator> mediator;
        private Mock<IAWSEventService> eventReader;
        private Mock<IAWSEventServiceFactory> eventServiceFactory;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<EventPolledReadingService>>();

            mediator = new Mock<IMediator>();

            eventReader = new Mock<IAWSEventService>();
            eventReader.Setup(x => x.EventTypes).Returns(new List<Type> { typeof(TestEvent) });
            eventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent());

            eventServiceFactory = new Mock<IAWSEventServiceFactory>();
            eventServiceFactory.Setup(x => x.GetAWSEventReaders(AwsEventReadMode.PolledRead)).Returns(new List<IAWSEventService>() { eventReader.Object });
        }

        public EventPolledReadingService SUT()
        {
            return new EventPolledReadingService(logger.Object, eventServiceFactory.Object, mediator.Object);
        }

        [Test]
        public async Task WhenNoEventPolledReaders_ShouldDoNothing()
        {
            eventServiceFactory.Setup(x => x.GetAWSEventReaders(AwsEventReadMode.PolledRead)).Returns(new List<IAWSEventService>());

            var service = SUT();

            await service.PolledReadEvents(default);

            eventReader.Verify(x => x.ReadEventStream(It.IsAny<CancellationToken>()), Moq.Times.Never);
        }

        [Test]
        public async Task WhenEventReadFromSQSqueue_ShouldPublishEventToConsumers_AndDeleteEventAfter()
        {
            var testEvent = new TestEvent() { state = "123" };
            eventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(testEvent));

            await SUT().PolledReadEvents(default);

            mediator.Verify(x => x.Publish(It.Is<object>(x => isDomainEventNotifaction(x, testEvent)), default));
            eventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.DomainEvent == testEvent)));
        }

        [Test]
        public async Task WhenEventReadFromSQSqueue_AndItsInvalidDomainEvent_ShouldNotPublishEventToConsumers_ButShouldRemoveTheUnknownEvent()
        {
            var unknownEventMessage = new Message() { MessageId = "unknownEventMessage" };
            eventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(domainEvent: null, unknownEventMessage));

            await SUT().PolledReadEvents(default);

            mediator.Verify(x => x.Publish(It.IsAny<object>(), default), Times.Never);
            eventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.Message == unknownEventMessage)));
        }

        [Test]
        public async Task WhenEventReadFromSQSqueue_AndErrorInPublishing_ShouldNotDeleteEvent()
        {
            var testEvent = new TestEvent() { state = "123" };
            eventReader.Setup(x => x.ReadEventStream(It.IsAny<CancellationToken>())).Returns(GetTestEvent(testEvent));
            mediator.Setup(x => x.Publish(It.IsAny<object>(), default)).Throws(new Exception());

            await SUT().PolledReadEvents(default);

            eventReader.Verify(x => x.DeleteEvent(It.Is<SQSEvent>(x => x.DomainEvent == testEvent)), Times.Never);
        }

        private bool isDomainEventNotifaction(object notification, TestEvent expected)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return domainEvent.state == expected.state;
        }

        private async IAsyncEnumerable<SQSEvent> GetTestEvent(DomainEvent domainEvent = null, Message message = null)
        {
            message = message ?? new Message()
            {
                MessageAttributes = new()
                {
                    { "EventType", new MessageAttributeValue() { StringValue = domainEvent?.GetType().Name } }
                }
            };

            var @event = new SQSEvent(domainEvent, message);

            yield return @event;

            await Task.CompletedTask;
        }
    }
}