using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Accounts.Application.Common.Behaviours;
using Accounts.Application.UnitTests.Common.TestHelper;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ValidationException = Accounts.Application.Common.Exceptions.ValidationException;

namespace Accounts.Application.UnitTests.Common.Behaviours
{
    public class TestMessageValidator { }

    public class ValidationBehaviourTests
    {
        private Mock<IValidator<TestMessage>> _validator;

        [SetUp]
        public void Start()
        {
            _validator = new Mock<IValidator<TestMessage>>();
        }

        private ValidationBehaviour<TestMessage> SUT(IValidator<TestMessage> validator = null)
        {
            var validators = new List<IValidator<TestMessage>>();
            if (validator != null)
            {
                validators.Add(validator);
            }

            return new ValidationBehaviour<TestMessage>(validators);
        }

        [Test]
        public async Task ShouldPassMessageToNextPipe_AfterProcessingMessage()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            await SUT().Send(context.Object, nextPipe.Object);

            nextPipe.Verify(x => x.Send(context.Object));
        }

        [Test]
        public void ShouldThrowValidationException_WhenValidationErrorsForMessage()
        {
            var message = new TestMessage();
            var context = MassTransitTestHelper.CreateMockConsumeContext(message);
            var nextPipe = MassTransitTestHelper.CreateMockNextPipeContext(message);

            var result = new ValidationResult(new List<ValidationFailure>() {
                new ValidationFailure("SomeProperty", "SomeError."),
                new ValidationFailure("AnotherProperty", "AnotherError.")
            });

            _validator.Setup(validator => validator.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(result);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await SUT(_validator.Object).Send(context.Object, nextPipe.Object));
            ex.Message.Should().Be("SomeError. AnotherError.");
        }
    }
}