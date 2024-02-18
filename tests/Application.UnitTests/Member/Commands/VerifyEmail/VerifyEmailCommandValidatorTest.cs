using Accounts.Application.Member.Commands.VerifyEmail;
using FluentAssertions;
using NUnit.Framework;

namespace Accounts.Application.UnitTests.Member.Commands.VerifyEmail;

public class VerifyEmailCommandValidatorTest
{
    [TestFixture]
    public class QueryValidatorTest
    {
        [TestCase("1001344949$22NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw=")]
        public void Should_PassValidationWithCode(string code)
        {
            var validator = new VerifyEmailCommandValidator();
            var verifyCode = new VerifyEmailCommand()
            {
                Code = code
            };
            validator.Validate(verifyCode).IsValid.Should().BeTrue();
        }

        [TestCase("100134494922NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw=")]
        public void ShouldNot_PassValidation_WithCode(string code)
        {
            var validator = new VerifyEmailCommandValidator();
            var verifyCode = new VerifyEmailCommand()
            {
                Code = code
            };
            validator.Validate(verifyCode).IsValid.Should().BeFalse();
        }

        [TestCase("")]
        public void ShouldNot_PassValidation_WithCodeEmpty(string code)
        {
            var validator = new VerifyEmailCommandValidator();
            var verifyCode = new VerifyEmailCommand()
            {
                Code = code
            };
            validator.Validate(verifyCode).IsValid.Should().BeFalse();
        }

        [TestCase("$22NJDe0nfEW0fRh6YMarbYUGH51noGPctS1L3A9SFpxNRt5Tw=")]
        public void ShouldNot_PassValidation_WithCodeWithNoMemberId(string code)
        {
            var validator = new VerifyEmailCommandValidator();
            var verifyCode = new VerifyEmailCommand()
            {
                Code = code
            };
            validator.Validate(verifyCode).IsValid.Should().BeFalse();
        }

        [TestCase("1001344949$")]
        public void ShouldNot_PassValidation_WithCodeWithNoHashedEmail(string code)
        {
            var validator = new VerifyEmailCommandValidator();
            var verifyCode = new VerifyEmailCommand()
            {
                Code = code
            };
            validator.Validate(verifyCode).IsValid.Should().BeFalse();
        }
    }
}