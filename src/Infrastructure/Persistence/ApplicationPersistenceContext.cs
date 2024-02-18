using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Common;
using Accounts.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public enum PersistOperation
{
    AddOrUpdate,
    Remove
};

public class ApplicationPersistenceContext<Key, Entity> : IApplicationPersistenceContext<Key, Entity>
    where Entity : DomainEntity, IHasIdentity<Key>
{
    private readonly IAccountsPersistanceContext<Key, Entity> _accountsContext;
    private readonly IDomainEntityPersistenceContext<Key, Entity> entityPersistenceContext;
    private readonly IEventOutboxPersistenceContext eventOutboxPersistenceContext;
    private readonly IDomainEventService domainEventService;
    private readonly ILogger<ApplicationPersistenceContext<Key, Entity>> logger;

    public ApplicationPersistenceContext(IDomainEntityPersistenceContext<Key, Entity> domainEntityPersistenceContext,
                                      IEventOutboxPersistenceContext eventOutboxPersistenceContext,
                                      IDomainEventService domainEventService,
                                      ILogger<ApplicationPersistenceContext<Key, Entity>> logger,
                                      IAccountsPersistanceContext<Key, Entity> accountsContext)
    {
        this.entityPersistenceContext = domainEntityPersistenceContext;
        this.eventOutboxPersistenceContext = eventOutboxPersistenceContext;
        this.domainEventService = domainEventService;
        this.logger = logger;
        _accountsContext = accountsContext;
    }

    public async Task<Entity> Get(Key key)
    {
        return await entityPersistenceContext.Get(key);
    }

    public async Task Save(Entity domainEntity, Guid? correlationID = default)
    {
        await Persist(domainEntity, PersistOperation.AddOrUpdate, correlationID);
    }

    public async Task Remove(Entity domainEntity, Guid? correlationID = default)
    {
        await Persist(domainEntity, PersistOperation.Remove, correlationID);
    }

    public async Task Persist(Entity domainEntity, PersistOperation persistOperation = PersistOperation.AddOrUpdate, Guid? correlationID = default)
    {
        await PersistEntity(domainEntity, persistOperation);
    }

    private async Task PersistEntity(Entity domainEntity, PersistOperation persistOperation)
    {
        switch (persistOperation)
        {
            case PersistOperation.Remove:
                await entityPersistenceContext.Remove(domainEntity);
                break;

            case PersistOperation.AddOrUpdate:
            default:
                await _accountsContext.AddOrUpdate(domainEntity);
                break;
        }
    }
}