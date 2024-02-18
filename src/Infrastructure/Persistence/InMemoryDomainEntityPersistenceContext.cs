using Accounts.Domain.Common;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
namespace Accounts.Infrastructure.Persistence
{
    public class InMemoryDomainEntityPersistenceContext<Key, Entity> : IDomainEntityPersistenceContext<Key, Entity>
         where Entity : DomainEntity, IHasIdentity<Key>
    {
        private readonly IMemoryCache memoryCache;

        public InMemoryDomainEntityPersistenceContext(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public async Task<Entity> Get(Key ID)
        {
            memoryCache.TryGetValue(ID, out Entity entity);
            return entity;
        }

        public async Task Save(Entity entity) => memoryCache.Set(entity.ID, entity);

        public async Task Remove(Entity entity) => memoryCache.Remove(entity.ID);
    }
}