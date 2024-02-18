using Accounts.Application.Common.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Accounts.Application.UnitTests.Common.Exceptions
{
    public class ExceptionTest
    {
        [Test]
        public void NotFoundException_Message_ShouldHaveValue()
        {
            var actual = new NotFoundException("Member Not Found");

            actual.Message.Should().BeEquivalentTo("Member Not Found");
        }

        [Test]
        public void NotFoundException_NoMessage_ShouldHaveNoValue()
        {
            var actual = new NotFoundException();

            actual.Message.Should().BeEquivalentTo("Exception of type 'Accounts.Application.Common.Exceptions.NotFoundException' was thrown.");
        }

        [Test]
        public void NotFoundException_WithInnerException_ShouldHaveInner()
        {
            var actual = new NotFoundException("Test", new System.Exception());

            actual.Message.Should().BeEquivalentTo("Test");
        }

        [Test]
        public void NotFoundException_WithKeyObject_ShouldHaveValueKey()
        {
            var actual = new NotFoundException("Test", new System.String("test"));

            actual.Message.Should().BeEquivalentTo("Entity \"Test\" (test) was not found.");
        }

        [Test]
        public void BadRequestException_WithKeyObject_ShouldHaveValueKey()
        {
            var actual = new BadRequestException("Test");

            actual.Message.Should().BeEquivalentTo("Test");
        }
    }
}