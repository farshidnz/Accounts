using Accounts.Application.Common.Models;
using Accounts.Domain.Common;
using System.Collections.Generic;

namespace Accounts.Infrastructure.AWS
{
    public class AWSMessageMetadata : IMessageMetadata
    {
        public string MessageId { get; set; }

        public string ReceiptHandle { get; set; }

        public string Body { get; set; }

        public string Md5OfBody { get; set; }

        public string Md5OfMessageAttributes { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public Dictionary<string, MessageAttribute> MessageAttributes { get; set; }

        public EventMetadata GetEventMetadata()
        {
            return new()
            {
                EventID = GetMessageAttribute(EventMessageAttributes.EventID).ToGuid(),
                EventType = GetMessageAttribute(EventMessageAttributes.EventType).StringValue,
                EventSource = GetMessageAttribute(EventMessageAttributes.EventSource).StringValue,
                CorrelationID = GetMessageAttribute(EventMessageAttributes.CorrelationID).ToGuid(),
                RaisedDateTimeUTC = GetMessageAttribute(EventMessageAttributes.EventRaisedDateTimeUTC).ToDateTimeOffset(),
                PublishedDateTimeUTC = GetMessageAttribute(EventMessageAttributes.EventPublishedDateTimeUTC).ToDateTimeOffset(),
                ContextualSequenceNumber = GetMessageAttribute(EventMessageAttributes.ContextualSequenceNumber).ToUlong(),
            };
        }

        private MessageAttribute GetMessageAttribute(EventMessageAttributes attribute)
        {
            MessageAttributes.TryGetValue(attribute.ToString(), out MessageAttribute messageAttribute);
            return messageAttribute ?? new();
        }
    }
}