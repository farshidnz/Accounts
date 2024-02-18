using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public interface IEventOutboxPersistenceContext
{
    Task<EventOutbox> Get();

    Task Save(EventOutbox eventOutbox);
}