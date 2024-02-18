using Accounts.Infrastructure.Persistence.Handlers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.UnitTests.Persistence
{
    [TestFixture]
    public class GuidTypeHandlerTest
    {
        private readonly Mock<IDbDataParameter> _parameter = new Mock<IDbDataParameter>();

        private static Guid guid => Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746");

        public GuidTypeHandlerTest()
        {
            _parameter.Setup(m => m.Value).Returns(guid);
        }

        [TestCase("a34d8785-f34b-42fc-ab0b-36f0b73dc746")]
        public void GivingAGuid_ItShouldAssignValue_ReturnGuid(System.Guid guid)
        {
            var handler = new GuidTypeHandler();
            handler.SetValue(_parameter.Object, guid);

            _parameter.Object.Value.Should().Be(guid);
        }

        [TestCase("a34d8785-f34b-42fc-ab0b-36f0b73dc746")]
        public void GivingAString_ItShouldParseItAsGuid_ReturnGuid(string guid)
        {
            var handler = new GuidTypeHandler();
            var result = handler.Parse(guid);

            result.GetType().Should().Be(typeof(System.Guid));
        }

        [TestCase("a34d8785-f34b-42fc")]
        [TestCase(null)]
        public void GivingANonGuid_ItShouldParseItAsGuid_ReturnGuidEmpty(string guid)
        {
            var handler = new GuidTypeHandler();
            var result = handler.Parse(guid);

            result.Should().Be(Guid.Empty);
        }
    }
}