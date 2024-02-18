using Accounts.Application.Member.Queries.GetMember.v1;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Query
{
    [TestFixture]
    public class QueryValidatorTest
    {
        [TestCase("email@gmail.com")]
        public void Should_PassValidationWithEmail(string email)
        {
            var validator = new GetMemberQueryValidator();
            var memberQuery = new GetMemberQuery()
            {
                Email = email
            };
            validator.Validate(memberQuery).IsValid.Should().BeTrue();
        }

        [TestCase("a34d8785-f34b-42fc-ab0b-36f0b73dc746")]
        public void Should_PassValidation_WithGuid(string cognitoId)
        {
            var validator = new GetMemberQueryValidator();
            var memberQuery = new GetMemberQuery()
            {
                CognitoId = Guid.Parse(cognitoId)
            };
            validator.Validate(memberQuery).IsValid.Should().BeTrue();
        }
    }
}