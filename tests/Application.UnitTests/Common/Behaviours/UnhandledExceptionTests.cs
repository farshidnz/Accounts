using Accounts.Application.Common.Behaviours;
using Accounts.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Accounts.Application.UnitTests.Common.TestHelper;
using System;

namespace Accounts.Application.UnitTests.Common.Behaviours
{
    public class UnhandledExceptionTests : TestBase
    {
        private Mock<ILogger<TestMessage>> _logger;

        [SetUp]
        public void Start()
        {
            _logger = new Mock<ILogger<TestMessage>>();
        }

        [Test]
        public async Task WhenMessageHandledByUnhandledExceptionBehaviourFilter_ShouldPassMessageToNextPipeAfterProcessing()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            await SUT().Send(context.Object, nextPipe.Object);

            nextPipe.Verify(x => x.Send(context.Object));
        }

        [Test]
        public void WhenMessageHandledByPerformanceBehaviourFilter_ShouldErrorLogMessage()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            nextPipe.Setup(x => x.Send(context.Object)).Throws(new Exception());

            var ex = Assert.ThrowsAsync<Exception>(async () => await SUT().Send(context.Object, nextPipe.Object));

            var logMessage = $"Accounts Request: Unhandled Exception for Request {typeof(TestMessage).Name} {message}";
            _logger.Verify(CheckLogMesssageMatches<TestMessage>(LogLevel.Error, logMessage));
        }

        private UnhandledExceptionBehaviour<TestMessage> SUT()
        {
            return new UnhandledExceptionBehaviour<TestMessage>(_logger.Object);
        }
    }
}
