using NUnit.Framework;
using System.Threading.Tasks;

namespace Accounts.Application.IntegrationTests
{
    using static Testing;
    [TestFixture]
    public class TestBase
    {
        [SetUp]
        public async Task TestSetUp()
        {
            await ResetState();
        }
    }
}
