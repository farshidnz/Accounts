using Accounts.Application.Common.Interfaces;
using Accounts.Application.Member.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Application.IntegrationTests.MemberApplication.Commands;

using Accounts.Application.Common.Exceptions;
using Accounts.Application.Member.Commands.VerifyEmail;
using Domain.Entities;
using FluentAssertions;
using MassTransit;
using NUnit.Framework;

public class VerifyEmailCommandTest : TestBase
{
    private class TestState
    {
        public VerifyEmailDto VerifyEmailDto { get; private set; }
        public Mock<IAccountsPersistanceContext<int, Member>> _accountsDbContext { get; } = new();
        public Mock<IApplicationPersistenceContext<int,Member>> _applicationPersistenceContext { get; } = new();
        private readonly Mock<ConsumeContext<VerifyEmailCommand>> _consumeContextMock = new();

        public VerifyEmailCommandConsumer VerifyEmailCommandConsumer { get; }
        private List<Member> memberTestData = new() { new Member { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), FirstName = "Carlos", LastName = "Robert", IsValidated = true, Email = "pepe@gmail.com" } };

        public TestState()
        {
            _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<VerifyEmailDto>()))
                    .Callback<VerifyEmailDto>(response => VerifyEmailDto = response);
            var memberContext = new MemberContext() { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), Email = "pepe@gmail.com" };

            _accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync(memberTestData.First());
            _applicationPersistenceContext.Setup(ap => ap.Save(It.IsAny<Member>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            VerifyEmailCommandConsumer = new VerifyEmailCommandConsumer(_accountsDbContext.Object,_applicationPersistenceContext.Object);
        }

        public void GivenCommandInput(VerifyEmailCommand command)
        {
            _consumeContextMock.Setup(c => c.Message).Returns(command);
        }

        public async Task WhenConsume() =>
                await VerifyEmailCommandConsumer.Consume(_consumeContextMock.Object);

        [Test]
        public async Task ShouldReturnMember_WhenMemberFound_GivenCognitoId()
        {
            var state = new TestState();
            var command = new VerifyEmailCommand()
            {
                Code = "1001344949$22NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw="
            };
            state.GivenCommandInput(command);

            await state.WhenConsume();

            state.VerifyEmailDto.Email.ToString().Should().Be("pepe@gmail.com");
        }

        [Test]
        public void ShouldThrowNotFoundException_WhenMemberNotFound()
        {
            var state = new TestState();

            var command = new VerifyEmailCommand()
            {
                Code = "100139$22NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw="
            };
            state.GivenCommandInput(command);

            state._accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync((Domain.Entities.Member)null);

            Assert.ThrowsAsync<NotFoundException>(async () => await state.WhenConsume());
        }

        [Test]
        public void ShouldThrowException_WhenCodeNotRightFormat()
        {
            var state = new TestState();

            var command = new VerifyEmailCommand()
            {
                Code = "22NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw="
            };
            state.GivenCommandInput(command);

            state._accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync((Domain.Entities.Member)null);

            Assert.ThrowsAsync<ValidationException>(async () => await state.WhenConsume());
        }
    }
}