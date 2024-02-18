using System;
using System.Collections.Generic;

namespace Accounts.Application.Common.Models
{
    public class DomainEventNotification<TDomainEvent>
    {
        public DomainEventNotification(TDomainEvent domainEvent, IMessageMetadata messageMetadata)
        {
            DomainEvent = domainEvent;
            MessageMetadata = messageMetadata;
        }

        public TDomainEvent DomainEvent { get; }
        public IMessageMetadata MessageMetadata { get; }
    }

    public class MessageAttribute
    {
        public string StringValue { get; set; }

        public Guid ToGuid()
        {
            Guid.TryParse(StringValue, out Guid result);
            return result;
        }

        public DateTimeOffset ToDateTimeOffset()
        {
            DateTimeOffset.TryParse(StringValue, out DateTimeOffset result);
            return result;
        }

        public ulong ToUlong()
        {
            ulong.TryParse(StringValue, out ulong result);
            return result;
        }
    }

    public interface IMessageMetadata
    {
        public string MessageId { get; set; }

        public string ReceiptHandle { get; set; }

        public string Body { get; set; }

        public string Md5OfBody { get; set; }

        public string Md5OfMessageAttributes { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public Dictionary<string, MessageAttribute> MessageAttributes { get; set; }
    }
}