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
    public class PerformanceBehaviourTests : TestBase
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

        private Mock<PerformanceBehaviour<TestMessage>> SUT()
        {
            // partial mock
            var sut = new Mock<PerformanceBehaviour<TestMessage>>(_logger.Object, _currentUserService.Object, _identityService.Object)
            {
                CallBase = true
            };
            return sut;
        }

        [Test]
        public async Task WhenMessageHandledByPerformanceBehaviourFilter_ShouldPassMessageToNextPipe()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            await SUT().Object.Send(context.Object, nextPipe.Object);

            nextPipe.Verify(x => x.Send(context.Object));
        }

        [Test]
        public async Task WhenMessageProcessingDidntTakeTooLong_EqualOrLessThan500ms_ShouldNotLogWarningMessage()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            var sut = SUT();
            sut.Setup(x => x.ElapsedMilliseconds).Returns(500);

            await sut.Object.Send(context.Object, nextPipe.Object);

            _logger.Verify(CheckLogMesssageMatches<TestMessage>(LogLevel.Warning, It.IsAny<string>()), Moq.Times.Never);
        }

        [TestCase("persons cognito Id", "bob")]
        public async Task WhenMessageProcessingTookTooLong_GreaterThan500ms_ShouldWarnLogMessage(string userId, string userName)
        {
            _currentUserService.Setup(x => x.UserId).Returns(userId);
            _identityService.Setup(x => x.GetUserNameAsync(userId)).Returns(Task.FromResult(userName));

            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            var sut = SUT();
            sut.Setup(x => x.ElapsedMilliseconds).Returns(501);

            await sut.Object.Send(context.Object, nextPipe.Object);

            var logMessage = $"Accounts Long Running Request: {typeof(TestMessage).Name} (501 milliseconds) {userId} {userName} {message}";
            _logger.Verify(CheckLogMesssageMatches<TestMessage>(LogLevel.Warning, logMessage));
        }
    }
}