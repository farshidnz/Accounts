using Accounts.Domain.Common;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public interface IDomainEntityPersistenceContext<Key, Entity> where Entity : DomainEntity, IHasIdentity<Key>
{
    Task<Entity> Get(Key ID);

    Task Save(Entity entity);

    Task Remove(Entity entity);
}