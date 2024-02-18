using Moq;
using NUnit.Framework;
using Accounts.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Accounts.Infrastructure.Persistence;
using MassTransit.Mediator;
using Accounts.Application.Common.Models;
using Accounts.Infrastructure.AWS;
using Accounts.Domain.UnitTests.Common;

namespace Accounts.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class DomainEventServiceTests
    {
        private Mock<ILogger<DomainEventService>> logger;
        private Mock<IEventOutboxPersistenceContext> eventOutboxPersistence;
        private Mock<IMediator> mediator;
        private Mock<IAWSEventServiceFactory> eventServiceFactory;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<DomainEventService>>();

            this.eventOutboxPersistence = new Mock<IEventOutboxPersistenceContext>();

            this.mediator = new Mock<IMediator>();

            this.eventServiceFactory = new Mock<IAWSEventServiceFactory>();
        }

        private DomainEventService SUT()
        {
            return new DomainEventService(eventOutboxPersistence.Object, logger.Object, mediator.Object, eventServiceFactory.Object);
        }

        [Test]
        public async Task PublishEventOutbox_ShouldPublishPendingOutgoingEvents_AndBeRomovedFromOutbox()
        {
            var testEvent = new TestEvent() { state = "1" };
            var events = new List<TestEvent>() { testEvent };
            var eventOutbox = new EventOutbox(events);
            eventOutboxPersistence.Setup(x => x.Get()).ReturnsAsync(eventOutbox);

            var externalSNSPublisher = new Mock<IAWSEventService>();
            var externalSQSPublisher = new Mock<IAWSEventService>();
            eventServiceFactory.Setup(x => x.GetAWSPublishersForEvent(testEvent)).Returns(new List<IAWSEventService>
            {
                externalSNSPublisher.Object, externalSQSPublisher.Object
            });

            await SUT().PublishEventOutbox();

            mediator.Verify(x => x.Publish(It.Is<object>(x => isDomainEventNotifaction(x, testEvent)), default));
            externalSNSPublisher.Verify(x => x.Publish(It.Is<TestEvent>(x => x.state == testEvent.state)));
            externalSQSPublisher.Verify(x => x.Publish(It.Is<TestEvent>(x => x.state == testEvent.state)));
            eventOutboxPersistence.Verify(x => x.Save(It.Is<EventOutbox>(outbox => outbox.Count == 0)));
        }

        [Test]
        public async Task PublishEventOutbox_WhenExceptionInPublishingEvent_ShouldNotRemoveEventFromOutbox()
        {
            var testEvent = new TestEvent() { state = "1" };
            var events = new List<TestEvent>() { testEvent };
            var eventOutbox = new EventOutbox(events);
            eventOutboxPersistence.Setup(x => x.Get()).ReturnsAsync(eventOutbox);

            var externalSNSPublisher = new Mock<IAWSEventService>();
            externalSNSPublisher.Setup(x => x.Publish(testEvent)).Throws(new System.Exception("error publishing"));
            eventServiceFactory.Setup(x => x.GetAWSPublishersForEvent(testEvent)).Returns(new List<IAWSEventService>
            {
                externalSNSPublisher.Object,
            });

            await SUT().PublishEventOutbox();

            eventOutboxPersistence.Verify(x => x.Save(It.Is<EventOutbox>(outbox => outbox.Count == 0)), Times.Never);
        }

        private bool isDomainEventNotifaction(object notification, TestEvent expected)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return domainEvent.state == expected.state;
        }
    }
}