using FluentAssertions;
using MassTransit.Mediator;
using Accounts.Application.Common.Models;
using Accounts.Domain.Common;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Extensions
{
    [TestFixture]
    public class MediatorExtensionsTests
    {
        private Mock<IMediator> mediator;

        [SetUp]
        public void Setup()
        {
            this.mediator = new Mock<IMediator>();
        }

        private IMediator SUT()
        {
            return mediator.Object;
        }

        [Test]
        public async Task GivenInternalEventPublish_AfterReadingFromSQS_ShouldPopulateEventMetadata()
        {
            var domainEvent = new TestEvent();
            var eventID = Guid.NewGuid();
            var raisedDateTime = DateTimeOffset.UtcNow;
            var publishedDateTime = DateTimeOffset.UtcNow;
            var correlationID = Guid.NewGuid();
            ulong seqNum = 9876543210;
            var sqsMessage = new Amazon.SQS.Model.Message()
            {
                MessageAttributes = new Dictionary<string, Amazon.SQS.Model.MessageAttributeValue> {
                    { "EventID"                  , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = eventID.ToString() } },
                    { "EventType"                , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = "TestEvent"} },
                    { "EventSource"              , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = "Accounts"} },
                    { "Domain"                   , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = "Test"} },
                    { "CorrelationID"            , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = correlationID.ToString()} },
                    { "EventRaisedDateTimeUTC"   , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = raisedDateTime.ToString("o")} },
                    { "EventPublishedDateTimeUTC", new Amazon.SQS.Model.MessageAttributeValue() { StringValue = publishedDateTime.ToString("o")} },
                    { "ContextualSequenceNumber" , new Amazon.SQS.Model.MessageAttributeValue() { StringValue = seqNum.ToString() } },
                }
            };

            await SUT().PublishEvent(domainEvent, sqsMessage);

            mediator.Verify(x => x.Publish(It.Is<object>(x => hasExpectedMetadata(x, m => m.EventType == "TestEvent" &&
                                                                                          m.EventSource == "Accounts" &&
                                                                                          EventMetadata.Domain == "Member" &&
                                                                                          m.EventID == eventID &&
                                                                                          m.CorrelationID == correlationID &&
                                                                                          m.RaisedDateTimeUTC == raisedDateTime &&
                                                                                          m.PublishedDateTimeUTC == publishedDateTime &&
                                                                                          m.ContextualSequenceNumber == seqNum)), default));
        }

        [Test]
        public async Task GivenInternalEventPublish_AfterLambdaTriggerFromSQS_ShouldPopulateEventMetadata()
        {
            var domainEvent = new TestEvent();
            var eventID = Guid.NewGuid();
            var raisedDateTime = DateTimeOffset.UtcNow;
            var publishedDateTime = DateTimeOffset.UtcNow;
            var correlationID = Guid.NewGuid();
            ulong seqNum = 9876543210;
            var sqsMessage = new Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage
            {
                MessageAttributes = new Dictionary<string, Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute> {
                    { "EventID"                  , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = eventID.ToString() } },
                    { "EventType"                , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = "TestEvent"} },
                    { "EventSource"              , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = "Accounts"} },
                    { "Domain"                   , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = "Test"} },
                    { "CorrelationID"            , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = correlationID.ToString()} },
                    { "EventRaisedDateTimeUTC"   , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = raisedDateTime.ToString("o")} },
                    { "EventPublishedDateTimeUTC", new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = publishedDateTime.ToString("o")} },
                    { "ContextualSequenceNumber" , new Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute() { StringValue = seqNum.ToString() } },
                }
            };

            await SUT().PublishEvent(domainEvent, sqsMessage);

            mediator.Verify(x => x.Publish(It.Is<object>(x => hasExpectedMetadata(x, m => m.EventType == "TestEvent" &&
                                                                                          m.EventSource == "Accounts" &&
                                                                                          EventMetadata.Domain == "Member" &&
                                                                                          m.EventID == eventID &&
                                                                                          m.CorrelationID == correlationID &&
                                                                                          m.RaisedDateTimeUTC == raisedDateTime &&
                                                                                          m.PublishedDateTimeUTC == publishedDateTime &&
                                                                                          m.ContextualSequenceNumber == seqNum)), default));
        }

        private bool hasExpectedMetadata(object notification, Func<EventMetadata, bool> comparator)
        {
            var domainEvent = (notification as DomainEventNotification<TestEvent>).DomainEvent;
            return comparator(domainEvent.Metadata);
        }
    }
}