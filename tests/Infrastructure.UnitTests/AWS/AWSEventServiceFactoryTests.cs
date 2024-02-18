using Amazon.SimpleNotificationService;
using Amazon.SQS;
using FluentAssertions;
using Accounts.Application.Common.Extensions;
using Accounts.Domain.UnitTests.Common;
using Accounts.Infrastructure.AWS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure.UnitTests.AWS
{
    [TestFixture]
    public class AWSEventServiceFactoryTests
    {
        private Mock<IAmazonSQS> sqsClient;
        private Mock<IAmazonSimpleNotificationService> snsClient;
        private Mock<ILoggerFactory> loggerFactory;
        private readonly Assembly testDomainAssembly = Assembly.Load("Accounts.Domain.UnitTests");

        [SetUp]
        public void Setup()
        {
            sqsClient = new Mock<IAmazonSQS>();
            snsClient = new Mock<IAmazonSimpleNotificationService>();
            loggerFactory = new Mock<ILoggerFactory>();
        }

        public AWSEventServiceFactory SUT(string appConifigJson)
        {
            var configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appConifigJson)))
                                                          .Build();

            // partial mock
            var sut = new Mock<AWSEventServiceFactory>(sqsClient.Object, snsClient.Object, loggerFactory.Object, configuration)
            {
                CallBase = true
            };
            sut.Setup(x => x.DomainAssembly).Returns(testDomainAssembly);

            return sut.Object;
        }

        [Test]
        public void WhenEmptyAppConfig_ThenThereShouldNotBePublishersForEvent()
        {
            var appConifig = GetAppConfigJson(new { });

            SUT(appConifig).GetAWSPublishersForEvent(new TestEvent()).Count().Should().Be(0);
        }

        [Test]
        public void WhenNoMatchingEventDestinationInAppConfig_ThenThereShouldNotBePublishersForEvent()
        {
            var appConifig = GetAppConfigJson(new
            {
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = "SQS",
                            Name = "queueName",
                            EventTypeName = "NotMatchingTestEvent"
                        }
                    }
                }
            });

            SUT(appConifig).GetAWSPublishersForEvent(new TestEvent()).Count().Should().Be(0);
        }

        [Test]
        public void WhenUnknownResourceTypeInAppConfig_ThenThereShouldNotBePublishersForEvent()
        {
            var appConifig = GetAppConfigJson(new
            {
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = "Unknown",
                            Name = "queueName",
                            EventTypeName = "String"
                        }
                    }
                }
            });

            SUT(appConifig).GetAWSPublishersForEvent(new TestEvent()).Count().Should().Be(0);
        }

        [TestCase("SNS")]
        public void WhenMatchingEventDestinationInAppConfig_ThenThereShouldBePublishersForEvent(string resourceType)
        {
            var appConifig = GetAppConfigJson(new
            {
                Environment = "test",
                ServiceName = "accounts",
                EventDestinations = new
                {
                    AWSResources = new[] {
                        new {
                            Type = resourceType,
                            Domain = "domain",
                            EventTypeName = "TestEvent"
                        }
                    }
                }
            });

            var publishers = SUT(appConifig).GetAWSPublishersForEvent(new TestEvent());
            publishers.Count.Should().Be(1);
            var publisher = publishers.First();
            publisher.AWSResourceName.Should().Be("test-domain");
            var eventType = publisher.EventTypes.First();
            eventType.Name.Should().Be("TestEvent");
            eventType.Should().Be(typeof(TestEvent));
        }

        private string GetAppConfigJson(dynamic appConfig)
        {
            return JsonConvert.SerializeObject(appConfig);
        }
    }
}