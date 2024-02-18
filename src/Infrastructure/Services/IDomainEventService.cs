using System.Threading.Tasks;

namespace Accounts.Infrastructure.Services
{
    public interface IDomainEventService
    {
        Task PublishEventOutbox();
    }
}