using Accounts.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.BackgroundHostedService
{
    // check if a background serivce is appropriate based on the way the microservice is deployed.
    // For e.g. if it a lambda triggered by api request, then there might not be an instance running
    // until the api request comes in.
    public class EventOutboxMonitoringService : BackgroundService
    {
        private readonly ILogger<EventOutboxMonitoringService> logger;
        private readonly IServiceProvider serviceProvider;

        public readonly string ServiceName = "Accounts-EventOutboxMonitoring";

        public EventOutboxMonitoringService(ILogger<EventOutboxMonitoringService> logger,
                                            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public virtual bool CancellationRequested(CancellationToken stoppingToken) => stoppingToken.IsCancellationRequested;

        public virtual Task TakeBreakBeforeResumingMonitoring(CancellationToken stoppingToken) => Task.Delay(5 * 60 * 1000, stoppingToken);

        public virtual IServiceScope CreateServiceScope() => serviceProvider.CreateScope();

        public virtual IDomainEventService GetDomainEventService(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<IDomainEventService>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => logger.LogInformation($"{ServiceName} background task is stopping due to CancellationToken."));

            while (!CancellationRequested(stoppingToken))
            {
                await MonitorEventOutbox(stoppingToken);
            }

            logger.LogInformation($"{ServiceName} background task is stopping.");
        }

        private async Task MonitorEventOutbox(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = CreateServiceScope();

                await GetDomainEventService(scope).PublishEventOutbox();

                await TakeBreakBeforeResumingMonitoring(stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{ServiceName} failed to monitor and publish Event Outbox, Error: {e.Message}");
            }
        }
    }
}