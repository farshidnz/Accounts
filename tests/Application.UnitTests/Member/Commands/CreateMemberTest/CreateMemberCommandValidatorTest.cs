using Accounts.Application.Member.Commands.CreateMember.v1;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Accounts.Application.UnitTests.Member.Commands.CreateMemberTest
{
    [TestFixture]
    internal class CreateMemberCommandValidatorTest : TestBase
    {
        [Test]
        public void Should_PassValidation_WithAllComply()
        {
            var validator = new CreateMemberCommandValidator();
            var memberQuery = new CreateMemberCommand()
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                Email = "pepe@gmail.com",
                ClientId = 123,
                MemberNewId = Guid.NewGuid().ToString()
            };
            validator.Validate(memberQuery).IsValid.Should().BeTrue();
        }

        [Test]
        public void Should_NotPassValidation_WithNotEmailValid()
        {
            var validator = new CreateMemberCommandValidator();
            var memberQuery = new CreateMemberCommand()
            {
                CognitoId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                Email = "pepegmail.com",
                ClientId = 123,
                MemberNewId = Guid.NewGuid().ToString()
            };
            validator.Validate(memberQuery).IsValid.Should().BeFalse();
        }

        [Test]
        public void Should_NotPassValidation_WithNotCognitoId()
        {
            var validator = new CreateMemberCommandValidator();
            var memberQuery = new CreateMemberCommand()
            {
                Email = "pepe@gmail.com",
                ClientId = 123,
                MemberNewId = Guid.NewGuid().ToString()
            };
            validator.Validate(memberQuery).IsValid.Should().BeFalse();
        }
    }
}