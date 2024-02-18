using Accounts.Application.Member.Commands.UpdateMember.v1;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Commands
{
    [TestFixture]
    internal class UpdateMemberCommandValidatorTest :TestBase
    {

        [TestCase("a34d8785-f34b-42fc-ab0b-36f0b73dc746")]
        public void Should_PassValidationWithCognito(Guid cognito)
        {
            var validator = new UpdateMemberCommandValidator();
            var memberQuery = new UpdateMemberCommand()
            {
                CognitoId = cognito
            };
            validator.Validate(memberQuery).IsValid.Should().BeTrue();
        }

        [TestCase(null)]
        public void Should_NotPassValidation_WithEmptyCognito(Guid? cognito)
        {
            var validator = new UpdateMemberCommandValidator();
            var memberQuery = new UpdateMemberCommand()
            {
                CognitoId = cognito
            };
            validator.Validate(memberQuery).IsValid.Should().BeFalse();
        }

    }
}
