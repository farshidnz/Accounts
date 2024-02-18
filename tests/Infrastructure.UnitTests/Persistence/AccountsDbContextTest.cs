using Accounts.Application.IntegrationTests;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.Configuration;
using Accounts.Infrastructure.Persistence;
using Amazon.Runtime.Internal.Util;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace Infrastructure.UnitTests.Persistence;

[TestFixture]
public class AccountsDbContextTest
{
    private class TestState
    {
        public AccountsDbContext accountsDbContext { get; }

        public DbConfiguration dbConfiguration { get; set; } = new();
        public Mock<IOptions<AWSAccountsSecret>> options = new();

        public TestState()
        {
            var configuration = new ConfigurationBuilder()
                   .AddJsonFile($"{Assembly.Load("Accounts.API").Folder()}/appsettings.json", true)
                   .AddJsonFile($"{Assembly.Load("Accounts.API").Folder()}/appsettings.Development.json", true)
                   .Build();

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("Accounts.Application"));
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("Accounts.Infrastructure"));
            options.Setup(op => op.Value).Returns(new AWSAccountsSecret() {dbname ="Accounts" });

            dbConfiguration.host = "testHost";
            dbConfiguration.port = "5132";
            dbConfiguration.password = "123456";
            dbConfiguration.username = "test";
            dbConfiguration.dbname = "Accounts";

            accountsDbContext = new AccountsDbContext(dbConfiguration, configuration);
        }
    }

    [Test]
    public void CheckDbConfigHasValue_ShouldReturn_ConnString()
    {
        var testState = new TestState();

        testState.accountsDbContext._dbConfiguration.Should().Be(testState.dbConfiguration);
    }
}