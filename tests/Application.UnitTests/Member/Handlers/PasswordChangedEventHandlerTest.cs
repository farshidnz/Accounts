using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.LinkCognitoId;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using MassTransit;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Handlers
{
    [TestFixture]
    internal class PasswordChangedEventHandlerTest
    {
        [Test]
        public async Task PasswordChangedEventHandler_ShouldCallLinkCognitoIdCommand()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<PasswordChanged>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<PasswordChanged>(new PasswordChanged
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                CognitoPoolId = "ap-southeast-2_9q6TXai99",
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "testPIIData",
            }, null));
            state._mediator.Setup(r => r.Publish<CognitoLinked>(It.IsAny<LinkCognitoIdCommand>(), default));

            PIIData piiData = new PIIData
            {
                Email = "test@cashrewards.com"
            };
            var decryptPIIData = JsonConvert.SerializeObject(piiData);
            state._encryptionService.Setup(r => r.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>())).ReturnsAsync(decryptPIIData);

            await state._PasswordChangedEventHandler.Consume(context.Object);
            state._encryptionService.Verify(c => c.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>()), Times.Once);
            state._mediator.Verify(c => c.Publish<LinkCognitoIdCommand>(It.IsAny<LinkCognitoIdCommand>(), default), Times.Once);
        }

        [TestCase(null)]
        [TestCase("testPIIData")]
        [TestCase("{\"FirstName\": \"Test\"}")]
        public async Task InvalidPIIDataPasswordChangedEventHandler_ShouldNotCallLinkCognitoIdCommand(string decryptedPIIData)
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<MemberCredentialChanged>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<MemberCredentialChanged>(new MemberCredentialChanged
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "testPIIData"
            }, null));
            state._mediator.Setup(r => r.Publish<CognitoLinked>(It.IsAny<LinkCognitoIdCommand>(), default));

            state._encryptionService.Setup(r => r.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>())).ReturnsAsync(decryptedPIIData);

            await state._MemberCredentialChangedEventHandler.Consume(context.Object);
            state._encryptionService.Verify(c => c.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>()), Times.Once);
            state._mediator.Verify(c => c.Publish<CognitoLinked>(It.IsAny<LinkCognitoIdCommand>(), default), Times.Never);
        }
    }
}