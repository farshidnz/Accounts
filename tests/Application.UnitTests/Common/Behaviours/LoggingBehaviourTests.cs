using Accounts.Application.Common.Behaviours;
using Accounts.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Accounts.Application.UnitTests.Common.TestHelper;

namespace Accounts.Application.UnitTests.Common.Behaviours
{
    public class LoggingBehaviourTests : TestBase
    {
        private Mock<ILogger<TestMessage>> _logger;
        private Mock<ICurrentUserService> _currentUserService;
        private Mock<IIdentityService> _identityService;

        [SetUp]
        public void Start()
        {
            this._logger = new Mock<ILogger<TestMessage>>();

            this._currentUserService = new Mock<ICurrentUserService>();

            this._identityService = new Mock<IIdentityService>();
        }

        [Test]
        public async Task WhenMessageHandledByLogginBehaviourFilter_ShouldPassMessageToNextPipeAfterProcessing()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            await SUT().Send(context.Object, nextPipe.Object);

            nextPipe.Verify(x => x.Send(context.Object));
        }

        [TestCase("persons cognito Id", "bob")]
        [TestCase(null, "")]
        [TestCase(null, null)]
        public async Task WhenMessageHandledByLogginBehaviourFilter_ShouldLogMessageAndUserDetail(string userId, string userName)
        {
            _currentUserService.Setup(x => x.UserId).Returns(userId);
            _identityService.Setup(x => x.GetUserNameAsync(userId)).Returns(Task.FromResult(userName));

            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            await SUT().Send(context.Object, nextPipe.Object);

            var logMessage = $"Accounts Request: {typeof(TestMessage).Name} {userId} {userName} {message}";
            this._logger.Verify(CheckLogMesssageMatches<TestMessage>(LogLevel.Information, logMessage));
        }

        private LoggingBehaviour<TestMessage> SUT()
        {
            return new LoggingBehaviour<TestMessage>(_logger.Object, _currentUserService.Object, _identityService.Object);
        }

        [TearDown]
        public void VerifyAndTearDown()
        {
        }
    }
}