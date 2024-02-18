using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.UpdateMember.v1;
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
    internal class MemberCredentialChangedEventHandlerTest
    {
        [TestCase("test@cashrewards.com", "+6140000000")]
        [TestCase("test@cashrewards.com", null)]
        [TestCase(null, "+6140000000")]
        public async Task MemberCredentialChangedEventHandler_ShouldDecryptedPIIData(string email, string phone)
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<MemberCredentialChanged>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<MemberCredentialChanged>(new MemberCredentialChanged
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "testPIIData"
            }, null));
            state._mediator.Setup(r => r.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default));

            PIIData piiData = new PIIData
            {
                Email = email,
                Phone = phone
            };
            var decryptPIIData = JsonConvert.SerializeObject(piiData);
            state._encryptionService.Setup(r => r.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>())).ReturnsAsync(decryptPIIData);

            await state._MemberCredentialChangedEventHandler.Consume(context.Object);
            state._encryptionService.Verify(c => c.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>()), Times.Once);
            state._mediator.Verify(c => c.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default), Times.Once);
        }

        [TestCase(null)]
        [TestCase("testPIIData")]
        [TestCase("{\"FirstName\": \"Test\"}")]
        public async Task InvalidPIIDataMemberCredentialChangedEventHandler_ShouldNotCallUpdateMemberCommand(string decryptedPIIData)
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<MemberCredentialChanged>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<MemberCredentialChanged>(new MemberCredentialChanged
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "testPIIData"
            }, null));
            state._mediator.Setup(r => r.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default));

            state._encryptionService.Setup(r => r.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>())).ReturnsAsync(decryptedPIIData);

            await state._MemberCredentialChangedEventHandler.Consume(context.Object);
            state._encryptionService.Verify(c => c.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>()), Times.Once);
            state._mediator.Verify(c => c.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default), Times.Never);
        }
    }
}