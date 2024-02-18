using Accounts.Infrastructure.Persistence;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Accounts.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var runner = services.GetRequiredService<IMigrationRunner>();
                    await ConfigureDatabase(host, runner);
                    //var context = services.GetRequiredService<ApplicationDbContext>();

                    //var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    //var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    //await ApplicationDbContextSeed.SeedDefaultUserAsync(userManager, roleManager);
                    //await ApplicationDbContextSeed.SeedSampleDataAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

            await host.RunAsync();
        }

        private static async Task ConfigureDatabase(IHost host, IMigrationRunner runner)
        {
            var databaseConfigurator = host.Services.GetService(typeof(IDatabaseConfigurator)) as IDatabaseConfigurator;
            await databaseConfigurator.ConfigureDatabase(runner);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}