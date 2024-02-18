using Accounts.Domain.Common;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public interface IOutboxRepository
{
    Task<int> SaveAndPublish<T>(string sql, T @event, object parameters, int? timeout = null) where T : DomainEntity;
}