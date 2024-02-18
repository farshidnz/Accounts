using Accounts.Domain.Common;
using Accounts.Infrastructure.OutboxMessages;
using Dapper;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class CapOutboxRepository : IOutboxRepository
{
    private readonly IAccountsDbContext _accountsDbContext;
    private readonly ICapPublisher _capBus;
    private readonly IConfiguration _configuration; // To be refactor to use Unleash instead of Environment Variable

    public CapOutboxRepository(IAccountsDbContext accountsDbContext, ICapPublisher capBus, IConfiguration configuration)
    {
        _accountsDbContext = accountsDbContext;
        _capBus = capBus;
        _configuration = configuration;
    }

    /// <summary>
    /// Saves the query in the database, and in the Cap table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="event"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<int> SaveAndPublish<T>(string sql, T @event, object parameters = null, int? timeout = null) where T : DomainEntity
    {
        ValidateEvent(@event);
        int id = 0;
        await using var connection = _accountsDbContext.CreateConnection();
        using var transaction = connection.BeginTransaction(_capBus);
        bool switchToPostgresDB = bool.TryParse(_configuration["SwitchToPostgresDB"], out var b) && b;

        if (switchToPostgresDB)
            id = await connection.ExecuteAsync(sql, parameters, null, timeout);

        if (@event.HasDomainEvents)
        {
            DomainEvent domainEvent = @event.DomainEvents.Dequeue();
            var outboxMessage = new OutboxMessage
            {
                CorrelationId = Guid.NewGuid().ToString(),
                MessageType = @event.MessageType,
                Message = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                   
                })
            };

            // https://github.com/dotnetcore/CAP/blob/20ff7d95315436a88ab2c0d0bf8ccb2fcaa27a8a/docs/content/user-guide/zh/transport/azure-service-bus.md
            // To guarantee FIFO
            var headers = new Dictionary<string, string>();
            await _capBus.PublishAsync(nameof(OutboxHandlers), outboxMessage, headers!);
        }
        await transaction.CommitAsync();
        return id;
    }

    private static void ValidateEvent<T>(T @event)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }
    }
}