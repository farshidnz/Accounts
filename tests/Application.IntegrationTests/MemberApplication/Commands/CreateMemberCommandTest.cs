namespace Accounts.Application.IntegrationTests.MemberApplication.Commands;

using Accounts.Application.Common.Exceptions;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.Member;
using Accounts.Application.Member.Commands.CreateMember.v1;
using Domain.Entities;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[TestFixture]
internal class CreateMemberCommandTest : TestBase
{
    
        public MemberInfo MemberInfoResponse { get; private set; }
        public Mock<IAccountsPersistanceContext<int, Member>> _accountsDbContext { get; } = new();

        private Mock<IApplicationPersistenceContext<int, Member>> _applicationContext { get; } = new();

        private readonly Mock<ConsumeContext<CreateMemberCommand>> _consumeContextMock = new();

        private readonly Mock<IEncryptionService> _encryptServiceMock = new();
        public CreateMemberCommandConsumer CreateMemberCommandConsumer { get; }
        private List<Member> memberTestData = new() { new Member { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), FirstName = "Carlos", LastName = "Robert" } };

        public CreateMemberCommandTest()
        {
            _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<MemberInfo>()))
                    .Callback<MemberInfo>(response => MemberInfoResponse = response);
            var memberContext = new MemberContext() { CognitoId = Guid.Parse("7a2aa6e3-9626-4972-98b5-dd0e0bd586ca"), Email = string.Empty };

            _accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync(memberTestData.First());
            _encryptServiceMock.Setup(encrypt => encrypt.Encrypt(It.IsAny<string>(), Domain.Enums.EncryptionDataType.Base64, It.IsAny<string>()))
                .ReturnsAsync("TestEncrypted");
            CreateMemberCommandConsumer = new CreateMemberCommandConsumer(_accountsDbContext.Object, _applicationContext.Object, _encryptServiceMock.Object);
        }

        public void GivenCommandInput(CreateMemberCommand command)
        {
            _consumeContextMock.Setup(c => c.Message).Returns(command);
        }

        public async Task WhenConsume() =>
                await CreateMemberCommandConsumer.Consume(_consumeContextMock.Object);

        [Test]
        public async Task ShouldCreateMember_WhenMemberNotFound_GivenCognitoId()
        {
            var state = new CreateMemberCommandTest();
            var command = new CreateMemberCommand()
            {
                CognitoId = Guid.Parse("32d8550b-1067-4d46-9de9-66b55759790c"),
                Email = string.Empty,
                FirstName = "Ignacio",
                LastName = "Pedro",
                LastLogon = new DateTime(2022, 01, 01),
                ClickWindowActive = true,
                PopUpActive = true,
                IsValidated = true,
                RequiredLogin = false,
                IsAvailable = false,
                MailChimpListEmailId = "pedro",
                DateReceivedNewsLetter = DateTime.Now,
                CommunicationEmail = "test",
                PayPalEmail = "emailTest",
                TwoFactorAuthId = "1",
                TwoFactorAuthenticatedEnable = true,
                TwoFactorAuthenticatedActivatedBy = DateTime.Now,
                TwoFactorAuthenticatedMobile = "040000333",
                TwoFactorAuthenticatedCountryCode = "2026",
                DateJoined = DateTime.Now,
                IsWithdrawalCapped = false,
                SignUpVerificationEmailSentStatus = null,
                Active = true
            };
            state.GivenCommandInput(command);
            state._accountsDbContext.Setup(ac => ac.GetMember(It.IsAny<MemberContext>())).ReturnsAsync((Domain.Entities.Member)null);
            await state.WhenConsume();

            state.MemberInfoResponse.FirstName.ToString().Should().Be("Ignacio");
            state.MemberInfoResponse.LastName.ToString().Should().Be("Pedro");
            state.MemberInfoResponse.LastLogon.Should().Be(new DateTime(2022, 01, 01));
        }

        [Test]
        public void ShouldThrowFoundException_WhenMemberFound()
        {
            var state = new CreateMemberCommandTest();
            var command = new CreateMemberCommand()
            {
                CognitoId = Guid.Parse("32d8550b-1067-4d46-9de9-66b55759790c"),
                Email = string.Empty,
                FirstName = "Ignacio",
                LastName = "Pedro",
                LastLogon = new DateTime(2022, 01, 01)
            };
            state.GivenCommandInput(command);

            Assert.ThrowsAsync<ValidationException>(async () => await state.WhenConsume());
        }
    
}