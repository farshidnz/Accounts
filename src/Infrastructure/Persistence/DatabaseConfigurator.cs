using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Persistence;

public interface IDatabaseConfigurator
{
    Task ConfigureDatabase(IMigrationRunner runner);
}

public class DatabaseConfigurator : IDatabaseConfigurator
{
    private readonly bool _configureDatabase;
    private readonly string _DbUsername;
    private readonly string _DbPassword;
    private readonly IAccountsDbContext _accountsDbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseConfigurator> _logger;

    public DatabaseConfigurator(
        IAccountsDbContext accountsDbContext,
        ILogger<DatabaseConfigurator> logger,
        IConfiguration configuration
        )
    {
        _configuration = configuration;
        _logger = logger;
        _configureDatabase = bool.TryParse(configuration["ConfigureDatabase"], out var b) && b;
        _DbUsername = _configuration["PostgresDbUsername"].ToLower();
        _DbPassword = _configuration["PostgresDbPassword"];
        _accountsDbContext = accountsDbContext;
    }

    public async Task ConfigureDatabase(IMigrationRunner runner)
    {
        await AddDatabaseUser();
        MigrateDatabase(runner);
    }

    private static void MigrateDatabase(IMigrationRunner runner)
    {
        Log.Information("Migrating database");
        runner.MigrateUp();
    }

    private NpgsqlConnection GetDatabase()
    {
        return _accountsDbContext.CreateDadminConnection();
    }

    /// <summary>
    /// creates the DB user or checks if it's necessary to create
    /// </summary>
    /// <returns></returns>
    public async Task AddDatabaseUser()
    {
        if (_configureDatabase)
        {
            if (await IsExistingUser(_DbUsername))
            {
                _logger.LogInformation("User {_DbUsername} already exists, proceed to update the password", _DbUsername);
                await UpdateUsersPassword(_DbUsername, _DbPassword);
            }
            else
            {
                bool capSchemaExists = await CheckIfSchemaExists("cap");
                await CreateUser(_DbUsername, _DbPassword, capSchemaExists);
            }
            if (!await CheckIfSchemaExists("dbo"))
            {
                await CreateSchema("dbo");
            }
        }
        else
            _logger.LogInformation("Skipping create of accounts database user");
    }

    public async Task RemoveUser(string username)
    {
        _logger.LogInformation("remove User {username} ", username);
        using var connection = GetDatabase();
        string sql = $"DROP OWNED BY {@username}; DROP ROLE {@username};";
        await connection.ExecuteAsync(sql);
    }

    public async Task UpdateUsersPassword(string username, string password)
    {
        _logger.LogInformation("update passwords User {username} ", username);
        using var connection = GetDatabase();
        string sql = $" ALTER USER {username} WITH PASSWORD '{password}';";
        await connection.ExecuteAsync(sql);
    }

    /// <summary>
    /// Check if the schema exists
    /// </summary>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    public async Task<bool> CheckIfSchemaExists(string schemaName)
    {
        using var connection = GetDatabase();
        string sql = $"SELECT schema_name FROM information_schema.schemata WHERE schema_name = @schemaname;";
        var response = await connection.QueryAsync(sql, new { schemaname = schemaName });
        return response.Any();
    }

    public async Task CreateSchema(string schemaName)
    {
        using var connection = GetDatabase();
        string sql = $"CREATE SCHEMA {schemaName};";

        await connection.ExecuteAsync(sql);
    }

    public async Task<bool> IsExistingUser(string username)
    {
        using var connection = GetDatabase();

        string sql = @"SELECT 1 FROM pg_roles where rolname = @rolename;";
        IEnumerable<PostgreSQLRoles> users = await connection.QueryAsync<PostgreSQLRoles>(sql, new { rolename = username });

        return users.Any();
    }

    public async Task CreateUser(string username, string password, bool capPrivileges)
    {
        using var connection = GetDatabase();

        string sql = $"CREATE ROLE {@username} WITH LOGIN NOSUPERUSER INHERIT NOCREATEDB CREATEROLE NOREPLICATION PASSWORD '{@password}'; ";
        sql += $"GRANT ALL PRIVILEGES ON DATABASE \"Accounts\" TO {@username};";
        sql += $"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA dbo,public {(capPrivileges ? ",cap" : "")} TO {@username};";
        sql += $"GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA dbo,public {(capPrivileges ? ",cap" : "")} TO {@username};";
        sql += $"GRANT USAGE ON SCHEMA dbo,public TO {@username};";

        await connection.ExecuteAsync(sql);
    }

    public class PostgreSQLRoles
    {
        public string rolname { get; set; }
    }
}