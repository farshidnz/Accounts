using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Entities;
using Accounts.Infrastructure.AWS;
using Accounts.Infrastructure.BackgroundHostedService;
using Accounts.Infrastructure.Identity;
using Accounts.Infrastructure.Persistence;
using Accounts.Infrastructure.Services;
using Amazon.KeyManagementService;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savorboard.CAP.InMemoryMessageQueue;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Accounts.Infrastructure.Configuration;

namespace Accounts.Infrastructure
{
    public static class DependencyInjection
    {
        public static string GetConnectionString(IServiceCollection services, IConfiguration configuration)
        {
            var sp = services.BuildServiceProvider();
            var dbConfig = sp.GetRequiredService<DbConfiguration>();
            if (dbConfig == null)
                throw new InvalidOperationException();

            // It should use the application user credential
            string connectionString = $"Server={dbConfig.host};Port={dbConfig.port.ToString()};Database={dbConfig.dbname};User Id={configuration["PostgresDbUsername"].ToLower()};Password={configuration["PostgresDbPassword"]}";
            return connectionString;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            // swap out the in memory implentation to one suitable for your choice of persistence
            services.AddSingleton<IEventOutboxPersistenceContext, InMemoryEventOutboxPersistenceContext>();
            services.AddSingleton<IEncryptionService, KMSEncryptionService>((sp) => new KMSEncryptionService(sp.GetService<IConfiguration>(), sp.GetService<ILogger<KMSEncryptionService>>(), new AmazonKeyManagementServiceClient()));

            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddScoped<IRepository, Repository>();
            services.AddTransient(typeof(IApplicationPersistenceContext<,>), typeof(ApplicationPersistenceContext<,>));
            services.AddTransient(typeof(IDomainEntityPersistenceContext<,>), typeof(InMemoryDomainEntityPersistenceContext<,>));
            
            services.AddTransient<IDateTime, DateTimeService>();

            services.AddTransient<IIdentityService, IdentityService>();

            services.AddTransient(typeof(IAccountsPersistanceContext<,>), typeof(AccountsPersistenceDbContext<,>));
            services.AddSingleton<IAwsSecretsManagerClientFactory, AwsSecretsManagerClientFactory>();

            // AWS services
            services.AddAWSService<IAmazonSQS>();
            services.AddAWSService<IAmazonSimpleNotificationService>();
            services.AddSingleton<IAWSEventServiceFactory, AWSEventServiceFactory>();

            // Register Hosted Background Services
            services.AddHostedService<EventOutboxMonitoringService>();
            services.AddHostedService<EventPolledReadingService>();            
            services.AddTransient<IOutboxRepository, CapOutboxRepository>();

            var connectionString = GetConnectionString(services, configuration);
            services
                .AddCap(options =>
                {
                    options.UseDashboard();
                    options.DefaultGroupName = "cap.queue.account";
                    options.UsePostgreSql(connectionString);
                    options.UseInMemoryMessageQueue();
                });
            services.AddTransient<ICapSubscribe>(provider => ActivatorUtilities.CreateInstance<OutboxHandlers.OutboxHandlers>(provider, typeof(Member).Assembly));

            return services;
        }
    }
}