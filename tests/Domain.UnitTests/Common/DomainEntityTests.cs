using FluentAssertions;
using Accounts.Domain.Attributes;
using Accounts.Domain.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Accounts.Domain.UnitTests.Common
{
    [TestFixture]
    public class DomainEntityTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public TestEntity SUT()
        {
            return new TestEntity("testId");
        }

        [Test]
        public void WhenRaisingEvent_ShouldSetEventMetadata()
        {
            var entity = SUT();

            var approximateRaiseTime = DateTimeOffset.UtcNow;
            entity.RaiseEvent(new TestEvent());

            var eventMetadata = entity.DomainEvents.Peek().Metadata;
            eventMetadata.EventID.Should().NotBe(Guid.Empty);
            eventMetadata.EventType.Should().Be(typeof(TestEvent).Name);
            eventMetadata.EventSource.Should().Be("Accounts");
            EventMetadata.Domain.Should().Be("Member");
            eventMetadata.RaisedDateTimeUTC.Should().BeOnOrAfter(approximateRaiseTime);
        }

        [TestCase(3)]
        public void WhenRaisingEvent_ShouldSetEventSequenceNumber_ToNextAvailable(int numOfEventsRaised)
        {
            var entity = SUT();

            for (var i = 0; i < numOfEventsRaised; i++)
            {
                entity.RaiseEvent(new TestEvent());
                ulong expectedSeqNum = (ulong)(i + 1);
                entity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(expectedSeqNum);
            }
        }

        [EventContextName("SomeBusinessContextForEvent")]
        private class ComplexEvent : DomainEvent
        {
            [EventContextProvider]
            public string Campaign { get; set; }

            [EventContextProvider]
            public Guid memberID { get; set; }

            [EventContextProvider]
            public int transaction { get; set; }

            public string state { get; set; }
        }

        [Test]
        public void WhenRaisingEventsWithSameEventContext_ShouldIncrementEventSequenceNumberForTheSameEventContext()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent()
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654,
                state = "created"
            };
            var evnt2 = new ComplexEvent()
            {
                transaction = 987654,
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);
        }

        [Test]
        public void WhenRaisingEventsWithDifferentEventContext_ShouldKeepEventSequenceNumberSeparateForDifferentEventContexts()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent()
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654, // different
                state = "created"
            };
            var evnt2 = new ComplexEvent()
            {
                transaction = 987321, // different
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);
        }

        [EventContextName("SomeBusinessContextForEvent")]
        private class DifferentComplexEvent : DomainEvent
        {
            [EventContextProvider]
            public string Campaign { get; set; }

            [EventContextProvider]
            public Guid memberID { get; set; }

            [EventContextProvider]
            public int transaction { get; set; }

            public string state { get; set; }
        }

        [Test]
        public void WhenRaisingDifferentEventsButForSameEventContext_ShouldIncrementEventSequenceNumberForTheSameEventContext()
        {
            Guid guid = Guid.NewGuid();
            var evnt1 = new ComplexEvent()
            {
                Campaign = "abcd",
                memberID = guid,
                transaction = 987654,
                state = "created"
            };
            var evnt2 = new DifferentComplexEvent()
            {
                transaction = 987654,
                memberID = guid,
                Campaign = "abcd",
                state = "updated"
            };

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);
        }

        private class SimpleEvent : DomainEvent
        { }

        private class AnotherSimpleEvent : DomainEvent
        { }

        [Test]
        public void WhenRaisingEventsWithUndefinedContextName_ShouldUseEventTypeNameToIdentifyContext()
        {
            var evnt1 = new SimpleEvent();
            var evnt2 = new SimpleEvent();
            var evnt3 = new AnotherSimpleEvent();

            var testEntity = SUT();

            testEntity.RaiseEvent(evnt1);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);

            testEntity.RaiseEvent(evnt2);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(2);

            testEntity.RaiseEvent(evnt3);
            testEntity.DomainEvents.Dequeue().Metadata.ContextualSequenceNumber.Should().Be(1);
        }
    }
}