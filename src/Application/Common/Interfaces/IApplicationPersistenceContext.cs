using Accounts.Domain.Common;
using System;
using System.Threading.Tasks;

namespace Accounts.Application.Common.Interfaces
{
    public interface IApplicationPersistenceContext<Key, Entity>  where Entity : IHasIdentity<Key>
    {
        public Task<Entity> Get(Key key);

        public Task Save(Entity value, Guid? correlationID = default);

        public Task Remove(Entity value, Guid? correlationID = default);
    }
}