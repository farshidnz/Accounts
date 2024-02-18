namespace Accounts.Infrastructure.OutboxMessages;

using Accounts.Domain.Common;

public class OutboxMessage
{
    public EventMetadata Metadata { get; set; } = new EventMetadata();
    public string MessageType { get; set; } = default!;
    public string CorrelationId { get; set; } = default!;
    public string Message { get; set; } = default!;
}