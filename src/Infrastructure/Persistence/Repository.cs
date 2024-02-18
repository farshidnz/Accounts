using Accounts.Domain.Common;
using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public interface IRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, int? timeOut = null);

    Task ExecuteAsync(string sql, object parameters = null, int? timeOut = null);
}

public class Repository : IRepository
{
    private readonly IAccountsDbContext _accountsDbContext;

    private readonly IOutboxRepository _outboxRepository;

    public Repository(IAccountsDbContext accountsDbContext, IOutboxRepository outboxRepository)
    {
        _accountsDbContext = accountsDbContext;
        _outboxRepository = outboxRepository;
    }

    protected virtual NpgsqlConnection CreateConnection() => _accountsDbContext.CreateConnection();

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, int? timeOut = null)
    {
        await using var connection = CreateConnection();

        return await connection.QueryAsync<T>(sql, parameters, null, timeOut);
    }

    public async Task ExecuteAsync(string sql, object parameters = null, int? timeOut = null)
    {
        await _outboxRepository.SaveAndPublish(sql, parameters as DomainEntity, parameters);
    }
}