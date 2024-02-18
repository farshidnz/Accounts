using MassTransit;
using Moq;

namespace Accounts.Application.UnitTests.Common.TestHelper
{
    public static class MassTransitTestHelper
    {
        public static Mock<ConsumeContext<T>> CreateMockConsumeContext<T>(T message) where T : class
        {
            var context = new Mock<ConsumeContext<T>>();
            context.Setup(x => x.Message).Returns(message);
            return context;
        }

        public static Mock<IPipe<ConsumeContext<T>>> CreateMockNextPipeContext<T>(T message = null) where T : class
        {
            var context = new Mock<IPipe<ConsumeContext<T>>>();
            return context;
        }
    }
}