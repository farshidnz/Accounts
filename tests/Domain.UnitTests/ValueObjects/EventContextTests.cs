using FluentAssertions;
using Accounts.Domain.ValueObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class EventContextTests
    {
        private Guid personId = Guid.NewGuid();
        private int transactionId = 987654;

        public EventContext SUT(List<object> propertyValues)
        {
            return new EventContext(propertyValues);
        }

        [Test]
        public void GivenSetOfPropertyValues_ContextToStringShouldListSetOfValues()
        {
            var eventContext = SUT(new List<object>()
            {
                "TestEvent",
                personId,
                transactionId
            });

            eventContext.ToString().Should().Be($"[TestEvent, {personId}, {transactionId}]");
        }

        [Test]
        public void GivenSameSetOfPropertyValues_EventContextShouldBeEqual()
        {
            var eventContext1 = SUT(new List<object>()
            {
                "TestEvent",
                personId,
                transactionId
            });
            var eventContext2 = SUT(new List<object>()
            {
                transactionId,
                personId,
                "TestEvent",
            });

            eventContext1.GetHashCode().Should().Be(eventContext2.GetHashCode());
            eventContext1.Equals(eventContext2).Should().BeTrue();
        }

        [Test]
        public void GivenDifferentSetOfPropertyValues_EventContextShouldNotBeEqual()
        {
            var eventContext1 = SUT(new List<object>()
            {
                "TestEvent",
                Guid.NewGuid(),
                transactionId
            });
            var eventContext2 = SUT(new List<object>()
            {
                "TestEvent",
                Guid.NewGuid(),
                transactionId,
            });

            eventContext1.GetHashCode().Should().NotBe(eventContext2.GetHashCode());
            eventContext1.Equals(eventContext2).Should().BeFalse();
        }
    }
}