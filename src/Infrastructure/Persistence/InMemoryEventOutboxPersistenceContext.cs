using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public class InMemoryEventOutboxPersistenceContext : IEventOutboxPersistenceContext
{
    private readonly IMemoryCache memoryCache;

    public InMemoryEventOutboxPersistenceContext(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

    public async Task<EventOutbox> Get()
    {
        memoryCache.TryGetValue("EventOutbox", out EventOutbox outbox);
        return outbox ?? new EventOutbox();
    }

    public async Task Save(EventOutbox eventOutbox) => memoryCache.Set("EventOutbox", eventOutbox);
}