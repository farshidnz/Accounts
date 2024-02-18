using FluentAssertions;
using MassTransit;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Member.Query
{
    [TestFixture]
    public class GetMemberQueryTest : TestBase
    {
        [Test]
        public async Task GetMemberQuery_ShouldReturnAMember()
        {
            var state = new TestState();

            var member = await state._repository.Object.QueryAsync<Domain.Entities.Member>("", state, 20);

            member.FirstOrDefault().MemberId.Should().Be(1001112621);
        }

        [Test]
        public async Task GetMemberQueryAsync_ShouldReturnAMember()
        {
            var state = new TestState();

            var member = await state._repository.Object.QueryAsync<Domain.Entities.Member>("1001112621", state, 20);

            member.FirstOrDefault().MemberId.Should().Be(1001112621);
        }
    }
}