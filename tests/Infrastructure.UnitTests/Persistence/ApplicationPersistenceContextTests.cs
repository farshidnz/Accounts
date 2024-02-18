using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Common;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.Persistence;
using Accounts.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.UnitTests.Persistence
{
    [TestFixture]
    public class ApplicationPersistenceContextTests
    {
        private Mock<IDomainEntityPersistenceContext<string, TestEntity>> entityPersistenceContext;
        private Mock<IEventOutboxPersistenceContext> eventOutboxPersistence;
        private EventOutbox eventOutbox;
        private Mock<IDomainEventService> domainEventService;
        private Mock<IAccountsPersistanceContext<string, TestEntity>> persistanceContext;
        private Mock<ILogger<ApplicationPersistenceContext<string, TestEntity>>> logger;

        private TestEntity domainEntity = new TestEntity("test id");

        [SetUp]
        public void Setup()
        {
            this.entityPersistenceContext = new Mock<IDomainEntityPersistenceContext<string, TestEntity>>();

            this.eventOutboxPersistence = new Mock<IEventOutboxPersistenceContext>();
            eventOutbox = new EventOutbox();
            eventOutboxPersistence.Setup(x => x.Get()).ReturnsAsync(eventOutbox);

            this.domainEventService = new Mock<IDomainEventService>();
            persistanceContext = new Mock<IAccountsPersistanceContext<string, TestEntity>>();
            persistanceContext.Setup(x => x.AddOrUpdate(new TestEntity("test id"))).Returns(Task.CompletedTask);
            this.logger = new Mock<ILogger<ApplicationPersistenceContext<string, TestEntity>>>();
        }

        private ApplicationPersistenceContext<string, TestEntity> SUT()
        {
            return new ApplicationPersistenceContext<string, TestEntity>(entityPersistenceContext.Object,
                                                                               eventOutboxPersistence.Object,
                                                                               domainEventService.Object,
                                                                               logger.Object,
                                                                               persistanceContext.Object);
        }

        //[TestCase(PersistOperation.AddOrUpdate)]
        //[TestCase(PersistOperation.Remove)]
        //public async Task WhenSavingEntity_ShouldPersistEntityAndItsDomainEventsToOutbox_AndTriggerOutboxEventPublishing(PersistOperation persistOperation)
        //{
        //    switch (persistOperation)
        //    {
        //        case PersistOperation.Remove:
        //            domainEntity.DoSomethingThatRaisesEvent("deleted");
        //            await SUT().Remove(domainEntity);
        //            entityPersistenceContext.Verify(x => x.Remove(domainEntity));
        //            eventOutboxPersistence.Verify(x => x.Save(It.Is<EventOutbox>(e =>
        //                                                                (e.Peek() as TestEvent).state == "deleted")));
        //            break;

        //        case PersistOperation.AddOrUpdate:
        //        default:
        //            domainEntity.DoSomethingThatRaisesEvent("updated");
        //            await SUT().Save(domainEntity);
        //            persistanceContext.Verify(x => x.AddOrUpdate(domainEntity));
        //            eventOutboxPersistence.Verify(x => x.Save(It.Is<EventOutbox>(e =>
        //                                                                (e.Peek() as TestEvent).state == "updated")));
        //            break;
        //    }

        //    domainEventService.Verify(x => x.PublishEventOutbox());
        //}

        //[Test]
        //public async Task WhenSavingEntity_ShouldAssignSameCorrelationID_ToAllEventsRaisedAsPartOfUpdate()
        //{
        //    var correlationID = Guid.NewGuid();

        //    var domainEntity = new TestEntity("test id");
        //    domainEntity.DoSomethingThatRaisesEvent("updated");
        //    domainEntity.DoSomethingThatRaisesEvent("deleted");

        //    await SUT().Save(domainEntity, correlationID);

        //    eventOutboxPersistence.Verify(x => x.Save(It.Is<EventOutbox>(outbox => CheckAllEventsHaveSameCorrelationID(outbox, correlationID))));
        //}

        //private bool CheckAllEventsHaveSameCorrelationID(Queue<DomainEvent> events, Guid correlationID)
        //{
        //    foreach (var @event in events)
        //    {
        //        var eventCorrelationID = @event.Metadata.CorrelationID;
        //        if (eventCorrelationID != correlationID || eventCorrelationID == default)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        //[Test]
        //public void WhenErrorInSavingEntity_ShouldNotAttemptToPublishOutbox()
        //{
        //    var domainEntity = new TestEntity("test id");
        //    domainEntity.DoSomethingThatRaisesEvent("updated");

        //    eventOutboxPersistence.Setup(x => x.Save(It.IsAny<EventOutbox>()))
        //                          .Throws(new Exception("some err within transaction scope of saving outbox"));

        //    var ex = Assert.ThrowsAsync<Exception>(async () => await SUT().Save(domainEntity));
        //    ex.Message.Should().Be("some err within transaction scope of saving outbox");

        //    domainEventService.Verify(x => x.PublishEventOutbox(), Times.Never);
        //}
    }
}