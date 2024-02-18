using Accounts.Application.Common.Exceptions;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.Member.Commands.LinkCognitoId;
using Accounts.Application.Member.Model;
using Accounts.Domain.Common;
using Accounts.Domain.Entities;
using Accounts.Domain.Events;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.IntegrationTests.MemberApplication.Commands;

public class LinkCognitoIdCommandTest : TestBase
{
    private class TestState
    {
        public Mock<IAccountsPersistanceContext<int, MemberCognito>> _accountsDbContext { get; } = new();
        private readonly Mock<ConsumeContext<LinkCognitoIdCommand>> _consumeContextMock = new();

        public LinkCognitoIdCommandConsumer Consumer { get; }
        private MemberCognito memberTestData =new MemberCognito {
            CognitoPoolId = 1,
            CognitoPoolName = "test",
            CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), 
            Email = "test@gmail.com",
            PIIData = "Test",
            PIIIEncryptAlgorithm = "SYSTEM_DEFAULT"
        };

        public MemberCognito MemberCognitoExpected { get; private set; }

        public TestState()
        {
            _accountsDbContext.Setup(ac => ac.GetCognitoPoolId(It.IsAny<string>())).ReturnsAsync(new MemberCognito
            {
                CognitoPoolId = memberTestData.CognitoPoolId,
                CognitoPoolName = memberTestData.CognitoPoolName
            });
            _accountsDbContext.Setup(ac => ac.AddOrUpdate(It.IsAny<MemberCognito>()))
                .Callback<MemberCognito>(response => MemberCognitoExpected = response);
            Consumer = new LinkCognitoIdCommandConsumer(_accountsDbContext.Object);
        }

        public void GivenCommandInput(LinkCognitoIdCommand command)
        {
            _consumeContextMock.Setup(c => c.Message).Returns(command);
        }

        public async Task WhenConsume() =>
                await Consumer.Consume(_consumeContextMock.Object);

        [Test]
        public async Task ShouldUpdateCoginitoAndPublishCognitoLinkedEvent_WhenMemberFound_GivenEmail()
        {
            var state = new TestState();
            var command = new LinkCognitoIdCommand()
            {
                CognitoId = memberTestData.CognitoId,
                CognitoPoolId = memberTestData.CognitoPoolName,  // This comes from the AWS cognito
                PIIData = memberTestData.PIIData,
                PIIIEncryptAlgorithm = memberTestData.PIIIEncryptAlgorithm,
                Email = memberTestData.Email
            };
            state.GivenCommandInput(command);

            await state.WhenConsume();

            // Check data stored to database
            state.MemberCognitoExpected.Email.ToString().Should().Be(memberTestData.Email);
            state.MemberCognitoExpected.CognitoId.Should().Be(memberTestData.CognitoId);
            state.MemberCognitoExpected.CognitoPoolId.ToString().Should().Be(memberTestData.CognitoPoolId.ToString());
            state.MemberCognitoExpected.PIIData.ToString().Should().Be(memberTestData.PIIData);
            state.MemberCognitoExpected.PIIIEncryptAlgorithm.ToString().Should().Be(memberTestData.PIIIEncryptAlgorithm);
            state.MemberCognitoExpected.HasDomainEvents.Should().BeTrue();

            // Check the domain event
            CognitoLinked cognitoLinkedEvent = (CognitoLinked)state.MemberCognitoExpected.DomainEvents.Dequeue();
            cognitoLinkedEvent.Metadata.EventType.ToString().Should().Be("CognitoLinked");
            cognitoLinkedEvent.CognitoId.Should().Be((Guid)memberTestData.CognitoId);
            cognitoLinkedEvent.CognitoPoolId.ToString().Should().Be(memberTestData.CognitoPoolName);
            cognitoLinkedEvent.PIIData.ToString().Should().Be(memberTestData.PIIData);
            cognitoLinkedEvent.PIIIEncryptAlgorithm.ToString().Should().Be(memberTestData.PIIIEncryptAlgorithm);
        }

        [Test]
        public void ShouldThrowNotFoundException_WhenCognitoPoolNotFound()
        {
            var state = new TestState();
            var command = new LinkCognitoIdCommand()
            {
                CognitoId = memberTestData.CognitoId,
                CognitoPoolId = memberTestData.CognitoPoolName,  // This comes from the AWS cognito
                PIIData = memberTestData.PIIData,
                PIIIEncryptAlgorithm = memberTestData.PIIIEncryptAlgorithm,
                Email = memberTestData.Email
            };
            state.GivenCommandInput(command);

            state._accountsDbContext.Setup(ac => ac.GetCognitoPoolId(It.IsAny<string>())).ReturnsAsync((Domain.Entities.MemberCognito)null);

            Assert.ThrowsAsync<NotFoundException>(async () => await state.WhenConsume());
        }
    }
}
