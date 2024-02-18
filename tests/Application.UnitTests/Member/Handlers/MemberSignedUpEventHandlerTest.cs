using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Accounts.Application.Common.Models;
using MassTransit;
using Accounts.Domain.Events;
using Newtonsoft.Json;
using System;
using Accounts.Domain.Enums;

namespace Accounts.Application.UnitTests.Member.Handlers
{
    [TestFixture]
    internal class MemberSignedUpEventHandlerTest : TestBase
    {

        [Test]
        public async Task MemberSignedUpEventHandler_ShouldDecryptedPIIData()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<MemberSignedUp>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<MemberSignedUp>(new MemberSignedUp
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "testPIIData",
                CognitoPoolId = "test",
                OriginationSource = 1
            }, null));

            PIIData piiData = new PIIData {
                Email = "test@cashrewards.com",
                Phone = "+6140000000"
            };

            var decryptPIIData = JsonConvert.SerializeObject(piiData);
            state._encryptionService.Setup(r => r.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>())).ReturnsAsync(decryptPIIData);

            await state._MemberSignedUpEventHandler.Consume(context.Object);
            state._encryptionService.Verify(c => c.Decrypt(It.IsAny<string>(), It.IsAny<EncryptionDataType>(), It.IsAny<string>()), Times.Once);
        }
    }
}
