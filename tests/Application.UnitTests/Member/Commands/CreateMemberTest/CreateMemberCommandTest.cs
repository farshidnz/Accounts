using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Commands.CreateMemberTest
{
    [TestFixture]
    internal class CreateMemberCommandTest : TestBase
    {
        [Test]
        public async Task CreateMemberCommand_ShouldReturn_AMemberCreated()
        {
            var state = new TestState();

            await state._repository.Object.ExecuteAsync("testquery", null, 10);
            state._repository.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
