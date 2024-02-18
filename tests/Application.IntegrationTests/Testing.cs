using Accounts.API;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.IntegrationTests;
using Accounts.Infrastructure.Identity;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Respawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[SetUpFixture]
public class Testing
{
    private static IConfigurationRoot _configuration;
    private static IServiceScopeFactory _scopeFactory;
    private static InMemoryTestHarness _harness;
    private static Checkpoint _checkpoint;
    private static string _currentUserId;

    [OneTimeSetUp]
    public async Task TryRunBeforeAnyTests()
    {
        try
        {
            await RunBeforeAnyTests();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        var configs = new Dictionary<string, string>
        {
            ["OverriddenConfig"] = "my-test-value"
        };

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{Assembly.Load("Accounts.API").Folder()}/appsettings.json", true)
            .AddJsonFile($"{Assembly.Load("Accounts.API").Folder()}/appsettings.Development.json", true)
            .AddInMemoryCollection(configs)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
       
       

        var services = new ServiceCollection();
        IWebHostEnvironment webHostBuilder = (IWebHostEnvironment)services.FirstOrDefault(d =>
           d.ServiceType == typeof(IWebHostEnvironment));
        var startup = new Startup(_configuration);
        services.AddControllers();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.EnvironmentName == "Development" &&
            w.ApplicationName == "Accounts.WebUI"));
        services.AddSingleton<IConfiguration>(_configuration);

        services.AddLogging();

        startup.ConfigureServices(services);

        // Replace service registration for ICurrentUserService
        // Remove existing registration
        var currentUserServiceDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ICurrentUserService));

        services.Remove(currentUserServiceDescriptor);

        // Register testing version
        services.AddTransient(provider =>
            Mock.Of<ICurrentUserService>(s => s.UserId == _currentUserId));

        services.AddMassTransitInMemoryTestHarness(cfg =>
        {
            cfg.AddConsumers(Assembly.Load("Accounts.Application"));
        });

        var provider = services.BuildServiceProvider();
        _scopeFactory = provider.GetService<IServiceScopeFactory>();

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "VersionInfo" }
        };

        _harness = provider.GetRequiredService<InMemoryTestHarness>();
        await _harness.Start();
    }

    public static async Task SendCommand<T>(T command)
    {
        using var scope = _scopeFactory.CreateScope();

        var bus = scope.ServiceProvider.GetService<IBus>();

        await bus.Publish(command);
    }

    public static async Task<TResponse> SendQuery<T, TResponse>(T query) where T : class
                                                                         where TResponse : class
    {
        using var scope = _scopeFactory.CreateScope();

        var bus = scope.ServiceProvider.GetService<IBus>();

        var client = bus.CreateRequestClient<T>();

        var response = await client.GetResponse<TResponse>(query);

        return response.Message;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", new string[] { });
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", new[] { "Administrator" });
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _currentUserId = user.Id;

            return _currentUserId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        //await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
        _currentUserId = null;
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _harness.Stop();
    }
}