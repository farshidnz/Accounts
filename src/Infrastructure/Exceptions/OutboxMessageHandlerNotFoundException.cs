using System;
using System.Runtime.Serialization;

namespace Accounts.Infrastructure.Exceptions;

[Serializable]
public class OutboxMessageHandlerNotFoundException : Exception
{
    public OutboxMessageHandlerNotFoundException(Exception innerException) : base(
        "Could not construct Rongo Outbox handler.", innerException)
    {
    }

    protected OutboxMessageHandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}