using Accounts.Application.Common.Exceptions;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.Member;
using Accounts.Application.Member.Queries.GetMember.v1;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Application.IntegrationTests.MemberApplication.Queries;

using Domain.Entities;

public class GetMemberTest : TestBase
{
    private class TestState
    {
        public MemberInfo MemberInfoResponse { get; private set; }
        public Mock<IAccountsPersistanceContext<int, Member>> _accountsDbContext { get; } = new();

        private readonly Mock<ConsumeContext<GetMemberQuery>> _consumeContextMock = new();

        public GetMemberQueryConsumer GetMemberQueryConsumer { get; }
        private List<Domain.Entities.Member> memberTestData = new List<Domain.Entities.Member>() { new Domain.Entities.Member { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca") } };

        public TestState()
        {
            _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<MemberInfo>()))
                    .Callback<MemberInfo>(response => MemberInfoResponse = response);
            var memberContext = new MemberContext() { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), Email = string.Empty };

            _accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync(memberTestData.First());

            GetMemberQueryConsumer = new GetMemberQueryConsumer(_accountsDbContext.Object);
        }

        public void GivenQueryInput(Guid? cognitoId, string email)
        {
            _consumeContextMock.Setup(c => c.Message).Returns(new GetMemberQuery()
            {
                CognitoId = cognitoId,
                Email = email
            });
        }

        public async Task WhenConsume() =>
                await GetMemberQueryConsumer.Consume(_consumeContextMock.Object);
    }

    [Test]
    public async Task ShouldReturnMember_WhenMemberFound_GivenCognitoId()
    {
        var state = new TestState();

        state.GivenQueryInput(Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), string.Empty);

        await state.WhenConsume();

        state.MemberInfoResponse.CognitoId.ToString().Should().Be("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca");
    }

    [Test]
    public void ShouldThrowNotFoundException_WhenMemberNotFound()
    {
        var state = new TestState();
        state._accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync((Domain.Entities.Member)null);
        state.GivenQueryInput(Guid.Parse("ca2aa6e3-9626-4972-98b5-dd0e0bd586ca"), string.Empty);

        Assert.ThrowsAsync<NotFoundException>(async () => await state.WhenConsume());
    }

    [Test]
    public async Task ShouldReturnMember_WhenMemberFound_GivenEmailAddress()
    {
        var state = new TestState();

        state.GivenQueryInput(null, "test@cashrewards.com");
        state._accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync(new Domain.Entities.Member { Email = "test@cashrewards.com" });
        await state.WhenConsume();

        state.MemberInfoResponse.Email.Should().Be("test@cashrewards.com");
    }
}