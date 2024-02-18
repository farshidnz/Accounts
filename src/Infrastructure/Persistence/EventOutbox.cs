using Accounts.Domain.Common;
using System.Collections.Generic;

namespace Accounts.Infrastructure.Persistence
{
    public class EventOutbox : Queue<DomainEvent>
    {
        public EventOutbox()
        {
        }

        public EventOutbox(IEnumerable<DomainEvent> collection) : base(collection)
        {
        }

        public void Append(Queue<DomainEvent> domainEvents)
        {
            while (domainEvents.Count > 0)
            {
                Enqueue(domainEvents.Dequeue());
            }
        }
    }
}