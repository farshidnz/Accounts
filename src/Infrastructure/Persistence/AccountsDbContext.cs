using Accounts.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Accounts.Infrastructure.Persistence;

public interface IAccountsDbContext
{
    NpgsqlConnection CreateConnection();

    NpgsqlConnection CreateDadminConnection();
}

public class AccountsDbContext : IAccountsDbContext
{
    public readonly DbConfiguration _dbConfiguration;
    private readonly string _DbUsername;
    private readonly string _DbPassword;

    public AccountsDbContext(DbConfiguration dbconfiguration,
            IConfiguration configuration
        )
    {
        _dbConfiguration = dbconfiguration;
        _DbUsername = configuration["PostgresDbUsername"].ToLower(); // it must be lower case
        _DbPassword = configuration["PostgresDbPassword"];
    }

    public NpgsqlConnection CreateConnection()
    {
        NpgsqlConnection conn = CreateConnection(_DbPassword, _dbConfiguration?.dbname, _dbConfiguration?.host, _dbConfiguration?.port.ToString(), _DbUsername);
        return conn;
    }

    public NpgsqlConnection CreateDadminConnection()
    {
        NpgsqlConnection conn = CreateConnection(_dbConfiguration?.password, _dbConfiguration?.dbname, _dbConfiguration?.host, _dbConfiguration?.port.ToString(), _dbConfiguration?.username);
        return conn;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="password"></param>
    /// <param name="databaseName"></param>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    private static NpgsqlConnection CreateConnection(
        string password,
        string databaseName,
        string host,
        string port,
        string userName)
    {
        NpgsqlConnection conn = new($"Server={host};Port={port};Database={databaseName};User Id={userName};Password={password}");
        conn.Open();
        return conn;
    }
}