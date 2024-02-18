using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Domain.Events;
using MassTransit;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Handlers
{
    [TestFixture]
    internal class MemberSignedInEventHandlerTest
    {
        [Test]
        public async Task MemberSignedInEventHandler_ShouldReturnAMemberUpdated()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<MemberSignedIn>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<MemberSignedIn>(new MemberSignedIn
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                PIIIEncryptAlgorithm = "SYSTEM_DEFAULT",
                PIIData = "",
                LastLogonUtc = DateTime.UtcNow,
            }, null));
            state._mediator.Setup(r => r.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default));

            await state._MemberSignedInEventHandler.Consume(context.Object);
            state._mediator.Verify(c => c.Publish<UpdateMemberCommand>(It.IsAny<UpdateMemberCommand>(), default), Times.Once);
        }
    }
}
