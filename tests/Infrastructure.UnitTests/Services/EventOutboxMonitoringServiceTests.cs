using Accounts.Infrastructure.BackgroundHostedService;
using Accounts.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class EventOutboxMonitoringServiceTests
    {
        private Mock<ILogger<EventOutboxMonitoringService>> logger;
        private Mock<IDomainEventService> domainEventService;
        private Mock<IServiceProvider> serviceProvider;
        private Mock<IServiceScope> serviceScope;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<EventOutboxMonitoringService>>();
            domainEventService = new Mock<IDomainEventService>();

            serviceProvider = new Mock<IServiceProvider>();
            serviceScope = new Mock<IServiceScope>();
        }

        public EventOutboxMonitoringService SUT()
        {
            // partial mock
            var sut = new Mock<EventOutboxMonitoringService>(logger.Object, serviceProvider.Object)
            {
                CallBase = true
            };
            sut.Setup(x => x.CreateServiceScope()).Returns(serviceScope.Object);
            sut.Setup(x => x.GetDomainEventService(It.IsAny<IServiceScope>())).Returns(domainEventService.Object);
            sut.SetupSequence(x => x.CancellationRequested(It.IsAny<CancellationToken>()))
                                    .Returns(false)
                                    .Returns(true);
            sut.Setup(x => x.TakeBreakBeforeResumingMonitoring(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            return sut.Object;
        }

        [Test]
        public void WhenMonitoringEventOutbox_ShouldPeriodicallyPublishAnyPendingOutgoingEvents()
        {
            var eventOutboxMonitoringService = SUT();

            eventOutboxMonitoringService.StartAsync(default);

            domainEventService.Verify(x => x.PublishEventOutbox());
        }
    }
}